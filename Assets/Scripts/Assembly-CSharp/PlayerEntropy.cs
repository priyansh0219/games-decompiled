using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEntropy : MonoBehaviour
{
	[Serializable]
	public class TechEntropy
	{
		public TechType techType;

		public FairRandomizer entropy;
	}

	public List<TechEntropy> randomizers = new List<TechEntropy>();

	public bool CheckChance(TechType techType, float percentChance)
	{
		bool result = false;
		for (int i = 0; i < randomizers.Count; i++)
		{
			if (randomizers[i].techType == techType)
			{
				result = randomizers[i].entropy.CheckChance(percentChance);
			}
		}
		return result;
	}
}
