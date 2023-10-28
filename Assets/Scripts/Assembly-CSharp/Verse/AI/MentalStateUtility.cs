using RimWorld;

namespace Verse.AI
{
	public static class MentalStateUtility
	{
		public static MentalStateDef GetWanderToOwnRoomStateOrFallback(Pawn pawn)
		{
			if (MentalStateDefOf.Wander_OwnRoom.Worker.StateCanOccur(pawn))
			{
				return MentalStateDefOf.Wander_OwnRoom;
			}
			if (MentalStateDefOf.Wander_Sad.Worker.StateCanOccur(pawn))
			{
				return MentalStateDefOf.Wander_Sad;
			}
			return null;
		}

		public static void TryTransitionToWanderOwnRoom(MentalState mentalState)
		{
			MentalStateDef wanderToOwnRoomStateOrFallback = GetWanderToOwnRoomStateOrFallback(mentalState.pawn);
			if (wanderToOwnRoomStateOrFallback != null)
			{
				mentalState.pawn.mindState.mentalStateHandler.TryStartMentalState(wanderToOwnRoomStateOrFallback, null, forceWake: false, mentalState.causedByMood, null, transitionSilently: true);
			}
			else
			{
				mentalState.RecoverFromState();
			}
		}
	}
}
