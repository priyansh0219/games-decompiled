using Verse;

namespace RimWorld
{
	public class CompProperties_CauseHediff_AoE : CompProperties
	{
		public HediffDef hediff;

		public BodyPartDef part;

		public float range;

		public bool onlyTargetMechs;

		public int checkInterval = 10;

		public SoundDef activeSound;

		public CompProperties_CauseHediff_AoE()
		{
			compClass = typeof(CompCauseHediff_AoE);
		}
	}
}
