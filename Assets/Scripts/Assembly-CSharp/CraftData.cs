using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UWE;
using UnityEngine;

public class CraftData
{
	public enum BackgroundType : byte
	{
		Normal = 0,
		Blueprint = 1,
		PlantWater = 2,
		PlantWaterSeed = 3,
		PlantAir = 4,
		PlantAirSeed = 5,
		ExosuitArm = 6
	}

	public class TechGroupComparer : IEqualityComparer<TechGroup>
	{
		public bool Equals(TechGroup x, TechGroup y)
		{
			int num = (int)x;
			return num.Equals((int)y);
		}

		public int GetHashCode(TechGroup techGroup)
		{
			return (int)techGroup;
		}
	}

	public class TechCategoryComparer : IEqualityComparer<TechCategory>
	{
		public bool Equals(TechCategory x, TechCategory y)
		{
			int num = (int)x;
			return num.Equals((int)y);
		}

		public int GetHashCode(TechCategory techCategory)
		{
			return (int)techCategory;
		}
	}

	public static TechGroupComparer sTechGroupComparer = new TechGroupComparer();

	public static TechCategoryComparer sTechCategoryComparer = new TechCategoryComparer();

	private static readonly Dictionary<TechGroup, Dictionary<TechCategory, List<TechType>>> groups = new Dictionary<TechGroup, Dictionary<TechCategory, List<TechType>>>(sTechGroupComparer)
	{
		{
			TechGroup.Resources,
			new Dictionary<TechCategory, List<TechType>>(sTechCategoryComparer)
			{
				{
					TechCategory.BasicMaterials,
					new List<TechType>
					{
						TechType.Titanium,
						TechType.TitaniumIngot,
						TechType.FiberMesh,
						TechType.Silicone,
						TechType.Glass,
						TechType.Bleach,
						TechType.Lubricant,
						TechType.EnameledGlass,
						TechType.PlasteelIngot
					}
				},
				{
					TechCategory.AdvancedMaterials,
					new List<TechType>
					{
						TechType.HydrochloricAcid,
						TechType.Benzene,
						TechType.AramidFibers,
						TechType.Aerogel,
						TechType.Polyaniline,
						TechType.HatchingEnzymes
					}
				},
				{
					TechCategory.Electronics,
					new List<TechType>
					{
						TechType.CopperWire,
						TechType.Battery,
						TechType.PrecursorIonBattery,
						TechType.PowerCell,
						TechType.PrecursorIonPowerCell,
						TechType.ComputerChip,
						TechType.WiringKit,
						TechType.AdvancedWiringKit,
						TechType.ReactorRod
					}
				}
			}
		},
		{
			TechGroup.Survival,
			new Dictionary<TechCategory, List<TechType>>(sTechCategoryComparer)
			{
				{
					TechCategory.Water,
					new List<TechType>
					{
						TechType.FilteredWater,
						TechType.DisinfectedWater
					}
				},
				{
					TechCategory.CookedFood,
					new List<TechType>
					{
						TechType.CookedHoleFish,
						TechType.CookedPeeper,
						TechType.CookedBladderfish,
						TechType.CookedGarryFish,
						TechType.CookedHoverfish,
						TechType.CookedReginald,
						TechType.CookedSpadefish,
						TechType.CookedBoomerang,
						TechType.CookedLavaBoomerang,
						TechType.CookedEyeye,
						TechType.CookedLavaEyeye,
						TechType.CookedOculus,
						TechType.CookedHoopfish,
						TechType.CookedSpinefish
					}
				},
				{
					TechCategory.CuredFood,
					new List<TechType>
					{
						TechType.CuredHoleFish,
						TechType.CuredPeeper,
						TechType.CuredBladderfish,
						TechType.CuredGarryFish,
						TechType.CuredHoverfish,
						TechType.CuredReginald,
						TechType.CuredSpadefish,
						TechType.CuredBoomerang,
						TechType.CuredLavaBoomerang,
						TechType.CuredEyeye,
						TechType.CuredLavaEyeye,
						TechType.CuredOculus,
						TechType.CuredHoopfish,
						TechType.CuredSpinefish
					}
				}
			}
		},
		{
			TechGroup.Personal,
			new Dictionary<TechCategory, List<TechType>>(sTechCategoryComparer)
			{
				{
					TechCategory.Equipment,
					new List<TechType>
					{
						TechType.Tank,
						TechType.DoubleTank,
						TechType.Fins,
						TechType.RadiationSuit,
						TechType.ReinforcedDiveSuit,
						TechType.WaterFiltrationSuit,
						TechType.FirstAidKit,
						TechType.FireExtinguisher,
						TechType.Rebreather,
						TechType.Compass,
						TechType.Thermometer,
						TechType.Pipe,
						TechType.PipeSurfaceFloater,
						TechType.PrecursorKey_Purple,
						TechType.PrecursorKey_Blue,
						TechType.PrecursorKey_Orange
					}
				},
				{
					TechCategory.Tools,
					new List<TechType>
					{
						TechType.Scanner,
						TechType.Welder,
						TechType.Flashlight,
						TechType.Knife,
						TechType.DiveReel,
						TechType.AirBladder,
						TechType.Flare,
						TechType.Builder,
						TechType.LaserCutter,
						TechType.StasisRifle,
						TechType.Terraformer,
						TechType.PropulsionCannon,
						TechType.LEDLight,
						TechType.Transfuser
					}
				}
			}
		},
		{
			TechGroup.Machines,
			new Dictionary<TechCategory, List<TechType>>(sTechCategoryComparer) { 
			{
				TechCategory.Machines,
				new List<TechType>
				{
					TechType.Seaglide,
					TechType.Constructor,
					TechType.Beacon,
					TechType.SmallStorage,
					TechType.Gravsphere,
					TechType.CyclopsDecoy
				}
			} }
		},
		{
			TechGroup.Constructor,
			new Dictionary<TechCategory, List<TechType>>(sTechCategoryComparer) { 
			{
				TechCategory.Constructor,
				new List<TechType>
				{
					TechType.Seamoth,
					TechType.Exosuit,
					TechType.RocketBase,
					TechType.RocketBaseLadder,
					TechType.RocketStage1,
					TechType.RocketStage2,
					TechType.RocketStage3
				}
			} }
		},
		{
			TechGroup.Workbench,
			new Dictionary<TechCategory, List<TechType>>(sTechCategoryComparer) { 
			{
				TechCategory.Workbench,
				new List<TechType>
				{
					TechType.LithiumIonBattery,
					TechType.HeatBlade,
					TechType.PlasteelTank,
					TechType.HighCapacityTank,
					TechType.UltraGlideFins,
					TechType.SwimChargeFins,
					TechType.RepulsionCannon,
					TechType.CyclopsHullModule2,
					TechType.CyclopsHullModule3,
					TechType.VehicleHullModule2,
					TechType.VehicleHullModule3,
					TechType.ExoHullModule2,
					TechType.PowerGlide
				}
			} }
		},
		{
			TechGroup.VehicleUpgrades,
			new Dictionary<TechCategory, List<TechType>>(sTechCategoryComparer) { 
			{
				TechCategory.VehicleUpgrades,
				new List<TechType>
				{
					TechType.VehicleHullModule1,
					TechType.VehicleArmorPlating,
					TechType.VehiclePowerUpgradeModule,
					TechType.VehicleStorageModule,
					TechType.SeamothSolarCharge,
					TechType.SeamothElectricalDefense,
					TechType.SeamothTorpedoModule,
					TechType.SeamothSonarModule,
					TechType.ExoHullModule1,
					TechType.ExosuitThermalReactorModule,
					TechType.ExosuitJetUpgradeModule,
					TechType.ExosuitPropulsionArmModule,
					TechType.ExosuitGrapplingArmModule,
					TechType.ExosuitDrillArmModule,
					TechType.ExosuitTorpedoArmModule,
					TechType.WhirlpoolTorpedo,
					TechType.GasTorpedo
				}
			} }
		},
		{
			TechGroup.MapRoomUpgrades,
			new Dictionary<TechCategory, List<TechType>>(sTechCategoryComparer) { 
			{
				TechCategory.MapRoomUpgrades,
				new List<TechType>
				{
					TechType.MapRoomHUDChip,
					TechType.MapRoomCamera,
					TechType.MapRoomUpgradeScanRange,
					TechType.MapRoomUpgradeScanSpeed
				}
			} }
		},
		{
			TechGroup.Cyclops,
			new Dictionary<TechCategory, List<TechType>>(sTechCategoryComparer)
			{
				{
					TechCategory.Cyclops,
					new List<TechType>
					{
						TechType.CyclopsHullBlueprint,
						TechType.CyclopsBridgeBlueprint,
						TechType.CyclopsEngineBlueprint,
						TechType.Cyclops
					}
				},
				{
					TechCategory.CyclopsUpgrades,
					new List<TechType>
					{
						TechType.CyclopsHullModule1,
						TechType.PowerUpgradeModule,
						TechType.CyclopsShieldModule,
						TechType.CyclopsSonarModule,
						TechType.CyclopsSeamothRepairModule,
						TechType.CyclopsFireSuppressionModule,
						TechType.CyclopsDecoyModule,
						TechType.CyclopsThermalReactorModule
					}
				}
			}
		},
		{
			TechGroup.BasePieces,
			new Dictionary<TechCategory, List<TechType>>(sTechCategoryComparer)
			{
				{
					TechCategory.BasePiece,
					new List<TechType>
					{
						TechType.BaseFoundation,
						TechType.BaseCorridorI,
						TechType.BaseCorridorL,
						TechType.BaseCorridorT,
						TechType.BaseCorridorX,
						TechType.BaseCorridorGlassI,
						TechType.BaseCorridorGlassL,
						TechType.BaseConnector
					}
				},
				{
					TechCategory.BaseRoom,
					new List<TechType>
					{
						TechType.BaseRoom,
						TechType.BaseMapRoom,
						TechType.BaseMoonpool,
						TechType.BaseObservatory,
						TechType.BaseLargeRoom,
						TechType.BaseGlassDome,
						TechType.BaseLargeGlassDome
					}
				},
				{
					TechCategory.BaseWall,
					new List<TechType>
					{
						TechType.BaseHatch,
						TechType.BaseWindow,
						TechType.BaseReinforcement
					}
				}
			}
		},
		{
			TechGroup.ExteriorModules,
			new Dictionary<TechCategory, List<TechType>>(sTechCategoryComparer)
			{
				{
					TechCategory.ExteriorModule,
					new List<TechType>
					{
						TechType.SolarPanel,
						TechType.ThermalPlant,
						TechType.PowerTransmitter
					}
				},
				{
					TechCategory.ExteriorLight,
					new List<TechType>
					{
						TechType.Techlight,
						TechType.Spotlight
					}
				},
				{
					TechCategory.ExteriorOther,
					new List<TechType>
					{
						TechType.FarmingTray,
						TechType.BasePipeConnector
					}
				}
			}
		},
		{
			TechGroup.InteriorPieces,
			new Dictionary<TechCategory, List<TechType>>(sTechCategoryComparer)
			{
				{
					TechCategory.InteriorPiece,
					new List<TechType>
					{
						TechType.BaseLadder,
						TechType.BaseFiltrationMachine,
						TechType.BaseBulkhead,
						TechType.BaseUpgradeConsole,
						TechType.BasePartition,
						TechType.BasePartitionDoor
					}
				},
				{
					TechCategory.InteriorRoom,
					new List<TechType>
					{
						TechType.BaseBioReactor,
						TechType.BaseNuclearReactor,
						TechType.BaseWaterPark
					}
				}
			}
		},
		{
			TechGroup.InteriorModules,
			new Dictionary<TechCategory, List<TechType>>(sTechCategoryComparer) { 
			{
				TechCategory.InteriorModule,
				new List<TechType>
				{
					TechType.Fabricator,
					TechType.Radio,
					TechType.MedicalCabinet,
					TechType.SmallLocker,
					TechType.Locker,
					TechType.BatteryCharger,
					TechType.PowerCellCharger,
					TechType.Aquarium,
					TechType.Workbench,
					TechType.Centrifuge,
					TechType.PlanterPot,
					TechType.PlanterPot2,
					TechType.PlanterPot3,
					TechType.PlanterBox,
					TechType.PlanterShelf
				}
			} }
		},
		{
			TechGroup.Miscellaneous,
			new Dictionary<TechCategory, List<TechType>>(sTechCategoryComparer)
			{
				{
					TechCategory.Misc,
					new List<TechType>
					{
						TechType.Bench,
						TechType.Bed1,
						TechType.Bed2,
						TechType.NarrowBed,
						TechType.StarshipDesk,
						TechType.StarshipChair,
						TechType.StarshipChair2,
						TechType.StarshipChair3,
						TechType.Sign,
						TechType.PictureFrame,
						TechType.StarshipCargoCrate,
						TechType.StarshipCircuitBox,
						TechType.StarshipMonitor,
						TechType.BarTable,
						TechType.Trashcans,
						TechType.LabTrashcan,
						TechType.VendingMachine,
						TechType.CoffeeVendingMachine,
						TechType.LabCounter,
						TechType.BasePlanter,
						TechType.SingleWallShelf,
						TechType.WallShelves
					}
				},
				{
					TechCategory.MiscHullplates,
					new List<TechType>
					{
						TechType.DevTestItem,
						TechType.SpecialHullPlate,
						TechType.BikemanHullPlate,
						TechType.EatMyDictionHullPlate,
						TechType.DioramaHullPlate,
						TechType.MarkiplierHullPlate,
						TechType.MuyskermHullPlate,
						TechType.LordMinionHullPlate,
						TechType.JackSepticEyeHullPlate,
						TechType.IGPHullPlate,
						TechType.GilathissHullPlate,
						TechType.Marki1,
						TechType.Marki2,
						TechType.JackSepticEye,
						TechType.EatMyDiction
					}
				}
			}
		}
	};

