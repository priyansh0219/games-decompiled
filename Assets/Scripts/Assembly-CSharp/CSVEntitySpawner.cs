using System.Collections.Generic;
using UWE;
using UnityEngine;

public class CSVEntitySpawner : MonoBehaviour, LargeWorldEntitySpawner
{
	private struct Data
	{
		public string classId;

		public bool isFragment;

		public int count;

		public float probability;
	}

	private LootDistributionData lootDistribution;

	private static List<Data> sData = new List<Data>();

	public void ResetSpawner()
	{
		lootDistribution = LootDistributionData.Load("Balance/EntityDistributions");
	}

	public EntitySlot.Filler GetPrefabForSlot(IEntitySlot slot, bool filterKnown = true)
	{
		EntitySlot.Filler result = default(EntitySlot.Filler);
		result.classId = null;
		result.count = 0;
		if (lootDistribution.GetBiomeLoot(slot.GetBiomeType(), out var data))
		{
			if (sData.Count > 0)
			{
				sData.Clear();
			}
			float num = 0f;
			float num2 = 0f;
			Dictionary<string, LootDistributionData.SrcData> srcDistribution = lootDistribution.srcDistribution;
			int i = 0;
			for (int count = data.prefabs.Count; i < count; i++)
			{
				LootDistributionData.PrefabData prefabData = data.prefabs[i];
				if (string.Equals(prefabData.classId, "None") || !srcDistribution.TryGetValue(prefabData.classId, out var _))
				{
					continue;
				}
				if (!WorldEntityDatabase.TryGetInfo(prefabData.classId, out var info))
				{
					Debug.LogErrorFormat(this, "Missing world entity info for prefab '{0}'", prefabData.classId);
				}
				else
				{
					if (!slot.IsTypeAllowed(info.slotType))
					{
						continue;
					}
					float num3 = prefabData.probability / slot.GetDensity();
					if (num3 <= 0f)
					{
						continue;
					}
					TechType techType = info.techType;
					bool flag = false;
					if (filterKnown)
					{
						flag = PDAScanner.IsFragment(techType);
						if (flag && PDAScanner.ContainsCompleteEntry(techType))
						{
							num2 += num3;
							continue;
						}
					}
					Data item = default(Data);
					item.classId = prefabData.classId;
					item.count = prefabData.count;
					item.probability = num3;
					item.isFragment = flag;
					sData.Add(item);
					if (flag)
					{
						num += num3;
					}
				}
			}
			bool flag2 = num2 > 0f && num > 0f;
			float num4 = (flag2 ? ((num2 + num) / num) : 1f);
			float num5 = 0f;
			for (int j = 0; j < sData.Count; j++)
			{
				Data value2 = sData[j];
				if (flag2 && value2.isFragment)
				{
					value2.probability *= num4;
					sData[j] = value2;
				}
				num5 += value2.probability;
			}
			Data data2 = default(Data);
			data2.count = 0;
			data2.classId = null;
			if (num5 > 0f)
			{
				float num6 = Random.value;
				if (num5 > 1f)
				{
					num6 *= num5;
				}
				float num7 = 0f;
				for (int k = 0; k < sData.Count; k++)
				{
					Data data3 = sData[k];
					num7 += data3.probability;
					if (num7 >= num6)
					{
						data2 = data3;
						break;
					}
				}
			}
			sData.Clear();
			if (data2.count > 0)
			{
				result.classId = data2.classId;
				result.count = data2.count;
			}
		}
		return result;
	}
}
