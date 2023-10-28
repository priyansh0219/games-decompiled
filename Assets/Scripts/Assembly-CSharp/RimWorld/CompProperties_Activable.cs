using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompProperties_Activable : CompProperties
	{
		public int cooldownTicks;

		public int activeTicks;

		public int ticksToActivate;

		[MustTranslate]
		public string jobString;

		[MustTranslate]
		public string onCooldownString;

		[NoTranslate]
		public string activateTexPath;

		public FleckDef cooldownFleck;

		public int cooldownFleckSpawnIntervalTicks;

		public float cooldownFleckScale = 1f;

		public bool cooldownPreventsRefuel;

		public TargetingParameters targetingParameters;

		public SoundDef soundActivate;

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string item in base.ConfigErrors(parentDef))
			{
				yield return item;
			}
			if (compClass != null && !typeof(CompActivable).IsAssignableFrom(compClass))
			{
				yield return parentDef.defName + " has compClass but is not subclass of CompActivable.";
			}
			if (activateTexPath.NullOrEmpty())
			{
				yield return parentDef.defName + " has no activate texture.";
			}
		}
	}
}
