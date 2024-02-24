using UnityEngine;

public class PrefabSpawn : PrefabSpawnBase, ICompileTimeCheckable
{
	public GameObject prefab;

	public override bool HasValidPrefab()
	{
		return prefab != null;
	}

	internal override SpawnRequest SpawnObjInternal(Transform objParent)
	{
		return CompletedSpawnRequest.Get(Object.Instantiate(prefab, objParent));
	}

	public string CompileTimeCheck()
	{
		return null;
	}
}
