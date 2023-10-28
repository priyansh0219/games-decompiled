using Verse;

namespace RimWorld
{
	[DefOf]
	public static class ResearchProjectDefOf
	{
		public static ResearchProjectDef CarpetMaking;

		public static ResearchProjectDef MicroelectronicsBasics;

		public static ResearchProjectDef Batteries;

		public static ResearchProjectDef ColoredLights;

		[MayRequireBiotech]
		public static ResearchProjectDef BasicMechtech;

		[MayRequireBiotech]
		public static ResearchProjectDef Archogenetics;

		static ResearchProjectDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ResearchProjectDefOf));
		}
	}
}
