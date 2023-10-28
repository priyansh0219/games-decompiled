using System.Collections.Generic;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse.AI;

namespace Verse
{
	public sealed class MapPawns
	{
		private class FactionDictionary
		{
			private Dictionary<Faction, List<Pawn>> pawnList = new Dictionary<Faction, List<Pawn>>(16);

			private List<Pawn> nullFactionPawns = new List<Pawn>(32);

			public List<Pawn> GetPawnList(Faction faction)
			{
				if (faction == null)
				{
					return nullFactionPawns;
				}
				if (pawnList.TryGetValue(faction, out var value))
				{
					return value;
				}
				List<Pawn> list = new List<Pawn>(32);
				pawnList[faction] = list;
				return list;
			}

			public IEnumerable<Faction> KnownFactions()
			{
				return pawnList.Keys.Concat(null);
			}
		}

		private Map map;

		private List<Pawn> pawnsSpawned = new List<Pawn>();

		private FactionDictionary pawnsInFactionSpawned = new FactionDictionary();

		private List<Pawn> prisonersOfColonySpawned = new List<Pawn>();

		private List<Pawn> slavesOfColonySpawned = new List<Pawn>();

		private List<Thing> tmpThings = new List<Thing>();

		private List<Pawn> allPawnsResult = new List<Pawn>();

		private List<Pawn> allPawnsUnspawnedResult = new List<Pawn>();

		private List<Pawn> prisonersOfColonyResult = new List<Pawn>();

		private List<Pawn> freeColonistsAndPrisonersResult = new List<Pawn>();

		private List<Pawn> freeAdultColonistsSpawnedResult = new List<Pawn>();

		private List<Pawn> freeColonistsAndPrisonersSpawnedResult = new List<Pawn>();

		private List<Pawn> spawnedPawnsWithAnyHediffResult = new List<Pawn>();

		private List<Pawn> spawnedHungryPawnsResult = new List<Pawn>();

		private List<Pawn> spawnedPawnsWithMiscNeedsResult = new List<Pawn>();

		private List<Pawn> spawnedColonyAnimalsResult = new List<Pawn>();

		private List<Pawn> spawnedDownedPawnsResult = new List<Pawn>();

		private List<Pawn> spawnedPawnsWhoShouldHaveSurgeryDoneNowResult = new List<Pawn>();

		private List<Pawn> spawnedPawnsWhoShouldHaveInventoryUnloadedResult = new List<Pawn>();

		private List<Pawn> slavesAndPrisonersOfColonySpawnedResult = new List<Pawn>();

		private List<Faction> tmpFactionsOnMap = new List<Faction>(16);

		private FactionDictionary pawnsInFactionResult = new FactionDictionary();

		private FactionDictionary freeHumanlikesOfFactionResult = new FactionDictionary();

		private FactionDictionary freeHumanlikesSpawnedOfFactionResult = new FactionDictionary();

		public List<Pawn> AllPawns
		{
			get
			{
				List<Pawn> allPawnsUnspawned = AllPawnsUnspawned;
				if (allPawnsUnspawned.Count == 0)
				{
					return pawnsSpawned;
				}
				allPawnsResult.Clear();
				allPawnsResult.AddRange(pawnsSpawned);
				allPawnsResult.AddRange(allPawnsUnspawned);
				return allPawnsResult;
			}
		}

		public List<Pawn> AllPawnsUnspawned
		{
			get
			{
				allPawnsUnspawnedResult.Clear();
				ThingOwnerUtility.GetAllThingsRecursively(map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), allPawnsUnspawnedResult, allowUnreal: true, null, alsoGetSpawnedThings: false);
				for (int num = allPawnsUnspawnedResult.Count - 1; num >= 0; num--)
				{
					if (allPawnsUnspawnedResult[num].Dead)
					{
						allPawnsUnspawnedResult.RemoveAt(num);
					}
				}
				return allPawnsUnspawnedResult;
			}
		}

		public List<Pawn> FreeColonists => FreeHumanlikesOfFaction(Faction.OfPlayer);

		public List<Pawn> PrisonersOfColony
		{
			get
			{
				prisonersOfColonyResult.Clear();
				List<Pawn> allPawns = AllPawns;
				for (int i = 0; i < allPawns.Count; i++)
				{
					if (allPawns[i].IsPrisonerOfColony)
					{
						prisonersOfColonyResult.Add(allPawns[i]);
					}
				}
				return prisonersOfColonyResult;
			}
		}

