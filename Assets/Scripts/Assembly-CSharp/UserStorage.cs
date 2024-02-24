using System.Collections.Generic;

public interface UserStorage
{
	UserStorageUtils.AsyncOperation InitializeAsync();

	UserStorageUtils.QueryOperation GetContainerNamesAsync();

	UserStorageUtils.AsyncOperation CreateContainerAsync(string containerName);

	UserStorageUtils.AsyncOperation DeleteContainerAsync(string containerName);

	UserStorageUtils.LoadOperation LoadFilesAsync(string containerName, List<string> fileNames);

	UserStorageUtils.SlotsOperation LoadSlotsAsync(List<string> containerNames, List<string> fileNames);

	UserStorageUtils.SaveOperation SaveFilesAsync(string containerName, Dictionary<string, byte[]> files);

	UserStorageUtils.AsyncOperation DeleteFilesAsync(string containerName, List<string> filePaths);

	UserStorageUtils.CopyOperation CopyFilesFromContainerAsync(string containerName, string destPath);

	UserStorageUtils.SaveOperation CopyFilesToContainerAsync(string containerName, string srcPath, List<string> updatedFiles, List<string> deletedFiles, List<string> unchangedFiles);
}
