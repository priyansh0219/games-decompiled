using System.Collections.Generic;
using FMOD.Studio;
using ProtoBuf;
using UWE;
using UnityEngine;

[RequireComponent(typeof(EnergyMixin))]
[RequireComponent(typeof(Rigidbody))]
[ProtoContract]
public class SeaMoth : Vehicle
{
	public const string slot1ID = "SeamothModule1";

	public const string slot2ID = "SeamothModule2";

	public const string slot3ID = "SeamothModule3";

	public const string slot4ID = "SeamothModule4";

	private static readonly string[] _slotIDs = new string[4] { "SeamothModule1", "SeamothModule2", "SeamothModule3", "SeamothModule4" };

	[AssertNotNull]
	public GameObject[] torpedoSilos;

	public Transform torpedoTubeLeft;

	public Transform torpedoTubeRight;

	public FMOD_CustomEmitter sonarSound;

	public FMOD_CustomEmitter enterSeamoth;

	public GameObject seamothElectricalDefensePrefab;

	public FMOD_StudioEventEmitter pulseChargeSound;

	public FMOD_StudioEventEmitter ambienceSound;

	public EngineRpmSFXManager engineSound;

	public ParticleSystem bubbles;

	public string onSpawnGoalText = "";

	public GameObject screenEffectModel;

	public Gradient gradientInner;

	public Gradient gradientOuter;

	public FMODAsset torpedoArmed;

	public FMODAsset torpedoDisarmed;

	public Animator animator;

	public ToggleLights toggleLights;

	[AssertNotNull]
	public VFXVolumetricLight[] volumeticLights;

	public GameObject lightsParent;

	private bool lightsActive = true;

	private float timeLastPlayerModeChange;

	public SeamothStorageInput[] storageInputs;

	public float enginePowerConsumption = 1f / 15f;

	private static readonly TechType[] fragmentTechTypes = new TechType[19]
	{
		TechType.SeamothFragment,
		TechType.StasisRifleFragment,
		TechType.ExosuitFragment,
		TechType.TransfuserFragment,
		TechType.TerraformerFragment,
		TechType.ReinforceHullFragment,
		TechType.WorkbenchFragment,
		TechType.PropulsionCannonFragment,
		TechType.BioreactorFragment,
		TechType.ThermalPlantFragment,
		TechType.NuclearReactorFragment,
		TechType.MoonpoolFragment,
		TechType.BaseFiltrationMachineFragment,
		TechType.BaseBioReactorFragment,
		TechType.BaseNuclearReactorFragment,
		TechType.ExosuitDrillArmFragment,
		TechType.ExosuitGrapplingArmFragment,
		TechType.ExosuitPropulsionArmFragment,
		TechType.ExosuitTorpedoArmFragment
	};

	private float _smoothedMoveSpeed;

	private PARAMETER_ID fmodIndexSpeed = FMODUWE.invalidParameterId;

	private Material rendererMaterial0;

	private Material rendererMaterial1;

	protected override string[] slotIDs => _slotIDs;

