using Verse;
using Verse.AI;

namespace RimWorld
{
	public class CompUseableDatacore : CompUsable
	{
		public override LocalTargetInfo GetExtraTarget(Pawn pawn)
		{
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.ResearchBench), PathEndMode.InteractionCell, TraverseParms.For(pawn, Danger.Some), 9999f, (Thing thing) => pawn.CanReserve(thing));
		}

		public override bool CanBeUsedBy(Pawn p, out string failReason)
		{
			if (!base.CanBeUsedBy(p, out failReason))
			{
				return false;
			}
			if (!GetExtraTarget(p).HasThing)
			{
				failReason = "NoResearchBench".Translate();
				return false;
			}
			failReason = null;
			return true;
		}

		public override void UsedBy(Pawn p)
		{
			base.UsedBy(p);
			Find.History.Notify_MechanoidDatacoreReadOrLost();
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			base.PostDestroy(mode, previousMap);
			Find.History.Notify_MechanoidDatacoreReadOrLost();
		}

		public override void Notify_AbandonedAtTile(int tile)
		{
			base.Notify_AbandonedAtTile(tile);
			Find.History.Notify_MechanoidDatacoreReadOrLost();
		}
	}
}
