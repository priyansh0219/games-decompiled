using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RitualObligationTargetWorker_AnyGatherSpotOrAltar : RitualObligationTargetWorker_AnyGatherSpot
	{
		public RitualObligationTargetWorker_AnyGatherSpotOrAltar()
		{
		}

		public RitualObligationTargetWorker_AnyGatherSpotOrAltar(RitualObligationTargetFilterDef def)
			: base(def)
		{
		}

		public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
		{
			if (!ModLister.CheckIdeology("Altar target"))
			{
				yield break;
			}
			List<Thing> partySpot = map.listerThings.ThingsOfDef(ThingDefOf.PartySpot);
			for (int k = 0; k < partySpot.Count; k++)
			{
				yield return partySpot[k];
			}
			List<Thing> ritualSpots = map.listerThings.ThingsOfDef(ThingDefOf.RitualSpot);
			for (int k = 0; k < ritualSpots.Count; k++)
			{
				yield return ritualSpots[k];
			}
			for (int k = 0; k < map.gatherSpotLister.activeSpots.Count; k++)
			{
				yield return map.gatherSpotLister.activeSpots[k].parent;
			}
			foreach (TargetInfo item in RitualObligationTargetWorker_Altar.GetTargetsWorker(obligation, map, parent.ideo))
			{
				yield return item;
			}
		}

		protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
		{
			if (!target.HasThing)
			{
				return false;
			}
			Thing thing = target.Thing;
			if (def.colonistThingsOnly && (thing.Faction == null || !thing.Faction.IsPlayer))
			{
				return false;
			}
			if (thing.def == ThingDefOf.PartySpot)
			{
				return true;
			}
			if (thing.def == ThingDefOf.RitualSpot)
			{
				return true;
			}
			CompGatherSpot compGatherSpot = thing.TryGetComp<CompGatherSpot>();
			if (compGatherSpot != null && compGatherSpot.Active)
			{
				return true;
			}
			return RitualObligationTargetWorker_Altar.CanUseTargetWorker(target, obligation, parent.ideo);
		}

		public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
		{
			yield return "RitualTargetGatherSpotInfo".Translate();
			foreach (string item in RitualObligationTargetWorker_Altar.GetTargetInfosWorker(parent.ideo))
			{
				yield return item;
			}
			yield return ThingDefOf.RitualSpot.LabelCap;
		}
	}
}
