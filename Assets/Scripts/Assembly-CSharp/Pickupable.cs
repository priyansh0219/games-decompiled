using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
[ProtoInclude(1010, typeof(CreepvineSeed))]
[DisallowMultipleComponent]
public class Pickupable : HandTarget, IProtoEventListenerAsync, IProtoTreeEventListener, IHandTarget, ILocalizationCheckable
{
	private static readonly Type[] dontDisableOnAttach = new Type[1] { typeof(EcoTarget) };

	private const int currentVersion = 4;

	[NonSerialized]
	[ProtoMember(1)]
	public TechType overrideTechType;

	[NonSerialized]
	[ProtoMember(2)]
	public bool overrideTechUsed;

	[NonSerialized]
	[ProtoMember(3)]
	public bool isLootCube;

	[ProtoMember(4)]
	public bool isPickupable = true;

	[ProtoMember(5)]
	public bool destroyOnDeath = true;

	[NonSerialized]
	[ProtoMember(6)]
	public bool _attached;

	[NonSerialized]
	[ProtoMember(7)]
	public bool _isInSub;

	[NonSerialized]
	[ProtoMember(8)]
	public int version;

	[NonSerialized]
	[ProtoMember(9)]
	public PickupableKinematicState isKinematic = PickupableKinematicState.NoKinematicStateSet;

	public bool randomizeRotationWhenDropped = true;

	[Tooltip("If this is true, isKinematic will be set to false when this is dropped, otherwise isKinematic will be set to true.")]
	public bool activateRigidbodyWhenDropped = true;

	[NonSerialized]
	public Event<Pickupable> pickedUpEvent = new Event<Pickupable>();

	[NonSerialized]
	public Event<Pickupable> droppedEvent = new Event<Pickupable>();

	public bool usePackUpIcon;

	private InventoryItem inventoryItem;

	private float timeDropped;

	private ItemPrefabData prefabData;

	private List<Behaviour> disabledBehaviours;

	private List<Collider> disabledColliders;

	private List<Rigidbody> disabledRigidbodies;

	public bool attached
	{
		get
		{
			return _attached;
		}
		set
		{
			if (_attached != value)
			{
				_attached = value;
				if (_attached)
				{
					pickedUpEvent.Trigger(this);
				}
			}
		}
	}

	public bool isInSub => _isInSub;

	public bool isDestroyed { get; private set; }

	public event OnTechTypeChanged onTechTypeChanged;

	public override void Awake()
	{
		base.Awake();
		prefabData = GetComponent<ItemPrefabData>();
	}

	public void SetInventoryItem(InventoryItem newInventoryItem)
	{
		if (inventoryItem != newInventoryItem)
		{
			if (inventoryItem != null)
			{
				inventoryItem.container?.RemoveItem(inventoryItem, forced: true, verbose: false);
			}
			inventoryItem = newInventoryItem;
			newInventoryItem?.SetTechType(GetTechType());
		}
	}

	public void SetTechTypeOverride(TechType techType, bool lootCube = false)
	{
		isLootCube = lootCube;
		ChangeTechTypeOverride(techType, useTechTypeOverride: true);
	}

	public void ResetTechTypeOverride()
	{
		ChangeTechTypeOverride(TechType.None, useTechTypeOverride: false);
	}

	private void ChangeTechTypeOverride(TechType techType, bool useTechTypeOverride)
	{
		TechType techType2 = GetTechType();
		overrideTechType = techType;
		overrideTechUsed = useTechTypeOverride;
		if (inventoryItem != null)
		{
			inventoryItem.SetTechType(GetTechType());
		}
		if (this.onTechTypeChanged != null)
		{
			this.onTechTypeChanged(this, techType2);
		}
	}

	public void PlayPickupSound()
	{
		FMODUWE.PlayOneShot(TechData.GetSoundPickup(GetTechType()), Player.main.transform.position);
	}

	public void PlayDropSound()
	{
		FMODUWE.PlayOneShot(TechData.GetSoundDrop(GetTechType()), Player.main.transform.position);
	}

	public void Pickup(bool events = true)
	{
		TechType techType = GetTechType();
		Initialize();
		if (events)
		{
			GoalManager.main.OnCustomGoalEvent($"Pickup_{techType.AsString()}");
			PlayPickupSound();
		}
	}

	public void Initialize()
	{
		SendMessage("OnExamine", SendMessageOptions.DontRequireReceiver);
		int num = 0;
		using (ListPool<Rigidbody> listPool = Pool<ListPool<Rigidbody>>.Get())
		{
			GetComponentsInChildren(includeInactive: true, listPool.list);
			num = listPool.list.Count;
		}
		if (num == 0)
		{
			base.gameObject.AddComponent<Rigidbody>();
		}
		else if (num > 1)
		{
			Debug.LogFormat("WARNING: pickupable {0} has more than 1 rigidbody component!", base.gameObject.name);
		}
		Deactivate();
		attached = true;
		if (_isInSub)
		{
			Unplace();
			_isInSub = false;
		}
	}

