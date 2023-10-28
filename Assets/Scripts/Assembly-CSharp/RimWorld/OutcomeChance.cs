using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class OutcomeChance
	{
		[MustTranslate]
		public string label;

		[MustTranslate]
		public string description;

		[MustTranslate]
		public string potentialExtraOutcomeDesc;

		public float chance;

		public ThoughtDef memory;

		public int positivityIndex;

		[NoTranslate]
		public List<string> roleIdsNotGainingMemory;

		public float ideoCertaintyOffset;

		public bool Positive => positivityIndex >= 0;

		public bool BestPositiveOutcome(LordJob_Ritual ritual)
		{
			foreach (OutcomeChance outcomeChance in ritual.Ritual.outcomeEffect.def.outcomeChances)
			{
				if (outcomeChance.positivityIndex > positivityIndex)
				{
					return false;
				}
			}
			return true;
		}
	}
}
