using System;
using System.Collections.Generic;
using ProtoBuf;
using UWE;
using UnityEngine;
using mset;

[ProtoContract]
[ProtoInclude(100, typeof(BaseRoot))]
public class SubRoot : MonoBehaviour, IProtoEventListener, IOnTakeDamage, IProtoTreeEventListener
{
	public struct DamageEvent
	{
		public SubRoot subRoot;

		public CrushDamageable victim;

		public float amount;

		public Vector3 position;
	}

	public float minSpeedToDamage = 5f;

	public float damagePerSpeed = 5f;

	public Transform riftCamOrientation;

	public Transform modulesRoot;

	public AnimationCurve thermalReactorCharge;

	[AssertNotNull]
	public FMODAsset hitSoundHard;

	[AssertNotNull]
	public FMODAsset hitSoundMedium;

	[AssertNotNull]
	public FMODAsset hitSoundSoft;

	public FMOD_CustomEmitter slotModSFX;

	public Rigidbody rb;

	public Transform centerOfMass;

	public GameObject insideSoundsRoot;

	public GameObject outsideSoundsRoot;

	public bool useTwoCams = true;

	public LightingController lightControl;

	public Sky interiorSky;

	public Sky glassSky;

	public Transform subAxis;

	public VoiceNotificationManager voiceNotificationManager;

	public VoiceNotification welcomeNotification;

	public VoiceNotification welcomeNotificationIssue;

	public VoiceNotification welcomeNotificationEmergency;

	public VoiceNotification noPowerNotification;

	public VoiceNotification hullBreachNotification;

	public VoiceNotification hullRestoredNotification;

	public VoiceNotification fireExtinguishedNotification;

	public VoiceNotification engineOverheatNotification;

	public VoiceNotification engineOverheatCriticalNotification;

	public VoiceNotification creatureAttackNotification;

	public VoiceNotification fireNotification;

	public VoiceNotification hullDamageNotification;

	public VoiceNotification cavitatingNotification;

	public VoiceNotification silentRunningNotification;

	public VoiceNotification aheadSlowNotification;

	public VoiceNotification aheadStandardNotification;

	public VoiceNotification aheadFlankNotification;

	public VoiceNotification enginePowerUpNotification;

	public VoiceNotification enginePowerDownNotification;

	public VoiceNotification hullLowNotification;

	public VoiceNotification hullCriticalNotification;

	public VoiceNotification abandonShipNotification;

	public VoiceNotification decoyNotification;

	public VoiceNotification fireSupressionNotification;

	public VFXVolumetricLight[] dimFloodlightsOnEnter;

	public WorldForces worldForces;

	public Event<DamageEvent> damagedEvent = new Event<DamageEvent>();

	public bool debugDamage;

	public GameObject subDamageSoundsPrefab;

	private SubDamageSounds subDamageSounds;

	public GameObject entranceHatch;

	public GameObject seamothLaunchBay;

	public bool shieldUpgrade;

	public bool sonarUpgrade;

	public bool vehicleRepairUpgrade;

	public bool decoyTubeSizeIncreaseUpgrade;

	public bool silentRunning;

	public bool subWarning;

	public bool fireSuppressionState;

	public bool subDestroyed;

	public bool thermalReactorUpgrade;

	public float shieldCD = 200f;

	public float sonarCD = 10f;

	public float decoyCD = 1f;

	public float fireSuppressionCD = 200f;

	public float silentRunningCD = 120f;

	public float shieldPowerCost = 50f;

	public float sonarPowerCost = 10f;

	public float silentRunningPowerCost = 5f;

	public CyclopsExternalDamageManager damageManager;

	public CyclopsNoiseManager noiseManager;

	public MeshRenderer shieldFX;

	public UpgradeConsole upgradeConsole;

	public PowerRelay powerRelay;

	public FMOD_CustomEmitter enterHatchSFX;

	public bool isBase;

	public bool isCyclops;

	[SerializeField]
	private List<TechType> targetingTechTypeExcludeList = new List<TechType>();