	private static readonly HashSet<TechType> blacklist = new HashSet<TechType>
	{
		TechType.DevTestItemBlueprintOld,
		TechType.DevTestItem,
		TechType.SpecialHullPlateBlueprintOld,
		TechType.BikemanHullPlateBlueprintOld,
		TechType.EatMyDictionHullPlateBlueprintOld,
		TechType.SpecialHullPlate,
		TechType.BikemanHullPlate,
		TechType.EatMyDictionHullPlate,
		TechType.DioramaHullPlate,
		TechType.MarkiplierHullPlate,
		TechType.MuyskermHullPlate,
		TechType.LordMinionHullPlate,
		TechType.JackSepticEyeHullPlate,
		TechType.IGPHullPlate,
		TechType.GilathissHullPlate,
		TechType.Marki1,
		TechType.Marki2,
		TechType.JackSepticEye,
		TechType.EatMyDiction,
		TechType.EnzymeCureBall,
		TechType.TimeCapsule
	};

	private static readonly Dictionary<string, TechType> entTechMap = new Dictionary<string, TechType>(StringComparer.OrdinalIgnoreCase);

	private static Dictionary<string, TechType> entClassTechTable = null;

	private static Dictionary<TechType, string> techMapping = null;

	private static bool cacheInitialized = false;

