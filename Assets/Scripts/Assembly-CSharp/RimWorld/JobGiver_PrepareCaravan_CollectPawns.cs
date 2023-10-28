using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobGiver_PrepareCaravan_CollectPawns : JobGiver_PrepareCaravan_RopePawns
	{
		protected override JobDef RopeJobDef => JobDefOf.PrepareCaravan_CollectAnimals;

		protected override bool AnimalNeedsGathering(Pawn pawn, Pawn animal)
		{
			return DoesAnimalNeedGathering(pawn, animal);
		}

		public static bool DoesAnimalNeedGathering(Pawn pawn, Pawn animal)
		{
			if (AnimalPenUtility.NeedsToBeManagedByRope(animal) && !GatherAnimalsAndSlavesForCaravanUtility.IsRopedByCaravanPawn(animal) && pawn.GetLord() == animal.GetLord())
			{
				return pawn.CanReserveAndReach(animal, PathEndMode.Touch, Danger.Deadly);
			}
			return false;
		}
	}
}
