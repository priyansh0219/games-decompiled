using System.Collections;
using UnityEngine;

public interface IAssetBundleWrapperRequest : IAsyncRequest, IEnumerator
{
	Object asset { get; }
}