	public static bool IsAllowed(TechType techType)
	{
		if (!Application.isEditor)
		{
			return !blacklist.Contains(techType);
		}
		return true;
	}

	public static HashSet<TechType> FilterAllowed(HashSet<TechType> techTypes)
	{
		if (Application.isEditor)
		{
			return techTypes;
		}
		HashSet<TechType> hashSet = new HashSet<TechType>();
		HashSet<TechType>.Enumerator enumerator = techTypes.GetEnumerator();
		while (enumerator.MoveNext())
		{
			TechType current = enumerator.Current;
			if (!blacklist.Contains(current))
			{
				hashSet.Add(current);
			}
		}
		return hashSet;
	}

	public static TechType GetTechForEntNameExpensive(string prefabName)
	{
		PrepareEntTechCache();
		return entTechMap.GetOrDefault(prefabName, TechType.None);
	}

	public static void DebugLogDatabase()
	{
		PreparePrefabIDCache();
		string text = "craftdata_log.txt";
		using (StreamWriter writer = FileUtils.CreateTextFile(text))
		{
			DebugWrite(writer, "BEGIN DebugLogDatabase (" + entClassTechTable.Count + " prefabs known, " + techMapping.Count + " tech types known)");
			foreach (KeyValuePair<string, TechType> item in entClassTechTable)
			{
				DebugWrite(writer, "  \"" + item.Key + "\" -> " + item.Value);
			}
			DebugWrite(writer, "-------------");
			foreach (KeyValuePair<TechType, string> item2 in techMapping)
			{
				DebugWrite(writer, string.Concat("  \"", item2.Key, "\" -> ", item2.Value));
			}
			DebugWrite(writer, "END DebugLogDatabase (see " + text + ")");
		}
	}

