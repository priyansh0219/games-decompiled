using System;
using UnityEngine;

namespace UWE
{
	[Serializable]
	public class WorldEntityInfo : IEquatable<WorldEntityInfo>
	{
		public string classId;

		public TechType techType;

		public EntitySlot.Type slotType;

		public bool prefabZUp;

		public LargeWorldEntity.CellLevel cellLevel;

		public Vector3 localScale;

		public bool Equals(WorldEntityInfo other)
		{
			if (other == null)
			{
				return false;
			}
			if (string.Equals(classId, other.classId) && techType == other.techType && slotType == other.slotType && prefabZUp == other.prefabZUp && cellLevel == other.cellLevel)
			{
				return localScale == other.localScale;
			}
			return false;
		}
	}
}
