using System.Collections;
using UnityEngine;

public class SkippableActualAssetBundleRequest<T> : IAssetBundleWrapperRequest, IAsyncRequest, IEnumerator, ISkippableRequest where T : Object
{
	private readonly AssetBundle assetBundle;

	private readonly string assetName;

	private IAssetBundleWrapperRequest request;

	public object Current
	{
		get
		{
			LazyInitializeAsyncRequest();
			return request.Current;
		}
	}

	public bool isDone
	{
		get
		{
			LazyInitializeAsyncRequest();
			return request.isDone;
		}
	}

	public float progress
	{
		get
		{
			LazyInitializeAsyncRequest();
			return request.progress;
		}
	}

	public Object asset => request.asset;

	public SkippableActualAssetBundleRequest(AssetBundle assetBundle, string assetName)
	{
		this.assetBundle = assetBundle;
		this.assetName = assetName;
	}

	public bool MoveNext()
	{
		LazyInitializeAsyncRequest();
		return request.MoveNext();
	}

	public void Reset()
	{
	}

	public void LazyInitializeSyncRequest()
	{
		if (request == null)
		{
			request = ActualAssetBundle.LoadAsset<T>(assetBundle, assetName);
		}
	}

	public void LazyInitializeAsyncRequest()
	{
		if (request == null)
		{
			request = ActualAssetBundle.LoadAssetAsync<T>(assetBundle, assetName);
		}
	}

	public override string ToString()
	{
		return $"Load asset '{assetName}' from bundle '{assetBundle.name}'";
	}
}