	private static void DebugWrite(StreamWriter writer, string value)
	{
		writer.WriteLine(value);
		Debug.Log(value);
	}

	public static void RebuildDatabase()
	{
		cacheInitialized = false;
		PreparePrefabIDCache();
	}

	public static void PreparePrefabIDCache()
	{
		if (cacheInitialized)
		{
			return;
		}
		entClassTechTable = new Dictionary<string, TechType>();
		techMapping = new Dictionary<TechType, string>(TechTypeExtensions.sTechTypeComparer);
		PrefabDatabase.LoadPrefabDatabase(SNUtils.prefabDatabaseFilename);
		Debug.LogFormat("Caching tech types for {0} prefabs", PrefabDatabase.prefabFiles.Count);
		foreach (KeyValuePair<string, string> prefabFile in PrefabDatabase.prefabFiles)
		{
			AddToCache(prefabFile.Key, prefabFile.Value);
		}
		cacheInitialized = true;
	}

	private static void PrepareEntTechCache()
	{
		if (entTechMap.Count <= 0)
		{
			EntTechData entTechData = Resources.Load<EntTechData>("EntTechData");
			EntTechData.Entry[] array = entTechData.entTechMap;
			foreach (EntTechData.Entry entry in array)
			{
				entTechMap[entry.prefabName] = entry.techType;
			}
			Resources.UnloadAsset(entTechData);
		}
	}

