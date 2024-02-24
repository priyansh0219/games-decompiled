using UnityEngine;

public class SubSpawn : MonoBehaviour
{
	public string subSceneName;

	private void Awake()
	{
	}

	private void OnEntityAwake()
	{
		if (LightmappedPrefabs.main != null)
		{
			LightmappedPrefabs.main.RequestScenePrefab(subSceneName, OnPrefabReady);
		}
		else
		{
			Debug.LogError("No LightmappedPrefabs instance in scene! Could not spawn submarine " + subSceneName);
		}
	}

	private void OnPrefabReady(GameObject prefab)
	{
		if (this != null && base.gameObject.activeInHierarchy)
		{
			Debug.Log("spawning submarine at " + base.transform.position);
			GameObject obj = Utils.SpawnPrefabAt(prefab, null, base.transform.position);
			obj.name = obj.name + " (from " + base.gameObject.GetFullHierarchyPath() + ")";
			obj.SetActive(value: true);
			Object.Destroy(base.gameObject);
		}
	}
}
