using System;
using UWE;
using UnityEngine;
using UnityEngine.XR;

[AddComponentMenu("Camera-Control/Mouse Look")]
public class MainCameraControl : MonoBehaviour
{
	public enum ShakeMode
	{
		Linear = 0,
		Cos = 1,
		Sqrt = 2,
		Quadratic = 3,
		BuildUp = 4
	}

	public float camPDAZOffset = 0.5f;

	public float camPDAZStart;

	public static MainCameraControl main;

	public float stepAmount;

	public float minimumX = -360f;

	public float maximumX = 360f;

	public float minimumY = -80f;

	public float maximumY = 80f;

	public bool mouseLookEnabled = true;

	public float rotationY;

	public float rotationX;

	public float camRotationX;

	public float camRotationY;

	public float skin = 0.3f;

	public Transform viewModel;

	public Vector3 cameraAngleMotion;

	public float maxViewModelRotation = 10f;

	public float maxViewModelMovement = 5f;

	public float cameraTiltMod = 0.3f;

	public Transform cameraOffsetTransform;

	public Transform cameraUPTransform;

	public bool _cinematicMode;

	public bool lookAroundMode;

	private PlayerController playerController;

	private Vector3 viewModelStartRotation;

	private Vector3 cameraPos;

	private float strafeTilt;

	private float viewModelLockedYaw;

	private bool wasInLockedMode;

	private UnderWaterTracker underWaterTracker;

	private float swimCameraAnimation;

	private bool wasInLookAroundMode;

	private float shakeAmount;

	private float camShake;

	private float remainingShakeTime;

	private ShakeMode currentShakeMode;

	private float totalShaketime;

	private float currentShakeFrequency = 1f;

	private float smoothedSpeed;

	private float impactBob;

	private float impactForce;

	private const ManagedUpdate.Queue updateQueue = ManagedUpdate.Queue.UpdateCameraTransform;

	public bool cinematicMode
	{
		get
		{
			return _cinematicMode;
		}
		set
		{
			if (value == _cinematicMode)
			{
				return;
			}
			Player componentInParent = GetComponentInParent<Player>();
			if (value)
			{
				componentInParent.transform.localEulerAngles = new Vector3(0f - rotationY, rotationX, 0f);
				base.transform.localEulerAngles = Vector3.zero;
				cameraUPTransform.localEulerAngles = Vector3.zero;
				rotationX = 0f;
				rotationY = 0f;
			}
			else
			{
				Vector3 localEulerAngles = componentInParent.transform.localEulerAngles;
				rotationX = localEulerAngles.y;
				rotationY = 0f - localEulerAngles.x;
				if (rotationY > 180f)
				{
					rotationY -= 360f;
				}
				if (rotationY < -180f)
				{
					rotationY += 360f;
				}
				rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
				localEulerAngles.x += rotationY;
				localEulerAngles.y = 0f;
				componentInParent.transform.localEulerAngles = localEulerAngles;
				cameraUPTransform.localEulerAngles = new Vector3(Mathf.Min(0f, 0f - rotationY), 0f, 0f);
				base.transform.localEulerAngles = new Vector3(Mathf.Max(0f, 0f - rotationY), rotationX, 0f);
				viewModel.transform.localEulerAngles = new Vector3(0f, rotationX, 0f);
			}
			_cinematicMode = value;
		}
	}

	public float GetImpactBob()
	{
		return impactBob;
	}

	public float GetCameraPitch()
	{
		float degs = base.transform.localEulerAngles.x + cameraUPTransform.localEulerAngles.x + cameraOffsetTransform.localEulerAngles.x;
		UWE.Utils.MakeAngleInCCWBounds(ref degs, -180f, 180f);
		return 0f - degs;
	}

	public void ShakeCamera(float intensity, float duration = -1f, ShakeMode mode = ShakeMode.Linear, float shakeFrequency = 1f)
	{
		shakeAmount = Mathf.Clamp(intensity, 0f, 5f);
		remainingShakeTime = ((duration > -1f) ? duration : (shakeAmount * 0.5f));
		totalShaketime = remainingShakeTime;
		currentShakeFrequency = ((shakeFrequency == 0f) ? 1f : shakeFrequency);
		currentShakeMode = mode;
	}

