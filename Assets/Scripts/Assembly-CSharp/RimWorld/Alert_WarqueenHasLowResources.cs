using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class Alert_WarqueenHasLowResources : Alert
	{
		private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

		private List<GlobalTargetInfo> Targets
		{
			get
			{
				CalculateTargets();
				return targets;
			}
		}

		private void CalculateTargets()
		{
			targets.Clear();
			if (!ModsConfig.BiotechActive)
			{
				return;
			}
			foreach (Pawn item in PawnsFinder.AllMaps_SpawnedPawnsInFaction(Faction.OfPlayer))
			{
				if (item.IsColonyMech)
				{
					CompMechCarrier comp = item.GetComp<CompMechCarrier>();
					if (comp != null && comp.LowIngredientCount)
					{
						targets.Add(item);
					}
				}
			}
		}

		public override string GetLabel()
		{
			if (defaultLabel.NullOrEmpty())
			{
				defaultLabel = "AlertWarqueenHasLowResources".Translate(PawnKindDefOf.Mech_Warqueen.LabelCap);
			}
			return defaultLabel;
		}

		public override TaggedString GetExplanation()
		{
			return "AlertWarqueenHasLowResourcesDesc".Translate(PawnKindDefOf.Mech_Warqueen.labelPlural);
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(Targets);
		}
	}
}