	private void Activate(bool registerEntity)
	{
		base.gameObject.SetActive(value: true);
		isPickupable = true;
		base.isValidHandTarget = true;
		PlayerTool component = GetComponent<PlayerTool>();
		if (component != null && component.mainCollider != null)
		{
			component.mainCollider.isTrigger = false;
		}
		if ((bool)LargeWorld.main && registerEntity)
		{
			LargeWorld.main.streamer.cellManager.RegisterEntity(base.gameObject);
		}
	}

	private void Deactivate()
	{
		base.gameObject.SetActive(value: false);
		isPickupable = false;
		base.isValidHandTarget = false;
		PlayerTool component = GetComponent<PlayerTool>();
		if (component != null && component.mainCollider != null)
		{
			component.mainCollider.isTrigger = true;
		}
		if (LargeWorld.main != null && LargeWorld.main.streamer != null && LargeWorld.main.streamer.cellManager != null)
		{
			base.transform.parent = null;
			LargeWorld.main.streamer.cellManager.UnregisterEntity(base.gameObject);
		}
	}

	public void SetVisible(bool visible)
	{
		if (attached)
		{
			if (visible)
			{
				base.gameObject.SetActive(value: true);
				DisableBehaviours();
				DisableColliders();
				DisableRigidbodies();
			}
			else
			{
				EnableRigidbodies();
				EnableColliders();
				EnableBehaviours();
				base.gameObject.SetActive(value: false);
			}
		}
	}

	public void Reparent(Transform parent)
	{
		Vector3 localScale = base.transform.localScale;
		base.transform.parent = parent;
		if (parent != null)
		{
			if (prefabData == null)
			{
				base.transform.localPosition = Vector3.zero;
				base.transform.localRotation = Quaternion.identity;
			}
			else
			{
				base.transform.localPosition = prefabData.localPosition;
				base.transform.localRotation = Quaternion.Euler(prefabData.localRotation);
			}
		}
		base.transform.localScale = localScale;
	}

	public void Drop()
	{
		Drop(base.transform.position);
	}

	private static Vector3 FindDropPosition(Vector3 fromPosition, Vector3 desiredPosition)
	{
		Vector3 result = desiredPosition;
		float num = float.PositiveInfinity;
		Vector3 normalized = (desiredPosition - fromPosition).normalized;
		Ray ray = new Ray(fromPosition, normalized);
		QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore;
		int num2 = UWE.Utils.SpherecastIntoSharedBuffer(ray, 0.3f, 5f, -5, queryTriggerInteraction);
		for (int i = 0; i < num2; i++)
		{
			bool num3 = UWE.Utils.sharedHitBuffer[i].collider.GetComponent<Player>() != null;
			float distance = UWE.Utils.sharedHitBuffer[i].distance;
			if (!num3 && distance > 0f && distance < num)
			{
				num = UWE.Utils.sharedHitBuffer[i].distance;
				result = UWE.Utils.sharedHitBuffer[i].point - normalized * 0.15f;
			}
		}
		return result;
	}

	public void Drop(Vector3 dropPosition, Vector3 pushVelocity = default(Vector3), bool checkPosition = true)
	{
		if (inventoryItem != null)
		{
			IItemsContainer container = inventoryItem.container;
			if (container != null)
			{
				container.RemoveItem(inventoryItem, forced: true, verbose: false);
				if (container == Inventory.main.container || container == Inventory.main.equipment)
				{
					PlayDropSound();
				}
			}
			inventoryItem = null;
		}
		Player component = Utils.GetLocalPlayer().GetComponent<Player>();
		WaterPark currentWaterPark = component.currentWaterPark;
		bool flag = currentWaterPark != null;
		SetVisible(visible: false);
		Reparent(null);
		if (checkPosition)
		{
			dropPosition = FindDropPosition(MainCamera.camera.transform.position, dropPosition);
		}
		base.transform.position = dropPosition;
		Activate(!flag);
		if (flag)
		{
			currentWaterPark.AddItem(this);
		}
		timeDropped = 0f;
		droppedEvent.Trigger(this);
		base.gameObject.SendMessage("OnDrop", SendMessageOptions.DontRequireReceiver);
		_isInSub = component.IsInSub() && !flag;
		Rigidbody component2 = GetComponent<Rigidbody>();
		attached = false;
		if (!flag)
		{
			component2.AddForce(pushVelocity, ForceMode.VelocityChange);
			Smell smell = base.gameObject.GetComponent<Smell>();
			if (smell == null)
			{
				smell = base.gameObject.AddComponent<Smell>();
			}
			smell.owner = component.gameObject;
			smell.strength = 1f;
			smell.falloff = 0.05f;
		}
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(component2, !activateRigidbodyWhenDropped);
		if (_isInSub)
		{
			Place();
		}
	}

