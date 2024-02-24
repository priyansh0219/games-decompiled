using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Rendering;

[ProtoContract]
public class MapRoomCamera : MonoBehaviour, IProtoEventListener
{
	private const int currentVersion = 2;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 2;

	[NonSerialized]
	[ProtoMember(2)]
	public int cameraNumber;

	[NonSerialized]
	[ProtoMember(3)]
	public bool lightState;

	[AssertNotNull]
	public Rigidbody rigidBody;

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter engineSound;

	[AssertNotNull]
	public FMODAsset lightOnSound;

	[AssertNotNull]
	public FMODAsset lightOffSound;

	[AssertNotNull]
	public GameObject screenEffectModel;

	[AssertNotNull]
	public Gradient gradientInner;

	[AssertNotNull]
	public Gradient gradientOuter;

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter droneIdle;

	[AssertNotNull]
	public EnergyMixin energyMixin;

	[AssertNotNull]
	public LiveMixin liveMixin;

	[AssertNotNull]
	public Pickupable pickupAble;

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter chargingSound;

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter connectingSound;

	[AssertNotNull]
	public FMODAsset connectedSound;

	[AssertNotNull]
	public GameObject lightsParent;

	[AssertNotNull]
	public WorldForces worldForces;

	[AssertNotNull]
	public PingInstance pingInstance;

	[AssertNotNull]
	public EcoTarget shinyTarget;

	[AssertNotNull]
	public RenderTexture mapRoomRenderTexture;

	private bool active;

	private MapRoomScreen screen;

	private Vector3 wishDir = Vector3.zero;

	private float controllStartTime;

	private MapRoomCameraDocking dockingPoint;

	private bool readyForControl;

	private bool justStartedControl;

	private static bool renderedFirstTexture = false;

	private Coroutine routine;

	public const GameInput.Button buttonPrev = GameInput.Button.CyclePrev;

	public const GameInput.Button buttonNext = GameInput.Button.CycleNext;

	public const GameInput.Button buttonToggleLight = GameInput.Button.RightHand;

	public const GameInput.Button buttonsExit = GameInput.Button.Exit;

	private const float acceleration = 20f;

	private const float sidewaysTorque = 45f;

	private const float stabilizeForce = 15f;

	private const float controllTimeDelay = 0.25f;

	[AssertLocalization(1)]
	private const string cameraInfoPingLabel = "MapRoomCameraInfo";

	public static List<MapRoomCamera> cameras = new List<MapRoomCamera>();

	private CommandBuffer cmdBuffer;

	public static event Action<MapRoomCamera> onMapRoomCameraChanged;

	public static event Action onMapRoomCameraExited;

	private IEnumerator Start()
	{
		if (version == 1)
		{
			version = 2;
			if (energyMixin != null)
			{
				yield return energyMixin.SpawnDefaultAsync(1f, DiscardTaskResult<bool>.Instance);
			}
		}
		cameras.Add(this);
		if (cameraNumber == 0)
		{
			cameraNumber = cameras.Count;
		}
		SetDocked(dockingPoint);
		screenEffectModel.GetComponent<Renderer>().materials[0].SetColor(ShaderPropertyID._Color, gradientInner.Evaluate(0f));
		screenEffectModel.GetComponent<Renderer>().materials[1].SetColor(ShaderPropertyID._Color, gradientOuter.Evaluate(0f));
		pickupAble.pickedUpEvent.AddHandler(base.gameObject, OnPickedUp);
		lightsParent.SetActive(lightState);
		Constructable component = GetComponent<Constructable>();
		if ((bool)component)
		{
			UnityEngine.Object.Destroy(component);
		}
		UpdatePingLabel();
	}

	public bool IsReady()
	{
		if (readyForControl)
		{
			return !justStartedControl;
		}
		return false;
	}

	public int GetCameraNumber()
	{
		return cameraNumber;
	}

	public bool CanBeControlled(MapRoomScreen byScreen = null)
	{
		if ((energyMixin.charge > 0f || !GameModeUtils.RequiresPower()) && liveMixin.IsAlive() && !pickupAble.attached)
		{
			return base.isActiveAndEnabled;
		}
		return false;
	}

	public float GetScreenDistance(MapRoomScreen fromScreen = null)
	{
		fromScreen = ((fromScreen != null) ? fromScreen : screen);
		if (!(fromScreen != null))
		{
			return 0f;
		}
		return (fromScreen.transform.position - base.transform.position).magnitude;
	}

	public float GetDepth()
	{
		return Mathf.Abs(Mathf.Min(0f, base.transform.position.y));
	}

	public void OnKill()
	{
		if (active && routine == null)
		{
			FreeCamera();
			ExitLockedMode();
			ClearRenderTexture(mapRoomRenderTexture);
		}
	}

	private void OnDisable()
	{
		if (active)
		{
			FreeCamera();
			ExitLockedMode();
		}
	}

