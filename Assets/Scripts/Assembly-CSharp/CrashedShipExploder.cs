using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class CrashedShipExploder : MonoBehaviour, IProtoEventListener
{
	public static CrashedShipExploder main;

	public GameObject crashedShipPrefab;

	private const float delayBeforeExplosionSound = 24f;

	private const float delayBeforeExplosionFX = 25f;

	private const float delayBeforeSwap = 27f;

	public GameObject[] disableOnExplosion;

	public GameObject[] enableOnExplosion;

	[AssertNotNull]
	public GameObject explodedExterior;

	public VFXController fxControl;

	public GameObject playerFXprefab;

	public VFXEarthquake earthquakeFX;

	public FMODAsset rumbleSound;

	public FMOD_StudioEventEmitter shipCountdownSound;

	public FMOD_StudioEventEmitter shipExplodeSound;

	private GameObject playerFXInstance;

	private VFXExplosionPlayerFX playerFXcontrol;

	private Utils.ScalarMonitor timeMonitor = new Utils.ScalarMonitor(0f);

	private bool initialized;

	private bool deserialized;

	private bool legacyData;

	private const int currentVersion = 2;

	private static string crashedShip = "crashedShip";

	[NonSerialized]
	[ProtoMember(1)]
	public float timeToStartCountdown;

	[NonSerialized]
	[ProtoMember(2)]
	public float timeSerialized;

	[NonSerialized]
	[ProtoMember(3)]
	public int version;

	[NonSerialized]
	[ProtoMember(4)]
	public float timeToStartWarning;

	private float timeNextShake;

	private void Start()
	{
		main = this;
		DevConsole.RegisterConsoleCommand(this, "countdownship");
		DevConsole.RegisterConsoleCommand(this, "explodeship");
		DevConsole.RegisterConsoleCommand(this, "restoreship");
		DevConsole.RegisterConsoleCommand(this, "explodeforce");
		if (!deserialized)
		{
			SwapModels(exploded: false);
		}
	}

	private void SetExplodeTime()
	{
		float num = UnityEngine.Random.Range(2.3f, 4f);
		timeToStartWarning = DayNightCycle.main.timePassedAsFloat;
		timeToStartCountdown = timeToStartWarning + num * 1200f;
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		version = 2;
		timeSerialized = timeMonitor.Get();
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		if (playerFXInstance != null)
		{
			UnityEngine.Object.Destroy(playerFXInstance);
		}
		fxControl.StopAndDestroy(0, 0f);
		fxControl.StopAndDestroy(1, 0f);
		timeMonitor.Init(timeSerialized);
		deserialized = true;
		if (version < 2)
		{
			Debug.LogWarning("Replacing legacy data " + timeSerialized + "s played, countdown at " + timeToStartCountdown + "s", this);
			timeToStartWarning = 1000000f;
			timeToStartCountdown = 1000000f;
			legacyData = true;
			base.enabled = true;
		}
		if (timeSerialized > timeToStartCountdown && timeSerialized < timeToStartCountdown + 27f)
		{
			fxControl.Play(1);
		}
		SwapModels(IsExploded());
	}

	private void OnConsoleCommand_countdownship()
	{
		timeToStartCountdown = timeMonitor.Get() + 1f;
		timeToStartWarning = timeToStartCountdown;
		ErrorMessage.AddDebug("initiate final countdown");
	}

	private void OnConsoleCommand_explodeship()
	{
		timeToStartCountdown = timeMonitor.Get() - 25f + 1f;
		timeToStartWarning = timeToStartCountdown - 1f;
		ErrorMessage.AddDebug("initiate ship explosion");
	}

	private void OnConsoleCommand_restoreship()
	{
		SetExplodeTime();
		SwapModels(exploded: false);
		fxControl.StopAndDestroy(0, 0f);
		fxControl.StopAndDestroy(1, 0f);
	}

	private void OnConsoleCommand_explodeforce()
	{
		CreateExplosiveForce();
	}

	private void Update()
	{
		if (LargeWorldStreamer.main == null || !LargeWorldStreamer.main.IsReady())
		{
			return;
		}
		if (!initialized && (!deserialized || legacyData))
		{
			Debug.Log("initializing first time explosion", this);
			SetExplodeTime();
			SwapModels(exploded: false);
			initialized = true;
		}
		if (DayNightCycle.main != null)
		{
			timeMonitor.Update(DayNightCycle.main.timePassedAsFloat);
			if (timeMonitor.JustWentAbove(timeToStartCountdown + 27f))
			{
				SwapModels(exploded: true);
			}
			else if (timeMonitor.JustWentAbove(timeToStartCountdown + 25f))
			{
				PlayExplosionFX();
				ShakePlayerCamera();
				CreateExplosiveForce();
				DamageSystem.RadiusDamage(2000f, base.transform.position, 500f, DamageType.Explosive, base.gameObject);
			}
			else if (timeMonitor.JustWentAbove(timeToStartCountdown + 24f))
			{
				shipExplodeSound.StartEvent();
			}
			else if (timeMonitor.JustWentAbove(timeToStartCountdown))
			{
				shipCountdownSound.StartEvent();
				base.gameObject.BroadcastMessage("OnShipExplode", SendMessageOptions.DontRequireReceiver);
				fxControl.Play(1);
			}
		}
		if (IsExploded())
		{
			UpdatePlayerCamShake();
		}
	}

	private void UpdatePlayerCamShake()
	{
		if (timeNextShake <= Time.time && Player.main.GetBiomeString() == crashedShip)
		{
			MainCameraControl.main.ShakeCamera(0.45f, 8f, MainCameraControl.ShakeMode.BuildUp, 1.4f);
			Utils.PlayFMODAsset(rumbleSound, Player.main.transform);
			timeNextShake = Time.time + 15f + UnityEngine.Random.value * 10f;
			if (earthquakeFX != null)
			{
				earthquakeFX.Shake();
			}
		}
	}

	private void CreateExplosiveForce()
	{
		WorldForces.AddExplosion(base.transform.position, timeMonitor.Get(), 8f, 5000f);
	}

	private void PlayExplosionFX()
	{
		fxControl.StopAndDestroy(1, 40f);
		fxControl.Play(0);
		GameObject localPlayer = Utils.GetLocalPlayer();
		playerFXInstance = Utils.SpawnZeroedAt(playerFXprefab, localPlayer.transform);
		playerFXcontrol = playerFXInstance.GetComponent<VFXExplosionPlayerFX>();
		playerFXcontrol.crashedShipTrans = base.transform;
	}

	private void ShakePlayerCamera()
	{
		MainCameraControl.main.ShakeCamera(4f, 8f, MainCameraControl.ShakeMode.Quadratic, 1.2f);
	}

	private void SwapModels(bool exploded)
	{
		for (int i = 0; i < disableOnExplosion.Length; i++)
		{
			disableOnExplosion[i].SetActive(!exploded);
		}
		for (int j = 0; j < enableOnExplosion.Length; j++)
		{
			enableOnExplosion[j].SetActive(exploded);
		}
	}

	public bool IsExploded()
	{
		return timeMonitor.Get() > timeToStartCountdown + 27f;
	}

	public float GetTimeToStartCountdown()
	{
		return timeToStartCountdown;
	}

	public float GetTimeToStartWarning()
	{
		return timeToStartWarning;
	}

	public void CullExplodedExterior(bool visible)
	{
		if (initialized && deserialized && IsExploded())
		{
			explodedExterior.SetActive(visible);
		}
	}
}
