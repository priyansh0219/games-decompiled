using System.Collections;

public interface IAssetBundleWrapperCreateRequest : IAsyncRequest, IEnumerator
{
	IAssetBundleWrapper assetBundle { get; }
}
