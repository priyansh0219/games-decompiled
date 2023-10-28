using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Alert_PsychicBondedSeparated : Alert
	{
		private List<Pawn> targets = new List<Pawn>();

		public Alert_PsychicBondedSeparated()
		{
			defaultLabel = "AlertPsychicBondedPawnsSeparated".Translate();
		}

		public override AlertReport GetReport()
		{
			if (!ModsConfig.BiotechActive)
			{
				return false;
			}
			GetTargets();
			return AlertReport.CulpritsAre(targets);
		}

		public override TaggedString GetExplanation()
		{
			return "AlertPsychicBondedPawnsSeparatedDesc".Translate() + ":\n" + targets.Select((Pawn x) => x.NameShortColored.Resolve()).ToLineList("  - ", capitalizeItems: true);
		}

		private void GetTargets()
		{
			targets.Clear();
			foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
			{
				if (item.RaceProps.Humanlike && item.Faction == Faction.OfPlayer)
				{
					Hediff firstHediffOfDef = item.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicBond);
					if (firstHediffOfDef != null && firstHediffOfDef is Hediff_PsychicBond bondHediff && !ThoughtWorker_PsychicBondProximity.NearPsychicBondedPerson(item, bondHediff))
					{
						targets.Add(item);
					}
				}
			}
		}
	}
}
