using Verse;

namespace RimWorld
{
	public class ThoughtWorker_ToxicFallout : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.MapHeld != null && !p.PositionHeld.Roofed(p.MapHeld) && p.MapHeld.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout))
			{
				return ThoughtState.ActiveDefault;
			}
			return ThoughtState.Inactive;
		}
	}
}
