using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using UWE;

public class UserStoragePC : UserStorage
{
	private class Wrapper
	{
		public readonly UserStorageUtils.AsyncOperation operation;

		public readonly string containerName;

		public readonly string savePath;

		public Wrapper(UserStorageUtils.AsyncOperation operation, string savePath, string containerName)
		{
			this.operation = operation;
			this.savePath = savePath;
			this.containerName = containerName;
		}
	}

	private sealed class GenericWrapper : Wrapper
	{
		public readonly List<string> files;

		public readonly object state;

		public GenericWrapper(UserStorageUtils.AsyncOperation operation, string savePath, string containerName, List<string> files = null, object state = null)
			: base(operation, savePath, containerName)
		{
			this.files = files;
			this.state = state;
		}
	}

	private sealed class CopyFilesToContainerWrapper : Wrapper
	{
		public readonly List<string> updatedFiles;

		public readonly List<string> deletedFiles;

		public readonly List<string> unchangedFiles;

		public readonly string sourcePath;

		public CopyFilesToContainerWrapper(UserStorageUtils.AsyncOperation operation, string savePath, string containerName, string sourcePath, List<string> updatedFiles, List<string> deletedFiles, List<string> unchangedFiles)
			: base(operation, savePath, containerName)
		{
			this.updatedFiles = updatedFiles;
			this.deletedFiles = deletedFiles;
			this.unchangedFiles = unchangedFiles;
			this.sourcePath = sourcePath;
		}
	}

	private sealed class UpgradeWrapper : Wrapper
	{
		public string backupPath;

		public List<string> containersToUpgrade;

		public UpgradeWrapper(UserStorageUtils.AsyncOperation operation, string savePath, string backupPath, List<string> containersToUpgrade)
			: base(operation, savePath, null)
		{
			this.backupPath = backupPath;
			this.containersToUpgrade = containersToUpgrade;
		}
	}

	private static readonly WorkerThread ioThread = ThreadUtils.StartWorkerThread("I/O", "UserStoragePC", ThreadPriority.BelowNormal, -2, 10);

	private string savePath;

	private const string batchCellsPattern = "baked-batch-cells-*.bin";

	private const string batchObjectsPattern = "batch-objects-*.*";

	private const string batchOctreesPattern = "compiled-batch-*.optoctrees";

	private static readonly Task.Function InitializeAsyncImplDelegate = InitializeAsyncImpl;

	private static readonly Task.Function GetContainerNamesAsyncImplDelegate = GetContainerNamesAsyncImpl;

	private static readonly Task.Function LoadFilesAsyncImplDelegate = LoadFilesAsyncImpl;

	private static readonly Task.Function SaveFilesAsyncImplDelegate = SaveFilesAsyncImpl;

	private static readonly Task.Function CreateContainerAsyncImplDelegate = CreateContainerAsyncImpl;

	private static readonly Task.Function DeleteContainerAsyncImplDelegate = DeleteContainerAsyncImpl;

	private static readonly Task.Function DeleteFilesAsyncImplDelegate = DeleteFilesAsyncImpl;

	private static readonly Task.Function CopyFilesFromContainerAsyncImplDelegate = CopyFilesFromContainerAsyncImpl;

	private static readonly Task.Function CopyFilesToContainerAsyncImplDelegate = CopyFilesToContainerAsyncImpl;

	private static readonly Task.Function UpgradeSaveDataAsyncImplDelegate = UpgradeSaveDataAsyncImpl;

	private static string GetSaveFilePath(string savePath, string containerName, string relativePath)
	{
		return Path.Combine(Path.Combine(savePath, containerName), relativePath);
	}

	public UserStoragePC(string _savePath)
	{
		savePath = _savePath;
	}

	public UserStorageUtils.AsyncOperation InitializeAsync()
	{
		UserStorageUtils.AsyncOperation asyncOperation = new UserStorageUtils.AsyncOperation();
		ioThread.Enqueue(InitializeAsyncImplDelegate, this, new Wrapper(asyncOperation, savePath, null));
		return asyncOperation;
	}

