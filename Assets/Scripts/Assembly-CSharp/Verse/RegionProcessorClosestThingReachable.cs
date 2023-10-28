using System;
using System.Collections.Generic;
using RimWorld;
using Verse.AI;

namespace Verse
{
	public class RegionProcessorClosestThingReachable : RegionProcessorDelegateCache
	{
		private TraverseParms traverseParams;

		private float maxDistance;

		private IntVec3 root;

		public Thing closestThing;

		public int regionsSeenScan;

		private bool ignoreEntirelyForbiddenRegions;

		private ThingRequest req;

		private PathEndMode peMode;

		private Func<Thing, float> priorityGetter;

		private Predicate<Thing> validator;

		private float bestPrio;

		private float closestDistSquared;

		private int minRegions;

		private float maxDistSquared;

		public void SetParameters(TraverseParms traverseParams, float maxDistance, IntVec3 root, bool ignoreEntirelyForbiddenRegions, ThingRequest req, PathEndMode peMode, Func<Thing, float> priorityGetter, Predicate<Thing> validator, int minRegions, float closestDistSquared = 9999999f, int regionsSeenScan = 0, float bestPrio = float.MinValue, Thing closestThing = null)
		{
			this.traverseParams = traverseParams;
			this.maxDistance = maxDistance;
			this.root = root;
			this.regionsSeenScan = regionsSeenScan;
			this.ignoreEntirelyForbiddenRegions = ignoreEntirelyForbiddenRegions;
			this.req = req;
			this.peMode = peMode;
			this.priorityGetter = priorityGetter;
			this.validator = validator;
			this.bestPrio = bestPrio;
			this.closestDistSquared = closestDistSquared;
			this.closestThing = closestThing;
			this.minRegions = minRegions;
			maxDistSquared = maxDistance * maxDistance;
		}

		public void Clear()
		{
			SetParameters(default(TraverseParms), 0f, default(IntVec3), ignoreEntirelyForbiddenRegions: false, default(ThingRequest), PathEndMode.None, null, null, 0, 0f, 0, 0f);
		}

		protected override bool RegionEntryPredicate(Region from, Region to)
		{
			if (!to.Allows(traverseParams, isDestination: false))
			{
				return false;
			}
			if (!(maxDistance > 5000f))
			{
				return to.extentsClose.ClosestDistSquaredTo(root) < maxDistSquared;
			}
			return true;
		}

		protected override bool RegionProcessor(Region reg)
		{
			if (RegionTraverser.ShouldCountRegion(reg))
			{
				regionsSeenScan++;
			}
			if (!reg.IsDoorway && !reg.Allows(traverseParams, isDestination: true))
			{
				return false;
			}
			if (!ignoreEntirelyForbiddenRegions || !reg.IsForbiddenEntirely(traverseParams.pawn))
			{
				List<Thing> list = reg.ListerThings.ThingsMatching(req);
				for (int i = 0; i < list.Count; i++)
				{
					Thing thing = list[i];
					if (!ReachabilityWithinRegion.ThingFromRegionListerReachable(thing, reg, peMode, traverseParams.pawn))
					{
						continue;
					}
					float num = ((priorityGetter != null) ? priorityGetter(thing) : 0f);
					if (!(num < bestPrio))
					{
						float num2 = (thing.Position - root).LengthHorizontalSquared;
						if ((num > bestPrio || num2 < closestDistSquared) && num2 < maxDistSquared && (validator == null || validator(thing)))
						{
							closestThing = thing;
							closestDistSquared = num2;
							bestPrio = num;
						}
					}
				}
			}
			if (regionsSeenScan >= minRegions)
			{
				return closestThing != null;
			}
			return false;
		}
	}
}
