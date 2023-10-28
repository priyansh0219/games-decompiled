using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class WorkGiver_FeedBabyManually : WorkGiver_Scanner
	{
		private static Dictionary<Pawn, Pawn> cachedBabyAutoBreastfeeder = new Dictionary<Pawn, Pawn>();

		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

		public override PathEndMode PathEndMode => PathEndMode.OnCell;

		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn.Map.mapPawns.SpawnedHungryPawns;
		}

		protected bool CanCreateManualFeedingJob(Pawn pawn, Thing t, bool forced)
		{
			Pawn baby = t as Pawn;
			if (!ChildcareUtility.CanSuckle(baby, out var reason))
			{
				return false;
			}
			if (ChildcareUtility.CanMomAutoBreastfeedBabyNow(pawn, baby, forced: false, out reason))
			{
				return false;
			}
			ChildcareUtility.BreastfeedFailReason? reason2 = null;
			if (!ChildcareUtility.CanSuckleNow(baby, out reason2))
			{
				if (forced)
				{
					JobFailReason.Is(reason2.Value.Translate(null, null, baby));
				}
				return false;
			}
			if (!ChildcareUtility.CanHaulBaby(pawn, baby, out reason))
			{
				return false;
			}
			if (!ChildcareUtility.CanHaulBabyNow(pawn, baby, forced, out reason2))
			{
				if (forced)
				{
					JobFailReason.Is(reason2.Value.Translate(pawn, null, baby));
				}
				return false;
			}
			if (!forced && !ChildcareUtility.WantsSuckle(baby, out reason))
			{
				return false;
			}
			if (!forced && AutoBreastfeederAvailable(baby, forced, out var _))
			{
				return false;
			}
			return true;
		}

		private bool AutoBreastfeederAvailable(Pawn baby, bool forced, out Pawn feeder)
		{
			Pawn pawn = cachedBabyAutoBreastfeeder.TryGetValue(baby);
			if (ChildcareUtility.CanMomAutoBreastfeedBabyNow(pawn, baby, forced, out var reason))
			{
				feeder = pawn;
				return true;
			}
			cachedBabyAutoBreastfeeder[baby] = null;
			foreach (Pawn item in ChildcareUtility.CanBreastfeedMothers(baby))
			{
				if (ChildcareUtility.CanMomAutoBreastfeedBabyNow(item, baby, forced, out reason))
				{
					cachedBabyAutoBreastfeeder[baby] = item;
					feeder = item;
					return true;
				}
			}
			feeder = null;
			return false;
		}
	}
}
