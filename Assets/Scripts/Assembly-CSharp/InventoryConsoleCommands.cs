using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UWE;
using UnityEngine;

public class InventoryConsoleCommands : MonoBehaviour
{
	private void Awake()
	{
		DevConsole.RegisterConsoleCommand(this, "pdalog");
		DevConsole.RegisterConsoleCommand(this, "unlockallbuildables");
		DevConsole.RegisterConsoleCommand(this, "techtype", caseSensitiveArgs: false, combineArgs: true);
		DevConsole.RegisterConsoleCommand(this, "item");
		DevConsole.RegisterConsoleCommand(this, "madloot");
		DevConsole.RegisterConsoleCommand(this, "niceloot");
		DevConsole.RegisterConsoleCommand(this, "spawnloot");
		DevConsole.RegisterConsoleCommand(this, "tools");
		DevConsole.RegisterConsoleCommand(this, "eggs");
		DevConsole.RegisterConsoleCommand(this, "unlockall");
		DevConsole.RegisterConsoleCommand(this, "unlock");
		DevConsole.RegisterConsoleCommand(this, "resourcesfor");
		DevConsole.RegisterConsoleCommand(this, "rotfood");
		DevConsole.RegisterConsoleCommand(this, "charge");
		DevConsole.RegisterConsoleCommand(this, "clearinventory");
		DevConsole.RegisterConsoleCommand(this, "vehicleupgrades");
		DevConsole.RegisterConsoleCommand(this, "seamothupgrades");
		DevConsole.RegisterConsoleCommand(this, "exosuitupgrades");
		DevConsole.RegisterConsoleCommand(this, "exosuitarms");
		DevConsole.RegisterConsoleCommand(this, "cyclopsupgrades");
		DevConsole.RegisterConsoleCommand(this, "tooltipdebug");
	}

