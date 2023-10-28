using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class EffecterMaintainer
	{
		private Map map;

		private List<Tuple<Effecter, TargetInfo>> maintainedEffecters = new List<Tuple<Effecter, TargetInfo>>();

		public EffecterMaintainer(Map map)
		{
			this.map = map;
		}

		public void AddEffecterToMaintain(Effecter eff, IntVec3 pos, int ticks)
		{
			eff.ticksLeft = ticks;
			TargetInfo item = new TargetInfo(pos, map);
			maintainedEffecters.Add(new Tuple<Effecter, TargetInfo>(eff, item));
		}

		public void AddEffecterToMaintain(Effecter eff, Thing target, int ticks)
		{
			eff.ticksLeft = ticks;
			maintainedEffecters.Add(new Tuple<Effecter, TargetInfo>(eff, target));
		}

		public void EffecterMaintainerTick()
		{
			for (int num = maintainedEffecters.Count - 1; num >= 0; num--)
			{
				Tuple<Effecter, TargetInfo> tuple = maintainedEffecters[num];
				if (tuple.Item1.ticksLeft > 0)
				{
					tuple.Item1.EffectTick(tuple.Item2, TargetInfo.Invalid);
					tuple.Item1.ticksLeft--;
				}
				else
				{
					tuple.Item1.Cleanup();
					maintainedEffecters.RemoveAt(num);
				}
			}
		}
	}
}
