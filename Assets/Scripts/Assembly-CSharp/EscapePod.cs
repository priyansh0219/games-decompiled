using System;
using System.Collections;
using ProtoBuf;
using Story;
using UWE;
using UnityEngine;

[ProtoContract]
public class EscapePod : MonoBehaviour, IProtoTreeEventListener, IProtoEventListenerAsync
{
	private const string saveLoadId = "EscapePod";

	public static EscapePod main;

	private const float storageTimeOpen = 0.25f;

	private const float storageTimeClose = 0.25f;

	private const string storageText = "EscapePodStorageOpen";

	private const string storageAnimProperty = "storage_open";

	private const string medKitTextOpen = "EscapePodMedKitOpen";

	private const string medKitTextClose = "EscapePodMedKitClose";

	private const string medKitAnimProperty = "firstaid_open";

	[AssertNotNull]
	public GameObject bottomHatchEntrance;

	[AssertNotNull]
	public Transform playerSpawn;

	[AssertNotNull]
	public PlayerCinematicController introCinematic;

	[AssertNotNull]
	public EscapePodCinematicControl escapePodCinematicControl;

	[AssertNotNull]
	public LiveMixin liveMixin;

	[AssertNotNull]
	public LightingController lightingController;

	[AssertNotNull]
	public PrefabSpawnBase vfxSpawner;

	[AssertNotNull]
	public PrefabSpawnBase radioSpawner;

	[AssertNotNull]
	public PrefabSpawnBase birdsSpawner;

	[AssertNotNull]
	public FMOD_CustomEmitter damagedSound;

	[AssertNotNull]
	public FMOD_CustomEmitter fixPanelPowerUp;

	[AssertNotNull]
	public StoryGoal fixPanelGoal;

	[AssertNotNull]
	public Animator animator;

	[AssertNotNull]
	public Rigidbody rigidbodyComponent;

	[AssertNotNull]
	public PingInstance pingInstance;

	[AssertNotNull]
	public StorageContainer storageContainer;

	[AssertNotNull]
	public Transform storagePivot;

	[AssertNotNull]
	public FMODAsset storageOpenSound;

	[AssertNotNull]
	public FMODAsset storageCloseSound;

	public Transform medKitPivot;

	public GameObject medKitColliderOpen;

	public GameObject medKitColliderClose;

	public FMODAsset medKitOpenSound;

	public FMODAsset medKitCloseSound;

	private bool _initialized;

	private float healthScalar;

	private bool isNewBorn = true;

	private bool damageEffectsShowing;

	private Sequence storageSequence = new Sequence(initialState: false);

	private bool medKitOpen;

	public PrefabSpawnBase[] moduleSpawners;

	private const int currentVersion = 4;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 4;

	[NonSerialized]
	[ProtoMember(2, OverwriteList = true)]
	public byte[] serializedStorage;

	[NonSerialized]
	[ProtoMember(3)]
	public bool startedIntroCinematic;

	[NonSerialized]
	[ProtoMember(4)]
	public Vector3 anchorPosition;

	[NonSerialized]
	[ProtoMember(5)]
	public bool bottomHatchUsed;

	[NonSerialized]
	[ProtoMember(6)]
	public bool topHatchUsed;

	public bool initialized
	{
		get
		{
			return _initialized;
		}
		private set
		{
			_initialized = value;
		}
	}

	private void Awake()
	{
		DevConsole.RegisterConsoleCommand(this, "randomstart");
		if (!Utils.GetContinueMode())
		{
			Player player = Player.main;
			player.SetPosition(playerSpawn.position, playerSpawn.rotation);
			player.escapePod.Update(newValue: true);
			player.currentEscapePod = this;
		}
		main = this;
	}

	private void Start()
	{
		bool flag = true;
		if (Application.isEditor && Application.loadedLevelName == "BareScene")
		{
			flag = false;
		}
		if (!Utils.GetContinueMode() && isNewBorn)
		{
			if (flag)
			{
				ChooseRandomStart();
			}
			if (!MainGameController.ShouldPlayIntro())
			{
				birdsSpawner.SpawnManual();
			}
			initialized = true;
			base.gameObject.BroadcastMessage("OnNewBorn", SendMessageOptions.DontRequireReceiver);
		}
		if (startedIntroCinematic)
		{
			ShowDamagedEffects();
		}
		healthScalar = liveMixin.GetHealthFraction();
		Physics.SyncTransforms();
		Invoke("ForceSkyApplier", 0.5f);
	}

