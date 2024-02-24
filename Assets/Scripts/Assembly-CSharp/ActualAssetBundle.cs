using UnityEngine;

public class ActualAssetBundle : IAssetBundleWrapper
{
	private AssetBundle assetBundle;

	public string name => assetBundle.name;

	public static IAssetBundleWrapperCreateRequest Load(string bundleName)
	{
		return new LoadedAssetBundleCreateRequest(new ActualAssetBundle(AssetBundle.LoadFromFile(AssetBundleUtils.GetLoadPath(bundleName))));
	}

	public static IAssetBundleWrapperCreateRequest LoadAsync(string bundleName)
	{
		return new ActualAssetBundleCreateRequest(AssetBundle.LoadFromFileAsync(AssetBundleUtils.GetLoadPath(bundleName)));
	}

	public static IAssetBundleWrapperRequest LoadAsset<T>(AssetBundle assetBundle, string assetName) where T : Object
	{
		return new LoadedAssetRequest(assetBundle.LoadAsset<T>(assetName));
	}

	public static IAssetBundleWrapperRequest LoadAssetAsync<T>(AssetBundle assetBundle, string assetName) where T : Object
	{
		return new ActualAssetBundleRequest(assetBundle.LoadAssetAsync<T>(assetName));
	}

	public ActualAssetBundle(AssetBundle _assetBundle)
	{
		assetBundle = _assetBundle;
	}

	public void Unload(bool unloadAllLoadedObjects)
	{
		if ((bool)assetBundle)
		{
			assetBundle.Unload(unloadAllLoadedObjects);
		}
	}

	public bool Contains(string name)
	{
		return assetBundle.Contains(name);
	}

	public IAssetBundleWrapperRequest LoadAssetAsync<T>(string name) where T : Object
	{
		return new SkippableActualAssetBundleRequest<T>(assetBundle, name);
	}
}
