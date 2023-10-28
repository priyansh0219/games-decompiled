namespace RimWorld
{
	[DefOf]
	public static class LearningDesireDefOf
	{
		[MayRequireBiotech]
		public static LearningDesireDef Lessontaking;

		[MayRequireBiotech]
		public static LearningDesireDef Workwatching;

		[MayRequireBiotech]
		public static LearningDesireDef NatureRunning;

		[MayRequireBiotech]
		public static LearningDesireDef Floordrawing;

		[MayRequireBiotech]
		public static LearningDesireDef Skydreaming;

		[MayRequireBiotech]
		public static LearningDesireDef Radiotalking;

		static LearningDesireDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(LearningDesireDefOf));
		}
	}
}
