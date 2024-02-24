using UnityEngine;

public interface IAssetBundleWrapper
{
	string name { get; }

	void Unload(bool unloadAllLoadedObjects);

	bool Contains(string name);

	IAssetBundleWrapperRequest LoadAssetAsync<T>(string name) where T : Object;
}
