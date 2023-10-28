namespace RimWorld
{
	[DefOf]
	public static class PawnGroupKindDefOf
	{
		public static PawnGroupKindDef Combat;

		public static PawnGroupKindDef Trader;

		public static PawnGroupKindDef Peaceful;

		public static PawnGroupKindDef Settlement;

		public static PawnGroupKindDef Settlement_RangedOnly;

		[MayRequireIdeology]
		public static PawnGroupKindDef Miners;

		[MayRequireIdeology]
		public static PawnGroupKindDef Farmers;

		[MayRequireIdeology]
		public static PawnGroupKindDef Loggers;

		[MayRequireIdeology]
		public static PawnGroupKindDef Hunters;

		static PawnGroupKindDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PawnGroupKindDefOf));
		}
	}
}
