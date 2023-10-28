using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_HiveDefense : JobGiver_AIFightEnemies
	{
		protected override IntVec3 GetFlagPosition(Pawn pawn)
		{
			if (pawn.mindState.duty.focus.Thing is Hive hive && hive.Spawned)
			{
				return hive.Position;
			}
			return pawn.Position;
		}

		protected override float GetFlagRadius(Pawn pawn)
		{
			return pawn.mindState.duty.radius;
		}

		protected override Job MeleeAttackJob(Thing enemyTarget)
		{
			Job job = base.MeleeAttackJob(enemyTarget);
			job.attackDoorIfTargetLost = true;
			return job;
		}
	}
}
