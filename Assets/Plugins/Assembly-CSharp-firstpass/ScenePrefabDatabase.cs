using System.Collections.Generic;
using UnityEngine;

public static class ScenePrefabDatabase
{
	private static Dictionary<string, GameObject> scenePrefabs = new Dictionary<string, GameObject>();

	public static bool TryGetScenePrefab(string classId, out GameObject prefab)
	{
		if (string.IsNullOrEmpty(classId))
		{
			prefab = null;
			return false;
		}
		return scenePrefabs.TryGetValue(classId, out prefab);
	}

	public static void AddScenePrefab(GameObject prefab)
	{
		PrefabIdentifier component = prefab.GetComponent<PrefabIdentifier>();
		if ((bool)component)
		{
			scenePrefabs[component.ClassId] = prefab;
		}
	}

	public static void LogInfo()
	{
		Debug.Log(scenePrefabs.Count + " scene prefabs in database");
		foreach (KeyValuePair<string, GameObject> scenePrefab in scenePrefabs)
		{
			Debug.Log(scenePrefab.Key + ": " + scenePrefab.Value, scenePrefab.Value);
		}
	}
}