	private float currPowerRating = 1f;

	private bool subLightsOn = true;

	private int lightingState;

	private float shieldIntensity;

	private float shieldImpactIntensity;

	private float shieldGoToIntensity;

	private float oldHPPercent = 1f;

	private OxygenManager oxygenMgr;

	private LiveMixin live;

	private const int currentVersion = 2;

	[NonSerialized]
	[ProtoMember(1)]
	public float floodFraction;

	[NonSerialized]
	[ProtoMember(2)]
	public string subName = "";

	[NonSerialized]
	[ProtoMember(3)]
	public Vector3 subColor = new Vector3(0.575f, 0.44f, 0.39f);

	[NonSerialized]
	[ProtoMember(4)]
	public int version = 2;

	[NonSerialized]
	[ProtoMember(5, OverwriteList = true)]
	public Vector3[] subColors;

	private float baseDrainSecs = 60f;

	private bool prevFrameFlooding;

	private int prevFrameBreaches;

	private bool subModulesDirty = true;

	private bool isFlooded;

	private static readonly string[] slotNames = new string[6] { "Module1", "Module2", "Module3", "Module4", "Module5", "Module6" };

	[AssertLocalization(1)]
	private const string powerRatingNowFormatKey = "PowerRatingNowFormat";

	private Rigidbody rigidbody;

	[AssertNotNull]
	public BehaviourLOD LOD;

	private DealDamageOnImpact dmg;

	private static readonly Dictionary<TechType, float> hullReinforcement = new Dictionary<TechType, float>
	{
		{
			TechType.HullReinforcementModule,
			800f
		},
		{
			TechType.HullReinforcementModule2,
			1600f
		},
		{
			TechType.HullReinforcementModule3,
			2800f
		},
		{
			TechType.CyclopsHullModule1,
			400f
		},
		{
			TechType.CyclopsHullModule2,
			800f
		},
		{
			TechType.CyclopsHullModule3,
			1200f
		}
	};

	public bool playerInside { get; private set; }

	public float GetLeakAmount()
	{
		return floodFraction;
	}

	public virtual void Awake()
	{
		Debug.Log("Cyclops::Awake frame " + Time.frameCount);
		oxygenMgr = GetComponent<OxygenManager>();
		subDamageSounds = UnityEngine.Object.Instantiate(subDamageSoundsPrefab, Vector3.zero, Quaternion.identity).GetComponent<SubDamageSounds>();
		subDamageSounds.gameObject.transform.parent = base.gameObject.transform;
	}

	public virtual void Start()
	{
		DevConsole.RegisterConsoleCommand(this, "crush");
		DevConsole.RegisterConsoleCommand(this, "flood");
		DevConsole.RegisterConsoleCommand(this, "damagesub");
		Utils.StopAllFMODEvents(insideSoundsRoot);
		Utils.StartAllFMODEvents(outsideSoundsRoot);
		live = GetComponent<LiveMixin>();
		if (live != null)
		{
			oldHPPercent = live.GetHealthFraction();
		}
		if (upgradeConsole != null)
		{
			upgradeConsole.modules.onEquip += OnSubModulesChanged;
			upgradeConsole.modules.onUnequip += OnSubModulesChanged;
		}
		powerRelay = GetComponentInChildren<PowerRelay>();
		rigidbody = GetComponent<Rigidbody>();
	}

	private void OnDestroy()
	{
		Targeting.techTypeIgnoreList.RemoveRange(targetingTechTypeExcludeList);
	}

	private void OnConsoleCommand_flood()
	{
		floodFraction = 0.5f;
	}

	private void OnConsoleCommand_crush()
	{
		CrushDamageRandomPart(1f);
	}

	private void OnConsoleCommand_damagesub()
	{
		live.TakeDamage(400f);
	}

	public OxygenManager GetOxygenManager()
	{
		return oxygenMgr;
	}

	public void OnKill()
	{
		Stabilizer component = GetComponent<Stabilizer>();
		if ((bool)component)
		{
			component.enabled = false;
		}
	}

