using System;
using System.Collections.Generic;
using ProtoBuf;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ProtoContract]
[ProtoInclude(100, typeof(BatteryCharger))]
[ProtoInclude(200, typeof(PowerCellCharger))]
public class Charger : MonoBehaviour, IProtoEventListener, IConstructable, IObstacle
{
	[Serializable]
	public struct SlotDefinition
	{
		public string id;

		public GameObject battery;

		public Image bar;

		public TextMeshProUGUI text;
	}

	protected const int currentVersion = 1;

	protected const float chargeAttemptInterval = 5f;

	[AssertNotNull]
	public ChildObjectIdentifier equipmentRoot;

	public List<SlotDefinition> slotDefinitions;

	[Range(0f, 0.01f)]
	public float chargeSpeed = 0.005f;

	[AssertNotNull]
	public GameObject ui;

	[AssertNotNull]
	public GameObject uiUnpowered;

	[AssertNotNull]
	public GameObject uiPowered;

	[AssertNotNull]
	public TextMeshProUGUI uiUnpoweredText;

	[AssertNotNull]
	public Animator animator;

	public string animatorOpenedStateName;

	public string animatorClosedStateName;

	public Color colorEmpty = new Color(1f, 0f, 0f, 1f);

	public Color colorHalf = new Color(1f, 1f, 0f, 1f);

	public Color colorFull = new Color(0f, 1f, 0f, 1f);

	public FMODAsset soundOpen;

	public FMODAsset soundClose;

	public FMOD_StudioEventEmitter soundCharge;

	[NonSerialized]
	[ProtoMember(1)]
	public int protoVersion = 1;

	[NonSerialized]
	[ProtoMember(2, OverwriteList = true)]
	public Dictionary<string, string> serializedSlots;

	protected Equipment equipment;

	protected Dictionary<string, SlotDefinition> slots;

	protected Dictionary<string, IBattery> batteries;

	protected bool opened;

	protected float nextChargeAttemptTimer;

	protected Sequence sequence = new Sequence();

	protected int animParamOpen;

	protected Dictionary<int, string> unpoweredNotifyStrings = new Dictionary<int, string>();

	protected virtual HashSet<TechType> allowedTech => null;

	protected virtual string labelInteract => string.Empty;

	protected virtual string labelStorage => string.Empty;

	protected virtual string labelIncompatibleItem => string.Empty;

	protected virtual string labelCantDeconstruct => string.Empty;

	protected virtual float animTimeOpen => 0f;

	private void Awake()
	{
		Initialize();
	}

	private void Start()
	{
		if (serializedSlots != null)
		{
			Dictionary<string, InventoryItem> items = StorageHelper.ScanItems(equipmentRoot.transform);
			equipment.RestoreEquipment(serializedSlots, items);
			serializedSlots = null;
			UnlockDefaultEquipmentSlots();
		}
		opened = HasChargables();
		animator.SetBool(animParamOpen, opened);
		animator.Play(opened ? animatorOpenedStateName : animatorClosedStateName);
		ToggleUI(opened);
	}

