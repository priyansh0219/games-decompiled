using System;
using System.Collections.Generic;
using UnityEngine;

public class WarperData : ScriptableObject
{
	[Serializable]
	public class WarpInCreature
	{
		public TechType techType;

		public int minNum;

		public int maxNum;
	}

	[Serializable]
	public class WarpInData
	{
		public string biomeName;

		public List<WarpInCreature> creatures = new List<WarpInCreature>();
	}

	public List<WarpInData> warpInCreaturesData = new List<WarpInData>();

	private Dictionary<string, int> biomeLookup;

	private void EnsureBiomeLookup()
	{
		if (biomeLookup != null)
		{
			return;
		}
		biomeLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		for (int i = 0; i < warpInCreaturesData.Count; i++)
		{
			string biomeName = warpInCreaturesData[i].biomeName;
			if (biomeLookup.ContainsKey(biomeName))
			{
				Debug.LogWarningFormat("WarperData warpInCreaturesData contains multiple instances of biome {0}", warpInCreaturesData[i].biomeName);
			}
			else
			{
				biomeLookup.Add(biomeName, i);
			}
		}
	}

	private int GetBiomeIndex(string biomeName)
	{
		if (!string.IsNullOrEmpty(biomeName))
		{
			EnsureBiomeLookup();
			if (biomeLookup.TryGetValue(biomeName, out var value))
			{
				return value;
			}
		}
		return -1;
	}

	public WarpInCreature GetRandomCreature(string biomeName)
	{
		int biomeIndex = GetBiomeIndex(biomeName);
		if (biomeIndex == -1)
		{
			return null;
		}
		List<WarpInCreature> creatures = warpInCreaturesData[biomeIndex].creatures;
		if (creatures == null || creatures.Count == 0)
		{
			return null;
		}
		return creatures.GetRandom();
	}
}
