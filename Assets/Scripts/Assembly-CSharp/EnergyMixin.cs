using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
[ProtoInclude(1000, typeof(BatterySource))]
public class EnergyMixin : MonoBehaviour, IProtoEventListener, ICraftTarget, IItemSelectorManager, IProtoTreeEventListener
{
	public delegate void OnPoweredChanged(bool powered);

	[Serializable]
	public struct BatteryModels
	{
		public GameObject model;

		public TechType techType;
	}

	[AssertNotNull]
	public ChildObjectIdentifier storageRoot;

	public TechType defaultBattery = TechType.Battery;

	[Range(0f, 1f)]
	public float defaultBatteryCharge = 1f;

	[AssertNotNull]
	public List<TechType> compatibleBatteries;

	public bool allowBatteryReplacement = true;

	public GameObject[] controlledObjects;

	public FMODAsset soundPowerUp;

	public FMODAsset soundPowerDown;

	public FMODAsset soundBatteryAdd;

	public FMODAsset soundBatteryRemove;

	public BatteryModels[] batteryModels;

	[NonSerialized]
	[ProtoMember(1)]
	public float energy = -1f;

	[NonSerialized]
	[ProtoMember(2)]
	public float maxEnergy = 100f;

	private bool _electronicsDisabled;

	private float enableElectonicsTime;

	protected StorageSlot batterySlot;

	protected IBattery battery;

	protected bool allowedToPlaySounds = true;

	[AssertLocalization]
	private const string powerSourceUnloaded = "PowerSourceUnloaded";

	[AssertLocalization(1)]
	private const string unloadPowerSourceFormat = "PowerSourceUnload";

	protected static Dictionary<TechType, List<InventoryItem>> sGroups = new Dictionary<TechType, List<InventoryItem>>(TechTypeExtensions.sTechTypeComparer);

	public float charge
	{
		get
		{
			float num = ((battery != null && !electronicsDisabled) ? battery.charge : 0f);
			if (!electronicsDisabled && Time.time < enableElectonicsTime + 2f)
			{
				num *= Mathf.InverseLerp(enableElectonicsTime, enableElectonicsTime + 2f, Time.time);
			}
			return num;
		}
	}

	public float capacity
	{
		get
		{
			if (battery == null)
			{
				return 0f;
			}
			return battery.capacity;
		}
	}

	public bool electronicsDisabled
	{
		get
		{
			return _electronicsDisabled;
		}
		protected set
		{
			if (value != _electronicsDisabled)
			{
				_electronicsDisabled = value;
				if (battery != null && battery.charge > 0f)
				{
					NotifyPowered(!_electronicsDisabled);
					PlayPowerSound(!_electronicsDisabled);
				}
			}
		}
	}

	public event OnPoweredChanged onPoweredChanged;

	protected virtual void Awake()
	{
		Initialize();
	}

	protected virtual void Start()
	{
		RestoreBattery();
	}

	private void Update()
	{
		if (electronicsDisabled && Time.time > enableElectonicsTime)
		{
			electronicsDisabled = false;
		}
	}

	public GameObject GetBatteryGameObject()
	{
		if (storageRoot != null)
		{
			Transform component = storageRoot.GetComponent<Transform>();
			int i = 0;
			for (int childCount = component.childCount; i < childCount; i++)
			{
				Transform child = component.GetChild(i);
				if (child.GetComponent<IBattery>() != null)
				{
					return child.gameObject;
				}
			}
		}
		return null;
	}

	public float GetBatteryChargeValue()
	{
		IBattery battery = GetBattery();
		if (battery != null && battery.capacity > 0f)
		{
			return Mathf.Clamp01(battery.charge / battery.capacity);
		}
		return 0f;
	}

	public IBattery GetBattery()
	{
		if (battery != null)
		{
			return battery;
		}
		if (storageRoot != null)
		{
			Transform component = storageRoot.GetComponent<Transform>();
			int i = 0;
			for (int childCount = component.childCount; i < childCount; i++)
			{
				if (component.GetChild(i).TryGetComponent<IBattery>(out var component2))
				{
					return component2;
				}
			}
		}
		return null;
	}

