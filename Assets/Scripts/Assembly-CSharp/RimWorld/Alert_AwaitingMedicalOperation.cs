using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_AwaitingMedicalOperation : Alert
	{
		private List<Pawn> awaitingMedicalOperationResult = new List<Pawn>();

		private List<Pawn> AwaitingMedicalOperation
		{
			get
			{
				awaitingMedicalOperationResult.Clear();
				List<Pawn> list = PawnsFinder.AllMaps_SpawnedPawnsInFaction(Faction.OfPlayer);
				for (int i = 0; i < list.Count; i++)
				{
					if (IsAwaiting(list[i]))
					{
						awaitingMedicalOperationResult.Add(list[i]);
					}
				}
				List<Pawn> allMaps_PrisonersOfColonySpawned = PawnsFinder.AllMaps_PrisonersOfColonySpawned;
				for (int j = 0; j < allMaps_PrisonersOfColonySpawned.Count; j++)
				{
					if (IsAwaiting(allMaps_PrisonersOfColonySpawned[j]) && (!ModsConfig.BiotechActive || allMaps_PrisonersOfColonySpawned[j].health.surgeryBills.Count != 1 || allMaps_PrisonersOfColonySpawned[j].health.surgeryBills[0].recipe != RecipeDefOf.ExtractHemogenPack || allMaps_PrisonersOfColonySpawned[j].guest.interactionMode != PrisonerInteractionModeDefOf.HemogenFarm))
					{
						awaitingMedicalOperationResult.Add(allMaps_PrisonersOfColonySpawned[j]);
					}
				}
				return awaitingMedicalOperationResult;
				bool IsAwaiting(Pawn p)
				{
					if (HealthAIUtility.ShouldHaveSurgeryDoneNow(p) && p.InBed())
					{
						return !awaitingMedicalOperationResult.Contains(p);
					}
					return false;
				}
			}
		}

		public override string GetLabel()
		{
			return "PatientsAwaitingMedicalOperation".Translate(AwaitingMedicalOperation.Count().ToStringCached());
		}

		public override TaggedString GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn item in AwaitingMedicalOperation)
			{
				stringBuilder.AppendLine("  - " + item.NameShortColored.Resolve());
			}
			return "PatientsAwaitingMedicalOperationDesc".Translate(stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(AwaitingMedicalOperation);
		}
	}
}
