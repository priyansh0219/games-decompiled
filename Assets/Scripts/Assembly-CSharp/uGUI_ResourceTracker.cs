using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class uGUI_ResourceTracker : MonoBehaviour
{
	private class Blip
	{
		public GameObject gameObject;

		public RectTransform rect;

		public TextMeshProUGUI text;

		public TechType techType;
	}

	private const ManagedUpdate.Queue updateQueue = ManagedUpdate.Queue.PreCanvasPing;

	[AssertNotNull]
	public RectTransform canvasRect;

	[AssertNotNull]
	public CanvasGroup canvasGroup;

	[AssertNotNull]
	public GameObject blip;

	private readonly List<Blip> blips = new List<Blip>();

	private readonly HashSet<ResourceTrackerDatabase.ResourceInfo> nodes = new HashSet<ResourceTrackerDatabase.ResourceInfo>();

	private readonly List<TechType> techTypes = new List<TechType>();

	private readonly List<MapRoomFunctionality> mapRooms = new List<MapRoomFunctionality>();

	private bool visible;

	private bool gatherNextTick;

	private RectTransform blipRect;

	private bool showAll;

	private void Start()
	{
		blipRect = blip.GetComponent<RectTransform>();
		DevConsole.RegisterConsoleCommand(this, "showresources");
		InvokeRepeating("GatherNodes", Random.value, 10f);
		ResourceTrackerDatabase.onResourceRemoved += OnResourceRemoved;
		MapRoomCamera.onMapRoomCameraChanged += OnMapRoomCameraChanged;
		MapRoomCamera.onMapRoomCameraExited += OnMapRoomCameraExited;
	}

	private void OnEnable()
	{
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.PreCanvasPing, UpdateBlips);
	}

	private void OnDisable()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.PreCanvasPing, UpdateBlips);
	}

	private void OnDestroy()
	{
		ResourceTrackerDatabase.onResourceRemoved -= OnResourceRemoved;
		MapRoomCamera.onMapRoomCameraChanged -= OnMapRoomCameraChanged;
		MapRoomCamera.onMapRoomCameraExited -= OnMapRoomCameraExited;
	}

	private void OnResourceRemoved(ResourceTrackerDatabase.ResourceInfo info)
	{
		gatherNextTick = true;
	}

	private void OnMapRoomCameraChanged(MapRoomCamera toCamera)
	{
		gatherNextTick = true;
	}

	private void OnMapRoomCameraExited()
	{
		OnMapRoomCameraChanged(null);
	}

	private void OnConsoleCommand_showresources()
	{
		showAll = !showAll;
		ErrorMessage.AddDebug("showresources = " + showAll);
	}

	private bool IsVisibleNow()
	{
		Player main = Player.main;
		if (main == null)
		{
			return false;
		}
		Inventory main2 = Inventory.main;
		if (main2 == null)
		{
			return false;
		}
		Equipment equipment = main2.equipment;
		if (equipment == null)
		{
			return false;
		}
		uGUI_CameraDrone main3 = uGUI_CameraDrone.main;
		if (main3 == null)
		{
			return false;
		}
		if (main.cinematicModeActive && main.GetPilotingChair() == null)
		{
			return false;
		}
		if (main.GetMode() == Player.Mode.Sitting)
		{
			return false;
		}
		PDA pDA = main.GetPDA();
		if (pDA == null)
		{
			return false;
		}
		_ = uGUI_PDA.main;
		if (pDA.isInUse)
		{
			return false;
		}
		if (uGUI.main.craftingMenu.selected)
		{
			return false;
		}
		if (!showAll && !(main3.GetCamera() != null))
		{
			return equipment.GetCount(TechType.MapRoomHUDChip) > 0;
		}
		return true;
	}

	private void GatherNodes()
	{
		if (showAll)
		{
			GatherAll();
		}
		else if (visible)
		{
			GatherScanned();
		}
	}

	private void GatherAll()
	{
		Camera camera = MainCamera.camera;
		nodes.Clear();
		techTypes.Clear();
		ResourceTrackerDatabase.GetTechTypesInRange(camera.transform.position, 500f, techTypes);
		for (int i = 0; i < techTypes.Count; i++)
		{
			TechType techType = techTypes[i];
			ResourceTrackerDatabase.GetNodes(camera.transform.position, 500f, techType, nodes);
		}
	}

	private void GatherScanned()
	{
		Camera camera = MainCamera.camera;
		nodes.Clear();
		mapRooms.Clear();
		MapRoomScreen screen = uGUI_CameraDrone.main.GetScreen();
		if (screen != null)
		{
			mapRooms.Add(screen.mapRoomFunctionality);
		}
		else
		{
			MapRoomFunctionality.GetMapRoomsInRange(camera.transform.position, 500f, mapRooms);
		}
		for (int i = 0; i < mapRooms.Count; i++)
		{
			if (mapRooms[i].GetActiveTechType() != 0)
			{
				mapRooms[i].GetDiscoveredNodes(nodes);
			}
		}
	}

	private void UpdateBlips()
	{
		bool flag = IsVisibleNow();
		if (visible != flag)
		{
			visible = flag;
			canvasGroup.alpha = (visible ? 1f : 0f);
		}
		if (!visible)
		{
			return;
		}
		Camera camera = MainCamera.camera;
		Vector3 position = camera.transform.position;
		Vector3 forward = camera.transform.forward;
		int num = 0;
		HashSet<ResourceTrackerDatabase.ResourceInfo>.Enumerator enumerator = nodes.GetEnumerator();
		while (enumerator.MoveNext())
		{
			ResourceTrackerDatabase.ResourceInfo current = enumerator.Current;
			if (Vector3.Dot(current.position - position, forward) > 0f)
			{
				if (num >= blips.Count)
				{
					GameObject gameObject = Object.Instantiate(this.blip, Vector3.zero, Quaternion.identity);
					RectTransform component = gameObject.GetComponent<RectTransform>();
					component.SetParent(canvasRect, worldPositionStays: false);
					component.localScale = blipRect.localScale;
					Blip blip = new Blip();
					blip.gameObject = gameObject;
					blip.rect = component;
					blip.text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
					blip.techType = TechType.None;
					blips.Add(blip);
				}
				Blip blip2 = blips[num];
				blip2.gameObject.SetActive(value: true);
				Vector2 vector = camera.WorldToViewportPoint(current.position);
				blip2.rect.anchorMin = vector;
				blip2.rect.anchorMax = vector;
				if (blip2.techType != current.techType)
				{
					string text = Language.main.Get(current.techType.AsString());
					blip2.text.text = text;
					blip2.techType = current.techType;
				}
				num++;
			}
		}
		for (int i = num; i < blips.Count; i++)
		{
			blips[i].gameObject.SetActive(value: false);
		}
	}

	private void LateUpdate()
	{
		if (gatherNextTick)
		{
			GatherScanned();
			gatherNextTick = false;
		}
	}
}
