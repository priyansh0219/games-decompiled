using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
[ProtoInclude(2100, typeof(SeaMoth))]
[ProtoInclude(2200, typeof(Exosuit))]
public abstract class Vehicle : HandTarget, IHandTarget, IProtoEventListener, IProtoEventListenerAsync, IProtoTreeEventListener, IAnimParamReceiver, IQuickSlots
{
	public enum DockType
	{
		None = 0,
		Base = 1,
		Cyclops = 2
	}

	public enum ControlSheme
	{
		Submarine = 0,
		Submersible = 1,
		Mech = 2
	}

	public TorpedoType[] torpedoTypes;

	private const int currentVersion = 4;

	private const float yUndockingSpeed = 5f;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 4;

	[NonSerialized]
	[ProtoMember(2)]
	public string vehicleName;

	[NonSerialized]
	[ProtoMember(3, OverwriteList = true)]
	public Vector3[] vehicleColors;

	[NonSerialized]
	[Obsolete("Obsolete since v2")]
	[ProtoMember(4, OverwriteList = true)]
	public byte[] serializedModules;

	[NonSerialized]
	[ProtoMember(5, OverwriteList = true)]
	public Dictionary<string, string> serializedModuleSlots;

	[NonSerialized]
	[ProtoMember(6)]
	public bool precursorOutOfWater;

	[NonSerialized]
	[ProtoMember(7)]
	public string pilotId;

	protected bool onGround;

	protected float timeOnGround;

	protected Vector3 surfaceNormal = Vector3.zero;

	protected bool constructionFallOverride;

	protected bool ignoreInput;

	protected bool teleporting;

	public Vector3 prevVelocity = Vector3.zero;

	public Animator mainAnimator;

	public GameObject destructionEffect;

	public SubName subName;

	[AssertNotNull]
	public string handLabel = "Control";

	public bool stabilizeRoll;

	public bool replenishesOxygen = true;

	public float oxygenPerSecond = 4f;

	public float oxygenEnergyCost = 0.1f;

	public ControlSheme controlSheme;

	public bool playerSits;

	public bool moveOnLand;

	public string customGoalOnEnter;

	public VoiceNotification welcomeNotification;

	public VoiceNotification noPowerWelcomeNotification;

	public FMODAsset enterSound;

	public Transform leftHandPlug;

	public Transform rightHandPlug;

	public float forwardForce = 13f;

	public float backwardForce = 5f;

	public float sidewardForce = 11.5f;

	public float sidewaysTorque = 8.5f;

	public float verticalForce = 11f;

	public float onGroundForceMultiplier = 1f;

	[AssertNotNull]
	public GameObject collisionModel;

	[AssertNotNull]
	public VehicleUpgradeConsoleInput upgradesInput;

	[AssertNotNull]
	public GameObject playerPosition;

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter chargingSound;

	[AssertNotNull]
	public FMODAsset splashSound;

	[AssertNotNull]
	public WorldForces worldForces;

	[AssertNotNull]
	public Rigidbody useRigidbody;

	[AssertNotNull]
	public CrushDamage crushDamage;

	[AssertNotNull]
	public WorldSettledTracker worldSettledTracker;

	[AssertNotNull]
	public VFXConstructing vfxConstructing;

	public FMODAsset powerDownSound;

	[AssertNotNull]
	[SerializeField]
	private EnergyInterface energyInterface;

	[AssertNotNull]
	public LiveMixin liveMixin;

	[AssertNotNull]
	public ChildObjectIdentifier modulesRoot;

	public DepthAlarms depthAlarms;

	private const float maxRechargeFraction = 0.0025f;

	private float timeLastCollision;

	private bool _turnedOn = true;

	private float timeUndocked;

	private bool rotationLocked;

	private float steeringReponsiveness = 0.02f;

	private float steeringWheelYaw;

	private float steeringWheelPitch;

	private float enginePowerRating = 1f;

	private bool wasPowered;

	private bool wasAboveWater;

	private VehicleAccelerationModifier[] accelerationModifiers;

	private PARAMETER_ID verticalVelocitySoundIndex = FMODUWE.invalidParameterId;

	private bool isInitialized;

	public Event<Vehicle> turnedOnEvent = new Event<Vehicle>();

	public Event<Vehicle> turnedOffEvent = new Event<Vehicle>();

	protected bool _docked;

	protected float timeDocked;

	protected Dictionary<string, int> slotIndexes;

	protected float[] quickSlotTimeUsed;

	protected float[] quickSlotCooldown;

	protected bool[] quickSlotToggled;

	protected float[] quickSlotCharge;

	protected int activeSlot = -1;

	public GameObject[] disableOnDock;

	public Collider[] disableDockedColliders;

	private bool lastPilotingState;

	protected virtual string vehicleDefaultName => "";

	protected virtual Vector3[] vehicleDefaultColors => new Vector3[1]
	{
		new Vector3(0f, 0f, 1f)
	};

	public Equipment modules { get; private set; }

	public bool turnedOn
	{
		get
		{
			return _turnedOn;
		}
		set
		{
			if (value != _turnedOn)
			{
				_turnedOn = value;
				if (value)
				{
					turnedOnEvent.Trigger(this);
				}
				else
				{
					turnedOffEvent.Trigger(this);
				}
			}
		}
	}

	public virtual bool playerFullyEntered { get; protected set; }

	protected virtual string[] slotIDs => null;

