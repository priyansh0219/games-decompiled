using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Verse.AI
{

public static class Toils_Reserve
{
    public static Toil ReserveDestination( TargetIndex ind)
	{
		Toil toil = ToilMaker.MakeToil();
		toil.initAction = ()=>
		{
            toil.actor.Map.pawnDestinationReservationManager.Reserve(
                toil.actor, toil.actor.CurJob, toil.actor.jobs.curJob.GetTarget(ind).Cell);
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		toil.atomicWithPrevious = true;
		return toil;
	}

    public static Toil Reserve( TargetIndex ind, int maxPawns = 1, int stackCount = ReservationManager.StackCount_All, ReservationLayerDef layer = null )
	{
		Toil toil = ToilMaker.MakeToil();
		toil.initAction = ()=>
		{
			if( !toil.actor.Reserve(toil.actor.jobs.curJob.GetTarget(ind), toil.actor.CurJob, maxPawns, stackCount, layer ) )
				toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		toil.atomicWithPrevious = true;
		return toil;
	}

    public static Toil ReserveDestinationOrThing(TargetIndex ind)
    {
        Toil toil = ToilMaker.MakeToil();
		toil.initAction = ()=>
		{
            LocalTargetInfo target = toil.actor.jobs.curJob.GetTarget(ind);
            if (target.HasThing)
            {
                if( !toil.actor.Reserve(target, toil.actor.CurJob) )
				    toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
            }
            else
                toil.actor.Map.pawnDestinationReservationManager.Reserve(
                    toil.actor, toil.actor.CurJob, target.Cell);
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		toil.atomicWithPrevious = true;
		return toil;
    }

	public static Toil ReserveQueue( TargetIndex ind, int maxPawns = 1, int stackCount = ReservationManager.StackCount_All, ReservationLayerDef layer = null )
	{
		Toil toil = ToilMaker.MakeToil();
		toil.initAction = ()=>
		{
			var queue = toil.actor.jobs.curJob.GetTargetQueue(ind);
			if( queue != null )
			{
				for( int i=0; i<queue.Count; i++ )
				{
					if( !toil.actor.Reserve(queue[i], toil.actor.CurJob, maxPawns, stackCount, layer ) )
						toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
				}
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		toil.atomicWithPrevious = true;
		return toil;
	}

	public static Toil Release( TargetIndex ind )
	{
		Toil toil = ToilMaker.MakeToil();
		toil.initAction = ()=>
		{
            LocalTargetInfo reservedItem = toil.actor.jobs.curJob.GetTarget(ind);
			toil.actor.Map.reservationManager.Release(reservedItem, toil.actor, toil.actor.CurJob);
            toil.actor.jobs.ReleaseReservations(reservedItem);
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		toil.atomicWithPrevious = true;
		return toil;
	}

    public static Toil ReleaseDestination()
	{
		Toil toil = ToilMaker.MakeToil();
		toil.initAction = ()=>
		{
			toil.actor.Map.pawnDestinationReservationManager.ReleaseClaimedBy(toil.actor, toil.actor.CurJob);
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		toil.atomicWithPrevious = true;
		return toil;
	}

    public static Toil ReleaseDestinationOrThing(TargetIndex ind)
	{
		Toil toil = ToilMaker.MakeToil();
		toil.initAction = ()=>
		{
            LocalTargetInfo target = toil.actor.jobs.curJob.GetTarget(ind);
            if (target.HasThing)
            {
                toil.actor.Map.reservationManager.Release(target, toil.actor, toil.actor.CurJob);
                toil.actor.jobs.ReleaseReservations(target);
            }
            else
			    toil.actor.Map.pawnDestinationReservationManager.ReleaseClaimedBy(toil.actor, toil.actor.CurJob);
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		toil.atomicWithPrevious = true;
		return toil;
	}


}}

