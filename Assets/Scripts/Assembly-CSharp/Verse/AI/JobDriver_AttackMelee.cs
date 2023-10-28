using System.Collections.Generic;
using RimWorld;
using Verse.AI.Group;

namespace Verse.AI
{
	public class JobDriver_AttackMelee : JobDriver
	{
		private int numMeleeAttacksMade;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref numMeleeAttacksMade, "numMeleeAttacksMade", 0);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (job.targetA.Thing is IAttackTarget target)
			{
				pawn.Map.attackTargetReservationManager.Reserve(pawn, job, target);
			}
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_General.DoAtomic(delegate
			{
				if (job.targetA.Thing is Pawn pawn && pawn.Downed && base.pawn.mindState.duty != null && base.pawn.mindState.duty.attackDownedIfStarving && base.pawn.Starving())
				{
					job.killIncappedTarget = true;
				}
			});
			yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
			yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, TargetIndex.B, delegate
			{
				Thing thing = job.GetTarget(TargetIndex.A).Thing;
				if (job.reactingToMeleeThreat && thing is Pawn p && !p.Awake())
				{
					EndJobWith(JobCondition.InterruptForced);
				}
				else if (pawn.meleeVerbs.TryMeleeAttack(thing, job.verbToUse) && pawn.CurJob != null && pawn.jobs.curDriver == this)
				{
					Lord lord = pawn.GetLord();
					if (lord?.LordJob != null && lord.LordJob is LordJob_Ritual_Duel lordJob_Ritual_Duel)
					{
						lordJob_Ritual_Duel.Notify_MeleeAttack(pawn, thing);
					}
					numMeleeAttacksMade++;
					if (numMeleeAttacksMade >= job.maxNumMeleeAttacks)
					{
						EndJobWith(JobCondition.Succeeded);
					}
				}
			}).FailOnDespawnedOrNull(TargetIndex.A);
		}

		public override void Notify_PatherFailed()
		{
			if (job.attackDoorIfTargetLost)
			{
				Thing thing;
				using (PawnPath pawnPath = base.Map.pathFinder.FindPath(pawn.Position, base.TargetA.Cell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors)))
				{
					if (!pawnPath.Found)
					{
						return;
					}
					thing = pawnPath.FirstBlockingBuilding(out var _, pawn);
				}
				if (thing != null && thing.Position.InHorDistOf(pawn.Position, 6f))
				{
					job.targetA = thing;
					job.maxNumMeleeAttacks = Rand.RangeInclusive(2, 5);
					job.expiryInterval = Rand.Range(2000, 4000);
					return;
				}
			}
			base.Notify_PatherFailed();
		}

		public override bool IsContinuation(Job j)
		{
			return job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
		}
	}
}