	public bool docked
	{
		get
		{
			return _docked;
		}
		set
		{
			if (value == _docked)
			{
				return;
			}
			if (value)
			{
				useRigidbody.isKinematic = true;
				collisionModel.SetActive(value: false);
				crushDamage.enabled = false;
				timeDocked = Time.time;
			}
			else
			{
				base.transform.parent = null;
				useRigidbody.isKinematic = false;
				collisionModel.SetActive(value: true);
				LargeWorldStreamer.main.cellManager.RegisterEntity(base.gameObject);
				timeUndocked = Time.time;
				crushDamage.enabled = true;
			}
			EcoTarget component = GetComponent<EcoTarget>();
			if ((bool)component)
			{
				component.enabled = !value;
			}
			for (int i = 0; i < disableOnDock.Length; i++)
			{
				GameObject gameObject = disableOnDock[i];
				if (!(gameObject == null))
				{
					gameObject.SetActive(!value);
				}
			}
			_docked = value;
			DockType dockType = DockType.None;
			if (_docked)
			{
				SubRoot componentInParent = this.GetComponentInParent<SubRoot>(includeInactive: true);
				if (componentInParent != null)
				{
					dockType = (componentInParent.isBase ? DockType.Base : DockType.Cyclops);
				}
				else
				{
					Debug.LogError("SeaMoth docked property set to true, but subRoot component not found in parent");
				}
			}
			OnDockedChanged(_docked, dockType);
			upgradesInput.SetDocked(dockType);
		}
	}

	public Vector3 AutopilotDestination { get; private set; }

	public bool IsAutopilotEnabled { get; private set; }

	public event QuickSlots.OnBind onBind;

	public event QuickSlots.OnToggle onToggle;

	public event QuickSlots.OnSelect onSelect;

	public float GetTemperature()
	{
		WaterTemperatureSimulation main = WaterTemperatureSimulation.main;
		if (!(main != null))
		{
			return 0f;
		}
		return main.GetTemperature(base.transform.position);
	}

	public virtual void GetDepth(out int depth, out int crushDepth)
	{
		depth = 0;
		crushDepth = 0;
		depth = Mathf.FloorToInt(crushDamage.GetDepth());
		crushDepth = Mathf.FloorToInt(crushDamage.crushDepth);
	}

	public bool GetRecentlyUndocked()
	{
		return timeUndocked + 5f > Time.time;
	}

	protected virtual void OnDockedChanged(bool docked, DockType dockType)
	{
		Player componentInChildren = GetComponentInChildren<Player>();
		if ((bool)componentInChildren)
		{
			componentInChildren.transform.parent = null;
		}
		if ((bool)componentInChildren)
		{
			componentInChildren.transform.parent = playerPosition.transform;
		}
		UpdateCollidersForDocking(docked);
	}

	private void UpdateCollidersForDocking(bool docked)
	{
		for (int i = 0; i < disableDockedColliders.Length; i++)
		{
			disableDockedColliders[i].enabled = !docked;
		}
	}

	public virtual IEnumerator Undock(Player player, float yUndockedPosition)
	{
		EnterVehicle(player, teleport: false);
		docked = false;
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(useRigidbody, isKinematic: true);
		Vector3 initialPosition = base.transform.position;
		Vector3 finalPosition = new Vector3(initialPosition.x, yUndockedPosition, initialPosition.z);
		float duration = (initialPosition.y - finalPosition.y) / 5f;
		float timeInterpolated = 0f;
		if (duration > 0f)
		{
			do
			{
				base.transform.position = Vector3.Lerp(initialPosition, finalPosition, timeInterpolated / duration);
				timeInterpolated += Time.deltaTime;
				yield return null;
			}
			while (timeInterpolated < duration);
		}
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(useRigidbody, isKinematic: false);
		useRigidbody.AddForce(Vector3.down * 5f, ForceMode.VelocityChange);
	}

	public override void Awake()
	{
		base.Awake();
		LazyInitialize();
	}

	private void LazyInitialize()
	{
		if (isInitialized)
		{
			return;
		}
		isInitialized = true;
		slotIndexes = new Dictionary<string, int>();
		int num = 0;
		string[] array = slotIDs;
		for (int i = 0; i < array.Length; i++)
		{
			_ = array[i];
			slotIndexes.Add(slotIDs[num], num);
			num++;
		}
		quickSlotTimeUsed = new float[slotIDs.Length];
		quickSlotCooldown = new float[slotIDs.Length];
		quickSlotToggled = new bool[slotIDs.Length];
		quickSlotCharge = new float[slotIDs.Length];
		if (vehicleName == null)
		{
			vehicleName = vehicleDefaultName;
			if ((bool)subName)
			{
				subName.DeserializeName(vehicleName);
			}
		}
		if (vehicleColors == null)
		{
			vehicleColors = vehicleDefaultColors;
			if ((bool)subName)
			{
				subName.DeserializeColors(vehicleColors);
			}
		}
		modules = new Equipment(base.gameObject, modulesRoot.transform);
		modules.SetLabel("CyclopsUpgradesStorageLabel");
		modules.onEquip += OnEquip;
		modules.onUnequip += OnUnequip;
		UnlockDefaultModuleSlots();
		upgradesInput.equipment = modules;
	}

	public void ReAttach(Vector3 fallbackPos)
	{
		Invoke("AttachToBay", 0.1f);
		docked = true;
		base.transform.position = fallbackPos;
		base.gameObject.SetActive(value: false);
	}

