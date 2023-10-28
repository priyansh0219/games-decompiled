namespace Verse
{
	public class HediffCompProperties_GiveHediff : HediffCompProperties
	{
		public HediffDef hediffDef;

		public bool skipIfAlreadyExists;

		public HediffCompProperties_GiveHediff()
		{
			compClass = typeof(HediffComp_GiveHediff);
		}
	}
}
