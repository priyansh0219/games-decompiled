using Verse;

namespace RimWorld
{
	public class CompProperties_MechPowerCell : CompProperties
	{
		public int totalPowerTicks = 2500;

		public CompProperties_MechPowerCell()
		{
			compClass = typeof(CompMechPowerCell);
		}
	}
}
