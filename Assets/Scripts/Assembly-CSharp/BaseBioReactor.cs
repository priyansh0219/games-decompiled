using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class BaseBioReactor : MonoBehaviour, IBaseModule, IProtoEventListenerAsync, IProtoTreeEventListener
{
	private const float powerPerSecond = 5f / 6f;

	private const float chargeMultiplier = 7f;

	private static readonly Dictionary<TechType, float> charge = new Dictionary<TechType, float>(TechTypeExtensions.sTechTypeComparer)
	{
		{
			TechType.Melon,
			420f
		},
		{
			TechType.PurpleVegetable,
			140f
		},
		{
			TechType.HangingFruit,
			210f
		},
		{
			TechType.SmallMelon,
			280f
		},
		{
			TechType.JellyPlant,
			245f
		},
		{
			TechType.BulboTreePiece,
			420f
		},
		{
			TechType.KooshChunk,
			420f
		},
		{
			TechType.CreepvinePiece,
			210f
		},
		{
			TechType.WhiteMushroom,
			210f
		},
		{
			TechType.AcidMushroom,
			210f
		},
		{
			TechType.SmallFan,
			70f
		},
		{
			TechType.PurpleRattle,
			140f
		},
		{
			TechType.PinkMushroom,
			105f
		},
		{
			TechType.BloodVine,
			70f
		},
		{
			TechType.BluePalm,
			70f
		},
		{
			TechType.BulboTree,
			70f
		},
		{
			TechType.Creepvine,
			70f
		},
		{
			TechType.EyesPlant,
			70f
		},
		{
			TechType.FernPalm,
			70f
		},
		{
			TechType.GabeSFeather,
			70f
		},
		{
			TechType.HangingFruitTree,
			70f
		},
		{
			TechType.SmallKoosh,
			70f
		},
		{
			TechType.MembrainTree,
			70f
		},
		{
			TechType.OrangeMushroom,
			70f
		},
		{
			TechType.OrangePetalsPlant,
			140f
		},
		{
			TechType.PinkFlower,
			70f
		},
		{
			TechType.PurpleBrainCoral,
			70f
		},
		{
			TechType.PurpleBranches,
			70f
		},
		{
			TechType.PurpleFan,
			70f
		},
		{
			TechType.PurpleStalk,
			70f
		},
		{
			TechType.PurpleTentacle,
			70f
		},
		{
			TechType.PurpleVasePlant,
			70f
		},
		{
			TechType.PurpleVegetablePlant,
			70f
		},
		{
			TechType.RedBasketPlant,
			70f
		},
		{
			TechType.RedBush,
			70f
		},
		{
			TechType.RedConePlant,
			70f
		},
		{
			TechType.RedGreenTentacle,
			70f
		},
		{
			TechType.RedRollPlant,
			70f
		},
		{
			TechType.SeaCrown,
			70f
		},
		{
			TechType.ShellGrass,
			70f
		},
		{
			TechType.SnakeMushroom,
			70f
		},
		{
			TechType.SpikePlant,
			70f
		},
		{
			TechType.SpottedLeavesPlant,
			70f
		},
		{
			TechType.JeweledDiskPiece,
			70f
		},
		{
			TechType.CoralChunk,
			70f
		},
		{
			TechType.StalkerTooth,
			70f
		},
		{
			TechType.TreeMushroomPiece,
			70f
		},
		{
			TechType.OrangeMushroomSpore,
			140f
		},
		{
			TechType.PurpleVasePlantSeed,
			140f
		},
		{
			TechType.AcidMushroomSpore,
			21f
		},
		{
			TechType.WhiteMushroomSpore,
			21f
		},
		{
			TechType.PinkMushroomSpore,
			14f
		},
		{
			TechType.PurpleRattleSpore,
			14f
		},
		{
			TechType.MelonSeed,
			70f
		},
		{
			TechType.PurpleBrainCoralPiece,
			70f
		},
		{
			TechType.SpikePlantSeed,
			70f
		},
		{
			TechType.BluePalmSeed,
			70f
		},
		{
			TechType.PurpleFanSeed,
			21f
		},
		{
			TechType.SmallFanSeed,
			28f
		},
		{
			TechType.PurpleTentacleSeed,
			70f
		},
		{
			TechType.JellyPlantSeed,
			7f
		},
		{
			TechType.GabeSFeatherSeed,
			70f
		},
		{
			TechType.SeaCrownSeed,
			70f
		},
		{
			TechType.MembrainTreeSeed,
			70f
		},
		{
			TechType.PinkFlowerSeed,
			28f
		},
		{
			TechType.FernPalmSeed,
			70f
		},
		{
			TechType.OrangePetalsPlantSeed,
			105f
		},
		{
			TechType.EyesPlantSeed,
			70f
		},
		{
			TechType.RedGreenTentacleSeed,
			70f
		},
		{
			TechType.PurpleStalkSeed,
			70f
		},
		{
			TechType.RedBasketPlantSeed,
			70f
		},
		{
			TechType.RedBushSeed,
			70f
		},
		{
			TechType.RedConePlantSeed,
			70f
		},
		{
			TechType.SpottedLeavesPlantSeed,
			70f
		},
		{
			TechType.RedRollPlantSeed,
			70f
		},
		{
			TechType.PurpleBranchesSeed,
			70f
		},
		{
			TechType.SnakeMushroomSpore,
			140f
		},
		{
			TechType.CreepvineSeedCluster,
			70f
		},
		{
			TechType.BloodOil,
			420f
		},
		{
			TechType.Bladderfish,
			210f
		},
		{
			TechType.Boomerang,
			280f
		},
		{
			TechType.LavaBoomerang,
			280f
		},
		{
			TechType.Eyeye,
			420f
		},
		{
			TechType.LavaEyeye,
			420f
		},
		{
			TechType.GarryFish,
			420f
		},
		{
			TechType.HoleFish,
			280f
		},
		{
			TechType.Hoopfish,
			210f
		},
		{
			TechType.Spinefish,
			210f
		},
		{
			TechType.Hoverfish,
			350f
		},
		{
			TechType.Oculus,
			630f
		},
		{
			TechType.Peeper,
			420f
		},
		{
			TechType.Reginald,
			490f
		},
		{
			TechType.Spadefish,
			420f
		},
		{
			TechType.CookedBladderfish,
			157.5f
		},
		{
			TechType.CookedBoomerang,
			210f
		},
		{
			TechType.CookedLavaBoomerang,
			210f
		},
		{
			TechType.CookedEyeye,
			315f
		},
		{
			TechType.CookedLavaEyeye,
			315f
		},
		{
			TechType.CookedGarryFish,
			245f
		},
		{
			TechType.CookedHoleFish,
			210f
		},
		{
			TechType.CookedHoopfish,
			157.5f
		},
		{
			TechType.CookedSpinefish,
			157.5f
		},
		{
			TechType.CookedHoverfish,
			262.5f
		},
		{
			TechType.CookedOculus,
			472.5f
		},
		{
			TechType.CookedPeeper,
			315f
		},
		{
			TechType.CookedReginald,
			367.5f
		},
		{
			TechType.CookedSpadefish,
			315f
		},
		{
			TechType.CuredBladderfish,
			119f
		},
		{
			TechType.CuredBoomerang,
			157.5f
		},
		{
			TechType.CuredLavaBoomerang,
			157.5f
		},
		{
			TechType.CuredEyeye,
			245f
		},
		{
			TechType.CuredLavaEyeye,
			245f
		},
		{
			TechType.CuredGarryFish,
			182f
		},
		{
			TechType.CuredHoleFish,
			157.5f
		},
		{
			TechType.CuredHoopfish,
			119f
		},
		{
			TechType.CuredSpinefish,
			119f
		},
		{
			TechType.CuredHoverfish,
			196f
		},
		{
			TechType.CuredOculus,
			353.5f
		},
		{
			TechType.CuredPeeper,
			238f
		},
		{
			TechType.CuredReginald,
			273f
		},
		{
			TechType.CuredSpadefish,
			238f
		},
		{
			TechType.Jumper,
			280f
		},
		{
			TechType.RabbitRay,
			420f
		},
		{
			TechType.Stalker,
			560f
		},
		{
			TechType.Jellyray,
			350f
		},
		{
			TechType.Gasopod,
			700f
		},
		{
			TechType.Sandshark,
			630f
		},
		{
			TechType.BoneShark,
			630f
		},
		{
			TechType.CrabSquid,
			770f
		},
		{
			TechType.Mesmer,
			560f
		},
		{
			TechType.Biter,
			140f
		},
		{
			TechType.Crabsnake,
			700f
		},
		{
			TechType.Shocker,
			770f
		},
		{
			TechType.Shuttlebug,
			280f
		},
		{
			TechType.LavaLizard,
			560f
		},
		{
			TechType.Reefback,
			840f
		},
		{
			TechType.Crash,
			560f
		},
		{
			TechType.SafeShallowsEgg,
			350f
		},
		{
			TechType.KelpForestEgg,
			350f
		},
		{
			TechType.GrassyPlateausEgg,
			350f
		},
		{
			TechType.GrandReefsEgg,
			350f
		},
		{
			TechType.MushroomForestEgg,
			350f
		},
		{
			TechType.KooshZoneEgg,
			350f
		},
		{
			TechType.TwistyBridgesEgg,
			350f
		},
		{
			TechType.StalkerEgg,
			105f
		},
		{
			TechType.StalkerEggUndiscovered,
			105f
		},
		{
			TechType.ReefbackEgg,
			280f
		},
		{
			TechType.ReefbackEggUndiscovered,
			280f
		},
		{
			TechType.SpadefishEgg,
			140f
		},
		{
			TechType.SpadefishEggUndiscovered,
			140f
		},
		{
			TechType.RabbitrayEgg,
			140f
		},
		{
			TechType.RabbitrayEggUndiscovered,
			140f
		},
		{
			TechType.MesmerEgg,
			175f
		},
		{
			TechType.MesmerEggUndiscovered,
			175f
		},
		{
			TechType.JumperEgg,
			105f
		},
		{
			TechType.JumperEggUndiscovered,
			105f
		},
		{
			TechType.SandsharkEgg,
			210f
		},
		{
			TechType.SandsharkEggUndiscovered,
			210f
		},
		{
			TechType.JellyrayEgg,
			119f
		},
		{
			TechType.JellyrayEggUndiscovered,
			119f
		},
		{
			TechType.BonesharkEgg,
			210f
		},
		{
			TechType.BonesharkEggUndiscovered,
			210f
		},
		{
			TechType.CrabsnakeEgg,
			231f
		},
		{
			TechType.CrabsnakeEggUndiscovered,
			231f
		},
		{
			TechType.ShockerEgg,
			259f
		},
		{
			TechType.ShockerEggUndiscovered,
			259f
		},
		{
			TechType.GasopodEgg,
			231f
		},
		{
			TechType.GasopodEggUndiscovered,
			231f
		},
		{
			TechType.CrashEgg,
			189f
		},
		{
			TechType.CrashEggUndiscovered,
			189f
		},
		{
			TechType.CutefishEgg,
			210f
		},
		{
			TechType.CutefishEggUndiscovered,
			210f
		},
		{
			TechType.CrabsquidEgg,
			259f
		},
		{
			TechType.CrabsquidEggUndiscovered,
			259f
		},
		{
			TechType.LavaLizardEgg,
			189f
		},
		{
			TechType.LavaLizardEggUndiscovered,
			189f
		},
		{
			TechType.Floater,
			50f
		},
		{
			TechType.Lubricant,
			20f
		},
		{
			TechType.SeaTreaderPoop,
			300f
		}
	};

	[AssertLocalization]
	private const string cantAddItem = "BaseBioReactorCantAddItem";

	[AssertLocalization]
	private const string cantRemoveItem = "BaseBioReactorCantRemoveItem";

	[AssertLocalization(2)]
	private const string useHandFormat = "UseBaseBioReactor";

	[AssertLocalization]
	private const string useHandTooltip = "Tooltip_UseBaseBioReactor";

	[AssertLocalization]
	private const string storageLabelKey = "BaseBioReactorStorageLabel";

	private const int _currentVersion = 3;

	[NonSerialized]
	[ProtoMember(1)]
	public int _protoVersion = 3;

	[NonSerialized]
	[ProtoMember(2)]
	public Base.Face _moduleFace;

	[NonSerialized]
	[ProtoMember(3)]
	public float _constructed = 1f;

	[NonSerialized]
	[Obsolete("Obsolete since v2")]
	[ProtoMember(4, OverwriteList = true)]
	public byte[] _serializedStorage;

	[NonSerialized]
	[ProtoMember(5)]
	public float _toConsume;

	[AssertNotNull]
	public ChildObjectIdentifier storageRoot;

	private ItemsContainer _container;

	private PowerRelay _powerRelay;

	private PowerSource _powerSource;

	private List<Pickupable> toRemove = new List<Pickupable>();

	public bool producingPower
	{
		get
		{
			if (_constructed >= 1f)
			{
				return container.count > 0;
			}
			return false;
		}
	}

	private ItemsContainer container
	{
		get
		{
			if (_container == null)
			{
				_container = new ItemsContainer(4, 4, storageRoot.transform, "BaseBioReactorStorageLabel", null);
				ItemsContainer itemsContainer = _container;
				itemsContainer.isAllowedToAdd = (IsAllowedToAdd)Delegate.Combine(itemsContainer.isAllowedToAdd, new IsAllowedToAdd(IsAllowedToAdd));
				ItemsContainer itemsContainer2 = _container;
				itemsContainer2.isAllowedToRemove = (IsAllowedToRemove)Delegate.Combine(itemsContainer2.isAllowedToRemove, new IsAllowedToRemove(IsAllowedToRemove));
				_container.onAddItem += OnAddItem;
				_container.onRemoveItem += OnRemoveItem;
			}
			return _container;
		}
	}

	public Base.Face moduleFace
	{
		get
		{
			return _moduleFace;
		}
		set
		{
			_moduleFace = value;
		}
	}

	public float constructed
	{
		get
		{
			return _constructed;
		}
		set
		{
			value = Mathf.Clamp01(value);
			if (_constructed != value)
			{
				_constructed = value;
				if (!(_constructed >= 1f) && _constructed <= 0f)
				{
					UnityEngine.Object.Destroy(base.gameObject);
				}
			}
		}
	}

	private void Start()
	{
		_powerRelay = base.gameObject.GetComponentInParent<PowerRelay>();
		if (_powerRelay == null)
		{
			Debug.LogError("BaseBioReactor could not find PowerRelay", this);
		}
		_powerSource = GetComponent<PowerSource>();
		if (_powerSource == null)
		{
			Debug.LogError("BaseBioReactor could not find PowerSource", this);
		}
	}

	private void Update()
	{
		if (!producingPower)
		{
			return;
		}
		float num = 5f / 6f * DayNightCycle.main.deltaTime;
		float num2 = _powerSource.maxPower - _powerSource.power;
		if (num2 > 0f)
		{
			if (num2 < num)
			{
				num = num2;
			}
			float amount = ProducePower(num);
			_powerSource.AddEnergy(amount, out var _);
		}
	}

	public void OnHover()
	{
		HandReticle main = HandReticle.main;
		main.SetText(HandReticle.TextType.Hand, Language.main.GetFormat("UseBaseBioReactor", Mathf.RoundToInt(_powerSource.GetPower()), Mathf.RoundToInt(_powerSource.GetMaxPower())), translate: false, GameInput.Button.LeftHand);
		main.SetText(HandReticle.TextType.HandSubscript, "Tooltip_UseBaseBioReactor", translate: true);
		main.SetIcon(HandReticle.IconType.Hand);
	}

	public void OnUse(BaseBioReactorGeometry model)
	{
		PDA pDA = Player.main.GetPDA();
		Inventory.main.SetUsedStorage(container);
		pDA.Open(PDATab.Inventory, model.storagePivot);
	}

	public static float GetCharge(TechType techType)
	{
		return charge.GetOrDefault(techType, -1f);
	}

	public static bool CanAdd(TechType techType)
	{
		return charge.ContainsKey(techType);
	}

	private void OnAddItem(InventoryItem item)
	{
		if (charge.ContainsKey(item.item.GetTechType()))
		{
			item.isEnabled = false;
			item.isBarVisible = false;
		}
		BaseBioReactorGeometry model = GetModel();
		if (model != null)
		{
			model.PlayHatchAnimation();
			model.SetState(producingPower);
		}
	}

	private void OnRemoveItem(InventoryItem item)
	{
		BaseBioReactorGeometry model = GetModel();
		if (model != null)
		{
			model.SetState(producingPower);
		}
	}

	private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
	{
		bool flag = false;
		if (pickupable != null)
		{
			TechType techType = pickupable.GetTechType();
			if (charge.ContainsKey(techType))
			{
				flag = true;
			}
		}
		if (!flag && verbose)
		{
			ErrorMessage.AddMessage(Language.main.Get("BaseBioReactorCantAddItem"));
		}
		return flag;
	}

	private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
	{
		bool flag = true;
		if (pickupable != null)
		{
			flag = !charge.ContainsKey(pickupable.GetTechType());
		}
		if (!flag && verbose)
		{
			ErrorMessage.AddMessage(Language.main.Get("BaseBioReactorCantRemoveItem"));
		}
		return flag;
	}

	private BaseBioReactorGeometry GetModel()
	{
		Base componentInParent = GetComponentInParent<Base>();
		if (componentInParent != null)
		{
			IBaseModuleGeometry moduleGeometry = componentInParent.GetModuleGeometry(moduleFace);
			if (moduleGeometry != null)
			{
				return moduleGeometry as BaseBioReactorGeometry;
			}
		}
		return null;
	}

	private float ProducePower(float requested)
	{
		float num = 0f;
		if (requested > 0f && container.count > 0)
		{
			_toConsume += requested;
			num = requested;
			foreach (InventoryItem item2 in (IEnumerable<InventoryItem>)container)
			{
				Pickupable item = item2.item;
				TechType techType = item.GetTechType();
				float value = 0f;
				if (charge.TryGetValue(techType, out value) && _toConsume >= value)
				{
					_toConsume -= value;
					toRemove.Add(item);
				}
			}
			for (int num2 = toRemove.Count - 1; num2 >= 0; num2--)
			{
				Pickupable pickupable = toRemove[num2];
				container.RemoveItem(pickupable, forced: true);
				UnityEngine.Object.Destroy(pickupable.gameObject);
			}
			toRemove.Clear();
			if (container.count == 0)
			{
				num -= _toConsume;
				_toConsume = 0f;
			}
		}
		return num;
	}

	public IEnumerator OnProtoDeserializeAsync(ProtobufSerializer serializer)
	{
		container.Clear();
		if (_protoVersion < 2)
		{
			if (_serializedStorage != null)
			{
				yield return StorageHelper.RestoreItemsAsync(serializer, _serializedStorage, container);
				_serializedStorage = null;
			}
			_protoVersion = 2;
		}
	}

	public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
	{
		StorageHelper.TransferItems(storageRoot.gameObject, container);
		if (_protoVersion < 3)
		{
			CoroutineHost.StartCoroutine(CleanUpDuplicatedStorage());
		}
	}

	private IEnumerator CleanUpDuplicatedStorage()
	{
		yield return StorageHelper.DestroyDuplicatedItems(storageRoot.transform.parent.gameObject);
		_protoVersion = Mathf.Max(_protoVersion, 3);
	}
}
