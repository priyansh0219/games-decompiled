using System;
using System.Collections.Generic;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class MapRoomFunctionality : MonoBehaviour, IObstacle
{
	public delegate void OnScanRangeChanged();

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public int numNodesScanned;

	[NonSerialized]
	[ProtoMember(3)]
	public TechType typeToScan;

	[AssertNotNull]
	public Transform wireFrameWorld;

	[AssertNotNull]
	public GameObject screenRoot;

	[AssertNotNull]
	public GameObject worlddisplay;

	[Tooltip("Blip that is always present on the map to show the room location.")]
	[AssertNotNull]
	public MapRoomCameraBlip roomBlip;

	[AssertNotNull]
	public GameObject blipPrefab;

	[AssertNotNull]
	public MapRoomCameraBlip cameraBlipPrefab;

	[AssertNotNull]
	public GameObject cameraBlipRoot;

	[AssertNotNull]
	public StorageContainer storageContainer;

	[AssertNotNull]
	public GameObject[] upgradeSlots;

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter ambientSound;

	[AssertNotNull]
	public PowerConsumer powerConsumer;

	[AssertNotNull]
	public RenderTexture mapRoomRenderTexture;

	[AssertNotNull]
	public MiniWorld miniWorld;

	[AssertNotNull]
	public GameObject scannerCullable;

	[AssertNotNull]
	public PlayerDistanceTracker playerDistanceTracker;

	public float hologramRadius = 1f;

	public int mapChunkSize = 32;

	public int mapLOD = 2;

	public float scanIntensity = 1f;

	public OnScanRangeChanged onScanRangeChanged;

	public const int mapScanRadius = 500;

	private const float defaultRange = 300f;

	private const float rangePerUpgrade = 50f;

	private const float baseScanTime = 14f;

	private const float scanTimeReductionPerUpgrade = 3f;

	private const float rotationTime = 50f;

	private const float powerPerSecond = 0.5f;

	private const float idlePowerPerSecond = 0.15f;

	[AssertLocalization(1)]
	private const string scanRangeText = "MapRoomScanningRange";

	[AssertLocalization]
	private const string deconstructNonEmptyMessage = "MapRoomDeconstructErrorNotEmpty";

	private GameObject mapWorld;

	private GameObject mapBlipRoot;

	private readonly List<ResourceTrackerDatabase.ResourceInfo> resourceNodes = new List<ResourceTrackerDatabase.ResourceInfo>();

	private readonly List<GameObject> mapBlips = new List<GameObject>();

	private readonly List<MapRoomCameraBlip> cameraBlips = new List<MapRoomCameraBlip>();

	private double timeLastScan;

	private bool scanActive;

	private bool prevScanActive;

	private float prevFadeRadius;

	private float prevScanInterval;

	private bool containerIsDirty = true;

	private static readonly List<MapRoomFunctionality> mapRooms = new List<MapRoomFunctionality>();

	private bool subscribed;

	private readonly TechType[] allowedUpgrades = new TechType[2]
	{
		TechType.MapRoomUpgradeScanRange,
		TechType.MapRoomUpgradeScanSpeed
	};

	private bool powered;

	private float timeLastPowerDrain;

	private float scanRange = 300f;

	private float scanInterval = 14f;

	private Coroutine loadingRoutine;

	private float mapScale => hologramRadius / 500f;

	public static void GetMapRoomsInRange(Vector3 position, float range, ICollection<MapRoomFunctionality> outlist)
	{
		float num = range * range;
		for (int i = 0; i < mapRooms.Count; i++)
		{
			MapRoomFunctionality mapRoomFunctionality = mapRooms[i];
			if ((mapRoomFunctionality.transform.position - position).sqrMagnitude <= num)
			{
				outlist.Add(mapRoomFunctionality);
			}
		}
	}

	private void Start()
	{
		wireFrameWorld.rotation = Quaternion.identity;
		if (typeToScan != 0)
		{
			double num = timeLastScan;
			int num2 = numNodesScanned;
			StartScanning(typeToScan);
			timeLastScan = num;
			numNodesScanned = num2;
		}
		GetComponentInParent<Base>().onPostRebuildGeometry += OnPostRebuildGeometry;
		ResourceTrackerDatabase.onResourceDiscovered += OnResourceDiscovered;
		ResourceTrackerDatabase.onResourceRemoved += OnResourceRemoved;
		mapRooms.Add(this);
		Subscribe(state: true);
		powered = !GameModeUtils.RequiresPower() || powerConsumer.IsPowered();
		screenRoot.SetActive(powered);
		worlddisplay.SetActive(powered);
		if (powered)
		{
			ambientSound.Play();
		}
	}

	private void OnPowerUp()
	{
		screenRoot.SetActive(value: true);
		worlddisplay.SetActive(value: true);
		timeLastScan = 0.0;
		ambientSound.Play();
	}

	private void OnPowerDown()
	{
		screenRoot.SetActive(value: false);
		worlddisplay.SetActive(value: false);
		ambientSound.Stop();
	}

	public void OnResourceDiscovered(ResourceTrackerDatabase.ResourceInfo info)
	{
		if (typeToScan == info.techType && (wireFrameWorld.position - info.position).sqrMagnitude <= 250000f)
		{
			resourceNodes.Add(info);
		}
	}

	public void OnResourceRemoved(ResourceTrackerDatabase.ResourceInfo info)
	{
		if (typeToScan == info.techType)
		{
			resourceNodes.Remove(info);
		}
	}

	public TechType GetActiveTechType()
	{
		return typeToScan;
	}

	private void OnPostRebuildGeometry(Base b)
	{
		Int3 @int = b.NormalizeCell(b.WorldToGrid(base.transform.position));
		Base.CellType cell = b.GetCell(@int);
		if (cell != Base.CellType.MapRoom && cell != Base.CellType.MapRoomRotated)
		{
			Debug.Log(string.Concat("map room had been destroyed, at cell ", @int, " new celltype is ", cell));
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Update()
	{
		bool flag = powered;
		powered = powerConsumer.IsPowered();
		if (powered != flag)
		{
			if (powered)
			{
				OnPowerUp();
			}
			else
			{
				OnPowerDown();
			}
		}
		if (containerIsDirty)
		{
			UpdateScanRangeAndInterval();
			UpdateModel();
			containerIsDirty = false;
		}
		UpdateScanning();
		miniWorld.gameObject.SetActive(playerDistanceTracker.playerNearby);
		scannerCullable.SetActive(playerDistanceTracker.playerNearby);
	}

	private void UpdateModel()
	{
		int count = storageContainer.container.count;
		for (int i = 0; i < upgradeSlots.Length; i++)
		{
			upgradeSlots[i].SetActive(i < count);
		}
		roomBlip.cameraName.text = Language.main.GetFormat("MapRoomScanningRange", Mathf.RoundToInt(scanRange));
	}

	private void UpdateScanRangeAndInterval()
	{
		float num = scanRange;
		scanRange = Mathf.Min(500f, 300f + (float)storageContainer.container.GetCount(TechType.MapRoomUpgradeScanRange) * 50f);
		scanInterval = Mathf.Max(1f, 14f - (float)storageContainer.container.GetCount(TechType.MapRoomUpgradeScanSpeed) * 3f);
		if (scanRange != num)
		{
			ObtainResourceNodes(typeToScan);
			if (onScanRangeChanged != null)
			{
				onScanRangeChanged();
			}
		}
	}

	public float GetScanRange()
	{
		return scanRange;
	}

	private void ObtainResourceNodes(TechType typeToScan)
	{
		resourceNodes.Clear();
		Vector3 scannerPos = wireFrameWorld.position;
		ICollection<ResourceTrackerDatabase.ResourceInfo> nodes = ResourceTrackerDatabase.GetNodes(typeToScan);
		if (nodes != null)
		{
			float num = scanRange * scanRange;
			foreach (ResourceTrackerDatabase.ResourceInfo item in nodes)
			{
				if ((scannerPos - item.position).sqrMagnitude <= num)
				{
					resourceNodes.Add(item);
				}
			}
		}
		resourceNodes.Sort(delegate(ResourceTrackerDatabase.ResourceInfo a, ResourceTrackerDatabase.ResourceInfo b)
		{
			float sqrMagnitude = (a.position - scannerPos).sqrMagnitude;
			float sqrMagnitude2 = (b.position - scannerPos).sqrMagnitude;
			return sqrMagnitude.CompareTo(sqrMagnitude2);
		});
	}

	public void StartScanning(TechType newTypeToScan)
	{
		typeToScan = newTypeToScan;
		ObtainResourceNodes(typeToScan);
		mapBlips.Clear();
		UnityEngine.Object.Destroy(mapBlipRoot);
		mapBlipRoot = new GameObject("MapBlipRoot");
		mapBlipRoot.transform.SetParent(wireFrameWorld, worldPositionStays: false);
		scanActive = typeToScan != TechType.None;
		numNodesScanned = 0;
		timeLastScan = 0.0;
	}

	public IList<ResourceTrackerDatabase.ResourceInfo> GetNodes()
	{
		return resourceNodes;
	}

	public void GetDiscoveredNodes(ICollection<ResourceTrackerDatabase.ResourceInfo> outNodes)
	{
		int num = Mathf.Min(numNodesScanned, resourceNodes.Count);
		for (int i = 0; i < num; i++)
		{
			outNodes.Add(resourceNodes[i]);
		}
	}

	private void UpdateBlips()
	{
		if (!scanActive)
		{
			return;
		}
		Vector3 position = mapBlipRoot.transform.position;
		int num = Mathf.Min(numNodesScanned + 1, resourceNodes.Count);
		if (num != numNodesScanned)
		{
			numNodesScanned = num;
		}
		for (int i = 0; i < num; i++)
		{
			Vector3 vector = (resourceNodes[i].position - position) * mapScale;
			if (i >= mapBlips.Count)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(blipPrefab, vector, Quaternion.identity);
				gameObject.transform.SetParent(mapBlipRoot.transform, worldPositionStays: false);
				mapBlips.Add(gameObject);
			}
			mapBlips[i].transform.localPosition = vector;
			mapBlips[i].SetActive(value: true);
		}
		for (int j = num; j < mapBlips.Count; j++)
		{
			mapBlips[j].SetActive(value: false);
		}
	}

	private void UpdateCameraBlips()
	{
		float num = scanRange * scanRange;
		Vector3 position = cameraBlipRoot.transform.position;
		int num2 = 0;
		for (int i = 0; i < MapRoomCamera.cameras.Count; i++)
		{
			MapRoomCamera mapRoomCamera = MapRoomCamera.cameras[i];
			if (mapRoomCamera.pickupAble.attached)
			{
				continue;
			}
			Vector3 position2 = mapRoomCamera.transform.position;
			if ((wireFrameWorld.position - position2).sqrMagnitude <= num)
			{
				Vector3 vector = (position2 - position) * mapScale;
				if (num2 >= cameraBlips.Count)
				{
					MapRoomCameraBlip mapRoomCameraBlip = UnityEngine.Object.Instantiate(cameraBlipPrefab, vector, Quaternion.identity);
					mapRoomCameraBlip.transform.SetParent(cameraBlipRoot.transform, worldPositionStays: false);
					cameraBlips.Add(mapRoomCameraBlip);
				}
				cameraBlips[num2].transform.localPosition = vector;
				cameraBlips[num2].gameObject.SetActive(value: true);
				cameraBlips[num2].cameraName.text = Language.main.GetFormat("MapRoomCameraInfoScreen", mapRoomCamera.GetCameraNumber());
				num2++;
			}
		}
		for (int j = num2; j < cameraBlips.Count; j++)
		{
			cameraBlips[j].gameObject.SetActive(value: false);
		}
	}

	private void UpdateScanning()
	{
		DayNightCycle main = DayNightCycle.main;
		if (!main)
		{
			return;
		}
		Material materialInstance = miniWorld.materialInstance;
		double timePassed = main.timePassed;
		if (timeLastScan + (double)scanInterval <= timePassed && powered)
		{
			timeLastScan = timePassed;
			UpdateBlips();
			UpdateCameraBlips();
			float num = scanRange * mapScale;
			if (prevFadeRadius != num)
			{
				materialInstance.SetFloat(ShaderPropertyID._FadeRadius, num);
				prevFadeRadius = num;
			}
		}
		if (scanActive != prevScanActive || scanInterval != prevScanInterval)
		{
			float num2 = 1f / scanInterval;
			materialInstance.SetFloat(ShaderPropertyID._ScanIntensity, scanActive ? scanIntensity : 0f);
			materialInstance.SetFloat(ShaderPropertyID._ScanFrequency, scanActive ? num2 : 0f);
			prevScanActive = scanActive;
			prevScanInterval = scanInterval;
		}
		if (powered && timeLastPowerDrain + 1f < Time.time)
		{
			powerConsumer.ConsumePower(scanActive ? 0.5f : 0.15f, out var _);
			timeLastPowerDrain = Time.time;
		}
	}

	private void OnDestroy()
	{
		if (!GameApplication.isQuitting)
		{
			Base componentInParent = GetComponentInParent<Base>();
			if ((bool)componentInParent)
			{
				componentInParent.onPostRebuildGeometry -= OnPostRebuildGeometry;
			}
			ResourceTrackerDatabase.onResourceDiscovered -= OnResourceDiscovered;
			ResourceTrackerDatabase.onResourceRemoved -= OnResourceRemoved;
			mapRooms.Remove(this);
		}
	}

	private void Subscribe(bool state)
	{
		if (subscribed != state)
		{
			if (subscribed)
			{
				storageContainer.container.onAddItem -= AddItem;
				storageContainer.container.onRemoveItem -= RemoveItem;
				storageContainer.container.isAllowedToAdd = null;
				storageContainer.container.isAllowedToRemove = null;
			}
			else
			{
				storageContainer.container.onAddItem += AddItem;
				storageContainer.container.onRemoveItem += RemoveItem;
				storageContainer.container.isAllowedToAdd = IsAllowedToAdd;
			}
			subscribed = state;
		}
	}

	private void AddItem(InventoryItem item)
	{
		containerIsDirty = true;
	}

	private void RemoveItem(InventoryItem item)
	{
		containerIsDirty = true;
	}

	private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
	{
		TechType techType = pickupable.GetTechType();
		for (int i = 0; i < allowedUpgrades.Length; i++)
		{
			if (allowedUpgrades[i] == techType)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsDeconstructionObstacle()
	{
		return !storageContainer.IsEmpty();
	}

	public bool CanDeconstruct(out string reason)
	{
		if (!storageContainer.IsEmpty())
		{
			reason = Language.main.Get("MapRoomDeconstructErrorNotEmpty");
			return false;
		}
		reason = null;
		return true;
	}
}
