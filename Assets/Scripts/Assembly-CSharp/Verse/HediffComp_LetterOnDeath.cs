using RimWorld;

namespace Verse
{
	public class HediffComp_LetterOnDeath : HediffComp
	{
		public HediffCompProperties_LetterOnDeath Props => (HediffCompProperties_LetterOnDeath)props;

		private bool ShouldSendLetter
		{
			get
			{
				if (Props.onlyIfNoMechanitorDied && Find.History.mechanitorEverDied)
				{
					return false;
				}
				if (parent.pawn != null && PawnGenerator.IsBeingGenerated(parent.pawn))
				{
					return false;
				}
				return true;
			}
		}

		public override void Notify_PawnDied()
		{
			base.Notify_PawnDied();
			if (ShouldSendLetter)
			{
				Find.LetterStack.ReceiveLetter(Props.letterLabel.Formatted(parent.Named("HEDIFF")), Props.letterText.Formatted(parent.pawn.Named("PAWN"), parent.Named("HEDIFF")), Props.letterDef ?? LetterDefOf.NeutralEvent, parent.pawn);
			}
		}
	}
}
