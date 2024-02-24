using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

public class LootDistributionData
{
	public class BiomeData
	{
		public BiomeType biome;

		public int count;

		public float probability;
	}

	public class SrcData
	{
		public string prefabPath;

		public List<BiomeData> distribution;

		public override string ToString()
		{
			return $"('{prefabPath}', {distribution.Count} biomes)";
		}

		public static SrcData Create(string prefabPath, List<BiomeData> distribution)
		{
			return new SrcData
			{
				prefabPath = prefabPath,
				distribution = distribution
			};
		}
	}

	public class PrefabData
	{
		public string classId;

		public int count;

		public float probability;
	}

	public class DstData
	{
		public List<PrefabData> prefabs;
	}

	public const string dataPath = "Balance/EntityDistributions";

	public const string fillerID = "None";

	public const string newID = "New";

	public Dictionary<string, SrcData> srcDistribution { get; private set; }

	public Dictionary<BiomeType, DstData> dstDistribution { get; private set; }

	public static LootDistributionData Load(string path)
	{
		LootDistributionData lootDistributionData = new LootDistributionData();
		TextAsset textAsset = Resources.Load<TextAsset>(path);
		if (textAsset != null)
		{
			string text = textAsset.text;
			if (!string.IsNullOrEmpty(text))
			{
				lootDistributionData.Initialize(ParseJson(text));
			}
			else
			{
				Debug.LogError($"LootDistributionData : Failed to load data! Contents of TextAsset file at path '{path}' is null or empty!");
			}
		}
		else
		{
			Debug.LogError($"LootDistributionData : Failed to load data! TextAsset at path '{path}' is not found!");
		}
		return lootDistributionData;
	}

	public static Dictionary<string, SrcData> ParseJson(string json)
	{
		JsonMapper.RegisterImporter<double, float>(Convert.ToSingle);
		Dictionary<string, SrcData> result = new Dictionary<string, SrcData>();
		try
		{
			result = JsonMapper.ToObject<Dictionary<string, SrcData>>(json);
		}
		catch (Exception ex)
		{
			Debug.LogErrorFormat("LootDistributionData : Initialize() : Failed to parse loot distribution data JSON string! Exception:\n{0}", ex.ToString());
		}
		return result;
	}

	public void Initialize(Dictionary<string, SrcData> src)
	{
		srcDistribution = src;
		dstDistribution = new Dictionary<BiomeType, DstData>();
		Dictionary<string, SrcData>.Enumerator enumerator = srcDistribution.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, SrcData> current = enumerator.Current;
			string key = current.Key;
			List<BiomeData> distribution = current.Value.distribution;
			if (distribution == null)
			{
				continue;
			}
			int i = 0;
			for (int count = distribution.Count; i < count; i++)
			{
				BiomeData biomeData = distribution[i];
				BiomeType biome = biomeData.biome;
				int count2 = biomeData.count;
				float probability = biomeData.probability;
				if (!dstDistribution.TryGetValue(biome, out var value))
				{
					value = new DstData();
					value.prefabs = new List<PrefabData>();
					dstDistribution.Add(biome, value);
				}
				PrefabData prefabData = new PrefabData();
				prefabData.classId = key;
				prefabData.count = count2;
				prefabData.probability = probability;
				value.prefabs.Add(prefabData);
			}
		}
	}

	public bool GetRandomLootForBiome(BiomeType biome, out PrefabData prefabData)
	{
		if (GetBiomeLoot(biome, out var data))
		{
			int randomIndex = GetRandomIndex(data.prefabs);
			if (randomIndex != -1)
			{
				prefabData = data.prefabs[randomIndex];
				if (!string.Equals(prefabData.classId, "None"))
				{
					return true;
				}
			}
		}
		prefabData = null;
		return false;
	}

	public bool GetBiomeLoot(BiomeType biome, out DstData data)
	{
		return dstDistribution.TryGetValue(biome, out data);
	}

	public bool GetPrefabData(string classId, out SrcData data)
	{
		if (!string.IsNullOrEmpty(classId))
		{
			return srcDistribution.TryGetValue(classId, out data);
		}
		data = null;
		return false;
	}

	public static int GetRandomIndex(List<PrefabData> prefabs)
	{
		int count = prefabs.Count;
		if (count > 0)
		{
			float num = 0f;
			for (int i = 0; i < count; i++)
			{
				float probability = prefabs[i].probability;
				if (probability > 0f)
				{
					num += probability;
				}
			}
			float num2 = Mathf.Max(num, 1f);
			float num3 = UnityEngine.Random.value * num2;
			float num4 = 0f;
			for (int j = 0; j < count; j++)
			{
				num4 += prefabs[j].probability;
				if (num3 < num4)
				{
					return j;
				}
			}
		}
		return -1;
	}
}
