namespace RimWorld
{
	[DefOf]
	public static class ComplexThreatDefOf
	{
		[MayRequireIdeology]
		public static ComplexThreatDef SleepingInsects;

		[MayRequireIdeology]
		public static ComplexThreatDef Infestation;

		[MayRequireIdeology]
		public static ComplexThreatDef SleepingMechanoids;

		[MayRequireIdeology]
		public static ComplexThreatDef CryptosleepPods;

		[MayRequireIdeology]
		public static ComplexThreatDef MechDrop;

		[MayRequireIdeology]
		public static ComplexThreatDef FuelNode;

		static ComplexThreatDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ComplexThreatDefOf));
		}
	}
}
