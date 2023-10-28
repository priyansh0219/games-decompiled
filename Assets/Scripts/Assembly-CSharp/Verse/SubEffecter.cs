using UnityEngine;

namespace Verse
{
	public class SubEffecter
	{
		public Effecter parent;

		public SubEffecterDef def;

		public Color? colorOverride;

		public Color EffectiveColor => colorOverride ?? def.color;

		public SubEffecter(SubEffecterDef subDef, Effecter parent)
		{
			def = subDef;
			this.parent = parent;
		}

		public virtual void SubEffectTick(TargetInfo A, TargetInfo B)
		{
		}

		public virtual void SubTrigger(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1)
		{
		}

		public virtual void SubCleanup()
		{
		}
	}
}
