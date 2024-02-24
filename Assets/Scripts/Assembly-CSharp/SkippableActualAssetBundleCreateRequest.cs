using System.Collections;

public class SkippableActualAssetBundleCreateRequest : IAssetBundleWrapperCreateRequest, IAsyncRequest, IEnumerator, ISkippableRequest
{
	private readonly string path;

	private IAssetBundleWrapperCreateRequest request;

	public object Current
	{
		get
		{
			LazyInitializeAsyncRequest();
			return request.Current;
		}
	}

	public IAssetBundleWrapper assetBundle
	{
		get
		{
			LazyInitializeAsyncRequest();
			return request.assetBundle;
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

	public SkippableActualAssetBundleCreateRequest(string path)
	{
		this.path = path;
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
			request = ActualAssetBundle.Load(path);
		}
	}

	public void LazyInitializeAsyncRequest()
	{
		if (request == null)
		{
			request = ActualAssetBundle.LoadAsync(path);
		}
	}

	public override string ToString()
	{
		return $"Load asset bundle '{path}'";
	}
}