	private static void InitializeAsyncImpl(object owner, object state)
	{
		Wrapper wrapper = (Wrapper)state;
		UserStorageUtils.AsyncOperation operation = wrapper.operation;
		UserStorageUtils.Result result = UserStorageUtils.Result.Success;
		string errorMessage = null;
		if (!Directory.Exists(wrapper.savePath))
		{
			try
			{
				Directory.CreateDirectory(wrapper.savePath);
			}
			catch (Exception ex)
			{
				result = UserStorageUtils.GetResultForException(ex);
				errorMessage = ex.Message;
			}
		}
		operation.SetComplete(result, errorMessage);
	}

	public UserStorageUtils.QueryOperation GetContainerNamesAsync()
	{
		UserStorageUtils.QueryOperation queryOperation = new UserStorageUtils.QueryOperation();
		ioThread.Enqueue(GetContainerNamesAsyncImplDelegate, this, new Wrapper(queryOperation, savePath, null));
		return queryOperation;
	}

	private static void GetContainerNamesAsyncImpl(object owner, object state)
	{
		Wrapper wrapper = (Wrapper)state;
		UserStorageUtils.QueryOperation queryOperation = (UserStorageUtils.QueryOperation)wrapper.operation;
		UserStorageUtils.Result result = UserStorageUtils.Result.Success;
		string errorMessage = null;
		if (Directory.Exists(wrapper.savePath))
		{
			try
			{
				string[] directories = Directory.GetDirectories(wrapper.savePath, "*", SearchOption.TopDirectoryOnly);
				for (int i = 0; i < directories.Length; i++)
				{
					string fileName = Path.GetFileName(directories[i]);
					queryOperation.results.Add(fileName);
				}
			}
			catch (Exception ex)
			{
				result = UserStorageUtils.GetResultForException(ex);
				errorMessage = ex.Message;
			}
		}
		queryOperation.SetComplete(result, errorMessage);
	}

	public UserStorageUtils.LoadOperation LoadFilesAsync(string containerName, List<string> fileNames)
	{
		UserStorageUtils.LoadOperation loadOperation = new UserStorageUtils.LoadOperation();
		ioThread.Enqueue(LoadFilesAsyncImplDelegate, this, new GenericWrapper(loadOperation, savePath, containerName, fileNames));
		return loadOperation;
	}

	private static void LoadFilesAsyncImpl(object owner, object state)
	{
		GenericWrapper obj = (GenericWrapper)state;
		UserStorageUtils.LoadOperation loadOperation = (UserStorageUtils.LoadOperation)obj.operation;
		string path = obj.savePath;
		string containerName = obj.containerName;
		List<string> files = obj.files;
		UserStorageUtils.Result result = UserStorageUtils.Result.Success;
		string errorMessage = null;
		if (!Directory.Exists(Path.Combine(path, containerName)))
		{
			result = UserStorageUtils.Result.UnknownError;
			errorMessage = $"Container {containerName} doesn't exist";
		}
		else
		{
			byte[] compressBuffer = new byte[4096];
			foreach (string item in files)
			{
				byte[] array = null;
				try
				{
					array = File.ReadAllBytes(GetSaveFilePath(path, containerName, item));
					if (array != null)
					{
						array = UserStorageUtils.Decompress(array, array.Length, compressBuffer);
					}
				}
				catch (Exception ex)
				{
					result = UserStorageUtils.GetResultForException(ex);
					errorMessage = ex.Message;
				}
				loadOperation.files[item] = array;
			}
		}
		loadOperation.SetComplete(result, errorMessage);
	}

	public UserStorageUtils.SlotsOperation LoadSlotsAsync(List<string> containerNames, List<string> fileNames)
	{
		return UserStorageUtils.LoadFilesAsync(this, containerNames, fileNames);
	}

	public UserStorageUtils.SaveOperation SaveFilesAsync(string containerName, Dictionary<string, byte[]> files)
	{
		UserStorageUtils.SaveOperation saveOperation = new UserStorageUtils.SaveOperation();
		ioThread.Enqueue(SaveFilesAsyncImplDelegate, this, new GenericWrapper(saveOperation, savePath, containerName, null, files));
		return saveOperation;
	}

