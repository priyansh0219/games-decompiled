using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class WorkGiver_CarryToBuilding : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

		public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

		public abstract ThingRequest ThingRequest { get; }

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			if (!base.ShouldSkip(pawn, forced))
			{
				return !pawn.RaceProps.Humanlike;
			}
			return true;
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t.IsForbidden(pawn) || !pawn.CanReserve(t, 1, -1, null, forced))
			{
				return false;
			}
			Pawn pawn2 = (Pawn)t;
			if (pawn2.IsPrisonerOfColony || !pawn2.health.capacities.CapableOf(PawnCapacityDefOf.Moving) || (def.workType != null && pawn2.WorkTypeIsDisabled(def.workType)))
			{
				return FindBuildingFor(pawn2, pawn, forced) != null;
			}
			return false;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = (Pawn)t;
			Building building = FindBuildingFor(pawn2, pawn, forced);
			if (building == null)
			{
				return null;
			}
			Job job = JobMaker.MakeJob(JobDefOf.CarryToBuilding, building, pawn2);
			job.count = 1;
			return job;
		}

		private Building FindBuildingFor(Pawn pawn, Pawn traveller, bool forced)
		{
			return (Building)GenClosest.ClosestThingReachable(traveller.Position, traveller.Map, ThingRequest, PathEndMode.InteractionCell, TraverseParms.For(traveller), 9999f, CanUse);
			bool CanUse(Thing thing)
			{
				if (!(thing is Building_Enterable building_Enterable))
				{
					return false;
				}
				if (!ThingRequest.Accepts(thing))
				{
					return false;
				}
				if (thing.IsForbidden(traveller) || !pawn.CanReserve(thing, 1, -1, null, forced))
				{
					return false;
				}
				if (traveller.Map.designationManager.DesignationOn(thing, DesignationDefOf.Deconstruct) != null)
				{
					return false;
				}
				if (building_Enterable.SelectedPawn != pawn)
				{
					return false;
				}
				if (!building_Enterable.CanAcceptPawn(pawn))
				{
					return false;
				}
				return true;
			}
		}
	}
}