	public Transform GetModulesRoot()
	{
		return modulesRoot;
	}

	public Vector3 GetWorldCenterOfMass()
	{
		if (rigidbody != null)
		{
			if (centerOfMass != null)
			{
				return centerOfMass.transform.position;
			}
			return base.transform.TransformPoint(rigidbody.centerOfMass);
		}
		return base.transform.position;
	}

	private void OnCollisionEnter(Collision col)
	{
		if (!col.gameObject.CompareTag("Creature") && !col.gameObject.CompareTag("Player"))
		{
			float magnitude = col.relativeVelocity.magnitude;
			float soundRadiusObsolete = Mathf.Clamp01(magnitude / 8f);
			if (magnitude > 8f)
			{
				Utils.PlayFMODAsset(hitSoundHard, col.contacts[0].point, soundRadiusObsolete);
			}
			else if (magnitude > 4f)
			{
				Utils.PlayFMODAsset(hitSoundMedium, col.contacts[0].point, soundRadiusObsolete);
			}
			else
			{
				Utils.PlayFMODAsset(hitSoundSoft, col.contacts[0].point, soundRadiusObsolete);
			}
		}
	}

	private void CrushDamageRandomPart(float amount)
	{
		CrushDamageable[] componentsInChildren = GetComponentsInChildren<CrushDamageable>();
		if (componentsInChildren.Length != 0)
		{
			CrushDamageable crushDamageable = componentsInChildren[UnityEngine.Random.Range(0, componentsInChildren.Length)];
			crushDamageable.OnDamaged(amount);
			if (debugDamage)
			{
				Debug.Log("damaging " + crushDamageable.gameObject.name);
			}
			DamageEvent damageEvent = default(DamageEvent);
			damageEvent.subRoot = this;
			damageEvent.amount = amount;
			damageEvent.victim = crushDamageable;
			damagedEvent.Trigger(damageEvent);
			subDamageSounds.Play(damageEvent);
		}
	}

	private void UpdateDamageSettings()
	{
		if (dmg == null)
		{
			dmg = GetComponent<DealDamageOnImpact>();
		}
		if (!(dmg == null))
		{
			if (rigidbody != null && rigidbody.velocity.magnitude < 1f)
			{
				dmg.speedMinimumForDamage = float.PositiveInfinity;
			}
			else
			{
				dmg.speedMinimumForDamage = 2f;
			}
			_ = live != null;
		}
	}

	public void ForceLightingState(bool lightingOn)
	{
		subLightsOn = lightingOn;
	}

	private void UpdateLighting()
	{
		if (!(lightControl != null))
		{
			return;
		}
		lightingState = 0;
		if (IsLeaking() || silentRunning || subWarning || fireSuppressionState)
		{
			lightingState = 1;
		}
		if (powerRelay != null)
		{
			if (powerRelay.GetPowerStatus() == PowerSystem.Status.Offline || !subLightsOn)
			{
				lightingState = 2;
			}
			if ((bool)live && !live.IsAlive())
			{
				lightingState = 2;
			}
		}
		if (lightingState != (int)lightControl.state)
		{
			lightControl.LerpToState(lightingState);
		}
	}

	private float GetTemperature()
	{
		WaterTemperatureSimulation main = WaterTemperatureSimulation.main;
		if (!(main != null))
		{
			return 0f;
		}
		return main.GetTemperature(base.transform.position);
	}

	private void UpdateThermalReactorCharge()
	{
		if (thermalReactorUpgrade)
		{
			float temperature = GetTemperature();
			float amount = thermalReactorCharge.Evaluate(temperature) * 1.5f * Time.deltaTime;
			powerRelay.AddEnergy(amount, out var _);
		}
	}