	private void ForceSkyApplier()
	{
		SkyEnvironmentChanged.Broadcast(base.gameObject, base.gameObject);
	}

	private void Update()
	{
		storageSequence.Update();
		UpdateDamagedEffects();
	}

	private void FixedUpdate()
	{
		Vector3 vector = anchorPosition - base.transform.position;
		if (vector.sqrMagnitude > 1f)
		{
			Vector3 force = Vector3.ClampMagnitude(vector, 1f);
			rigidbodyComponent.AddForce(force, ForceMode.Acceleration);
		}
	}

	public void ShowDamagedEffects()
	{
		if (!damageEffectsShowing)
		{
			vfxSpawner.SpawnManual();
			damagedSound.Play();
			damageEffectsShowing = true;
		}
	}

	public void UpdateDamagedEffects()
	{
		healthScalar = Mathf.MoveTowards(healthScalar, liveMixin.GetHealthFraction(), Time.deltaTime / 10f);
		animator.SetFloat("lifepod_damage", healthScalar);
		if (!damageEffectsShowing || !(liveMixin.GetHealthFraction() > 0.99f))
		{
			return;
		}
		if (vfxSpawner.spawnedObj != null)
		{
			ParticleSystem component = vfxSpawner.spawnedObj.GetComponent<ParticleSystem>();
			if (component != null)
			{
				component.Stop();
			}
		}
		damagedSound.Stop();
		damageEffectsShowing = false;
		lightingController.LerpToState(0, 5f);
		uGUI_EscapePod.main.SetHeader(Language.main.Get("IntroEscapePod4Header"), new Color32(159, 243, 63, byte.MaxValue));
		uGUI_EscapePod.main.SetContent(Language.main.Get("IntroEscapePod4Content"), new Color32(159, 243, 63, byte.MaxValue));
		uGUI_EscapePod.main.SetPower(Language.main.Get("IntroEscapePod4Power"), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
	}

	public void DamagePlayer()
	{
		LiveMixin component = Player.main.GetComponent<LiveMixin>();
		if ((bool)component && component.IsFullHealth())
		{
			component.TakeDamage(20f);
		}
	}

	public void DamageRadio()
	{
		if (radioSpawner != null && radioSpawner.spawnedObj != null)
		{
			LiveMixin component = radioSpawner.spawnedObj.GetComponent<LiveMixin>();
			if ((bool)component && component.IsFullHealth())
			{
				component.TakeDamage(80f);
			}
		}
	}

	public bool IsPlayingIntroCinematic()
	{
		return introCinematic.cinematicModeActive;
	}

	public void OnRepair()
	{
		fixPanelGoal.Trigger();
		fixPanelPowerUp.Play();
	}

	public void TriggerIntroCinematic()
	{
		if (!startedIntroCinematic)
		{
			introCinematic.StartCinematicMode(Player.main);
			startedIntroCinematic = true;
		}
	}

	public void StopIntroCinematic(bool isInterrupted)
	{
		introCinematic.OnPlayerCinematicModeEnd();
		escapePodCinematicControl.StopAll();
		ShowDamagedEffects();
		DamageRadio();
		birdsSpawner.SpawnManual();
		if (Player.main.liveMixin.IsFullHealth() && !GameModeUtils.IsInvisible())
		{
			Player.main.liveMixin.health -= 20f;
		}
		if (isInterrupted)
		{
			lightingController.SnapToState(2);
			uGUI_EscapePod.main.SetHeader(Language.main.Get("IntroEscapePod3Header"), new Color32(243, 201, 63, byte.MaxValue), 2f);
			uGUI_EscapePod.main.SetContent(Language.main.Get("IntroEscapePod3Content"), new Color32(233, 63, 27, byte.MaxValue));
			uGUI_EscapePod.main.SetPower(Language.main.Get("IntroEscapePod3Power"), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			introCinematic.interpolationTimeOut = 0f;
		}
		else
		{
			uGUI_EscapePod.main.SetHeader(Language.main.Get("IntroEscapePod2Header"), new Color32(243, 201, 63, byte.MaxValue), 2f);
			uGUI_EscapePod.main.SetContent(Language.main.Get("IntroEscapePod2Content"), new Color32(233, 63, 27, byte.MaxValue));
			uGUI_EscapePod.main.SetPower(Language.main.Get("IntroEscapePod2Power"), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
		}
	}

	public IEnumerator OnProtoDeserializeAsync(ProtobufSerializer serializer)
	{
		if (version < 2)
		{
			storageContainer.ResetContainer();
			yield return StorageHelper.RestoreItemsAsync(serializer, serializedStorage, storageContainer.container);
			version = 2;
		}
		if (version < 3)
		{
			anchorPosition = base.transform.position;
			for (int i = 0; i < moduleSpawners.Length; i++)
			{
				moduleSpawners[i].SpawnManual();
			}
			version = 3;
		}
		isNewBorn = false;
	}

	public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
	{
		if (version < 4)
		{
			CoroutineHost.StartCoroutine(CleanUpDuplicatedStorage());
		}
		initialized = true;
	}

	private IEnumerator CleanUpDuplicatedStorage()
	{
		yield return StorageHelper.DestroyDuplicatedItems(base.gameObject);
		version = Mathf.Max(version, 4);
	}

	public bool IsNewBorn()
	{
		return isNewBorn;
	}

	public void OnConsoleCommand_randomstart()
	{
		ChooseRandomStart();
	}

	private void ChooseRandomStart()
	{
		Vector3 randomStartPoint = RandomStart.main.GetRandomStartPoint();
		StartAtPosition(randomStartPoint);
	}

	public void StartAtPosition(Vector3 position)
	{
		base.transform.position = position;
		anchorPosition = position;
		RespawnPlayer();
	}

	public void RespawnPlayer()
	{
		Player.main.SetPosition(playerSpawn.transform.position, playerSpawn.transform.rotation);
		Player.main.escapePod.Update(newValue: true);
	}

	public void StorageHover()
	{
		HandReticle handReticle = HandReticle.main;
		handReticle.SetText(HandReticle.TextType.Hand, "EscapePodStorageOpen", translate: true, GameInput.Button.LeftHand);
		handReticle.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		handReticle.SetIcon(HandReticle.IconType.Hand);
	}

	public void StorageOpen()
	{
		if (!storageSequence.active)
		{
			SafeAnimator.SetBool(animator, "storage_open", value: true);
			if (storageOpenSound != null)
			{
				Utils.PlayFMODAsset(storageOpenSound, storagePivot, 1f);
			}
			storageSequence.Set(0.25f, target: true, StorageOnFlapOpened);
		}
	}

	private void StorageOnFlapOpened()
	{
		PDA pDA = Player.main.GetPDA();
		Inventory.main.SetUsedStorage(storageContainer.container);
		if (!pDA.Open(PDATab.Inventory, storagePivot, StorageOnClosePDA))
		{
			StorageOnClosePDA(pDA);
		}
	}

	private void StorageOnClosePDA(PDA pda)
	{
		storageSequence.Set(0.25f, target: false);
		if (storageCloseSound != null)
		{
			Utils.PlayFMODAsset(storageCloseSound, storagePivot, 1f);
		}
		SafeAnimator.SetBool(animator, "storage_open", value: false);
	}

	public void MedKitHover()
	{
		HandReticle handReticle = HandReticle.main;
		handReticle.SetText(HandReticle.TextType.Hand, medKitOpen ? "EscapePodMedKitClose" : "EscapePodMedKitOpen", translate: true, GameInput.Button.LeftHand);
		handReticle.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		handReticle.SetIcon(HandReticle.IconType.Hand);
	}

	public void MedKitOpen()
	{
		medKitOpen = true;
		MedKitUpdateState();
	}

	public void MedKitClose()
	{
		medKitOpen = false;
		MedKitUpdateState();
	}

	private void MedKitUpdateState()
	{
		SafeAnimator.SetBool(animator, "firstaid_open", medKitOpen);
		FMODAsset fMODAsset = (medKitOpen ? medKitOpenSound : medKitCloseSound);
		if (fMODAsset != null)
		{
			Utils.PlayFMODAsset(fMODAsset, medKitPivot, 1f);
		}
	}
}
