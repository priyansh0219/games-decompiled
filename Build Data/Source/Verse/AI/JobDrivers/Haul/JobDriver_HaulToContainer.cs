using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;


namespace Verse.AI{
public class JobDriver_HaulToContainer : JobDriver
{
    //Working vars
    private Effecter graveDigEffect;
    
	//Constants
	protected const TargetIndex CarryThingIndex = TargetIndex.A;
	public const TargetIndex DestIndex = TargetIndex.B;
	protected const TargetIndex PrimaryDestIndex = TargetIndex.C;
    protected const int DiggingEffectInterval = 80;

	public Thing ThingToCarry { get { return (Thing)job.GetTarget(CarryThingIndex); } }
	public Thing Container { get { return (Thing)job.GetTarget(DestIndex); } }
	protected virtual int Duration { get { return Container != null && Container is Building ? Container.def.building.haulToContainerDuration : 0; } }
    protected virtual EffecterDef WorkEffecter => null;
    protected virtual SoundDef WorkSustainer => null;

    public override string GetReport()
	{
		Thing hauledThing = null;
		if( pawn.CurJob == job && pawn.carryTracker.CarriedThing != null )
			hauledThing = pawn.carryTracker.CarriedThing;
		else
			hauledThing = TargetThingA;

		if( hauledThing == null || !job.targetB.HasThing )
			return "ReportHaulingUnknown".Translate();
		else
		{
			string key = job.GetTarget(DestIndex).Thing is Building_Grave ? "ReportHaulingToGrave" : "ReportHaulingTo"; // Special text for hauling to grave
			return key.Translate(hauledThing.Label, job.targetB.Thing.LabelShort.Named("DESTINATION"), hauledThing.Named("THING"));
		}
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if( !pawn.Reserve(job.GetTarget(CarryThingIndex), job, errorOnFailed: errorOnFailed) )
			return false;
			
		if( !pawn.Reserve(job.GetTarget(DestIndex), job, errorOnFailed: errorOnFailed) )
			return false;

		pawn.ReserveAsManyAsPossible(job.GetTargetQueue(CarryThingIndex), job);
		pawn.ReserveAsManyAsPossible(job.GetTargetQueue(DestIndex), job);

		return true;
	}

    protected virtual void ModifyPrepareToil(Toil toil) { }

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDestroyedOrNull( CarryThingIndex );
		this.FailOnDestroyedNullOrForbidden( DestIndex );
		this.FailOn(() => TransporterUtility.WasLoadingCanceled(Container));
		this.FailOn(() => CompBiosculpterPod.WasLoadingCanceled(Container));
        this.FailOn(() => Building_SubcoreScanner.WasLoadingCancelled(Container));
		this.FailOn(() =>
			{
				var thingOwner = Container.TryGetInnerInteractableThingOwner();
				if( thingOwner != null && !thingOwner.CanAcceptAnyOf(ThingToCarry) )
					return true;

				// e.g. grave
				var haulDestination = Container as IHaulDestination;
				if( haulDestination != null && !haulDestination.Accepts(ThingToCarry) )
					return true;

				return false;
			});

		var getToHaulTarget = Toils_Goto.GotoThing( CarryThingIndex, PathEndMode.ClosestTouch )
			.FailOnSomeonePhysicallyInteracting(CarryThingIndex);

		var uninstallIfMinifiable = Toils_Construct.UninstallIfMinifiable(CarryThingIndex)
			.FailOnSomeonePhysicallyInteracting(CarryThingIndex);

		var startCarryingThing = Toils_Haul.StartCarryThing(CarryThingIndex, subtractNumTakenFromJobCount: true);
		
        var jumpIfAlsoCollectingNextTarget = Toils_Haul.JumpIfAlsoCollectingNextTargetInQueue( getToHaulTarget, CarryThingIndex );
		var carryToContainer = Toils_Haul.CarryHauledThingToContainer();
		
        //Jump moving to and attempting to carry our target if we're already carrying it (e.g. drafted carry of a pawn)
        yield return Toils_Jump.JumpIf(jumpIfAlsoCollectingNextTarget, () => pawn.IsCarryingThing(ThingToCarry));

        yield return getToHaulTarget;
        yield return uninstallIfMinifiable;
        yield return startCarryingThing;
        yield return jumpIfAlsoCollectingNextTarget;
        yield return carryToContainer;
        
		yield return Toils_Goto.MoveOffTargetBlueprint(DestIndex);
		
		//Prepare
		{
			var prepare = Toils_General.Wait(Duration, face: DestIndex);
			prepare.WithProgressBarToilDelay(DestIndex);

            var workEffecter = WorkEffecter;
            if (workEffecter != null)
                prepare.WithEffect(workEffecter, DestIndex);

            var workSustainer = WorkSustainer;
            if (workSustainer != null)
                prepare.PlaySustainerOrSound(workSustainer);
            
            var destThing = job.GetTarget(DestIndex).Thing;
            
            prepare.tickAction = () =>
            {
                if( pawn.IsHashIntervalTick(DiggingEffectInterval) && destThing is Building_Grave )
                {
                    if( graveDigEffect == null )
                    {
                        graveDigEffect = EffecterDefOf.BuryPawn.Spawn();
                        graveDigEffect.Trigger(destThing, destThing);
                    }
                }
                
                graveDigEffect?.EffectTick(destThing, destThing);
            };

            ModifyPrepareToil(prepare);

            yield return prepare;
		}
		
		yield return Toils_Construct.MakeSolidThingFromBlueprintIfNecessary(DestIndex, PrimaryDestIndex);
		
		yield return Toils_Haul.DepositHauledThingInContainer(DestIndex, PrimaryDestIndex);
		
		yield return Toils_Haul.JumpToCarryToNextContainerIfPossible(carryToContainer, PrimaryDestIndex);
	}
}}