	private void Update()
	{
		if (LOD.IsMinimal())
		{
			return;
		}
		UpdateDamageSettings();
		UpdateLighting();
		UpdateThermalReactorCharge();
		if (shieldFX != null && shieldFX.gameObject.activeSelf)
		{
			shieldImpactIntensity = Mathf.MoveTowards(shieldImpactIntensity, 0f, Time.deltaTime / 4f);
			shieldIntensity = Mathf.MoveTowards(shieldIntensity, shieldGoToIntensity, Time.deltaTime / 2f);
			shieldFX.material.SetFloat(ShaderPropertyID._Intensity, shieldIntensity);
			shieldFX.material.SetFloat(ShaderPropertyID._ImpactIntensity, shieldImpactIntensity);
			if (Mathf.Approximately(shieldIntensity, 0f) && shieldGoToIntensity == 0f)
			{
				shieldFX.gameObject.SetActive(value: false);
			}
		}
		UpdateSubModules();
	}

	private void FixedUpdate()
	{
		if (rigidbody != null && !rigidbody.isKinematic && floodFraction - 0.3f > 0f)
		{
			float num = floodFraction * 2f;
			float num2 = floodFraction * 2f;
			if (rigidbody.velocity.y > 0f - num2)
			{
				rigidbody.AddForce(new Vector3(0f, 0f - num, 0f), ForceMode.VelocityChange);
			}
		}
	}

	public virtual bool IsLeaking()
	{
		return floodFraction > 0f;
	}

	private void SetCyclopsUpgrades()
	{
		if (!(upgradeConsole != null) || !live.IsAlive())
		{
			return;
		}
		shieldUpgrade = false;
		sonarUpgrade = false;
		vehicleRepairUpgrade = false;
		decoyTubeSizeIncreaseUpgrade = false;
		thermalReactorUpgrade = false;
		TechType[] array = new TechType[6];
		Equipment modules = upgradeConsole.modules;
		for (int i = 0; i < 6; i++)
		{
			string slot = slotNames[i];
			TechType techTypeInSlot = modules.GetTechTypeInSlot(slot);
			switch (techTypeInSlot)
			{
			case TechType.CyclopsShieldModule:
				shieldUpgrade = true;
				break;
			case TechType.CyclopsSonarModule:
				sonarUpgrade = true;
				break;
			case TechType.CyclopsSeamothRepairModule:
				vehicleRepairUpgrade = true;
				break;
			case TechType.CyclopsDecoyModule:
				decoyTubeSizeIncreaseUpgrade = true;
				break;
			case TechType.CyclopsThermalReactorModule:
				thermalReactorUpgrade = true;
				break;
			}
			array[i] = techTypeInSlot;
		}
		if (slotModSFX != null)
		{
			slotModSFX.Play();
		}
		BroadcastMessage("RefreshUpgradeConsoleIcons", array, SendMessageOptions.RequireReceiver);
	}

	private void SetExtraDepth()
	{
		if (!(upgradeConsole != null))
		{
			return;
		}
		Equipment modules = upgradeConsole.modules;
		float num = 0f;
		for (int i = 1; i < 7; i++)
		{
			string slot = $"Module{i}";
			TechType techTypeInSlot = modules.GetTechTypeInSlot(slot);
			if (hullReinforcement.ContainsKey(techTypeInSlot))
			{
				float num2 = hullReinforcement[techTypeInSlot];
				if (num2 > num)
				{
					num = num2;
				}
			}
		}
		base.gameObject.GetComponent<CrushDamage>().SetExtraCrushDepth(num);
	}

	private void UpdatePowerRating()
	{
		float num = currPowerRating;
		currPowerRating = 1f;
		if (upgradeConsole != null)
		{
			Equipment modules = upgradeConsole.modules;
			currPowerRating = ((modules.GetCount(TechType.PowerUpgradeModule) > 0) ? 3f : currPowerRating);
		}
		if (currPowerRating != num)
		{
			ErrorMessage.AddMessage(Language.main.GetFormat("PowerRatingNowFormat", currPowerRating));
		}
	}

	public float GetPowerRating()
	{
		return currPowerRating;
	}

	private void OnSubModulesChanged(string slot, InventoryItem item)
	{
		subModulesDirty = true;
	}

