using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Gendarme;
using ICSharpCode.SharpZipLib.Zip;
using UWE;
using UnityEngine;
using XGamingRuntime;

public sealed class UserStorageXSave : UserStorage
{
	private delegate void OperationResultCallback(UserStorageUtils.Result result, string errorMessage);

	private class SaveUpdate
	{
		private readonly XGameSaveContainerHandle containerHandle;

		private readonly string containerDisplayName;

		private XGameSaveUpdateHandle updateHandle;

		private int blockLength;

		public int saveDataSize;

		public SaveUpdate(XGameSaveContainerHandle containerHandle, string containerDisplayName, out UserStorageUtils.Result result, out string errorMessage)
		{
			this.containerHandle = containerHandle;
			this.containerDisplayName = containerDisplayName;
			int errorCode = SDK.XGameSaveCreateUpdate(containerHandle, containerDisplayName, out updateHandle);
			result = GetResultForErrorCode(errorCode, "XGameSaveCreateUpdate");
			errorMessage = ((result == UserStorageUtils.Result.Success) ? null : GetErrorMessageForErrorCode(errorCode));
		}

		public IEnumerator AddFile(string fileName, byte[] fileData, OperationResultCallback onComplete)
		{
			int len = fileData.Length;
			if (len > 16777216)
			{
				string text = $"File '{fileName}' exceeds the maximum size limit";
				Debug.LogError(text);
				onComplete?.Invoke(UserStorageUtils.Result.UnknownError, text);
				yield break;
			}
			int hresult;
			UserStorageUtils.Result resultForErrorCode;
			if (blockLength + len > 16777216)
			{
				CallbackAwaiter<UserStorageUtils.Result, string> awaiter = new CallbackAwaiter<UserStorageUtils.Result, string>();
				ioThread.StartCoroutine(Submit(awaiter.Call));
				yield return awaiter;
				if (awaiter.arg1 != 0)
				{
					onComplete?.Invoke(awaiter.arg1, awaiter.arg2);
					yield break;
				}
				CreateNewHandle(out hresult);
				resultForErrorCode = GetResultForErrorCode(hresult, "XGameSaveCreateUpdate");
				if (resultForErrorCode != 0)
				{
					onComplete?.Invoke(resultForErrorCode, GetErrorMessageForErrorCode(hresult));
					yield break;
				}
			}
			hresult = SDK.XGameSaveSubmitBlobWrite(updateHandle, fileName, fileData);
			resultForErrorCode = GetResultForErrorCode(hresult, "XGameSaveSubmitBlobWrite");
			saveDataSize += len;
			blockLength += len;
			onComplete?.Invoke(resultForErrorCode, (resultForErrorCode == UserStorageUtils.Result.Success) ? null : GetErrorMessageForErrorCode(hresult));
		}

		public IEnumerator Submit(OperationResultCallback onComplete)
		{
			CallbackAwaiter<int> awaiter = new CallbackAwaiter<int>();
			SDK.XGameSaveSubmitUpdateAsync(updateHandle, awaiter.Call);
			yield return awaiter;
			UserStorageUtils.Result resultForErrorCode = GetResultForErrorCode(awaiter.arg1, "XGameSaveSubmitUpdateAsync");
			onComplete?.Invoke(resultForErrorCode, (resultForErrorCode == UserStorageUtils.Result.Success) ? null : GetErrorMessageForErrorCode(awaiter.arg1));
		}

		private void CreateNewHandle(out int hresult)
		{
			blockLength = 0;
			SDK.XGameSaveCloseUpdateHandle(updateHandle);
			updateHandle = null;
			hresult = SDK.XGameSaveCreateUpdate(containerHandle, containerDisplayName, out updateHandle);
		}
	}

	private const int compressionBufferSize = 4096;

	private readonly string serviceConfigurationId;

	private XUserHandle currentUser;

	private XGameSaveProviderHandle provider;

	private readonly List<string> slotFileNames = new List<string>();

