namespace RimWorld
{
	[DefOf]
	public static class ExpansionDefOf
	{
		public static ExpansionDef Core;

		static ExpansionDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ExpansionDefOf));
		}
	}
}
