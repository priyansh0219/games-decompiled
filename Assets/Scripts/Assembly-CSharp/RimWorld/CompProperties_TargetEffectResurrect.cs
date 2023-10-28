using Verse;

namespace RimWorld
{
	public class CompProperties_TargetEffectResurrect : CompProperties
	{
		public ThingDef moteDef;

		public CompProperties_TargetEffectResurrect()
		{
			compClass = typeof(CompTargetEffect_Resurrect);
		}
	}
}
