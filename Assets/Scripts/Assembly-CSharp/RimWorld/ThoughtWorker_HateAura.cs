using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_HateAura : ThoughtWorker
	{
		private enum HateAuraLevel
		{
			None = 0,
			Intense = 1,
			Strong = 2,
			Distant = 3
		}

		private const float IntenseDistance = 6.9f;

		private const float StrongDistance = 15.9f;

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			HateAuraLevel hateAuraLevel = HateAuraLevel.None;
			if (p.Map != null)
			{
				List<Thing> list = p.Map.listerThings.ThingsOfDef(ThingDefOf.Mech_Apocriton);
				list.SortBy((Thing m) => m.Position.DistanceToSquared(m.Position));
				if (list.Count > 0)
				{
					float num = list[0].Position.DistanceTo(p.Position);
					hateAuraLevel = ((num <= 6.9f) ? HateAuraLevel.Intense : ((!(num <= 15.9f)) ? HateAuraLevel.Distant : HateAuraLevel.Strong));
				}
			}
			switch (hateAuraLevel)
			{
			case HateAuraLevel.None:
				return false;
			case HateAuraLevel.Intense:
				return ThoughtState.ActiveAtStage(0);
			case HateAuraLevel.Strong:
				return ThoughtState.ActiveAtStage(1);
			case HateAuraLevel.Distant:
				return ThoughtState.ActiveAtStage(2);
			default:
				throw new NotImplementedException();
			}
		}
	}
}
