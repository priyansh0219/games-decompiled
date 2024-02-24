using System.Collections.Generic;
using UnityEngine;

public class AssetBundleManager : MonoBehaviour
{
	private class LoadedAssetBundle
	{
		public string bundleName;

		public IAssetBundleWrapperCreateRequest createRequest;

		public int numReferences;

		public void Unload(bool unloadAllLoadedObjects)
		{
			if (createRequest != null && createRequest.assetBundle != null)
			{
				createRequest.assetBundle.Unload(unloadAllLoadedObjects);
			}
		}
	}

	private static AssetBundleManager instance;

	private static readonly Dictionary<string, LoadedAssetBundle> loadedBundles = new Dictionary<string, LoadedAssetBundle>();

	private void Awake()
	{
		instance = this;
	}

	public static IAssetBundleWrapperCreateRequest LoadBundleAsync(string bundleName)
	{
		if (!loadedBundles.TryGetValue(bundleName, out var value))
		{
			value = new LoadedAssetBundle();
			value.bundleName = bundleName;
			value.createRequest = AssetBundleWrapperUtils.LoadAsync(bundleName);
			value.numReferences = 0;
			loadedBundles.Add(bundleName, value);
		}
		value.numReferences++;
		return value.createRequest;
	}

	public static void UnloadBundle(string bundleName)
	{
		if (!loadedBundles.TryGetValue(bundleName, out var value) || value.numReferences == 0)
		{
			Debug.LogErrorFormat("Attempting to unload asset bundle {0} which isn't loaded", bundleName);
		}
		else
		{
			value.numReferences--;
		}
	}

	public static void Deinitialize()
	{
		Dictionary<string, LoadedAssetBundle>.Enumerator enumerator = loadedBundles.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Value.Unload(unloadAllLoadedObjects: true);
		}
		loadedBundles.Clear();
	}

	private void Update()
	{
		Dictionary<string, LoadedAssetBundle>.Enumerator enumerator = loadedBundles.GetEnumerator();
		while (enumerator.MoveNext())
		{
			LoadedAssetBundle value = enumerator.Current.Value;
			if (value.numReferences <= 0 && value.createRequest.isDone)
			{
				Debug.LogFormat(this, "Unloading asset bundle '{0}' because it is no longer referenced", value.bundleName);
				loadedBundles.Remove(enumerator.Current.Key);
				value.Unload(unloadAllLoadedObjects: true);
				break;
			}
		}
	}
}
