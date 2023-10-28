using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_InsultingSpree : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!(pawn.MentalState is MentalState_InsultingSpree mentalState_InsultingSpree) || mentalState_InsultingSpree.target == null || !pawn.CanReach(mentalState_InsultingSpree.target, PathEndMode.Touch, Danger.Deadly))
			{
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.Insult, mentalState_InsultingSpree.target);
		}
	}
}