	protected override string vehicleDefaultName
	{
		get
		{
			Language main = Language.main;
			if (!(main != null))
			{
				return "SEAMOTH";
			}
			return main.Get("SeamothDefaultName");
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

	protected override void OnDockedChanged(bool docked, DockType dockType)
	{
		base.OnDockedChanged(docked, dockType);
		int i = 0;
		for (int num = storageInputs.Length; i < num; i++)
		{
			SeamothStorageInput seamothStorageInput = storageInputs[i];
			if (seamothStorageInput != null)
			{
				seamothStorageInput.SetDocked(dockType);
			}
		}
	}

	public override void Awake()
	{
		base.Awake();
		base.modules.isAllowedToRemove = IsAllowedToRemove;
	}

	public override void Start()
	{
		base.Start();
		if (onSpawnGoalText != "")
		{
			GoalManager.main.OnCustomGoalEvent(onSpawnGoalText);
		}
		if (screenEffectModel != null)
		{
			screenEffectModel.GetComponent<Renderer>().materials[0].SetColor(ShaderPropertyID._Color, gradientInner.Evaluate(0f));
			screenEffectModel.GetComponent<Renderer>().materials[1].SetColor(ShaderPropertyID._Color, gradientOuter.Evaluate(0f));
		}
		animator = GetComponentInChildren<Animator>();
		toggleLights.lightsCallback += onLightsToggled;
		Utils.GetLocalPlayerComp().playerModeChanged.AddHandler(base.gameObject, OnPlayerModeChanged);
	}

	public override void SetPlayerInside(bool inside)
	{
		base.SetPlayerInside(inside);
		Player.main.inSeamoth = inside;
		playerFullyEntered = true;
	}

	protected override void EnterVehicle(Player player, bool teleport, bool playEnterAnimation = true)
	{
		base.EnterVehicle(player, teleport, playEnterAnimation);
		if (!playEnterAnimation)
		{
			playerFullyEntered = true;
		}
	}

	public void OnPlayerEntered()
	{
		if (GetComponentInChildren<Player>() != null)
		{
			playerFullyEntered = true;
		}
	}

	public override bool GetAllowedToEject()
	{
		return !base.docked;
	}

	private void UpdateDockedAnim()
	{
		animator.SetBool("docked", base.docked);
	}

	public override void Update()
	{
		base.Update();
		UpdateSounds();
		if (GetPilotingMode() && !ignoreInput)
		{
			string buttonFormat = LanguageCache.GetButtonFormat("PressToExit", GameInput.Button.Exit);
			HandReticle.main.SetTextRaw(HandReticle.TextType.Use, buttonFormat);
			HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, string.Empty);
			Vector3 vector = (AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero);
			if (vector.magnitude > 0.1f)
			{
				ConsumeEngineEnergy(Time.deltaTime * enginePowerConsumption * vector.magnitude);
			}
			toggleLights.CheckLightToggle();
		}
		UpdateScreenFX();
		UpdateDockedAnim();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		bool isKinematic = ShouldSetKinematic();
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(useRigidbody, isKinematic);
	}

	public void GetHUDValues(out float health, out float power)
	{
		health = liveMixin.GetHealthFraction();
		GetEnergyValues(out var charge, out var capacity);
		power = ((charge > 0f && capacity > 0f) ? (charge / capacity) : 0f);
	}

	public override void SubConstructionComplete()
	{
		base.SubConstructionComplete();
		lightsParent.SetActive(value: true);
	}

	public void onLightsToggled(bool active)
	{
	}

	private void UpdateScreenFX()
	{
		if (GetPilotingMode())
		{
			float to = Mathf.Clamp(base.transform.InverseTransformDirection(useRigidbody.velocity).z / 10f, 0f, 1f);
			_smoothedMoveSpeed = UWE.Utils.Slerp(_smoothedMoveSpeed, to, Time.deltaTime);
		}
		else
		{
			_smoothedMoveSpeed = 0f;
		}
		if (screenEffectModel != null)
		{
			if (rendererMaterial0 == null)
			{
				Renderer component = screenEffectModel.GetComponent<Renderer>();
				rendererMaterial0 = component.materials[0];
				rendererMaterial1 = component.materials[1];
			}
			Color value = gradientInner.Evaluate(_smoothedMoveSpeed);
			Color value2 = gradientOuter.Evaluate(_smoothedMoveSpeed);
			rendererMaterial0.SetColor(ShaderPropertyID._Color, value);
			rendererMaterial1.SetColor(ShaderPropertyID._Color, value2);
		}
		screenEffectModel.SetActive(_smoothedMoveSpeed > 0f);
	}

	private void UpdateSounds()
	{
		Vector3 vector = (AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero);
		if (CanPilot() && vector.magnitude > 0f && GetPilotingMode())
		{
			engineSound.AccelerateInput();
			if (FMODUWE.IsInvalidParameterId(fmodIndexSpeed))
			{
				fmodIndexSpeed = ambienceSound.GetParameterIndex("speed");
			}
			if ((bool)ambienceSound && ambienceSound.GetIsPlaying())
			{
				ambienceSound.SetParameterValue(fmodIndexSpeed, useRigidbody.velocity.magnitude);
			}
		}
		bool flag = false;
		int i = 0;
		for (int num = quickSlotCharge.Length; i < num; i++)
		{
			if (quickSlotCharge[i] > 0f)
			{
				flag = true;
				break;
			}
		}
		if (pulseChargeSound.GetIsStartingOrPlaying() != flag)
		{
			if (flag)
			{
				pulseChargeSound.StartEvent();
			}
			else
			{
				pulseChargeSound.Stop();
			}
		}
	}

	public override bool CanPilot()
	{
		if (!FPSInputModule.current.lockMovement)
		{
			return IsPowered();
		}
		return false;
	}

	protected override void OnPilotModeBegin()
	{
		base.OnPilotModeBegin();
		UWE.Utils.EnterPhysicsSyncSection();
		Player.main.inSeamoth = true;
		bubbles.Play();
		ambienceSound.PlayUI();
		Player.main.armsController.SetWorldIKTarget(leftHandPlug, rightHandPlug);
		if ((bool)enterSeamoth)
		{
			enterSeamoth.Play();
		}
		int num = volumeticLights.Length;
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				volumeticLights[i].DisableVolume();
			}
		}
		onLightsToggled(toggleLights.GetLightsActive());
	}

	protected override void OnPilotModeEnd()
	{
		base.OnPilotModeEnd();
		UWE.Utils.ExitPhysicsSyncSection();
		Player.main.inSeamoth = false;
		bubbles.Stop();
		playerFullyEntered = false;
		ambienceSound.Stop();
		Player.main.armsController.SetWorldIKTarget(null, null);
		int num = volumeticLights.Length;
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				volumeticLights[i].RestoreVolume();
			}
		}
		onLightsToggled(active: false);
	}

	private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
	{
		if (pickupable.GetTechType() == TechType.VehicleStorageModule)
		{
			SeamothStorageContainer component = pickupable.GetComponent<SeamothStorageContainer>();
			if (component != null)
			{
				bool flag = component.container.count == 0;
				if (verbose && !flag)
				{
					ErrorMessage.AddDebug(Language.main.Get("SeamothStorageNotEmpty"));
				}
				return flag;
			}
			Debug.LogError("No SeamothStorageContainer found on SeamothStorageModule item");
		}
		return true;
	}

	public override void DeselectSlots()
	{
		if (playerFullyEntered)
		{
			base.DeselectSlots();
		}
	}

	protected override void OnUpgradeModuleChange(int slotID, TechType techType, bool added)
	{
		if (slotID >= 0 && slotID < storageInputs.Length)
		{
			storageInputs[slotID].SetEnabled(added && techType == TechType.VehicleStorageModule);
			GameObject gameObject = torpedoSilos[slotID];
			if (gameObject != null)
			{
				gameObject.SetActive(added && techType == TechType.SeamothTorpedoModule);
			}
		}
		int count = base.modules.GetCount(techType);
		switch (techType)
		{
		case TechType.SeamothReinforcementModule:
		case TechType.VehicleHullModule1:
		case TechType.VehicleHullModule2:
		case TechType.VehicleHullModule3:
		{
			Dictionary<TechType, float> dictionary = new Dictionary<TechType, float>
			{
				{
					TechType.SeamothReinforcementModule,
					800f
				},
				{
					TechType.VehicleHullModule1,
					100f
				},
				{
					TechType.VehicleHullModule2,
					300f
				},
				{
					TechType.VehicleHullModule3,
					700f
				}
			};
			float num = 0f;
			for (int i = 0; i < slotIDs.Length; i++)
			{
				string slot = slotIDs[i];
				TechType techTypeInSlot = base.modules.GetTechTypeInSlot(slot);
				if (dictionary.ContainsKey(techTypeInSlot))
				{
					float num2 = dictionary[techTypeInSlot];
					if (num2 > num)
					{
						num = num2;
					}
				}
			}
			crushDamage.SetExtraCrushDepth(num);
			break;
		}
		case TechType.SeamothSolarCharge:
			CancelInvoke("UpdateSolarRecharge");
			if (count > 0)
			{
				InvokeRepeating("UpdateSolarRecharge", 1f, 1f);
			}
			break;
		default:
			base.OnUpgradeModuleChange(slotID, techType, added);
			break;
		}
	}

	private void PlayTorpedoSound(int slotID, bool selected)
	{
		ItemsContainer storageInSlot = GetStorageInSlot(slotID, TechType.SeamothTorpedoModule);
		TechType techType = TechType.WhirlpoolTorpedo;
		if (selected && storageInSlot.GetCount(techType) > 0)
		{
			Utils.PlayFMODAsset(torpedoArmed, base.transform, 1f);
		}
		else
		{
			Utils.PlayFMODAsset(torpedoDisarmed, base.transform, 1f);
		}
	}

	protected override void OnUpgradeModuleUse(TechType techType, int slotID)
	{
		bool flag = true;
		float num = 0f;
		switch (techType)
		{
		case TechType.SeamothSonarModule:
			sonarSound.Stop();
			sonarSound.Play();
			SNCameraRoot.main.SonarPing();
			num = 5f;
			break;
		case TechType.SeamothElectricalDefense:
		{
			float charge = quickSlotCharge[slotID];
			float slotCharge = GetSlotCharge(slotID);
			ElectricalDefense component = Utils.SpawnZeroedAt(seamothElectricalDefensePrefab, base.transform).GetComponent<ElectricalDefense>();
			component.charge = charge;
			component.chargeScalar = slotCharge;
			num = 5f;
			break;
		}
		case TechType.SeamothTorpedoModule:
		{
			Transform muzzle = ((slotID == GetSlotIndex("SeamothModule1") || slotID == GetSlotIndex("SeamothModule3")) ? torpedoTubeLeft : torpedoTubeRight);
			ItemsContainer storageInSlot = GetStorageInSlot(slotID, TechType.SeamothTorpedoModule);
			TorpedoType torpedoType = null;
			for (int i = 0; i < torpedoTypes.Length; i++)
			{
				if (storageInSlot.Contains(torpedoTypes[i].techType))
				{
					torpedoType = torpedoTypes[i];
					break;
				}
			}
			flag = Vehicle.TorpedoShot(storageInSlot, torpedoType, muzzle);
			if (flag)
			{
				if (storageInSlot.count == 0)
				{
					Utils.PlayFMODAsset(torpedoDisarmed, base.transform, 1f);
				}
				num = 5f;
			}
			else
			{
				ErrorMessage.AddError(Language.main.Get("VehicleTorpedoNoAmmo"));
			}
			break;
		}
		}
		if (flag)
		{
			quickSlotTimeUsed[slotID] = Time.time;
			quickSlotCooldown[slotID] = num;
		}
	}

	protected override void OnUpgradeModuleToggle(int slotID, bool active)
	{
		switch (base.modules.GetTechTypeInSlot(slotIDs[slotID]))
		{
		case TechType.LootSensorMetal:
			if (active)
			{
				InvokeRepeating("UpdateLootSensorMetal", 1f, 1f);
			}
			else
			{
				CancelInvoke("UpdateLootSensorMetal");
			}
			break;
		case TechType.LootSensorLithium:
			if (active)
			{
				InvokeRepeating("UpdateLootSensorLithium", 1f, 1f);
			}
			else
			{
				CancelInvoke("UpdateLootSensorLithium");
			}
			break;
		case TechType.LootSensorFragment:
			if (active)
			{
				InvokeRepeating("UpdateLootSensorFragment", 1f, 1f);
			}
			else
			{
				CancelInvoke("UpdateLootSensorFragment");
			}
			break;
		}
	}

	private void UpdateSolarRecharge()
	{
		DayNightCycle main = DayNightCycle.main;
		if (!(main == null))
		{
			int count = base.modules.GetCount(TechType.SeamothSolarCharge);
			float num = Mathf.Clamp01((200f + base.transform.position.y) / 200f);
			float localLightScalar = main.GetLocalLightScalar();
			float amount = 1f * localLightScalar * num * (float)count;
			AddEnergy(amount);
		}
	}

	private void OnPlayerModeChanged(Player.Mode mode)
	{
		timeLastPlayerModeChange = Time.time;
	}

	private void CheckLootSensor(TechType scanType)
	{
		bool num = LootSensor.IsLootDetected(scanType, base.transform.position, 300);
		if (num)
		{
			ErrorMessage.AddMessage("LootSensor detected " + scanType.AsString());
		}
		if (num)
		{
			FMODUWE.PlayOneShot("event:/interface/on_long", base.gameObject.transform.position, 0.5f);
		}
	}

	private void CheckLootSensor(TechType[] scanTypes)
	{
		TechType detectedType;
		bool num = LootSensor.IsLootDetected(scanTypes, base.transform.position, 300, out detectedType);
		if (num)
		{
			ErrorMessage.AddMessage("LootSensor detected " + detectedType.AsString());
		}
		if (num)
		{
			FMODUWE.PlayOneShot("event:/interface/on_long", base.gameObject.transform.position, 0.5f);
		}
	}

	private void UpdateLootSensorMetal()
	{
		CheckLootSensor(TechType.ScrapMetal);
	}

	private void UpdateLootSensorLithium()
	{
		CheckLootSensor(TechType.Lithium);
	}

	private void UpdateLootSensorFragment()
	{
		CheckLootSensor(fragmentTechTypes);
	}

	public void OnHoverTorpedoStorage(HandTargetEventData eventData)
	{
		if (base.modules.GetCount(TechType.SeamothTorpedoModule) > 0)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, "SeamothTorpedoStorage", translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
		}
	}

	public void OnOpenTorpedoStorage(HandTargetEventData eventData)
	{
		OpenTorpedoStorage(eventData.transform);
	}

	public void OpenTorpedoStorage(Transform useTransform)
	{
		if (base.modules.GetCount(TechType.SeamothTorpedoModule) > 0)
		{
			Inventory.main.ClearUsedStorage();
			int num = slotIDs.Length;
			for (int i = 0; i < num; i++)
			{
				ItemsContainer storageInSlot = GetStorageInSlot(i, TechType.SeamothTorpedoModule);
				Inventory.main.SetUsedStorage(storageInSlot, append: true);
			}
			Player.main.GetPDA().Open(PDATab.Inventory, useTransform);
		}
	}
}