	private const int maximumSyncSize = 16777216;

	private const int maxFileNamesPerDelete = 1000;

	private const long _maxSafeZipDataSize = 16515072L;

	private static readonly WorkerThread ioThread = ThreadUtils.StartWorkerThread("I/O", "XSave", System.Threading.ThreadPriority.BelowNormal, -4, 128);

	private static UserStorageUtils.Result GetResultForErrorCode(int errorCode, string source = "XSave")
	{
		switch ((uint)errorCode)
		{
		case 2156068868u:
		case 0u:
			return UserStorageUtils.Result.Success;
		case 2156068866u:
			return UserStorageUtils.Result.NoAccess;
		case 2156068872u:
			return UserStorageUtils.Result.NotFound;
		case 2147500036u:
		case 2147942487u:
			return UserStorageUtils.Result.NotSignedIn;
		default:
			Debug.LogErrorFormat("{0} error code {1:X8}", source, errorCode);
			return UserStorageUtils.Result.UnknownError;
		}
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	private static string GetErrorMessageForErrorCode(int errorCode)
	{
		if (errorCode == 0)
		{
			return null;
		}
		return $"Error #{errorCode:X8}";
	}

	private static bool Ensure(UserStorageUtils.AsyncOperation op, object handler, string functionName, int hresult)
	{
		if (handler != null)
		{
			return false;
		}
		Debug.LogErrorFormat("{0} returned null", functionName);
		op.SetComplete(GetResultForErrorCode(hresult, functionName), GetErrorMessageForErrorCode(hresult));
		return true;
	}

	private static void SetComplete(UserStorageUtils.AsyncOperation op, int errorCode)
	{
		UserStorageUtils.Result resultForErrorCode = GetResultForErrorCode(errorCode);
		string errorMessageForErrorCode = GetErrorMessageForErrorCode(errorCode);
		op.SetComplete(resultForErrorCode, errorMessageForErrorCode);
	}

	private static void SetComplete(UserStorageUtils.AsyncOperation op, Exception exception)
	{
		UserStorageUtils.Result resultForException = UserStorageUtils.GetResultForException(exception);
		string message = exception.Message;
		op.SetComplete(resultForException, message);
	}

	private static string GetSlotCacheFileName(string container, string fileName)
	{
		return $"{container}-{fileName}";
	}

	private void DisposeConnectedStorage()
	{
		if (provider != null)
		{
			SDK.XGameSaveCloseProvider(provider);
			provider = null;
		}
	}

	public UserStorageXSave(string serviceConfigurationId)
	{
		this.serviceConfigurationId = serviceConfigurationId;
	}

	public void SetCurrentUser(XUserHandle _currentUser)
	{
		currentUser = _currentUser;
	}

	public UserStorageUtils.AsyncOperation InitializeAsync()
	{
		UserStorageUtils.AsyncOperation asyncOperation = new UserStorageUtils.AsyncOperation();
		ioThread.StartCoroutine(InitializeAsync(asyncOperation, this, currentUser, serviceConfigurationId));
		return asyncOperation;
	}

	private static IEnumerator InitializeAsync(UserStorageUtils.AsyncOperation op, UserStorageXSave storage, XUserHandle user, string serviceConfigurationId)
	{
		yield return null;
		storage.DisposeConnectedStorage();
		CallbackAwaiter<int, XGameSaveProviderHandle> awaiter = new CallbackAwaiter<int, XGameSaveProviderHandle>();
		SDK.XGameSaveInitializeProviderAsync(user, serviceConfigurationId, syncOnDemand: false, awaiter.Call);
		yield return awaiter;
		switch (GetResultForErrorCode(awaiter.arg1, "XGameSaveInitializeProviderAsync"))
		{
		case UserStorageUtils.Result.Success:
			storage.provider = awaiter.arg2;
			break;
		case UserStorageUtils.Result.NotSignedIn:
			Debug.Log("UserStorageXSave: User not signed in...");
			break;
		}
		SetComplete(op, awaiter.arg1);
	}

	private int OpenOrCreateContainer(string containerName, out XGameSaveContainerHandle container)
	{
		return SDK.XGameSaveCreateContainer(provider, containerName, out container);
	}

	private void CloseContainer(XGameSaveContainerHandle container)
	{
		if (container != null)
		{
			SDK.XGameSaveCloseContainer(container);
		}
	}

	public UserStorageUtils.QueryOperation GetContainerNamesAsync()
	{
		UserStorageUtils.QueryOperation queryOperation = new UserStorageUtils.QueryOperation();
		ioThread.StartCoroutine(GetContainerNamesAsync(queryOperation, provider));
		return queryOperation;
	}

	private static IEnumerator GetContainerNamesAsync(UserStorageUtils.QueryOperation op, XGameSaveProviderHandle storage)
	{
		yield return null;
		XGameSaveContainerInfo[] containerInfos;
		int errorCode = SDK.XGameSaveEnumerateContainerInfo(storage, out containerInfos);
		if (GetResultForErrorCode(errorCode, "XGameSaveEnumerateContainerInfo") == UserStorageUtils.Result.Success)
		{
			op.results.AddRange(from info in containerInfos
				select info.Name into name
				where name != "options"
				select name);
		}
		SetComplete(op, errorCode);
	}

	public UserStorageUtils.LoadOperation LoadFilesAsync(string containerName, List<string> fileNames)
	{
		UserStorageUtils.LoadOperation loadOperation = new UserStorageUtils.LoadOperation();
		ioThread.StartCoroutine(LoadFilesAsync(loadOperation, this, containerName, fileNames));
		return loadOperation;
	}

	private static IEnumerator LoadFilesAsync(UserStorageUtils.LoadOperation op, UserStorageXSave storage, string containerName, List<string> fileNames, Action onComplete = null)
	{
		yield return null;
		XGameSaveContainerHandle container;
		int hresult = storage.OpenOrCreateContainer(containerName, out container);
		if (Ensure(op, container, "OpenOrCreateContainer", hresult))
		{
			onComplete?.Invoke();
			yield break;
		}
		string[] blobNames = fileNames.Select(ContainerUtils.EncodeFileName).ToArray();
		CallbackAwaiter<int, XGameSaveBlob[]> awaiter = new CallbackAwaiter<int, XGameSaveBlob[]>();
		SDK.XGameSaveReadBlobDataAsync(container, blobNames, awaiter.Call);
		yield return awaiter;
		if (GetResultForErrorCode(awaiter.arg1, "XGameSaveReadBlobDataAsync") == UserStorageUtils.Result.Success)
		{
			byte[] compressBuffer = new byte[4096];
			XGameSaveBlob[] arg = awaiter.arg2;
			foreach (XGameSaveBlob xGameSaveBlob in arg)
			{
				string key = ContainerUtils.DecodeFileName(xGameSaveBlob.Info.Name);
				op.files[key] = CompressionUtils.Decompress(xGameSaveBlob.Data, compressBuffer);
			}
		}
		storage.CloseContainer(container);
		SetComplete(op, awaiter.arg1);
		onComplete?.Invoke();
	}

	public UserStorageUtils.SlotsOperation LoadSlotsAsync(List<string> containerNames, List<string> fileNames)
	{
		return UserStorageUtils.LoadFilesAsync(this, containerNames, fileNames);
	}

	public UserStorageUtils.SaveOperation SaveFilesAsync(string containerName, Dictionary<string, byte[]> files)
	{
		UserStorageUtils.SaveOperation saveOperation = new UserStorageUtils.SaveOperation();
		ioThread.StartCoroutine(SaveFilesAsync(saveOperation, this, containerName, files));
		return saveOperation;
	}

	private static IEnumerator SaveFilesAsync(UserStorageUtils.SaveOperation op, UserStorageXSave storage, string containerName, Dictionary<string, byte[]> files)
	{
		yield return null;
		int hresult = storage.OpenOrCreateContainer(containerName, out var container);
		if (Ensure(op, container, "OpenOrCreateContainer", hresult))
		{
			yield break;
		}
		hresult = SDK.XGameSaveCreateUpdate(container, containerName, out var updateHandle);
		if (GetResultForErrorCode(hresult, "XGameSaveCreateUpdate") == UserStorageUtils.Result.Success)
		{
			byte[] compressBuffer = new byte[4096];
			foreach (KeyValuePair<string, byte[]> file in files)
			{
				string blobName = ContainerUtils.EncodeFileName(file.Key);
				byte[] array = CompressionUtils.Compress(file.Value, compressBuffer);
				hresult = SDK.XGameSaveSubmitBlobWrite(updateHandle, blobName, array);
				if (GetResultForErrorCode(hresult, "XGameSaveSubmitBlobWrite") != 0)
				{
					break;
				}
				op.saveDataSize += array.Length;
			}
		}
		if (GetResultForErrorCode(hresult) == UserStorageUtils.Result.Success)
		{
			CallbackAwaiter<int> awaiter = new CallbackAwaiter<int>();
			SDK.XGameSaveSubmitUpdateAsync(updateHandle, awaiter.Call);
			yield return awaiter;
			hresult = awaiter.arg1;
			GetResultForErrorCode(hresult, "XGameSaveSubmitUpdateAsync");
		}
		if (updateHandle != null)
		{
			SDK.XGameSaveCloseUpdateHandle(updateHandle);
		}
		storage.CloseContainer(container);
		SetComplete(op, hresult);
	}

	public UserStorageUtils.SaveOperation CopyFilesToContainerAsync(string containerName, string sourcePath, List<string> updatedFiles, List<string> deletedFiles, List<string> unchangedFiles)
	{
		UserStorageUtils.SaveOperation saveOperation = new UserStorageUtils.SaveOperation();
		ioThread.StartCoroutine(CopyFilesToContainerAsync(saveOperation, this, containerName, sourcePath, updatedFiles, deletedFiles, unchangedFiles));
		return saveOperation;
	}

	private static IEnumerator CopyFilesToContainerAsync(UserStorageUtils.SaveOperation op, UserStorageXSave storage, string containerName, string sourcePath, List<string> updatedFiles, List<string> deletedFiles, List<string> unchangedFiles)
	{
		yield return null;
		UserStorageUtils.Result result = UserStorageUtils.Result.Success;
		string errorMessage = null;
		XGameSaveContainerHandle container;
		int hresult = storage.OpenOrCreateContainer(containerName, out container);
		if (Ensure(op, container, "OpenOrCreateContainer", hresult))
		{
			yield break;
		}
		SDK.XGameSaveEnumerateBlobInfo(container, out var blobInfos);
		Dictionary<string, byte[]> slotFilesToCache = new Dictionary<string, byte[]>();
		List<string> list = new List<string>();
		List<string> updatedMiscFiles = new List<string>();
		List<string> deletedMiscFiles = new List<string>();
		foreach (string updatedFile in updatedFiles)
		{
			if (!UserStorageUtils.TryAddFileToBatchCellsRowList(updatedFile, list))
			{
				updatedMiscFiles.Add(updatedFile);
			}
		}
		foreach (string deletedFile in deletedFiles)
		{
			if (!UserStorageUtils.TryAddFileToBatchCellsRowList(deletedFile, list))
			{
				deletedMiscFiles.Add(deletedFile);
			}
		}
		SaveUpdate saveUpdate = new SaveUpdate(container, containerName, out result, out errorMessage);
		if (result == UserStorageUtils.Result.Success)
		{
			byte[] buffer = new byte[4096];
			List<string> filesToCompress = new List<string>();
			List<string> encodedZipFileNames = new List<string>();
			Debug.Log($"UserStorageXSave::CopyFilesToContainerAsync - compressing batch cells for {list.Count} rows");
			foreach (string updatedRow2 in list)
			{
				UserStorageUtils.CollectBatchCellFilesByRow(updatedRow2, updatedFiles, unchangedFiles, filesToCompress);
				CallbackAwaiter<UserStorageUtils.Result, string> awaiter3 = new CallbackAwaiter<UserStorageUtils.Result, string>();
				ioThread.StartCoroutine(CompressFilesToZip(sourcePath, updatedRow2, filesToCompress, saveUpdate, awaiter3.Call, encodedZipFileNames));
				yield return awaiter3;
				if (awaiter3.arg1 != 0)
				{
					result = awaiter3.arg1;
					errorMessage = awaiter3.arg2;
					break;
				}
				if (blobInfos != null)
				{
					string value = ContainerUtils.EncodeFileName(updatedRow2 + "-");
					XGameSaveBlobInfo[] array = blobInfos;
					for (int i = 0; i < array.Length; i++)
					{
						string name = array[i].Name;
						if (!encodedZipFileNames.Contains(name) && name.Contains(value))
						{
							deletedMiscFiles.Add(ContainerUtils.DecodeFileName(name));
						}
					}
				}
				filesToCompress.Clear();
				encodedZipFileNames.Clear();
			}
			Debug.Log("UserStorageXSave::CopyFilesToContainerAsync - saving misc files");
			foreach (string updatedRow2 in updatedMiscFiles)
			{
				string path = Path.Combine(sourcePath, updatedRow2);
				string fileName = ContainerUtils.EncodeFileName(updatedRow2);
				byte[] uncompressed = File.ReadAllBytes(path);
				byte[] fileData = CompressionUtils.Compress(uncompressed, buffer);
				CallbackAwaiter<UserStorageUtils.Result, string> awaiter3 = new CallbackAwaiter<UserStorageUtils.Result, string>();
				ioThread.StartCoroutine(saveUpdate.AddFile(fileName, fileData, awaiter3.Call));
				yield return awaiter3;
				if (awaiter3.arg1 != 0)
				{
					result = awaiter3.arg1;
					errorMessage = awaiter3.arg2;
					break;
				}
				if (storage.slotFileNames.Contains(updatedRow2))
				{
					slotFilesToCache.Add(GetSlotCacheFileName(containerName, updatedRow2), uncompressed);
				}
			}
		}
		if (result == UserStorageUtils.Result.Success)
		{
			CallbackAwaiter<UserStorageUtils.Result, string> awaiter3 = new CallbackAwaiter<UserStorageUtils.Result, string>();
			ioThread.StartCoroutine(saveUpdate.Submit(awaiter3.Call));
			yield return awaiter3;
			result = awaiter3.arg1;
			errorMessage = awaiter3.arg2;
			op.saveDataSize = saveUpdate.saveDataSize;
		}
		storage.CloseContainer(container);
		if (op.done)
		{
			yield break;
		}
		if (result != 0)
		{
			Debug.Log("UserStorageXSave::CopyFilesToContainerAsync - not success 3");
			op.SetComplete(result, errorMessage);
			yield break;
		}
		Debug.Log("UserStorageXSave::CopyFilesToContainerAsync - deleting misc files");
		if (deletedFiles.Count > 0)
		{
			UserStorageUtils.AsyncOperation deleteOp = new UserStorageUtils.AsyncOperation();
			CallbackAwaiter callbackAwaiter = new CallbackAwaiter();
			ioThread.StartCoroutine(DeleteFilesAsync(deleteOp, storage, containerName, deletedMiscFiles, callbackAwaiter.Call));
			yield return callbackAwaiter;
			if (!deleteOp.GetSuccessful())
			{
				op.SetComplete(deleteOp.result, deleteOp.errorMessage);
				yield break;
			}
		}
		if (slotFilesToCache.Count > 0)
		{
			ioThread.StartCoroutine(SaveFilesAsync(op, storage, "options", slotFilesToCache));
		}
		else
		{
			op.SetComplete(result, errorMessage);
		}
	}

	private static IEnumerator CompressFilesToZip(string sourcePath, string updatedRow, IList<string> filesToCompress, SaveUpdate saveUpdate, OperationResultCallback onComplete, List<string> encodedZipFileNames)
	{
		if (filesToCompress.Count == 0)
		{
			onComplete?.Invoke(UserStorageUtils.Result.Success, null);
			yield break;
		}
		int groupNumber = 0;
		MemoryStream memoryStream = new MemoryStream();
		ZipOutputStream zipOutputStream = new ZipOutputStream(memoryStream);
		byte[] compressBuffer = new byte[4096];
		string fileName2;
		string encodedZipFileName2;
		CallbackAwaiter<UserStorageUtils.Result, string> awaiter2;
		foreach (string fileName in filesToCompress)
		{
			byte[] compressed;
			try
			{
				byte[] input = File.ReadAllBytes(Path.Combine(sourcePath, fileName));
				compressed = UserStorageUtils.Compress(input, compressBuffer);
			}
			catch (Exception ex)
			{
				zipOutputStream.Close();
				onComplete?.Invoke(UserStorageUtils.Result.UnknownError, ex.Message);
				yield break;
			}
			if ((long)compressed.Length > 16515072L)
			{
				string text = $"File '{fileName}' exceeds the maximum size limit";
				Debug.LogError(text);
				onComplete?.Invoke(UserStorageUtils.Result.UnknownError, text);
				yield break;
			}
			if (zipOutputStream.Length + compressed.Length > 16515072)
			{
				zipOutputStream.Finish();
				zipOutputStream.Close();
				fileName2 = $"{updatedRow}-grp{groupNumber}.zip";
				encodedZipFileName2 = ContainerUtils.EncodeFileName(fileName2);
				encodedZipFileNames.Add(encodedZipFileName2);
				awaiter2 = new CallbackAwaiter<UserStorageUtils.Result, string>();
				ioThread.StartCoroutine(saveUpdate.AddFile(encodedZipFileName2, memoryStream.ToArray(), awaiter2.Call));
				yield return awaiter2;
				if (awaiter2.arg1 != 0)
				{
					onComplete?.Invoke(awaiter2.arg1, awaiter2.arg2);
					yield break;
				}
				memoryStream = new MemoryStream();
				zipOutputStream = new ZipOutputStream(memoryStream);
				groupNumber++;
			}
			try
			{
				ZipEntry entry = new ZipEntry(Path.GetFileName(fileName))
				{
					DateTime = DateTime.Now,
					CompressionMethod = CompressionMethod.Stored
				};
				zipOutputStream.PutNextEntry(entry);
				zipOutputStream.Write(compressed, 0, compressed.Length);
			}
			catch (Exception ex2)
			{
				zipOutputStream.Close();
				onComplete?.Invoke(UserStorageUtils.Result.UnknownError, ex2.Message);
				yield break;
			}
		}
		zipOutputStream.Finish();
		zipOutputStream.Close();
		fileName2 = $"{updatedRow}-grp{groupNumber}.zip";
		encodedZipFileName2 = ContainerUtils.EncodeFileName(fileName2);
		awaiter2 = new CallbackAwaiter<UserStorageUtils.Result, string>();
		ioThread.StartCoroutine(saveUpdate.AddFile(encodedZipFileName2, memoryStream.ToArray(), awaiter2.Call));
		yield return awaiter2;
		encodedZipFileNames.Add(encodedZipFileName2);
		onComplete?.Invoke(awaiter2.arg1, awaiter2.arg2);
	}

	public UserStorageUtils.AsyncOperation CreateContainerAsync(string containerName)
	{
		UserStorageUtils.AsyncOperation asyncOperation = new UserStorageUtils.AsyncOperation();
		ioThread.StartCoroutine(CreateContainerAsync(asyncOperation, this, containerName));
		return asyncOperation;
	}

	private static IEnumerator CreateContainerAsync(UserStorageUtils.AsyncOperation op, UserStorageXSave storage, string containerName)
	{
		yield return null;
		XGameSaveContainerHandle container;
		int hresult = storage.OpenOrCreateContainer(containerName, out container);
		if (!Ensure(op, container, "OpenOrCreateContainer", hresult))
		{
			storage.CloseContainer(container);
			op.SetComplete(UserStorageUtils.Result.Success, null);
		}
	}

	public UserStorageUtils.AsyncOperation DeleteContainerAsync(string containerName)
	{
		UserStorageUtils.AsyncOperation asyncOperation = new UserStorageUtils.AsyncOperation();
		ioThread.StartCoroutine(DeleteContainerAsync(asyncOperation, provider, containerName));
		return asyncOperation;
	}

	private static IEnumerator DeleteContainerAsync(UserStorageUtils.AsyncOperation op, XGameSaveProviderHandle provider, string containerName)
	{
		yield return null;
		CallbackAwaiter<int> awaiter = new CallbackAwaiter<int>();
		SDK.XGameSaveDeleteContainerAsync(provider, containerName, awaiter.Call);
		yield return awaiter;
		SetComplete(op, awaiter.arg1);
	}

	public UserStorageUtils.AsyncOperation DeleteFilesAsync(string containerName, List<string> fileNames)
	{
		UserStorageUtils.AsyncOperation asyncOperation = new UserStorageUtils.AsyncOperation();
		ioThread.StartCoroutine(DeleteFilesAsync(asyncOperation, this, containerName, fileNames));
		return asyncOperation;
	}

	private static IEnumerator DeleteFilesAsync(UserStorageUtils.AsyncOperation op, UserStorageXSave storage, string containerName, List<string> fileNames, Action onComplete = null)
	{
		yield return null;
		int hresult = storage.OpenOrCreateContainer(containerName, out var container);
		if (Ensure(op, container, "OpenOrCreateContainer", hresult))
		{
			onComplete?.Invoke();
			yield break;
		}
		hresult = SDK.XGameSaveCreateUpdate(container, containerName, out var updateHandle);
		if (GetResultForErrorCode(hresult, "XGameSaveCreateUpdate") == UserStorageUtils.Result.Success)
		{
			List<string> encodedFileNames = new List<string>();
			foreach (string fileName in fileNames)
			{
				if (encodedFileNames.Count >= 1000)
				{
					foreach (string item in encodedFileNames)
					{
						hresult = SDK.XGameSaveSubmitBlobDelete(updateHandle, item);
						if (GetResultForErrorCode(hresult, "XGameSaveSubmitBlobDelete") != 0)
						{
							break;
						}
					}
					if (GetResultForErrorCode(hresult) == UserStorageUtils.Result.Success)
					{
						CallbackAwaiter<int> awaiter2 = new CallbackAwaiter<int>();
						SDK.XGameSaveSubmitUpdateAsync(updateHandle, awaiter2.Call);
						yield return awaiter2;
						hresult = awaiter2.arg1;
						if (GetResultForErrorCode(hresult, "XGameSaveSubmitUpdateAsync") == UserStorageUtils.Result.Success)
						{
							SDK.XGameSaveCloseUpdateHandle(updateHandle);
							hresult = SDK.XGameSaveCreateUpdate(container, containerName, out updateHandle);
							GetResultForErrorCode(hresult, "XGameSaveCreateUpdate");
						}
					}
					encodedFileNames.Clear();
				}
				if (GetResultForErrorCode(hresult) == UserStorageUtils.Result.Success)
				{
					string text = ContainerUtils.EncodeFileName(fileName);
					hresult = SDK.XGameSaveEnumerateBlobInfoByName(container, text, out var blobInfos);
					if (GetResultForErrorCode(hresult, "XGameSaveSubmitBlobDelete") != 0)
					{
						break;
					}
					if (blobInfos.Length != 0)
					{
						encodedFileNames.Add(text);
					}
				}
			}
			if (GetResultForErrorCode(hresult) == UserStorageUtils.Result.Success && encodedFileNames.Count > 0)
			{
				foreach (string item2 in encodedFileNames)
				{
					hresult = SDK.XGameSaveSubmitBlobDelete(updateHandle, item2);
					if (GetResultForErrorCode(hresult, "XGameSaveSubmitBlobDelete") != 0)
					{
						break;
					}
				}
				if (GetResultForErrorCode(hresult) == UserStorageUtils.Result.Success)
				{
					CallbackAwaiter<int> awaiter2 = new CallbackAwaiter<int>();
					SDK.XGameSaveSubmitUpdateAsync(updateHandle, awaiter2.Call);
					yield return awaiter2;
					hresult = awaiter2.arg1;
					GetResultForErrorCode(hresult, "XGameSaveSubmitUpdateAsync");
				}
			}
		}
		if (updateHandle != null)
		{
			SDK.XGameSaveCloseUpdateHandle(updateHandle);
		}
		storage.CloseContainer(container);
		SetComplete(op, hresult);
		onComplete?.Invoke();
	}

	public UserStorageUtils.CopyOperation CopyFilesFromContainerAsync(string containerName, string destinationPath)
	{
		UserStorageUtils.CopyOperation copyOperation = new UserStorageUtils.CopyOperation();
		ioThread.StartCoroutine(CopyFilesFromContainerAsync(copyOperation, this, containerName, destinationPath));
		return copyOperation;
	}

	private static IEnumerator CopyFilesFromContainerAsync(UserStorageUtils.CopyOperation op, UserStorageXSave storage, string containerName, string destinationPath)
	{
		yield return null;
		Debug.Log("CopyFilesFromContainerAsync: copy from container " + containerName);
		int hresult = storage.OpenOrCreateContainer(containerName, out var container);
		if (Ensure(op, container, "OpenOrCreateContainer", hresult))
		{
			yield break;
		}
		hresult = SDK.XGameSaveEnumerateBlobInfo(container, out var blobInfos);
		if (GetResultForErrorCode(hresult, "XGameSaveEnumerateBlobInfo") == UserStorageUtils.Result.Success)
		{
			string[] blobNames = blobInfos.Select((XGameSaveBlobInfo p) => p.Name).ToArray();
			CallbackAwaiter<int, XGameSaveBlob[]> awaiter = new CallbackAwaiter<int, XGameSaveBlob[]>();
			SDK.XGameSaveReadBlobDataAsync(container, blobNames, awaiter.Call);
			yield return awaiter;
			hresult = awaiter.arg1;
			if (GetResultForErrorCode(hresult, "XGameSaveReadBlobDataAsync") == UserStorageUtils.Result.Success)
			{
				XGameSaveBlob[] arg = awaiter.arg2;
				Debug.Log($"CopyFilesFromContainerAsync: retrieved {arg.Length} blobs");
				byte[] array = new byte[4096];
				XGameSaveBlob[] array2 = arg;
				foreach (XGameSaveBlob obj in array2)
				{
					byte[] data = obj.Data;
					string text = ContainerUtils.DecodeFileName(obj.Info.Name);
					string path = Path.Combine(destinationPath, text);
					string extension = Path.GetExtension(text);
					try
					{
						if (extension == ".zip")
						{
							MemoryStream inputStream = new MemoryStream(data);
							string directoryName = Path.GetDirectoryName(path);
							UserStorageUtils.TryDecompressFromZip(inputStream, directoryName, array);
						}
						else
						{
							byte[] bytes = CompressionUtils.Decompress(data, array);
							Directory.CreateDirectory(Path.GetDirectoryName(path));
							File.WriteAllBytes(path, bytes);
						}
					}
					catch (Exception exception)
					{
						storage.CloseContainer(container);
						SetComplete(op, exception);
						yield break;
					}
				}
			}
		}
		storage.CloseContainer(container);
		SetComplete(op, hresult);
	}
}
