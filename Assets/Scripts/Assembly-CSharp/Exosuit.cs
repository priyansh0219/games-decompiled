using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FMOD.Studio;
using ProtoBuf;
using RootMotion.FinalIK;
using UWE;
using UnityEngine;

[ProtoContract]
public class Exosuit : Vehicle, IGroundMoveable
{
	public enum Arm
	{
		Left = 0,
		Right = 1
	}

	[Serializable]
	public struct ExosuitArmPrefab
	{
		public TechType techType;

		public GameObject prefab;
	}

	private static readonly Dictionary<TechType, float> crushDepths = new Dictionary<TechType, float>
	{
		{
			TechType.SeamothReinforcementModule,
			800f
		},
		{
			TechType.VehicleHullModule1,
			150f
		},
		{
			TechType.VehicleHullModule2,
			400f
		},
		{
			TechType.VehicleHullModule3,
			800f
		},
		{
			TechType.ExoHullModule1,
			400f
		},
		{
			TechType.ExoHullModule2,
			800f
		}
	};

	public const string leftArmID = "ExosuitArmLeft";

	public const string rightArmID = "ExosuitArmRight";

	private static readonly string[] _slotIDs = new string[6] { "ExosuitArmLeft", "ExosuitArmRight", "ExosuitModule1", "ExosuitModule2", "ExosuitModule3", "ExosuitModule4" };

	public FMOD_CustomLoopingEmitter ambienceSound;

	public FMOD_CustomLoopingEmitter loopingJetSound;

	public FMODAsset landSound;

	public Transform bottomTransform;

	public FMODAsset jumpSound;

	public Transform leftArmAttach;

	public Transform rightArmAttach;

	public ExosuitArmPrefab[] armPrefabs;

	public VFXController fxcontrol;

	[AssertNotNull]
	public StorageContainer storageContainer;

	public float xzSpeed;

	public Transform storageFlap;

	public CanvasRenderer nameLabelCanvas;

	public GameObject mainModel;

	public GameObject dynamicModel;

	public Transform aimTargetLeft;

	public Transform aimTargetRight;

	public Transform dropTarget;

	public AnimationCurve thermalReactorCharge;

	public float timeForFullVirbation = 1f;

	private AimIK[] aimIK;

	private const float jumpInterval = 1f;

	private const float jumpForce = 5f;

	private const float upgradedJumpForce = 7f;

	private const float jetAccel = 6.5f;

	private const float upgradedJetAccel = 8.5f;

	private const float aboveWaterDrag = 0.1f;

	private const float underWaterDrag = 0.2f;

	private const float onGroundDrag = 3f;

	private const float flyingAccelScale = 0.22f;

	private const float upgradedFlyingAccelScale = 0.3f;

	private const float kinematicDistanceSqr = 1296f;

	private const int armsCount = 2;

	private const float additionalGrapplingGravity = 3f;

	[AssertLocalization]
	private const string storageShrinkError = "ExosuitStorageShrinkError";

	private float thrustPower = 1f;

	private float thrustConsumption = 0.09f;

	private bool jetDownLastFrame;

	private bool _jetsActive;

	private float timeJetsActiveChanged;

	private float openedFraction;

	private Animator[] animators;

	private float timeLastJumped = -10f;

	private float thrustIntensity;

	private bool IKenabled;

	private const float moveEnergyPerSecond = 1f / 12f;

	private const float jumpEnergyCost = 1.2f;

	private IExosuitArm leftArm;

	private IExosuitArm rightArm;

	private GameObject activeTarget;

	private TechType currentLeftArmType;

	private TechType currentRightArmType;

	private bool jumpJetsUpgraded;

	private const int storageWidth = 6;

	private const int storageHeight = 4;

	private const int storageModuleHeight = 1;

	private Vector3 smoothedVelocity = Vector3.zero;

	private const float smoothSpeed = 4f;

	private PARAMETER_ID fmodIndexSpeed = FMODUWE.invalidParameterId;

	private PARAMETER_ID fmodIndexRotate = FMODUWE.invalidParameterId;

	private bool _playerFullyEntered;

	private bool leftButtonDownProcessed;

	private bool rightButtonDownProcessed;

	private StringBuilder sb = new StringBuilder();

	private float startFlapPitch;

	private bool firstColorRegenerate = true;

	private bool armsDirty = true;

	private bool destroyed;

	private bool rotationDirty;

	private bool _cinematicMode;

	private Color lightColor;

	private string uiStringPrimary;

	private bool areFXPlaying;

	[AssertLocalization(1)]
	private const string pressToExitButtonFormat = "PressToExit";

	[AssertLocalization(1)]
	private const string propulsionCannonToReleaseButtonFormat = "PropulsionCannonToRelease";

	private bool hasInitStrings;

	private bool lastHasPropCannon;

	private List<ExosuitGrapplingArm> grapplingArms = new List<ExosuitGrapplingArm>();

	private string[] colorParams = new string[5] { "_Color", "_Tint", "_Color", "_Color2", "_Color3" };

	private string[] specParams = new string[5] { "_SpecColor", "", "_SpecColor", "_SpecColor2", "_SpecColor3" };

	private const int nameLabel = 1;

	private const int interior = 2;

	protected override string[] slotIDs => _slotIDs;

	protected override string vehicleDefaultName
	{
		get
		{
			Language main = Language.main;
			if (!(main != null))
			{
				return "EXOSUIT";
			}
			return main.Get("ExosuitDefaultName");
		}
	}