	private static void AddToCache(string classId, string filename)
	{
		TechType techForEntNameExpensive = GetTechForEntNameExpensive(Path.GetFileNameWithoutExtension(filename));
		entClassTechTable[classId] = techForEntNameExpensive;
		if (techForEntNameExpensive != 0)
		{
			techMapping[techForEntNameExpensive] = classId;
		}
	}

	public static string GetClassIdForTechType(TechType techType)
	{
		PreparePrefabIDCache();
		return techMapping.GetOrDefault(techType, null);
	}

	public static IEnumerator InstantiateFromPrefabAsync(TechType techType, IOut<GameObject> result, bool customOnly = false)
	{
		CoroutineTask<GameObject> request = GetPrefabForTechTypeAsync(techType, customOnly);
		yield return request;
		GameObject result2 = request.GetResult();
		if (result2 != null)
		{
			result.Set(Utils.SpawnFromPrefab(result2, null));
		}
		else if (!customOnly)
		{
			result.Set(Utils.CreateGenericLoot(techType));
		}
	}

	public static GameObject InstantiateFromPrefab(GameObject prefab, TechType techType, bool customOnly = false)
	{
		if (prefab != null)
		{
			return Utils.SpawnFromPrefab(prefab, null);
		}
		if (!customOnly)
		{
			return Utils.CreateGenericLoot(techType);
		}
		return null;
	}

	public static IEnumerator AddToInventoryAsync(TechType techType, IOut<GameObject> result, int num = 1, bool noMessage = false, bool spawnIfCantAdd = true)
	{
		GameObject lastSpawnedInst = null;
		TaskResult<GameObject> instResult = new TaskResult<GameObject>();
		Transform transform = MainCamera.camera.transform;
		Vector3 position = transform.position + transform.forward * 3f;
		for (int i = 0; i < num; i++)
		{
			yield return InstantiateFromPrefabAsync(techType, instResult);
			GameObject gameObject = instResult.Get();
			if (!(gameObject != null))
			{
				continue;
			}
			gameObject.transform.position = position;
			CrafterLogic.NotifyCraftEnd(gameObject, techType);
			Pickupable component = gameObject.GetComponent<Pickupable>();
			Inventory inventory = Inventory.Get();
			if (!(component != null) || !(inventory != null))
			{
				continue;
			}
			if (!inventory.HasRoomFor(component) || !inventory.Pickup(component, noMessage))
			{
				ErrorMessage.AddError(Language.main.Get("InventoryFull"));
				if (!spawnIfCantAdd)
				{
					UnityEngine.Object.Destroy(gameObject);
				}
			}
			else
			{
				lastSpawnedInst = gameObject;
			}
		}
		result.Set(lastSpawnedInst);
	}

	public static void AddToInventory(TechType techType, int num = 1, bool noMessage = false, bool spawnIfCantAdd = true)
	{
		CoroutineHost.StartCoroutine(AddToInventoryAsync(techType, DiscardTaskResult<GameObject>.Instance, num, noMessage, spawnIfCantAdd));
	}

	public static CoroutineTask<GameObject> GetPrefabForTechTypeAsync(TechType techType, bool verbose = true)
	{
		TaskResult<GameObject> result = new TaskResult<GameObject>();
		return new CoroutineTask<GameObject>(GetPrefabForTechTypeAsync(techType, verbose, result), result);
	}

