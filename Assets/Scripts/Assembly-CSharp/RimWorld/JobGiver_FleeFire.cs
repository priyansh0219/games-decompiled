using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_FleeFire : ThinkNode_JobGiver
	{
		private const int MinFiresNearbyRadius = 20;

		private const int MinFiresNearbyRegionsToScan = 18;

		private const int DistToFireToFlee = 20;

		protected override Job TryGiveJob(Pawn pawn)
		{
			pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Fire);
			TraverseParms tp = TraverseParms.For(pawn);
			Thing closestFire = null;
			float closestDistSq = -1f;
			RegionTraverser.BreadthFirstTraverse(pawn.Position, pawn.Map, (Region from, Region to) => to.Allows(tp, isDestination: false), delegate(Region x)
			{
				List<Thing> list = x.ListerThings.ThingsInGroup(ThingRequestGroup.Fire);
				for (int i = 0; i < list.Count; i++)
				{
					float num = pawn.Position.DistanceToSquared(list[i].Position);
					if (!(num > 400f) && (closestFire == null || num < closestDistSq))
					{
						closestDistSq = num;
						closestFire = list[i];
					}
				}
				return closestDistSq <= 400f;
			}, 18);
			if (closestFire != null && closestDistSq <= 400f)
			{
				Job job = JobGiver_AnimalFlee.FleeJob(pawn, closestFire);
				if (job != null)
				{
					return job;
				}
			}
			return null;
		}
	}
}
