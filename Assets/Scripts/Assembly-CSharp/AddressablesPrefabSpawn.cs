using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablesPrefabSpawn : PrefabSpawnBase
{
	public AssetReferenceGameObject prefab;

	private AsyncOperationHandle<GameObject> handle;

	public override bool HasValidPrefab()
	{
		if (prefab != null)
		{
			return prefab.RuntimeKeyIsValid();
		}
		return false;
	}

	internal override SpawnRequest SpawnObjInternal(Transform objParent)
	{
		handle = prefab.InstantiateAsync(objParent);
		return AddressablesPrefabSpawnRequest.Get(handle);
	}

	private void OnDestroy()
	{
		AddressablesUtility.QueueRelease(ref handle);
	}
}
