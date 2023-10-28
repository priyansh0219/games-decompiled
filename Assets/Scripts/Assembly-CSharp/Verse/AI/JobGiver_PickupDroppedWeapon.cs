using RimWorld;

namespace Verse.AI
{
	public class JobGiver_PickupDroppedWeapon : ThinkNode_JobGiver
	{
		public bool ignoreForbidden;

		protected override Job TryGiveJob(Pawn pawn)
		{
			Thing thing = pawn.mindState?.droppedWeapon;
			if (thing == null || !thing.Spawned || thing.Map != pawn.Map)
			{
				return null;
			}
			if (!pawn.CanReserveAndReach(thing, PathEndMode.Touch, Danger.Deadly))
			{
				return null;
			}
			Job job = JobMaker.MakeJob(JobDefOf.Equip, pawn.mindState.droppedWeapon);
			job.ignoreForbidden = ignoreForbidden;
			return job;
		}
	}
}
