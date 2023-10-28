using RimWorld;

namespace Verse
{
	public class HediffComp_DisappearsPausable : HediffComp_Disappears
	{
		private const int PauseCheckInterval = 120;

		protected virtual bool Paused => false;

		public override string CompLabelInBracketsExtra
		{
			get
			{
				if (!base.Props.showRemainingTime || Paused)
				{
					return null;
				}
				return ticksToDisappear.ToStringTicksToPeriod(allowSeconds: true, shortForm: true, canUseDecimals: true, allowYears: true, canUseDecimalsShortForm: true);
			}
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			if (base.Pawn.IsHashIntervalTick(120) && !Paused)
			{
				ticksToDisappear -= 120;
			}
		}
	}
}