	private static void SaveFilesAsyncImpl(object owner, object state)
	{
		GenericWrapper obj = (GenericWrapper)state;
		UserStorageUtils.SaveOperation saveOperation = (UserStorageUtils.SaveOperation)obj.operation;
		Dictionary<string, byte[]> dictionary = (Dictionary<string, byte[]>)obj.state;
		string path = obj.savePath;
		string containerName = obj.containerName;
		string path2 = Path.Combine(path, containerName);
		if (!Directory.Exists(path2))
		{
			Directory.CreateDirectory(path2);
		}
		UserStorageUtils.Result result = UserStorageUtils.Result.Success;
		string errorMessage = null;
		Dictionary<string, byte[]>.Enumerator enumerator = dictionary.GetEnumerator();
		while (enumerator.MoveNext())
		{
			try
			{
				string saveFilePath = GetSaveFilePath(path, containerName, enumerator.Current.Key);
				byte[] value = enumerator.Current.Value;
				File.WriteAllBytes(saveFilePath, value);
			}
			catch (Exception ex)
			{
				result = UserStorageUtils.GetResultForException(ex);
				errorMessage = ex.Message;
			}
		}
		saveOperation.SetComplete(result, errorMessage);
	}

	public UserStorageUtils.AsyncOperation CreateContainerAsync(string containerName)
	{
		UserStorageUtils.AsyncOperation asyncOperation = new UserStorageUtils.AsyncOperation();
		ioThread.Enqueue(CreateContainerAsyncImplDelegate, this, new Wrapper(asyncOperation, savePath, containerName));
		return asyncOperation;
	}

	private static void CreateContainerAsyncImpl(object owner, object state)
	{
		Wrapper obj = (Wrapper)state;
		UserStorageUtils.AsyncOperation operation = obj.operation;
		string path = obj.savePath;
		string containerName = obj.containerName;
		string path2 = Path.Combine(path, containerName);
		UserStorageUtils.Result result = UserStorageUtils.Result.Success;
		string errorMessage = null;
		if (!Directory.Exists(path2))
		{
			try
			{
				Directory.CreateDirectory(path2);
			}
			catch (Exception ex)
			{
				result = UserStorageUtils.GetResultForException(ex);
				errorMessage = ex.Message;
			}
		}
		operation.SetComplete(result, errorMessage);
	}

	public UserStorageUtils.AsyncOperation DeleteContainerAsync(string containerName)
	{
		UserStorageUtils.AsyncOperation asyncOperation = new UserStorageUtils.AsyncOperation();
		ioThread.Enqueue(DeleteContainerAsyncImplDelegate, this, new Wrapper(asyncOperation, savePath, containerName));
		return asyncOperation;
	}

	private static void DeleteContainerAsyncImpl(object owner, object state)
	{
		Wrapper obj = (Wrapper)state;
		UserStorageUtils.AsyncOperation operation = obj.operation;
		string path = obj.savePath;
		string containerName = obj.containerName;
		string path2 = Path.Combine(path, containerName);
		if (Directory.Exists(path2))
		{
			Directory.Delete(path2, recursive: true);
		}
		operation.SetComplete(UserStorageUtils.Result.Success, null);
	}

	public UserStorageUtils.AsyncOperation DeleteFilesAsync(string containerName, List<string> filePaths)
	{
		UserStorageUtils.AsyncOperation asyncOperation = new UserStorageUtils.AsyncOperation();
		ioThread.Enqueue(DeleteFilesAsyncImplDelegate, this, new GenericWrapper(asyncOperation, savePath, containerName, filePaths));
		return asyncOperation;
	}

	private static void DeleteFilesAsyncImpl(object owner, object state)
	{
		GenericWrapper obj = (GenericWrapper)state;
		UserStorageUtils.AsyncOperation operation = obj.operation;
		string path = obj.savePath;
		string containerName = obj.containerName;
		foreach (string file in obj.files)
		{
			string path2 = Path.Combine(Path.Combine(path, containerName), file);
			if (File.Exists(path2))
			{
				File.Delete(path2);
			}
		}
		operation.SetComplete(UserStorageUtils.Result.Success, null);
	}

