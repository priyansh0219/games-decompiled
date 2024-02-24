using System;

namespace Story
{
	[Serializable]
	public class UnlockItemData
	{
		public TechType techType;

		public int count;

		public void Trigger()
		{
			CraftData.AddToInventory(techType, count);
		}

		public override string ToString()
		{
			return $"{count} {techType}";
		}
	}
}
