using System.Collections;
using UnityEngine;

public class ActualAssetBundleCreateRequest : IAssetBundleWrapperCreateRequest, IAsyncRequest, IEnumerator
{
	private AssetBundleCreateRequest request;

	public object Current => request;

	public IAssetBundleWrapper assetBundle => new ActualAssetBundle(request.assetBundle);

	public bool isDone => request.isDone;

	public float progress => request.progress;

	public ActualAssetBundleCreateRequest(AssetBundleCreateRequest _request)
	{
		request = _request;
	}

	public bool MoveNext()
	{
		return !request.isDone;
	}

	public void Reset()
	{
	}
}