	private static IEnumerator GetPrefabForTechTypeAsync(TechType techType, bool verbose, IOut<GameObject> result)
	{
		PreparePrefabIDCache();
		if (!techMapping.TryGetValue(techType, out var classId))
		{
			if (verbose)
			{
				Debug.LogErrorFormat("Could not find prefab class id for tech type {0}. Probably missing from EntTechData.asset", techType);
			}
			result.Set(null);
			yield break;
		}
		IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classId);
		yield return request;
		if (!request.TryGetPrefab(out var prefab))
		{
			if (verbose)
			{
				Debug.LogErrorFormat("Could not find prefab for class id {0} (tech type {1}). Probably mising from prefab database", classId, techType);
			}
			result.Set(null);
		}
		else
		{
			result.Set(prefab);
		}
	}

	public static TechType GetTechType(GameObject obj)
	{
		GameObject go;
		return GetTechType(obj, out go);
	}

	public static TechType GetTechType(GameObject obj, out GameObject go)
	{
		try
		{
			PreparePrefabIDCache();
			Transform transform = obj.transform;
			bool flag;
			TechTag component;
			bool flag2;
			PrefabIdentifier component2;
			do
			{
				flag = transform.TryGetComponent<TechTag>(out component);
				flag2 = transform.TryGetComponent<PrefabIdentifier>(out component2);
				transform = transform.parent;
			}
			while (transform != null && !flag && !flag2);
			if (component != null)
			{
				go = component.gameObject;
				return component.type;
			}
			if (component2 != null)
			{
				go = component2.gameObject;
				return entClassTechTable.GetOrDefault(component2.ClassId, TechType.None);
			}
			go = null;
			return TechType.None;
		}
		finally
		{
		}
	}

	public static void GetBuilderCategories(TechGroup group, List<TechCategory> result, bool append = false)
	{
		if (!append)
		{
			result.Clear();
		}
		if (groups.TryGetValue(group, out var value))
		{
			Dictionary<TechCategory, List<TechType>>.Enumerator enumerator = value.GetEnumerator();
			while (enumerator.MoveNext())
			{
				result.Add(enumerator.Current.Key);
			}
		}
	}

	public static void GetBuilderTech(TechGroup group, TechCategory category, List<TechType> result, bool append = false)
	{
		if (!append)
		{
			result.Clear();
		}
		if (groups.TryGetValue(group, out var value) && value.TryGetValue(category, out var value2))
		{
			for (int i = 0; i < value2.Count; i++)
			{
				TechType item = value2[i];
				result.Add(item);
			}
		}
	}

	public static void GetBuilderGroupTech(TechGroup group, List<TechType> result, bool append = false)
	{
		if (!append)
		{
			result.Clear();
		}
		if (!groups.TryGetValue(group, out var value))
		{
			return;
		}
		Dictionary<TechCategory, List<TechType>>.Enumerator enumerator = value.GetEnumerator();
		while (enumerator.MoveNext())
		{
			List<TechType> value2 = enumerator.Current.Value;
			for (int i = 0; i < value2.Count; i++)
			{
				result.Add(value2[i]);
			}
		}
	}

	public static bool GetBuilderIndex(TechType techType, out TechGroup group, out TechCategory category, out int index)
	{
		Dictionary<TechGroup, Dictionary<TechCategory, List<TechType>>>.Enumerator enumerator = groups.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<TechGroup, Dictionary<TechCategory, List<TechType>>> current = enumerator.Current;
			TechGroup key = current.Key;
			Dictionary<TechCategory, List<TechType>>.Enumerator enumerator2 = current.Value.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				KeyValuePair<TechCategory, List<TechType>> current2 = enumerator2.Current;
				TechCategory key2 = current2.Key;
				int num = current2.Value.IndexOf(techType);
				if (num != -1)
				{
					group = key;
					category = key2;
					index = num;
					return true;
				}
			}
		}
		group = TechGroup.Miscellaneous;
		category = TechCategory.Misc;
		index = int.MaxValue;
		return false;
	}

	public static string CompileTimeCheck()
	{
		List<TechType> list = new List<TechType>();
		Type typeFromHandle = typeof(TechType);
		foreach (Dictionary<TechCategory, List<TechType>> value in groups.Values)
		{
			foreach (List<TechType> value2 in value.Values)
			{
				foreach (TechType item in value2)
				{
					string enumName = typeFromHandle.GetEnumName(item);
					if (Attribute.IsDefined(typeFromHandle.GetField(enumName), typeof(ObsoleteAttribute)))
					{
						list.Add(item);
					}
				}
			}
		}
		if (list.Count > 0)
		{
			return "Craft data contains obsolete tech types:\n" + string.Join("\n", list);
		}
		return null;
	}
}
