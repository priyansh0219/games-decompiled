using System;

namespace Story
{
	[Serializable]
	public class UnlockBlueprintData
	{
		public enum UnlockType
		{
			Locked = 0,
			Available = 1
		}

		public TechType techType;

		public UnlockType unlockType;

		public void Trigger()
		{
			switch (unlockType)
			{
			case UnlockType.Locked:
				PDAScanner.AddByUnlockable(techType);
				break;
			case UnlockType.Available:
				KnownTech.Add(techType);
				break;
			}
		}

		public override string ToString()
		{
			return $"{techType} {unlockType}";
		}
	}
}
