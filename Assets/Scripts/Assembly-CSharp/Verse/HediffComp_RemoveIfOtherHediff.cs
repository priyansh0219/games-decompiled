namespace Verse
{
	public class HediffComp_RemoveIfOtherHediff : HediffComp_MessageBase
	{
		private const int MtbRemovalCheckInterval = 1000;

		protected HediffCompProperties_RemoveIfOtherHediff Props => (HediffCompProperties_RemoveIfOtherHediff)props;

		public override void CompPostTick(ref float severityAdjustment)
		{
			base.CompPostTick(ref severityAdjustment);
			if (ShouldRemove())
			{
				Message();
				parent.pawn.health.RemoveHediff(parent);
			}
		}

		private bool ShouldRemove()
		{
			if (base.CompShouldRemove)
			{
				return true;
			}
			Hediff firstHediffOfDef;
			if ((firstHediffOfDef = base.Pawn.health.hediffSet.GetFirstHediffOfDef(Props.otherHediff)) == null)
			{
				return false;
			}
			if (Props.stages.HasValue && !Props.stages.Value.Includes(firstHediffOfDef.CurStageIndex))
			{
				return false;
			}
			if (Props.mtbHours > 0)
			{
				if (!base.Pawn.IsHashIntervalTick(1000))
				{
					return false;
				}
				if (!Rand.MTBEventOccurs(Props.mtbHours, 2500f, 1000f))
				{
					return false;
				}
			}
			return true;
		}
	}
}
