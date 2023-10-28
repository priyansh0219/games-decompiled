using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IngestionOutcomeDoer_GiveHediff : IngestionOutcomeDoer
	{
		public HediffDef hediffDef;

		public float severity = -1f;

		public ChemicalDef toleranceChemical;

		private bool divideByBodySize;

		public bool multiplyByGeneToleranceFactors;

		protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
		{
			Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
			float effect = ((!(severity > 0f)) ? hediffDef.initialSeverity : severity);
			if (divideByBodySize)
			{
				effect /= pawn.BodySize;
			}
			AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize_NewTemp(pawn, toleranceChemical, ref effect, multiplyByGeneToleranceFactors);
			hediff.Severity = effect;
			pawn.health.AddHediff(hediff);
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
		{
			if (!parentDef.IsDrug || !(chance >= 1f))
			{
				yield break;
			}
			foreach (StatDrawEntry item in hediffDef.SpecialDisplayStats(StatRequest.ForEmpty()))
			{
				yield return item;
			}
		}
	}
}
