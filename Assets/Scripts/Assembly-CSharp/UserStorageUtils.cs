using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using Platform.IO;
using UWE;

public static class UserStorageUtils
{
	public enum Result
	{
		Success = 0,
		UnknownError = 1,
		OutOfSpace = 2,
		NoAccess = 3,
		NotFound = 4,
		InvalidFormat = 5,
		OutOfSlots = 6,
		NotSignedIn = 7
	}

	public class AsyncOperation : IEnumerator
	{
		public Result result;

		public string errorMessage;

		public bool done;

		public object Current => null;

		public bool MoveNext()
		{
			return !done;
		}

		public void SetComplete(Result _result, string _errorMessage)
		{
			result = _result;
			errorMessage = _errorMessage;
			done = true;
		}

		public void Reset()
		{
		}

		public bool GetSuccessful()
		{
			return result == Result.Success;
		}
	}

	public class UpdateOperation : IEnumerator
	{
		public enum Mode
		{
			Saving = 0,
			Deleting = 1,
			SaveFailed = 2,
			DeleteFailed = 3
		}

		public Mode mode;

		public AsyncOperation deleteOperation;

		public SaveOperation saveOperation;

		public object Current => null;

		public Result result => GetResultOperation().result;

		public string errorMessage => GetResultOperation().errorMessage;

		public AsyncOperation GetResultOperation()
		{
			if (mode == Mode.Saving || mode == Mode.SaveFailed || deleteOperation == null)
			{
				return saveOperation;
			}
			return deleteOperation;
		}

		public bool MoveNext()
		{
			if (mode == Mode.Deleting)
			{
				if (deleteOperation != null)
				{
					bool flag = deleteOperation.MoveNext();
					if (flag)
					{
						return flag;
					}
					if (deleteOperation.GetSuccessful())
					{
						if (saveOperation != null)
						{
							mode = Mode.Saving;
						}
					}
					else
					{
						mode = Mode.DeleteFailed;
					}
				}
				else
				{
					mode = Mode.Saving;
				}
			}
			if (mode == Mode.Saving)
			{
				return saveOperation.MoveNext();
			}
			return false;
		}

		public void Reset()
		{
		}

		public bool GetSuccessful()
		{
			if (saveOperation.GetSuccessful())
			{
				return deleteOperation?.GetSuccessful() ?? true;
			}
			return false;
		}
	}

	public class QueryOperation : AsyncOperation
	{
		public readonly List<string> results = new List<string>();
	}

	public class LoadOperation : AsyncOperation
	{
		public readonly Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();
	}

	public class SlotsOperation : AsyncOperation
	{
		public readonly Dictionary<string, LoadOperation> slots = new Dictionary<string, LoadOperation>();
	}

	public class SaveOperation : AsyncOperation
	{
		public int saveDataSize;
	}

	public class CopyOperation : AsyncOperation
	{
	}

	public class UpgradeOperation : AsyncOperation
	{
		public int itemsTotal;

		public int itemsPrecessed;
	}

	private static readonly string _regexBatchCells = ".*baked-batch-cells-\\d+";

	public const int defaultCompressionBufferSize = 4096;

	private static readonly byte[] compressionHeader = new byte[4] { 67, 77, 80, 0 };

	public static Result GetResultForException(Exception exception)
	{
		if (UWE.Utils.GetDiskFull(exception))
		{
			return Result.OutOfSpace;
		}
		if (exception is UnauthorizedAccessException)
		{
			return Result.NoAccess;
		}
		if (exception is DirectoryNotFoundException)
		{
			return Result.NotFound;
		}
		return Result.UnknownError;
	}

	public static SlotsOperation LoadFilesAsync(UserStorage userStorage, List<string> containerNames, List<string> fileNames)
	{
		SlotsOperation slotsOperation = new SlotsOperation();
		foreach (string containerName in containerNames)
		{
			slotsOperation.slots[containerName] = userStorage.LoadFilesAsync(containerName, fileNames);
		}
		slotsOperation.SetComplete(Result.Success, null);
		return slotsOperation;
	}

	public static byte[] Compress(byte[] input, byte[] compressBuffer = null)
	{
		Deflater deflater = new Deflater();
		deflater.SetInput(input);
		deflater.Finish();
		if (compressBuffer == null)
		{
			compressBuffer = new byte[4096];
		}
		using (ScratchMemoryStream scratchMemoryStream = new ScratchMemoryStream())
		{
			scratchMemoryStream.Write(compressionHeader, 0, compressionHeader.Length);
			while (!deflater.IsFinished)
			{
				int count = deflater.Deflate(compressBuffer);
				scratchMemoryStream.Write(compressBuffer, 0, count);
			}
			return scratchMemoryStream.ToArray();
		}
	}

