using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UWE
{
	public class LoadingPrefabRequest : IPrefabRequest, IEnumerator
	{
		private readonly string filename;

		private AsyncOperationHandle<GameObject> request;

		private bool hasRequest;

		public object Current
		{
			get
			{
				LazyInitializeAsyncRequest();
				return request;
			}
		}

		public LoadingPrefabRequest(string filename, AsyncOperationHandle<GameObject> request)
		{
			this.filename = filename;
			this.request = request;
			hasRequest = request.IsValid();
		}

		public LoadingPrefabRequest(string filename)
		{
			this.filename = filename;
		}

		public bool TryGetPrefab(out GameObject result)
		{
			result = request.Result;
			return result != null;
		}

		public void Release()
		{
			if (request.IsValid())
			{
				Addressables.Release(request);
			}
		}

		public void LazyInitializeAsyncRequest()
		{
			if (!hasRequest)
			{
				request = AddressablesUtility.LoadAsync<GameObject>(filename);
				hasRequest = true;
			}
		}

		public bool MoveNext()
		{
			LazyInitializeAsyncRequest();
			return !request.IsDone;
		}

		public void Reset()
		{
		}
	}
}
