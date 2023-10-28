namespace RimWorld
{
	[DefOf]
	public static class PrisonerInteractionModeDefOf
	{
		public static PrisonerInteractionModeDef NoInteraction;

		public static PrisonerInteractionModeDef AttemptRecruit;

		public static PrisonerInteractionModeDef ReduceResistance;

		public static PrisonerInteractionModeDef Release;

		public static PrisonerInteractionModeDef Execution;

		[MayRequireIdeology]
		public static PrisonerInteractionModeDef Enslave;

		[MayRequireIdeology]
		public static PrisonerInteractionModeDef ReduceWill;

		[MayRequireIdeology]
		public static PrisonerInteractionModeDef Convert;

		[MayRequireBiotech]
		public static PrisonerInteractionModeDef Bloodfeed;

		[MayRequireBiotech]
		public static PrisonerInteractionModeDef HemogenFarm;

		static PrisonerInteractionModeDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PrisonerInteractionModeDefOf));
		}
	}
}
