using System;
using System.Collections;
using UWE;
using UnityEngine;
using UnityEngine.XR;

public class GroundMotor : PlayerMotor, IGroundMoveable
{
	[Serializable]
	public class CharacterMotorMovement
	{
		public float maxForwardSpeed = 10f;

		public float maxSidewaysSpeed = 10f;

		public float maxBackwardsSpeed = 10f;

		public AnimationCurve slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90f, 1f), new Keyframe(0f, 1f), new Keyframe(90f, 0f));

		public float maxFallSpeed = 20f;

		[NonSerialized]
		public CollisionFlags collisionFlags;

		[NonSerialized]
		public Vector3 velocity;

		[NonSerialized]
		public Vector3 frameVelocity = new Vector3(0f, 0f, 0f);

		[NonSerialized]
		public Vector3 hitPoint = new Vector3(0f, 0f, 0f);

		[NonSerialized]
		public Vector3 lastHitPoint = new Vector3(float.PositiveInfinity, 0f, 0f);
	}

	public enum MovementTransferOnJump
	{
		None = 0,
		InitTransfer = 1,
		PermaTransfer = 2,
		PermaLocked = 3
	}

	[Serializable]
	public class CharacterMotorJumping
	{
		public bool enabled = true;

		public float baseHeight = 1f;

		public float extraHeight = 4.1f;

		public float perpAmount;

		public float steepPerpAmount = 0.5f;

		[NonSerialized]
		public bool jumping;

		[NonSerialized]
		public bool holdingJumpButton;

		[NonSerialized]
		public float lastStartTime;

		[NonSerialized]
		public float lastButtonDownTime = -100f;

		[NonSerialized]
		public Vector3 jumpDir = new Vector3(0f, 1f, 0f);
	}

	[Serializable]
	public class CharacterMotorMovingPlatform
	{
		public bool enabled = true;

		public MovementTransferOnJump movementTransfer = MovementTransferOnJump.PermaTransfer;

		[NonSerialized]
		public Transform hitPlatform;

		[NonSerialized]
		public Transform activePlatform;

		[NonSerialized]
		public Vector3 activeLocalPoint;

		[NonSerialized]
		public Vector3 activeGlobalPoint;

		[NonSerialized]
		public Quaternion activeLocalRotation;

		[NonSerialized]
		public Quaternion activeGlobalRotation;

		[NonSerialized]
		public Matrix4x4 lastMatrix;

		[NonSerialized]
		public Vector3 platformVelocity;

		[NonSerialized]
		public bool newPlatform;
	}

	[Serializable]
	public class CharacterMotorSliding
	{
		public bool enabled = true;

		public float slidingSpeed = 15f;

		public float sidewaysControl = 1f;

		public float speedControl = 0.4f;

		[NonSerialized]
		public ICharacterMotorSlidingOverride slidingOverride;
	}

	[Serializable]
	public class CharacterMotorController
	{
		public float stepOffset = 0.3f;

		public float slopeLimit = 45f;
	}

	[Serializable]
	public class CharacterMotorFloating
	{
		public float gravity = 8f;

		public float airAcceleration = 10f;

		public float maxFallSpeed = 3f;

		public float jumpHeight = 2f;
	}

	public CharacterController controller;

	public CharacterMotorMovement movement = new CharacterMotorMovement();

	public CharacterMotorJumping jumping = new CharacterMotorJumping();

	public CharacterMotorMovingPlatform movingPlatform = new CharacterMotorMovingPlatform();

	public CharacterMotorSliding sliding = new CharacterMotorSliding();

	public CharacterMotorController controllerSetup = new CharacterMotorController();

	public CharacterMotorFloating floatingModeSetup = new CharacterMotorFloating();

	[NonSerialized]
	public bool allowMidAirJumping;

	[NonSerialized]
	private Vector3 groundNormal = Vector3.zero;

	public float minWindSpeedToAffectMovement = 15f;

	public float percentWindDampeningOnGround = 0.25f;

	public float percentWindDampeningInAir = 0.5f;

	public bool flyCheatEnabled;

	[NonSerialized]
	public bool floatingModeEnabled;

	private bool sprinting;

	private Vector3 colliderCenter;

	private Vector3 lastGroundNormal;

	private VFXSurfaceTypes groundSurfaceType;

	private Vector3 previousVelocity;

	private float lastCameraOffset;

	private void Awake()
	{
		controller = base.gameObject.AddComponent<CharacterController>();
		controller.enabled = false;
		controller.stepOffset = controllerSetup.stepOffset;
		controller.slopeLimit = controllerSetup.slopeLimit;
		DevConsole.RegisterConsoleCommand(this, "fly");
	}

	private void OnConsoleCommand_fly()
	{
		flyCheatEnabled = !flyCheatEnabled;
		ErrorMessage.AddDebug("fly cheat = " + flyCheatEnabled);
	}

	public override bool IsSprinting()
	{
		return sprinting;
	}

	public override Vector3 GetColliderPosition()
	{
		return controller.center;
	}

	public override void SetControllerRadius(float radius)
	{
		controller.radius = radius;
	}

	public override float GetControllerRadius()
	{
		return controller.radius;
	}

	public void MoveDown(float distance)
	{
		controller.Move(-Vector3.up * distance);
	}

	public override void SetControllerHeight(float height, float cameraOffset)
	{
		if (controller.height != height || lastCameraOffset != cameraOffset)
		{
			float num = height - controller.height;
			if (num > 0f)
			{
				base.transform.localPosition = base.transform.localPosition + Vector3.up * num;
			}
			controller.height = height;
			colliderCenter.y = (0f - controller.height) * 0.5f - cameraOffset;
			controller.center = colliderCenter;
			lastCameraOffset = cameraOffset;
		}
	}

	public override float GetControllerHeight()
	{
		return controller.height;
	}

	public override void SetEnabled(bool enabled)
	{
		if (controller != null)
		{
			controller.enabled = enabled;
			if (enabled)
			{
				movement.velocity = playerController.velocity;
				movingPlatform.activePlatform = null;
			}
		}
		jumpPressed = false;
		sprintPressed = false;
		base.enabled = enabled;
	}

	private void UpdateFunction()
	{
		float deltaTime = Time.deltaTime;
		sprinting = false;
		if (!canControl)
		{
			return;
		}
		previousVelocity = movement.velocity;
		Vector3 vector = default(Vector3);
		vector = movement.velocity;
		vector = ApplyInputVelocityChange(vector);
		vector = ApplyGravityAndJumping(vector);
		if ((bool)movingPlatform.activePlatform && !IsPlatformUsable())
		{
			movingPlatform.activePlatform = null;
		}
		Vector3 vector2 = default(Vector3);
		if (MoveWithPlatform())
		{
			vector2 = movingPlatform.activePlatform.TransformPoint(movingPlatform.activeLocalPoint) - movingPlatform.activeGlobalPoint;
			if (vector2 != Vector3.zero)
			{
				controller.Move(vector2);
			}
			Quaternion quaternion = movingPlatform.activePlatform.rotation * movingPlatform.activeLocalRotation;
			Quaternion quaternion2 = default(Quaternion);
			float y = (quaternion * Quaternion.Inverse(movingPlatform.activeGlobalRotation)).eulerAngles.y;
			if (y != 0f)
			{
				base.transform.Rotate(0f, y, 0f);
			}
		}
		Vector3 position = base.transform.position;
		Vector3 vector3 = default(Vector3);
		vector3 = vector * deltaTime;
		float num = 0f;
		if (!underWater)
		{
			num = Mathf.Max(controller.stepOffset, new Vector3(vector3.x, 0f, vector3.z).magnitude);
			if (grounded)
			{
				vector3 -= num * Vector3.up;
			}
		}
		movingPlatform.hitPlatform = null;
		sliding.slidingOverride = null;
		groundNormal = Vector3.zero;
		_ = base.transform.position;
		movement.collisionFlags = controller.Move(vector3);
		movement.lastHitPoint = movement.hitPoint;
		lastGroundNormal = groundNormal;
		if (movingPlatform.enabled && movingPlatform.activePlatform != movingPlatform.hitPlatform && movingPlatform.hitPlatform != null)
		{
			movingPlatform.activePlatform = movingPlatform.hitPlatform;
			movingPlatform.lastMatrix = movingPlatform.hitPlatform.localToWorldMatrix;
			movingPlatform.newPlatform = true;
		}
		Vector3 vector4 = new Vector3(vector.x, 0f, vector.z);
		Vector3 velocity = ((deltaTime > 0f) ? ((base.transform.position - position) / deltaTime) : vector);
		if (velocity.sqrMagnitude <= 0.2f)
		{
			velocity = vector;
		}
		if (velocity.y > 0f || movement.collisionFlags == CollisionFlags.None)
		{
			velocity.y = vector.y;
		}
		movement.velocity = velocity;
		Vector3 lhs = new Vector3(movement.velocity.x, 0f, movement.velocity.z);
		if (vector4 == Vector3.zero)
		{
			movement.velocity = new Vector3(0f, movement.velocity.y, 0f);
		}
		else
		{
			float value = Vector3.Dot(lhs, vector4) / vector4.sqrMagnitude;
			movement.velocity = vector4 * Mathf.Clamp01(value) + movement.velocity.y * Vector3.up;
		}
		if ((double)movement.velocity.y < (double)vector.y - 0.001)
		{
			if (movement.velocity.y < 0f)
			{
				movement.velocity.y = vector.y;
			}
			else
			{
				jumping.holdingJumpButton = false;
			}
		}
		if (grounded && !IsGroundedTest())
		{
			grounded = false;
			if (movingPlatform.enabled && (movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer || movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer))
			{
				movement.frameVelocity = movingPlatform.platformVelocity;
				movement.velocity += movingPlatform.platformVelocity;
			}
			SendMessage("OnFall", SendMessageOptions.DontRequireReceiver);
			base.transform.position += num * Vector3.up;
		}
		else if (!grounded && IsGroundedTest())
		{
			grounded = true;
			jumping.jumping = false;
			SubtractNewPlatformVelocity();
			MovementCollisionData movementCollisionData = default(MovementCollisionData);
			movementCollisionData.impactVelocity = previousVelocity;
			movementCollisionData.surfaceType = groundSurfaceType;
			SendMessage("OnLand", movementCollisionData, SendMessageOptions.DontRequireReceiver);
		}
		if (MoveWithPlatform())
		{
			movingPlatform.activeGlobalPoint = base.transform.position + Vector3.up * (controller.center.y - controller.height * 0.5f + controller.radius);
			movingPlatform.activeLocalPoint = movingPlatform.activePlatform.InverseTransformPoint(movingPlatform.activeGlobalPoint);
			movingPlatform.activeGlobalRotation = base.transform.rotation;
			movingPlatform.activeLocalRotation = Quaternion.Inverse(movingPlatform.activePlatform.rotation) * movingPlatform.activeGlobalRotation;
		}
	}

	private bool IsPlatformUsable()
	{
		if (movingPlatform.activePlatform != null && movingPlatform.activePlatform.gameObject.activeInHierarchy && !movingPlatform.activePlatform.IsChildOf(Player.mainObject.transform))
		{
			return IsValidPlatform(movingPlatform.activePlatform.gameObject);
		}
		return false;
	}

	public override Vector3 UpdateMove()
	{
		if (movingPlatform.enabled && !underWater)
		{
			if (IsPlatformUsable())
			{
				if (!movingPlatform.newPlatform)
				{
					float deltaTime = Time.deltaTime;
					movingPlatform.platformVelocity = ((deltaTime > 0f) ? ((movingPlatform.activePlatform.localToWorldMatrix.MultiplyPoint3x4(movingPlatform.activeLocalPoint) - movingPlatform.lastMatrix.MultiplyPoint3x4(movingPlatform.activeLocalPoint)) / deltaTime) : Vector3.zero);
				}
				movingPlatform.lastMatrix = movingPlatform.activePlatform.localToWorldMatrix;
				movingPlatform.newPlatform = false;
			}
			else
			{
				movingPlatform.platformVelocity = Vector3.zero;
			}
		}
		UpdateFunction();
		return movement.velocity;
	}

	private Vector3 ApplyInputVelocityChange(Vector3 velocity)
	{
		if (playerController == null || playerController.forwardReference == null)
		{
			return Vector3.zero;
		}
		Quaternion quaternion = ((underWater && canSwim) ? playerController.forwardReference.rotation : Quaternion.Euler(0f, playerController.forwardReference.rotation.eulerAngles.y, 0f));
		Vector3 vector = movementInputDirection;
		float num = Mathf.Min(1f, vector.magnitude);
		float num2 = ((underWater && canSwim) ? vector.y : 0f);
		vector.y = 0f;
		vector = quaternion * vector;
		vector.y += num2;
		vector.Normalize();
		Vector3 vector2 = default(Vector3);
		if (grounded && !underWater && TooSteep() && sliding.enabled)
		{
			vector2 = GetSlidingDirection();
			Vector3 vector3 = Vector3.Project(movementInputDirection, vector2);
			vector2 = vector2 + vector3 * sliding.speedControl + (movementInputDirection - vector3) * sliding.sidewaysControl;
			vector2 *= sliding.slidingSpeed;
		}
		else
		{
			float num3 = 1f;
			if (!underWater && sprintPressed && grounded)
			{
				float z = movementInputDirection.z;
				if (z > 0f)
				{
					num3 *= forwardSprintModifier;
				}
				else if (z == 0f)
				{
					num3 *= strafeSprintModifier;
				}
				sprinting = true;
			}
			vector2 = vector * forwardMaxSpeed * num3 * num;
		}
		if (!underWater && XRSettings.enabled)
		{
			vector2 *= VROptions.groundMoveScale;
		}
		if (!underWater && movingPlatform.enabled && movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer)
		{
			vector2 += movement.frameVelocity;
			vector2.y = 0f;
		}
		if (!underWater)
		{
			if (grounded)
			{
				vector2 = AdjustGroundVelocityToNormal(vector2, groundNormal);
			}
			else
			{
				velocity.y = 0f;
			}
		}
		float num4 = GetMaxAcceleration(grounded) * Time.deltaTime;
		Vector3 vector4 = default(Vector3);
		vector4 = vector2 - velocity;
		if (vector4.sqrMagnitude > num4 * num4)
		{
			vector4 = vector4.normalized * num4;
		}
		if (grounded || canControl)
		{
			velocity += vector4;
		}
		if (grounded && !underWater)
		{
			velocity.y = Mathf.Min(velocity.y, 0f);
		}
		return velocity;
	}

	private Vector3 ApplyGravityAndJumping(Vector3 velocity)
	{
		if (underWater)
		{
			return velocity;
		}
		if (!jumpPressed || !canControl)
		{
			jumping.holdingJumpButton = false;
			jumping.lastButtonDownTime = -100f;
		}
		if (jumpPressed && (jumping.lastButtonDownTime < 0f || allowMidAirJumping || flyCheatEnabled) && canControl)
		{
			jumping.lastButtonDownTime = Time.time;
		}
		if (!grounded)
		{
			float num = (floatingModeEnabled ? floatingModeSetup.gravity : gravity);
			velocity.y = movement.velocity.y - num * Time.deltaTime;
			float num2 = (floatingModeEnabled ? floatingModeSetup.maxFallSpeed : movement.maxFallSpeed);
			velocity.y = Mathf.Max(velocity.y, 0f - num2);
		}
		if (grounded || allowMidAirJumping || flyCheatEnabled)
		{
			if (canControl && (double)(Time.time - jumping.lastButtonDownTime) < 0.2)
			{
				grounded = false;
				jumping.jumping = true;
				jumping.lastStartTime = Time.time;
				jumping.lastButtonDownTime = -100f;
				jumping.holdingJumpButton = true;
				if (TooSteep())
				{
					jumping.jumpDir = Vector3.Slerp(Vector3.up, groundNormal, jumping.steepPerpAmount);
				}
				else
				{
					jumping.jumpDir = Vector3.Slerp(Vector3.up, groundNormal, jumping.perpAmount);
				}
				velocity.y = 0f;
				float targetJumpHeight = (floatingModeEnabled ? floatingModeSetup.jumpHeight : jumping.baseHeight);
				velocity += jumping.jumpDir * CalculateJumpVerticalSpeed(targetJumpHeight);
				if (movingPlatform.enabled && movingPlatform.movementTransfer != MovementTransferOnJump.InitTransfer)
				{
					_ = movingPlatform.movementTransfer;
					_ = 2;
				}
				SendMessage("OnJump", SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				jumping.holdingJumpButton = false;
			}
		}
		return velocity;
	}

	private bool IsValidPlatform(GameObject go)
	{
		if (go.layer == LayerID.TerrainCollider)
		{
			return true;
		}
		GameObject gameObject = UWE.Utils.GetEntityRoot(go);
		if (!gameObject)
		{
			gameObject = go;
		}
		return gameObject.GetComponent<IMovementPlatform>()?.IsPlatform() ?? true;
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		MovementCollisionData movementCollisionData = default(MovementCollisionData);
		movementCollisionData.impactVelocity = movement.velocity - previousVelocity;
		VFXSurfaceTypes vFXSurfaceTypes = Utils.GetObjectSurfaceType(hit.gameObject);
		if (vFXSurfaceTypes == VFXSurfaceTypes.none)
		{
			vFXSurfaceTypes = Utils.GetTerrainSurfaceType(hit.point, hit.normal);
		}
		movementCollisionData.surfaceType = vFXSurfaceTypes;
		if (hit.normal.y > 0f && hit.normal.y > groundNormal.y && hit.moveDirection.y < 0f)
		{
			if ((double)(hit.point - movement.lastHitPoint).sqrMagnitude > 0.001 || lastGroundNormal == Vector3.zero)
			{
				groundNormal = hit.normal;
			}
			else
			{
				groundNormal = lastGroundNormal;
			}
			if (IsValidPlatform(hit.gameObject))
			{
				movingPlatform.hitPlatform = hit.collider.transform;
			}
			movement.hitPoint = hit.point;
			movement.frameVelocity = Vector3.zero;
			groundSurfaceType = vFXSurfaceTypes;
			ICharacterMotorSlidingOverride component = hit.gameObject.GetComponent<ICharacterMotorSlidingOverride>();
			if (component != null && !component.Equals(null))
			{
				component.OnPlayerHit(hit, this);
				sliding.slidingOverride = component;
			}
		}
		SendMessage("OnMovementCollision", movementCollisionData, SendMessageOptions.DontRequireReceiver);
	}

	public VFXSurfaceTypes GetGroundSurfaceType()
	{
		return groundSurfaceType;
	}

	private IEnumerator SubtractNewPlatformVelocity()
	{
		if (!movingPlatform.enabled || (movingPlatform.movementTransfer != MovementTransferOnJump.InitTransfer && movingPlatform.movementTransfer != MovementTransferOnJump.PermaTransfer))
		{
			yield break;
		}
		if (movingPlatform.newPlatform)
		{
			Transform platform = movingPlatform.activePlatform;
			yield return new WaitForFixedUpdate();
			yield return new WaitForFixedUpdate();
			if (grounded && platform == movingPlatform.activePlatform)
			{
				Debug.Log("CharacterMotor.SubtractNewPlatformVelocity() yielding WaitForFixedUpdate (was 1) - seeing weird results?");
				yield return new WaitForFixedUpdate();
			}
		}
		movement.velocity -= movingPlatform.platformVelocity;
	}

	private bool MoveWithPlatform()
	{
		if (!underWater && movingPlatform.enabled && (grounded || movingPlatform.movementTransfer == MovementTransferOnJump.PermaLocked))
		{
			return movingPlatform.activePlatform != null;
		}
		return false;
	}

	private Vector3 AdjustGroundVelocityToNormal(Vector3 hVelocity, Vector3 groundNormal)
	{
		return Vector3.Cross(Vector3.Cross(Vector3.up, hVelocity), groundNormal).normalized * hVelocity.magnitude;
	}

	private bool IsGroundedTest()
	{
		return PlayerMotor.IsWalkable(groundNormal);
	}

	private float GetMaxAcceleration(bool grounded)
	{
		if (grounded || underWater)
		{
			return groundAcceleration;
		}
		if (floatingModeEnabled)
		{
			return floatingModeSetup.airAcceleration;
		}
		return airAcceleration;
	}

	private float CalculateJumpVerticalSpeed(float targetJumpHeight)
	{
		JumpGene component = base.gameObject.GetComponent<JumpGene>();
		float num = (component ? (component.Scalar * 5f * targetJumpHeight) : targetJumpHeight);
		float num2 = (floatingModeEnabled ? floatingModeSetup.gravity : gravity);
		return Mathf.Sqrt(2f * num * num2);
	}

	private bool IsJumping()
	{
		return jumping.jumping;
	}

	private bool IsSliding()
	{
		if (grounded && sliding.enabled)
		{
			return TooSteep();
		}
		return false;
	}

	private Vector3 GetSlidingDirection()
	{
		if (sliding.slidingOverride != null && !sliding.slidingOverride.Equals(null))
		{
			return sliding.slidingOverride.GetMoveDirection();
		}
		return new Vector3(groundNormal.x, 0f, groundNormal.z).normalized;
	}

	private bool IsTouchingCeiling()
	{
		return (movement.collisionFlags & CollisionFlags.Above) != 0;
	}

	public bool IsGrounded()
	{
		return grounded;
	}

	private bool TooSteep()
	{
		if (sliding.slidingOverride != null && !sliding.slidingOverride.Equals(null))
		{
			return sliding.slidingOverride.IsTooSteep();
		}
		return groundNormal.y <= Mathf.Cos(controller.slopeLimit * ((float)System.Math.PI / 180f));
	}

	private Vector3 GetDirection()
	{
		return movementInputDirection;
	}

	private float MaxSpeedInDirection(Vector3 desiredMovementDirection)
	{
		if (desiredMovementDirection == Vector3.zero)
		{
			return 0f;
		}
		float num = ((desiredMovementDirection.z > 0f) ? movement.maxForwardSpeed : movement.maxBackwardsSpeed) / movement.maxSidewaysSpeed;
		Vector3 normalized = new Vector3(desiredMovementDirection.x, 0f, desiredMovementDirection.z / num).normalized;
		return new Vector3(normalized.x, 0f, normalized.z * num).magnitude * movement.maxSidewaysSpeed;
	}

	public Vector3 GetVelocity()
	{
		return movement.velocity;
	}

	public override void SetVelocity(Vector3 velocity)
	{
		movement.velocity = velocity;
	}

	public void OnTeleport()
	{
		movingPlatform.activePlatform = null;
	}

	Vector3 IGroundMoveable.GetVelocity()
	{
		return movement.velocity;
	}

	bool IGroundMoveable.IsOnGround()
	{
		if (grounded)
		{
			return !underWater;
		}
		return false;
	}

	bool IGroundMoveable.IsUnderwater()
	{
		return underWater;
	}

	bool IGroundMoveable.IsActive()
	{
		return base.enabled;
	}

	Vector3 IGroundMoveable.GetGroundNormal()
	{
		return groundNormal;
	}

	public override Collider GetCollider()
	{
		return controller;
	}
}
