namespace RimWorld
{
	[DefOf]
	public static class AbilityDefOf
	{
		[MayRequireRoyalty]
		public static AbilityDef Speech;

		[MayRequireBiotech]
		public static AbilityDef ReimplantXenogerm;

		[MayRequireBiotech]
		public static AbilityDef ResurrectionMech;

		[MayRequireBiotech]
		public static AbilityDef Bloodfeed;

		static AbilityDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(AbilityDefOf));
		}
	}
}
