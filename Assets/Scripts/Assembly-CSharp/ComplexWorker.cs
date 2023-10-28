using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;

public class ComplexWorker
{
	private static readonly FloatRange ThreatPointsFactorRange = new FloatRange(0.25f, 0.35f);

	private static SimpleCurve EntranceCountOverAreaCurve = new SimpleCurve
	{
		new CurvePoint(0f, 1f),
		new CurvePoint(1000f, 1f),
		new CurvePoint(1500f, 2f),
		new CurvePoint(5000f, 3f),
		new CurvePoint(10000f, 4f)
	};

	public ComplexDef def;

	private static List<CellRect> tmpRoomMapRects = new List<CellRect>();

	private static List<Thing> tmpSpawnedThreatThings = new List<Thing>();

	private static List<ComplexThreat> useableThreats = new List<ComplexThreat>();

	private const string ThreatTriggerSignal = "ThreatTriggerSignal";

	private static List<IntVec3> tmpSpawnLocations = new List<IntVec3>();

	public virtual Faction GetFixedHostileFactionForThreats()
	{
		return null;
	}

	public virtual ComplexSketch GenerateSketch(IntVec2 size, Faction faction = null)
	{
		Sketch sketch = new Sketch();
		ThingDef thingDef = BaseGenUtility.RandomCheapWallStuff(faction ?? Faction.OfAncients, notVeryFlammable: true);
		int entranceCount = GenMath.RoundRandom(EntranceCountOverAreaCurve.Evaluate(size.Area));
		ComplexLayout complexLayout = ComplexLayoutGenerator.GenerateRandomLayout(new CellRect(0, 0, size.x, size.z), 6, 6, 0.2f, null, entranceCount);
		ThingDef stuff = thingDef;
		for (int i = complexLayout.container.minX; i <= complexLayout.container.maxX; i++)
		{
			for (int j = complexLayout.container.minZ; j <= complexLayout.container.maxZ; j++)
			{
				IntVec3 intVec = new IntVec3(i, 0, j);
				int roomIdAt = complexLayout.GetRoomIdAt(intVec);
				if (complexLayout.IsWallAt(intVec))
				{
					sketch.AddThing(ThingDefOf.Wall, intVec, Rot4.North, (roomIdAt % 2 == 0) ? thingDef : ThingDefOf.Steel);
				}
				if (complexLayout.IsFloorAt(intVec) || complexLayout.IsDoorAt(intVec))
				{
					sketch.AddTerrain(TerrainDefOf.PavedTile, intVec);
				}
				if (complexLayout.IsDoorAt(intVec))
				{
					sketch.AddThing(ThingDefOf.Door, intVec, Rot4.North, stuff);
				}
			}
		}
		ComplexRoomParams roomParams = default(ComplexRoomParams);
		roomParams.sketch = sketch;
		if (!def.roomDefs.NullOrEmpty())
		{
			List<ComplexRoomDef> usedDefs = new List<ComplexRoomDef>();
			foreach (ComplexRoom room in complexLayout.Rooms)
			{
				roomParams.room = room;
				if (def.roomDefs.Where((ComplexRoomDef d) => d.CanResolve(roomParams) && usedDefs.Count((ComplexRoomDef ud) => ud == d) < d.maxCount).TryRandomElementByWeight((ComplexRoomDef d) => d.selectionWeight, out var result))
				{
					room.def = result;
					usedDefs.Add(room.def);
				}
			}
		}
		foreach (ComplexRoom room2 in complexLayout.Rooms)
		{
			if (room2.def != null)
			{
				roomParams.room = room2;
				room2.def.ResolveSketch(roomParams);
			}
		}
		return new ComplexSketch
		{
			structure = sketch,
			layout = complexLayout,
			complexDef = def
		};
	}

