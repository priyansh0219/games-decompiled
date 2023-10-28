using RimWorld;

namespace Verse.AI
{
	public class JobGiver_IdleForever : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			Job job = JobMaker.MakeJob(JobDefOf.Wait_Downed);
			if (pawn.Deathresting)
			{
				job.forceSleep = true;
			}
			return job;
		}
	}
}
