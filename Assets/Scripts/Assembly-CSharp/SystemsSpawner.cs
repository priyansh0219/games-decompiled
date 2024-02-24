using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemsSpawner : MonoBehaviour
{
	private static Dictionary<string, GameObject> spawnedSingletons = new Dictionary<string, GameObject>();

	public GameObject singletonPrefab;

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
		if (spawnedSingletons.TryGetValue(singletonPrefab.name, out var value))
		{
			Object.DestroyObject(base.gameObject);
			return;
		}
		value = Object.Instantiate(singletonPrefab, Vector3.zero, Quaternion.identity);
		Object.DontDestroyOnLoad(value);
		value.transform.parent = base.transform;
		spawnedSingletons.Add(singletonPrefab.name, value);
		StartCoroutine(SetupSingleton(value));
	}

	private IEnumerator SetupSingleton(GameObject singleton)
	{
		PlatformServices platformServices;
		do
		{
			platformServices = PlatformUtils.main.GetServices();
			yield return null;
		}
		while (platformServices == null);
		SentrySdk component = singleton.GetComponent<SentrySdk>();
		component._ensureServerAccessHandler = platformServices.TryEnsureServerAccessAsync;
		component._isServerAccessibleHandler = platformServices.CanAccessServers;
	}
}
