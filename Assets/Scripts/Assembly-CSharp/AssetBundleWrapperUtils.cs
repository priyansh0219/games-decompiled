public static class AssetBundleWrapperUtils
{
	public static IAssetBundleWrapperCreateRequest LoadAsync(string path)
	{
		return new SkippableActualAssetBundleCreateRequest(path);
	}
}