	public UserStorageUtils.CopyOperation CopyFilesFromContainerAsync(string containerName, string destinationPath)
	{
		UserStorageUtils.CopyOperation copyOperation = new UserStorageUtils.CopyOperation();
		ioThread.Enqueue(CopyFilesFromContainerAsyncImplDelegate, this, new GenericWrapper(copyOperation, savePath, containerName, null, destinationPath));
		return copyOperation;
	}

	public UserStorageUtils.SaveOperation CopyFilesToContainerAsync(string containerName, string srcPath, List<string> updatedFiles, List<string> deletedFiles, List<string> unchangedFiles = null)
	{
		UserStorageUtils.SaveOperation saveOperation = new UserStorageUtils.SaveOperation();
		ioThread.Enqueue(CopyFilesToContainerAsyncImplDelegate, this, new CopyFilesToContainerWrapper(saveOperation, savePath, containerName, srcPath, updatedFiles, deletedFiles, unchangedFiles));
		return saveOperation;
	}

	private static void CopyFilesFromContainerAsyncImpl(object owner, object state)
	{
		GenericWrapper obj = (GenericWrapper)state;
		UserStorageUtils.CopyOperation copyOperation = (UserStorageUtils.CopyOperation)obj.operation;
		string text = (string)obj.state;
		string path = obj.savePath;
		string containerName = obj.containerName;
		UserStorageUtils.Result result = UserStorageUtils.Result.Success;
		string errorMessage = null;
		string text2 = Path.Combine(path, containerName);
		try
		{
			if (!Directory.Exists(text2))
			{
				throw new Exception("CopyFilesFromContainerAsync: SourcePath='" + text2 + "' DNE!");
			}
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string[] directories = Directory.GetDirectories(text2, "*", SearchOption.AllDirectories);
			for (int i = 0; i < directories.Length; i++)
			{
				Directory.CreateDirectory(directories[i].Replace(text2, text));
			}
			byte[] array = new byte[4096];
			directories = Directory.GetFiles(text2, "*.*", SearchOption.AllDirectories);
			foreach (string text3 in directories)
			{
				if (Path.GetExtension(text3) == ".zip")
				{
					string directoryName = Path.GetDirectoryName(text3.Replace(text2, text));
					DecompressFilesFromZip(text3, directoryName, array);
					continue;
				}
				string path2 = text3.Replace(text2, text);
				byte[] array2 = File.ReadAllBytes(text3);
				using (FileStream streamDestination = File.Create(path2))
				{
					UserStorageUtils.DecompressToStream(array2, array2.Length, streamDestination, array);
				}
			}
		}
		catch (Exception ex)
		{
			result = UserStorageUtils.GetResultForException(ex);
			errorMessage = ex.Message;
		}
		copyOperation.SetComplete(result, errorMessage);
	}

	private static void CopyFilesToContainerAsyncImpl(object owner, object state)
	{
		CopyFilesToContainerWrapper obj = (CopyFilesToContainerWrapper)state;
		UserStorageUtils.SaveOperation saveOperation = (UserStorageUtils.SaveOperation)obj.operation;
		string sourcePath = obj.sourcePath;
		string path = obj.savePath;
		string containerName = obj.containerName;
		List<string> updatedFiles = obj.updatedFiles;
		List<string> deletedFiles = obj.deletedFiles;
		List<string> unchangedFiles = obj.unchangedFiles;
		UserStorageUtils.Result result = UserStorageUtils.Result.Success;
		string errorMessage = null;
		string text = Path.Combine(path, containerName);
		try
		{
			List<string> list = new List<string>();
			foreach (string item in updatedFiles)
			{
				if (!UserStorageUtils.TryAddFileToBatchCellsRowList(item, list))
				{
					string sourceFileName = Path.Combine(sourcePath, item);
					string text2 = Path.Combine(text, item);
					Directory.CreateDirectory(Path.GetDirectoryName(text2));
					File.Copy(sourceFileName, text2, overwrite: true);
				}
			}
			foreach (string item2 in deletedFiles)
			{
				if (!UserStorageUtils.TryAddFileToBatchCellsRowList(item2, list))
				{
					string path2 = Path.Combine(text, item2);
					if (File.Exists(path2))
					{
						File.Delete(path2);
					}
				}
			}
			CompressUpdatedRowListToZip(sourcePath, text, list, updatedFiles, unchangedFiles);
		}
		catch (Exception ex)
		{
			result = UserStorageUtils.GetResultForException(ex);
			errorMessage = ex.Message;
		}
		saveOperation.SetComplete(result, errorMessage);
	}

