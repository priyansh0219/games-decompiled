using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_PatientGoToBed : ThinkNode_JobGiver
	{
		public bool respectTimetable = true;

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!HealthAIUtility.ShouldSeekMedicalRest(pawn))
			{
				return null;
			}
			if (respectTimetable && RestUtility.TimetablePreventsLayDown(pawn) && !HealthAIUtility.ShouldHaveSurgeryDoneNow(pawn) && !HealthAIUtility.ShouldBeTendedNowByPlayer(pawn))
			{
				return null;
			}
			if (RestUtility.DisturbancePreventsLyingDown(pawn))
			{
				return null;
			}
			Thing thing = RestUtility.FindBedFor(pawn, pawn, checkSocialProperness: false);
			if (thing == null)
			{
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.LayDown, thing);
		}

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_PatientGoToBed obj = (JobGiver_PatientGoToBed)base.DeepCopy(resolve);
			obj.respectTimetable = respectTimetable;
			return obj;
		}
	}
}
