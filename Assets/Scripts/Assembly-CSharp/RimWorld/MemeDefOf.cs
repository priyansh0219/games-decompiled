namespace RimWorld
{
	[DefOf]
	public static class MemeDefOf
	{
		[MayRequireIdeology]
		public static MemeDef Nudism;

		[MayRequireIdeology]
		public static MemeDef MaleSupremacy;

		[MayRequireIdeology]
		public static MemeDef FemaleSupremacy;

		[MayRequireIdeology]
		public static MemeDef Rancher;

		[MayRequireIdeology]
		public static MemeDef TreeConnection;

		[MayRequireIdeology]
		public static MemeDef Blindsight;

		[MayRequireIdeology]
		public static MemeDef Transhumanist;

		[MayRequireIdeology]
		public static MemeDef Tunneler;

		[MayRequireIdeology]
		public static MemeDef Darkness;

		[MayRequireIdeology]
		public static MemeDef Structure_Ideological;

		static MemeDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(MemeDefOf));
		}
	}
}
