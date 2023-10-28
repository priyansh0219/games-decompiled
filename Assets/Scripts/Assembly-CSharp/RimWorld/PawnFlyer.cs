using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class PawnFlyer : Thing, IThingHolder
	{
		private ThingOwner<Thing> innerContainer;

		protected Vector3 startVec;

		private float flightDistance;

		private bool pawnWasDrafted;

		private bool pawnCanFireAtWill = true;

		protected int ticksFlightTime = 120;

		protected int ticksFlying;

		private JobQueue jobQueue;

		protected EffecterDef flightEffecterDef;

		protected SoundDef soundLanding;

		private Thing carriedThing;

		public Pawn FlyingPawn
		{
			get
			{
				if (innerContainer.InnerListForReading.Count <= 0)
				{
					return null;
				}
				return innerContainer.InnerListForReading[0] as Pawn;
			}
		}

		public Thing CarriedThing => carriedThing;

		public Vector3 DestinationPos
		{
			get
			{
				Pawn flyingPawn = FlyingPawn;
				return GenThing.TrueCenter(base.Position, flyingPawn.Rotation, flyingPawn.def.size, flyingPawn.def.Altitude);
			}
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return innerContainer;
		}

		public PawnFlyer()
		{
			innerContainer = new ThingOwner<Thing>(this);
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				float a = Mathf.Max(flightDistance, 1f) / def.pawnFlyer.flightSpeed;
				a = Mathf.Max(a, def.pawnFlyer.flightDurationMin);
				ticksFlightTime = a.SecondsToTicks();
				ticksFlying = 0;
			}
		}

		protected virtual void RespawnPawn()
		{
			Pawn flyingPawn = FlyingPawn;
			innerContainer.TryDrop(flyingPawn, base.Position, flyingPawn.MapHeld, ThingPlaceMode.Direct, out var lastResultingThing, null, null, playDropSound: false);
			if (flyingPawn.drafter != null)
			{
				flyingPawn.drafter.Drafted = pawnWasDrafted;
				flyingPawn.drafter.FireAtWill = pawnCanFireAtWill;
			}
			if (carriedThing != null && innerContainer.TryDrop(carriedThing, base.Position, flyingPawn.MapHeld, ThingPlaceMode.Direct, out lastResultingThing, null, null, playDropSound: false))
			{
				carriedThing.DeSpawn();
				if (!flyingPawn.carryTracker.TryStartCarry(carriedThing))
				{
					Log.Error("Could not carry " + carriedThing.ToStringSafe() + " after respawning flyer pawn.");
				}
			}
			if (jobQueue != null)
			{
				flyingPawn.jobs.RestoreCapturedJobs(jobQueue);
			}
			flyingPawn.jobs.CheckForJobOverride();
		}

		public override void Tick()
		{
			if (ticksFlying >= ticksFlightTime)
			{
				RespawnPawn();
				Destroy();
			}
			else
			{
				if (ticksFlying % 5 == 0)
				{
					CheckDestination();
				}
				innerContainer.ThingOwnerTick();
			}
			ticksFlying++;
		}

		private void CheckDestination()
		{
			if (JumpUtility.ValidJumpTarget(base.Map, base.Position))
			{
				return;
			}
			int num = GenRadial.NumCellsInRadius(3.9f);
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = base.Position + GenRadial.RadialPattern[i];
				if (JumpUtility.ValidJumpTarget(base.Map, intVec))
				{
					base.Position = intVec;
					break;
				}
			}
		}

		public static PawnFlyer MakeFlyer(ThingDef flyingDef, Pawn pawn, IntVec3 destCell, EffecterDef flightEffecterDef, SoundDef landingSound, bool flyWithCarriedThing = false)
		{
			PawnFlyer pawnFlyer = (PawnFlyer)ThingMaker.MakeThing(flyingDef);
			if (!pawnFlyer.ValidateFlyer())
			{
				return null;
			}
			pawnFlyer.startVec = pawn.TrueCenter();
			pawnFlyer.flightDistance = pawn.Position.DistanceTo(destCell);
			pawnFlyer.pawnWasDrafted = pawn.Drafted;
			pawnFlyer.flightEffecterDef = flightEffecterDef;
			pawnFlyer.soundLanding = landingSound;
			if (pawn.drafter != null)
			{
				pawnFlyer.pawnCanFireAtWill = pawn.drafter.FireAtWill;
			}
			if (pawn.CurJob != null)
			{
				if (pawn.CurJob.def == JobDefOf.CastJump)
				{
					pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
				}
				else
				{
					pawn.jobs.SuspendCurrentJob(JobCondition.InterruptForced);
				}
			}
			pawnFlyer.jobQueue = pawn.jobs.CaptureAndClearJobQueue();
			if (flyWithCarriedThing && pawn.carryTracker.CarriedThing != null && pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Direct, out pawnFlyer.carriedThing))
			{
				if (pawnFlyer.carriedThing.holdingOwner != null)
				{
					pawnFlyer.carriedThing.holdingOwner.Remove(pawnFlyer.carriedThing);
				}
				pawnFlyer.carriedThing.DeSpawn();
			}
			pawn.DeSpawn(DestroyMode.WillReplace);
			if (!pawnFlyer.innerContainer.TryAdd(pawn))
			{
				Log.Error("Could not add " + pawn.ToStringSafe() + " to a flyer.");
				pawn.Destroy();
			}
			if (pawnFlyer.carriedThing != null && !pawnFlyer.innerContainer.TryAdd(pawnFlyer.carriedThing))
			{
				Log.Error("Could not add " + pawnFlyer.carriedThing.ToStringSafe() + " to a flyer.");
			}
			return pawnFlyer;
		}

		protected virtual bool ValidateFlyer()
		{
			return true;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
			Scribe_Values.Look(ref startVec, "startVec");
			Scribe_Values.Look(ref flightDistance, "flightDistance", 0f);
			Scribe_Values.Look(ref pawnWasDrafted, "pawnWasDrafted", defaultValue: false);
			Scribe_Values.Look(ref pawnCanFireAtWill, "pawnCanFireAtWill", defaultValue: true);
			Scribe_Values.Look(ref ticksFlightTime, "ticksFlightTime", 0);
			Scribe_Values.Look(ref ticksFlying, "ticksFlying", 0);
			Scribe_Deep.Look(ref jobQueue, "jobQueue");
			Scribe_Defs.Look(ref flightEffecterDef, "flightEffecterDef");
			Scribe_Defs.Look(ref soundLanding, "soundLanding");
			Scribe_References.Look(ref carriedThing, "carriedThing");
		}
	}
}