	public UserStorageUtils.UpgradeOperation UpgradeSaveDataAsync(string backupPath = null)
	{
		List<string> list = new List<string>();
		if (Directory.Exists(savePath))
		{
			string[] directories = Directory.GetDirectories(savePath);
			foreach (string text in directories)
			{
				if (!(Path.GetFileName(text) == "options") && (Directory.EnumerateFiles(text, "batch-objects-*.*", SearchOption.AllDirectories).Any() || Directory.EnumerateFiles(text, "baked-batch-cells-*.bin", SearchOption.AllDirectories).Any() || Directory.EnumerateFiles(text, "compiled-batch-*.optoctrees", SearchOption.AllDirectories).Any()))
				{
					list.Add(text);
				}
			}
		}
		UserStorageUtils.UpgradeOperation upgradeOperation = new UserStorageUtils.UpgradeOperation();
		upgradeOperation.itemsTotal = list.Count;
		ioThread.Enqueue(UpgradeSaveDataAsyncImplDelegate, this, new UpgradeWrapper(upgradeOperation, savePath, backupPath, list));
		return upgradeOperation;
	}

	private static void UpgradeSaveDataAsyncImpl(object owner, object state)
	{
		UpgradeWrapper obj = (UpgradeWrapper)state;
		UserStorageUtils.UpgradeOperation upgradeOperation = (UserStorageUtils.UpgradeOperation)obj.operation;
		string path = obj.savePath;
		string backupPath = obj.backupPath;
		List<string> containersToUpgrade = obj.containersToUpgrade;
		bool flag = !string.IsNullOrEmpty(backupPath);
		UserStorageUtils.Result result = UserStorageUtils.Result.Success;
		string errorMessage = null;
		if (Directory.Exists(path))
		{
			if (flag)
			{
				try
				{
					Directory.CreateDirectory(backupPath);
				}
				catch (Exception ex)
				{
					result = UserStorageUtils.GetResultForException(ex);
					errorMessage = $"Failed to create backup directory: {ex.Message}";
					upgradeOperation.SetComplete(result, errorMessage);
					return;
				}
			}
			foreach (string item in containersToUpgrade)
			{
				string fileName = Path.GetFileName(item);
				try
				{
					if (flag)
					{
						string defaultPath = Path.Combine(backupPath, fileName);
						defaultPath = GetNonExistingDirectoryPath(defaultPath);
						UWE.Utils.CopyDirectory(item, defaultPath);
					}
					UpgradeSaveData(item);
				}
				catch (Exception ex2)
				{
					result = UserStorageUtils.GetResultForException(ex2);
					errorMessage = $"Failed to upgrade save slot {fileName}: {ex2.Message}";
				}
				upgradeOperation.itemsPrecessed++;
			}
		}
		upgradeOperation.SetComplete(result, errorMessage);
	}

