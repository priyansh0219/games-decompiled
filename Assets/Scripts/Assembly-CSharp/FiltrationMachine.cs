using System;
using System.Collections;
using ProtoBuf;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[ProtoContract]
public class FiltrationMachine : MonoBehaviour, IBaseModule, IProtoEventListener
{
	private const float spawnWaterInterval = 840f;

	private const float spawnSaltInterval = 420f;

	private const float powerPerSecond = 0.85f;

	private const float filterInterval = 1f;

	[AssertLocalization(2)]
	private const string filtrationProgressFormat = "FiltrationProgress";

	[AssertLocalization]
	private const string filtrationCompleteMessage = "FiltrationComplete";

	[AssertLocalization]
	private const string unpoweredMessage = "unpowered";

	[AssertLocalization]
	private const string usePrompt = "UseFiltrationMachine";

	public StorageContainer storageContainer;

	[SerializeField]
	[AssertNotNull]
	private AssetReferenceGameObject waterPrefabReference;

	[SerializeField]
	[AssertNotNull]
	private AssetReferenceGameObject saltPrefabReference;

	private AsyncOperationHandle<GameObject> waterPrefabHandle;

	private AsyncOperationHandle<GameObject> saltPrefabHandle;

	private GameObject waterPrefab;

	private GameObject saltPrefab;

	public int maxWater = 2;

	public int maxSalt = 2;

	public float atmosphericWaterScalar;

	public float atmosphericSaltScalar;

	private bool fastFiltering;

	private PowerRelay powerRelay;

	private Base baseComp;

	private const int currentVersion = 2;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 2;

	[NonSerialized]
	[ProtoMember(2)]
	public float timeRemainingWater = -1f;

	[NonSerialized]
	[ProtoMember(3)]
	public float timeRemainingSalt = -1f;

	[NonSerialized]
	[ProtoMember(4)]
	public Base.Face _moduleFace;

	[NonSerialized]
	[ProtoMember(5)]
	public float _constructed = 1f;