	private void UpdateSubModules()
	{
		if (subModulesDirty)
		{
			subModulesDirty = false;
			SetCyclopsUpgrades();
			SetExtraDepth();
			UpdatePowerRating();
			BroadcastMessage("UpdateAbilities", null, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void OnPlayerEntered(Player player)
	{
		playerInside = true;
		Utils.StartAllFMODEvents(insideSoundsRoot);
		Utils.StopAllFMODEvents(outsideSoundsRoot, allowFadeOut: true);
		DealDamageOnImpact component = GetComponent<DealDamageOnImpact>();
		if ((bool)component)
		{
			component.AddException(player.gameObject);
		}
		if ((bool)voiceNotificationManager && !WaitScreen.IsWaiting)
		{
			if ((bool)powerRelay && powerRelay.GetPowerStatus() == PowerSystem.Status.Offline)
			{
				voiceNotificationManager.PlayVoiceNotification(noPowerNotification);
			}
			else if (IsLeaking() || subWarning || ((bool)live && live.GetHealthFraction() < 0.25f))
			{
				voiceNotificationManager.PlayVoiceNotification(welcomeNotificationEmergency);
			}
			else if ((bool)live && live.GetHealthFraction() < 0.8f)
			{
				voiceNotificationManager.PlayVoiceNotification(welcomeNotificationIssue);
			}
			else
			{
				voiceNotificationManager.PlayVoiceNotification(welcomeNotification);
			}
		}
		if (!isBase)
		{
			GoalManager main = GoalManager.main;
			if ((bool)main)
			{
				main.OnCustomGoalEvent("Board_Cyclops");
			}
			if ((bool)enterHatchSFX)
			{
				enterHatchSFX.Play();
			}
			if ((bool)live)
			{
				live.invincible = false;
			}
			VFXVolumetricLight[] array = dimFloodlightsOnEnter;
			foreach (VFXVolumetricLight vFXVolumetricLight in array)
			{
				if ((bool)vFXVolumetricLight)
				{
					vFXVolumetricLight.SendMessage("DisableVolume", null, SendMessageOptions.DontRequireReceiver);
				}
			}
			BroadcastMessage("PlayerEnteredSub", null, SendMessageOptions.DontRequireReceiver);
			BroadcastMessage("RestoreEngineState", false, SendMessageOptions.RequireReceiver);
		}
		Targeting.techTypeIgnoreList.AddRange(targetingTechTypeExcludeList);
	}

	public void OnPlayerExited(Player player)
	{
		playerInside = false;
		if (!isBase)
		{
			live.invincible = true;
			if (player.GetMode() == Player.Mode.Piloting)
			{
				player.ExitPilotingMode();
			}
		}
		Utils.StopAllFMODEvents(insideSoundsRoot, allowFadeOut: true);
		Utils.StartAllFMODEvents(outsideSoundsRoot);
		DealDamageOnImpact component = GetComponent<DealDamageOnImpact>();
		if (component != null)
		{
			component.RemoveException(player.gameObject);
		}
		VFXVolumetricLight[] array = dimFloodlightsOnEnter;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SendMessage("RestoreVolume", null, SendMessageOptions.DontRequireReceiver);
		}
		BroadcastMessage("SaveEngineStateAndPowerDown", null, SendMessageOptions.DontRequireReceiver);
		Targeting.techTypeIgnoreList.RemoveRange(targetingTechTypeExcludeList);
	}

	public string GetSubName()
	{
		SubName[] componentsInChildren = GetComponentsInChildren<SubName>(includeInactive: true);
		if (componentsInChildren.Length != 0)
		{
			return componentsInChildren[0].GetName();
		}
		return "";
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		SubName[] componentsInChildren = GetComponentsInChildren<SubName>(includeInactive: true);
		if (componentsInChildren.Length != 0)
		{
			SubName subName = componentsInChildren[0];
			this.subName = subName.GetName();
			subColors = subName.GetColors();
		}
		subColor = Vector3.zero;
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		if (subColor != Vector3.zero)
		{
			subColors = new Vector3[4]
			{
				new Vector3(1f, 0f, 1f),
				subColor,
				new Vector3(0.1083f, 0.83f, 0.96f),
				new Vector3(0f, 0f, 0f)
			};
			version = 2;
		}
		else if (version == 1)
		{
			Array.Resize(ref subColors, 4);
			subColors[3] = new Vector3(0f, 0f, 0f);
			version = 2;
		}
		SubName[] componentsInChildren = GetComponentsInChildren<SubName>(includeInactive: true);
		if (componentsInChildren.Length != 0)
		{
			SubName obj = componentsInChildren[0];
			obj.DeserializeName(subName);
			obj.DeserializeColors(subColors);
		}
	}

	void IProtoTreeEventListener.OnProtoSerializeObjectTree(ProtobufSerializer serializer)
	{
	}

	void IProtoTreeEventListener.OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
	{
		if (!isCyclops)
		{
			return;
		}
		using (ListPool<Constructable> listPool = Pool<ListPool<Constructable>>.Get())
		{
			List<Constructable> list = listPool.list;
			GetComponentsInChildren(includeInactive: true, list);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].ExcludeFromSubParentRigidbody();
			}
		}
	}

