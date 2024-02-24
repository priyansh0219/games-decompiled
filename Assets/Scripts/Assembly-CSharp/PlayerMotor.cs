using System;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class PlayerMotor : MonoBehaviour
{
	[AssertNotNull]
	public Rigidbody rb;

	[AssertNotNull]
	public PlayerController playerController;

	public bool usingGravity;

	public bool canControl = true;

	public float forwardMaxSpeed = 5f;

	public float backwardMaxSpeed = 4f;

	public float strafeMaxSpeed = 5f;

	public float verticalMaxSpeed = 5f;

	public float climbSpeed = 2f;

	public float gravity = 12f;

	public float underWaterGravity;

	public bool canSwim = true;

	public float forwardSprintModifier = 2f;

	public float strafeSprintModifier = 2f;

	public float swimDrag = 2f;

	public float groundDrag = 1.8f;

	public float airDrag;

	public float ladderDrag = 1.7f;

	public float ladderAcceleration = 18f;

	[FormerlySerializedAs("acceleration")]
	public float waterAcceleration = 20f;

	public float groundAcceleration = 45f;

	public float airAcceleration = 5f;

	public bool canJump = true;

	public float jumpHeight = 2f;

	[NonSerialized]
	public bool grounded = true;

	protected Vector3 movementInputDirection;

	protected bool jumpPressed;

	protected bool sprintPressed;

	protected bool underWater;

	protected float debugSpeedMult = 1f;

	private static int layerMask;

	public abstract void SetVelocity(Vector3 velocity);

	public abstract void SetControllerRadius(float radius);

	public abstract float GetControllerRadius();

	public abstract void SetControllerHeight(float height, float cameraOffset);

	public abstract float GetControllerHeight();

	public abstract void SetEnabled(bool enabled);

	public abstract Vector3 UpdateMove();

	public abstract bool IsSprinting();

	public abstract Vector3 GetColliderPosition();

	public virtual void Start()
	{
		DevConsole.RegisterConsoleCommand(this, "swimx");
		DevConsole.RegisterConsoleCommand(this, "shark");
		DevConsole.RegisterConsoleCommand(this, "seal");
		DevConsole.RegisterConsoleCommand(this, "sonic");
		rb.freezeRotation = true;
		layerMask = 524288;
		layerMask = ~layerMask;
	}

	public void SetUnderWater(bool newUnderWater)
	{
		underWater = newUnderWater;
	}

	public static bool IsWalkable(Vector3 normal)
	{
		return normal.y > 0.01f;
	}

	private void Update()
	{
		if (playerController.inputEnabled && !FPSInputModule.current.lockMovement)
		{
			jumpPressed = GameInput.GetButtonHeld(GameInput.Button.Jump);
			sprintPressed = GameInput.IsRunning;
			if (!canControl)
			{
				movementInputDirection = Vector3.zero;
			}
			else
			{
				movementInputDirection = GameInput.GetMoveDirection();
			}
		}
		else
		{
			movementInputDirection = Vector3.zero;
		}
	}

	private void OnConsoleCommand_shark()
	{
		if ((double)Mathf.Abs(debugSpeedMult - 1f) < 0.0001)
		{
			debugSpeedMult = 10f;
		}
		else
		{
			debugSpeedMult = 1f;
		}
		ErrorMessage.AddDebug("Speed mult cheat = " + debugSpeedMult);
	}

	private void OnConsoleCommand_seal()
	{
		if ((double)Mathf.Abs(debugSpeedMult - 1f) < 0.0001)
		{
			debugSpeedMult = 5f;
		}
		else
		{
			debugSpeedMult = 1f;
		}
		ErrorMessage.AddDebug("Speed mult cheat = " + debugSpeedMult);
	}

	private void OnConsoleCommand_sonic()
	{
		if ((double)Mathf.Abs(debugSpeedMult - 1f) < 0.0001)
		{
			debugSpeedMult = 50f;
		}
		else
		{
			debugSpeedMult = 1f;
		}
		ErrorMessage.AddDebug("Speed mult cheat = " + debugSpeedMult);
	}

	private void OnConsoleCommand_swimx(NotificationCenter.Notification n)
	{
		DevConsole.ParseFloat(n, 0, out debugSpeedMult, 1f);
		ErrorMessage.AddDebug("Speed mult cheat = " + debugSpeedMult);
	}

	public abstract Collider GetCollider();
}
