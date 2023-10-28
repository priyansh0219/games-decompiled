using RimWorld;

namespace Verse.AI
{
	public class JobDriver_WaitDowned : JobDriver_Wait
	{
		public override string GetReport()
		{
			if (pawn.Deathresting)
			{
				return ReportStringProcessed(SanguophageUtility.DeathrestJobReport(pawn));
			}
			return base.GetReport();
		}

		public override void DecorateWaitToil(Toil wait)
		{
			base.DecorateWaitToil(wait);
			wait.AddFailCondition(() => !pawn.Downed);
		}
	}
}
