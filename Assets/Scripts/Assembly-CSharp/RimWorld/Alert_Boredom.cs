using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_Boredom : Alert
	{
		private const float JoyNeedThreshold = 0.24000001f;

		private List<Pawn> boredPawnsResult = new List<Pawn>();

		private List<Pawn> BoredPawns
		{
			get
			{
				boredPawnsResult.Clear();
				foreach (Pawn item in PawnsFinder.AllMaps_FreeColonistsSpawned)
				{
					if (item.needs.joy != null && (item.needs.joy.CurLevelPercentage < 0.24000001f || item.GetTimeAssignment() == TimeAssignmentDefOf.Joy) && item.needs.joy.tolerances.BoredOfAllAvailableJoyKinds(item))
					{
						boredPawnsResult.Add(item);
					}
				}
				return boredPawnsResult;
			}
		}

		public Alert_Boredom()
		{
			defaultLabel = "Boredom".Translate();
			defaultPriority = AlertPriority.Medium;
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(BoredPawns);
		}

		public override TaggedString GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			Pawn pawn = null;
			foreach (Pawn boredPawn in BoredPawns)
			{
				stringBuilder.AppendLine("   " + boredPawn.Label);
				if (pawn == null)
				{
					pawn = boredPawn;
				}
			}
			string text = ((pawn != null) ? JoyUtility.JoyKindsOnMapString(pawn.Map) : "");
			return "BoredomDesc".Translate(stringBuilder.ToString().TrimEndNewlines(), pawn.LabelShort, text, pawn.Named("PAWN"));
		}
	}
}