	public void StartSubShielded()
	{
		live.shielded = true;
		shieldFX.gameObject.SetActive(value: true);
		shieldGoToIntensity = 1f;
		LavaLarva[] componentsInChildren = base.gameObject.GetComponentsInChildren<LavaLarva>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].GetComponent<LiveMixin>().TakeDamage(1f, default(Vector3), DamageType.Electrical);
		}
	}

	public void EndSubShielded()
	{
		live.shielded = false;
		shieldGoToIntensity = 0f;
	}

	public virtual void OnTakeDamage(DamageInfo damageInfo)
	{
		if (live.GetHealthFraction() < 0.5f && oldHPPercent >= 0.5f)
		{
			voiceNotificationManager.PlayVoiceNotification(hullLowNotification);
		}
		else if (live.GetHealthFraction() < 0.25f && oldHPPercent >= 0.25f)
		{
			voiceNotificationManager.PlayVoiceNotification(hullCriticalNotification);
		}
		else if (live.health <= 0f)
		{
			voiceNotificationManager.ClearQueue();
			voiceNotificationManager.PlayVoiceNotification(abandonShipNotification, addToQueue: false, forcePlay: true);
			Invoke("PowerDownCyclops", 13f);
			Invoke("DestroyCyclopsSubRoot", 18f);
			if (Vector3.Distance(base.transform.position, Player.main.transform.position) < 20f)
			{
				MainCameraControl.main.ShakeCamera(1.5f, 20f);
			}
		}
		if (damageManager != null)
		{
			damageManager.NotifyAllOfDamage();
		}
		oldHPPercent = live.GetHealthFraction();
		if (shieldFX != null && shieldFX.gameObject.activeSelf)
		{
			shieldImpactIntensity = 1f;
			shieldFX.material.SetVector(ShaderPropertyID._ImpactPosition, damageInfo.position);
			if (damageInfo.dealer != null && (bool)damageInfo.dealer.GetComponent<LiveMixin>())
			{
				damageInfo.dealer.GetComponent<LiveMixin>().TakeDamage(20f, default(Vector3), DamageType.Electrical);
			}
		}
	}

	private void PowerDownCyclops()
	{
		if (!isBase)
		{
			float amountConsumed = 0f;
			powerRelay.ConsumeEnergy(99999f, out amountConsumed);
		}
	}

	private void DestroyCyclopsSubRoot()
	{
		if (!isBase)
		{
			silentRunning = false;
			SendMessage("DestroyCyclops");
		}
	}

	public virtual bool IsUnderwater(Vector3 wsPos)
	{
		_ = (double)floodFraction;
		_ = 0.0001;
		return false;
	}

	public void ForceInterpolationLock(RigidbodyInterpolation interpolation, bool locked)
	{
		if (rb != null)
		{
			rb.interpolation = interpolation;
			worldForces.lockInterpolation = locked;
		}
	}
}