	private void UpdateCamShake()
	{
		if (XRSettings.enabled)
		{
			camShake = 0f;
			return;
		}
		remainingShakeTime = UWE.Utils.Slerp(remainingShakeTime, 0f, Time.deltaTime);
		float num = ((totalShaketime == 0f) ? 0f : (remainingShakeTime / totalShaketime));
		switch (currentShakeMode)
		{
		case ShakeMode.Cos:
			num = Mathf.Cos((float)System.Math.PI + num * ((float)System.Math.PI / 2f)) + 1f;
			break;
		case ShakeMode.Sqrt:
			num = Mathf.Sqrt(num);
			break;
		case ShakeMode.Quadratic:
			num *= num;
			break;
		case ShakeMode.BuildUp:
			num = 1f - Mathf.Abs(num - 0.5f) / 0.5f;
			break;
		}
		if (remainingShakeTime > 0f)
		{
			camShake = shakeAmount * num * Mathf.Cos(Time.time * 35f * currentShakeFrequency);
		}
		else
		{
			camShake = 0f;
		}
	}

	private void Awake()
	{
		main = this;
		camPDAZStart = cameraOffsetTransform.localPosition.z;
		if ((bool)GetComponent<Rigidbody>())
		{
			GetComponent<Rigidbody>().freezeRotation = true;
		}
		playerController = base.gameObject.FindAncestor<PlayerController>();
		viewModelStartRotation = viewModel.localRotation.eulerAngles;
		cameraAngleMotion = viewModelStartRotation;
		cameraPos = base.transform.position;
		underWaterTracker = Utils.FindAncestorWithComponent<UnderWaterTracker>(base.gameObject);
		DevConsole.RegisterConsoleCommand(this, "camerabobbing");
		DevConsole.RegisterConsoleCommand(this, "cambob");
		DevConsole.RegisterConsoleCommand(this, "camshake");
	}

	private bool GetCameraBob()
	{
		if (Player.main.GetMode() == Player.Mode.Normal && swimCameraAnimation > 0f)
		{
			if (!XRSettings.enabled)
			{
				return MiscSettings.cameraBobbing;
			}
			return false;
		}
		return false;
	}

	private void OnConsoleCommand_camerabobbing()
	{
		MiscSettings.cameraBobbing = !MiscSettings.cameraBobbing;
	}

	private void OnConsoleCommand_cambob()
	{
		OnConsoleCommand_camerabobbing();
	}

	private void OnConsoleCommand_camshake()
	{
		ShakeCamera(3f);
	}

	public void LookAt(Vector3 point)
	{
		Player.main.transform.LookAt(point);
		rotationX = 0f;
		rotationY = 0f;
	}

	private void UpdateStrafeTilt()
	{
		bool num = Player.main.GetMode() == Player.Mode.Normal && Player.main.IsUnderwater() && playerController.inputEnabled && !FPSInputModule.current.lockMovement && !XRSettings.enabled;
		float x = GameInput.GetMoveDirection().x;
		float num2 = (num ? x : 0f);
		strafeTilt -= Time.deltaTime * num2 * 12f;
		strafeTilt = Mathf.Clamp(strafeTilt, -10f, 10f);
		strafeTilt = UWE.Utils.Slerp(strafeTilt, 0f, Time.unscaledDeltaTime * 4f);
	}

