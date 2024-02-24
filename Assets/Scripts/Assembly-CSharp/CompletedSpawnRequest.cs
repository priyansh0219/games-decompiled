using System;
using UWE;
using UnityEngine;

public class CompletedSpawnRequest : SpawnRequest
{
	private static readonly ObjectPool<CompletedSpawnRequest> pool = ObjectPoolHelper.CreatePool<CompletedSpawnRequest>("CompletedSpawnRequest", 16);

	public static CompletedSpawnRequest Get(GameObject instance)
	{
		CompletedSpawnRequest completedSpawnRequest = pool.Get();
		completedSpawnRequest.Result = instance;
		return completedSpawnRequest;
	}

	public override void RegisterCallback(Action<SpawnRequest> spawnCallback)
	{
		spawnCallback(this);
	}

	protected override void Release()
	{
		pool.Return(this);
	}
}
