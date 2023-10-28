using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class GenLeaving
	{
		private const float LeaveFraction_Kill = 0.25f;

		private const float LeaveFraction_Cancel = 1f;

		public const float LeaveFraction_DeconstructDefault = 0.5f;

		private const float LeaveFraction_FailConstruction = 0.5f;

		private static List<Thing> tmpKilledLeavings = new List<Thing>();

		private static List<IntVec3> tmpCellsCandidates = new List<IntVec3>();

		public static void DoLeavingsFor(Thing diedThing, Map map, DestroyMode mode, List<Thing> listOfLeavingsOut = null)
		{
			DoLeavingsFor(diedThing, map, mode, diedThing.OccupiedRect().ExpandedBy(diedThing.def.killedLeavingsExpandRect), null, listOfLeavingsOut);
		}

		public static void DoLeavingsFor(Thing diedThing, Map map, DestroyMode mode, CellRect leavingsRect, Predicate<IntVec3> nearPlaceValidator = null, List<Thing> listOfLeavingsOut = null)
		{
			if (Current.ProgramState != ProgramState.Playing && mode != DestroyMode.Refund)
			{
				return;
			}
			int num;
			switch (mode)
			{
			case DestroyMode.Vanish:
			case DestroyMode.QuestLogic:
				return;
			default:
				num = ((mode == DestroyMode.KillFinalizeLeavingsOnly) ? 1 : 0);
				break;
			case DestroyMode.KillFinalize:
				num = 1;
				break;
			}
			bool flag = (byte)num != 0;
			if (flag && diedThing.def.filthLeaving != null)
			{
				for (int i = leavingsRect.minZ; i <= leavingsRect.maxZ; i++)
				{
					for (int j = leavingsRect.minX; j <= leavingsRect.maxX; j++)
					{
						FilthMaker.TryMakeFilth(new IntVec3(j, 0, i), map, diedThing.def.filthLeaving, Rand.RangeInclusive(1, 3));
					}
				}
			}
			ThingOwner<Thing> thingOwner = new ThingOwner<Thing>();
			if (flag)
			{
				List<ThingDefCountClass> list = new List<ThingDefCountClass>();
				if (diedThing.def.killedLeavings != null)
				{
					list.AddRange(diedThing.def.killedLeavings);
				}
				if (diedThing.HostileTo(Faction.OfPlayer) && !diedThing.def.killedLeavingsPlayerHostile.NullOrEmpty())
				{
					list.AddRange(diedThing.def.killedLeavingsPlayerHostile);
				}
				for (int k = 0; k < list.Count; k++)
				{
					Thing thing = ThingMaker.MakeThing(list[k].thingDef);
					thing.stackCount = list[k].count;
					thingOwner.TryAdd(thing);
				}
			}
			if (CanBuildingLeaveResources(diedThing, mode) && mode != DestroyMode.KillFinalizeLeavingsOnly)
			{
				if (diedThing is Frame frame)
				{
					for (int num2 = frame.resourceContainer.Count - 1; num2 >= 0; num2--)
					{
						int num3 = GetBuildingResourcesLeaveCalculator(diedThing, mode)(frame.resourceContainer[num2].stackCount);
						if (num3 > 0)
						{
							frame.resourceContainer.TryTransferToContainer(frame.resourceContainer[num2], thingOwner, num3);
						}
					}
					frame.resourceContainer.ClearAndDestroyContents();
				}
				else
				{
					List<ThingDefCountClass> list2 = diedThing.CostListAdjusted();
					for (int l = 0; l < list2.Count; l++)
					{
						ThingDefCountClass thingDefCountClass = list2[l];
						if (thingDefCountClass.thingDef == ThingDefOf.ReinforcedBarrel && !Find.Storyteller.difficulty.classicMortars)
						{
							CompRefuelable compRefuelable = diedThing.TryGetComp<CompRefuelable>();
							if (compRefuelable != null && compRefuelable.Props.fuelIsMortarBarrel && compRefuelable.FuelPercentOfMax < 0.5f)
							{
								continue;
							}
						}
						int num4 = GetBuildingResourcesLeaveCalculator(diedThing, mode)(thingDefCountClass.count);
						if (num4 > 0 && mode == DestroyMode.KillFinalize && thingDefCountClass.thingDef.slagDef != null)
						{
							int count = thingDefCountClass.thingDef.slagDef.smeltProducts.First((ThingDefCountClass pro) => pro.thingDef == ThingDefOf.Steel).count;
							int a = num4 / count;
							a = Mathf.Min(a, diedThing.def.Size.Area / 2);
							for (int m = 0; m < a; m++)
							{
								thingOwner.TryAdd(ThingMaker.MakeThing(thingDefCountClass.thingDef.slagDef));
							}
							num4 -= a * count;
						}
						if (num4 > 0)
						{
							Thing thing2 = ThingMaker.MakeThing(thingDefCountClass.thingDef);
							thing2.stackCount = num4;
							thingOwner.TryAdd(thing2);
						}
					}
				}
			}
			tmpKilledLeavings.Clear();
			List<IntVec3> list3 = leavingsRect.Cells.InRandomOrder().ToList();
			int num5 = 0;
			while (thingOwner.Count > 0)
			{
				if (mode == DestroyMode.KillFinalize && !map.areaManager.Home[list3[num5]])
				{
					thingOwner[0].SetForbidden(value: true, warnOnFail: false);
				}
				if (!thingOwner.TryDrop(thingOwner[0], list3[num5], map, ThingPlaceMode.Near, out var lastResultingThing, null, nearPlaceValidator))
				{
					Log.Warning(string.Concat("Failed to place all leavings for destroyed thing ", diedThing, " at ", leavingsRect.CenterCell));
					break;
				}
				tmpKilledLeavings.Add(lastResultingThing);
				num5++;
				if (num5 >= list3.Count)
				{
					num5 = 0;
				}
			}
			listOfLeavingsOut?.AddRange(tmpKilledLeavings);
			if (mode == DestroyMode.KillFinalize && tmpKilledLeavings.Count > 0)
			{
				QuestUtility.SendQuestTargetSignals(diedThing.questTags, "KilledLeavingsLeft", diedThing.Named("DROPPER"), tmpKilledLeavings.Named("SUBJECT"));
			}
			tmpKilledLeavings.Clear();
		}

		public static void DoLeavingsFor(TerrainDef terrain, IntVec3 cell, Map map)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			ThingOwner<Thing> thingOwner = new ThingOwner<Thing>();
			List<ThingDefCountClass> list = terrain.CostListAdjusted(null);
			for (int i = 0; i < list.Count; i++)
			{
				ThingDefCountClass thingDefCountClass = list[i];
				int num = GenMath.RoundRandom((float)thingDefCountClass.count * terrain.resourcesFractionWhenDeconstructed);
				if (num > 0)
				{
					Thing thing = ThingMaker.MakeThing(thingDefCountClass.thingDef);
					thing.stackCount = num;
					thingOwner.TryAdd(thing);
				}
			}
			while (thingOwner.Count > 0)
			{
				if (!thingOwner.TryDrop(thingOwner[0], cell, map, ThingPlaceMode.Near, out var _))
				{
					Log.Warning(string.Concat("Failed to place all leavings for removed terrain ", terrain, " at ", cell));
					break;
				}
			}
		}

		public static bool CanBuildingLeaveResources(Thing destroyedThing, DestroyMode mode)
		{
			if (!(destroyedThing is Building))
			{
				return false;
			}
			if (mode == DestroyMode.Deconstruct && typeof(Frame).IsAssignableFrom(destroyedThing.GetType()))
			{
				mode = DestroyMode.Cancel;
			}
			switch (mode)
			{
			case DestroyMode.Vanish:
				return false;
			case DestroyMode.WillReplace:
				return false;
			case DestroyMode.KillFinalize:
				return destroyedThing.def.leaveResourcesWhenKilled;
			case DestroyMode.Deconstruct:
				return destroyedThing.def.resourcesFractionWhenDeconstructed != 0f;
			case DestroyMode.Cancel:
				return true;
			case DestroyMode.FailConstruction:
				return true;
			case DestroyMode.Refund:
				return true;
			case DestroyMode.QuestLogic:
				return false;
			case DestroyMode.KillFinalizeLeavingsOnly:
				return false;
			default:
				throw new ArgumentException("Unknown destroy mode " + mode);
			}
		}

		private static Func<int, int> GetBuildingResourcesLeaveCalculator(Thing destroyedThing, DestroyMode mode)
		{
			if (!CanBuildingLeaveResources(destroyedThing, mode))
			{
				return (int count) => 0;
			}
			if (mode == DestroyMode.Deconstruct && typeof(Frame).IsAssignableFrom(destroyedThing.GetType()))
			{
				mode = DestroyMode.Cancel;
			}
			switch (mode)
			{
			case DestroyMode.Vanish:
				return (int count) => 0;
			case DestroyMode.WillReplace:
				return (int count) => 0;
			case DestroyMode.KillFinalize:
				return (int count) => GenMath.RoundRandom((float)count * 0.25f);
			case DestroyMode.Deconstruct:
				return (int count) => Mathf.Min(GenMath.RoundRandom((float)count * destroyedThing.def.resourcesFractionWhenDeconstructed), count);
			case DestroyMode.Cancel:
				return (int count) => GenMath.RoundRandom((float)count * 1f);
			case DestroyMode.FailConstruction:
				return (int count) => GenMath.RoundRandom((float)count * 0.5f);
			case DestroyMode.Refund:
				return (int count) => count;
			case DestroyMode.QuestLogic:
				return (int count) => 0;
			case DestroyMode.KillFinalizeLeavingsOnly:
				return (int count) => 0;
			default:
				throw new ArgumentException("Unknown destroy mode " + mode);
			}
		}

		public static void DropFilthDueToDamage(Thing t, float damageDealt)
		{
			if (!t.def.useHitPoints || !t.Spawned || t.def.filthLeaving == null)
			{
				return;
			}
			CellRect cellRect = t.OccupiedRect().ExpandedBy(1);
			tmpCellsCandidates.Clear();
			foreach (IntVec3 item in cellRect)
			{
				if (item.InBounds(t.Map) && item.Walkable(t.Map))
				{
					tmpCellsCandidates.Add(item);
				}
			}
			if (tmpCellsCandidates.Any())
			{
				int num = GenMath.RoundRandom(damageDealt * Mathf.Min(1f / 60f, 1f / ((float)t.MaxHitPoints / 10f)));
				for (int i = 0; i < num; i++)
				{
					FilthMaker.TryMakeFilth(tmpCellsCandidates.RandomElement(), t.Map, t.def.filthLeaving);
				}
				tmpCellsCandidates.Clear();
			}
		}
	}
}
