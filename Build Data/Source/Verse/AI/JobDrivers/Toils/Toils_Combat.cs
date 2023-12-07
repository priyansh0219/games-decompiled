using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace Verse.AI{
public static class Toils_Combat
{
	public static Toil TrySetJobToUseAttackVerb(TargetIndex targetInd)
	{
		Toil toil = ToilMaker.MakeToil();
		toil.initAction = ()=>
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
                bool allowManualCastWeapons = !actor.IsColonist;

				Verb verb = actor.TryGetAttackVerb(curJob.GetTarget(targetInd).Thing, allowManualCastWeapons);

				if( verb == null )
				{
					actor.jobs.EndCurrentJob( JobCondition.Incompletable);
					return;
				}

				curJob.verbToUse = verb;
			};
		return toil;
	}
	

	public static Toil GotoCastPosition( TargetIndex targetInd, TargetIndex castPositionInd = TargetIndex.None, bool closeIfDowned = false, float maxRangeFactor = 1f )
	{
		Toil toil = ToilMaker.MakeToil();
		toil.initAction = ()=>
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				Thing targThing = curJob.GetTarget(targetInd).Thing;
				Pawn targPawn = targThing as Pawn;

                if( actor == targThing ) // we are already there, as in the case of a self-cast verb
                {
                    actor.pather.StopDead();
                    actor.jobs.curDriver.ReadyForNextToil();
                    return;
                }

                if( targThing == null ) // target is a map location not a thing, don't attempt to get closer
                {
                    actor.pather.StopDead();
                    actor.jobs.curDriver.ReadyForNextToil();
                    return;
                }

				//We get closer if the target is downed and we can
				CastPositionRequest req = new CastPositionRequest();
				req.caster = toil.actor;
				req.target = targThing;
				req.verb = curJob.verbToUse;
                req.maxRangeFromTarget = (!closeIfDowned||targPawn==null||!targPawn.Downed)
					? Mathf.Max( curJob.verbToUse.verbProps.range * maxRangeFactor, ShootTuning.MeleeRange )
					: Mathf.Min( curJob.verbToUse.verbProps.range, targPawn.RaceProps.executionRange );
				req.wantCoverFromTarget = false;

                if (castPositionInd != TargetIndex.None)
                    req.preferredCastPosition = curJob.GetTarget(castPositionInd).Cell;

				IntVec3 dest;
				if( !CastPositionFinder.TryFindCastPosition( req, out dest ) )
				{
					toil.actor.jobs.EndCurrentJob( JobCondition.Incompletable );
					return;
				}

				toil.actor.pather.StartPath( dest, PathEndMode.OnCell );

				actor.Map.pawnDestinationReservationManager.Reserve( actor, curJob, dest );
			};
		toil.FailOnDespawnedOrNull( targetInd );
		toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;

		return toil;
	}
    
    public static Toil CastVerb(TargetIndex targetInd, bool canHitNonTargetPawns = true)
    {
        return CastVerb(targetInd, TargetIndex.None, canHitNonTargetPawns);
    }
    
	public static Toil CastVerb(TargetIndex targetInd, TargetIndex destInd, bool canHitNonTargetPawns = true)
	{
		Toil toil = ToilMaker.MakeToil();
		toil.initAction = ()=>
			{
                var target = toil.actor.jobs.curJob.GetTarget(targetInd);
                var dest = destInd != TargetIndex.None ? toil.actor.jobs.curJob.GetTarget(destInd) : LocalTargetInfo.Invalid;
				toil.actor.jobs.curJob.verbToUse.TryStartCastOn(target, dest, canHitNonTargetPawns: canHitNonTargetPawns, preventFriendlyFire: toil.actor.jobs.curJob.preventFriendlyFire);
			};
		toil.defaultCompleteMode = ToilCompleteMode.FinishedBusy;
        toil.activeSkill = () => GetActiveSkillForToil(toil);
		return toil;
	}

    public static SkillDef GetActiveSkillForToil(Toil toil)
    {
        var verb = toil.actor.jobs.curJob.verbToUse;
        if (verb != null && verb.EquipmentSource != null)
        {
            if (verb.EquipmentSource.def.IsMeleeWeapon)
                return SkillDefOf.Melee;
            if (verb.EquipmentSource.def.IsRangedWeapon)
                return SkillDefOf.Shooting;
        }

        return null;
    }

    public static Toil FollowAndMeleeAttack(TargetIndex targetInd, Action hitAction)
    {
        return FollowAndMeleeAttack(targetInd, TargetIndex.None, hitAction);
    }
    public static Toil FollowAndMeleeAttack(TargetIndex targetInd, TargetIndex standPositionInd, Action hitAction)
	{
		//Follow and attack victim
		Toil followAndAttack = ToilMaker.MakeToil();
		followAndAttack.tickAction = ()=>
			{
				Pawn actor = followAndAttack.actor;
				Job curJob = actor.jobs.curJob;
				JobDriver driver = actor.jobs.curDriver;
                var target = curJob.GetTarget(targetInd);
                Thing victim = target.Thing;
				Pawn victimPawn = victim as Pawn;

				if( !victim.Spawned || (victimPawn != null && victimPawn.IsInvisible()) )
				{
					driver.ReadyForNextToil();
					return;
				}

                LocalTargetInfo pathDest = target;
                var peMode = PathEndMode.Touch;
                if( standPositionInd != TargetIndex.None ) // use requested stand position, if supplied
                {
                    var standPosition = curJob.GetTarget(standPositionInd);
                    if( standPosition.IsValid )
                    {
                        pathDest = standPosition;
                        peMode = PathEndMode.OnCell;
                    }
                }

				if( pathDest != actor.pather.Destination
                    || (!actor.pather.Moving && !actor.CanReachImmediate(target, PathEndMode.Touch)) )
                {
                    actor.pather.StartPath( pathDest, peMode );
                }
				else
				{
					if( actor.CanReachImmediate(target, PathEndMode.Touch) )
					{
						//Do not attack downed people unless the job specifies to do so
                        if( victimPawn != null && victimPawn.Downed && !curJob.killIncappedTarget )
						{
							driver.ReadyForNextToil();
							return;
						}

						//Try to hit them
						hitAction();
					}
				}
			};
        followAndAttack.activeSkill = () => SkillDefOf.Melee;
		followAndAttack.defaultCompleteMode = ToilCompleteMode.Never;
		return followAndAttack;
	}
}}



