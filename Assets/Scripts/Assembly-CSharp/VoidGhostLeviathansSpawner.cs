using System;
using System.Collections.Generic;
using UnityEngine;

public class VoidGhostLeviathansSpawner : MonoBehaviour
{
	public float timeBeforeFirstSpawn = 3f;

	public float spawnInterval = 20f;

	public int maxSpawns = 3;

	public float spawnDistance = 50f;

	[AssertNotNull]
	public GameObject ghostLeviathanPrefab;

	private HashSet<GameObject> spawnedCreatures = new HashSet<GameObject>();

	private bool playerIsInVoid;

	private float timeNextSpawn = -1f;

	private const string kVoidBiome = "void";

	public static VoidGhostLeviathansSpawner main { get; private set; }

	private void Start()
	{
		main = this;
		InvokeRepeating("UpdateSpawn", 0f, 2f);
	}

	private void UpdateSpawn()
	{
		Player player = Player.main;
		if ((bool)player)
		{
			bool flag = IsVoidBiome(player.GetBiomeString());
			if (playerIsInVoid != flag)
			{
				playerIsInVoid = flag;
				timeNextSpawn = CalculateTimeNextSpawn();
			}
			if (timeNextSpawn > 0f && Time.time > timeNextSpawn && TryGetSpawnPosition(player.transform.position, out var spawnPosition))
			{
				GameObject item = UnityEngine.Object.Instantiate(ghostLeviathanPrefab, spawnPosition, Quaternion.identity);
				spawnedCreatures.Add(item);
				timeNextSpawn = CalculateTimeNextSpawn();
			}
		}
	}

	private bool IsVoidBiome(string biomeName)
	{
		return string.Equals(biomeName, "void", StringComparison.OrdinalIgnoreCase);
	}

	private bool TryGetSpawnPosition(Vector3 playerPosition, out Vector3 spawnPosition)
	{
		spawnPosition = Vector3.zero;
		if (!LargeWorld.main)
		{
			return false;
		}
		for (int i = 0; i < 10; i++)
		{
			spawnPosition = playerPosition + UnityEngine.Random.onUnitSphere * spawnDistance;
			string biome = LargeWorld.main.GetBiome(spawnPosition);
			if (spawnPosition.y < -100f && IsVoidBiome(biome))
			{
				return true;
			}
		}
		return false;
	}

	private float CalculateTimeNextSpawn()
	{
		if (playerIsInVoid && spawnedCreatures.Count < maxSpawns)
		{
			if (spawnedCreatures.Count == 0)
			{
				return Time.time + timeBeforeFirstSpawn;
			}
			return Time.time + spawnInterval;
		}
		return -1f;
	}

	public bool IsPlayerInVoid()
	{
		return playerIsInVoid;
	}

	public void OnGhostLeviathanDestroyed(GameObject ghostLeviathanGO)
	{
		if (spawnedCreatures.Remove(ghostLeviathanGO) && playerIsInVoid)
		{
			float num = CalculateTimeNextSpawn();
			timeNextSpawn = ((timeNextSpawn > 0f) ? Mathf.Min(timeNextSpawn, num) : num);
		}
	}
}
