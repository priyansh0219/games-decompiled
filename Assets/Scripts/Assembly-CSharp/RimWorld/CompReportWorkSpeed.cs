using Verse;

namespace RimWorld
{
	public class CompReportWorkSpeed : ThingComp
	{
		public override string CompInspectStringExtra()
		{
			if (parent.def.statBases == null)
			{
				return null;
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			foreach (StatDef item in DefDatabase<StatDef>.AllDefsListForReading)
			{
				if (item == null || item.parts == null || item.Worker.IsDisabledFor(parent))
				{
					continue;
				}
				foreach (StatPart part in item.parts)
				{
					if (part is StatPart_WorkTableOutdoors || part is StatPart_Outdoors)
					{
						flag = true;
					}
					else if (part is StatPart_WorkTableTemperature)
					{
						flag2 = true;
					}
					else if (part is StatPart_WorkTableUnpowered)
					{
						flag3 = true;
					}
				}
			}
			bool num = flag && StatPart_WorkTableOutdoors.Applies(parent.def, parent.Map, parent.Position);
			bool flag4 = flag2 && StatPart_WorkTableTemperature.Applies(parent);
			bool flag5 = flag3 && StatPart_WorkTableUnpowered.Applies(parent);
			string text = string.Format(arg1: parent.GetStatValue(StatDefOf.WorkTableWorkSpeedFactor).ToStringPercent(), format: "{0}: {1}", arg0: "WorkSpeedPenalty".Translate());
			string text2 = "";
			if (num)
			{
				text2 += "Outdoors".Translate();
			}
			if (flag4)
			{
				string text3 = "BadTemperature".Translate();
				text2 = (text2.NullOrEmpty() ? (text2 + text3) : (text2 + ", " + text3));
			}
			if (flag5)
			{
				string text4 = "NoPower".Translate();
				text2 = (text2.NullOrEmpty() ? (text2 + text4) : (text2 + ", " + text4));
			}
			CompAffectedByFacilities comp = parent.GetComp<CompAffectedByFacilities>();
			if (comp != null)
			{
				foreach (Thing item2 in comp.LinkedFacilitiesListForReading)
				{
					if (item2.def.GetCompProperties<CompProperties_Facility>().statOffsets.GetStatOffsetFromList(StatDefOf.WorkTableWorkSpeedFactor) != 0f)
					{
						string label = item2.def.label;
						text2 = (text2.NullOrEmpty() ? (text2 + label) : (text2 + ", " + label));
					}
				}
			}
			if (!text2.NullOrEmpty())
			{
				text = text + " (" + text2 + ")";
			}
			return text;
		}
	}
}
