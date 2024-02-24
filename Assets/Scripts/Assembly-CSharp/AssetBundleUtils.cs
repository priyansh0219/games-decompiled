using System.IO;
using UnityEngine;

public static class AssetBundleUtils
{
	public static string GetStandAloneLoadPath()
	{
		return Path.Combine(Application.streamingAssetsPath, "AssetBundles");
	}

	public static string GetLoadPath(string assetBundleName)
	{
		return Path.Combine(GetStandAloneLoadPath(), assetBundleName);
	}
}
