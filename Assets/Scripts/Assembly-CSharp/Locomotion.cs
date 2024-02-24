using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class Locomotion : MonoBehaviour, IManagedFixedUpdateBehaviour, IManagedBehaviour
{
	public float maxVelocity = 10f;

	public float maxAcceleration = 10f;

	public float forwardRotationSpeed = 0.6f;

	public float upRotationSpeed = 3f;

	[AssertNotNull]
	public BehaviourLOD levelOfDetail;

	[Range(0f, 1f)]
	public float driftFactor = 0.5f;

	public bool canMoveAboveWater;

	public bool canWalkOnSurface;

	public bool freezeHorizontalRotation;

	public bool rotateToSurfaceNormal = true;

	public Vector3 acceleration;

	private OnSurfaceTracker onSurfaceTracker;

	private Transform lookTarget;

	private Vector3 intentDirection = Vector3.zero;

	[AssertNotNull]
	public Rigidbody useRigidbody;

	private Vector3 previousForwardError = Vector3.zero;

	private Vector3 previousUpError = Vector3.zero;

	private const float swimForwardFactor = 5f;

	private const float swimUpFactor = 1f;

	private const float onSurfaceForwardFactor = 1f;

	private const float onSurfaceUpFactor = 5f;

	private readonly Vector3 cachedZero = Vector3.zero;

	private readonly Vector3 cachedUp = Vector3.up;

	private readonly Vector3 cachedForward = Vector3.forward;

	private Vector3 torque = Vector3.zero;

	private Vector3 targetForward = Vector3.zero;

	private Vector3 normal = Vector3.up;

	private Vector3 forwardTorque = Vector3.zero;

	private Vector3 upTorque = Vector3.zero;

	public int managedFixedUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "Locomotion";
	}

	private void Start()
	{
		onSurfaceTracker = GetComponent<OnSurfaceTracker>();
	}

	private void OnKill()
	{
		base.enabled = false;
	}

	public void Idle()
	{
		acceleration = cachedZero;
		intentDirection = cachedZero;
	}

	public void ApplyVelocity(Vector3 velocity)
	{
		intentDirection = velocity.normalized;
		Vector3 target = Vector3.ClampMagnitude(velocity, maxVelocity);
		acceleration = Derive(useRigidbody.velocity, target, maxAcceleration, 0.1f);
	}

	public void GoTo(Vector3 targetPosition, Vector3 targetDirection, float deltaTime)
	{
		intentDirection = targetDirection.normalized;
		Vector3 target = Derive(useRigidbody.position, targetPosition, maxVelocity, deltaTime);
		acceleration = Derive(useRigidbody.velocity, target, maxAcceleration, 0.1f);
	}

	public void SetLookTarget(Transform target)
	{
		lookTarget = target;
	}

	public void ManagedFixedUpdate()
	{
		Vector3 position = base.transform.position;
		Quaternion rotation = base.transform.rotation;
		Vector3 lhs = rotation * cachedUp;
		if (position.y < 0f || canMoveAboveWater)
		{
			useRigidbody.AddForce(acceleration, ForceMode.Acceleration);
		}
		bool flag = useRigidbody.velocity.sqrMagnitude > 0.01f;
		bool flag2 = false;
		normal = cachedUp;
		if (onSurfaceTracker != null && onSurfaceTracker.onSurface)
		{
			flag2 = canWalkOnSurface;
			if (rotateToSurfaceNormal)
			{
				normal = onSurfaceTracker.surfaceNormal;
			}
		}
		torque = cachedZero;
		bool flag3 = false;
		float fixedDeltaTime = Time.fixedDeltaTime;
		Vector3 lhs2 = rotation * cachedForward;
		if (flag && (lookTarget != null || intentDirection.sqrMagnitude > 0.01f))
		{
			targetForward = cachedZero;
			if (lookTarget != null)
			{
				targetForward.x = lookTarget.position.x - position.x;
				targetForward.y = lookTarget.position.y - position.y;
				targetForward.z = lookTarget.position.z - position.z;
			}
			else
			{
				float num = 1f - driftFactor;
				targetForward.x = driftFactor * (intentDirection.x * maxVelocity) + num * useRigidbody.velocity.x;
				targetForward.y = driftFactor * (intentDirection.y * maxVelocity) + num * useRigidbody.velocity.y;
				targetForward.z = driftFactor * (intentDirection.z * maxVelocity) + num * useRigidbody.velocity.z;
			}
			targetForward.Normalize();
			if (freezeHorizontalRotation || flag2)
			{
				float num2 = Vector3.Dot(normal, targetForward);
				targetForward.x -= normal.x * num2;
				targetForward.y -= normal.y * num2;
				targetForward.z -= normal.z * num2;
			}
			Vector3 error = Vector3.Cross(lhs2, targetForward);
			UpdatePID(ref forwardTorque, ref previousForwardError, error, fixedDeltaTime);
			float num3 = (flag2 ? 1f : 5f) * forwardRotationSpeed;
			torque.x = forwardTorque.x * num3;
			torque.y = forwardTorque.y * num3;
			torque.z = forwardTorque.z * num3;
			flag3 = true;
		}
		Vector3 rhs = normal;
		if (!freezeHorizontalRotation && !flag2)
		{
			float num4 = Vector3.Dot(lhs2, normal);
			rhs.x = normal.x - lhs2.x * num4;
			rhs.y = normal.y - lhs2.y * num4;
			rhs.z = normal.z - lhs2.z * num4;
		}
		Vector3 error2 = Vector3.Cross(lhs, rhs);
		if (flag2 && !flag && error2.sqrMagnitude < 0.01f)
		{
			useRigidbody.angularVelocity = cachedZero;
		}
		else
		{
			UpdatePID(ref upTorque, ref previousUpError, error2, fixedDeltaTime);
			float num5 = (flag2 ? 5f : 1f) * upRotationSpeed;
			torque.x += upTorque.x * num5;
			torque.y += upTorque.y * num5;
			torque.z += upTorque.z * num5;
			flag3 = true;
		}
		if (flag3)
		{
			useRigidbody.AddTorque(torque, ForceMode.Acceleration);
		}
	}

	public static Vector3 Derive(Vector3 current, Vector3 target, float maxDelta, float deltaTime)
	{
		return Vector3.ClampMagnitude((target - current) / deltaTime, maxDelta);
	}

	public static Vector3 Derive(Quaternion current, Quaternion target, float maxDelta, float deltaTime)
	{
		(Quaternion.Inverse(current) * target).ToAngleAxis(out var angle, out var axis);
		angle = Mathf.Repeat(angle + 180f, 360f) - 180f;
		angle *= (float)Math.PI / 180f;
		float num = Mathf.Clamp(angle / deltaTime, 0f - maxDelta, maxDelta);
		return axis * num;
	}

	private static void UpdatePID(ref Vector3 adjusted, ref Vector3 previousError, Vector3 error, float deltaTime)
	{
		float num = (error.x - previousError.x) / deltaTime;
		float num2 = (error.y - previousError.y) / deltaTime;
		float num3 = (error.z - previousError.z) / deltaTime;
		previousError.x = error.x;
		previousError.y = error.y;
		previousError.z = error.z;
		adjusted.x = error.x + num;
		adjusted.y = error.y + num2;
		adjusted.z = error.z + num3;
	}

	private void OnEnable()
	{
		BehaviourUpdateUtils.Register(this);
	}

	private void OnDisable()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	private void OnDestroy()
	{
		BehaviourUpdateUtils.Deregister(this);
	}
}
