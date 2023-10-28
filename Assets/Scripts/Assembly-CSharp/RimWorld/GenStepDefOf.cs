using Verse;

namespace RimWorld
{
	[DefOf]
	public static class GenStepDefOf
	{
		public static GenStepDef PreciousLump;

		[MayRequireRoyalty]
		public static GenStepDef AnimaTrees;

		[MayRequireBiotech]
		public static GenStepDef PoluxTrees;

		static GenStepDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(GenStepDefOf));
		}
	}
}
