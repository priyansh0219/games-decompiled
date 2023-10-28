using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Alert_NeedResearchBench : Alert
	{
		private bool HasRequiredResearchBench
		{
			get
			{
				ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					if (currentProj.requiredResearchBuilding != null)
					{
						if (maps[i].listerBuildings.AllBuildingsColonistOfDef(currentProj.requiredResearchBuilding).Any())
						{
							return true;
						}
					}
					else if (maps[i].listerBuildings.AllBuildingsColonistOfClass<Building_ResearchBench>().Any())
					{
						return true;
					}
				}
				return false;
			}
		}

		public Alert_NeedResearchBench()
		{
			defaultLabel = "NeedResearchBench".Translate();
		}

		public override AlertReport GetReport()
		{
			return Find.ResearchManager.currentProj != null && !HasRequiredResearchBench;
		}

		protected override void OnClick()
		{
			Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research);
		}

		public override TaggedString GetExplanation()
		{
			return "NeedResearchBenchDesc".Translate(Find.ResearchManager.currentProj.label, Find.ResearchManager.currentProj.requiredResearchBuilding ?? ThingDefOf.SimpleResearchBench) + ("\n\n(" + "ClickToOpenResearchTab".Translate() + ")");
		}
	}
}
