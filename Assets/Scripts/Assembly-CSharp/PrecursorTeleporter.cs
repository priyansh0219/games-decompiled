using System;
using System.Collections;
using ProtoBuf;
using UnityEngine;
using UnityEngine.AddressableAssets;

[ProtoContract]
public class PrecursorTeleporter : MonoBehaviour
{
	public delegate void TeleportAction();

	public string teleporterIdentifier;

	public Vector3 warpToPos;

	public float warpToAngle;

	[AssertNotNull]
	public PlayerCinematicController cinematicTriggerIn;

	[AssertNotNull]
	public AssetReferenceGameObject cinematicEndControllerPrefabReference;

	[AssertNotNull]
	public GameObject portalFxPrefab;

	[AssertNotNull]
	public Transform portalFxSpawnPoint;

	public Transform registerPrisonExitPoint;

	public bool alwaysOn;

	public bool hasFirstUse = true;

	private GameObject warpObject;

	private VFXPrecursorTeleporter portalFxControl;

	[NonSerialized]
	[ProtoMember(1)]
	public bool isOpen;

	[AssertNotNull]
	public FMODAsset powerUpSound;

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter activeLoopSound;

	public static event TeleportAction TeleportEventStart;

	public static event TeleportAction TeleportEventEnd;

	private void OnEnable()
	{
		TeleporterManager.TeleporterActivateEvent += OnActivateTeleporter;
	}

	private void OnDisable()
	{
		TeleporterManager.TeleporterActivateEvent -= OnActivateTeleporter;
	}

	private void Start()
	{
		GameObject gameObject = Utils.SpawnZeroedAt(portalFxPrefab, portalFxSpawnPoint);
		portalFxControl = gameObject.GetComponent<VFXPrecursorTeleporter>();
		if (alwaysOn)
		{
			InitializeDoor(open: true);
		}
		else
		{
			bool teleporterActive = TeleporterManager.GetTeleporterActive(teleporterIdentifier);
			InitializeDoor(teleporterActive);
		}
		if (registerPrisonExitPoint != null)
		{
			PrisonManager.main.RegisterExitPoint(registerPrisonExitPoint.position);
		}
	}

	private void SetWarpPosition()
	{
		if (!(warpObject == null))
		{
			Quaternion quaternion = Quaternion.Euler(new Vector3(0f, warpToAngle, 0f));
			if ((bool)warpObject.GetComponentInChildren<Vehicle>())
			{
				warpObject.GetComponentInChildren<Vehicle>().TeleportVehicle(warpToPos, quaternion);
			}
			else
			{
				warpObject.transform.position = warpToPos;
				warpObject.transform.rotation = quaternion;
			}
			Player.main.WaitForTeleportation();
			warpObject = null;
		}
	}

	public static void TeleportationComplete()
	{
		if (PrecursorTeleporter.TeleportEventEnd != null)
		{
			PrecursorTeleporter.TeleportEventEnd();
		}
	}

	public void OnPlayerCinematicModeEnd(PlayerCinematicController controller)
	{
		warpObject = null;
		BeginTeleportPlayer(Player.main.gameObject);
	}

	public IEnumerator BeginTeleportPlayer(GameObject teleportObject)
	{
		if (!alwaysOn && (!TeleporterManager.GetTeleporterActive(teleporterIdentifier) || warpObject != null))
		{
			yield break;
		}
		warpObject = teleportObject;
		bool flag = teleportObject.Equals(Player.main.gameObject);
		bool flag2 = Player.main.AddUsedTool(TechType.PrecursorTeleporter) || PlayerToolConsoleCommands.debugFirstUse;
		if (hasFirstUse && flag2 && flag)
		{
			cinematicTriggerIn.cinematicModeActive = false;
			cinematicTriggerIn.StartCinematicMode(Player.main);
			yield break;
		}
		Player.main.cinematicModeActive = true;
		Player.main.playerController.inputEnabled = false;
		Inventory.main.quickSlots.SetIgnoreHotkeyInput(ignore: true);
		Player.main.GetPDA().Close();
		Player.main.GetPDA().SetIgnorePDAInput(ignore: true);
		Player.main.teleportingLoopSound.Play();
		if (flag)
		{
			_ = Quaternion.identity;
			Quaternion rotation = Quaternion.Euler(0f, warpToAngle, 0f);
			CoroutineTask<GameObject> task = AddressablesUtility.InstantiateAsync(cinematicEndControllerPrefabReference.RuntimeKey as string, null, warpToPos, rotation);
			yield return task;
			if (task.GetResult() == null)
			{
				Debug.LogError("PrecursorTeleporter.BeginTeleportPlayer failed: " + base.gameObject.name);
				Player.main.CompleteTeleportation();
				yield break;
			}
		}
		Vehicle component = teleportObject.GetComponent<Vehicle>();
		if (component != null)
		{
			component.OnTeleportationStart();
		}
		if (PrecursorTeleporter.TeleportEventStart != null)
		{
			PrecursorTeleporter.TeleportEventStart();
		}
		Invoke("SetWarpPosition", 1f);
	}

	private void OnActivateTeleporter(string identifier)
	{
		if (!(identifier != teleporterIdentifier))
		{
			ToggleDoor(open: true);
		}
	}

	private void InitializeDoor(bool open)
	{
		if (portalFxControl != null)
		{
			portalFxControl.Toggle(open);
		}
		if (open && !isOpen)
		{
			isOpen = true;
			TeleporterManager.ActivateTeleporter(teleporterIdentifier);
			activeLoopSound.Play();
		}
	}

	public void ToggleDoor(bool open)
	{
		if (portalFxControl != null)
		{
			if (open)
			{
				portalFxControl.FadeIn();
			}
			else
			{
				portalFxControl.FadeOut();
			}
		}
		if (open && !isOpen)
		{
			isOpen = true;
			Utils.PlayFMODAsset(powerUpSound, base.transform);
			TeleporterManager.ActivateTeleporter(teleporterIdentifier);
			activeLoopSound.Play();
		}
	}
}
