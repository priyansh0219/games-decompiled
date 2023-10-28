using Verse;

namespace RimWorld
{
	public class WorkGiver_CarryToSubcoreScanner : WorkGiver_CarryToBuilding
	{
		public override ThingRequest ThingRequest => ThingRequest.ForGroup(ThingRequestGroup.SubcoreScanner);

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			if (!base.ShouldSkip(pawn, forced))
			{
				return !ModsConfig.BiotechActive;
			}
			return true;
		}
	}
}
