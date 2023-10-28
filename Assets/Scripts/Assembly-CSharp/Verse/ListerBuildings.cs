using System;
using System.Collections.Generic;
using RimWorld;
using Verse.AI;

namespace Verse
{
	public sealed class ListerBuildings
	{
		public List<Building> allBuildingsColonist = new List<Building>();

		public List<Building> allBuildingsNonColonist = new List<Building>();

		public HashSet<Building> allBuildingsColonistCombatTargets = new HashSet<Building>();

		public HashSet<Building> allBuildingsColonistElecFire = new HashSet<Building>();

		public HashSet<Building> allBuildingsAnimalPenMarkers = new HashSet<Building>();

		public HashSet<Building> allBuildingsHitchingPosts = new HashSet<Building>();

		private Dictionary<Thing, Blueprint_Install> reinstallationMap = new Dictionary<Thing, Blueprint_Install>();

		public void Add(Building b)
		{
			if (b.def.building != null && b.def.building.isNaturalRock)
			{
				return;
			}
			if (b.Faction == Faction.OfPlayer)
			{
				allBuildingsColonist.Add(b);
				if (b is IAttackTarget)
				{
					allBuildingsColonistCombatTargets.Add(b);
				}
			}
			else
			{
				allBuildingsNonColonist.Add(b);
			}
			CompProperties_Power compProperties = b.def.GetCompProperties<CompProperties_Power>();
			if (compProperties != null && compProperties.shortCircuitInRain)
			{
				allBuildingsColonistElecFire.Add(b);
			}
			if (b.TryGetComp<CompAnimalPenMarker>() != null)
			{
				allBuildingsAnimalPenMarkers.Add(b);
			}
			if (b.def == ThingDefOf.CaravanPackingSpot)
			{
				allBuildingsHitchingPosts.Add(b);
			}
		}

		public void Remove(Building b)
		{
			allBuildingsColonist.Remove(b);
			allBuildingsNonColonist.Remove(b);
			if (b is IAttackTarget)
			{
				allBuildingsColonistCombatTargets.Remove(b);
			}
			CompProperties_Power compProperties = b.def.GetCompProperties<CompProperties_Power>();
			if (compProperties != null && compProperties.shortCircuitInRain)
			{
				allBuildingsColonistElecFire.Remove(b);
			}
			allBuildingsAnimalPenMarkers.Remove(b);
			allBuildingsHitchingPosts.Remove(b);
		}

		public void RegisterInstallBlueprint(Blueprint_Install blueprint)
		{
			reinstallationMap.Add(blueprint.MiniToInstallOrBuildingToReinstall.GetInnerIfMinified(), blueprint);
		}

		public void DeregisterInstallBlueprint(Blueprint_Install blueprint)
		{
			Thing thing = blueprint.MiniToInstallOrBuildingToReinstall?.GetInnerIfMinified();
			if (thing != null)
			{
				reinstallationMap.Remove(thing);
				return;
			}
			Thing thing2 = null;
			foreach (KeyValuePair<Thing, Blueprint_Install> item in reinstallationMap)
			{
				if (item.Value == blueprint)
				{
					thing2 = item.Key;
					break;
				}
			}
			if (thing2 != null)
			{
				reinstallationMap.Remove(thing2);
			}
		}

		public bool ColonistsHaveBuilding(ThingDef def)
		{
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				if (allBuildingsColonist[i].def == def)
				{
					return true;
				}
			}
			return false;
		}

		public bool ColonistsHaveBuilding(Func<Thing, bool> predicate)
		{
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				if (predicate(allBuildingsColonist[i]))
				{
					return true;
				}
			}
			return false;
		}

		public bool ColonistsHaveResearchBench()
		{
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				if (allBuildingsColonist[i] is Building_ResearchBench)
				{
					return true;
				}
			}
			return false;
		}

		public bool ColonistsHaveBuildingWithPowerOn(ThingDef def)
		{
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				if (allBuildingsColonist[i].def == def)
				{
					CompPowerTrader compPowerTrader = allBuildingsColonist[i].TryGetComp<CompPowerTrader>();
					if (compPowerTrader == null || compPowerTrader.PowerOn)
					{
						return true;
					}
				}
			}
			return false;
		}

		public IEnumerable<Building> AllBuildingsColonistOfDef(ThingDef def)
		{
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				if (allBuildingsColonist[i].def == def)
				{
					yield return allBuildingsColonist[i];
				}
			}
		}

		public IEnumerable<Building> AllBuildingsNonColonistOfDef(ThingDef def)
		{
			for (int i = 0; i < allBuildingsNonColonist.Count; i++)
			{
				if (allBuildingsNonColonist[i].def == def)
				{
					yield return allBuildingsNonColonist[i];
				}
			}
		}

		public bool TryGetReinstallBlueprint(Thing building, out Blueprint_Install bp)
		{
			return reinstallationMap.TryGetValue(building, out bp);
		}

		public IEnumerable<T> AllBuildingsColonistOfClass<T>() where T : Building
		{
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				if (allBuildingsColonist[i] is T val)
				{
					yield return val;
				}
			}
		}

		public void Notify_FactionRemoved(Faction faction)
		{
			for (int i = 0; i < allBuildingsNonColonist.Count; i++)
			{
				if (allBuildingsNonColonist[i].Faction == faction)
				{
					allBuildingsNonColonist[i].SetFaction(null);
				}
			}
		}
	}
}
