using System.Collections.Generic;
using UnityEngine;

public class PrefabListSpawner : MonoBehaviour
{
	[AssertNotNull]
	public List<GameObject> prefabs;

	private void Awake()
	{
		foreach (GameObject prefab in prefabs)
		{
			if (prefab != null)
			{
				Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
			}
		}
	}
}