	private void Place()
	{
		DisableColliders();
		DisableRigidbodies();
	}

	private void Unplace()
	{
		EnableRigidbodies();
		EnableColliders();
	}

	private void DisableBehaviours()
	{
		disabledBehaviours = new List<Behaviour>();
		using (ListPool<Behaviour> listPool = Pool<ListPool<Behaviour>>.Get())
		{
			List<Behaviour> list = listPool.list;
			GetComponentsInChildren(list);
			for (int i = 0; i < list.Count; i++)
			{
				Behaviour behaviour = list[i];
				if (behaviour == null)
				{
					Debug.LogWarning("Discarded missing behaviour on a Pickupable gameObject", this);
					continue;
				}
				Type type = behaviour.GetType();
				if (!behaviour.enabled)
				{
					continue;
				}
				bool flag = true;
				for (int j = 0; j < dontDisableOnAttach.Length; j++)
				{
					if (type.Equals(dontDisableOnAttach[j]))
					{
						flag = false;
					}
				}
				if (flag)
				{
					behaviour.enabled = false;
					disabledBehaviours.Add(behaviour);
				}
			}
		}
	}

	private void DisableColliders()
	{
		disabledColliders = new List<Collider>();
		using (ListPool<Collider> listPool = Pool<ListPool<Collider>>.Get())
		{
			List<Collider> list = listPool.list;
			GetComponentsInChildren(list);
			for (int i = 0; i < list.Count; i++)
			{
				Collider collider = list[i];
				if (collider.enabled && !collider.isTrigger)
				{
					collider.gameObject.layer = LayerID.Useable;
					collider.isTrigger = true;
					disabledColliders.Add(collider);
				}
			}
		}
	}

