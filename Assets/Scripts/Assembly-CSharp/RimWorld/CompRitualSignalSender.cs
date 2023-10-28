using Verse;

namespace RimWorld
{
	public class CompRitualSignalSender : ThingComp
	{
		public bool ritualTarget;

		private const int CheckInterval = 30;

		public const string RitualTargetChangedSignal = "RitualTargetChanged";

		public override void CompTick()
		{
			if (parent.IsHashIntervalTick(30))
			{
				bool num = ritualTarget;
				ritualTarget = parent.IsRitualTarget();
				if (num != ritualTarget)
				{
					parent.BroadcastCompSignal("RitualTargetChanged");
				}
			}
		}

		public override void PostExposeData()
		{
			Scribe_Values.Look(ref ritualTarget, "ritualTarget", defaultValue: false);
		}
	}
}