	private void OnConsoleCommand_techtype(NotificationCenter.Notification n)
	{
		if (n != null && n.data != null && n.data.Count > 0)
		{
			string text = (string)n.data[0];
			List<string> keysFor = Language.main.GetKeysFor(text, StringComparison.OrdinalIgnoreCase);
			if (keysFor.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < keysFor.Count; i++)
				{
					if (TechTypeExtensions.FromString(keysFor[i], out var techType, ignoreCase: true))
					{
						string text2 = techType.AsString();
						if (stringBuilder.Length > 0)
						{
							stringBuilder.Append(" ");
						}
						stringBuilder.Append(text2);
						ErrorMessage.AddDebug($"TechType for '{text}' string is {text2}");
					}
				}
				if (stringBuilder.Length > 0)
				{
					GUIUtility.systemCopyBuffer = stringBuilder.ToString();
					return;
				}
			}
			ErrorMessage.AddDebug($"'{text}' is not a TechType");
		}
		ErrorMessage.AddDebug("Usage: techtype translated_name");
	}

	private void OnConsoleCommand_item(NotificationCenter.Notification n)
	{
		if (n == null || n.data == null || n.data.Count <= 0)
		{
			return;
		}
		string text = (string)n.data[0];
		if (UWE.Utils.TryParseEnum<TechType>(text, out var result))
		{
			if (CraftData.IsAllowed(result))
			{
				int number = 1;
				if (n.data.Count > 1 && int.TryParse((string)n.data[1], out var result2))
				{
					number = result2;
				}
				StartCoroutine(ItemCmdSpawnAsync(number, result));
			}
		}
		else
		{
			IEnumerable<string> techTypeNamesSuggestion = TechTypeExtensions.GetTechTypeNamesSuggestion(text);
			ErrorMessage.AddDebug(string.Format("Could not find tech type for '{0}'. Did you mean:\n{1}", text, string.Join("\n", techTypeNamesSuggestion)));
		}
	}

	private static IEnumerator ItemCmdSpawnAsync(int number, TechType techType)
	{
		TaskResult<GameObject> result = new TaskResult<GameObject>();
		for (int i = 0; i < number; i++)
		{
			yield return CraftData.InstantiateFromPrefabAsync(techType, result);
			GameObject gameObject = result.Get();
			if (gameObject != null)
			{
				gameObject.transform.position = MainCamera.camera.transform.position + MainCamera.camera.transform.forward * 3f;
				CrafterLogic.NotifyCraftEnd(gameObject, techType);
				Pickupable component = gameObject.GetComponent<Pickupable>();
				if (component != null && !Inventory.main.Pickup(component))
				{
					ErrorMessage.AddError(Language.main.Get("InventoryFull"));
				}
			}
		}
	}

	private void OnConsoleCommand_madloot()
	{
		CraftData.AddToInventory(TechType.Knife);
		CraftData.AddToInventory(TechType.Scanner);
		CraftData.AddToInventory(TechType.Builder);
		CraftData.AddToInventory(TechType.Titanium, 10);
		CraftData.AddToInventory(TechType.Glass, 10);
		CraftData.AddToInventory(TechType.Battery, 3);
		CraftData.AddToInventory(TechType.ComputerChip, 4);
	}

	private void OnConsoleCommand_niceloot()
	{
		CraftData.AddToInventory(TechType.RadiationSuit);
		CraftData.AddToInventory(TechType.ReinforcedDiveSuit);
		CraftData.AddToInventory(TechType.WaterFiltrationSuit);
		CraftData.AddToInventory(TechType.ScrapMetal);
		CraftData.AddToInventory(TechType.StasisRifle);
		CraftData.AddToInventory(TechType.CrashPowder);
		CraftData.AddToInventory(TechType.CoralChunk);
		CraftData.AddToInventory(TechType.JeweledDiskPiece);
		CraftData.AddToInventory(TechType.Quartz);
		CraftData.AddToInventory(TechType.UraniniteCrystal);
		CraftData.AddToInventory(TechType.Nickel);
		CraftData.AddToInventory(TechType.AluminumOxide);
		CraftData.AddToInventory(TechType.Copper);
		CraftData.AddToInventory(TechType.Diamond);
		CraftData.AddToInventory(TechType.Gold);
		CraftData.AddToInventory(TechType.Kyanite);
		CraftData.AddToInventory(TechType.Lead);
		CraftData.AddToInventory(TechType.Lithium);
		CraftData.AddToInventory(TechType.Magnetite);
		CraftData.AddToInventory(TechType.Salt);
		CraftData.AddToInventory(TechType.Silver);
		CraftData.AddToInventory(TechType.StalkerTooth);
		CraftData.AddToInventory(TechType.Sulphur);
		CraftData.AddToInventory(TechType.MercuryOre);
		CraftData.AddToInventory(TechType.Tank);
		CraftData.AddToInventory(TechType.HighCapacityTank);
		CraftData.AddToInventory(TechType.Compass);
		CraftData.AddToInventory(TechType.Signal);
		CraftData.AddToInventory(TechType.RadiationHelmet);
		CraftData.AddToInventory(TechType.Rebreather);
	}

	private void OnConsoleCommand_spawnloot()
	{
		StartCoroutine(SpawnLootAsync());
	}

	private static IEnumerator SpawnLootAsync()
	{
		yield return Utils.CreateNPrefabs(TechType.Copper);
		yield return Utils.CreateNPrefabs(TechType.Gold);
		yield return Utils.CreateNPrefabs(TechType.Magnesium);
		yield return Utils.CreateNPrefabs(TechType.ScrapMetal);
		yield return Utils.CreateNPrefabs(TechType.ScrapMetal);
		yield return Utils.CreateNPrefabs(TechType.ScrapMetal);
		yield return Utils.CreateNPrefabs(TechType.ScrapMetal);
		yield return Utils.CreateNPrefabs(TechType.Quartz);
		yield return Utils.CreateNPrefabs(TechType.Salt);
	}

	private void OnConsoleCommand_tools()
	{
		CraftData.AddToInventory(TechType.Scanner);
		CraftData.AddToInventory(TechType.Welder);
		CraftData.AddToInventory(TechType.Flashlight);
		CraftData.AddToInventory(TechType.Knife);
		CraftData.AddToInventory(TechType.DiveReel);
		CraftData.AddToInventory(TechType.AirBladder);
		CraftData.AddToInventory(TechType.Flare);
		CraftData.AddToInventory(TechType.Builder);
		CraftData.AddToInventory(TechType.LaserCutter);
		CraftData.AddToInventory(TechType.StasisRifle);
		CraftData.AddToInventory(TechType.PropulsionCannon);
		CraftData.AddToInventory(TechType.LEDLight);
	}

	private void OnConsoleCommand_eggs()
	{
		CraftData.AddToInventory(TechType.BonesharkEgg);
		CraftData.AddToInventory(TechType.CrabsnakeEgg);
		CraftData.AddToInventory(TechType.CrabsquidEgg);
		CraftData.AddToInventory(TechType.CutefishEgg);
		CraftData.AddToInventory(TechType.JellyrayEgg);
		CraftData.AddToInventory(TechType.JumperEgg);
		CraftData.AddToInventory(TechType.MesmerEgg);
		CraftData.AddToInventory(TechType.RabbitrayEgg);
		CraftData.AddToInventory(TechType.SandsharkEgg);
		CraftData.AddToInventory(TechType.ShockerEgg);
		CraftData.AddToInventory(TechType.SpadefishEgg);
		CraftData.AddToInventory(TechType.SafeShallowsEgg);
		CraftData.AddToInventory(TechType.KelpForestEgg);
		CraftData.AddToInventory(TechType.GrassyPlateausEgg);
		CraftData.AddToInventory(TechType.GrandReefsEgg);
		CraftData.AddToInventory(TechType.MushroomForestEgg);
		CraftData.AddToInventory(TechType.KooshZoneEgg);
		CraftData.AddToInventory(TechType.TwistyBridgesEgg);
		CraftData.AddToInventory(TechType.LavaZoneEgg);
		CraftData.AddToInventory(TechType.StalkerEgg);
		CraftData.AddToInventory(TechType.ReefbackEgg);
		CraftData.AddToInventory(TechType.GasopodEgg);
		CraftData.AddToInventory(TechType.CrashEgg);
		CraftData.AddToInventory(TechType.LavaLizardEgg);
	}

	private void GetSpawnPosition(float maxDist, out Vector3 position, out Quaternion rotation)
	{
		Transform transform = MainCamera.camera.transform;
		Vector3 forward = transform.forward;
		position = transform.position + maxDist * forward;
		Vector3 toDirection = Vector3.up;
		Vector3 origin = transform.position + forward;
		RaycastHit hitInfo = default(RaycastHit);
		if (Physics.Raycast(origin, forward, out hitInfo, maxDist))
		{
			position = hitInfo.point;
			toDirection = hitInfo.normal;
		}
		rotation = Quaternion.FromToRotation(Vector3.up, toDirection);
	}

	private void OnConsoleCommand_entityslot(NotificationCenter.Notification n)
	{
		if (n == null)
		{
			return;
		}
		int num = ((n.data != null) ? n.data.Count : 0);
		if (num > 0)
		{
			string text = (string)n.data[0];
			if (UWE.Utils.TryParseEnum<BiomeType>(text, out var result))
			{
				GetSpawnPosition(10f, out var position, out var rotation);
				GameObject obj = new GameObject("EntitySlotTest");
				Transform component = obj.GetComponent<Transform>();
				component.position = position;
				component.rotation = rotation;
				LargeWorldEntity largeWorldEntity = obj.AddComponent<LargeWorldEntity>();
				largeWorldEntity.cellLevel = LargeWorldEntity.CellLevel.Near;
				EntitySlot entitySlot = obj.AddComponent<EntitySlot>();
				entitySlot.biomeType = result;
				entitySlot.autoGenerated = true;
				if (num > 1)
				{
					DevConsole.ParseFloat(n, 1, out entitySlot.density, 1f);
				}
				entitySlot.allowedTypes = new List<EntitySlot.Type>
				{
					EntitySlot.Type.Small,
					EntitySlot.Type.Medium,
					EntitySlot.Type.Large,
					EntitySlot.Type.Tall,
					EntitySlot.Type.Creature
				};
				LargeWorld.main.streamer.cellManager.RegisterEntity(largeWorldEntity);
			}
			else
			{
				ErrorMessage.AddDebug($"Can't parse {text} as BiomeType");
			}
		}
		else
		{
			ErrorMessage.AddDebug("Usage: entityslot BiomeType [density]");
		}
	}

	private IEnumerator OnConsoleCommand_lootprobability(NotificationCenter.Notification n)
	{
		if (n == null || n.data == null)
		{
			yield break;
		}
		if (UWE.Utils.TryParseEnum<BiomeType>((string)n.data[0], out var biomeType))
		{
			int testCount = 100;
			if (n.data.Count > 1 && int.TryParse((string)n.data[1], out var result))
			{
				testCount = Mathf.Min(result, 10000);
			}
			LargeWorldEntitySpawner csvEntitySpawner = LargeWorldStreamer.main.cellManager.spawner;
			GameObject test = new GameObject("CSVEntitySpawner Test");
			EntitySlot entitySlot = test.AddComponent<EntitySlot>();
			entitySlot.biomeType = biomeType;
			entitySlot.allowedTypes = new List<EntitySlot.Type>
			{
				EntitySlot.Type.Small,
				EntitySlot.Type.Medium,
				EntitySlot.Type.Large,
				EntitySlot.Type.Tall,
				EntitySlot.Type.Creature
			};
			entitySlot.density = 1f;
			entitySlot.autoGenerated = true;
			Dictionary<string, int> counter = new Dictionary<string, int>();
			for (int i = 0; i < testCount; i++)
			{
				string prefab = "None";
				IPrefabRequest request = PrefabDatabase.GetPrefabAsync(csvEntitySpawner.GetPrefabForSlot(entitySlot).classId);
				yield return request;
				if (request.TryGetPrefab(out var prefab2))
				{
					prefab = prefab2.name;
				}
				if (counter.TryGetValue(prefab, out var value))
				{
					counter[prefab] = value + 1;
				}
				else
				{
					counter.Add(prefab, 1);
				}
			}
			UnityEngine.Object.Destroy(test);
			List<string> list = new List<string>(counter.Keys);
			list.Sort();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("Probabilities distribution test for biome {0} ({1} test runs):\n", biomeType, testCount);
			int j = 0;
			for (int count = list.Count; j < count; j++)
			{
				string text = list[j];
				float num = (float)counter[text] / (float)testCount;
				stringBuilder.AppendFormat("{0} - {1}%\n", text, num * 100f);
			}
			string message = stringBuilder.ToString();
			ErrorMessage.AddDebug(message);
			Debug.Log(message);
		}
		else
		{
			ErrorMessage.AddDebug("Usage: lootprobability BiomeType [testCount]");
		}
	}

	private void OnConsoleCommand_pendingitem(NotificationCenter.Notification n)
	{
		if (n != null && n.data != null && n.data.Count > 0)
		{
			_ = (string)n.data[0];
			if (UWE.Utils.TryParseEnum<TechType>((string)n.data[0], out var result) && CraftData.IsAllowed(result))
			{
				StartCoroutine(PendingItemCmdAsync(result));
			}
			else
			{
				ErrorMessage.AddDebug("Could not find tech type for tech name = " + base.name);
			}
		}
	}

	private static IEnumerator PendingItemCmdAsync(TechType techType)
	{
		TaskResult<GameObject> result = new TaskResult<GameObject>();
		yield return CraftData.InstantiateFromPrefabAsync(techType, result);
		GameObject gameObject = result.Get();
		if (gameObject != null)
		{
			Pickupable component = gameObject.GetComponent<Pickupable>();
			if (component != null)
			{
				Inventory.main.AddPending(component);
			}
		}
	}

	public void OnConsoleCommand_unlockall()
	{
		KnownTech.UnlockAll(verbose: false);
	}

	public void OnConsoleCommand_unlockallbuildables()
	{
		foreach (TechType sTechType in TechTypeExtensions.sTechTypes)
		{
			if (TechData.GetBuildable(sTechType) && CraftData.IsAllowed(sTechType))
			{
				KnownTech.Add(sTechType, verbose: false);
			}
		}
	}

	private void OnConsoleCommand_unlock(NotificationCenter.Notification n)
	{
		if (n != null && n.data != null)
		{
			string text = (string)n.data[0];
			TechType result;
			if (text == "all")
			{
				KnownTech.UnlockAll(verbose: false);
			}
			else if (UWE.Utils.TryParseEnum<TechType>(text, out result) && CraftData.IsAllowed(result) && KnownTech.Add(result, verbose: false))
			{
				ErrorMessage.AddDebug("Unlocked " + Language.main.Get(result.AsString()));
			}
		}
	}

	private void OnConsoleCommand_unlockforced(NotificationCenter.Notification n)
	{
		if (n != null && n.data != null && UWE.Utils.TryParseEnum<TechType>((string)n.data[0], out var result) && CraftData.IsAllowed(result) && KnownTech.Add(result, verbose: false))
		{
			ErrorMessage.AddDebug("Unlocked " + Language.main.Get(result.AsString()));
		}
	}

	private void OnConsoleCommand_lock(NotificationCenter.Notification n)
	{
		if (n == null || n.data == null)
		{
			return;
		}
		string text = (string)n.data[0];
		TechType result;
		if (text == "all")
		{
			List<TechType> list = new List<TechType>(KnownTech.GetTech());
			for (int i = 0; i < list.Count; i++)
			{
				KnownTech.Remove(list[i]);
			}
		}
		else if (UWE.Utils.TryParseEnum<TechType>(text, out result) && CraftData.IsAllowed(result))
		{
			_ = 0u | (KnownTech.Remove(result) ? 1u : 0u);
			PDAScanner.RemoveAllEntriesWhichUnlocks(result);
			ErrorMessage.AddDebug("Locked " + Language.main.Get(result.AsString()));
		}
	}

	private void OnConsoleCommand_addlocked(NotificationCenter.Notification n)
	{
		if (n != null && n.data != null)
		{
			int count = n.data.Count;
			if (count > 0 && UWE.Utils.TryParseEnum<TechType>((string)n.data[0], out var result) && CraftData.IsAllowed(result))
			{
				int result2 = 0;
				if (count > 1)
				{
					int.TryParse((string)n.data[1], out result2);
				}
				PDAScanner.AddByUnlockable(result, result2);
				ErrorMessage.AddDebug($"Progress for {Language.main.Get(result.AsString())} is set to {result2}");
				return;
			}
		}
		ErrorMessage.AddDebug("Usage: addlocked TechType [progress]");
	}

	private void OnConsoleCommand_resourcesfor(NotificationCenter.Notification n)
	{
		if (n == null || n.data == null || !UWE.Utils.TryParseEnum<TechType>((string)n.data[0], out var result))
		{
			return;
		}
		ReadOnlyCollection<Ingredient> ingredients = TechData.GetIngredients(result);
		if (ingredients != null)
		{
			for (int i = 0; i < ingredients.Count; i++)
			{
				Ingredient ingredient = ingredients[i];
				CraftData.AddToInventory(ingredient.techType, ingredient.amount, noMessage: false, spawnIfCantAdd: false);
			}
		}
	}

	private void OnConsoleCommand_fishes(NotificationCenter.Notification n)
	{
		CraftData.AddToInventory(TechType.Bleach);
		CraftData.AddToInventory(TechType.HoleFish);
		CraftData.AddToInventory(TechType.Peeper);
		CraftData.AddToInventory(TechType.Bladderfish);
		CraftData.AddToInventory(TechType.GarryFish);
		CraftData.AddToInventory(TechType.Hoverfish);
		CraftData.AddToInventory(TechType.Reginald);
		CraftData.AddToInventory(TechType.Spadefish);
		CraftData.AddToInventory(TechType.Boomerang);
		CraftData.AddToInventory(TechType.Eyeye);
		CraftData.AddToInventory(TechType.Oculus);
		CraftData.AddToInventory(TechType.Hoopfish);
		CraftData.AddToInventory(TechType.Spinefish);
		CraftData.AddToInventory(TechType.LavaBoomerang);
		CraftData.AddToInventory(TechType.LavaEyeye);
	}

	private void OnConsoleCommand_rotfood(NotificationCenter.Notification n)
	{
		foreach (InventoryItem item2 in (IEnumerable<InventoryItem>)Inventory.main.container)
		{
			Pickupable item = item2.item;
			if (item != null)
			{
				Eatable component = item.GetComponent<Eatable>();
				if (component != null)
				{
					float num = ((component.foodValue >= component.waterValue) ? component.foodValue : component.waterValue);
					component.timeDecayStart = DayNightCycle.main.timePassedAsFloat - num / component.kDecayRate;
				}
			}
		}
	}

	private void OnConsoleCommand_pdalog(NotificationCenter.Notification n)
	{
		if (n == null || n.data == null || n.data.Count <= 0)
		{
			return;
		}
		string text = (string)n.data[0];
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		if (string.Equals(text, "all", StringComparison.OrdinalIgnoreCase))
		{
			using (Dictionary<string, PDALog.EntryData>.Enumerator enumerator = PDALog.GetMapping())
			{
				while (enumerator.MoveNext())
				{
					PDALog.Add(enumerator.Current.Key, playSound: false);
				}
				return;
			}
		}
		PDALog.Add(text);
	}

	private void OnConsoleCommand_vehicleupgrades(NotificationCenter.Notification n)
	{
		CraftData.AddToInventory(TechType.VehicleArmorPlating);
		CraftData.AddToInventory(TechType.VehiclePowerUpgradeModule);
		CraftData.AddToInventory(TechType.VehicleStorageModule);
		CraftData.AddToInventory(TechType.LootSensorMetal);
		CraftData.AddToInventory(TechType.LootSensorLithium);
		CraftData.AddToInventory(TechType.LootSensorFragment);
	}

	private void OnConsoleCommand_seamothupgrades(NotificationCenter.Notification n)
	{
		CraftData.AddToInventory(TechType.VehicleHullModule1);
		CraftData.AddToInventory(TechType.VehicleHullModule2);
		CraftData.AddToInventory(TechType.VehicleHullModule3);
		CraftData.AddToInventory(TechType.SeamothSolarCharge);
		CraftData.AddToInventory(TechType.SeamothElectricalDefense);
		CraftData.AddToInventory(TechType.SeamothTorpedoModule);
		CraftData.AddToInventory(TechType.SeamothSonarModule);
	}

	private void OnConsoleCommand_exosuitupgrades(NotificationCenter.Notification n)
	{
		CraftData.AddToInventory(TechType.ExoHullModule1);
		CraftData.AddToInventory(TechType.ExoHullModule2);
		CraftData.AddToInventory(TechType.ExosuitThermalReactorModule);
		CraftData.AddToInventory(TechType.ExosuitJetUpgradeModule);
	}

	private void OnConsoleCommand_exosuitarms(NotificationCenter.Notification n)
	{
		CraftData.AddToInventory(TechType.ExosuitPropulsionArmModule);
		CraftData.AddToInventory(TechType.ExosuitGrapplingArmModule);
		CraftData.AddToInventory(TechType.ExosuitDrillArmModule);
		CraftData.AddToInventory(TechType.ExosuitTorpedoArmModule);
	}

	private void OnConsoleCommand_cyclopsupgrades(NotificationCenter.Notification n)
	{
		CraftData.AddToInventory(TechType.CyclopsHullModule1);
		CraftData.AddToInventory(TechType.CyclopsHullModule2);
		CraftData.AddToInventory(TechType.CyclopsHullModule3);
		CraftData.AddToInventory(TechType.PowerUpgradeModule);
		CraftData.AddToInventory(TechType.CyclopsShieldModule);
		CraftData.AddToInventory(TechType.CyclopsSonarModule);
		CraftData.AddToInventory(TechType.CyclopsSeamothRepairModule);
		CraftData.AddToInventory(TechType.CyclopsDecoyModule);
		CraftData.AddToInventory(TechType.CyclopsFireSuppressionModule);
		CraftData.AddToInventory(TechType.CyclopsThermalReactorModule);
	}

	private void OnConsoleCommand_tooltipdebug(NotificationCenter.Notification n)
	{
		TooltipFactory.debug = !TooltipFactory.debug;
		ErrorMessage.AddError(string.Format("Tooltip debug is now {0}.", TooltipFactory.debug ? "on" : "off"));
	}

	private void OnConsoleCommand_equipment()
	{
		CraftData.AddToInventory(TechType.Fins);
		CraftData.AddToInventory(TechType.Tank);
		CraftData.AddToInventory(TechType.Compass);
		CraftData.AddToInventory(TechType.RadiationHelmet);
		CraftData.AddToInventory(TechType.RadiationGloves);
		CraftData.AddToInventory(TechType.RadiationSuit);
	}

	private void OnConsoleCommand_charge(NotificationCenter.Notification n)
	{
		if (n == null || n.data == null)
		{
			return;
		}
		if (DevConsole.ParseFloat(n, 0, out var value, 1f))
		{
			value = Mathf.Clamp01(value);
		}
		foreach (InventoryItem item2 in (IEnumerable<InventoryItem>)Inventory.main.container)
		{
			if (item2 == null)
			{
				continue;
			}
			Pickupable item = item2.item;
			if (item != null)
			{
				IBattery component = item.GetComponent<IBattery>();
				if (component != null)
				{
					component.charge = value * component.capacity;
				}
			}
		}
	}

	private void OnConsoleCommand_clearinventory(NotificationCenter.Notification n)
	{
		Inventory.main.container.Clear();
		Inventory.main.equipment.ClearItems();
	}

	private void OnConsoleCommand_resizestorage(NotificationCenter.Notification n)
	{
		Inventory main = Inventory.main;
		int usedStorageCount = main.GetUsedStorageCount();
		int result;
		int result2;
		if (usedStorageCount == 0)
		{
			ErrorMessage.AddDebug("Inventory.usedStorage is empty. Nothing to resize");
		}
		else if (n != null && n.data != null && n.data.Count == 2 && int.TryParse((string)n.data[0], out result) && int.TryParse((string)n.data[1], out result2) && result >= 1 && result2 >= 1)
		{
			for (int i = 0; i < usedStorageCount; i++)
			{
				if (main.GetUsedStorage(i) is ItemsContainer itemsContainer)
				{
					itemsContainer.Resize(result, result2);
				}
			}
		}
		else
		{
			ErrorMessage.AddDebug("Usage: 'resizestorage width height'");
		}
	}
}
