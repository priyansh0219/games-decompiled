using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public abstract class JobGiver_AIFightEnemy : ThinkNode_JobGiver
	{
		private float targetAcquireRadius = 56f;

		private float targetKeepRadius = 65f;

		private bool needLOSToAcquireNonPawnTargets;

		private bool chaseTarget;

		protected bool allowTurrets;

		public static readonly IntRange ExpiryInterval_ShooterSucceeded = new IntRange(450, 550);

		public static readonly IntRange ExpiryInterval_Melee = new IntRange(360, 480);

		private const int MinTargetDistanceToMove = 5;

		private const int TicksSinceEngageToLoseTarget = 400;

		protected abstract bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest, Verb verbToUse = null);

		protected virtual float GetFlagRadius(Pawn pawn)
		{
			return 999999f;
		}

		protected virtual IntVec3 GetFlagPosition(Pawn pawn)
		{
			return IntVec3.Invalid;
		}

		protected virtual bool ExtraTargetValidator(Pawn pawn, Thing target)
		{
			if (pawn.IsColonyMechPlayerControlled && target.Faction == Faction.OfPlayer)
			{
				return false;
			}
			return true;
		}

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_AIFightEnemy obj = (JobGiver_AIFightEnemy)base.DeepCopy(resolve);
			obj.targetAcquireRadius = targetAcquireRadius;
			obj.targetKeepRadius = targetKeepRadius;
			obj.needLOSToAcquireNonPawnTargets = needLOSToAcquireNonPawnTargets;
			obj.chaseTarget = chaseTarget;
			obj.allowTurrets = allowTurrets;
			return obj;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			UpdateEnemyTarget(pawn);
			Thing enemyTarget = pawn.mindState.enemyTarget;
			if (enemyTarget == null)
			{
				return null;
			}
			if (enemyTarget is Pawn pawn2 && pawn2.IsInvisible())
			{
				return null;
			}
			bool flag = !pawn.IsColonist;
			if (flag)
			{
				Job abilityJob = GetAbilityJob(pawn, enemyTarget);
				if (abilityJob != null)
				{
					return abilityJob;
				}
			}
			Verb verb = pawn.TryGetAttackVerb(enemyTarget, flag, allowTurrets);
			if (verb == null)
			{
				return null;
			}
			if (verb.verbProps.IsMeleeAttack)
			{
				return MeleeAttackJob(enemyTarget);
			}
			bool num = CoverUtility.CalculateOverallBlockChance(pawn, enemyTarget.Position, pawn.Map) > 0.01f;
			bool flag2 = pawn.Position.Standable(pawn.Map) && pawn.Map.pawnDestinationReservationManager.CanReserve(pawn.Position, pawn, pawn.Drafted);
			bool flag3 = verb.CanHitTarget(enemyTarget);
			bool flag4 = (pawn.Position - enemyTarget.Position).LengthHorizontalSquared < 25;
			if ((num && flag2 && flag3) || (flag4 && flag3))
			{
				return JobMaker.MakeJob(JobDefOf.Wait_Combat, ExpiryInterval_ShooterSucceeded.RandomInRange, checkOverrideOnExpiry: true);
			}
			if (!TryFindShootingPosition(pawn, out var dest))
			{
				return null;
			}
			if (dest == pawn.Position)
			{
				return JobMaker.MakeJob(JobDefOf.Wait_Combat, ExpiryInterval_ShooterSucceeded.RandomInRange, checkOverrideOnExpiry: true);
			}
			Job job = JobMaker.MakeJob(JobDefOf.Goto, dest);
			job.expiryInterval = ExpiryInterval_ShooterSucceeded.RandomInRange;
			job.checkOverrideOnExpire = true;
			return job;
		}

		private Job GetAbilityJob(Pawn pawn, Thing enemyTarget)
		{
			if (pawn.abilities == null)
			{
				return null;
			}
			List<Ability> list = pawn.abilities.CastableOffensiveAbilities(enemyTarget);
			if (list.NullOrEmpty())
			{
				return null;
			}
			if (pawn.Position.Standable(pawn.Map) && pawn.Map.pawnDestinationReservationManager.CanReserve(pawn.Position, pawn, pawn.Drafted))
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].verb.CanHitTarget(enemyTarget))
					{
						return list[i].GetJob(enemyTarget, enemyTarget);
					}
				}
				for (int j = 0; j < list.Count; j++)
				{
					LocalTargetInfo localTargetInfo = list[j].AIGetAOETarget();
					if (localTargetInfo.IsValid)
					{
						return list[j].GetJob(localTargetInfo, localTargetInfo);
					}
				}
			}
			return null;
		}

		protected virtual Job MeleeAttackJob(Thing enemyTarget)
		{
			Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, enemyTarget);
			job.expiryInterval = ExpiryInterval_Melee.RandomInRange;
			job.checkOverrideOnExpire = true;
			job.expireRequiresEnemiesNearby = true;
			return job;
		}

		protected virtual void UpdateEnemyTarget(Pawn pawn)
		{
			Thing thing = pawn.mindState.enemyTarget;
			if (thing != null && (thing.Destroyed || Find.TickManager.TicksGame - pawn.mindState.lastEngageTargetTick > 400 || !pawn.CanReach(thing, PathEndMode.Touch, Danger.Deadly) || (float)(pawn.Position - thing.Position).LengthHorizontalSquared > targetKeepRadius * targetKeepRadius || ((IAttackTarget)thing).ThreatDisabled(pawn)))
			{
				thing = null;
			}
			if (thing == null)
			{
				thing = FindAttackTargetIfPossible(pawn);
				if (thing != null)
				{
					pawn.mindState.Notify_EngagedTarget();
					pawn.GetLord()?.Notify_PawnAcquiredTarget(pawn, thing);
				}
			}
			else
			{
				Thing thing2 = FindAttackTargetIfPossible(pawn);
				if (thing2 == null && !chaseTarget)
				{
					thing = null;
				}
				else if (thing2 != null && thing2 != thing)
				{
					pawn.mindState.Notify_EngagedTarget();
					thing = thing2;
				}
			}
			pawn.mindState.enemyTarget = thing;
			if (thing is Pawn && thing.Faction == Faction.OfPlayer && pawn.Position.InHorDistOf(thing.Position, 40f))
			{
				Find.TickManager.slower.SignalForceNormalSpeed();
			}
		}

		private Thing FindAttackTargetIfPossible(Pawn pawn)
		{
			if (pawn.TryGetAttackVerb(null, !pawn.IsColonist) == null)
			{
				return null;
			}
			return FindAttackTarget(pawn);
		}

		protected virtual Thing FindAttackTarget(Pawn pawn)
		{
			TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
			if (needLOSToAcquireNonPawnTargets)
			{
				targetScanFlags |= TargetScanFlags.NeedLOSToNonPawns;
			}
			if (PrimaryVerbIsIncendiary(pawn))
			{
				targetScanFlags |= TargetScanFlags.NeedNonBurning;
			}
			return (Thing)AttackTargetFinder.BestAttackTarget(pawn, targetScanFlags, (Thing x) => ExtraTargetValidator(pawn, x), 0f, targetAcquireRadius, GetFlagPosition(pawn), GetFlagRadius(pawn));
		}

		private bool PrimaryVerbIsIncendiary(Pawn pawn)
		{
			if (pawn.equipment != null && pawn.equipment.Primary != null)
			{
				List<Verb> allVerbs = pawn.equipment.Primary.GetComp<CompEquippable>().AllVerbs;
				for (int i = 0; i < allVerbs.Count; i++)
				{
					if (allVerbs[i].verbProps.isPrimary)
					{
						return allVerbs[i].IsIncendiary_Ranged();
					}
				}
			}
			return false;
		}
	}
}