	public bool producingWater
	{
		get
		{
			if (_constructed >= 1f && timeRemainingWater >= 0f)
			{
				return powerRelay.IsPowered();
			}
			return false;
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

	private void OnEnable()
	{
		storageContainer.enabled = true;
		storageContainer.container.onAddItem += AddItem;
		storageContainer.container.onRemoveItem += RemoveItem;
		storageContainer.container.isAllowedToAdd = IsAllowedToAdd;
	}

	private void OnDisable()
	{
		storageContainer.container.onAddItem -= AddItem;
		storageContainer.container.onRemoveItem -= RemoveItem;
		storageContainer.container.isAllowedToAdd = null;
		storageContainer.enabled = false;
	}

	private IEnumerator Start()
	{
		DevConsole.RegisterConsoleCommand(this, "filterwater");
		DevConsole.RegisterConsoleCommand(this, "filtersalt");
		DevConsole.RegisterConsoleCommand(this, "filterfast");
		baseComp = GetComponentInParent<Base>();
		powerRelay = base.gameObject.GetComponentInParent<PowerRelay>();
		waterPrefabHandle = AddressablesUtility.LoadAsync<GameObject>(waterPrefabReference.RuntimeKey);
		yield return waterPrefabHandle;
		waterPrefab = waterPrefabHandle.Result;
		saltPrefabHandle = AddressablesUtility.LoadAsync<GameObject>(saltPrefabReference.RuntimeKey);
		yield return saltPrefabHandle;
		saltPrefab = saltPrefabHandle.Result;
		Invoke("DelayedStart", 0.5f);
		InvokeRepeating("UpdateFiltering", 1f, 1f);
	}

	private void OnDestroy()
	{
		AddressablesUtility.QueueRelease(ref waterPrefabHandle);
		AddressablesUtility.QueueRelease(ref saltPrefabHandle);
	}

	public void OnHover(HandTargetEventData eventData)
	{
		if (!(constructed < 1f))
		{
			string text = Language.main.Get("FiltrationComplete");
			if (GameModeUtils.RequiresPower() && powerRelay.GetPower() < 0.85f)
			{
				text = Language.main.Get("unpowered");
			}
			else if (timeRemainingWater >= 0f || timeRemainingSalt >= 0f)
			{
				float arg = 1f - Mathf.Clamp01(timeRemainingWater / 840f);
				float arg2 = 1f - Mathf.Clamp01(timeRemainingSalt / 420f);
				text = Language.main.GetFormat("FiltrationProgress", arg, arg2);
			}
			HandReticle.main.SetText(HandReticle.TextType.Hand, "UseFiltrationMachine", translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, text, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Interact);
		}
	}

	public void OnUse(BaseFiltrationMachineGeometry model)
	{
		if (!(constructed < 1f))
		{
			storageContainer.Open(model.spawnPoint);
		}
	}

	private BaseFiltrationMachineGeometry GetModel()
	{
		if (baseComp == null)
		{
			baseComp = GetComponentInParent<Base>();
		}
		if (baseComp != null)
		{
			IBaseModuleGeometry moduleGeometry = baseComp.GetModuleGeometry(moduleFace);
			if (moduleGeometry != null)
			{
				return moduleGeometry as BaseFiltrationMachineGeometry;
			}
		}
		return null;
	}

	private void DelayedStart()
	{
		TryFilterWater();
		TryFilterSalt();
	}

	private bool IsUnderwater()
	{
		return base.transform.position.y < -1f;
	}

	private void UpdateFiltering()
	{
		if (_constructed < 1f)
		{
			return;
		}
		if (timeRemainingWater > 0f || timeRemainingSalt > 0f)
		{
			PowerRelay powerRelay = this.powerRelay;
			float num = 1f * DayNightCycle.main.dayNightSpeed;
			if (!GameModeUtils.RequiresPower() || (powerRelay != null && powerRelay.GetPower() >= 0.85f * num))
			{
				if (GameModeUtils.RequiresPower())
				{
					powerRelay.ConsumeEnergy(0.85f * num, out var _);
				}
				if (timeRemainingWater > 0f)
				{
					float num2 = num;
					if (fastFiltering)
					{
						num2 *= 80f;
					}
					if (!IsUnderwater())
					{
						num2 *= atmosphericWaterScalar;
					}
					if (num2 > 0f)
					{
						timeRemainingWater = Mathf.Max(0f, timeRemainingWater - num2);
					}
					if (timeRemainingWater == 0f)
					{
						timeRemainingWater = -1f;
						Spawn(waterPrefab);
						TryFilterWater();
					}
				}
				if (timeRemainingSalt > 0f)
				{
					float num3 = num;
					if (fastFiltering)
					{
						num3 *= 80f;
					}
					if (!IsUnderwater())
					{
						num3 *= atmosphericSaltScalar;
					}
					if (num3 > 0f)
					{
						timeRemainingSalt = Mathf.Max(0f, timeRemainingSalt - num3);
					}
					if (timeRemainingSalt == 0f)
					{
						timeRemainingSalt = -1f;
						Spawn(saltPrefab);
						TryFilterSalt();
					}
				}
			}
		}
		BaseFiltrationMachineGeometry model = GetModel();
		if (model != null)
		{
			model.SetDirty();
		}
	}

	private bool Spawn(GameObject prefab)
	{
		Vector2int itemSize = TechData.GetItemSize(prefab.GetComponent<Pickupable>().GetTechType());
		if (!storageContainer.container.HasRoomFor(itemSize.x, itemSize.y))
		{
			Debug.Log("no room in filtration machine!");
			return false;
		}
		Pickupable component = UnityEngine.Object.Instantiate(prefab.gameObject).GetComponent<Pickupable>();
		component.Pickup(events: false);
		InventoryItem item = new InventoryItem(component);
		storageContainer.container.UnsafeAdd(item);
		return true;
	}

	private void TryFilterWater()
	{
		if (!(timeRemainingWater > 0f) && (IsUnderwater() || atmosphericWaterScalar != 0f) && storageContainer.container.GetCount(TechType.BigFilteredWater) < maxWater)
		{
			timeRemainingWater = 840f;
		}
	}

	private void TryFilterSalt()
	{
		if (!(timeRemainingSalt > 0f) && (IsUnderwater() || atmosphericSaltScalar != 0f) && storageContainer.container.GetCount(TechType.Salt) < maxSalt)
		{
			timeRemainingSalt = 420f;
		}
	}

	private void AddItem(InventoryItem item)
	{
		BaseFiltrationMachineGeometry model = GetModel();
		if (model != null)
		{
			model.SetDirty();
		}
	}

	private void RemoveItem(InventoryItem item)
	{
		TryFilterWater();
		TryFilterSalt();
		BaseFiltrationMachineGeometry model = GetModel();
		if (model != null)
		{
			model.SetDirty();
		}
	}

	private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
	{
		return false;
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		if (version >= 2)
		{
			return;
		}
		version = 2;
		Constructable component = GetComponent<Constructable>();
		if (component != null)
		{
			constructed = component.amount;
			UnityEngine.Object.Destroy(component);
		}
		Base componentInParent = GetComponentInParent<Base>();
		if (componentInParent != null)
		{
			Vector3 point = base.transform.position + base.transform.right * Base.cellSize.x;
			Int3 cell = componentInParent.WorldToGrid(point);
			Base.Direction[] horizontalDirections = Base.HorizontalDirections;
			foreach (Base.Direction direction in horizontalDirections)
			{
				Base.Face face = new Base.Face(cell, direction);
				if (componentInParent.GetFaceRaw(face) == Base.FaceType.FiltrationMachine)
				{
					face.cell -= componentInParent.GetAnchor();
					_moduleFace = face;
					return;
				}
			}
		}
		Debug.LogError("Failed to upgrade savegame data. FiltrationMachine IBaseModule is not found", this);
	}

	private void OnConsoleCommand_filterfast()
	{
		fastFiltering = !fastFiltering;
		ErrorMessage.AddDebug("fast filtering " + fastFiltering);
	}

	private void OnConsoleCommand_filterwater()
	{
		if (storageContainer.container.GetCount(TechType.BigFilteredWater) < maxWater)
		{
			timeRemainingWater = -1f;
			Spawn(waterPrefab);
			TryFilterWater();
			ErrorMessage.AddDebug("filtered water, water amount" + storageContainer.container.GetCount(TechType.BigFilteredWater));
		}
	}

	private void OnConsoleCommand_filtersalt()
	{
		if (storageContainer.container.GetCount(TechType.Salt) < maxSalt)
		{
			timeRemainingSalt = -1f;
			Spawn(saltPrefab);
			TryFilterSalt();
			ErrorMessage.AddDebug("filtered salt, salt amount:" + storageContainer.container.GetCount(TechType.Salt));
		}
	}
}