	public static byte[] CompressIfUncompressed(byte[] input, byte[] compressBuffer = null)
	{
		if (GetIsCompressed(input))
		{
			return input;
		}
		return Compress(input, compressBuffer);
	}

	private static bool GetIsCompressed(byte[] input)
	{
		if (input.Length < compressionHeader.Length)
		{
			return false;
		}
		for (int i = 0; i < compressionHeader.Length; i++)
		{
			if (input[i] != compressionHeader[i])
			{
				return false;
			}
		}
		return true;
	}

	public static byte[] Decompress(byte[] input, int count, byte[] compressBuffer = null)
	{
		if (!GetIsCompressed(input))
		{
			return input;
		}
		if (compressBuffer == null)
		{
			compressBuffer = new byte[4096];
		}
		Inflater inflater = new Inflater();
		inflater.SetInput(input, compressionHeader.Length, count - compressionHeader.Length);
		using (ScratchMemoryStream scratchMemoryStream = new ScratchMemoryStream())
		{
			while (!inflater.IsFinished)
			{
				int count2 = inflater.Inflate(compressBuffer);
				scratchMemoryStream.Write(compressBuffer, 0, count2);
			}
			return scratchMemoryStream.ToArray();
		}
	}

	public static void DecompressToStream(byte[] input, int count, Stream streamDestination, byte[] compressBuffer = null)
	{
		if (!GetIsCompressed(input))
		{
			streamDestination.Write(input, 0, input.Length);
			return;
		}
		if (compressBuffer == null)
		{
			compressBuffer = new byte[4096];
		}
		Inflater inflater = new Inflater();
		inflater.SetInput(input, compressionHeader.Length, count - compressionHeader.Length);
		using (ScratchMemoryStream scratchMemoryStream = new ScratchMemoryStream())
		{
			while (!inflater.IsFinished)
			{
				int count2 = inflater.Inflate(compressBuffer);
				scratchMemoryStream.Write(compressBuffer, 0, count2);
			}
			scratchMemoryStream.WriteTo(streamDestination);
		}
	}

	public static bool TryAddFileToBatchCellsRowList(string filePath, List<string> updatedBatchCellsRowList)
	{
		return TryAddFileToUpdatedRowList(filePath, _regexBatchCells, updatedBatchCellsRowList);
	}

	public static bool TryAddFileToUpdatedRowList(string filePath, string fileRegexPattern, List<string> updatedRowList)
	{
		Match match = Regex.Match(filePath, fileRegexPattern);
		if (match.Success)
		{
			if (!updatedRowList.Contains(match.Value))
			{
				updatedRowList.Add(match.Value);
			}
			return true;
		}
		return false;
	}

	public static void CollectBatchCellFilesByRow(string batchCellRow, List<string> updatedFiles, List<string> unchangedFiles, List<string> result)
	{
		CollectBatchCellFiles($"{batchCellRow}-", updatedFiles, unchangedFiles, result);
	}

	public static void CollectBatchCellFiles(string fileNamePrefix, List<string> updatedFiles, List<string> unchangedFiles, List<string> result)
	{
		if (result == null)
		{
			result = new List<string>();
		}
		foreach (string updatedFile in updatedFiles)
		{
			if (updatedFile.Contains(fileNamePrefix))
			{
				result.Add(updatedFile);
			}
		}
		if (unchangedFiles == null)
		{
			return;
		}
		foreach (string unchangedFile in unchangedFiles)
		{
			if (unchangedFile.Contains(fileNamePrefix))
			{
				result.Add(unchangedFile);
			}
		}
	}

	public static bool TryDecompressFromZip(Stream inputStream, string destinationPath, byte[] buffer)
	{
		if (inputStream == null || inputStream.Length == 0L)
		{
			return false;
		}
		bool result = true;
		using (ZipInputStream zipInputStream = new ZipInputStream(inputStream))
		{
			try
			{
				MemoryStream memoryStream = new MemoryStream();
				ZipEntry nextEntry;
				while ((nextEntry = zipInputStream.GetNextEntry()) != null)
				{
					if (nextEntry.IsFile)
					{
						string name = nextEntry.Name;
						string path = Platform.IO.Path.Combine(destinationPath, name);
						Platform.IO.Directory.CreateDirectory(Platform.IO.Path.GetDirectoryName(path));
						memoryStream.SetLength(0L);
						zipInputStream.CopyTo(memoryStream);
						byte[] array = memoryStream.ToArray();
						using (FileStream streamDestination = Platform.IO.File.Create(path))
						{
							DecompressToStream(array, array.Length, streamDestination, buffer);
						}
					}
				}
			}
			catch (Exception)
			{
				result = false;
			}
		}
		return result;
	}
}
