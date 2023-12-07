using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Verse;
using Verse.Sound;
using RimWorld;

namespace Verse.AI{
public class JobDriver_Equip : JobDriver
{
	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		int maxPawns = 1, stackCount = ReservationManager.StackCount_All;
		if (job.targetA.HasThing && job.targetA.Thing.Spawned && job.targetA.Thing.def.IsIngestible)
		{
			// Special case for ingestibles, beer for example can be equipped
			// In this case we need to only register one item of the stack with the max pawn count used for all ingestibles.
			maxPawns = Toils_Ingest.MaxPawnReservations;
			stackCount = 1;
		}

		return pawn.Reserve(job.targetA, job, maxPawns, stackCount, errorOnFailed: errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDestroyedOrNull(TargetIndex.A);
		this.FailOnBurningImmobile(TargetIndex.A);

        //Clear dropped weapon
        yield return Toils_General.Do(() => pawn.mindState.droppedWeapon = null);

		//Goto equipment
		{
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
				.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		}
		
		//Take equipment
		{
			Toil takeEquipment = ToilMaker.MakeToil();
			takeEquipment.initAction = ()=>
			{
				ThingWithComps eq = ((ThingWithComps)job.targetA.Thing);
				ThingWithComps toEquip = null;

				if( eq.def.stackLimit > 1 && eq.stackCount > 1 )
					toEquip = (ThingWithComps)eq.SplitOff(1);
				else
				{
					toEquip = eq;
					toEquip.DeSpawn();
				}

				pawn.equipment.MakeRoomFor(toEquip);
				pawn.equipment.AddEquipment(toEquip);
		
				if( eq.def.soundInteract != null )
					eq.def.soundInteract.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
			};
			takeEquipment.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return takeEquipment;
		}
	}
}}











