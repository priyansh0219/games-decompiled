using System;
using UnityEngine;

public class ReefbackSlotsData : ScriptableObject
{
	[Serializable]
	public class ReefbackSlotPlant
	{
		public string tag;

		public GameObject[] prefabVariants;

		public float probability;

		public Vector3 startRotation;
	}

	[Serializable]
	public class ReefbackSlotCreature
	{
		public string tag;

		public GameObject prefab;

		public int minNumber;

		public int maxNumber;

		public float probability;
	}

	public ReefbackSlotPlant[] plants;

	public ReefbackSlotCreature[] creatures;
}
