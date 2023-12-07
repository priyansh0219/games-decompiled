using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Verse.AI
{

public static class Toils_General
{
    public static Toil StopDead()
    {
        var toil = ToilMaker.MakeToil();
        toil.initAction = ()=>
        {
            toil.actor.pather.StopDead();
        };
        toil.defaultCompleteMode = ToilCompleteMode.Instant;

        return toil;
    }

	public static Toil Wait( int ticks, TargetIndex face = TargetIndex.None )
	{
		var toil = ToilMaker.MakeToil();
		toil.initAction = ()=>
			{
				toil.actor.pather.StopDead();
			};
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.defaultDuration = ticks;

		if( face != TargetIndex.None )
		{
			toil.handlingFacing = true;
			toil.tickAction = () => toil.actor.rotationTracker.FaceTarget(toil.actor.CurJob.GetTarget(face));
		}

		return toil;
	}

	public static Toil WaitWith(TargetIndex targetInd, int ticks, bool useProgressBar = false, bool maintainPosture = false, bool maintainSleep = false, TargetIndex face = TargetIndex.None)
	{
		var toil = ToilMaker.MakeToil();
		toil.initAction = () =>
			{
				toil.actor.pather.StopDead();

				var otherPawn = toil.actor.CurJob.GetTarget(targetInd).Thing as Pawn;

				if( otherPawn != null )
				{
					if( otherPawn == toil.actor )
						Log.Warning("Executing WaitWith toil but otherPawn is the same as toil.actor");
					else
						PawnUtility.ForceWait(otherPawn, ticks, maintainPosture: maintainPosture, maintainSleep: maintainSleep);
				}
			};
		toil.FailOnDespawnedOrNull(targetInd);
		toil.FailOnCannotTouch(targetInd, PathEndMode.Touch);
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.defaultDuration = ticks;

        if(face != TargetIndex.None)
        {
            toil.handlingFacing = true;
            toil.tickAction = () => toil.actor.rotationTracker.FaceTarget(toil.actor.CurJob.GetTarget(face));
        }

		if( useProgressBar )
			toil.WithProgressBarToilDelay(targetInd);

		return toil;
	}

	public static Toil RemoveDesignationsOnThing( TargetIndex ind, DesignationDef def )
	{
		Toil toil = ToilMaker.MakeToil();
		toil.initAction = ()=>
			{
				toil.actor.Map.designationManager.RemoveAllDesignationsOn( toil.actor.jobs.curJob.GetTarget(ind).Thing );
			};
		return toil;

	}

	public static Toil ClearTarget( TargetIndex ind )
	{
		Toil toil = ToilMaker.MakeToil();
		toil.initAction = ()=>
			{
				toil.GetActor().CurJob.SetTarget(ind, null);
			};
		return toil;
	}

	public static Toil PutCarriedThingInInventory()
	{
		Toil toil = ToilMaker.MakeToil();
		toil.initAction = ()=>
			{
				var actor = toil.GetActor();
				if( actor.carryTracker.CarriedThing != null )
				{
					//Try transfer to inventory
					if( !actor.carryTracker.innerContainer.TryTransferToContainer(actor.carryTracker.CarriedThing, actor.inventory.innerContainer) )
					{
						//Failed: try drop
						actor.carryTracker.TryDropCarriedThing(actor.Position, actor.carryTracker.CarriedThing.stackCount, ThingPlaceMode.Near, out _ );
					}
				}
			};
		return toil;
	}

	public static Toil Do(Action action)
	{
		var toil = ToilMaker.MakeToil();
		toil.initAction = action;
		return toil;
	}

	public static Toil DoAtomic(Action action)
	{
		var toil = ToilMaker.MakeToil();
		toil.initAction = action;
		toil.atomicWithPrevious = true;
		return toil;
	}

	public static Toil Open(TargetIndex openableInd)
	{
		var open = ToilMaker.MakeToil();
		open.initAction = () =>
			{
				var actor = open.actor;
				var t = actor.CurJob.GetTarget(openableInd).Thing;

				var des = actor.Map.designationManager.DesignationOn(t, DesignationDefOf.Open);
				if( des != null )
					des.Delete();

				var openable = (IOpenable)t;

				if( openable.CanOpen )
				{
					openable.Open();
					actor.records.Increment(RecordDefOf.ContainersOpened);
				}
			};
		open.defaultCompleteMode = ToilCompleteMode.Instant;
		return open;
	}

	// This is intended as a destination for jumps. It doesn't do anything, it just makes complex jobdriver flow easier to grok.
	public static Toil Label()
	{
		Toil toil = ToilMaker.MakeToil();
		toil.atomicWithPrevious = true;
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		return toil;
	}

    public static Toil WaitWhileExtractingContents(TargetIndex containerInd, TargetIndex contentsInd, int openTicks)
    {
        //Wait while opening
        var extract = Toils_General.Wait(openTicks, containerInd)
            .WithProgressBarToilDelay(containerInd)
            .FailOnDespawnedOrNull(containerInd);
        extract.handlingFacing = true;
        extract.AddPreInitAction(() => 
        {
            var actor = extract.actor;
            var contents = actor.CurJob.GetTarget(contentsInd).Thing;
            QuestUtility.SendQuestTargetSignals(contents.questTags, QuestUtility.QuestTargetSignalPart_StartedExtractingFromContainer, contents.Named(SignalArgsNames.Subject));
        });
        return extract;
    }

}

}
