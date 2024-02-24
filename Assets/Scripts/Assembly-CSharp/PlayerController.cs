using System;
using UWE;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public enum ControllerSize
	{
		Swim = 0,
		Stand = 1
	}

	public delegate bool TestObjectDelegate(GameObject gameObject);

	[AssertNotNull]
	public Player player;

	[AssertNotNull]
	public Rigidbody useRigidbody;

	[SerializeField]
	[AssertNotNull]
	private MainCameraControl cameraControl;

	public float standheight = 1.5f;

	public float swimheight = 0.5f;

	[Tooltip("collision extent above the camera, suggested value -0.3")]
	public float cameraOffset;

	public Vector3 velocity = Vector3.zero;

	public PlayerMotor underWaterController;

	public PlayerMotor groundController;

	public PlayerMotor activeController;

	public float controllerRadius = 0.3f;

	public float defaultSwimDrag = 2.5f;

	public bool inputEnabled = true;

	private float currentControllerHeight = 1.5f;

	private float desiredControllerHeight = 1.5f;

	private bool underWater;

	private bool inVehicle;

	public float swimForwardMaxSpeed = 6.64f;

	public float swimBackwardMaxSpeed = 6.64f;

	public float swimStrafeMaxSpeed = 6.64f;

	public float swimVerticalMaxSpeed = 6.6f;

	public float swimWaterAcceleration = 20f;

	public float seaglideForwardMaxSpeed = 25f;

	public float seaglideBackwardMaxSpeed = 6.35f;

	public float seaglideStrafeMaxSpeed = 6.35f;

	public float seaglideVerticalMaxSpeed = 6.34f;

	public float seaglideWaterAcceleration = 36.56f;

	public float seaglideSwimDrag = 2.5f;

	public float walkRunForwardMaxSpeed = 3.5f;

	public float walkRunBackwardMaxSpeed = 5f;

	public float walkRunStrafeMaxSpeed = 5f;

	[Tooltip("Default value for MainCameraControl.minimumY.")]
	[SerializeField]
	private float defaultCameraMinimumY = -87f;

	[Tooltip("Value for MainCameraControl.minimumY when motor mode is Walk or Run.")]
	[SerializeField]
	private float walkRunCameraMinimumY = -87f;

	public Transform forwardReference
	{
		get
		{
			return MainCamera.camera.transform;
		}
		private set
		{
		}
	}

	private event Action<Collider> onTriggerExitRecovery;

	private void Start()
	{
		underWaterController.SetEnabled(enabled: false);
		groundController.SetEnabled(enabled: false);
		activeController = (player.IsUnderwaterForSwimming() ? underWaterController : groundController);
		groundController.SetControllerRadius(controllerRadius);
		underWaterController.SetControllerRadius(controllerRadius);
		ForceControllerSize();
	}

	public bool IsSprinting()
	{
		if (base.enabled && activeController.IsSprinting())
		{
			return GameInput.GetVector2(GameInput.Button.Move).sqrMagnitude > 0.0001f;
		}
		return false;
	}

	public void SetEnabled(bool enabled)
	{
		velocity = Vector3.zero;
		if (activeController != null)
		{
			activeController.SetVelocity(velocity);
		}
		if (!enabled)
		{
			underWaterController.SetEnabled(enabled: false);
			groundController.SetEnabled(enabled: false);
		}
		else if (activeController != null)
		{
			activeController.SetEnabled(enabled: true);
		}
		base.enabled = enabled;
		if (!enabled && activeController != null)
		{
			this.onTriggerExitRecovery?.Invoke(activeController.GetCollider());
		}
	}

	public Vector3 ForceControllerSize()
	{
		Vector3 position = base.transform.position;
		player.UpdateIsUnderwater();
		HandleUnderWaterState();
		bool flag = player.IsUnderwaterForSwimming();
		bool flag2 = player.GetVehicle();
		desiredControllerHeight = ((flag || flag2) ? swimheight : standheight);
		desiredControllerHeight -= cameraOffset;
		currentControllerHeight = desiredControllerHeight;
		groundController.SetControllerHeight(currentControllerHeight, cameraOffset);
		underWaterController.SetControllerHeight(currentControllerHeight, cameraOffset);
		return base.transform.position - position;
	}

	private void HandleControllerStateAfterDeserialization()
	{
		HandleUnderWaterState();
		HandleControllerState();
	}

	private void HandleControllerState()
	{
		groundController.SetEnabled(enabled: false);
		underWaterController.SetEnabled(enabled: false);
		if (!inVehicle)
		{
			if (underWater)
			{
				activeController = (player.IsInSub() ? groundController : underWaterController);
				desiredControllerHeight = swimheight - cameraOffset;
				activeController.SetControllerHeight(currentControllerHeight, cameraOffset);
				activeController.SetEnabled(base.enabled);
			}
			else
			{
				activeController = groundController;
				desiredControllerHeight = standheight - cameraOffset;
				activeController.SetControllerHeight(currentControllerHeight, cameraOffset);
				activeController.SetEnabled(base.enabled);
			}
		}
	}

	private void HandleUnderWaterState()
	{
		bool flag = player.IsUnderwaterForSwimming();
		bool flag2 = player.GetVehicle();
		if (underWater != flag || inVehicle != flag2)
		{
			underWater = flag;
			inVehicle = flag2;
			HandleControllerState();
		}
		activeController.SetUnderWater(underWater);
	}

	public void SetMotorMode(Player.MotorMode newMotorMode)
	{
		float forwardMaxSpeed = swimForwardMaxSpeed;
		float backwardMaxSpeed = swimBackwardMaxSpeed;
		float strafeMaxSpeed = swimStrafeMaxSpeed;
		float verticalMaxSpeed = swimVerticalMaxSpeed;
		float waterAcceleration = swimWaterAcceleration;
		float underWaterGravity = 0f;
		float swimDrag = defaultSwimDrag;
		bool canSwim = true;
		float minimumY = defaultCameraMinimumY;
		switch (newMotorMode)
		{
		case Player.MotorMode.Seaglide:
			forwardMaxSpeed = seaglideForwardMaxSpeed;
			backwardMaxSpeed = seaglideBackwardMaxSpeed;
			strafeMaxSpeed = seaglideStrafeMaxSpeed;
			verticalMaxSpeed = seaglideVerticalMaxSpeed;
			waterAcceleration = seaglideWaterAcceleration;
			swimDrag = seaglideSwimDrag;
			break;
		case Player.MotorMode.Walk:
		case Player.MotorMode.Run:
			forwardMaxSpeed = walkRunForwardMaxSpeed;
			backwardMaxSpeed = walkRunBackwardMaxSpeed;
			strafeMaxSpeed = walkRunStrafeMaxSpeed;
			minimumY = walkRunCameraMinimumY;
			break;
		}
		underWaterController.forwardMaxSpeed = forwardMaxSpeed;
		underWaterController.backwardMaxSpeed = backwardMaxSpeed;
		underWaterController.strafeMaxSpeed = strafeMaxSpeed;
		underWaterController.verticalMaxSpeed = verticalMaxSpeed;
		underWaterController.waterAcceleration = waterAcceleration;
		underWaterController.underWaterGravity = underWaterGravity;
		underWaterController.swimDrag = swimDrag;
		underWaterController.canSwim = canSwim;
		groundController.forwardMaxSpeed = forwardMaxSpeed;
		groundController.backwardMaxSpeed = backwardMaxSpeed;
		groundController.strafeMaxSpeed = strafeMaxSpeed;
		groundController.verticalMaxSpeed = verticalMaxSpeed;
		groundController.underWaterGravity = underWaterGravity;
		groundController.canSwim = canSwim;
		cameraControl.minimumY = minimumY;
		HandleControllerState();
	}

	public bool TestHasSpace(Vector3 position)
	{
		RaycastHit hitInfo;
		return !Physics.CapsuleCast(position, position, controllerRadius + 0.01f, Vector3.up, out hitInfo, currentControllerHeight, -524289, QueryTriggerInteraction.Ignore);
	}

	public void SetControllerSize(ControllerSize controllerSize)
	{
		switch (controllerSize)
		{
		case ControllerSize.Swim:
			currentControllerHeight = swimheight - cameraOffset;
			break;
		case ControllerSize.Stand:
			currentControllerHeight = standheight - cameraOffset;
			break;
		}
		SetControllerHeight(currentControllerHeight);
	}

	public void SetControllerHeight(float controllerHeight)
	{
		currentControllerHeight = controllerHeight;
		underWaterController.SetControllerHeight(controllerHeight, cameraOffset);
		groundController.SetControllerHeight(controllerHeight, cameraOffset);
	}

	public bool WayToPositionClear(Vector3 position, ControllerSize controllerSize, GameObject ignoreObj = null, bool ignoreLiving = false, Vector3? fromPosition = null)
	{
		float controllerHeight = 0f;
		switch (controllerSize)
		{
		case ControllerSize.Swim:
			controllerHeight = swimheight - cameraOffset;
			break;
		case ControllerSize.Stand:
			controllerHeight = standheight - cameraOffset;
			break;
		}
		RaycastHit hit;
		return !Trace(fromPosition.Value, position, out hit, ignoreObj, ignoreLiving, controllerHeight);
	}

	public bool WayToPositionClear(Vector3 toPosition, Vector3 fromPosition, ControllerSize controllerSize, TestObjectDelegate testObject)
	{
		float num = 0f;
		switch (controllerSize)
		{
		case ControllerSize.Swim:
			num = swimheight - cameraOffset;
			break;
		case ControllerSize.Stand:
			num = standheight - cameraOffset;
			break;
		}
		Vector3 point = fromPosition - Vector3.up * 0.5f * num;
		Vector3 point2 = fromPosition + Vector3.up * 0.5f * num;
		Vector3 value = toPosition - fromPosition;
		int num2 = UWE.Utils.CapsuleCastIntoSharedBuffer(point, point2, controllerRadius + 0.01f, Vector3.Normalize(value), value.magnitude, -524289, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < num2; i++)
		{
			RaycastHit raycastHit = UWE.Utils.sharedHitBuffer[i];
			GameObject gameObject = raycastHit.collider.gameObject;
			if (!(gameObject == base.gameObject) && !testObject(gameObject))
			{
				return false;
			}
		}
		return true;
	}

	public bool WayToPositionClear(Vector3 position, GameObject ignoreObj = null, bool ignoreLiving = false, Vector3? fromPosition = null)
	{
		if (!fromPosition.HasValue)
		{
			fromPosition = base.transform.position;
		}
		RaycastHit hit;
		return !Trace(fromPosition.Value, position, out hit, ignoreObj, ignoreLiving);
	}

	public bool Trace(Vector3 from, Vector3 to, out RaycastHit hit, GameObject ignoreObj = null, bool ignoreLiving = false, float controllerHeight = 0f)
	{
		float num = ((controllerHeight > 0f) ? controllerHeight : currentControllerHeight);
		Vector3 point = from - Vector3.up * 0.5f * num;
		Vector3 point2 = from + Vector3.up * 0.5f * num;
		Vector3 value = to - from;
		hit = default(RaycastHit);
		int num2 = UWE.Utils.CapsuleCastIntoSharedBuffer(layermask: ~((1 << LayerID.Player) | (1 << LayerID.OnlyVehicle)), point1: point, point2: point2, radius: controllerRadius + 0.01f, direction: Vector3.Normalize(value), maxDistance: value.magnitude, queryTriggerInteraction: QueryTriggerInteraction.Ignore);
		for (int i = 0; i < num2; i++)
		{
			RaycastHit raycastHit = UWE.Utils.sharedHitBuffer[i];
			GameObject gameObject = raycastHit.collider.gameObject;
			if (!(gameObject == base.gameObject) && (!(ignoreObj != null) || (!UWE.Utils.IsAncestorOf(ignoreObj, gameObject) && !UWE.Utils.IsAncestorOf(gameObject, ignoreObj))) && (!ignoreLiving || !(gameObject.GetComponentInParent<Living>() != null)))
			{
				hit = raycastHit;
				return true;
			}
		}
		return false;
	}

	public void UpdateController()
	{
		Vector3 colliderPosition = activeController.GetColliderPosition();
		float num = Mathf.MoveTowards(currentControllerHeight, desiredControllerHeight, Time.deltaTime * 2f);
		float num2 = desiredControllerHeight - currentControllerHeight;
		bool flag = true;
		if (num2 > 0f)
		{
			Vector3 vector = base.transform.TransformPoint(colliderPosition) + new Vector3(0f, currentControllerHeight * 0.5f, 0f);
			flag = !Physics.CapsuleCast(vector, vector, controllerRadius + 0.01f, Vector3.up, out var _, num2, -524289);
		}
		if (flag)
		{
			currentControllerHeight = num;
		}
		underWaterController.SetControllerHeight(currentControllerHeight, cameraOffset);
		groundController.SetControllerHeight(currentControllerHeight, cameraOffset);
		velocity = activeController.UpdateMove();
	}

	private void FixedUpdate()
	{
		HandleUnderWaterState();
		if (!useRigidbody.isKinematic)
		{
			UpdateController();
		}
	}

	private void Update()
	{
		if (useRigidbody.isKinematic)
		{
			UpdateController();
		}
		if (activeController != groundController)
		{
			velocity = useRigidbody.velocity;
		}
	}

	public Vector3 GetPlayerCenterPosition()
	{
		Vector3 colliderPosition = activeController.GetColliderPosition();
		return base.transform.TransformPoint(colliderPosition);
	}

	public void IgnoreCollisions(Collider collider, bool ignore)
	{
		Physics.IgnoreCollision(groundController.GetCollider(), collider, ignore);
		Physics.IgnoreCollision(underWaterController.GetCollider(), collider, ignore);
	}

	public void AddOnTriggerExitRecovery(Action<Collider> onTriggerExit)
	{
		onTriggerExitRecovery -= onTriggerExit;
		onTriggerExitRecovery += onTriggerExit;
	}

	public void RemoveOnTriggerExitRecovery(Action<Collider> onTriggerExit)
	{
		onTriggerExitRecovery -= onTriggerExit;
	}
}
