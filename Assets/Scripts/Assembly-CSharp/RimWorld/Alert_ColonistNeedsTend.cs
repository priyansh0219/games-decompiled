using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.Steam;

namespace RimWorld
{
	public class Alert_ColonistNeedsTend : Alert
	{
		private List<Pawn> needingColonistsResult = new List<Pawn>();

		private List<Pawn> NeedingColonists
		{
			get
			{
				needingColonistsResult.Clear();
				foreach (Pawn allMaps_FreeColonist in PawnsFinder.AllMaps_FreeColonists)
				{
					if ((allMaps_FreeColonist.Spawned || allMaps_FreeColonist.BrieflyDespawned()) && allMaps_FreeColonist.health.HasHediffsNeedingTendByPlayer(forAlert: true))
					{
						Building_Bed building_Bed = allMaps_FreeColonist.CurrentBed();
						if ((building_Bed == null || !building_Bed.Medical) && !Alert_ColonistNeedsRescuing.NeedsRescue(allMaps_FreeColonist) && !ChildcareUtility.BabyBeingPlayedWith(allMaps_FreeColonist))
						{
							needingColonistsResult.Add(allMaps_FreeColonist);
						}
					}
				}
				return needingColonistsResult;
			}
		}

		public Alert_ColonistNeedsTend()
		{
			defaultLabel = "ColonistNeedsTreatment".Translate();
			defaultPriority = AlertPriority.High;
		}

		public override TaggedString GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn needingColonist in NeedingColonists)
			{
				stringBuilder.AppendLine("  - " + needingColonist.NameShortColored.Resolve());
			}
			if (SteamDeck.IsSteamDeckInNonKeyboardMode)
			{
				return "ColonistNeedsTreatmentDescController".Translate(stringBuilder.ToString());
			}
			return "ColonistNeedsTreatmentDesc".Translate(stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(NeedingColonists);
		}
	}
}
