using System;
using UnityEngine;

public class MagmaPillarSpawn : AddressablesPrefabSpawn
{
	public bool initializedRandomTime;

	public override bool GetTimeToSpawn()
	{
		if (!initializedRandomTime)
		{
			return true;
		}
		return base.GetTimeToSpawn();
	}

	protected override void SpawnObj(Action<GameObject> spawnCallback = null)
	{
		base.SpawnObj(delegate(GameObject instance)
		{
			if (!initializedRandomTime)
			{
				spawnedObj.GetComponent<MagmaPillar>().SetRandomTimeLived();
				initializedRandomTime = true;
			}
			if (spawnCallback != null)
			{
				spawnCallback(instance);
			}
		});
	}
}