	public IEnumerator SetBatteryAsync(TechType techType, float normalizedCharge, TaskResult<InventoryItem> result)
	{
		Initialize();
		RestoreBattery();
		if (!compatibleBatteries.Contains(techType))
		{
			Debug.LogErrorFormat("Incompatible battery TechType.{0}", techType.AsString());
			yield break;
		}
		if (batterySlot == null)
		{
			Debug.LogError("batterySlot is null on EnergyMixin");
			yield break;
		}
		if (normalizedCharge < 0f)
		{
			normalizedCharge = 1f;
		}
		TaskResult<GameObject> batteryResult = new TaskResult<GameObject>();
		yield return SpawnBatteryAsync(techType, Mathf.Clamp01(normalizedCharge), batteryResult);
		GameObject gameObject = batteryResult.Get();
		if (gameObject == null)
		{
			Debug.LogErrorFormat("Failed to spawn battery GameObject for TechType.{0}", techType.AsString());
			yield break;
		}
		Pickupable component = gameObject.GetComponent<Pickupable>();
		if (component == null)
		{
			Debug.LogErrorFormat("No Pickupable component found on spawned Battery for TechType.{0}", techType.AsString());
			UnityEngine.Object.Destroy(gameObject);
			yield break;
		}
		InventoryItem inventoryItem = batterySlot.AddItem(component);
		result.Set(inventoryItem);
		if (inventoryItem == null)
		{
			Debug.LogError("Failed to add battery to EnergyMixin batterySlot");
			UnityEngine.Object.Destroy(gameObject);
		}
	}

	public IEnumerator SpawnDefaultAsync(float normalizedCharge, TaskResult<bool> result)
	{
		Initialize();
		RestoreBattery();
		result.Set(value: false);
		if (battery == null && defaultBattery != 0)
		{
			allowedToPlaySounds = false;
			TaskResult<InventoryItem> inventoryItemResult = new TaskResult<InventoryItem>();
			yield return SetBatteryAsync(defaultBattery, normalizedCharge, inventoryItemResult);
			if (inventoryItemResult.Get() != null)
			{
				result.Set(value: true);
			}
			allowedToPlaySounds = true;
		}
	}

	public bool HasItem()
	{
		if (batterySlot.storedItem != null)
		{
			return true;
		}
		return false;
	}

	public float ModifyCharge(float amount)
	{
		float num = 0f;
		if (GameModeUtils.RequiresPower() && !electronicsDisabled)
		{
			float num2 = charge;
			if (battery != null)
			{
				if (amount >= 0f)
				{
					num = Mathf.Min(amount, battery.capacity - battery.charge);
					battery.charge += num;
				}
				else
				{
					num = 0f - Mathf.Min(0f - amount, battery.charge);
					battery.charge += num;
				}
			}
			float num3 = charge;
			if (num2 == 0f && num3 > 0f)
			{
				NotifyPowered(powered: true);
				PlayPowerSound(powered: true);
			}
			else if (num2 > 0f && num3 == 0f)
			{
				NotifyPowered(powered: false);
				PlayPowerSound(powered: false);
			}
		}
		return num;
	}

	public bool IsDepleted()
	{
		return charge == 0f;
	}

	public bool AddEnergy(float amount)
	{
		return ModifyCharge(Mathf.Abs(amount)) > 0f;
	}

	public bool ConsumeEnergy(float amount)
	{
		return 0f - ModifyCharge(0f - Mathf.Abs(amount)) > 0f;
	}

	public float GetEnergyScalar()
	{
		if (battery == null)
		{
			return 0f;
		}
		return charge / capacity;
	}