	public virtual void Spawn(ComplexSketch sketch, Map map, IntVec3 center, float? threatPoints = null, List<Thing> allSpawnedThings = null)
	{
		List<Thing> list = allSpawnedThings ?? new List<Thing>();
		sketch.structure.Spawn(map, center, null, Sketch.SpawnPosType.Unchanged, Sketch.SpawnMode.Normal, wipeIfCollides: true, clearEdificeWhereFloor: true, list, dormant: false, buildRoofsInstantly: true);
		List<List<CellRect>> list2 = new List<List<CellRect>>();
		List<ComplexRoom> rooms = sketch.layout.Rooms;
		for (int i = 0; i < rooms.Count; i++)
		{
			tmpRoomMapRects.Clear();
			for (int j = 0; j < rooms[i].rects.Count; j++)
			{
				tmpRoomMapRects.Add(rooms[i].rects[j].MovedBy(center));
			}
			List<CellRect> list3 = new List<CellRect>();
			for (int k = 0; k < tmpRoomMapRects.Count; k++)
			{
				CellRect item = LargestAreaFinder.ExpandRect(tmpRoomMapRects[k], map, new HashSet<IntVec3>(), (IntVec3 c) => CanExpand(c, map));
				list3.Add(item);
			}
			list2.Add(list3);
			tmpRoomMapRects.Clear();
		}
		if (!sketch.thingsToSpawn.NullOrEmpty())
		{
			HashSet<List<CellRect>> usedRooms = new HashSet<List<CellRect>>();
			foreach (Thing item2 in sketch.thingsToSpawn)
			{
				List<CellRect> roomUsed;
				Rot4 rotUsed;
				IntVec3 loc = FindBestSpawnLocation(list2, item2.def, map, out roomUsed, out rotUsed, usedRooms);
				if (!loc.IsValid)
				{
					loc = FindBestSpawnLocation(list2, item2.def, map, out roomUsed, out rotUsed);
				}
				if (!loc.IsValid)
				{
					item2.Destroy();
					continue;
				}
				GenSpawn.Spawn(item2, loc, map, rotUsed);
				list.Add(item2);
				if (sketch.thingDiscoveredMessage.NullOrEmpty())
				{
					continue;
				}
				string signalTag = "ThingDiscovered" + Find.UniqueIDsManager.GetNextSignalTagID();
				foreach (CellRect item3 in roomUsed)
				{
					RectTrigger obj = (RectTrigger)ThingMaker.MakeThing(ThingDefOf.RectTrigger);
					obj.signalTag = signalTag;
					obj.Rect = item3;
					GenSpawn.Spawn(obj, item3.CenterCell, map);
				}
				SignalAction_Message obj2 = (SignalAction_Message)ThingMaker.MakeThing(ThingDefOf.SignalAction_Message);
				obj2.signalTag = signalTag;
				obj2.message = sketch.thingDiscoveredMessage;
				obj2.messageType = MessageTypeDefOf.PositiveEvent;
				obj2.lookTargets = item2;
				GenSpawn.Spawn(obj2, loc, map);
			}
		}
		if (threatPoints.HasValue && !def.threats.NullOrEmpty())
		{
			PreSpawnThreats(list2, map, list);
			SpawnThreats(sketch, map, center, threatPoints.Value, list, list2);
		}
		PostSpawnStructure(list2, map, list);
		sketch.thingsToSpawn.Clear();
		tmpSpawnedThreatThings.Clear();
		bool CanExpand(IntVec3 c, Map m)
		{
			Building edifice = c.GetEdifice(m);
			if (edifice != null && (edifice.def == ThingDefOf.Wall || edifice.def == ThingDefOf.Door))
			{
				return true;
			}
			return false;
		}
	}

