using UnityEngine;

public class CyclopsExternalCams : MonoBehaviour, IInputHandler
{
	public LiveMixin liveMixin;

	public Transform[] externalCamPositions = new Transform[3];

	public Light cameraLight;

	public CyclopsLightingPanel lightingPanel;

	private bool active;

	private int cameraIndex;

	public const GameInput.Button buttonPrev = GameInput.Button.CyclePrev;

	public const GameInput.Button buttonNext = GameInput.Button.CycleNext;

	public const GameInput.Button buttonToggleLight = GameInput.Button.LeftHand;

	public static readonly GameInput.Button[] buttonsExit = new GameInput.Button[2]
	{
		GameInput.Button.Exit,
		GameInput.Button.RightHand
	};

	private static int lightState = 1;

	private const ManagedUpdate.Queue updateQueue = ManagedUpdate.Queue.LateUpdate;

	private void Start()
	{
		cameraLight.enabled = false;
	}

	private void ChangeCamera(int iterate)
	{
		int num = externalCamPositions.Length;
		cameraIndex = (cameraIndex + iterate) % num;
		if (cameraIndex < 0)
		{
			cameraIndex += num;
		}
		for (int i = 0; i < externalCamPositions.Length; i++)
		{
			Transform transform = externalCamPositions[i];
			if (!(transform == null) && transform.TryGetComponent<CyclopsCameraInput>(out var component))
			{
				if (i == cameraIndex)
				{
					component.ActivateCamera(cameraLight);
				}
				else
				{
					component.DeactivateCamera();
				}
			}
		}
		uGUI_CameraCyclops.main.SetCamera(cameraIndex);
		lightState = 1;
		SetLight();
	}

	public bool GetActive()
	{
		return active;
	}

	public void SetActive(bool value)
	{
		if (active == value)
		{
			return;
		}
		active = value;
		if (active)
		{
			InputHandlerStack.main.Push(this);
			_ = Player.main;
			MainCameraControl.main.enabled = false;
			Player.main.SetHeadVisible(visible: true);
			VRUtil.Recenter();
			cameraLight.enabled = true;
			ChangeCamera(0);
			if ((bool)lightingPanel)
			{
				lightingPanel.TempTurnOffFloodlights();
			}
		}
		else
		{
			_ = Player.main;
			SNCameraRoot.main.transform.localPosition = Vector3.zero;
			SNCameraRoot.main.transform.localRotation = Quaternion.identity;
			MainCameraControl.main.enabled = true;
			Player.main.SetHeadVisible(visible: false);
			uGUI_CameraCyclops.main.SetCamera(-1);
			cameraLight.enabled = false;
			if ((bool)lightingPanel)
			{
				lightingPanel.RestoreFloodlightsFromTempState();
			}
		}
	}

	public bool HandleInput()
	{
		if (!active)
		{
			return false;
		}
		if (!liveMixin.IsAlive())
		{
			SetActive(value: false);
			return false;
		}
		Transform transform = externalCamPositions[cameraIndex];
		if (transform == null || !transform.TryGetComponent<CyclopsCameraInput>(out var component))
		{
			SetActive(value: false);
			return false;
		}
		if (GameInputExtensions.GetButtonDown(buttonsExit))
		{
			SetActive(value: false);
			return false;
		}
		if (GameInput.GetButtonDown(GameInput.Button.CycleNext))
		{
			ChangeCamera(1);
		}
		else if (GameInput.GetButtonDown(GameInput.Button.CyclePrev))
		{
			ChangeCamera(-1);
		}
		if (GameInput.GetButtonDown(GameInput.Button.LeftHand))
		{
			IterateLightState();
			SetLight();
		}
		component.HandleInput();
		SNCameraRoot.main.transform.position = transform.position;
		SNCameraRoot.main.transform.rotation = transform.rotation;
		return true;
	}

	public bool HandleLateInput()
	{
		return true;
	}

	public void OnFocusChanged(InputFocusMode mode)
	{
		switch (mode)
		{
		}
	}

	private void SetLight()
	{
		switch (lightState)
		{
		case 0:
			cameraLight.enabled = false;
			break;
		case 1:
			cameraLight.enabled = true;
			cameraLight.color = Color.white;
			break;
		case 2:
			cameraLight.enabled = true;
			cameraLight.color = new Color(0.5f, 0.5f, 0.5f, 1f);
			break;
		}
	}

	private static void IterateLightState()
	{
		lightState++;
		if (lightState == 3)
		{
			lightState = 0;
		}
	}
}