	private void OnDestroy()
	{
		if (active)
		{
			FreeCamera();
			ExitLockedMode();
			ClearRenderTexture(mapRoomRenderTexture);
		}
		cameras.Remove(this);
	}

	public void ControlCamera(MapRoomScreen screen)
	{
		controllStartTime = Time.time;
		active = true;
		Player.main.EnterLockedMode(null);
		rigidBody.velocity = Vector3.zero;
		rigidBody.angularVelocity = Vector3.zero;
		MainCameraControl.main.enabled = false;
		this.screen = screen;
		uGUI_CameraDrone.main.SetCamera(this);
		uGUI_CameraDrone.main.SetScreen(screen);
		screenEffectModel.SetActive(value: true);
		droneIdle.Play();
		readyForControl = false;
		connectingSound.Play();
		Player.main.SetHeadVisible(visible: true);
		justStartedControl = true;
		VRUtil.Recenter();
		if (MapRoomCamera.onMapRoomCameraChanged != null)
		{
			MapRoomCamera.onMapRoomCameraChanged(this);
		}
	}

	public void FreeCamera(bool resetPlayerPosition = true)
	{
		rigidBody.velocity = Vector3.zero;
		engineSound.Stop();
		screenEffectModel.SetActive(value: false);
		droneIdle.Stop();
		connectingSound.Stop();
	}

	public void ExitLockedMode(bool resetPlayerPosition = true)
	{
		screen.OnCameraFree(this);
		screen = null;
		uGUI_CameraDrone.main.SetCamera(null);
		uGUI_CameraDrone.main.SetScreen(null);
		Player.main.ExitLockedMode(respawn: false, findNewPosition: false);
		active = false;
		if (resetPlayerPosition)
		{
			SNCameraRoot.main.transform.localPosition = Vector3.zero;
			SNCameraRoot.main.transform.localRotation = Quaternion.identity;
		}
		MainCameraControl.main.enabled = true;
		Player.main.SetHeadVisible(visible: false);
		if (resetPlayerPosition && MapRoomCamera.onMapRoomCameraExited != null)
		{
			MapRoomCamera.onMapRoomCameraExited();
		}
	}

	private IEnumerator CaptureScreenAndExitLockedMode()
	{
		Camera camera = MainCamera.camera;
		if (cmdBuffer == null)
		{
			cmdBuffer = new CommandBuffer();
			cmdBuffer.name = "ControlRoomCapture";
		}
		MathExtensions.RectFit(camera.pixelWidth, camera.pixelHeight, mapRoomRenderTexture.width, mapRoomRenderTexture.height, RectScaleMode.Envelope, out var scale, out var offset);
		cmdBuffer.Clear();
		cmdBuffer.Blit(BuiltinRenderTextureType.CameraTarget, mapRoomRenderTexture, scale, offset);
		MainCamera.camera.AddCommandBuffer(CameraEvent.AfterImageEffects, cmdBuffer);
		yield return new WaitForEndOfFrame();
		MainCamera.camera.RemoveCommandBuffer(CameraEvent.AfterImageEffects, cmdBuffer);
		ExitLockedMode();
		routine = null;
	}

	public static void ClearRenderTexture(RenderTexture renderTexture)
	{
		if (!(renderTexture == null))
		{
			RenderTexture renderTexture2 = RenderTexture.active;
			RenderTexture.active = renderTexture;
			GL.Clear(clearDepth: true, clearColor: true, Color.black);
			RenderTexture.active = renderTexture2;
		}
	}

	private bool IsControlled()
	{
		if (active)
		{
			return controllStartTime + 0.25f <= Time.time;
		}
		return false;
	}

	public void SetDocked(MapRoomCameraDocking dockingPoint)
	{
		this.dockingPoint = dockingPoint;
		if (!pickupAble.attached)
		{
			rigidBody.isKinematic = dockingPoint != null;
		}
	}

	public void OnShinyPickUp(GameObject byObject)
	{
		if ((bool)dockingPoint)
		{
			dockingPoint.UndockCamera();
		}
	}

	public void OnPickedUp(Pickupable p)
	{
		if ((bool)dockingPoint)
		{
			dockingPoint.UndockCamera();
		}
		lightsParent.SetActive(value: false);
	}

	private void UpdateEnergyRecharge()
	{
		bool flag = false;
		float charge = energyMixin.charge;
		float capacity = energyMixin.capacity;
		if (dockingPoint != null && charge < capacity)
		{
			float amount = Mathf.Min(capacity - charge, capacity * 0.1f);
			PowerRelay componentInParent = dockingPoint.GetComponentInParent<PowerRelay>();
			if (componentInParent == null)
			{
				Debug.LogError("camera drone is docked but can't access PowerRelay component");
			}
			float amountConsumed = 0f;
			componentInParent.ConsumeEnergy(amount, out amountConsumed);
			if (!GameModeUtils.RequiresPower() || amountConsumed > 0f)
			{
				energyMixin.AddEnergy(amountConsumed);
				flag = true;
			}
		}
		if (flag)
		{
			chargingSound.Play();
		}
		else
		{
			chargingSound.Stop();
		}
	}

