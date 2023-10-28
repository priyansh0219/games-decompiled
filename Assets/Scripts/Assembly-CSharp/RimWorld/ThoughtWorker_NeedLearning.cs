using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_NeedLearning : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!ModLister.CheckBiotech("Learning"))
			{
				return ThoughtState.Inactive;
			}
			if (p.needs.learning == null)
			{
				return ThoughtState.Inactive;
			}
			switch (p.needs.learning.CurCategory)
			{
			case LearningCategory.Empty:
				return ThoughtState.ActiveAtStage(0);
			case LearningCategory.VeryLow:
				return ThoughtState.ActiveAtStage(1);
			case LearningCategory.Low:
				return ThoughtState.ActiveAtStage(2);
			case LearningCategory.Satisfied:
				return ThoughtState.Inactive;
			case LearningCategory.High:
				return ThoughtState.ActiveAtStage(3);
			case LearningCategory.Extreme:
				return ThoughtState.ActiveAtStage(4);
			default:
				throw new NotImplementedException();
			}
		}
	}
}