	protected override Vector3[] vehicleDefaultColors => new Vector3[5]
	{
		new Vector3(0f, 0f, 1f),
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, 0f, 1f),
		new Vector3(0.577f, 0.447f, 0.604f),
		new Vector3(0.114f, 0.729f, 0.965f)
	};

	private bool jetsActive
	{
		get
		{
			return _jetsActive;
		}
		set
		{
			if (_jetsActive != value)
			{
				timeJetsActiveChanged = Time.time;
				_jetsActive = value;
			}
		}
	}

	public TechType leftArmType => currentLeftArmType;

	public TechType rightArmType => currentRightArmType;

	public override bool playerFullyEntered
	{
		get
		{
			return _playerFullyEntered;
		}
		protected set
		{
			_playerFullyEntered = value;
			if (_playerFullyEntered)
			{
				Player.main.armsController.SetWorldIKTarget(leftHandPlug, rightHandPlug);
			}
			else
			{
				Player.main.armsController.SetWorldIKTarget(null, null);
			}
		}
	}

	public bool cinematicMode
	{
		get
		{
			return _cinematicMode;
		}
		set
		{
			if (_cinematicMode && !value)
			{
				rotationDirty = true;
			}
			_cinematicMode = value;
		}
	}

	public override void Awake()
	{
		base.Awake();
		Equipment equipment = base.modules;
		equipment.isAllowedToRemove = (IsAllowedToRemove)Delegate.Combine(equipment.isAllowedToRemove, new IsAllowedToRemove(IsAllowedToRemove));
		Equipment equipment2 = base.modules;
		equipment2.isAllowedToAdd = (IsAllowedToAdd)Delegate.Combine(equipment2.isAllowedToAdd, new IsAllowedToAdd(IsAllowedToAdd));
	}

	public override void Start()
	{
		base.Start();
		SetRotationLocked(locked: true);
		UpdateExosuitArms();
		UpdateStorageSize();
		startFlapPitch = storageFlap.localEulerAngles.x;
		aimIK = GetComponentsInChildren<AimIK>();
		SetIKEnabled(enabled: true);
		UpdateColliders();
		Light componentInChildren = base.transform.GetComponentInChildren<Light>();
		if (componentInChildren != null)
		{
			lightColor = componentInChildren.color;
		}
	}

	public void OnDestroy()
	{
		destroyed = true;
	}

	public override bool CanPilot()
	{
		if (!FPSInputModule.current.lockMovement)
		{
			return IsPowered();
		}
		return false;
	}

	public void MarkArmsDirty()
	{
		armsDirty = true;
		currentLeftArmType = TechType.None;
		currentRightArmType = TechType.None;
	}

	private void OnEnable()
	{
		MarkArmsDirty();
		Language.OnLanguageChanged += OnLanguageChanged;
		GameInput.OnBindingsChanged += OnBindingsChanged;
	}

	private void OnDisable()
	{
		Language.OnLanguageChanged -= OnLanguageChanged;
		GameInput.OnBindingsChanged -= OnBindingsChanged;
	}

	private void OnLanguageChanged()
	{
		hasInitStrings = false;
	}

	private void OnBindingsChanged()
	{
		hasInitStrings = false;
	}

	public bool HasClaw()
	{
		if (currentLeftArmType != TechType.ExosuitClawArmModule)
		{
			return currentRightArmType == TechType.ExosuitClawArmModule;
		}
		return true;
	}

	public bool HasDrill()
	{
		if (currentLeftArmType != TechType.ExosuitDrillArmModule)
		{
			return currentRightArmType == TechType.ExosuitDrillArmModule;
		}
		return true;
	}

	protected override void EnterVehicle(Player player, bool teleport, bool playEnterAnimation = true)
	{
		base.EnterVehicle(player, teleport, playEnterAnimation);
		if (!playEnterAnimation)
		{
			playerFullyEntered = true;
		}
		if (player != null)
		{
			player.precursorOutOfWater = precursorOutOfWater;
		}
	}

	public void OnPlayerEntered()
	{
		if (GetComponentInChildren<Player>() != null)
		{
			playerFullyEntered = true;
		}
	}

	public override void Update()
	{
		base.Update();
		UpdateThermalReactorCharge();
		if (storageContainer.GetOpen())
		{
			openedFraction = Mathf.Clamp01(openedFraction + Time.deltaTime * 2f);
		}
		else
		{
			openedFraction = Mathf.Clamp01(openedFraction - Time.deltaTime * 2f);
		}
		storageFlap.localEulerAngles = new Vector3(startFlapPitch + openedFraction * 80f, 0f, 0f);
		bool pilotingMode = GetPilotingMode();
		bool flag = onGround || Time.time - timeOnGround <= 0.5f;
		mainAnimator.SetBool("sit", !pilotingMode && flag && !IsUnderwater());
		bool flag2 = pilotingMode && !base.docked;
		if (pilotingMode)
		{
			Player.main.transform.localPosition = Vector3.zero;
			Player.main.transform.localRotation = Quaternion.identity;
			Vector3 vector = ((!base.IsAutopilotEnabled) ? (AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero) : CalculateAutopilotLocalWishDir());
			bool flag3 = vector.y > 0f;
			bool flag4 = IsPowered() && liveMixin.IsAlive();
			if (flag3 && flag4)
			{
				thrustPower = Mathf.Clamp01(thrustPower - Time.deltaTime * thrustConsumption);
				if ((onGround || Time.time - timeOnGround <= 1f) && !jetDownLastFrame)
				{
					ApplyJumpForce();
				}
				jetsActive = true;
			}
			else
			{
				jetsActive = false;
				float num = Time.deltaTime * thrustConsumption * 0.7f;
				if (onGround)
				{
					num = Time.deltaTime * thrustConsumption * 4f;
				}
				thrustPower = Mathf.Clamp01(thrustPower + num);
			}
			jetDownLastFrame = flag3;
			if (timeJetsActiveChanged + 0.3f <= Time.time)
			{
				if (jetsActive && thrustPower > 0f)
				{
					loopingJetSound.Play();
					fxcontrol.Play(0);
					areFXPlaying = true;
				}
				else if (areFXPlaying)
				{
					loopingJetSound.Stop();
					fxcontrol.Stop(0);
					areFXPlaying = false;
				}
			}
			if (flag3 || vector.x != 0f || vector.z != 0f)
			{
				ConsumeEngineEnergy(1f / 12f * Time.deltaTime);
			}
			if (jetsActive)
			{
				thrustIntensity += Time.deltaTime / timeForFullVirbation;
			}
			else
			{
				thrustIntensity -= Time.deltaTime * 10f;
			}
			thrustIntensity = Mathf.Clamp01(thrustIntensity);
			if (AvatarInputHandler.main.IsEnabled() && !ignoreInput)
			{
				Vector3 eulerAngles = base.transform.eulerAngles;
				eulerAngles.x = MainCamera.camera.transform.eulerAngles.x;
				Quaternion aimDirection = Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
				Quaternion aimDirection2 = aimDirection;
				leftArm.Update(ref aimDirection);
				rightArm.Update(ref aimDirection2);
				if (flag2)
				{
					Vector3 b = MainCamera.camera.transform.position + aimDirection * Vector3.forward * 100f;
					Vector3 b2 = MainCamera.camera.transform.position + aimDirection2 * Vector3.forward * 100f;
					aimTargetLeft.transform.position = Vector3.Lerp(aimTargetLeft.transform.position, b, Time.deltaTime * 15f);
					aimTargetRight.transform.position = Vector3.Lerp(aimTargetRight.transform.position, b2, Time.deltaTime * 15f);
				}
				bool hasPropCannon = rightArm is ExosuitPropulsionArm || leftArm is ExosuitPropulsionArm;
				UpdateUIText(hasPropCannon);
				if (GameInput.GetButtonDown(GameInput.Button.AltTool) && !rightArm.OnAltDown())
				{
					leftArm.OnAltDown();
				}
			}
			UpdateActiveTarget();
			UpdateSounds();
		}
		if (!flag2)
		{
			bool flag5 = false;
			bool flag6 = false;
			if (!Mathf.Approximately(aimTargetLeft.transform.localPosition.y, 0f))
			{
				float y = Mathf.MoveTowards(aimTargetLeft.transform.localPosition.y, 0f, Time.deltaTime * 50f);
				aimTargetLeft.transform.localPosition = new Vector3(aimTargetLeft.transform.localPosition.x, y, aimTargetLeft.transform.localPosition.z);
			}
			else
			{
				flag5 = true;
			}
			if (!Mathf.Approximately(aimTargetRight.transform.localPosition.y, 0f))
			{
				float y2 = Mathf.MoveTowards(aimTargetRight.transform.localPosition.y, 0f, Time.deltaTime * 50f);
				aimTargetRight.transform.localPosition = new Vector3(aimTargetRight.transform.localPosition.x, y2, aimTargetRight.transform.localPosition.z);
			}
			else
			{
				flag6 = true;
			}
			if (flag5 && flag6)
			{
				SetIKEnabled(enabled: false);
			}
		}
		UpdateAnimations();
		if (armsDirty)
		{
			UpdateExosuitArms();
		}
	}

	private void UpdateUIText(bool hasPropCannon)
	{
		if (!hasInitStrings || lastHasPropCannon != hasPropCannon)
		{
			sb.Length = 0;
			sb.AppendLine(LanguageCache.GetButtonFormat("PressToExit", GameInput.Button.Exit));
			if (hasPropCannon)
			{
				sb.AppendLine(LanguageCache.GetButtonFormat("PropulsionCannonToRelease", GameInput.Button.AltTool));
			}
			lastHasPropCannon = hasPropCannon;
			uiStringPrimary = sb.ToString();
		}
		HandReticle.main.SetTextRaw(HandReticle.TextType.Use, uiStringPrimary);
		HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, string.Empty);
		hasInitStrings = true;
	}

	public void GetHUDValues(out float health, out float power, out float thrust)
	{
		health = liveMixin.GetHealthFraction();
		GetEnergyValues(out var charge, out var capacity);
		power = ((charge > 0f && capacity > 0f) ? (charge / capacity) : 0f);
		thrust = thrustPower;
	}

	private void UpdateThermalReactorCharge()
	{
		if (base.modules.GetCount(TechType.ExosuitThermalReactorModule) > 0)
		{
			float temperature = GetTemperature();
			float num = thermalReactorCharge.Evaluate(temperature);
			AddEnergy(num * Time.deltaTime);
		}
	}

	protected override bool CanLand()
	{
		if (Time.time - timeLastJumped <= 0.5f)
		{
			return false;
		}
		return base.CanLand();
	}

	protected override void OnLand()
	{
		Utils.PlayFMODAsset(landSound, bottomTransform);
		fxcontrol.Play(2);
	}

	private void UpdateSounds()
	{
		if (FMODUWE.IsInvalidParameterId(fmodIndexSpeed))
		{
			fmodIndexSpeed = ambienceSound.GetParameterIndex("speed");
		}
		if (FMODUWE.IsInvalidParameterId(fmodIndexRotate))
		{
			fmodIndexRotate = ambienceSound.GetParameterIndex("rotate");
		}
		ambienceSound.SetParameterValue(fmodIndexSpeed, useRigidbody.velocity.magnitude);
		ambienceSound.SetParameterValue(fmodIndexRotate, useRigidbody.angularVelocity.magnitude);
	}

	public GameObject GetActiveTarget()
	{
		return activeTarget;
	}

	public GameObject GetInteractableRoot(GameObject target)
	{
		if (target == null)
		{
			return null;
		}
		GameObject entityRoot = UWE.Utils.GetEntityRoot(target);
		entityRoot = ((entityRoot != null) ? entityRoot : target);
		target = rightArm.GetInteractableRoot(entityRoot);
		if (target == null)
		{
			target = leftArm.GetInteractableRoot(entityRoot);
		}
		return target;
	}

	public IExosuitArm GetArm(int armIndex)
	{
		switch (armIndex)
		{
		case 0:
			return leftArm;
		case 1:
			return rightArm;
		default:
			return null;
		}
	}

	private void UpdateActiveTarget()
	{
		GameObject result = null;
		Targeting.GetTarget(base.gameObject, 6f, out result, out var _);
		activeTarget = GetInteractableRoot(result);
		if (activeTarget != null)
		{
			GUIHand component = Player.main.GetComponent<GUIHand>();
			GUIHand.Send(activeTarget, HandTargetEventType.Hover, component);
		}
	}

	public override void OnCollisionEnter(Collision collision)
	{
		base.OnCollisionEnter(collision);
		useRigidbody.isKinematic = false;
	}

	public bool IsUnderwater()
	{
		if (base.transform.position.y < worldForces.waterDepth + 2f)
		{
			return !precursorOutOfWater;
		}
		return false;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		bool isGrappling = GetIsGrappling();
		worldForces.handleGravity = !onGround;
		bool flag = IsUnderwater();
		if (jetsActive && flag && thrustPower > 0f)
		{
			float num = 0.8f + thrustPower * 0.2f;
			float num2 = Mathf.Clamp01(Mathf.Max(0f, 0f - useRigidbody.velocity.y) / 6f) + 1f;
			useRigidbody.AddForce(Vector3.up * (jumpJetsUpgraded ? 8.5f : 6.5f) * num * num2, ForceMode.Acceleration);
		}
		float drag = 0.2f;
		if (onGround && !isGrappling)
		{
			drag = 3f;
		}
		else if (!flag)
		{
			drag = 0.1f;
		}
		useRigidbody.drag = drag;
		bool isKinematic = ShouldSetKinematic();
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(useRigidbody, isKinematic, setCollisionDetectionMode: true);
		if (isGrappling)
		{
			useRigidbody.AddForce(Vector3.down * 3f, ForceMode.Acceleration);
		}
		if (!cinematicMode && rotationDirty)
		{
			Vector3 localEulerAngles = base.transform.localEulerAngles;
			Quaternion quaternion = Quaternion.Euler(0f, localEulerAngles.y, 0f);
			if (Mathf.Abs(localEulerAngles.x) < 0.001f && Mathf.Abs(localEulerAngles.z) < 0.001f)
			{
				rotationDirty = false;
				base.transform.localRotation = quaternion;
			}
			else
			{
				Quaternion localRotation = base.transform.localRotation;
				base.transform.localRotation = Quaternion.Lerp(localRotation, quaternion, Time.deltaTime * 3f);
			}
		}
	}

	protected override void OnDockedChanged(bool docked, DockType dockType)
	{
		base.OnDockedChanged(docked, dockType);
		UpdateColliders();
		MarkArmsDirty();
	}

	private void UpdateColliders()
	{
		if (leftArm != null)
		{
			Collider[] componentsInChildren = leftArm.GetGameObject().GetComponentsInChildren<Collider>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (!componentsInChildren[i].isTrigger)
				{
					componentsInChildren[i].enabled = !base.docked;
				}
			}
		}
		if (rightArm == null)
		{
			return;
		}
		Collider[] componentsInChildren2 = rightArm.GetGameObject().GetComponentsInChildren<Collider>(includeInactive: true);
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			if (!componentsInChildren2[j].isTrigger)
			{
				componentsInChildren2[j].enabled = !base.docked;
			}
		}
	}

	private void ApplyJumpForce()
	{
		if (timeLastJumped + 1f <= Time.time)
		{
			if (onGround)
			{
				fxcontrol.Play(1);
				Utils.PlayFMODAsset(jumpSound, base.transform);
			}
			ConsumeEngineEnergy(1.2f);
			useRigidbody.AddForce(Vector3.up * (jumpJetsUpgraded ? 7f : 5f), ForceMode.VelocityChange);
			timeLastJumped = Time.time;
			timeOnGround = 0f;
			onGround = false;
		}
	}

	private void SetIKEnabled(bool enabled)
	{
		if (enabled == IKenabled)
		{
			return;
		}
		for (int i = 0; i < aimIK.Length; i++)
		{
			if (enabled)
			{
				aimIK[i].solver.IKPositionWeight = 1f;
			}
			else
			{
				aimIK[i].solver.IKPositionWeight = 0f;
			}
		}
		IKenabled = enabled;
	}

	public override void SetPlayerInside(bool inside)
	{
		base.SetPlayerInside(inside);
		Player.main.inExosuit = inside;
		playerFullyEntered = true;
	}

	protected override void OnPilotModeBegin()
	{
		base.OnPilotModeBegin();
		SetIKEnabled(enabled: true);
		ambienceSound.Play();
		Player.main.inExosuit = true;
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(useRigidbody, isKinematic: false);
		thrustIntensity = 0f;
	}

	protected override void OnPilotModeEnd()
	{
		base.OnPilotModeEnd();
		loopingJetSound.Stop();
		jetsActive = false;
		activeTarget = null;
		leftArm.ResetArm();
		rightArm.ResetArm();
		playerFullyEntered = false;
		ambienceSound.Stop();
		Player.main.inExosuit = false;
		fxcontrol.Stop(0);
		Player.main.playerAnimator.SetBool("exosuit_use_left", value: false);
		Player.main.playerAnimator.SetBool("exosuit_use_right", value: false);
		mainAnimator.SetBool("use_tool_left", value: false);
		mainAnimator.SetBool("use_tool_right", value: false);
	}

	public override bool IsToggled(int slotID)
	{
		if (slotID == GetSlotIndex("ExosuitArmLeft") || slotID == GetSlotIndex("ExosuitArmRight"))
		{
			return true;
		}
		return base.IsToggled(slotID);
	}

	public override TechType[] GetSlotBinding()
	{
		int num = slotIDs.Length;
		TechType[] array = new TechType[num];
		for (int i = 0; i < num; i++)
		{
			TechType techType = base.modules.GetTechTypeInSlot(slotIDs[i]);
			if (techType == TechType.None && (i == GetSlotIndex("ExosuitArmLeft") || i == GetSlotIndex("ExosuitArmRight")))
			{
				techType = TechType.ExosuitClawArmModule;
			}
			array[i] = techType;
		}
		return array;
	}

	public override TechType GetSlotBinding(int slotID)
	{
		if (slotID < 0 || slotID >= slotIDs.Length)
		{
			return TechType.None;
		}
		string slot = slotIDs[slotID];
		TechType techType = base.modules.GetTechTypeInSlot(slot);
		if (techType == TechType.None && (slotID == GetSlotIndex("ExosuitArmLeft") || slotID == GetSlotIndex("ExosuitArmRight")))
		{
			techType = TechType.ExosuitClawArmModule;
		}
		return techType;
	}

	public override InventoryItem GetSlotItem(int slotID)
	{
		if (slotID < 0 || slotID >= slotIDs.Length)
		{
			return null;
		}
		string slot = slotIDs[slotID];
		return base.modules.GetItemInSlot(slot);
	}

	public override void SlotKeyDown(int slotID)
	{
		if (playerFullyEntered && !ignoreInput)
		{
			slotID += 2;
			if (slotID >= 0 && slotID < slotIDs.Length)
			{
				base.SlotKeyDown(slotID);
			}
		}
	}

	public override void SlotKeyHeld(int slotID)
	{
		if (playerFullyEntered && !ignoreInput)
		{
			slotID += 2;
			if (slotID >= 0 && slotID < slotIDs.Length)
			{
				base.SlotKeyHeld(slotID);
			}
		}
	}

	public override void SlotKeyUp(int slotID)
	{
		if (playerFullyEntered && !ignoreInput)
		{
			slotID += 2;
			int num = slotIDs.Length - 2;
			if (slotID >= 0 && slotID < num)
			{
				base.SlotKeyUp(slotID);
			}
		}
	}

	public override void SlotNext()
	{
		if (!playerFullyEntered || ignoreInput)
		{
			return;
		}
		int num = GetActiveSlotID() - 2;
		int num2 = GetSlotCount() - 2;
		int num3 = ((num >= 0) ? (num + 1) : 0);
		if (num3 >= num2)
		{
			num3 = 0;
		}
		int num4 = num3;
		do
		{
			TechType techType;
			QuickSlotType quickSlotType = GetQuickSlotType(num4 + 2, out techType);
			if (quickSlotType == QuickSlotType.Selectable || quickSlotType == QuickSlotType.SelectableChargeable)
			{
				SlotKeyDown(num4);
				break;
			}
			num4++;
			if (num4 >= num2)
			{
				num4 = 0;
			}
		}
		while (num4 != num3);
	}

	public override void SlotPrevious()
	{
		if (!playerFullyEntered || ignoreInput)
		{
			return;
		}
		int num = GetActiveSlotID() - 2;
		int num2 = GetSlotCount() - 2;
		int num3 = ((num < 0) ? (num2 - 1) : (num - 1));
		if (num3 < 0)
		{
			num3 = num2 - 1;
		}
		int num4 = num3;
		do
		{
			TechType techType;
			QuickSlotType quickSlotType = GetQuickSlotType(num4 + 2, out techType);
			if (quickSlotType == QuickSlotType.Selectable || quickSlotType == QuickSlotType.SelectableChargeable)
			{
				SlotKeyDown(num4);
				break;
			}
			num4--;
			if (num4 < 0)
			{
				num4 = num2 - 1;
			}
		}
		while (num4 != num3);
	}

	public override void SlotLeftDown()
	{
		if (playerFullyEntered && AvatarInputHandler.main.IsEnabled() && !ignoreInput)
		{
			leftButtonDownProcessed = true;
			int slotIndex = GetSlotIndex("ExosuitArmLeft");
			if (IsPowered() && GetQuickSlotCooldown(slotIndex) == 1f && GetQuickSlotType(slotIndex, out var techType) == QuickSlotType.Selectable && ConsumeEnergy(techType) && leftArm.OnUseDown(out var cooldownDuration))
			{
				Player.main.playerAnimator.SetBool("exosuit_use_left", value: true);
				mainAnimator.SetBool("use_tool_left", value: true);
				quickSlotTimeUsed[slotIndex] = Time.time;
				quickSlotCooldown[slotIndex] = cooldownDuration;
			}
		}
	}

	public override void SlotLeftHeld()
	{
		if (!playerFullyEntered || !AvatarInputHandler.main.IsEnabled() || ignoreInput || !leftButtonDownProcessed)
		{
			return;
		}
		int slotIndex = GetSlotIndex("ExosuitArmLeft");
		if (!IsPowered() || GetQuickSlotCooldown(slotIndex) != 1f)
		{
			return;
		}
		TechType techType;
		QuickSlotType quickSlotType = GetQuickSlotType(slotIndex, out techType);
		if (quickSlotType == QuickSlotType.Selectable && leftArm.OnUseHeld(out var cooldownDuration))
		{
			quickSlotTimeUsed[slotIndex] = Time.time;
			quickSlotCooldown[slotIndex] = cooldownDuration;
		}
		if (quickSlotType == QuickSlotType.SelectableChargeable)
		{
			ChargeModule(techType, slotIndex);
			if (leftArm.OnUseHeld(out var cooldownDuration2))
			{
				quickSlotTimeUsed[slotIndex] = Time.time;
				quickSlotCooldown[slotIndex] = cooldownDuration2;
			}
		}
	}

	public override void SlotLeftUp()
	{
		leftButtonDownProcessed = false;
		if (!playerFullyEntered || !AvatarInputHandler.main.IsEnabled() || ignoreInput)
		{
			return;
		}
		int slotIndex = GetSlotIndex("ExosuitArmLeft");
		TechType techType;
		switch (GetQuickSlotType(slotIndex, out techType))
		{
		case QuickSlotType.Selectable:
		{
			Player.main.playerAnimator.SetBool("exosuit_use_left", value: false);
			mainAnimator.SetBool("use_tool_left", value: false);
			leftArm.OnUseUp(out var _);
			break;
		}
		case QuickSlotType.SelectableChargeable:
			if (IsPowered() && GetQuickSlotCooldown(slotIndex) == 1f)
			{
				if (leftArm.OnUseUp(out var cooldownDuration))
				{
					Player.main.playerAnimator.SetBool("exosuit_use_left", value: false);
					mainAnimator.SetBool("use_tool_left", value: false);
					quickSlotTimeUsed[slotIndex] = Time.time;
					quickSlotCooldown[slotIndex] = cooldownDuration;
				}
				quickSlotCharge[slotIndex] = 0f;
			}
			break;
		}
	}

	public override void SlotRightDown()
	{
		if (playerFullyEntered && AvatarInputHandler.main.IsEnabled() && !ignoreInput)
		{
			rightButtonDownProcessed = true;
			int slotIndex = GetSlotIndex("ExosuitArmRight");
			if (IsPowered() && GetQuickSlotCooldown(slotIndex) == 1f && GetQuickSlotType(slotIndex, out var techType) == QuickSlotType.Selectable && ConsumeEnergy(techType) && rightArm.OnUseDown(out var cooldownDuration))
			{
				Player.main.playerAnimator.SetBool("exosuit_use_right", value: true);
				mainAnimator.SetBool("use_tool_right", value: true);
				quickSlotTimeUsed[slotIndex] = Time.time;
				quickSlotCooldown[slotIndex] = cooldownDuration;
			}
		}
	}

	public override void SlotRightHeld()
	{
		if (!playerFullyEntered || !AvatarInputHandler.main.IsEnabled() || ignoreInput || !rightButtonDownProcessed)
		{
			return;
		}
		int slotIndex = GetSlotIndex("ExosuitArmRight");
		if (!IsPowered() || GetQuickSlotCooldown(slotIndex) != 1f)
		{
			return;
		}
		TechType techType;
		switch (GetQuickSlotType(slotIndex, out techType))
		{
		case QuickSlotType.Selectable:
		{
			if (rightArm.OnUseHeld(out var cooldownDuration2))
			{
				quickSlotTimeUsed[slotIndex] = Time.time;
				quickSlotCooldown[slotIndex] = cooldownDuration2;
			}
			break;
		}
		case QuickSlotType.SelectableChargeable:
		{
			ChargeModule(techType, slotIndex);
			if (rightArm.OnUseHeld(out var cooldownDuration))
			{
				quickSlotTimeUsed[slotIndex] = Time.time;
				quickSlotCooldown[slotIndex] = cooldownDuration;
			}
			break;
		}
		}
	}

	public override void SlotRightUp()
	{
		rightButtonDownProcessed = false;
		if (!playerFullyEntered || !AvatarInputHandler.main.IsEnabled() || ignoreInput)
		{
			return;
		}
		int slotIndex = GetSlotIndex("ExosuitArmRight");
		TechType techType;
		switch (GetQuickSlotType(slotIndex, out techType))
		{
		case QuickSlotType.Selectable:
		{
			Player.main.playerAnimator.SetBool("exosuit_use_right", value: false);
			mainAnimator.SetBool("use_tool_right", value: false);
			rightArm.OnUseUp(out var _);
			break;
		}
		case QuickSlotType.SelectableChargeable:
			if (IsPowered() && GetQuickSlotCooldown(slotIndex) == 1f)
			{
				if (rightArm.OnUseUp(out var cooldownDuration))
				{
					Player.main.playerAnimator.SetBool("exosuit_use_right", value: false);
					mainAnimator.SetBool("use_tool_right", value: false);
					quickSlotTimeUsed[slotIndex] = Time.time;
					quickSlotCooldown[slotIndex] = cooldownDuration;
				}
				quickSlotCharge[slotIndex] = 0f;
			}
			break;
		}
	}

	public override void DeselectSlots()
	{
		if (playerFullyEntered && !ignoreInput)
		{
			base.DeselectSlots();
		}
	}

	private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
	{
		if (pickupable.GetTechType() == TechType.VehicleStorageModule)
		{
			bool flag = storageContainer.container.HasRoomFor(6, 1);
			if (verbose && !flag)
			{
				ErrorMessage.AddDebug(Language.main.Get("ExosuitStorageShrinkError"));
			}
			return flag;
		}
		return true;
	}

	private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
	{
		TechType techType = pickupable.GetTechType();
		if (techType == TechType.ExosuitJetUpgradeModule || techType == TechType.ExosuitThermalReactorModule)
		{
			return base.modules.GetCount(techType) < 1;
		}
		return true;
	}

	protected override QuickSlotType GetQuickSlotType(int slotID, out TechType techType)
	{
		if (slotID >= 0 && slotID < slotIDs.Length)
		{
			techType = base.modules.GetTechTypeInSlot(slotIDs[slotID]);
			if (techType == TechType.None && (slotID == GetSlotIndex("ExosuitArmLeft") || slotID == GetSlotIndex("ExosuitArmRight")))
			{
				techType = TechType.ExosuitClawArmModule;
			}
			if (techType != 0)
			{
				return TechData.GetSlotType(techType);
			}
		}
		techType = TechType.None;
		return QuickSlotType.None;
	}

	protected override void OnUpgradeModuleChange(int slotID, TechType techType, bool added)
	{
		switch (techType)
		{
		case TechType.ExosuitDrillArmModule:
		case TechType.ExosuitPropulsionArmModule:
		case TechType.ExosuitGrapplingArmModule:
		case TechType.ExosuitTorpedoArmModule:
			MarkArmsDirty();
			break;
		case TechType.VehicleHullModule1:
		case TechType.VehicleHullModule2:
		case TechType.VehicleHullModule3:
		case TechType.ExoHullModule1:
		case TechType.ExoHullModule2:
		{
			float num = 0f;
			for (int i = 0; i < slotIDs.Length; i++)
			{
				string slot = slotIDs[i];
				TechType techTypeInSlot = base.modules.GetTechTypeInSlot(slot);
				if (crushDepths.TryGetValue(techTypeInSlot, out var value) && value > num)
				{
					num = value;
				}
			}
			crushDamage.SetExtraCrushDepth(num);
			break;
		}
		case TechType.ExosuitJetUpgradeModule:
			jumpJetsUpgraded = added || base.modules.GetCount(TechType.ExosuitJetUpgradeModule) > 0;
			break;
		case TechType.VehicleStorageModule:
			UpdateStorageSize();
			break;
		default:
			base.OnUpgradeModuleChange(slotID, techType, added);
			break;
		}
	}

	private void UpdateStorageSize()
	{
		int height = 4 + base.modules.GetCount(TechType.VehicleStorageModule);
		storageContainer.Resize(6, height);
	}

	private bool GetIsGrappling()
	{
		for (int i = 0; i < grapplingArms.Count; i++)
		{
			if (grapplingArms[i].GetIsGrappling())
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateAnimations()
	{
		Vector3 velocity = useRigidbody.velocity;
		Vector3 b = base.transform.InverseTransformVector(velocity);
		bool flag = true;
		if (b.sqrMagnitude < 0.2f)
		{
			b = Vector3.zero;
			flag = false;
		}
		smoothedVelocity = Vector3.Slerp(smoothedVelocity, b, 4f * Time.deltaTime);
		bool flag2 = Time.time - timeLastJumped <= 0.5f;
		bool flag3 = Time.time - timeOnGround <= 0.5f;
		bool value = (!jetsActive && !flag2 && flag3) || !flag;
		mainAnimator.SetFloat("move_speed_x", smoothedVelocity.x);
		mainAnimator.SetFloat("move_speed_y", smoothedVelocity.y);
		mainAnimator.SetFloat("move_speed_z", smoothedVelocity.z);
		mainAnimator.SetBool("onGround", value);
		mainAnimator.SetFloat("thrustIntensity", thrustIntensity);
	}

	protected override void OverrideAcceleration(ref Vector3 acceleration)
	{
		if (!onGround)
		{
			float num = (jumpJetsUpgraded ? 0.3f : 0.22f);
			acceleration.x *= num;
			acceleration.z *= num;
		}
	}

	private GameObject GetArmPrefab(TechType techType)
	{
		GameObject result = null;
		for (int i = 0; i < armPrefabs.Length; i++)
		{
			if (armPrefabs[i].techType == techType)
			{
				result = armPrefabs[i].prefab;
				break;
			}
		}
		return result;
	}

	private IExosuitArm SpawnArm(TechType techType, Transform parent)
	{
		GameObject obj = UnityEngine.Object.Instantiate(GetArmPrefab(techType));
		obj.transform.parent = parent.transform;
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localPosition = Vector3.zero;
		return obj.GetComponent<IExosuitArm>();
	}

	private void UpdateExosuitArms()
	{
		if (destroyed)
		{
			return;
		}
		TechType slotBinding = GetSlotBinding(GetSlotIndex("ExosuitArmLeft"));
		TechType slotBinding2 = GetSlotBinding(GetSlotIndex("ExosuitArmRight"));
		if (currentLeftArmType != slotBinding)
		{
			if (leftArm != null)
			{
				UnityEngine.Object.Destroy(leftArm.GetGameObject());
			}
			leftArm = SpawnArm(slotBinding, leftArmAttach);
			leftArm.SetSide(Arm.Left);
			currentLeftArmType = slotBinding;
		}
		if (currentRightArmType != slotBinding2)
		{
			if (rightArm != null)
			{
				UnityEngine.Object.Destroy(rightArm.GetGameObject());
			}
			rightArm = SpawnArm(slotBinding2, rightArmAttach);
			rightArm.SetSide(Arm.Right);
			currentRightArmType = slotBinding2;
		}
		animators = GetComponentsInChildren<Animator>();
		vfxConstructing.Regenerate();
		RegenerateRenderInfo();
		armsDirty = false;
		grapplingArms.Clear();
		if (slotBinding == TechType.ExosuitGrapplingArmModule)
		{
			grapplingArms.Add(leftArm as ExosuitGrapplingArm);
		}
		if (slotBinding2 == TechType.ExosuitGrapplingArmModule)
		{
			grapplingArms.Add(rightArm as ExosuitGrapplingArm);
		}
		UpdateColliders();
	}

	private void RegenerateRenderInfo()
	{
		Renderer[] componentsInChildren = dynamicModel.GetComponentsInChildren<Renderer>();
		int num = componentsInChildren.Length;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			num2 += componentsInChildren[i].materials.Length;
		}
		Vector3[] array = (firstColorRegenerate ? vehicleColors : subName.GetColors());
		int num3 = array.Length;
		firstColorRegenerate = false;
		subName.rendererInfo = new SubName.ColorData[num3];
		for (int j = 0; j < num3; j++)
		{
			SubName.ColorData colorData = new SubName.ColorData();
			subName.rendererInfo[j] = colorData;
			colorData.defaultHSB = vehicleDefaultColors[j];
			colorData.HSB = array[j];
			switch (j)
			{
			case 1:
				colorData.renderers = new SubName.RenderData[1];
				colorData.renderers[0].canvasRenderer = nameLabelCanvas;
				colorData.renderers[0].colorProperties = new SubName.PropertyData[1]
				{
					new SubName.PropertyData(isSpecular: false, colorParams[j])
				};
				continue;
			case 2:
				colorData.renderers = new SubName.RenderData[1];
				colorData.renderers[0].renderer = mainModel.GetComponent<Renderer>();
				colorData.renderers[0].colorProperties = new SubName.PropertyData[2]
				{
					new SubName.PropertyData(isSpecular: false, colorParams[j]),
					new SubName.PropertyData(isSpecular: true, specParams[j])
				};
				continue;
			}
			colorData.renderers = new SubName.RenderData[num2 + 1];
			colorData.renderers[0] = default(SubName.RenderData);
			colorData.renderers[0].renderer = mainModel.GetComponent<Renderer>();
			colorData.renderers[0].materialIndex = 1;
			colorData.renderers[0].colorProperties = new SubName.PropertyData[2]
			{
				new SubName.PropertyData(isSpecular: false, colorParams[j]),
				new SubName.PropertyData(isSpecular: true, specParams[j])
			};
			int num4 = 1;
			for (int k = 0; k < num; k++)
			{
				for (int l = 0; l < componentsInChildren[k].materials.Length; l++)
				{
					colorData.renderers[num4] = default(SubName.RenderData);
					colorData.renderers[num4].renderer = componentsInChildren[k];
					colorData.renderers[num4].materialIndex = l;
					colorData.renderers[num4].colorProperties = new SubName.PropertyData[2]
					{
						new SubName.PropertyData(isSpecular: false, colorParams[j]),
						new SubName.PropertyData(isSpecular: true, specParams[j])
					};
					num4++;
				}
			}
		}
		subName.DeserializeColors(array);
	}

	public override IEnumerator Undock(Player player, float yUndockedPosition)
	{
		rotationDirty = true;
		return base.Undock(player, yUndockedPosition);
	}

	Vector3 IGroundMoveable.GetVelocity()
	{
		return useRigidbody.velocity;
	}

	bool IGroundMoveable.IsOnGround()
	{
		return onGround;
	}

	bool IGroundMoveable.IsActive()
	{
		return base.enabled;
	}

	VFXSurfaceTypes IGroundMoveable.GetGroundSurfaceType()
	{
		return VFXSurfaceTypes.none;
	}

	Vector3 IGroundMoveable.GetGroundNormal()
	{
		return surfaceNormal;
	}
}
