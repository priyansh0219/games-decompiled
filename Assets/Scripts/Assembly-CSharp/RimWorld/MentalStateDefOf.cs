using Verse;

namespace RimWorld
{
	[DefOf]
	public static class MentalStateDefOf
	{
		public static MentalStateDef Berserk;

		public static MentalStateDef Binging_DrugExtreme;

		public static MentalStateDef BerserkMechanoid;

		[MayRequireBiotech]
		public static MentalStateDef CocoonDisturbed;

		[MayRequireBiotech]
		public static MentalStateDef BerserkWarcall;

		public static MentalStateDef Wander_Psychotic;

		public static MentalStateDef Binging_DrugMajor;

		public static MentalStateDef Wander_Sad;

		public static MentalStateDef Wander_OwnRoom;

		public static MentalStateDef PanicFlee;

		public static MentalStateDef Manhunter;

		public static MentalStateDef ManhunterPermanent;

		public static MentalStateDef SocialFighting;

		public static MentalStateDef Roaming;

		static MentalStateDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(MentalStateDefOf));
		}
	}
}
