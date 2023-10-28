using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

public class ComplexWorker_Ancient : ComplexWorker
{
	public override Faction GetFixedHostileFactionForThreats()
	{
		if (Rand.Chance(def.fixedHostileFactionChance))
		{
			if (Faction.OfInsects != null && Faction.OfMechanoids != null)
			{
				if (!Rand.Bool)
				{
					return Faction.OfMechanoids;
				}
				return Faction.OfInsects;
			}
			if (Faction.OfInsects != null)
			{
				return Faction.OfInsects;
			}
			return Faction.OfMechanoids;
		}
		return null;
	}

	protected override void PostSpawnStructure(List<List<CellRect>> rooms, Map map, List<Thing> allSpawnedThings)
	{
		if (!ModsConfig.IdeologyActive)
		{
			return;
		}
		if (def.roomRewardCrateFactor > 0f)
		{
			int num = 0;
			for (int i = 0; i < allSpawnedThings.Count; i++)
			{
				if (allSpawnedThings[i] is Building_Crate)
				{
					num++;
				}
			}
			int num2 = Mathf.RoundToInt((float)rooms.Count * def.roomRewardCrateFactor) - num;
			if (num2 <= 0)
			{
				return;
			}
			ThingSetMakerDef thingSetMakerDef = def.rewardThingSetMakerDef ?? ThingSetMakerDefOf.Reward_ItemsStandard;
			foreach (List<CellRect> item in rooms.InRandomOrder())
			{
				bool flag = true;
				IEnumerable<IntVec3> enumerable = item.SelectMany((CellRect r) => r.Cells);
				foreach (IntVec3 item2 in enumerable)
				{
					Building edifice = item2.GetEdifice(map);
					if (edifice != null && edifice is Building_Crate)
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
				if (ComplexUtility.TryFindRandomSpawnCell(ThingDefOf.AncientHermeticCrate, enumerable, map, out var spawnPosition, 1, Rot4.South))
				{
					Building_Crate building_Crate = (Building_Crate)GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.AncientHermeticCrate), spawnPosition, map, Rot4.South);
					List<Thing> list = thingSetMakerDef.root.Generate(default(ThingSetMakerParams));
					for (int num3 = list.Count - 1; num3 >= 0; num3--)
					{
						Thing thing = list[num3];
						if (!building_Crate.TryAcceptThing(thing, allowSpecialEffects: false))
						{
							thing.Destroy();
						}
					}
					num2--;
				}
				if (num2 <= 0)
				{
					break;
				}
			}
		}
		foreach (List<CellRect> item3 in rooms.InRandomOrder())
		{
			foreach (IntVec3 item4 in item3.SelectMany((CellRect r) => r.Cells).InRandomOrder())
			{
				if (CanPlaceCommsConsoleAt(item4))
				{
					GenSpawn.Spawn(ThingDefOf.AncientCommsConsole, item4, map);
					return;
				}
			}
		}
		bool CanPlaceCommsConsoleAt(IntVec3 cell)
		{
			foreach (IntVec3 item5 in GenAdj.OccupiedRect(cell, Rot4.North, ThingDefOf.AncientCommsConsole.Size).ExpandedBy(1))
			{
				if (item5.GetEdifice(map) != null)
				{
					return false;
				}
			}
			return true;
		}
	}
}
