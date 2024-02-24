using System.Collections;
using UnityEngine;

public class ActualAssetBundleRequest : IAssetBundleWrapperRequest, IAsyncRequest, IEnumerator
{
	private AssetBundleRequest request;

	public object Current => request;

	public bool isDone => request.isDone;

	public float progress => request.progress;

	public Object asset => request.asset;

	public ActualAssetBundleRequest(AssetBundleRequest _request)
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