	private void Update()
	{
		UpdateEnergyRecharge();
	}

	public void HandleInput()
	{
		if (!IsControlled() || !base.transform.gameObject.activeInHierarchy)
		{
			return;
		}
		if (GameInput.GetButtonDown(GameInput.Button.AutoMove))
		{
			GameInput.AutoMove = !GameInput.AutoMove;
		}
		if (!IsReady() && LargeWorldStreamer.main.IsWorldSettled())
		{
			readyForControl = true;
			connectingSound.Stop();
			Utils.PlayFMODAsset(connectedSound, base.transform);
		}
		if (CanBeControlled() && readyForControl)
		{
			Vector2 lookDelta = GameInput.GetLookDelta();
			rigidBody.AddTorque(base.transform.up * lookDelta.x * 45f * 0.0015f, ForceMode.VelocityChange);
			rigidBody.AddTorque(base.transform.right * (0f - lookDelta.y) * 45f * 0.0015f, ForceMode.VelocityChange);
			wishDir = GameInput.GetMoveDirection();
			wishDir.Normalize();
			if (dockingPoint != null && wishDir != Vector3.zero)
			{
				dockingPoint.UndockCamera();
			}
		}
		else
		{
			wishDir = Vector3.zero;
		}
		if (GameInput.GetButtonDown(GameInput.Button.Exit))
		{
			if (routine == null)
			{
				FreeCamera();
				routine = StartCoroutine(CaptureScreenAndExitLockedMode());
			}
		}
		else if (GameInput.GetButtonDown(GameInput.Button.CycleNext))
		{
			screen.CycleCamera();
		}
		else if (GameInput.GetButtonDown(GameInput.Button.CyclePrev))
		{
			screen.CycleCamera(-1);
		}
		else if (GameInput.GetButtonDown(GameInput.Button.RightHand))
		{
			bool flag = !lightsParent.activeInHierarchy;
			lightsParent.SetActive(flag);
			if (flag)
			{
				FMODUWE.PlayOneShot(lightOnSound, base.transform.position);
			}
			else
			{
				FMODUWE.PlayOneShot(lightOffSound, base.transform.position);
			}
		}
		if (Player.main != null && Player.main.liveMixin != null && !Player.main.liveMixin.IsAlive())
		{
			FreeCamera();
			ExitLockedMode();
		}
		float magnitude = rigidBody.velocity.magnitude;
		float time = Mathf.Clamp(base.transform.InverseTransformDirection(rigidBody.velocity).z / 15f, 0f, 1f);
		if (magnitude > 2f)
		{
			engineSound.Play();
			energyMixin.ConsumeEnergy(Time.deltaTime * 0.06666f);
		}
		else
		{
			engineSound.Stop();
		}
		screenEffectModel.GetComponent<Renderer>().materials[0].SetColor(ShaderPropertyID._Color, gradientInner.Evaluate(time));
		screenEffectModel.GetComponent<Renderer>().materials[1].SetColor(ShaderPropertyID._Color, gradientOuter.Evaluate(time));
		if (justStartedControl)
		{
			justStartedControl = false;
			return;
		}
		SNCameraRoot.main.transform.position = base.transform.position;
		SNCameraRoot.main.transform.rotation = base.transform.rotation;
	}

	private void StabilizeRoll()
	{
		float num = Mathf.Abs(base.transform.eulerAngles.z - 180f);
		if (num <= 178f)
		{
			float num2 = Mathf.Clamp(1f - num / 180f, 0f, 0.5f) * 15f;
			rigidBody.AddTorque(base.transform.forward * num2 * Time.fixedDeltaTime * Mathf.Sign(base.transform.eulerAngles.z - 180f), ForceMode.VelocityChange);
		}
	}

	private void FixedUpdate()
	{
		if (IsControlled() && base.transform.position.y < worldForces.waterDepth)
		{
			rigidBody.AddForce(base.transform.rotation * (20f * wishDir), ForceMode.Acceleration);
			StabilizeRoll();
		}
	}

	public static void GetCamerasInRange(Vector3 position, float range, ICollection<MapRoomCamera> outlist)
	{
		float num = range * range;
		for (int i = 0; i < cameras.Count; i++)
		{
			MapRoomCamera mapRoomCamera = cameras[i];
			if ((mapRoomCamera.transform.position - position).sqrMagnitude <= num)
			{
				outlist.Add(mapRoomCamera);
			}
		}
	}

	private void UpdatePingLabel()
	{
		if (pingInstance != null)
		{
			pingInstance.SetLabel(Language.main.GetFormat("MapRoomCameraInfo", GetCameraNumber()));
		}
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		lightState = lightsParent.activeInHierarchy;
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		UpdatePingLabel();
		lightsParent.SetActive(lightState);
	}
}
