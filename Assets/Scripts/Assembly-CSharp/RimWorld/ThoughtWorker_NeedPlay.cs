using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_NeedPlay : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.needs.play == null)
			{
				return ThoughtState.Inactive;
			}
			switch (p.needs.play.CurCategory)
			{
			case PlayCategory.Empty:
				return ThoughtState.ActiveAtStage(0);
			case PlayCategory.VeryLow:
				return ThoughtState.ActiveAtStage(1);
			case PlayCategory.Low:
				return ThoughtState.ActiveAtStage(2);
			case PlayCategory.Satisfied:
				return ThoughtState.Inactive;
			case PlayCategory.High:
				return ThoughtState.ActiveAtStage(3);
			case PlayCategory.Extreme:
				return ThoughtState.ActiveAtStage(4);
			default:
				throw new NotImplementedException();
			}
		}
	}
}