	public void HandHover()
	{
		if (!allowBatteryReplacement)
		{
			return;
		}
		HandReticle main = HandReticle.main;
		string text = "";
		string text2 = "";
		InventoryItem storedItem = batterySlot.storedItem;
		if (storedItem != null)
		{
			Pickupable item = storedItem.item;
			text = Language.main.Get(item.GetTechName());
			IBattery component = item.GetComponent<IBattery>();
			if (component != null)
			{
				text2 = component.GetChargeValueText();
			}
		}
		else
		{
			text = Language.main.Get("PowerSourceUnloaded");
		}
		HandReticle.main.SetText(HandReticle.TextType.Hand, text, translate: false, GameInput.Button.LeftHand);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, text2, translate: false);
		main.SetIcon(HandReticle.IconType.Hand);
	}

	public void InitiateReload()
	{
		if (allowBatteryReplacement)
		{
			uGUI.main.itemSelector.Initialize(this, SpriteManager.Get(SpriteManager.Group.Item, "nobattery"), new List<IItemsContainer>
			{
				Inventory.main.container,
				batterySlot
			});
		}
	}

	protected void Initialize()
	{
		if (batterySlot == null)
		{
			batterySlot = new StorageSlot(storageRoot.transform, string.Empty);
			batterySlot.onAddItem += OnAddItem;
			batterySlot.onRemoveItem += OnRemoveItem;
		}
	}

	protected void RestoreBattery()
	{
		allowedToPlaySounds = false;
		batterySlot.Restore();
		if (battery == null)
		{
			NotifyHasBattery(batterySlot.storedItem);
		}
		allowedToPlaySounds = true;
	}

	protected IEnumerator SpawnBatteryAsync(TechType batteryTech, float normalizedCharge, TaskResult<GameObject> result)
	{
		CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(batteryTech);
		yield return request;
		GameObject result2 = request.GetResult();
		if (result2 != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(result2);
			IBattery component = gameObject.GetComponent<IBattery>();
			if (component != null)
			{
				component.charge = Mathf.Clamp01(normalizedCharge) * component.capacity;
				result.Set(gameObject);
			}
			else
			{
				UnityEngine.Object.Destroy(gameObject);
				Debug.LogError("Instantiated Battery prefab does not have a component which implements IBattery interface");
			}
		}
		else
		{
			Debug.LogErrorFormat("No prefab found for TechType.{0}", batteryTech);
		}
	}

	protected int CompareByCharge(InventoryItem item1, InventoryItem item2)
	{
		IBattery component = item1.item.GetComponent<IBattery>();
		IBattery component2 = item2.item.GetComponent<IBattery>();
		if (component != null && component2 != null)
		{
			float num = component.charge;
			float value = component2.charge;
			return -num.CompareTo(value);
		}
		if (component == null && component2 == null)
		{
			return 0;
		}
		if (component2 == null)
		{
			return -1;
		}
		return 1;
	}

	protected void OnAddItem(InventoryItem item)
	{
		battery = null;
		InventoryItem storedItem = batterySlot.storedItem;
		if (storedItem != null)
		{
			Pickupable item2 = storedItem.item;
			if (item2 != null)
			{
				battery = item2.GetComponent<IBattery>();
			}
		}
		NotifyHasBattery(item);
		PlayBatterySound(hasBattery: true);
		if (charge > 0f)
		{
			NotifyPowered(powered: true);
			PlayPowerSound(powered: true);
		}
	}

	protected void OnRemoveItem(InventoryItem item)
	{
		IBattery battery = this.battery;
		this.battery = null;
		NotifyHasBattery(null);
		PlayBatterySound(hasBattery: false);
		if (battery != null && battery.charge > 0f)
		{
			NotifyPowered(powered: false);
			PlayPowerSound(powered: false);
		}
	}

	protected void NotifyPowered(bool powered)
	{
		if (this.onPoweredChanged != null)
		{
			this.onPoweredChanged(powered);
		}
	}

	protected virtual void NotifyHasBattery(InventoryItem item)
	{
		bool flag = item != null && item.item != null;
		int i = 0;
		for (int num = controlledObjects.Length; i < num; i++)
		{
			GameObject gameObject = controlledObjects[i];
			if (gameObject != null)
			{
				gameObject.SetActive(flag);
			}
		}
		int j = 0;
		for (int num2 = this.batteryModels.Length; j < num2; j++)
		{
			BatteryModels batteryModels = this.batteryModels[j];
			batteryModels.model.SetActive(flag && batteryModels.techType == item.item.GetTechType());
		}
	}

	protected void PlayPowerSound(bool powered)
	{
		if (allowedToPlaySounds)
		{
			FMODAsset fMODAsset = (powered ? soundPowerUp : soundPowerDown);
			if (fMODAsset != null)
			{
				FMODUWE.PlayOneShot(fMODAsset, base.transform.position);
			}
		}
	}

	protected void PlayBatterySound(bool hasBattery)
	{
		if (allowedToPlaySounds)
		{
			FMODAsset fMODAsset = (hasBattery ? soundBatteryAdd : soundBatteryRemove);
			if (fMODAsset != null)
			{
				FMODUWE.PlayOneShot(fMODAsset, base.transform.position);
			}
		}
	}

	public void OnCraftEnd(TechType techType)
	{
		CoroutineHost.StartCoroutine(SpawnDefaultAsync(defaultBatteryCharge, DiscardTaskResult<bool>.Instance));
	}

	public bool Filter(InventoryItem item)
	{
		return compatibleBatteries.Contains(item.item.GetTechType());
	}

	public int Sort(List<InventoryItem> items)
	{
		InventoryItem storedItem = batterySlot.storedItem;
		bool flag = storedItem != null && items.Remove(storedItem);
		sGroups.Clear();
		int i = 0;
		for (int count = items.Count; i < count; i++)
		{
			InventoryItem inventoryItem = items[i];
			TechType techType = inventoryItem.item.GetTechType();
			if (!sGroups.TryGetValue(techType, out var value))
			{
				value = new List<InventoryItem>();
				sGroups.Add(techType, value);
			}
			value.Add(inventoryItem);
		}
		Dictionary<TechType, List<InventoryItem>>.Enumerator enumerator = sGroups.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Value.Sort(CompareByCharge);
		}
		items.Clear();
		if (flag)
		{
			items.Insert(0, storedItem);
		}
		int j = 0;
		for (int count2 = compatibleBatteries.Count; j < count2; j++)
		{
			TechType key = compatibleBatteries[j];
			if (sGroups.TryGetValue(key, out var value2))
			{
				items.AddRange(value2);
				sGroups.Remove(key);
			}
		}
		if (sGroups.Count > 0)
		{
			enumerator = sGroups.GetEnumerator();
			while (enumerator.MoveNext())
			{
				items.AddRange(enumerator.Current.Value);
			}
			sGroups.Clear();
		}
		if (!flag)
		{
			return -1;
		}
		return 0;
	}

	public string GetText(InventoryItem item)
	{
		if (item != null)
		{
			Pickupable item2 = item.item;
			string text = Language.main.Get(item2.GetTechName());
			IBattery component = item2.GetComponent<IBattery>();
			if (component != null)
			{
				return $"{text}\n{component.GetChargeValueText()}";
			}
			return text;
		}
		InventoryItem storedItem = batterySlot.storedItem;
		if (storedItem != null)
		{
			Pickupable item3 = storedItem.item;
			string arg = Language.main.Get(item3.GetTechName());
			return Language.main.GetFormat("PowerSourceUnload", arg);
		}
		return Language.main.Get("PowerSourceUnloaded");
	}

	public void Select(InventoryItem item)
	{
		InventoryItem storedItem = batterySlot.storedItem;
		if (storedItem != null)
		{
			if (item != null)
			{
				if (storedItem != item)
				{
					batterySlot.RemoveItem();
					batterySlot.AddItem(item);
					Inventory.main.ForcePickup(storedItem.item);
					Pickupable item2 = item.item;
					if (item2 != null)
					{
						uGUI_IconNotifier.main.Play(item2.GetTechType(), uGUI_IconNotifier.AnimationType.To);
					}
				}
			}
			else
			{
				batterySlot.RemoveItem();
				Inventory.main.ForcePickup(storedItem.item);
			}
		}
		else if (item != null)
		{
			batterySlot.AddItem(item);
			Pickupable item3 = item.item;
			if (item3 != null)
			{
				uGUI_IconNotifier.main.Play(item3.GetTechType(), uGUI_IconNotifier.AnimationType.To);
			}
		}
	}

	public void DisableElectronicsForTime(float time)
	{
		enableElectonicsTime = Mathf.Max(enableElectonicsTime, Time.time + time);
		electronicsDisabled = true;
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		Initialize();
		if (energy >= 0f)
		{
			StartCoroutine(SpawnDefaultBatteryAsync());
		}
	}

	private IEnumerator SpawnDefaultBatteryAsync()
	{
		TaskResult<bool> result = new TaskResult<bool>();
		yield return SpawnDefaultAsync(Mathf.Clamp01(energy / maxEnergy), result);
		if (result.Get())
		{
			energy = -1f;
		}
	}

	public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
	{
		Initialize();
		RestoreBattery();
	}
}
