using ProtoBuf;
using TMPro;
using UnityEngine;

[ProtoContract]
public class MapRoomScreen : HandTarget, IHandTarget, IInputHandler
{
	[AssertLocalization]
	public string hoverText = "ControllCamera";

	[AssertNotNull]
	public TextMeshProUGUI cameraText;

	[Tooltip("The GameObject that contains the preview of what the reported camera in Camera Text is seeing.")]
	[AssertNotNull]
	public GameObject cameraPreview;

	[AssertNotNull]
	public MapRoomFunctionality mapRoomFunctionality;

	private const int kNoCameras = -1;

	private int currentIndex = -1;

	private MapRoomCamera currentCamera;

	private BaseRoot baseRoot;

	public const float maxCameraDistance = 500f;

	[AssertLocalization(1)]
	public const string cameraInfoText = "MapRoomCameraInfoScreen";

	[AssertLocalization]
	private const string noCamerasText = "MapRoomCameraScreenNoCameras";

	private void Start()
	{
		baseRoot = GetComponentInParent<BaseRoot>();
		MapRoomCamera.onMapRoomCameraChanged += OnMapRoomCameraChanged;
		Player.main.currentSubChangedEvent.AddHandler(this, OnCurrentSubChanged);
		if (Player.main.currentSub == baseRoot)
		{
			OnCurrentSubChanged(baseRoot);
		}
	}

	private void OnDisable()
	{
		if ((bool)currentCamera)
		{
			currentCamera.FreeCamera();
			currentCamera.ExitLockedMode();
		}
	}

	private void OnDestroy()
	{
		MapRoomCamera.onMapRoomCameraChanged -= OnMapRoomCameraChanged;
		if ((bool)Player.main)
		{
			Player.main.currentSubChangedEvent.RemoveHandler(this);
		}
	}

	public void OnMapRoomCameraChanged(MapRoomCamera toCamera)
	{
		cameraText.text = Language.main.GetFormat("MapRoomCameraInfoScreen", toCamera.GetCameraNumber());
		currentIndex = MapRoomCamera.cameras.IndexOf(toCamera);
		cameraPreview.SetActive(value: true);
	}

	private void OnCurrentSubChanged(SubRoot sub)
	{
		if (sub == baseRoot)
		{
			MapRoomCamera mapRoomCamera = FindCamera();
			if (mapRoomCamera == null)
			{
				cameraText.text = Language.main.GetFormat("MapRoomCameraScreenNoCameras");
				currentIndex = -1;
				cameraPreview.SetActive(value: false);
			}
			else
			{
				OnMapRoomCameraChanged(mapRoomCamera);
			}
		}
	}

	public MapRoomCamera GetCurrentCamera()
	{
		return currentCamera;
	}

	private int NormalizeIndex(int index)
	{
		if (MapRoomCamera.cameras.Count != 0)
		{
			if (index < 0)
			{
				index += MapRoomCamera.cameras.Count;
			}
			else if (index >= MapRoomCamera.cameras.Count)
			{
				index %= MapRoomCamera.cameras.Count;
			}
		}
		return index;
	}

	public MapRoomCamera FindCamera(int direction = 1)
	{
		direction = (int)Mathf.Sign(direction);
		for (int i = 0; i < MapRoomCamera.cameras.Count; i++)
		{
			int index = NormalizeIndex(currentIndex + i * direction);
			if (MapRoomCamera.cameras[index].CanBeControlled(this))
			{
				currentIndex = index;
				return MapRoomCamera.cameras[index];
			}
		}
		return null;
	}

	public void OnHandHover(GUIHand hand)
	{
		if (currentIndex != -1)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, hoverText, translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Interact);
		}
	}

	public void OnHandClick(GUIHand guiHand)
	{
		if (currentIndex != -1)
		{
			currentIndex = NormalizeIndex(currentIndex);
			MapRoomCamera mapRoomCamera = FindCamera();
			if ((bool)mapRoomCamera)
			{
				currentCamera = mapRoomCamera;
				mapRoomCamera.ControlCamera(this);
				InputHandlerStack.main.Push(this);
			}
		}
	}

	public void OnCameraFree(MapRoomCamera camera)
	{
		if (camera == currentCamera)
		{
			currentCamera = null;
		}
	}

	public void CycleCamera(int direction = 1)
	{
		currentIndex += direction;
		currentIndex = NormalizeIndex(currentIndex);
		MapRoomCamera mapRoomCamera = FindCamera(direction);
		if (mapRoomCamera != null && mapRoomCamera != currentCamera)
		{
			currentCamera.FreeCamera(resetPlayerPosition: false);
			currentCamera.ExitLockedMode(resetPlayerPosition: false);
			currentCamera = mapRoomCamera;
			mapRoomCamera.ControlCamera(this);
		}
	}

	public bool HandleInput()
	{
		if (currentCamera != null)
		{
			currentCamera.HandleInput();
			return true;
		}
		return false;
	}

	public bool HandleLateInput()
	{
		return true;
	}

	public void OnFocusChanged(InputFocusMode mode)
	{
	}
}