	private void Update()
	{
		sequence.Update();
		if (Time.deltaTime == 0f)
		{
			return;
		}
		if (nextChargeAttemptTimer > 0f)
		{
			nextChargeAttemptTimer -= DayNightCycle.main.deltaTime;
			if (nextChargeAttemptTimer < 0f)
			{
				nextChargeAttemptTimer = 0f;
			}
		}
		bool charging = false;
		if (nextChargeAttemptTimer <= 0f)
		{
			int num = 0;
			bool flag = false;
			PowerRelay powerRelay = PowerSource.FindRelay(base.transform);
			if (powerRelay != null)
			{
				float num2 = 0f;
				Dictionary<string, IBattery>.Enumerator enumerator = batteries.GetEnumerator();
				while (enumerator.MoveNext())
				{
					IBattery value = enumerator.Current.Value;
					if (value == null)
					{
						continue;
					}
					float charge = value.charge;
					float capacity = value.capacity;
					if (charge < capacity)
					{
						num++;
						float num3 = DayNightCycle.main.deltaTime * chargeSpeed * capacity;
						if (charge + num3 > capacity)
						{
							num3 = capacity - charge;
						}
						num2 += num3;
					}
				}
				float amountConsumed = 0f;
				if (num2 > 0f && powerRelay.GetPower() > num2)
				{
					flag = true;
					powerRelay.ConsumeEnergy(num2, out amountConsumed);
				}
				if (amountConsumed > 0f)
				{
					charging = true;
					float num4 = amountConsumed / (float)num;
					Dictionary<string, IBattery>.Enumerator enumerator2 = batteries.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						KeyValuePair<string, IBattery> current = enumerator2.Current;
						string key = current.Key;
						IBattery value2 = current.Value;
						if (value2 == null)
						{
							continue;
						}
						float charge2 = value2.charge;
						float capacity2 = value2.capacity;
						if (charge2 < capacity2)
						{
							float num5 = num4;
							float num6 = capacity2 - charge2;
							if (num5 > num6)
							{
								num5 = num6;
							}
							value2.charge += num5;
							if (slots.TryGetValue(key, out var value3))
							{
								UpdateVisuals(value3, value2.charge / value2.capacity);
							}
						}
					}
				}
			}
			if (num == 0 || !flag)
			{
				nextChargeAttemptTimer = 5f;
			}
			ToggleUIPowered(num == 0 || flag);
		}
		if (nextChargeAttemptTimer >= 0f)
		{
			int num7 = Mathf.CeilToInt(nextChargeAttemptTimer);
			string value4 = null;
			if (!unpoweredNotifyStrings.TryGetValue(num7, out value4))
			{
				value4 = Language.main.GetFormat("ChargerInsufficientPower", num7);
				unpoweredNotifyStrings.Add(num7, value4);
			}
			uiUnpoweredText.text = value4;
		}
		ToggleChargeSound(charging);
	}

	public void OnHandHover(HandTargetEventData eventData)
	{
		if (base.enabled)
		{
			HandReticle main = HandReticle.main;
			main.SetIcon(HandReticle.IconType.Hand);
			main.SetText(HandReticle.TextType.Hand, labelInteract, translate: true, GameInput.Button.LeftHand);
			main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		}
	}

	public void OnHandClick(HandTargetEventData eventData)
	{
		if (base.enabled && !sequence.active)
		{
			eventData.guiHand.player.GetPDA();
			if (opened)
			{
				OpenPDA();
				return;
			}
			opened = true;
			OnOpen();
			UpdateVisuals();
			sequence.Set(animTimeOpen, target: true, OpenPDA);
		}
	}

	protected virtual bool Initialize()
	{
		if (equipment == null)
		{
			equipment = new Equipment(base.gameObject, equipmentRoot.transform);
			equipment.SetLabel(labelStorage);
			equipment.isAllowedToAdd = IsAllowedToAdd;
			equipment.onEquip += OnEquip;
			equipment.onUnequip += OnUnequip;
			batteries = new Dictionary<string, IBattery>();
			slots = new Dictionary<string, SlotDefinition>();
			int i = 0;
			for (int count = slotDefinitions.Count; i < count; i++)
			{
				SlotDefinition value = slotDefinitions[i];
				string id = value.id;
				if (!string.IsNullOrEmpty(id) && !batteries.ContainsKey(id))
				{
					batteries[id] = null;
					slots[id] = value;
					Image bar = value.bar;
					if (bar != null)
					{
						bar.material = new Material(bar.material);
					}
				}
			}
			UnlockDefaultEquipmentSlots();
			UpdateVisuals();
			return true;
		}
		return false;
	}

	protected void UnlockDefaultEquipmentSlots()
	{
		equipment.AddSlots(slots.Keys);
	}

	protected bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
	{
		if (pickupable != null)
		{
			TechType techType = pickupable.GetTechType();
			if (allowedTech != null && allowedTech.Contains(techType))
			{
				return true;
			}
			if (verbose)
			{
				ErrorMessage.AddMessage(Language.main.Get(labelIncompatibleItem));
			}
			return false;
		}
		return false;
	}

	protected void OnEquip(string slot, InventoryItem item)
	{
		nextChargeAttemptTimer = 0f;
		if (item != null)
		{
			Pickupable item2 = item.item;
			if (item2 != null)
			{
				IBattery component = item2.GetComponent<IBattery>();
				if (component != null && batteries.ContainsKey(slot))
				{
					batteries[slot] = component;
				}
			}
		}
		if (!slots.TryGetValue(slot, out var value))
		{
			return;
		}
		GameObject battery = value.battery;
		if (battery != null)
		{
			battery.SetActive(value: true);
		}
		if (item == null)
		{
			return;
		}
		Pickupable item3 = item.item;
		if (item3 != null)
		{
			IBattery component2 = item3.GetComponent<IBattery>();
			if (component2 != null)
			{
				UpdateVisuals(value, component2.charge / component2.capacity);
			}
		}
	}

	protected void OnUnequip(string slot, InventoryItem item)
	{
		if (batteries.ContainsKey(slot))
		{
			batteries[slot] = null;
		}
		if (slots.TryGetValue(slot, out var value))
		{
			UpdateVisuals(value, -1f);
		}
	}

	protected bool HasChargables()
	{
		Dictionary<string, IBattery>.Enumerator enumerator = batteries.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Value != null)
			{
				return true;
			}
		}
		return false;
	}

	protected void UpdateVisuals()
	{
		Dictionary<string, SlotDefinition>.Enumerator enumerator = slots.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, SlotDefinition> current = enumerator.Current;
			string key = current.Key;
			InventoryItem itemInSlot = equipment.GetItemInSlot(key);
			SlotDefinition value = current.Value;
			float n = -1f;
			if (itemInSlot != null)
			{
				Pickupable item = itemInSlot.item;
				if (item != null)
				{
					IBattery component = item.GetComponent<IBattery>();
					if (component != null)
					{
						n = component.charge / component.capacity;
					}
				}
			}
			UpdateVisuals(value, n);
		}
	}

	protected void UpdateVisuals(SlotDefinition definition, float n)
	{
		GameObject battery = definition.battery;
		if (battery != null)
		{
			battery.SetActive(n >= 0f);
		}
		TextMeshProUGUI text = definition.text;
		if (text != null)
		{
			text.text = ((n >= 0f) ? $"{n:P0}" : Language.main.Get("ChargerSlotEmpty"));
		}
		Image bar = definition.bar;
		if (bar != null)
		{
			Material material = bar.material;
			if (n >= 0f)
			{
				Color value = ((n < 0.5f) ? Color.Lerp(colorEmpty, colorHalf, 2f * n) : Color.Lerp(colorHalf, colorFull, 2f * n - 1f));
				material.SetColor(ShaderPropertyID._Color, value);
				material.SetFloat(ShaderPropertyID._Amount, n);
			}
			else
			{
				material.SetColor(ShaderPropertyID._Color, colorEmpty);
				material.SetFloat(ShaderPropertyID._Amount, 0f);
			}
		}
	}

	protected void ToggleChargeSound(bool charging)
	{
		if (!(soundCharge != null))
		{
			return;
		}
		bool isStartingOrPlaying = soundCharge.GetIsStartingOrPlaying();
		if (charging)
		{
			if (!isStartingOrPlaying)
			{
				soundCharge.StartEvent();
			}
		}
		else if (isStartingOrPlaying)
		{
			soundCharge.Stop();
		}
	}

	protected void ToggleUI(bool active)
	{
		ui.SetActive(active);
	}

	protected void ToggleUIPowered(bool powered)
	{
		uiPowered.SetActive(powered);
		uiUnpowered.SetActive(!powered);
	}

	protected void OnOpen()
	{
		animator.SetBool(animParamOpen, value: true);
		if (soundOpen != null)
		{
			FMODUWE.PlayOneShot(soundOpen, base.transform.position);
		}
		ToggleUI(active: true);
	}

	protected void OnCloseCallback(PDA pda)
	{
		if (!HasChargables())
		{
			opened = false;
			sequence.Reset();
			animator.SetBool(animParamOpen, value: false);
			if (soundClose != null)
			{
				FMODUWE.PlayOneShot(soundClose, base.transform.position);
			}
			ToggleUI(active: false);
		}
	}

	protected void OpenPDA()
	{
		sequence.Reset();
		PDA pDA = Player.main.GetPDA();
		if (!pDA.isInUse)
		{
			Inventory.main.SetUsedStorage(equipment);
			pDA.Open(PDATab.Inventory, base.transform, OnCloseCallback);
		}
	}

	public bool IsDeconstructionObstacle()
	{
		return true;
	}

	public bool CanDeconstruct(out string reason)
	{
		bool flag = !HasChargables();
		reason = (flag ? null : Language.main.Get(labelCantDeconstruct));
		return flag;
	}

	public void OnConstructedChanged(bool constructed)
	{
		if (!constructed)
		{
			opened = false;
			ToggleUI(active: false);
			ToggleChargeSound(charging: false);
		}
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		Initialize();
		serializedSlots = equipment.SaveEquipment();
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
	}
}