		public List<Pawn> FreeColonistsAndPrisoners
		{
			get
			{
				List<Pawn> freeColonists = FreeColonists;
				List<Pawn> prisonersOfColony = PrisonersOfColony;
				if (prisonersOfColony.Count == 0)
				{
					return freeColonists;
				}
				freeColonistsAndPrisonersResult.Clear();
				freeColonistsAndPrisonersResult.AddRange(freeColonists);
				freeColonistsAndPrisonersResult.AddRange(prisonersOfColony);
				return freeColonistsAndPrisonersResult;
			}
		}

		public int ColonistCount
		{
			get
			{
				if (Current.ProgramState != ProgramState.Playing)
				{
					Log.Error("ColonistCount while not playing. This should get the starting player pawn count.");
					return 3;
				}
				int num = 0;
				List<Pawn> allPawns = AllPawns;
				for (int i = 0; i < allPawns.Count; i++)
				{
					if (allPawns[i].IsColonist)
					{
						num++;
					}
				}
				return num;
			}
		}

		public int AllPawnsCount => AllPawns.Count;

		public int AllPawnsUnspawnedCount => AllPawnsUnspawned.Count;

		public int FreeColonistsCount => FreeColonists.Count;

		public int PrisonersOfColonyCount => PrisonersOfColony.Count;

		public int FreeColonistsAndPrisonersCount => FreeColonistsCount + PrisonersOfColonyCount;