	private static void UpgradeSaveData(string saveDir)
	{
		if (!Directory.Exists(saveDir))
		{
			return;
		}
		List<string> list = new List<string>();
		string[] files = Directory.GetFiles(saveDir, "*.*", SearchOption.AllDirectories);
		foreach (string text in files)
		{
			if (!(Path.GetExtension(text) == ".zip"))
			{
				UserStorageUtils.TryAddFileToBatchCellsRowList(text, list);
			}
		}
		if (list.Count > 0)
		{
			byte[] buffer = new byte[4096];
			foreach (string item in list)
			{
				string text2 = $"{item}-grp0.zip";
				if (File.Exists(text2))
				{
					string directoryName = Path.GetDirectoryName(text2);
					DecompressFilesFromZip(text2, directoryName, buffer);
				}
				string directoryName2 = Path.GetDirectoryName(item);
				string searchPattern = $"*{Path.GetFileName(item)}-*.bin";
				string[] files2 = Directory.GetFiles(directoryName2, searchPattern);
				CompressFilesToZip(files2, Path.GetDirectoryName(item), text2, buffer);
				files = files2;
				for (int i = 0; i < files.Length; i++)
				{
					File.Delete(files[i]);
				}
			}
		}
		files = Directory.GetFiles(saveDir, "batch-objects-*.*", SearchOption.AllDirectories);
		for (int i = 0; i < files.Length; i++)
		{
			File.Delete(files[i]);
		}
		files = Directory.GetFiles(saveDir, "compiled-batch-*.optoctrees", SearchOption.AllDirectories);
		for (int i = 0; i < files.Length; i++)
		{
			File.Delete(files[i]);
		}
	}

	private static string GetNonExistingDirectoryPath(string defaultPath)
	{
		string text = defaultPath;
		int num = 1;
		while (Directory.Exists(text))
		{
			text = $"{defaultPath}_{num}";
			num++;
		}
		return text;
	}

	private static void CompressUpdatedRowListToZip(string sourcePath, string destinationPath, List<string> updatedRowList, List<string> updatedFiles, List<string> unchangedFiles)
	{
		List<string> list = new List<string>();
		byte[] buffer = new byte[4096];
		foreach (string updatedRow in updatedRowList)
		{
			UserStorageUtils.CollectBatchCellFilesByRow(updatedRow, updatedFiles, unchangedFiles, list);
			string text = $"{destinationPath}/{updatedRow}-grp0.zip";
			string directoryName = Path.GetDirectoryName(text);
			string searchPattern = $"*{Path.GetFileName(updatedRow)}-*.zip";
			string[] files;
			if (Directory.Exists(directoryName))
			{
				files = Directory.GetFiles(directoryName, searchPattern);
				for (int i = 0; i < files.Length; i++)
				{
					File.Delete(files[i]);
				}
			}
			CompressFilesToZip(list, sourcePath, text, buffer);
			list.Clear();
			searchPattern = $"*{Path.GetFileName(updatedRow)}-*.bin";
			files = Directory.GetFiles(directoryName, searchPattern);
			for (int i = 0; i < files.Length; i++)
			{
				File.Delete(files[i]);
			}
		}
	}

	private static void CompressFilesToZip(IList<string> filesToCompress, string sourcePath, string destinationFileName, byte[] buffer)
	{
		if (filesToCompress.Count == 0)
		{
			return;
		}
		Directory.CreateDirectory(Path.GetDirectoryName(destinationFileName));
		using (ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(destinationFileName)))
		{
			foreach (string item in filesToCompress)
			{
				byte[] array = UserStorageUtils.CompressIfUncompressed(File.ReadAllBytes(Path.Combine(sourcePath, item)), buffer);
				ZipEntry entry = new ZipEntry(Path.GetFileName(item))
				{
					DateTime = DateTime.Now,
					CompressionMethod = CompressionMethod.Stored
				};
				zipOutputStream.PutNextEntry(entry);
				zipOutputStream.Write(array, 0, array.Length);
			}
			zipOutputStream.Finish();
			zipOutputStream.Close();
		}
	}

	private static void DecompressFilesFromZip(string sourceFilePath, string destinationPath, byte[] buffer)
	{
		if (!File.Exists(sourceFilePath))
		{
			return;
		}
		using (Stream inputStream = File.OpenRead(sourceFilePath))
		{
			UserStorageUtils.TryDecompressFromZip(inputStream, destinationPath, buffer);
		}
	}
}
