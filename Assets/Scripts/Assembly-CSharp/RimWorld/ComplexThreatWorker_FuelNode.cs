using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ComplexThreatWorker_FuelNode : ComplexThreatWorker
	{
		private const string TriggerStartWickAction = "TriggerStartWickAction";

		private const string CompletedStartWickAction = "CompletedStartWickAction";

		private static readonly FloatRange ExplosiveRadiusRandomRange = new FloatRange(2f, 12f);

		private const float ExplosiveRadiusThreatPointsFactor = 10f;

		private const float RoomEntryTriggerChance = 0.25f;

		protected override bool CanResolveInt(ComplexResolveParams parms)
		{
			IntVec3 spawnPosition;
			if (base.CanResolveInt(parms))
			{
				return ComplexUtility.TryFindRandomSpawnCell(ThingDefOf.AncientFuelNode, parms.room.SelectMany((CellRect r) => r.Cells), parms.map, out spawnPosition);
			}
			return false;
		}

		protected override void ResolveInt(ComplexResolveParams parms, ref float threatPointsUsed, List<Thing> outSpawnedThings)
		{
			ComplexUtility.TryFindRandomSpawnCell(ThingDefOf.AncientFuelNode, parms.room.SelectMany((CellRect r) => r.Cells), parms.map, out var spawnPosition);
			Thing thing = GenSpawn.Spawn(ThingDefOf.AncientFuelNode, spawnPosition, parms.map);
			SignalAction_StartWick signalAction_StartWick = (SignalAction_StartWick)ThingMaker.MakeThing(ThingDefOf.SignalAction_StartWick);
			signalAction_StartWick.thingWithWick = thing;
			signalAction_StartWick.signalTag = parms.triggerSignal;
			signalAction_StartWick.completedSignalTag = "CompletedStartWickAction" + Find.UniqueIDsManager.GetNextSignalTagID();
			if (parms.delayTicks.HasValue)
			{
				signalAction_StartWick.delayTicks = parms.delayTicks.Value;
				SignalAction_Message obj = (SignalAction_Message)ThingMaker.MakeThing(ThingDefOf.SignalAction_Message);
				obj.signalTag = parms.triggerSignal;
				obj.lookTargets = thing;
				obj.messageType = MessageTypeDefOf.ThreatBig;
				obj.message = "MessageFuelNodeDelayActivated".Translate(ThingDefOf.AncientFuelNode.label);
				GenSpawn.Spawn(obj, parms.room[0].CenterCell, parms.map);
			}
			GenSpawn.Spawn(signalAction_StartWick, parms.room[0].CenterCell, parms.map);
			CompExplosive compExplosive = thing.TryGetComp<CompExplosive>();
			float randomInRange = ExplosiveRadiusRandomRange.RandomInRange;
			compExplosive.customExplosiveRadius = randomInRange;
			SignalAction_Message obj2 = (SignalAction_Message)ThingMaker.MakeThing(ThingDefOf.SignalAction_Message);
			obj2.message = "MessageFuelNodeTriggered".Translate(thing.LabelShort);
			obj2.messageType = MessageTypeDefOf.NegativeEvent;
			obj2.lookTargets = thing;
			obj2.signalTag = signalAction_StartWick.completedSignalTag;
			GenSpawn.Spawn(obj2, parms.room[0].CenterCell, parms.map);
			threatPointsUsed = randomInRange * 10f;
		}
	}
}