		public bool AnyPawnBlockingMapRemoval
		{
			get
			{
				Faction ofPlayer = Faction.OfPlayer;
				for (int i = 0; i < pawnsSpawned.Count; i++)
				{
					if (!pawnsSpawned[i].Downed && pawnsSpawned[i].IsColonist)
					{
						return true;
					}
					if (pawnsSpawned[i].relations != null && pawnsSpawned[i].relations.relativeInvolvedInRescueQuest != null)
					{
						return true;
					}
					if (pawnsSpawned[i].Faction == ofPlayer || pawnsSpawned[i].HostFaction == ofPlayer)
					{
						Job curJob = pawnsSpawned[i].CurJob;
						if (curJob != null && curJob.exitMapOnArrival)
						{
							return true;
						}
						if (pawnsSpawned[i].health.hediffSet.InLabor())
						{
							return true;
						}
					}
					if (CaravanExitMapUtility.FindCaravanToJoinFor(pawnsSpawned[i]) != null && !pawnsSpawned[i].Downed)
					{
						return true;
					}
					if (ModsConfig.BiotechActive && pawnsSpawned[i].IsColonyMech && pawnsSpawned[i].GetOverseer() != null)
					{
						return true;
					}
				}
				List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.ThingHolder);
				for (int j = 0; j < list.Count; j++)
				{
					IThingHolder thingHolder = PlayerEjectablePodHolder(list[j], includeCryptosleepCaskets: false);
					if (thingHolder == null)
					{
						continue;
					}
					tmpThings.Clear();
					ThingOwnerUtility.GetAllThingsRecursively(thingHolder, tmpThings);
					for (int k = 0; k < tmpThings.Count; k++)
					{
						if (tmpThings[k] is Pawn pawn && !pawn.Dead && !pawn.Downed && (pawn.IsColonist || pawn.IsColonyMech))
						{
							tmpThings.Clear();
							return true;
						}
					}
				}
				tmpThings.Clear();
				return false;
			}
		}

		public List<Pawn> AllPawnsSpawned => pawnsSpawned;

		public List<Pawn> FreeColonistsSpawned => FreeHumanlikesSpawnedOfFaction(Faction.OfPlayer);

		public List<Pawn> FreeAdultColonistsSpawned
		{
			get
			{
				freeAdultColonistsSpawnedResult.Clear();
				foreach (Pawn item in FreeColonistsSpawned)
				{
					if (item.DevelopmentalStage.Adult())
					{
						freeAdultColonistsSpawnedResult.Add(item);
					}
				}
				return freeAdultColonistsSpawnedResult;
			}
		}

		public List<Pawn> PrisonersOfColonySpawned => prisonersOfColonySpawned;

		public List<Pawn> SlavesOfColonySpawned => slavesOfColonySpawned;

		public List<Pawn> FreeColonistsAndPrisonersSpawned
		{
			get
			{
				List<Pawn> freeColonistsSpawned = FreeColonistsSpawned;
				List<Pawn> list = PrisonersOfColonySpawned;
				if (list.Count == 0)
				{
					return freeColonistsSpawned;
				}
				freeColonistsAndPrisonersSpawnedResult.Clear();
				freeColonistsAndPrisonersSpawnedResult.AddRange(freeColonistsSpawned);
				freeColonistsAndPrisonersSpawnedResult.AddRange(list);
				return freeColonistsAndPrisonersSpawnedResult;
			}
		}

		public List<Pawn> SpawnedPawnsWithAnyHediff
		{
			get
			{
				spawnedPawnsWithAnyHediffResult.Clear();
				List<Pawn> allPawnsSpawned = AllPawnsSpawned;
				for (int i = 0; i < allPawnsSpawned.Count; i++)
				{
					if (allPawnsSpawned[i].health.hediffSet.hediffs.Count != 0)
					{
						spawnedPawnsWithAnyHediffResult.Add(allPawnsSpawned[i]);
					}
				}
				return spawnedPawnsWithAnyHediffResult;
			}
		}

		public List<Pawn> SpawnedHungryPawns
		{
			get
			{
				spawnedHungryPawnsResult.Clear();
				List<Pawn> allPawnsSpawned = AllPawnsSpawned;
				for (int i = 0; i < allPawnsSpawned.Count; i++)
				{
					if (FeedPatientUtility.IsHungry(allPawnsSpawned[i]))
					{
						spawnedHungryPawnsResult.Add(allPawnsSpawned[i]);
					}
				}
				return spawnedHungryPawnsResult;
			}
		}

		public List<Pawn> SpawnedPawnsWithMiscNeeds
		{
			get
			{
				spawnedPawnsWithMiscNeedsResult.Clear();
				List<Pawn> allPawnsSpawned = AllPawnsSpawned;
				for (int i = 0; i < allPawnsSpawned.Count; i++)
				{
					if (!allPawnsSpawned[i].needs.MiscNeeds.NullOrEmpty())
					{
						spawnedPawnsWithMiscNeedsResult.Add(allPawnsSpawned[i]);
					}
				}
				return spawnedPawnsWithMiscNeedsResult;
			}
		}

		public List<Pawn> SpawnedColonyAnimals
		{
			get
			{
				spawnedColonyAnimalsResult.Clear();
				List<Pawn> list = SpawnedPawnsInFaction(Faction.OfPlayer);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].RaceProps.Animal)
					{
						spawnedColonyAnimalsResult.Add(list[i]);
					}
				}
				return spawnedColonyAnimalsResult;
			}
		}

		public List<Pawn> SpawnedDownedPawns
		{
			get
			{
				spawnedDownedPawnsResult.Clear();
				List<Pawn> allPawnsSpawned = AllPawnsSpawned;
				for (int i = 0; i < allPawnsSpawned.Count; i++)
				{
					if (allPawnsSpawned[i].Downed)
					{
						spawnedDownedPawnsResult.Add(allPawnsSpawned[i]);
					}
				}
				return spawnedDownedPawnsResult;
			}
		}

		public List<Pawn> SpawnedPawnsWhoShouldHaveSurgeryDoneNow
		{
			get
			{
				spawnedPawnsWhoShouldHaveSurgeryDoneNowResult.Clear();
				List<Pawn> allPawnsSpawned = AllPawnsSpawned;
				for (int i = 0; i < allPawnsSpawned.Count; i++)
				{
					if (HealthAIUtility.ShouldHaveSurgeryDoneNow(allPawnsSpawned[i]))
					{
						spawnedPawnsWhoShouldHaveSurgeryDoneNowResult.Add(allPawnsSpawned[i]);
					}
				}
				return spawnedPawnsWhoShouldHaveSurgeryDoneNowResult;
			}
		}

		public List<Pawn> SpawnedPawnsWhoShouldHaveInventoryUnloaded
		{
			get
			{
				spawnedPawnsWhoShouldHaveInventoryUnloadedResult.Clear();
				List<Pawn> allPawnsSpawned = AllPawnsSpawned;
				for (int i = 0; i < allPawnsSpawned.Count; i++)
				{
					if (allPawnsSpawned[i].inventory.UnloadEverything)
					{
						spawnedPawnsWhoShouldHaveInventoryUnloadedResult.Add(allPawnsSpawned[i]);
					}
				}
				return spawnedPawnsWhoShouldHaveInventoryUnloadedResult;
			}
		}

		public int AllPawnsSpawnedCount => pawnsSpawned.Count;

		public int FreeColonistsSpawnedCount => FreeColonistsSpawned.Count;

		public int FreeAdultColonistsSpawnedCount => FreeAdultColonistsSpawned.Count;

		public int PrisonersOfColonySpawnedCount => PrisonersOfColonySpawned.Count;

		public int FreeColonistsAndPrisonersSpawnedCount => FreeColonistsAndPrisonersSpawned.Count;

		public int ColonistsSpawnedCount
		{
			get
			{
				int num = 0;
				List<Pawn> list = SpawnedPawnsInFaction(Faction.OfPlayer);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].IsColonist)
					{
						num++;
					}
				}
				return num;
			}
		}

		public int FreeColonistsSpawnedOrInPlayerEjectablePodsCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < pawnsSpawned.Count; i++)
				{
					if (pawnsSpawned[i].IsFreeColonist)
					{
						num++;
					}
				}
				List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.ThingHolder);
				for (int j = 0; j < list.Count; j++)
				{
					IThingHolder thingHolder = PlayerEjectablePodHolder(list[j]);
					if (thingHolder == null)
					{
						continue;
					}
					tmpThings.Clear();
					ThingOwnerUtility.GetAllThingsRecursively(thingHolder, tmpThings);
					for (int k = 0; k < tmpThings.Count; k++)
					{
						if (tmpThings[k] is Pawn pawn && !pawn.Dead && pawn.IsFreeColonist)
						{
							num++;
						}
					}
				}
				tmpThings.Clear();
				return num;
			}
		}

		public int SlavesAndPrisonersOfColonySpawnedCount => SlavesAndPrisonersOfColonySpawned.Count;

		public bool AnyColonistSpawned
		{
			get
			{
				List<Pawn> list = SpawnedPawnsInFaction(Faction.OfPlayer);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].IsColonist)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool AnyFreeColonistSpawned
		{
			get
			{
				List<Pawn> list = SpawnedPawnsInFaction(Faction.OfPlayer);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].IsFreeColonist)
					{
						return true;
					}
				}
				return false;
			}
		}

		public List<Pawn> SlavesAndPrisonersOfColonySpawned
		{
			get
			{
				slavesAndPrisonersOfColonySpawnedResult.Clear();
				slavesAndPrisonersOfColonySpawnedResult.AddRange(prisonersOfColonySpawned);
				slavesAndPrisonersOfColonySpawnedResult.AddRange(slavesOfColonySpawned);
				return slavesAndPrisonersOfColonySpawnedResult;
			}
		}

		private static IThingHolder PlayerEjectablePodHolder(Thing thing, bool includeCryptosleepCaskets = true)
		{
			Building_CryptosleepCasket building_CryptosleepCasket = thing as Building_CryptosleepCasket;
			CompTransporter compTransporter = thing.TryGetComp<CompTransporter>();
			CompBiosculpterPod compBiosculpterPod = thing.TryGetComp<CompBiosculpterPod>();
			if ((includeCryptosleepCaskets && building_CryptosleepCasket != null && building_CryptosleepCasket.def.building.isPlayerEjectable) || thing is IActiveDropPod || thing is PawnFlyer || thing is Building_Enterable || compTransporter != null || compBiosculpterPod != null)
			{
				IThingHolder thingHolder = compTransporter;
				object obj = thingHolder;
				if (obj == null)
				{
					thingHolder = compBiosculpterPod;
					obj = thingHolder ?? (thing as IThingHolder);
				}
				return (IThingHolder)obj;
			}
			return null;
		}

		public MapPawns(Map map)
		{
			this.map = map;
		}

		private void EnsureFactionsListsInit()
		{
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				pawnsInFactionSpawned.GetPawnList(allFactionsListForReading[i]);
			}
		}

		public List<Faction> FactionsOnMap()
		{
			tmpFactionsOnMap.Clear();
			foreach (Faction item in pawnsInFactionResult.KnownFactions())
			{
				if (pawnsInFactionResult.GetPawnList(item).Count > 0)
				{
					tmpFactionsOnMap.Add(item);
				}
			}
			return tmpFactionsOnMap;
		}

		public List<Pawn> PawnsInFaction(Faction faction)
		{
			List<Pawn> pawnList = pawnsInFactionResult.GetPawnList(faction);
			pawnList.Clear();
			List<Pawn> allPawns = AllPawns;
			for (int i = 0; i < allPawns.Count; i++)
			{
				if (allPawns[i].Faction == faction)
				{
					pawnList.Add(allPawns[i]);
				}
			}
			return pawnList;
		}

		public List<Pawn> SpawnedPawnsInFaction(Faction faction)
		{
			EnsureFactionsListsInit();
			return pawnsInFactionSpawned.GetPawnList(faction);
		}

		public List<Pawn> FreeHumanlikesOfFaction(Faction faction)
		{
			List<Pawn> pawnList = freeHumanlikesOfFactionResult.GetPawnList(faction);
			pawnList.Clear();
			List<Pawn> allPawns = AllPawns;
			for (int i = 0; i < allPawns.Count; i++)
			{
				if (allPawns[i].Faction == faction && (allPawns[i].HostFaction == null || allPawns[i].IsSlave) && allPawns[i].RaceProps.Humanlike)
				{
					pawnList.Add(allPawns[i]);
				}
			}
			return pawnList;
		}

		public List<Pawn> FreeHumanlikesSpawnedOfFaction(Faction faction)
		{
			List<Pawn> pawnList = freeHumanlikesSpawnedOfFactionResult.GetPawnList(faction);
			pawnList.Clear();
			List<Pawn> list = SpawnedPawnsInFaction(faction);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].HostFaction == null && list[i].RaceProps.Humanlike)
				{
					pawnList.Add(list[i]);
				}
			}
			return pawnList;
		}

		public void RegisterPawn(Pawn p)
		{
			if (p.Dead)
			{
				Log.Warning(string.Concat("Tried to register dead pawn ", p, " in ", GetType(), "."));
			}
			else if (!p.Spawned)
			{
				Log.Warning(string.Concat("Tried to register despawned pawn ", p, " in ", GetType(), "."));
			}
			else if (p.Map != map)
			{
				Log.Warning(string.Concat("Tried to register pawn ", p, " but his Map is not this one."));
			}
			else
			{
				if (!p.mindState.Active)
				{
					return;
				}
				EnsureFactionsListsInit();
				if (!pawnsSpawned.Contains(p))
				{
					pawnsSpawned.Add(p);
				}
				if (p.Faction != null)
				{
					List<Pawn> pawnList = pawnsInFactionSpawned.GetPawnList(p.Faction);
					if (!pawnList.Contains(p))
					{
						pawnList.Add(p);
						if (p.Faction == Faction.OfPlayer)
						{
							pawnList.InsertionSort(delegate(Pawn a, Pawn b)
							{
								int num = ((a.playerSettings != null) ? a.playerSettings.joinTick : 0);
								int value = ((b.playerSettings != null) ? b.playerSettings.joinTick : 0);
								return num.CompareTo(value);
							});
						}
					}
				}
				if (p.IsPrisonerOfColony && !prisonersOfColonySpawned.Contains(p))
				{
					prisonersOfColonySpawned.Add(p);
				}
				if (p.IsSlaveOfColony && !slavesOfColonySpawned.Contains(p))
				{
					slavesOfColonySpawned.Add(p);
				}
				DoListChangedNotifications();
			}
		}

		public void DeRegisterPawn(Pawn p)
		{
			EnsureFactionsListsInit();
			pawnsSpawned.Remove(p);
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				Faction faction = allFactionsListForReading[i];
				pawnsInFactionSpawned.GetPawnList(faction).Remove(p);
			}
			prisonersOfColonySpawned.Remove(p);
			slavesOfColonySpawned.Remove(p);
			DoListChangedNotifications();
		}

		public void UpdateRegistryForPawn(Pawn p)
		{
			DeRegisterPawn(p);
			if (p.Spawned && p.Map == map)
			{
				RegisterPawn(p);
			}
			DoListChangedNotifications();
		}

		private void DoListChangedNotifications()
		{
			MainTabWindowUtility.NotifyAllPawnTables_PawnsChanged();
			if (Find.ColonistBar != null)
			{
				Find.ColonistBar.MarkColonistsDirty();
			}
		}

		public void LogListedPawns()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("MapPawns:");
			stringBuilder.AppendLine("pawnsSpawned");
			foreach (Pawn item in pawnsSpawned)
			{
				stringBuilder.AppendLine("    " + item.ToString());
			}
			stringBuilder.AppendLine("AllPawnsUnspawned");
			foreach (Pawn item2 in AllPawnsUnspawned)
			{
				stringBuilder.AppendLine("    " + item2.ToString());
			}
			foreach (Faction item3 in pawnsInFactionSpawned.KnownFactions())
			{
				stringBuilder.AppendLine("pawnsInFactionSpawned[" + item3.ToStringSafe() + "]");
				foreach (Pawn pawn in pawnsInFactionSpawned.GetPawnList(item3))
				{
					stringBuilder.AppendLine("    " + pawn.ToString());
				}
			}
			stringBuilder.AppendLine("prisonersOfColonySpawned");
			foreach (Pawn item4 in prisonersOfColonySpawned)
			{
				stringBuilder.AppendLine("    " + item4.ToString());
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
