namespace RimWorld
{
	[DefOf]
	public static class ComplexDefOf
	{
		[MayRequireIdeology]
		public static ComplexDef AncientComplex;

		[MayRequireIdeology]
		public static ComplexDef AncientComplex_Loot;

		[MayRequireBiotech]
		public static ComplexDef AncientComplex_Mechanitor_Loot;

		static ComplexDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ComplexDefOf));
		}
	}
}
