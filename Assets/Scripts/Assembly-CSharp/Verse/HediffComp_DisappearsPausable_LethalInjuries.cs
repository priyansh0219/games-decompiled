using RimWorld;

namespace Verse
{
	public class HediffComp_DisappearsPausable_LethalInjuries : HediffComp_DisappearsPausable
	{
		protected override bool Paused => SanguophageUtility.ShouldBeDeathrestingOrInComaInsteadOfDead(base.Pawn);

		public override string CompTipStringExtra
		{
			get
			{
				if (Paused)
				{
					return "PawnWillKeepRegeneratingLethalInjuries".Translate(base.Pawn.Named("PAWN")).Colorize(ColorLibrary.RedReadable);
				}
				return base.CompTipStringExtra;
			}
		}
	}
}
