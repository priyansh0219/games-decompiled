using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_Filter_AnyPawnHasHediff : QuestPart_Filter
	{
		public List<Pawn> pawns;

		public HediffDef hediff;

		protected override bool Pass(SignalArgs args)
		{
			if (pawns.NullOrEmpty())
			{
				return false;
			}
			foreach (Pawn pawn in pawns)
			{
				if (pawn != null && pawn.health.hediffSet.HasHediff(hediff))
				{
					return true;
				}
			}
			return false;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
			Scribe_Defs.Look(ref hediff, "hediff");
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				pawns.RemoveAll((Pawn x) => x == null);
			}
		}
	}
}
