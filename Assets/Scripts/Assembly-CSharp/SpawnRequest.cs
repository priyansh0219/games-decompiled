using System;
using UnityEngine;

public abstract class SpawnRequest
{
	public GameObject Result { get; protected set; }

	public abstract void RegisterCallback(Action<SpawnRequest> spawnCallback);

	protected abstract void Release();

	public GameObject GetResultAndRelease()
	{
		GameObject result = Result;
		Result = null;
		Release();
		return result;
	}
}
