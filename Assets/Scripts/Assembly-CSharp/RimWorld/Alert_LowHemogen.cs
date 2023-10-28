using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class Alert_LowHemogen : Alert
	{
		private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

		private List<string> targetLabels = new List<string>();

		private List<GlobalTargetInfo> Targets
		{
			get
			{
				CalculateTargets();
				return targets;
			}
		}

		public override string GetLabel()
		{
			if (Targets.Count == 1)
			{
				return "AlertLowHemogen".Translate() + ": " + targetLabels[0];
			}
			return "AlertLowHemogen".Translate();
		}

		private void CalculateTargets()
		{
			targets.Clear();
			targetLabels.Clear();
			if (!ModsConfig.BiotechActive)
			{
				return;
			}
			foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
			{
				if (item.RaceProps.Humanlike && item.Faction == Faction.OfPlayer)
				{
					Gene_Hemogen gene_Hemogen = item.genes?.GetFirstGeneOfType<Gene_Hemogen>();
					if (gene_Hemogen != null && gene_Hemogen.Value < gene_Hemogen.MinLevelForAlert)
					{
						targets.Add(item);
						targetLabels.Add(item.NameShortColored.Resolve());
					}
				}
			}
		}

		public override TaggedString GetExplanation()
		{
			return "AlertLowHemogenDesc".Translate() + ":\n" + targetLabels.ToLineList("  - ");
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(Targets);
		}
	}
}
