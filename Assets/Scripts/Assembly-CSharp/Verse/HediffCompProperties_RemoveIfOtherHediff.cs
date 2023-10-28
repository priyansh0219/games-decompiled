namespace Verse
{
	public class HediffCompProperties_RemoveIfOtherHediff : HediffCompProperties_MessageBase
	{
		public HediffDef otherHediff;

		public IntRange? stages;

		public int mtbHours;

		public HediffCompProperties_RemoveIfOtherHediff()
		{
			compClass = typeof(HediffComp_RemoveIfOtherHediff);
		}
	}
}
