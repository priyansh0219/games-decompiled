namespace RimWorld
{
	[DefOf]
	public static class SitePartDefOf
	{
		public static SitePartDef Outpost;

		public static SitePartDef Turrets;

		public static SitePartDef Manhunters;

		public static SitePartDef SleepingMechanoids;

		public static SitePartDef AmbushHidden;

		public static SitePartDef AmbushEdge;

		public static SitePartDef PreciousLump;

		public static SitePartDef PossibleUnknownThreatMarker;

		public static SitePartDef BanditCamp;

		[MayRequireIdeology]
		public static SitePartDef WorshippedTerminal;

		[MayRequireIdeology]
		public static SitePartDef AncientComplex;

		[MayRequireIdeology]
		public static SitePartDef AncientAltar;

		[MayRequireIdeology]
		public static SitePartDef Archonexus;

		[MayRequireBiotech]
		public static SitePartDef AncientComplex_Mechanitor;

		static SitePartDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(SitePartDefOf));
		}
	}
}
