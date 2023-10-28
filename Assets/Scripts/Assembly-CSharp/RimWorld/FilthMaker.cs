using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class FilthMaker
	{
		private static List<Filth> toBeRemoved = new List<Filth>();

		public static bool CanMakeFilth(IntVec3 c, Map map, ThingDef filthDef, FilthSourceFlags additionalFlags = FilthSourceFlags.None)
		{
			TerrainDef terrain = c.GetTerrain(map);
			if (!filthDef.filth.ignoreFilthMultiplierStat && (filthDef.filth.placementMask & FilthSourceFlags.Natural) == 0 && Rand.Value > terrain.GetStatValueAbstract(StatDefOf.FilthMultiplier))
			{
				return false;
			}
			FilthSourceFlags filthSourceFlags = filthDef.filth.placementMask | additionalFlags;
			if (terrain.filthAcceptanceMask != 0 && filthSourceFlags.HasFlag(FilthSourceFlags.Pawn))
			{
				if (c.GetRoof(map) != null)
				{
					return true;
				}
				Room room = c.GetRoom(map);
				if (room != null && !room.TouchesMapEdge && !room.UsesOutdoorTemperature)
				{
					return true;
				}
			}
			return TerrainAcceptsFilth(terrain, filthDef, additionalFlags);
		}

		public static bool TerrainAcceptsFilth(TerrainDef terrainDef, ThingDef filthDef, FilthSourceFlags additionalFlags = FilthSourceFlags.None)
		{
			if (terrainDef.filthAcceptanceMask == FilthSourceFlags.None)
			{
				return false;
			}
			FilthSourceFlags filthSourceFlags = filthDef.filth.placementMask | additionalFlags;
			return (terrainDef.filthAcceptanceMask & filthSourceFlags) == filthSourceFlags;
		}

		public static bool TryMakeFilth(IntVec3 c, Map map, ThingDef filthDef, int count = 1, FilthSourceFlags additionalFlags = FilthSourceFlags.None, bool shouldPropagate = true)
		{
			bool flag = false;
			for (int i = 0; i < count; i++)
			{
				flag |= TryMakeFilth(c, map, filthDef, null, shouldPropagate, additionalFlags);
			}
			return flag;
		}

		public static bool TryMakeFilth(IntVec3 c, Map map, ThingDef filthDef, string source, int count = 1, FilthSourceFlags additionalFlags = FilthSourceFlags.None)
		{
			bool flag = false;
			for (int i = 0; i < count; i++)
			{
				flag |= TryMakeFilth(c, map, filthDef, Gen.YieldSingle(source), shouldPropagate: true, additionalFlags);
			}
			return flag;
		}

		public static bool TryMakeFilth(IntVec3 c, Map map, ThingDef filthDef, IEnumerable<string> sources, FilthSourceFlags additionalFlags = FilthSourceFlags.None)
		{
			return TryMakeFilth(c, map, filthDef, sources, shouldPropagate: true, additionalFlags);
		}

		private static bool TryMakeFilth(IntVec3 c, Map map, ThingDef filthDef, IEnumerable<string> sources, bool shouldPropagate, FilthSourceFlags additionalFlags = FilthSourceFlags.None)
		{
			Filth filth = (Filth)(from t in c.GetThingList(map)
				where t.def == filthDef
				select t).FirstOrDefault();
			if (!c.WalkableByAny(map) || (filth != null && !filth.CanBeThickened))
			{
				if (shouldPropagate)
				{
					List<IntVec3> list = GenAdj.AdjacentCells8WayRandomized();
					for (int i = 0; i < 8; i++)
					{
						IntVec3 c2 = c + list[i];
						if (c2.InBounds(map) && TryMakeFilth(c2, map, filthDef, sources, shouldPropagate: false))
						{
							return true;
						}
					}
				}
				filth?.AddSources(sources);
				return false;
			}
			if (filth != null)
			{
				filth.ThickenFilth();
				filth.AddSources(sources);
			}
			else
			{
				if (!CanMakeFilth(c, map, filthDef, additionalFlags))
				{
					return false;
				}
				Filth obj = (Filth)ThingMaker.MakeThing(filthDef);
				obj.AddSources(sources);
				GenSpawn.Spawn(obj, c, map);
			}
			FilthMonitor.Notify_FilthSpawned();
			return true;
		}

		public static void RemoveAllFilth(IntVec3 c, Map map)
		{
			toBeRemoved.Clear();
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i] is Filth item)
				{
					toBeRemoved.Add(item);
				}
			}
			for (int j = 0; j < toBeRemoved.Count; j++)
			{
				toBeRemoved[j].Destroy();
			}
			toBeRemoved.Clear();
		}
	}
}
