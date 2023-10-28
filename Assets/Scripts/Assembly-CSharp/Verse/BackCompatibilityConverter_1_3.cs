using System;
using System.Xml;
using RimWorld;

namespace Verse
{
	public class BackCompatibilityConverter_1_3 : BackCompatibilityConverter
	{
		public override bool AppliesToVersion(int majorVer, int minorVer)
		{
			switch (majorVer)
			{
			case 1:
				return minorVer <= 3;
			default:
				return false;
			case 0:
				return true;
			}
		}

		public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
		{
			if (defType == typeof(JobDef) && defName == "TriggerFirefoamPopper")
			{
				return "TriggerObject";
			}
			if (defType == typeof(TerrainDef) && defName == "CarpetDark")
			{
				return "CarpetGreyDark";
			}
			return null;
		}

		public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
		{
			return null;
		}

		public override void PostExposeData(object obj)
		{
			if (obj is Pawn pawn && pawn.RaceProps.Humanlike && pawn.genes == null)
			{
				pawn.genes = new Pawn_GeneTracker(pawn);
			}
		}
	}
}
