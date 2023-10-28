using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Researcher : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				if (Find.ResearchManager.currentProj == null)
				{
					return ThingRequest.ForGroup(ThingRequestGroup.Nothing);
				}
				return ThingRequest.ForGroup(ThingRequestGroup.ResearchBench);
			}
		}

		public override bool Prioritized => true;

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			if (Find.ResearchManager.currentProj == null)
			{
				return true;
			}
			return false;
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
			if (currentProj == null)
			{
				return false;
			}
			if (!(t is Building_ResearchBench bench))
			{
				return false;
			}
			if (!currentProj.CanBeResearchedAt(bench, ignoreResearchBenchPowerStatus: false))
			{
				return false;
			}
			if (!pawn.CanReserve(t, 1, -1, null, forced) || (t.def.hasInteractionCell && !pawn.CanReserveSittableOrSpot(t.InteractionCell, forced)))
			{
				return false;
			}
			if (!new HistoryEvent(HistoryEventDefOf.Researching, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
			{
				return false;
			}
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return JobMaker.MakeJob(JobDefOf.Research, t);
		}

		public override float GetPriority(Pawn pawn, TargetInfo t)
		{
			return t.Thing.GetStatValue(StatDefOf.ResearchSpeedFactor);
		}
	}
}
