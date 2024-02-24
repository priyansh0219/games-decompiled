using System.Collections;
using UnityEngine;

public class LoadedAssetRequest : IAssetBundleWrapperRequest, IAsyncRequest, IEnumerator
{
	private readonly Object loadedAsset;

	public object Current => null;

	public bool isDone => true;

	public float progress => 1f;

	public Object asset => loadedAsset;

	public LoadedAssetRequest(Object loadedAsset)
	{
		this.loadedAsset = loadedAsset;
	}

	public bool MoveNext()
	{
		return false;
	}

	public void Reset()
	{
	}
}
