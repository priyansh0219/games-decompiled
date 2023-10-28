using Verse;

namespace RimWorld
{
	public class TraitMentalStateGiver
	{
		public TraitDegreeData traitDegreeData;

		public virtual bool CheckGive(Pawn pawn, int checkInterval)
		{
			if (traitDegreeData.forcedMentalState != null)
			{
				float forcedMentalStateMtbDays = traitDegreeData.forcedMentalStateMtbDays;
				if (forcedMentalStateMtbDays > 0f && Rand.MTBEventOccurs(forcedMentalStateMtbDays, 60000f, checkInterval) && traitDegreeData.forcedMentalState.Worker.StateCanOccur(pawn))
				{
					return pawn.mindState.mentalStateHandler.TryStartMentalState(traitDegreeData.forcedMentalState, "MentalStateReason_Trait".Translate(traitDegreeData.label));
				}
			}
			if (traitDegreeData.randomMentalState == null)
			{
				return false;
			}
			float curMood = pawn.mindState.mentalBreaker.CurMood;
			if (Rand.MTBEventOccurs(traitDegreeData.randomMentalStateMtbDaysMoodCurve.Evaluate(curMood), 60000f, checkInterval) && traitDegreeData.randomMentalState.Worker.StateCanOccur(pawn))
			{
				return pawn.mindState.mentalStateHandler.TryStartMentalState(traitDegreeData.randomMentalState, "MentalStateReason_Trait".Translate(traitDegreeData.GetLabelFor(pawn)));
			}
			return false;
		}
	}
}
