using System;
using UWE;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablesPrefabSpawnRequest : SpawnRequest
{
	private static readonly ObjectPool<AddressablesPrefabSpawnRequest> pool = ObjectPoolHelper.CreatePool<AddressablesPrefabSpawnRequest>("AddressablesPrefabSpawnRequest", 16);

	private readonly Action<AsyncOperationHandle<GameObject>> OnHandleCompletedDelegate;

	private AsyncOperationHandle<GameObject> handle;

	private Action<SpawnRequest> spawnCallback;

	public AddressablesPrefabSpawnRequest()
	{
		OnHandleCompletedDelegate = OnHandleCompleted;
	}

	public static AddressablesPrefabSpawnRequest Get(AsyncOperationHandle<GameObject> handle)
	{
		AddressablesPrefabSpawnRequest addressablesPrefabSpawnRequest = pool.Get();
		addressablesPrefabSpawnRequest.handle = handle;
		handle.Completed += addressablesPrefabSpawnRequest.OnHandleCompletedDelegate;
		return addressablesPrefabSpawnRequest;
	}

	public override void RegisterCallback(Action<SpawnRequest> spawnCallback)
	{
		if (handle.IsDone)
		{
			base.Result = handle.Result;
			spawnCallback(this);
		}
		else
		{
			this.spawnCallback = spawnCallback;
		}
	}

	protected override void Release()
	{
		spawnCallback = null;
		handle = default(AsyncOperationHandle<GameObject>);
		pool.Return(this);
	}

	private void OnHandleCompleted(AsyncOperationHandle<GameObject> obj)
	{
		base.Result = obj.Result;
		if (spawnCallback != null)
		{
			spawnCallback(this);
		}
	}
}
