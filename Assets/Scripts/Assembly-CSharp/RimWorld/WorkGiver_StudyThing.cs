using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_StudyThing : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Studiable);

		public override bool Prioritized => true;

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			CompStudiable compStudiable = t.TryGetComp<CompStudiable>();
			if (compStudiable == null || compStudiable.Completed)
			{
				return false;
			}
			if (!pawn.CanReserve(t, 1, -1, null, forced) || (t.def.hasInteractionCell && !pawn.CanReserveSittableOrSpot(t.InteractionCell, forced)))
			{
				return false;
			}
			if (t.IsForbidden(pawn))
			{
				return false;
			}
			if (!new HistoryEvent(HistoryEventDefOf.Researching, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
			{
				return false;
			}
			if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Study) == null)
			{
				return false;
			}
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return JobMaker.MakeJob(JobDefOf.StudyBuilding, t);
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.Study);
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Studiable);
		}
	}
}
