using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class JobDriver_ManTurret : JobDriver
	{
		private const float SearchRadius = 40f;

		private const int MaxPawnReservations = 10;

		private static bool GunNeedsLoading(Building b)
		{
			if (!(b is Building_TurretGun building_TurretGun))
			{
				return false;
			}
			CompChangeableProjectile compChangeableProjectile = building_TurretGun.gun.TryGetComp<CompChangeableProjectile>();
			if (compChangeableProjectile == null || compChangeableProjectile.Loaded)
			{
				return false;
			}
			return true;
		}

		private static bool GunNeedsRefueling(Building b)
		{
			if (!(b is Building_TurretGun thing))
			{
				return false;
			}
			CompRefuelable compRefuelable = thing.TryGetComp<CompRefuelable>();
			if (compRefuelable == null || compRefuelable.HasFuel || !compRefuelable.Props.fuelIsMortarBarrel || Find.Storyteller.difficulty.classicMortars)
			{
				return false;
			}
			return true;
		}

		public static Thing FindAmmoForTurret(Pawn pawn, Building_TurretGun gun)
		{
			StorageSettings allowedShellsSettings = (pawn.IsColonist ? gun.gun.TryGetComp<CompChangeableProjectile>().allowedShellsSettings : null);
			Predicate<Thing> validator = delegate(Thing t)
			{
				if (t.IsForbidden(pawn))
				{
					return false;
				}
				if (!pawn.CanReserve(t, 10, 1))
				{
					return false;
				}
				return (allowedShellsSettings == null || allowedShellsSettings.AllowedToAccept(t)) ? true : false;
			};
			return GenClosest.ClosestThingReachable(gun.Position, gun.Map, ThingRequest.ForGroup(ThingRequestGroup.Shell), PathEndMode.OnCell, TraverseParms.For(pawn), 40f, validator);
		}

		public static Thing FindFuelForTurret(Pawn pawn, Building_TurretGun gun)
		{
			CompRefuelable refuelableComp = gun.TryGetComp<CompRefuelable>();
			if (refuelableComp == null)
			{
				return null;
			}
			Predicate<Thing> validator = (Thing t) => !t.IsForbidden(pawn) && pawn.CanReserve(t, 10, 1) && refuelableComp.Props.fuelFilter.Allows(t);
			return GenClosest.ClosestThingReachable(gun.Position, gun.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.OnCell, TraverseParms.For(pawn), 40f, validator);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			Toil gotoTurret = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			Toil refuelIfNeeded = ToilMaker.MakeToil("MakeNewToils");
			refuelIfNeeded.initAction = delegate
			{
				Pawn actor4 = refuelIfNeeded.actor;
				Building building2 = (Building)actor4.CurJob.targetA.Thing;
				Building_TurretGun building_TurretGun3 = building2 as Building_TurretGun;
				if (!GunNeedsRefueling(building2))
				{
					JumpToToil(gotoTurret);
				}
				else
				{
					Thing thing2 = FindFuelForTurret(pawn, building_TurretGun3);
					if (thing2 == null)
					{
						CompRefuelable compRefuelable = building2.TryGetComp<CompRefuelable>();
						if (actor4.Faction == Faction.OfPlayer && compRefuelable != null)
						{
							Messages.Message("MessageOutOfNearbyFuelFor".Translate(actor4.LabelShort, building_TurretGun3.Label, actor4.Named("PAWN"), building_TurretGun3.Named("GUN"), compRefuelable.Props.fuelFilter.Summary.Named("FUEL")).CapitalizeFirst(), building_TurretGun3, MessageTypeDefOf.NegativeEvent);
						}
						actor4.jobs.EndCurrentJob(JobCondition.Incompletable);
					}
					actor4.CurJob.targetB = thing2;
					actor4.CurJob.count = 1;
				}
			};
			yield return refuelIfNeeded;
			yield return Toils_Reserve.Reserve(TargetIndex.B, 10, 1);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(240).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A)
				.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
				.WithProgressBarToilDelay(TargetIndex.A);
			yield return Toils_Refuel.FinalizeRefueling(TargetIndex.A, TargetIndex.B);
			Toil loadIfNeeded = ToilMaker.MakeToil("MakeNewToils");
			loadIfNeeded.initAction = delegate
			{
				Pawn actor3 = loadIfNeeded.actor;
				Building obj = (Building)actor3.CurJob.targetA.Thing;
				Building_TurretGun building_TurretGun2 = obj as Building_TurretGun;
				if (!GunNeedsLoading(obj))
				{
					JumpToToil(gotoTurret);
				}
				else
				{
					Thing thing = FindAmmoForTurret(pawn, building_TurretGun2);
					if (thing == null)
					{
						if (actor3.Faction == Faction.OfPlayer)
						{
							Messages.Message("MessageOutOfNearbyShellsFor".Translate(actor3.LabelShort, building_TurretGun2.Label, actor3.Named("PAWN"), building_TurretGun2.Named("GUN")).CapitalizeFirst(), building_TurretGun2, MessageTypeDefOf.NegativeEvent);
						}
						actor3.jobs.EndCurrentJob(JobCondition.Incompletable);
					}
					actor3.CurJob.targetB = thing;
					actor3.CurJob.count = 1;
				}
			};
			yield return loadIfNeeded;
			yield return Toils_Reserve.Reserve(TargetIndex.B, 10, 1);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil loadShell = ToilMaker.MakeToil("MakeNewToils");
			loadShell.initAction = delegate
			{
				Pawn actor2 = loadShell.actor;
				Building_TurretGun building_TurretGun = ((Building)actor2.CurJob.targetA.Thing) as Building_TurretGun;
				SoundDefOf.Artillery_ShellLoaded.PlayOneShot(new TargetInfo(building_TurretGun.Position, building_TurretGun.Map));
				building_TurretGun.gun.TryGetComp<CompChangeableProjectile>().LoadShell(actor2.CurJob.targetB.Thing.def, 1);
				actor2.carryTracker.innerContainer.ClearAndDestroyContents();
			};
			yield return loadShell;
			yield return gotoTurret;
			Toil man = ToilMaker.MakeToil("MakeNewToils");
			man.tickAction = delegate
			{
				Pawn actor = man.actor;
				Building building = (Building)actor.CurJob.targetA.Thing;
				if (GunNeedsLoading(building))
				{
					JumpToToil(loadIfNeeded);
				}
				else if (GunNeedsRefueling(building))
				{
					JumpToToil(refuelIfNeeded);
				}
				else
				{
					building.GetComp<CompMannable>().ManForATick(actor);
					man.actor.rotationTracker.FaceCell(building.Position);
				}
			};
			man.handlingFacing = true;
			man.defaultCompleteMode = ToilCompleteMode.Never;
			man.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			yield return man;
		}
	}
}
