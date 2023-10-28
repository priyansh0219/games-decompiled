using System.Collections.Generic;
using RimWorld;

namespace Verse.AI
{
	public class JobDriver_HaulToContainer : JobDriver
	{
		private Effecter graveDigEffect;

		protected const TargetIndex CarryThingIndex = TargetIndex.A;

		public const TargetIndex DestIndex = TargetIndex.B;

		protected const TargetIndex PrimaryDestIndex = TargetIndex.C;

		protected const int DiggingEffectInterval = 80;

		public Thing ThingToCarry => (Thing)job.GetTarget(TargetIndex.A);

		public Thing Container => (Thing)job.GetTarget(TargetIndex.B);

		protected virtual int Duration
		{
			get
			{
				if (Container == null || !(Container is Building))
				{
					return 0;
				}
				return Container.def.building.haulToContainerDuration;
			}
		}

		protected virtual EffecterDef WorkEffecter => null;

		protected virtual SoundDef WorkSustainer => null;

		public override string GetReport()
		{
			Thing thing = null;
			thing = ((pawn.CurJob != job || pawn.carryTracker.CarriedThing == null) ? base.TargetThingA : pawn.carryTracker.CarriedThing);
			if (thing == null || !job.targetB.HasThing)
			{
				return "ReportHaulingUnknown".Translate();
			}
			return ((job.GetTarget(TargetIndex.B).Thing is Building_Grave) ? "ReportHaulingToGrave" : "ReportHaulingTo").Translate(thing.Label, job.targetB.Thing.LabelShort.Named("DESTINATION"), thing.Named("THING"));
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (!pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			if (!pawn.Reserve(job.GetTarget(TargetIndex.B), job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.A), job);
			pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
			return true;
		}

		protected virtual void ModifyPrepareToil(Toil toil)
		{
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
			this.FailOn(() => TransporterUtility.WasLoadingCanceled(Container));
			this.FailOn(() => CompBiosculpterPod.WasLoadingCanceled(Container));
			this.FailOn(() => Building_SubcoreScanner.WasLoadingCancelled(Container));
			this.FailOn(delegate
			{
				ThingOwner thingOwner = Container.TryGetInnerInteractableThingOwner();
				if (thingOwner != null && !thingOwner.CanAcceptAnyOf(ThingToCarry))
				{
					return true;
				}
				return (Container is IHaulDestination haulDestination && !haulDestination.Accepts(ThingToCarry)) ? true : false;
			});
			Toil getToHaulTarget = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			Toil uninstallIfMinifiable = Toils_Construct.UninstallIfMinifiable(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			Toil startCarryingThing = Toils_Haul.StartCarryThing(TargetIndex.A, putRemainderInQueue: false, subtractNumTakenFromJobCount: true);
			Toil jumpIfAlsoCollectingNextTarget = Toils_Haul.JumpIfAlsoCollectingNextTargetInQueue(getToHaulTarget, TargetIndex.A);
			Toil carryToContainer = Toils_Haul.CarryHauledThingToContainer();
			yield return Toils_Jump.JumpIf(jumpIfAlsoCollectingNextTarget, () => pawn.IsCarryingThing(ThingToCarry));
			yield return getToHaulTarget;
			yield return uninstallIfMinifiable;
			yield return startCarryingThing;
			yield return jumpIfAlsoCollectingNextTarget;
			yield return carryToContainer;
			yield return Toils_Goto.MoveOffTargetBlueprint(TargetIndex.B);
			Toil toil = Toils_General.Wait(Duration, TargetIndex.B);
			toil.WithProgressBarToilDelay(TargetIndex.B);
			EffecterDef workEffecter = WorkEffecter;
			if (workEffecter != null)
			{
				toil.WithEffect(workEffecter, TargetIndex.B);
			}
			SoundDef workSustainer = WorkSustainer;
			if (workSustainer != null)
			{
				toil.PlaySustainerOrSound(workSustainer);
			}
			Thing destThing = job.GetTarget(TargetIndex.B).Thing;
			toil.tickAction = delegate
			{
				if (pawn.IsHashIntervalTick(80) && destThing is Building_Grave && graveDigEffect == null)
				{
					graveDigEffect = EffecterDefOf.BuryPawn.Spawn();
					graveDigEffect.Trigger(destThing, destThing);
				}
				graveDigEffect?.EffectTick(destThing, destThing);
			};
			ModifyPrepareToil(toil);
			yield return toil;
			yield return Toils_Construct.MakeSolidThingFromBlueprintIfNecessary(TargetIndex.B, TargetIndex.C);
			yield return Toils_Haul.DepositHauledThingInContainer(TargetIndex.B, TargetIndex.C);
			yield return Toils_Haul.JumpToCarryToNextContainerIfPossible(carryToContainer, TargetIndex.C);
		}
	}
}
