using System;

namespace Verse
{
	public static class MechWeightClassUtility
	{
		public static string ToStringHuman(this MechWeightClass wc)
		{
			switch (wc)
			{
			case MechWeightClass.Light:
				return "MechWeightClass_Light".Translate();
			case MechWeightClass.Medium:
				return "MechWeightClass_Medium".Translate();
			case MechWeightClass.Heavy:
				return "MechWeightClass_Heavy".Translate();
			case MechWeightClass.UltraHeavy:
				return "MechWeightClass_Ultraheavy".Translate();
			default:
				throw new Exception("Unknown mech weight class " + wc);
			}
		}
	}
}