	private void OnEnable()
	{
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.UpdateCameraTransform, OnUpdate);
	}

	private void OnDisable()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.UpdateCameraTransform, OnUpdate);
	}

	private void OnUpdate()
	{
		float deltaTime = Time.deltaTime;
		if (underWaterTracker.isUnderWater)
		{
			swimCameraAnimation = Mathf.Clamp01(swimCameraAnimation + deltaTime);
		}
		else
		{
			swimCameraAnimation = Mathf.Clamp01(swimCameraAnimation - deltaTime);
		}
		_ = minimumY;
		_ = maximumY;
		Vector3 velocity = playerController.velocity;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool inExosuit = Player.main.inExosuit;
		bool flag4 = uGUI_BuilderMenu.IsOpen();
		if (Player.main != null)
		{
			flag = Player.main.GetPDA().isInUse;
			flag3 = Player.main.motorMode == Player.MotorMode.Vehicle;
			flag2 = flag || flag3 || cinematicMode;
			if (XRSettings.enabled && VROptions.gazeBasedCursor)
			{
				flag2 = flag2 || flag4;
			}
		}
		if (flag2 != wasInLockedMode || lookAroundMode != wasInLookAroundMode)
		{
			camRotationX = 0f;
			camRotationY = 0f;
			wasInLockedMode = flag2;
			wasInLookAroundMode = lookAroundMode;
		}
		bool flag5 = (!cinematicMode || (lookAroundMode && !flag)) && mouseLookEnabled && (flag3 || AvatarInputHandler.main == null || AvatarInputHandler.main.IsEnabled() || Builder.isPlacing);
		if (flag3 && !XRSettings.enabled && !inExosuit)
		{
			flag5 = false;
		}
		Transform transform = base.transform;
		float num = ((flag || lookAroundMode || Player.main.GetMode() == Player.Mode.LockedPiloting) ? 1 : (-1));
		if (!flag2 || (cinematicMode && !lookAroundMode))
		{
			cameraOffsetTransform.localEulerAngles = UWE.Utils.LerpEuler(cameraOffsetTransform.localEulerAngles, Vector3.zero, deltaTime * 5f);
		}
		else
		{
			transform = cameraOffsetTransform;
			rotationY = Mathf.LerpAngle(rotationY, 0f, PDA.deltaTime * 15f);
			base.transform.localEulerAngles = new Vector3(Mathf.LerpAngle(base.transform.localEulerAngles.x, 0f, PDA.deltaTime * 15f), base.transform.localEulerAngles.y, 0f);
			cameraUPTransform.localEulerAngles = UWE.Utils.LerpEuler(cameraUPTransform.localEulerAngles, Vector3.zero, PDA.deltaTime * 15f);
		}
		if (!XRSettings.enabled)
		{
			Vector3 localPosition = cameraOffsetTransform.localPosition;
			localPosition.z = Mathf.Clamp(localPosition.z + PDA.deltaTime * num * 0.25f, 0f + camPDAZStart, camPDAZOffset + camPDAZStart);
			cameraOffsetTransform.localPosition = localPosition;
		}
		Vector2 vector = Vector2.zero;
		if (flag5 && FPSInputModule.current.lastGroup == null)
		{
			vector = GameInput.GetLookDelta();
			if (XRSettings.enabled && VROptions.disableInputPitch)
			{
				vector.y = 0f;
			}
			if (inExosuit)
			{
				vector.x = 0f;
			}
			vector *= Player.main.mesmerizedSpeedMultiplier;
		}
		UpdateCamShake();
		if (cinematicMode && !lookAroundMode)
		{
			camRotationX = Mathf.LerpAngle(camRotationX, 0f, deltaTime * 2f);
			camRotationY = Mathf.LerpAngle(camRotationY, 0f, deltaTime * 2f);
			base.transform.localEulerAngles = new Vector3(0f - camRotationY + camShake, camRotationX, 0f);
		}
		else if (flag2)
		{
			if (!XRSettings.enabled)
			{
				bool flag6 = !lookAroundMode || flag;
				bool num2 = !lookAroundMode || flag;
				Vehicle vehicle = Player.main.GetVehicle();
				if (vehicle != null)
				{
					flag6 = vehicle.controlSheme != Vehicle.ControlSheme.Mech || flag;
				}
				camRotationX += vector.x;
				camRotationY += vector.y;
				camRotationX = Mathf.Clamp(camRotationX, -60f, 60f);
				camRotationY = Mathf.Clamp(camRotationY, -60f, 60f);
				if (num2)
				{
					camRotationX = Mathf.LerpAngle(camRotationX, 0f, PDA.deltaTime * 10f);
				}
				if (flag6)
				{
					camRotationY = Mathf.LerpAngle(camRotationY, 0f, PDA.deltaTime * 10f);
				}
				cameraOffsetTransform.localEulerAngles = new Vector3(0f - camRotationY, camRotationX + camShake, 0f);
			}
		}
		else
		{
			rotationX += vector.x;
			rotationY += vector.y;
			rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
			cameraUPTransform.localEulerAngles = new Vector3(Mathf.Min(0f, 0f - rotationY + camShake), 0f, 0f);
			transform.localEulerAngles = new Vector3(Mathf.Max(0f, 0f - rotationY + camShake), rotationX, 0f);
		}
		UpdateStrafeTilt();
		Vector3 localEulerAngles = base.transform.localEulerAngles + new Vector3(0f, 0f, cameraAngleMotion.y * cameraTiltMod + strafeTilt + camShake * 0.5f);
		float num3 = 0f - skin;
		if (!flag2 && GetCameraBob())
		{
			float to = Mathf.Min(1f, velocity.magnitude / 5f);
			smoothedSpeed = UWE.Utils.Slerp(smoothedSpeed, to, deltaTime);
			num3 += (Mathf.Sin(Time.time * 6f) - 1f) * (0.02f + smoothedSpeed * 0.15f) * swimCameraAnimation;
		}
		if (impactForce > 0f)
		{
			impactBob = Mathf.Min(0.9f, impactBob + impactForce * deltaTime);
			impactForce -= Mathf.Max(1f, impactForce) * deltaTime * 5f;
		}
		num3 -= impactBob;
		num3 -= stepAmount;
		if (impactBob > 0f)
		{
			impactBob = Mathf.Max(0f, impactBob - Mathf.Pow(impactBob, 0.5f) * Time.deltaTime * 3f);
		}
		stepAmount = Mathf.Lerp(stepAmount, 0f, deltaTime * Mathf.Abs(stepAmount));
		base.transform.localPosition = new Vector3(0f, num3, 0f);
		base.transform.localEulerAngles = localEulerAngles;
		if (Player.main.motorMode == Player.MotorMode.Vehicle)
		{
			base.transform.localEulerAngles = Vector3.zero;
		}
		Vector3 localEulerAngles2 = new Vector3(0f, base.transform.localEulerAngles.y, 0f);
		Vector3 localPosition2 = base.transform.localPosition;
		if (XRSettings.enabled)
		{
			if (flag2 && !flag3)
			{
				localEulerAngles2.y = viewModelLockedYaw;
			}
			else
			{
				localEulerAngles2.y = 0f;
			}
			if (!flag3 && !cinematicMode)
			{
				if (!flag2)
				{
					Quaternion rotation = playerController.forwardReference.rotation;
					localEulerAngles2.y = (base.gameObject.transform.parent.rotation.GetInverse() * rotation).eulerAngles.y;
				}
				localPosition2 = base.gameObject.transform.parent.worldToLocalMatrix.MultiplyPoint(playerController.forwardReference.position);
			}
		}
		viewModel.transform.localEulerAngles = localEulerAngles2;
		viewModel.transform.localPosition = localPosition2;
	}

	public void OnLand(Vector3 velocity)
	{
		impactForce = Mathf.Clamp(0f - velocity.y, 0f, 15f);
	}

	public void SaveLockedVRViewModelAngle()
	{
		viewModelLockedYaw = viewModel.transform.localEulerAngles.y;
	}

	public void ResetLockedVRViewModelAngle()
	{
		viewModelLockedYaw = 0f;
	}

	public void SetEnabled(bool val)
	{
		mouseLookEnabled = val;
	}

	public void ResetCamera()
	{
		cameraOffsetTransform.localRotation = Quaternion.identity;
		cameraUPTransform.localRotation = Quaternion.identity;
		base.transform.localRotation = Quaternion.identity;
		rotationY = 0f;
		rotationX = 0f;
		camRotationX = 0f;
		camRotationY = 0f;
	}
}
