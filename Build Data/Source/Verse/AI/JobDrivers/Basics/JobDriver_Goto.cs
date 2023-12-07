using UnityEngine;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse.AI.Group;


namespace Verse.AI{
public class JobDriver_Goto : JobDriver
{
	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		pawn.Map.pawnDestinationReservationManager.Reserve( pawn, job, job.targetA.Cell );

		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
        var lookAtTarget = job.GetTarget(TargetIndex.B);

		{
			var gotoCell = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			gotoCell.AddPreTickAction(() =>
				{
					// we check exit grid every tick to make sure the pawn leaves the map as soon as possible
					if( job.exitMapOnArrival && pawn.Map.exitMapGrid.IsExitCell(pawn.Position) )
						TryExitMap();
				});

			// only allowed to join or create caravan?
			gotoCell.FailOn(() => job.failIfCantJoinOrCreateCaravan && !CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(pawn));

            // Legacy behavior: fail if we are pathing to a dead pawn.
            gotoCell.FailOn(() =>
            {
                var targetThing = job.GetTarget(TargetIndex.A).Thing;
                if (!(targetThing is Pawn pawn))
                    return false;

                return pawn.ParentHolder is Corpse;
            });

            // Legacy behavior: fail if we are pathing to a destroyed thing.
            gotoCell.FailOn(() => job.GetTarget(TargetIndex.A).Thing?.Destroyed == true);

            if (lookAtTarget.IsValid)
            {
                gotoCell.tickAction += () => pawn.rotationTracker.FaceCell(lookAtTarget.Cell);
                gotoCell.handlingFacing = true;
            }

            gotoCell.AddFinishAction(() =>
            {
                if (job.controlGroupTag == null)
                    return;
                
                if(job.controlGroupTag != null)
                {
                    var overseer = pawn.GetOverseer();
                    if(overseer != null)
                    {
                        var controlGroup = overseer.mechanitor.GetControlGroup(pawn);
                        controlGroup.SetTag(pawn, job.controlGroupTag);
                    }
                }
            });

			yield return gotoCell;
		}

		{
			Toil arrive = ToilMaker.MakeToil();
			arrive.initAction = () =>
				{
					// check if we arrived to our forced goto position
					if( pawn.mindState != null && pawn.mindState.forcedGotoPosition == TargetA.Cell )
						pawn.mindState.forcedGotoPosition = IntVec3.Invalid;

                    if( !job.ritualTag.NullOrEmpty() )
                    {
                        var lordJob = pawn.GetLord()?.LordJob as LordJob_Ritual;
                        if( lordJob != null )
                            lordJob.AddTagForPawn(pawn, job.ritualTag);
                    }

                    if( job.exitMapOnArrival && (pawn.Position.OnEdge(pawn.Map) || pawn.Map.exitMapGrid.IsExitCell(pawn.Position)) )
						TryExitMap();
				};

			arrive.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return arrive;
		}
	}

    private void TryExitMap()
	{
		// only allowed to join or create caravan?
		if( job.failIfCantJoinOrCreateCaravan && !CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(pawn) )
			return;

        if( ModsConfig.BiotechActive )
            MechanitorUtility.Notify_PawnGotoLeftMap(pawn, pawn.Map);

		pawn.ExitMap(true, CellRect.WholeMap(Map).GetClosestEdge(pawn.Position));
	}
}}

