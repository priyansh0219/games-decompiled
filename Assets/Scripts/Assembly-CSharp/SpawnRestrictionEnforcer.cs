using System;
using System.Collections.Generic;
using Platform.IO;
using UWE;
using UnityEngine;

public static class SpawnRestrictionEnforcer
{
	private class SpawnRestriction
	{
		public string prefab;

		public string classId;

		public float spawnRate;
	}

	private static Dictionary<string, SpawnRestriction> restrictionDictionary;

	private static Dictionary<string, float> restrictionTrackers;

	private static void SetupIfNecessary()
	{
		if (restrictionDictionary == null)
		{
			restrictionDictionary = new Dictionary<string, SpawnRestriction>();
			restrictionTrackers = new Dictionary<string, float>();
			if (!CommandLine.GetFlag("-nospawnrestrictions"))
			{
				ReadRestrictionsFromCSV();
				ProcessRestrictionsForRuntime();
				SetupRestrictionTrackers();
			}
		}
	}

	private static void ReadRestrictionsFromCSV()
	{
		string text = SpawnRestrictionDataPath();
		if (!FileUtils.FileExists(text))
		{
			return;
		}
		try
		{
			foreach (SpawnRestriction item in CSVUtils.Load<SpawnRestriction>(text))
			{
				if (!restrictionDictionary.ContainsKey(item.classId))
				{
					restrictionDictionary.Add(item.classId, item);
				}
			}
		}
		catch (Exception)
		{
			Debug.LogErrorFormat("Failed to process spawn restriction file {0}", text);
		}
	}

	private static string SpawnRestrictionDataPath()
	{
		string text = QualitySettings.names[QualitySettings.GetQualityLevel()];
		string arg = string.Join("_", text.ToLower().Split(Path.GetInvalidFileNameChars()));
		return SNUtils.InsideUnmanaged($"spawnrestrictions-{arg}.csv");
	}

	private static void ProcessRestrictionsForRuntime()
	{
		foreach (string item in new List<string>(restrictionDictionary.Keys))
		{
			if (Mathf.Approximately(restrictionDictionary[item].spawnRate, 1f))
			{
				restrictionDictionary.Remove(item);
			}
			else
			{
				restrictionDictionary[item].spawnRate = 1f / (1f - restrictionDictionary[item].spawnRate);
			}
		}
	}

	private static void SetupRestrictionTrackers()
	{
		restrictionTrackers = new Dictionary<string, float>();
		foreach (KeyValuePair<string, SpawnRestriction> item in restrictionDictionary)
		{
			restrictionTrackers[item.Key] = 0f;
		}
	}

	public static bool ShouldSpawn(string classId)
	{
		SetupIfNecessary();
		if (restrictionDictionary.Count == 0)
		{
			return true;
		}
		if (classId == null)
		{
			return true;
		}
		bool result = true;
		float value = 0f;
		if (restrictionTrackers.TryGetValue(classId, out value))
		{
			value += 1f;
			if (value >= restrictionDictionary[classId].spawnRate)
			{
				result = false;
				value -= restrictionDictionary[classId].spawnRate;
			}
			restrictionTrackers[classId] = value;
		}
		return result;
	}

	public static bool ShouldSpawn(GameObject prefab)
	{
		SetupIfNecessary();
		if (restrictionDictionary.Count == 0)
		{
			return true;
		}
		if (prefab == null)
		{
			return true;
		}
		PrefabIdentifier component = null;
		if (prefab.TryGetComponent<PrefabIdentifier>(out component))
		{
			return ShouldSpawn(component.ClassId);
		}
		return true;
	}
}
