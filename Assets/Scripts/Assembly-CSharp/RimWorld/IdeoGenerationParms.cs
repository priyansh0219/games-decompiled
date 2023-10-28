using System.Collections.Generic;

namespace RimWorld
{
	public struct IdeoGenerationParms
	{
		public FactionDef forFaction;

		public bool forceNoExpansionIdeo;

		public bool classic;

		public List<PreceptDef> disallowedPrecepts;

		public List<MemeDef> disallowedMemes;

		public List<MemeDef> forcedMemes;

		public bool forceNoWeaponPreference;

		public bool forNewFluidIdeo;

		public IdeoGenerationParms(FactionDef forFaction, bool forceNoExpansionIdeo = false, List<PreceptDef> disallowedPrecepts = null, List<MemeDef> disallowedMemes = null, List<MemeDef> forcedMemes = null, bool classic = false, bool forceNoWeaponPreference = false, bool forNewFluidIdeo = false)
		{
			this.forFaction = forFaction;
			this.forceNoExpansionIdeo = forceNoExpansionIdeo;
			this.disallowedPrecepts = disallowedPrecepts;
			this.disallowedMemes = disallowedMemes;
			this.forcedMemes = forcedMemes;
			this.classic = classic;
			this.forceNoWeaponPreference = forceNoWeaponPreference;
			this.forNewFluidIdeo = forNewFluidIdeo;
		}
	}
}
