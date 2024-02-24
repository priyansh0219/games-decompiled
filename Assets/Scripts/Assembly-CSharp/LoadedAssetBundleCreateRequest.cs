using System.Collections;

public class LoadedAssetBundleCreateRequest : IAssetBundleWrapperCreateRequest, IAsyncRequest, IEnumerator
{
	private readonly IAssetBundleWrapper bundle;

	public object Current => null;

	public IAssetBundleWrapper assetBundle => bundle;

	public bool isDone => true;

	public float progress => 1f;

	public LoadedAssetBundleCreateRequest(IAssetBundleWrapper bundle)
	{
		this.bundle = bundle;
	}

	public bool MoveNext()
	{
		return false;
	}

	public void Reset()
	{
	}
}
