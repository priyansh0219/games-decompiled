namespace RimWorld
{
	public class CompProperties_AbilityFireSpew : CompProperties_AbilityEffect
	{
		public float range;

		public float lineWidthEnd;

		public CompProperties_AbilityFireSpew()
		{
			compClass = typeof(CompAbilityEffect_FireSpew);
		}
	}
}
