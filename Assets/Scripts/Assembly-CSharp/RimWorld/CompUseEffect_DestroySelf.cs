using Verse;

namespace RimWorld
{
	public class CompUseEffect_DestroySelf : CompUseEffect
	{
		private int delayTicks = -1;

		private CompProperties_UseEffectDestroySelf Props => (CompProperties_UseEffectDestroySelf)props;

		public override float OrderPriority => Props.orderPriority;

		public override void DoEffect(Pawn usedBy)
		{
			base.DoEffect(usedBy);
			if (Props.delayTicks <= 0)
			{
				DoDestroy();
			}
			else
			{
				delayTicks = Props.delayTicks;
			}
		}

		private void DoDestroy()
		{
			if (Props.effecterDef != null)
			{
				Effecter effecter = new Effecter(Props.effecterDef);
				effecter.Trigger(new TargetInfo(parent.Position, parent.Map), TargetInfo.Invalid);
				effecter.Cleanup();
			}
			if (Props.spawnLeavings)
			{
				GenLeaving.DoLeavingsFor(parent, parent.MapHeld, DestroyMode.KillFinalizeLeavingsOnly);
			}
			parent.SplitOff(1).Destroy();
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref delayTicks, "delayTicks", -1);
		}

		public override void CompTick()
		{
			base.CompTick();
			if (delayTicks > 0)
			{
				delayTicks--;
			}
			if (delayTicks == 0)
			{
				DoDestroy();
				delayTicks = -1;
			}
		}
	}
}
