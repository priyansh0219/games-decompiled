using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_Berserk : ThinkNode_JobGiver
	{
		private const float WaitChance = 0.5f;

		private const int WaitTicks = 90;

		private const int MinMeleeChaseTicks = 420;

		private const int MaxMeleeChaseTicks = 900;

		private float maxAttackDistance = 40f;

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (Rand.Value < 0.5f)
			{
				Job job = JobMaker.MakeJob(JobDefOf.Wait_Combat);
				job.expiryInterval = 90;
				job.canUseRangedWeapon = false;
				return job;
			}
			if (pawn.TryGetAttackVerb(null) == null)
			{
				return null;
			}
			Pawn pawn2 = FindPawnTarget(pawn);
			if (pawn2 != null)
			{
				Job job2 = JobMaker.MakeJob(JobDefOf.AttackMelee, pawn2);
				job2.maxNumMeleeAttacks = 1;
				job2.expiryInterval = Rand.Range(420, 900);
				job2.canBashDoors = true;
				return job2;
			}
			return null;
		}

		private Pawn FindPawnTarget(Pawn pawn)
		{
			return (Pawn)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedReachable, (Thing x) => x is Pawn pawn2 && pawn2.Spawned && !pawn2.Downed && !pawn2.IsInvisible(), 0f, maxAttackDistance, default(IntVec3), float.MaxValue, canBashDoors: true);
		}

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_Berserk obj = (JobGiver_Berserk)base.DeepCopy(resolve);
			obj.maxAttackDistance = maxAttackDistance;
			return obj;
		}
	}
}