	private void AttachToBay()
	{
		base.gameObject.SetActive(value: true);
		bool flag = false;
		Transform parent = base.transform.parent;
		if ((bool)parent && (bool)parent.GetComponent<SubRoot>())
		{
			VehicleDockingBay[] componentsInChildren = parent.GetComponentsInChildren<VehicleDockingBay>();
			float num = float.PositiveInfinity;
			VehicleDockingBay vehicleDockingBay = null;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				float magnitude = (base.transform.position - componentsInChildren[i].transform.position).magnitude;
				if (magnitude < num)
				{
					vehicleDockingBay = componentsInChildren[i];
					num = magnitude;
				}
			}
			if (vehicleDockingBay != null)
			{
				vehicleDockingBay.SetVehicleDocked(this);
				flag = true;
			}
		}
		if (!flag)
		{
			docked = false;
		}
	}

	private void UnlockDefaultModuleSlots()
	{
		modules.AddSlots(slotIDs);
	}

	protected void UpdateModuleSlots()
	{
		for (int i = 0; i < slotIDs.Length; i++)
		{
			TechType techTypeInSlot = modules.GetTechTypeInSlot(slotIDs[i]);
			if (techTypeInSlot != 0)
			{
				UpgradeModuleChanged(slotIDs[i], techTypeInSlot, added: true);
			}
		}
	}

	public int GetSlotIndex(string slot)
	{
		if (slotIndexes.TryGetValue(slot, out var value))
		{
			return value;
		}
		return -1;
	}

	public bool IsInsideAquarium()
	{
		return PrisonManager.IsInsideAquarium(base.transform.position);
	}

	protected void OnEquip(string slot, InventoryItem item)
	{
		Pickupable item2 = item.item;
		TechType techType = ((item2 != null) ? item2.GetTechType() : TechType.None);
		UpgradeModuleChanged(slot, techType, added: true);
	}

	protected void OnUnequip(string slot, InventoryItem item)
	{
		Pickupable item2 = item.item;
		TechType techType = ((item2 != null) ? item2.GetTechType() : TechType.None);
		UpgradeModuleChanged(slot, techType, added: false);
	}

	public virtual void Start()
	{
		UpdateModuleSlots();
		InvokeRepeating("PassiveEnergyConsumption", UnityEngine.Random.value, 1f);
		InvokeRepeating("UpdateEnergyRecharge", UnityEngine.Random.value, 1f);
		wasPowered = IsPowered();
		accelerationModifiers = base.gameObject.GetComponentsInChildren<VehicleAccelerationModifier>();
		if (mainAnimator != null)
		{
			mainAnimator.SetBool("vr_active", GameOptions.GetVrAnimationMode());
		}
		DevConsole.RegisterConsoleCommand(this, "interpolate");
		if (pilotId != null)
		{
			UniqueIdentifier.TryGetIdentifier(pilotId, out var uid);
			if ((bool)uid)
			{
				EnterVehicle(uid.GetComponent<Player>(), teleport: true, playEnterAnimation: false);
			}
		}
	}

	private void UpdateEnergyRecharge()
	{
		bool flag = false;
		energyInterface.GetValues(out var charge, out var capacity);
		if (docked && timeDocked + 4f < Time.time && charge < capacity)
		{
			float amount = Mathf.Min(capacity - charge, capacity * 0.0025f);
			PowerRelay componentInParent = GetComponentInParent<PowerRelay>();
			if (componentInParent == null)
			{
				Debug.LogError("vehicle is docked but can't access PowerRelay component");
			}
			float amountConsumed = 0f;
			componentInParent.ConsumeEnergy(amount, out amountConsumed);
			if (!GameModeUtils.RequiresPower() || amountConsumed > 0f)
			{
				energyInterface.AddEnergy(amountConsumed);
				flag = true;
			}
		}
		if (flag)
		{
			chargingSound.Play();
		}
		else
		{
			chargingSound.Stop();
		}
	}

	public string GetName()
	{
		if (!(subName != null))
		{
			return vehicleName;
		}
		return subName.GetName();
	}

	public virtual void SetPlayerInside(bool inside)
	{
		mainAnimator.SetBool("player_in", inside);
		Player.main.currentMountedVehicle = this;
	}

	void IAnimParamReceiver.ForwardAnimationParameterBool(string parameterName, bool value)
	{
		Animator[] componentsInChildren = GetComponentsInChildren<Animator>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetBool(parameterName, value);
		}
	}

	private void ReplenishOxygen()
	{
		if (turnedOn && replenishesOxygen && GetPilotingMode() && CanPilot())
		{
			OxygenManager oxygenMgr = Player.main.oxygenMgr;
			oxygenMgr.GetTotal(out var available, out var capacity);
			float amount = Mathf.Min(capacity - available, oxygenPerSecond * Time.deltaTime) * oxygenEnergyCost;
			float secondsToAdd = energyInterface.ConsumeEnergy(amount) / oxygenEnergyCost;
			oxygenMgr.AddOxygen(secondsToAdd);
		}
	}

	public bool GetPilotingMode()
	{
		Player localPlayerComp = Utils.GetLocalPlayerComp();
		if (localPlayerComp.GetMode() == Player.Mode.LockedPiloting)
		{
			return localPlayerComp.transform.parent == playerPosition.transform;
		}
		return false;
	}

	public void OnHandHover(GUIHand hand)
	{
		if (!GetPilotingMode() && GetEnabled())
		{
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
			HandReticle.main.SetText(HandReticle.TextType.Hand, handLabel, translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
		if (!GetPilotingMode() && GetEnabled())
		{
			EnterVehicle(hand.player, teleport: true);
			if ((bool)enterSound)
			{
				Utils.PlayFMODAsset(enterSound, base.transform);
			}
		}
	}

	protected virtual void EnterVehicle(Player player, bool teleport, bool playEnterAnimation = true)
	{
		if (!(player != null))
		{
			return;
		}
		player.SetCurrentSub(null);
		player.playerController.UpdateController();
		player.EnterLockedMode(playerPosition.transform, teleport);
		player.sitting = playerSits;
		player.currentMountedVehicle = this;
		player.playerController.ForceControllerSize();
		if (!string.IsNullOrEmpty(customGoalOnEnter))
		{
			GoalManager.main.OnCustomGoalEvent(customGoalOnEnter);
		}
		if (!energyInterface.hasCharge)
		{
			if ((bool)noPowerWelcomeNotification)
			{
				noPowerWelcomeNotification.Play();
			}
		}
		else if ((bool)welcomeNotification)
		{
			welcomeNotification.Play();
		}
		pilotId = player.GetComponent<UniqueIdentifier>().Id;
		mainAnimator.SetBool("enterAnimation", playEnterAnimation);
	}

	private bool GetEnabled()
	{
		return liveMixin.IsAlive();
	}

	private void OnKill()
	{
		if (GetPilotingMode())
		{
			OnPilotModeEnd();
			if (!Player.main.ToNormalMode())
			{
				Player.main.ToNormalMode(findNewPosition: false);
				Player.main.transform.parent = null;
			}
		}
		if ((bool)destructionEffect)
		{
			GameObject obj = UnityEngine.Object.Instantiate(destructionEffect);
			obj.transform.position = base.transform.position;
			obj.transform.rotation = base.transform.rotation;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	protected virtual void OnPilotModeBegin()
	{
		uGUI.main.quickSlots.SetTarget(this);
		if ((bool)mainAnimator)
		{
			mainAnimator.SetBool("player_in", value: true);
		}
		useRigidbody.interpolation = (Player.interpolate ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None);
	}

	protected virtual void OnPilotModeEnd()
	{
		ResetSlotsState();
		uGUI.main.quickSlots.SetTarget(null);
		if ((bool)mainAnimator)
		{
			mainAnimator.SetBool("player_in", value: false);
		}
		pilotId = null;
	}

	private void OnConsoleCommand_interpolate()
	{
		if (GetPilotingMode())
		{
			useRigidbody.interpolation = (Player.interpolate ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None);
		}
	}

	protected void SetRotationLocked(bool locked)
	{
		rotationLocked = locked;
		useRigidbody.freezeRotation = true;
		Vector3 eulerAngles = base.transform.eulerAngles;
		base.transform.eulerAngles = new Vector3(0f, eulerAngles.y, 0f);
	}

	protected virtual void OverrideAcceleration(ref Vector3 acceleration)
	{
	}

	private void PlaySplashSound()
	{
		EventInstance @event = FMODUWE.GetEvent(splashSound);
		@event.set3DAttributes(base.transform.position.To3DAttributes());
		if (FMODUWE.IsInvalidParameterId(verticalVelocitySoundIndex))
		{
			verticalVelocitySoundIndex = FMODUWE.GetEventInstanceParameterIndex(@event, "verticalVelocity");
		}
		@event.setParameterValueByIndex(verticalVelocitySoundIndex, useRigidbody.velocity.y);
		@event.start();
		@event.release();
	}

	private void ApplyPhysicsMove()
	{
		if (!GetPilotingMode())
		{
			return;
		}
		if (worldForces.IsAboveWater() != wasAboveWater)
		{
			PlaySplashSound();
			wasAboveWater = worldForces.IsAboveWater();
		}
		bool flag = base.transform.position.y < Ocean.GetOceanLevel() && base.transform.position.y < worldForces.waterDepth && !precursorOutOfWater;
		if (!(moveOnLand || flag))
		{
			return;
		}
		if (controlSheme == ControlSheme.Submersible)
		{
			Vector3 vector = ((!IsAutopilotEnabled) ? (AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero) : CalculateAutopilotLocalWishDir());
			vector.Normalize();
			float num = Mathf.Abs(vector.x) * sidewardForce + Mathf.Max(0f, vector.z) * forwardForce + Mathf.Max(0f, 0f - vector.z) * backwardForce + Mathf.Abs(vector.y * verticalForce);
			Vector3 acceleration = base.transform.rotation * (num * vector) * Time.deltaTime;
			for (int i = 0; i < accelerationModifiers.Length; i++)
			{
				accelerationModifiers[i].ModifyAcceleration(ref acceleration);
			}
			useRigidbody.AddForce(acceleration, ForceMode.VelocityChange);
		}
		else if (controlSheme == ControlSheme.Submarine || controlSheme == ControlSheme.Mech)
		{
			Vector3 lhs;
			Vector3 vector2;
			if (IsAutopilotEnabled)
			{
				lhs = CalculateAutopilotLocalWishDir();
				lhs = Vector3.Min(Vector3.Max(lhs, -Vector3.one), Vector3.one);
				vector2 = lhs;
				vector2.y = 0f;
			}
			else
			{
				lhs = (AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero);
				vector2 = new Vector3(lhs.x, 0f, lhs.z);
			}
			float num2 = Mathf.Abs(vector2.x) * sidewardForce + Mathf.Max(0f, vector2.z) * forwardForce + Mathf.Max(0f, 0f - vector2.z) * backwardForce;
			vector2 = base.transform.rotation * vector2;
			vector2.y = 0f;
			vector2 = Vector3.Normalize(vector2);
			if (onGround)
			{
				vector2 = Vector3.ProjectOnPlane(vector2, surfaceNormal);
				vector2.y = Mathf.Clamp(vector2.y, -0.5f, 0.5f);
				num2 *= onGroundForceMultiplier;
			}
			Vector3 vector3 = new Vector3(0f, lhs.y, 0f);
			vector3.y *= verticalForce * Time.deltaTime;
			Vector3 acceleration2 = num2 * vector2 * Time.deltaTime + vector3;
			OverrideAcceleration(ref acceleration2);
			for (int j = 0; j < accelerationModifiers.Length; j++)
			{
				accelerationModifiers[j].ModifyAcceleration(ref acceleration2);
			}
			useRigidbody.AddForce(acceleration2, ForceMode.VelocityChange);
		}
	}

	protected Vector3 CalculateAutopilotLocalWishDir()
	{
		Vector3 direction = AutopilotDestination - base.transform.position;
		return base.transform.InverseTransformDirection(direction);
	}

	public virtual void TeleportVehicle(Vector3 goToPos, Quaternion dir, bool keepRigidBodyKinematicState = false)
	{
		bool isKinematic = useRigidbody.isKinematic;
		useRigidbody.isKinematic = true;
		base.transform.position = goToPos;
		base.transform.rotation = dir;
		base.transform.Translate(new Vector3(0f, 2f, 4f), Space.Self);
		surfaceNormal = Vector3.up;
		onGround = false;
		Player.main.onTeleportationComplete += OnTeleportationComplete;
		if (keepRigidBodyKinematicState)
		{
			useRigidbody.isKinematic = isKinematic;
		}
	}

	public void OnTeleportationStart()
	{
		teleporting = true;
		ignoreInput = true;
	}

	private void OnTeleportationComplete()
	{
		teleporting = false;
		ignoreInput = false;
	}

	public void SetAutopilotDestination(Vector3 autopilotPosition)
	{
		AutopilotDestination = autopilotPosition;
	}

	public void EnableAutopilot()
	{
		IsAutopilotEnabled = true;
	}

	public void DisableAutopilot()
	{
		IsAutopilotEnabled = false;
	}

	public virtual void Update()
	{
		if (CanPilot())
		{
			steeringWheelYaw = Mathf.Lerp(steeringWheelYaw, 0f, Time.deltaTime);
			steeringWheelPitch = Mathf.Lerp(steeringWheelPitch, 0f, Time.deltaTime);
			if ((bool)mainAnimator)
			{
				mainAnimator.SetFloat(AnimatorHashID.view_yaw, steeringWheelYaw * 70f);
				mainAnimator.SetFloat(AnimatorHashID.view_pitch, steeringWheelPitch * 45f);
			}
		}
		if (GetPilotingMode() && CanPilot() && (moveOnLand || base.transform.position.y < Ocean.GetOceanLevel()))
		{
			Vector2 vector = (AvatarInputHandler.main.IsEnabled() ? GameInput.GetLookDelta() : Vector2.zero);
			steeringWheelYaw = Mathf.Clamp(steeringWheelYaw + vector.x * steeringReponsiveness, -1f, 1f);
			steeringWheelPitch = Mathf.Clamp(steeringWheelPitch + vector.y * steeringReponsiveness, -1f, 1f);
			if (controlSheme == ControlSheme.Submersible)
			{
				float num = 3f;
				useRigidbody.AddTorque(base.transform.up * vector.x * sidewaysTorque * 0.0015f * num, ForceMode.VelocityChange);
				useRigidbody.AddTorque(base.transform.right * (0f - vector.y) * sidewaysTorque * 0.0015f * num, ForceMode.VelocityChange);
				useRigidbody.AddTorque(base.transform.forward * (0f - vector.x) * sidewaysTorque * 0.0002f * num, ForceMode.VelocityChange);
			}
			else if ((controlSheme == ControlSheme.Submarine || controlSheme == ControlSheme.Mech) && vector.x != 0f)
			{
				useRigidbody.AddTorque(base.transform.up * vector.x * sidewaysTorque, ForceMode.VelocityChange);
			}
		}
		bool flag = IsPowered();
		if (wasPowered != flag)
		{
			wasPowered = flag;
			OnPoweredChanged(flag);
		}
		ReplenishOxygen();
	}

	protected virtual void OnPoweredChanged(bool powered)
	{
		if (powerDownSound != null && !powered)
		{
			Utils.PlayFMODAsset(powerDownSound, base.transform, 1f);
		}
	}

	private void StabilizeRoll()
	{
		float num = Mathf.Abs(base.transform.eulerAngles.z - 180f);
		if (num <= 178f)
		{
			float num2 = Mathf.Clamp01(1f - num / 180f) * 20f;
			useRigidbody.AddTorque(base.transform.forward * num2 * Time.fixedDeltaTime * Mathf.Sign(base.transform.eulerAngles.z - 180f), ForceMode.VelocityChange);
		}
	}

	public virtual void FixedUpdate()
	{
		bool pilotingMode = GetPilotingMode();
		if (pilotingMode != lastPilotingState)
		{
			if (pilotingMode)
			{
				OnPilotModeBegin();
			}
			else
			{
				OnPilotModeEnd();
			}
			lastPilotingState = pilotingMode;
		}
		if (CanPilot())
		{
			if (pilotingMode)
			{
				ApplyPhysicsMove();
			}
			if (stabilizeRoll)
			{
				StabilizeRoll();
			}
		}
		prevVelocity = useRigidbody.velocity;
		if (constructionFallOverride && base.transform.position.y < 0f)
		{
			constructionFallOverride = false;
		}
	}

	public virtual bool GetAllowedToEject()
	{
		return true;
	}

	protected virtual bool CanLand()
	{
		return true;
	}

	public virtual bool CanPilot()
	{
		return !FPSInputModule.current.lockMovement;
	}

	protected virtual void OnLand()
	{
	}

	private void HandleOnGround(Collision collision)
	{
		if (!CanLand())
		{
			return;
		}
		timeLastCollision = Time.time;
		surfaceNormal = new Vector3(0f, -1f, 0f);
		int num = 0;
		for (int i = 0; i < collision.contacts.Length; i++)
		{
			ContactPoint contactPoint = collision.contacts[i];
			if (contactPoint.normal.y > surfaceNormal.y)
			{
				surfaceNormal.y = contactPoint.normal.y;
			}
			num++;
		}
		if (num > 0)
		{
			if (surfaceNormal.y > 0.5f)
			{
				if (!onGround && prevVelocity.y < -6f)
				{
					OnLand();
				}
				onGround = true;
				timeOnGround = Time.time;
			}
			else
			{
				onGround = false;
			}
			if (controlSheme == ControlSheme.Mech && onGround)
			{
				worldForces.handleGravity = false;
			}
		}
		else
		{
			surfaceNormal = new Vector3(0f, 1f, 0f);
			if (controlSheme == ControlSheme.Mech)
			{
				worldForces.handleGravity = true;
			}
		}
	}

	public virtual void OnCollisionEnter(Collision collision)
	{
		HandleOnGround(collision);
	}

	private void OnCollisionStay(Collision collision)
	{
		HandleOnGround(collision);
	}

	private void HandleOnGroundExit()
	{
		onGround = false;
		surfaceNormal = new Vector3(0f, 1f, 0f);
		if (controlSheme == ControlSheme.Mech)
		{
			worldForces.handleGravity = true;
		}
	}

	private void OnCollisionExit(Collision collisionInfo)
	{
		HandleOnGroundExit();
	}

	public virtual void OnProtoSerialize(ProtobufSerializer serializer)
	{
		vehicleName = ((subName != null) ? subName.GetName() : vehicleDefaultName);
		vehicleColors = ((subName != null) ? subName.GetColors() : vehicleDefaultColors);
		serializedModuleSlots = modules.SaveEquipment();
	}

	public virtual void OnProtoDeserialize(ProtobufSerializer serializer)
	{
	}

	public virtual IEnumerator OnProtoDeserializeAsync(ProtobufSerializer serializer)
	{
		LazyInitialize();
		Vector3[] array = vehicleDefaultColors;
		if (vehicleName == null)
		{
			vehicleName = vehicleDefaultName;
		}
		if (vehicleColors == null)
		{
			vehicleColors = array;
		}
		else if (vehicleColors.Length != array.Length)
		{
			int num = Mathf.Min(vehicleColors.Length, array.Length);
			for (int i = 0; i < num; i++)
			{
				array[i] = vehicleColors[i];
			}
			vehicleColors = array;
		}
		if ((bool)subName)
		{
			subName.DeserializeName(vehicleName);
			subName.DeserializeColors(vehicleColors);
		}
		modules.Clear();
		if (serializedModules != null && serializedModuleSlots != null)
		{
			yield return StorageHelper.RestoreEquipmentAsync(serializer, serializedModules, serializedModuleSlots, modules);
			serializedModules = null;
			serializedModuleSlots = null;
		}
		Transform parent = base.transform.parent;
		if ((bool)parent && (bool)parent.GetComponent<SubRoot>())
		{
			ReAttach(base.transform.position);
		}
	}

	public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
	{
		if (version < 2)
		{
			StoreInformationIdentifier[] componentsInChildren = base.gameObject.GetComponentsInChildren<StoreInformationIdentifier>(includeInactive: true);
			foreach (StoreInformationIdentifier storeInformationIdentifier in componentsInChildren)
			{
				if ((bool)storeInformationIdentifier && storeInformationIdentifier.transform.parent == base.transform)
				{
					UnityEngine.Object.Destroy(storeInformationIdentifier.gameObject);
				}
			}
			version = 2;
		}
		else if (serializedModuleSlots != null)
		{
			StorageHelper.TransferEquipment(modulesRoot.gameObject, serializedModuleSlots, modules);
			serializedModuleSlots = null;
		}
		UnlockDefaultModuleSlots();
		if (version < 4)
		{
			CoroutineHost.StartCoroutine(CleanUpDuplicatedStorage());
		}
	}

	private IEnumerator CleanUpDuplicatedStorage()
	{
		yield return StorageHelper.DestroyDuplicatedItems(base.gameObject);
		version = Mathf.Max(version, 4);
	}

	private void PassiveEnergyConsumption()
	{
		int i = 0;
		for (int num = slotIDs.Length; i < num; i++)
		{
			TechType techType;
			QuickSlotType quickSlotType = GetQuickSlotType(i, out techType);
			if (quickSlotType == QuickSlotType.Toggleable && quickSlotToggled[i])
			{
				if (!ConsumeEnergy(techType))
				{
					ToggleSlot(i, state: false);
				}
			}
			else
			{
				_ = 1;
			}
		}
	}

	protected float GetQuickSlotCooldown(int slotID)
	{
		float num = quickSlotCooldown[slotID];
		if (!(num > 0f))
		{
			return 1f;
		}
		return Mathf.Clamp01((Time.time - quickSlotTimeUsed[slotID]) / num);
	}

	protected bool ShouldSetKinematic()
	{
		if (teleporting)
		{
			return true;
		}
		if (constructionFallOverride)
		{
			return false;
		}
		if (GetPilotingMode())
		{
			return false;
		}
		if (worldSettledTracker.worldSettled && !docked)
		{
			return !vfxConstructing.IsConstructed();
		}
		return true;
	}

	public virtual void SubConstructionComplete()
	{
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(useRigidbody, isKinematic: false);
		constructionFallOverride = true;
	}

	public bool IsPowered()
	{
		return energyInterface.hasCharge;
	}

	protected void GetEnergyValues(out float charge, out float capacity)
	{
		energyInterface.GetValues(out charge, out capacity);
	}

	protected bool HasEnoughEnergy(float energyCost)
	{
		energyInterface.GetValues(out var charge, out var _);
		if (charge >= energyCost)
		{
			return true;
		}
		return false;
	}

	protected void AddEnergy(float amount)
	{
		energyInterface.AddEnergy(amount);
	}

	protected bool ConsumeEngineEnergy(float energyCost)
	{
		float a = energyCost / enginePowerRating;
		int sourceCount;
		float b = energyInterface.TotalCanProvide(out sourceCount);
		return ConsumeEnergy(Mathf.Min(a, b));
	}

	protected bool ConsumeEnergy(float amount)
	{
		if (HasEnoughEnergy(amount))
		{
			energyInterface.ConsumeEnergy(amount);
			return true;
		}
		return false;
	}

	protected bool HasEnoughEnergy(TechType techType)
	{
		TechData.GetEnergyCost(techType, out var result);
		return HasEnoughEnergy(result);
	}

	protected bool ConsumeEnergy(TechType techType)
	{
		TechData.GetEnergyCost(techType, out var result);
		return ConsumeEnergy(result);
	}

	public virtual TechType[] GetSlotBinding()
	{
		int num = slotIDs.Length;
		TechType[] array = new TechType[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = modules.GetTechTypeInSlot(slotIDs[i]);
		}
		return array;
	}

	public virtual TechType GetSlotBinding(int slotID)
	{
		if (slotID < 0 || slotID >= slotIDs.Length)
		{
			return TechType.None;
		}
		string slot = slotIDs[slotID];
		return modules.GetTechTypeInSlot(slot);
	}

	public virtual InventoryItem GetSlotItem(int slotID)
	{
		if (slotID < 0 || slotID >= slotIDs.Length)
		{
			return null;
		}
		string slot = slotIDs[slotID];
		return modules.GetItemInSlot(slot);
	}

	public virtual int GetSlotByItem(InventoryItem item)
	{
		if (item != null)
		{
			int i = 0;
			for (int num = slotIDs.Length; i < num; i++)
			{
				if (modules.GetItemInSlot(slotIDs[i]) == item)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public virtual float GetSlotProgress(int slotID)
	{
		return GetQuickSlotCooldown(slotID);
	}

	public virtual float GetSlotCharge(int slotID)
	{
		if (slotID < 0 || slotID >= slotIDs.Length)
		{
			return 1f;
		}
		TechType techType;
		QuickSlotType quickSlotType = GetQuickSlotType(slotID, out techType);
		if (quickSlotType == QuickSlotType.Chargeable || quickSlotType == QuickSlotType.SelectableChargeable)
		{
			float maxCharge = TechData.GetMaxCharge(techType);
			if (maxCharge > 0f)
			{
				return quickSlotCharge[slotID] / maxCharge;
			}
		}
		return 1f;
	}

	public virtual void SlotKeyDown(int slotID)
	{
		if (ignoreInput || slotID < 0 || slotID >= slotIDs.Length)
		{
			return;
		}
		TechType techType;
		QuickSlotType quickSlotType = GetQuickSlotType(slotID, out techType);
		if (quickSlotType == QuickSlotType.Selectable || quickSlotType == QuickSlotType.SelectableChargeable)
		{
			if (activeSlot >= 0 && activeSlot < slotIDs.Length)
			{
				quickSlotCharge[activeSlot] = 0f;
			}
			ToggleSlot(activeSlot, state: false);
			if (activeSlot == slotID)
			{
				activeSlot = -1;
			}
			else
			{
				ToggleSlot(slotID, state: true);
				activeSlot = slotID;
			}
			NotifySelectSlot(activeSlot);
		}
		else
		{
			if (GetQuickSlotCooldown(slotID) != 1f)
			{
				return;
			}
			switch (quickSlotType)
			{
			case QuickSlotType.Instant:
				if (ConsumeEnergy(techType))
				{
					OnUpgradeModuleUse(techType, slotID);
				}
				break;
			case QuickSlotType.Toggleable:
				if (quickSlotToggled[slotID])
				{
					ToggleSlot(slotID, state: false);
				}
				else if (HasEnoughEnergy(techType))
				{
					ToggleSlot(slotID, state: true);
				}
				break;
			}
		}
	}

	public virtual void SlotKeyHeld(int slotID)
	{
		if (!ignoreInput && slotID >= 0 && slotID < slotIDs.Length && GetQuickSlotCooldown(slotID) == 1f && GetQuickSlotType(slotID, out var techType) == QuickSlotType.Chargeable)
		{
			ChargeModule(techType, slotID);
		}
	}

	public virtual void SlotKeyUp(int slotID)
	{
		if (!ignoreInput && slotID >= 0 && slotID < slotIDs.Length && GetQuickSlotCooldown(slotID) == 1f && GetQuickSlotType(slotID, out var techType) == QuickSlotType.Chargeable)
		{
			OnUpgradeModuleUse(techType, slotID);
			quickSlotCharge[slotID] = 0f;
		}
	}

	public virtual void SlotNext()
	{
		if (ignoreInput)
		{
			return;
		}
		int activeSlotID = GetActiveSlotID();
		int slotCount = GetSlotCount();
		int num = ((activeSlotID < 0) ? (-1) : activeSlotID);
		for (int i = 0; i < slotCount; i++)
		{
			num++;
			if (num >= slotCount)
			{
				num = 0;
			}
			TechType slotBinding = GetSlotBinding(num);
			if (slotBinding != 0)
			{
				QuickSlotType slotType = TechData.GetSlotType(slotBinding);
				if (slotType == QuickSlotType.Selectable || slotType == QuickSlotType.SelectableChargeable)
				{
					SlotKeyDown(num);
					break;
				}
			}
		}
	}

	public virtual void SlotPrevious()
	{
		if (ignoreInput)
		{
			return;
		}
		int activeSlotID = GetActiveSlotID();
		int slotCount = GetSlotCount();
		int num = ((activeSlotID < 0) ? slotCount : activeSlotID);
		for (int i = 0; i < slotCount; i++)
		{
			num--;
			if (num < 0)
			{
				num = slotCount - 1;
			}
			TechType slotBinding = GetSlotBinding(num);
			if (slotBinding != 0)
			{
				QuickSlotType slotType = TechData.GetSlotType(slotBinding);
				if (slotType == QuickSlotType.Selectable || slotType == QuickSlotType.SelectableChargeable)
				{
					SlotKeyDown(num);
					break;
				}
			}
		}
	}

	public virtual void SlotLeftDown()
	{
		if (!ignoreInput && activeSlot >= 0 && GetQuickSlotCooldown(activeSlot) == 1f && GetQuickSlotType(activeSlot, out var techType) == QuickSlotType.Selectable && ConsumeEnergy(techType))
		{
			OnUpgradeModuleUse(techType, activeSlot);
		}
	}

	public virtual void SlotLeftHeld()
	{
		if (!ignoreInput && activeSlot >= 0 && GetQuickSlotCooldown(activeSlot) == 1f && GetQuickSlotType(activeSlot, out var techType) == QuickSlotType.SelectableChargeable)
		{
			ChargeModule(techType, activeSlot);
		}
	}

	public virtual void SlotLeftUp()
	{
		if (!ignoreInput && activeSlot >= 0 && GetQuickSlotCooldown(activeSlot) == 1f && GetQuickSlotType(activeSlot, out var techType) == QuickSlotType.SelectableChargeable && quickSlotCharge[activeSlot] > 0f)
		{
			OnUpgradeModuleUse(techType, activeSlot);
			quickSlotCharge[activeSlot] = 0f;
		}
	}

	public virtual void SlotRightDown()
	{
	}

	public virtual void SlotRightHeld()
	{
	}

	public virtual void SlotRightUp()
	{
	}

	public virtual void DeselectSlots()
	{
		if (ignoreInput)
		{
			return;
		}
		int i = 0;
		for (int num = slotIDs.Length; i < num; i++)
		{
			TechType techType;
			QuickSlotType quickSlotType = GetQuickSlotType(i, out techType);
			if (quickSlotType == QuickSlotType.Toggleable || quickSlotType == QuickSlotType.Selectable || quickSlotType == QuickSlotType.SelectableChargeable)
			{
				ToggleSlot(i, state: false);
			}
			quickSlotCharge[i] = 0f;
		}
		activeSlot = -1;
		NotifySelectSlot(activeSlot);
		Player.main.TryEject();
	}

	public virtual int GetActiveSlotID()
	{
		return activeSlot;
	}

	public virtual bool IsToggled(int slotID)
	{
		if (slotID < 0 || slotID >= slotIDs.Length)
		{
			return false;
		}
		if (TechData.GetSlotType(GetSlotBinding(slotID)) == QuickSlotType.Passive)
		{
			return true;
		}
		return quickSlotToggled[slotID];
	}

	public virtual int GetSlotCount()
	{
		return slotIDs.Length;
	}

	public void Bind(int slotID, InventoryItem item)
	{
	}

	public void Unbind(int slotID)
	{
	}

	protected void UpgradeModuleChanged(string slot, TechType techType, bool added)
	{
		if (!slotIndexes.TryGetValue(slot, out var value))
		{
			value = -1;
		}
		OnUpgradeModuleChange(value, techType, added);
	}

	protected virtual void OnUpgradeModuleChange(int slotID, TechType techType, bool added)
	{
		int count = modules.GetCount(techType);
		switch (techType)
		{
		case TechType.VehiclePowerUpgradeModule:
			enginePowerRating = 1f + 1f * (float)count;
			break;
		case TechType.VehicleArmorPlating:
			GetComponent<DealDamageOnImpact>().mirroredSelfDamageFraction = 0.5f * Mathf.Pow(0.5f, count);
			break;
		}
	}

	protected virtual void OnUpgradeModuleToggle(int slotID, bool active)
	{
	}

	protected virtual void OnUpgradeModuleUse(TechType techType, int slotID)
	{
	}

	protected void NotifyToggleSlot(int slotID)
	{
		bool flag = quickSlotToggled[slotID];
		OnUpgradeModuleToggle(slotID, flag);
		if (this.onToggle != null)
		{
			this.onToggle(slotID, flag);
		}
	}

	protected void NotifySelectSlot(int slotID)
	{
		if (this.onSelect != null)
		{
			this.onSelect(slotID);
		}
	}

	protected void ToggleSlot(int slotID, bool state)
	{
		if (slotID >= 0 && slotID < slotIDs.Length && quickSlotToggled[slotID] != state)
		{
			quickSlotToggled[slotID] = state;
			NotifyToggleSlot(slotID);
		}
	}

	protected void ToggleSlot(int slotID)
	{
		if (slotID >= 0 && slotID < slotIDs.Length)
		{
			quickSlotToggled[slotID] = !quickSlotToggled[slotID];
			NotifyToggleSlot(slotID);
		}
	}

	public void GetAllStorages(List<IItemsContainer> containers)
	{
		containers.Clear();
		for (int i = 0; i < slotIDs.Length; i++)
		{
			ItemsContainer storageInSlot = GetStorageInSlot(i, TechType.VehicleStorageModule);
			if (storageInSlot != null)
			{
				containers.Add(storageInSlot);
			}
		}
	}

	public ItemsContainer GetStorageInSlot(int slotID, TechType techType)
	{
		InventoryItem slotItem = GetSlotItem(slotID);
		if (slotItem == null)
		{
			return null;
		}
		Pickupable item = slotItem.item;
		if (item.GetTechType() != techType)
		{
			return null;
		}
		SeamothStorageContainer component = item.GetComponent<SeamothStorageContainer>();
		if (component == null)
		{
			return null;
		}
		return component.container;
	}

	protected void ChargeModule(TechType techType, int slotID)
	{
		float num = quickSlotCharge[slotID];
		float maxCharge = TechData.GetMaxCharge(techType);
		TechData.GetEnergyCost(techType, out var result);
		float num2 = result * Time.deltaTime;
		float num3 = maxCharge - num;
		bool flag = num2 >= num3;
		float b = (flag ? Mathf.Max(0f, num3) : num2);
		int sourceCount;
		float num4 = Mathf.Min(energyInterface.TotalCanProvide(out sourceCount), b);
		ConsumeEnergy(num4);
		quickSlotCharge[slotID] += num4;
		if (quickSlotCharge[slotID] > 0f && (flag || num4 == 0f))
		{
			OnUpgradeModuleUse(techType, slotID);
			quickSlotCharge[slotID] = 0f;
		}
	}

	private void ResetSlotsState()
	{
		int i = 0;
		for (int num = slotIDs.Length; i < num; i++)
		{
			TechType techType;
			QuickSlotType quickSlotType = GetQuickSlotType(i, out techType);
			if (quickSlotType == QuickSlotType.Toggleable || quickSlotType == QuickSlotType.Selectable || quickSlotType == QuickSlotType.SelectableChargeable)
			{
				ToggleSlot(i, state: false);
			}
			quickSlotTimeUsed[i] = 0f;
			quickSlotCooldown[i] = 0f;
			quickSlotCharge[i] = 0f;
		}
		activeSlot = -1;
		NotifySelectSlot(activeSlot);
	}

	protected virtual QuickSlotType GetQuickSlotType(int slotID, out TechType techType)
	{
		if (slotID >= 0 && slotID < slotIDs.Length)
		{
			techType = modules.GetTechTypeInSlot(slotIDs[slotID]);
			if (techType != 0)
			{
				return TechData.GetSlotType(techType);
			}
		}
		techType = TechType.None;
		return QuickSlotType.None;
	}

	public static bool TorpedoShot(ItemsContainer container, TorpedoType torpedoType, Transform muzzle)
	{
		if (torpedoType != null && container.DestroyItem(torpedoType.techType))
		{
			GameObject obj = UnityEngine.Object.Instantiate(torpedoType.prefab);
			obj.GetComponent<Transform>();
			SeamothTorpedo component = obj.GetComponent<SeamothTorpedo>();
			_ = Vector3.zero;
			Transform aimingTransform = Player.main.camRoot.GetAimingTransform();
			Rigidbody componentInParent = muzzle.GetComponentInParent<Rigidbody>();
			component.Shoot(speed: Vector3.Dot(rhs: (componentInParent != null) ? componentInParent.velocity : Vector3.zero, lhs: aimingTransform.forward), position: muzzle.position, rotation: aimingTransform.rotation, lifeTime: -1f);
			return true;
		}
		return false;
	}
}