	private void DisableRigidbodies()
	{
		disabledRigidbodies = new List<Rigidbody>();
		Rigidbody[] componentsInChildren = GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody rigidbody in componentsInChildren)
		{
			UWE.Utils.SetIsKinematicAndUpdateInterpolation(rigidbody, isKinematic: true);
			disabledRigidbodies.Add(rigidbody);
		}
	}

	private void EnableColliders()
	{
		if (disabledColliders != null)
		{
			for (int i = 0; i < disabledColliders.Count; i++)
			{
				Collider collider = disabledColliders[i];
				collider.isTrigger = false;
				collider.gameObject.layer = LayerID.Default;
			}
			disabledColliders = null;
		}
	}

	private void EnableBehaviours()
	{
		if (disabledBehaviours == null)
		{
			return;
		}
		for (int i = 0; i < disabledBehaviours.Count; i++)
		{
			Behaviour behaviour = disabledBehaviours[i];
			if (behaviour != null)
			{
				behaviour.enabled = true;
			}
		}
		disabledBehaviours = null;
	}

	private void EnableRigidbodies()
	{
		if (disabledRigidbodies != null)
		{
			for (int i = 0; i < disabledRigidbodies.Count; i++)
			{
				UWE.Utils.SetIsKinematicAndUpdateInterpolation(disabledRigidbodies[i], isKinematic: false);
			}
			disabledRigidbodies = null;
		}
	}

	public TechType GetTechType()
	{
		if (!overrideTechUsed)
		{
			return CraftData.GetTechType(base.gameObject);
		}
		return overrideTechType;
	}

	public string GetTechName()
	{
		return GetTechType().AsString();
	}

	private bool AllowedToPickUp()
	{
		if (isPickupable && Time.time - timeDropped > 1f)
		{
			return Player.main.HasInventoryRoom(this);
		}
		return false;
	}

	public void OnHandClick(GUIHand hand)
	{
		if (!hand.IsFreeToInteract() || !AllowedToPickUp())
		{
			return;
		}
		if (!Inventory.Get().Pickup(this))
		{
			ErrorMessage.AddWarning(Language.main.Get("InventoryFull"));
			return;
		}
		Player.main.PlayGrab();
		WaterParkItem component = GetComponent<WaterParkItem>();
		if (component != null)
		{
			component.SetWaterPark(null);
		}
	}

	public void OnHandHover(GUIHand hand)
	{
		HandReticle main = HandReticle.main;
		if (!hand.IsFreeToInteract())
		{
			return;
		}
		TechType techType = GetTechType();
		if (AllowedToPickUp())
		{
			string text = string.Empty;
			string text2 = string.Empty;
			Exosuit exosuit = Player.main.GetVehicle() as Exosuit;
			bool flag = exosuit == null || exosuit.HasClaw();
			if (flag)
			{
				ISecondaryTooltip component = base.gameObject.GetComponent<ISecondaryTooltip>();
				if (component != null)
				{
					text2 = component.GetSecondaryTooltip();
				}
				text = (usePackUpIcon ? LanguageCache.GetPackUpText(techType) : LanguageCache.GetPickupText(techType));
				main.SetIcon(usePackUpIcon ? HandReticle.IconType.PackUp : HandReticle.IconType.Hand);
			}
			if ((bool)exosuit)
			{
				GameInput.Button button = (flag ? GameInput.Button.LeftHand : GameInput.Button.None);
				if (exosuit.leftArmType != TechType.ExosuitClawArmModule)
				{
					button = GameInput.Button.RightHand;
				}
				HandReticle.main.SetText(HandReticle.TextType.Hand, text, translate: false, button);
				HandReticle.main.SetText(HandReticle.TextType.HandSubscript, text2, translate: false);
			}
			else
			{
				HandReticle.main.SetText(HandReticle.TextType.Hand, text, translate: false, GameInput.Button.LeftHand);
				HandReticle.main.SetText(HandReticle.TextType.HandSubscript, text2, translate: false);
			}
		}
		else if (isPickupable && !Player.main.HasInventoryRoom(this))
		{
			main.SetText(HandReticle.TextType.Hand, techType.AsString(), translate: true);
			main.SetText(HandReticle.TextType.HandSubscript, "InventoryFull", translate: true);
		}
		else
		{
			main.SetText(HandReticle.TextType.Hand, techType.AsString(), translate: true);
			main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		}
	}

	private void OnDestroy()
	{
		isDestroyed = true;
		if (!SceneCleaner.isLoading)
		{
			SetInventoryItem(null);
		}
	}

	public IEnumerator OnProtoDeserializeAsync(ProtobufSerializer serializer)
	{
		if (version < 3 && _attached && _isInSub && GetComponent<WaterParkItem>() != null && GetComponentInParent<WaterPark>() != null)
		{
			_attached = false;
			_isInSub = false;
		}
		if (_isInSub)
		{
			Place();
		}
		if (isLootCube && overrideTechUsed)
		{
			CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(overrideTechType, verbose: false);
			yield return request;
			GameObject result = request.GetResult();
			if ((bool)result)
			{
				Eatable component = result.GetComponent<Eatable>();
				if ((bool)component)
				{
					Eatable eatable = base.gameObject.EnsureComponent<Eatable>();
					eatable.timeDecayStart = component.timeDecayStart;
					eatable.foodValue = component.foodValue;
					eatable.waterValue = component.waterValue;
					eatable.decomposes = component.decomposes;
					eatable.kDecayRate = component.kDecayRate;
				}
			}
		}
		if (_attached)
		{
			if ((bool)LargeWorld.main)
			{
				LargeWorld.main.streamer.cellManager.UnregisterEntity(base.gameObject);
			}
			pickedUpEvent.Trigger(this);
		}
		Rigidbody component2 = GetComponent<Rigidbody>();
		if ((bool)component2)
		{
			switch (isKinematic)
			{
			case PickupableKinematicState.NoKinematicStateSet:
				isKinematic = ((!component2.isKinematic) ? PickupableKinematicState.NonKinematic : PickupableKinematicState.Kinematic);
				break;
			case PickupableKinematicState.Kinematic:
				UWE.Utils.SetIsKinematicAndUpdateInterpolation(component2, isKinematic: true);
				break;
			case PickupableKinematicState.NonKinematic:
				UWE.Utils.SetIsKinematicAndUpdateInterpolation(component2, isKinematic: false);
				break;
			}
		}
		else
		{
			isKinematic = PickupableKinematicState.Invalid;
		}
	}

	public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
	{
		version = 4;
		Rigidbody component = GetComponent<Rigidbody>();
		if ((bool)component)
		{
			isKinematic = ((!component.isKinematic) ? PickupableKinematicState.NonKinematic : PickupableKinematicState.Kinematic);
		}
		else
		{
			isKinematic = PickupableKinematicState.Invalid;
		}
	}

	public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
	{
		if (attached)
		{
			bool activeSelf = base.gameObject.activeSelf;
			Transform parent = base.transform.parent;
			base.gameObject.SetActive(value: true);
			base.transform.SetParent(null);
			base.transform.SetParent(parent);
			base.gameObject.SetActive(activeSelf);
		}
	}

	public string CompileTimeCheck(ILanguage language)
	{
		TechType techType = GetTechType();
		string text = language.CheckTechType(techType);
		if (text == null)
		{
			if (!isPickupable)
			{
				return null;
			}
			text = language.CheckTechTypeTooltip(techType);
		}
		return text;
	}
}