	private void SpawnThreats(ComplexSketch sketch, Map map, IntVec3 center, float threatPoints, List<Thing> spawnedThings, List<List<CellRect>> roomRects)
	{
		ComplexResolveParams threatParams = default(ComplexResolveParams);
		threatParams.map = map;
		threatParams.complexRect = sketch.structure.OccupiedRect.MovedBy(center);
		threatParams.hostileFaction = GetFixedHostileFactionForThreats();
		threatParams.allRooms = roomRects;
		threatParams.points = threatPoints;
		StringBuilder stringBuilder = null;
		if (DebugViewSettings.logComplexGenPoints)
		{
			stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("----- Logging points for " + def.defName + ". -----");
			stringBuilder.AppendLine($"Total threat points: {threatPoints}");
			stringBuilder.AppendLine($"Room count: {roomRects.Count}");
			stringBuilder.AppendLine($"Approx points per room: {threatParams.points}");
			if (threatParams.hostileFaction != null)
			{
				stringBuilder.AppendLine($"Faction: {threatParams.hostileFaction}");
			}
		}
		useableThreats.Clear();
		useableThreats.AddRange(def.threats.Where((ComplexThreat t) => Rand.Chance(t.chancePerComplex)));
		float num = 0f;
		int num2 = 100;
		Dictionary<List<CellRect>, List<ComplexThreatDef>> usedThreatsByRoom = new Dictionary<List<CellRect>, List<ComplexThreatDef>>();
		while (num < threatPoints && num2 > 0)
		{
			num2--;
			List<CellRect> room = roomRects.RandomElement();
			threatParams.room = room;
			threatParams.spawnedThings = spawnedThings;
			float b = threatPoints - num;
			threatParams.points = Mathf.Min(ThreatPointsFactorRange.RandomInRange * threatPoints, b);
			if (useableThreats.Where(delegate(ComplexThreat t)
			{
				int num3 = 0;
				foreach (KeyValuePair<List<CellRect>, List<ComplexThreatDef>> item in usedThreatsByRoom)
				{
					num3 += item.Value.Count((ComplexThreatDef td) => td == t.def);
				}
				if (num3 >= t.maxPerComplex)
				{
					return false;
				}
				return (!usedThreatsByRoom.ContainsKey(room) || usedThreatsByRoom[room].Count((ComplexThreatDef td) => td == t.def) < t.maxPerRoom) && t.def.Worker.CanResolve(threatParams);
			}).TryRandomElementByWeight((ComplexThreat t) => t.selectionWeight, out var result))
			{
				if (stringBuilder != null)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("-> Resolving threat " + result.def.defName);
				}
				float threatPointsUsed = 0f;
				result.def.Worker.Resolve(threatParams, ref threatPointsUsed, tmpSpawnedThreatThings, stringBuilder);
				num += threatPointsUsed;
				if (!usedThreatsByRoom.ContainsKey(room))
				{
					usedThreatsByRoom[room] = new List<ComplexThreatDef>();
				}
				usedThreatsByRoom[room].Add(result.def);
			}
		}
		if (stringBuilder != null)
		{
			stringBuilder.AppendLine($"Total threat points spent: {num}");
			Log.Message(stringBuilder.ToString());
		}
	}

	private bool TryGetThingTriggerSignal(ComplexResolveParams threatParams, out string triggerSignal)
	{
		if (threatParams.room == null || threatParams.spawnedThings.NullOrEmpty())
		{
			triggerSignal = null;
			return false;
		}
		List<CellRect> room = threatParams.room;
		for (int i = 0; i < threatParams.spawnedThings.Count; i++)
		{
			Thing thing = threatParams.spawnedThings[i];
			if (!room.Any((CellRect r) => r.Contains(thing.Position)))
			{
				continue;
			}
			CompHackable compHackable = thing.TryGetComp<CompHackable>();
			if (compHackable != null && !compHackable.IsHacked)
			{
				if (Rand.Bool)
				{
					if (compHackable.hackingStartedSignal == null)
					{
						compHackable.hackingStartedSignal = "ThreatTriggerSignal" + Find.UniqueIDsManager.GetNextSignalTagID();
					}
					triggerSignal = compHackable.hackingStartedSignal;
				}
				else
				{
					if (compHackable.hackingCompletedSignal == null)
					{
						compHackable.hackingCompletedSignal = "ThreatTriggerSignal" + Find.UniqueIDsManager.GetNextSignalTagID();
					}
					triggerSignal = compHackable.hackingCompletedSignal;
				}
				return true;
			}
			if (thing is Building_Casket building_Casket && building_Casket.CanOpen)
			{
				if (building_Casket.openedSignal.NullOrEmpty())
				{
					building_Casket.openedSignal = "ThreatTriggerSignal" + Find.UniqueIDsManager.GetNextSignalTagID();
				}
				triggerSignal = building_Casket.openedSignal;
				return true;
			}
		}
		triggerSignal = null;
		return false;
	}

	protected virtual void PreSpawnThreats(List<List<CellRect>> rooms, Map map, List<Thing> allSpawnedThings)
	{
	}

	protected virtual void PostSpawnStructure(List<List<CellRect>> rooms, Map map, List<Thing> allSpawnedThings)
	{
	}

	protected static IntVec3 FindBestSpawnLocation(List<List<CellRect>> rooms, ThingDef thingDef, Map map, out List<CellRect> roomUsed, out Rot4 rotUsed, HashSet<List<CellRect>> usedRooms = null)
	{
		tmpSpawnLocations.Clear();
		foreach (List<CellRect> item in rooms.InRandomOrder())
		{
			if (usedRooms != null && usedRooms.Contains(item))
			{
				continue;
			}
			tmpSpawnLocations.Clear();
			tmpSpawnLocations.AddRange(item.SelectMany((CellRect r) => r.Cells));
			foreach (IntVec3 item2 in tmpSpawnLocations.InRandomOrder())
			{
				for (int i = 0; i < 4; i++)
				{
					Rot4 rot = new Rot4(i);
					CellRect cellRect = GenAdj.OccupiedRect(item2, rot, thingDef.size);
					bool flag = false;
					foreach (IntVec3 cell in cellRect.Cells)
					{
						if (!cell.Standable(map) || cell.GetDoor(map) != null)
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						continue;
					}
					bool flag2 = false;
					foreach (IntVec3 edgeCell in cellRect.ExpandedBy(1).EdgeCells)
					{
						if (edgeCell.GetThingList(map).Any((Thing t) => t.def == ThingDefOf.Door))
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2 && ThingUtility.InteractionCellWhenAt(thingDef, item2, rot, map).Standable(map))
					{
						tmpSpawnLocations.Clear();
						usedRooms?.Add(item);
						roomUsed = item;
						rotUsed = rot;
						return item2;
					}
				}
			}
		}
		roomUsed = null;
		rotUsed = default(Rot4);
		return IntVec3.Invalid;
	}
}
