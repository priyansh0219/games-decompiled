using UnityEngine;

[RequireComponent(typeof(SplineFollowing))]
[DisallowMultipleComponent]
public class SwimBehaviour : MonoBehaviour, IManagedUpdateBehaviour, IManagedBehaviour
{
	[AssertNotNull]
	public SplineFollowing splineFollowing;

	private Vector3 originalTargetPosition;

	private Vector3 originalTargetDirection;

	private float originalVelocity;

	private bool overridingTarget;

	private float timeEndOverride;

	private const float turnAroundDistanceThreshold = 1f;

	private const float turnAroundDotThreshold = 0f;

	private const float turnAroundDuration = 0.2f;

	private const float turnAroundMinDiameter = 2f;

	[Range(0f, 1f)]
	public float turnSpeed = 1f;

	private const float overshootDotThreshold = 0.95f;

	private const float overshootDuration = 0.5f;

	private const float overshootFactor = 3f;

	public int managedUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "SwimBehavior";
	}

	public void ManagedUpdate()
	{
		if (!overridingTarget)
		{
			BehaviourUpdateUtils.Deregister(this);
			return;
		}
		float time = Time.time;
		if (overridingTarget && time > timeEndOverride)
		{
			SwimToInternal(originalTargetPosition, originalTargetDirection, originalVelocity, overshoot: false, ignoreTargetOverride: true);
			if (time > timeEndOverride)
			{
				EndTargetOverride();
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (overridingTarget)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(base.transform.position, originalTargetPosition);
		}
	}

	public virtual void Idle()
	{
		EndTargetOverride();
		splineFollowing.Idle();
	}

	public void LookAt(Transform target)
	{
		splineFollowing.SetLookTarget(target);
	}

	public void LookForward()
	{
		splineFollowing.SetLookTarget(null);
	}

	public void SwimTo(Vector3 targetPosition, float velocity)
	{
		Vector3 vector = targetPosition - base.transform.position;
		vector.y = 0f;
		SwimTo(targetPosition, vector.normalized, velocity);
	}

	public void SwimTo(Vector3 targetPosition, Vector3 targetDirection, float velocity)
	{
		SwimToInternal(targetPosition, targetDirection, velocity, overshoot: false);
	}

	public void Attack(Vector3 targetPosition, Vector3 targetDirection, float velocity)
	{
		SwimToInternal(targetPosition, targetDirection, velocity, overshoot: true);
	}

	private void SwimToInternal(Vector3 targetPosition, Vector3 targetDirection, float velocity, bool overshoot, bool ignoreTargetOverride = false)
	{
		originalTargetPosition = targetPosition;
		originalTargetDirection = targetDirection;
		originalVelocity = velocity;
		if (!overridingTarget || ignoreTargetOverride)
		{
			Vector3 toTarget = targetPosition - base.transform.position;
			Vector3 normalized = toTarget.normalized;
			if (toTarget.magnitude > 1f && Vector3.Dot(base.transform.forward, normalized) < 0f)
			{
				TurnAround(toTarget, velocity);
			}
			else if (overshoot && Vector3.Dot(base.transform.forward, normalized) > 0.95f)
			{
				Overshoot(targetPosition, normalized, velocity);
			}
			else
			{
				GoToInternal(targetPosition, targetDirection, velocity);
			}
		}
	}

	protected virtual void GoToInternal(Vector3 targetPosition, Vector3 targetDirection, float velocity)
	{
		splineFollowing.GoTo(targetPosition, targetDirection, velocity);
	}

	private void TurnAround(Vector3 toTarget, float velocity)
	{
		if (StartTargetOverride(0.2f))
		{
			Vector3 normalized = toTarget.normalized;
			Vector3 vector = Vector3.Dot(base.transform.up, normalized) * base.transform.up;
			Vector3 b = base.transform.right * Mathf.Sign(Vector3.Dot(base.transform.right, normalized));
			Vector3 normalized2 = (Vector3.Slerp(base.transform.forward, b, turnSpeed) + vector * turnSpeed).normalized;
			float num = Mathf.Max(velocity, 2f);
			Vector3 targetPosition = base.transform.position + normalized2 * num;
			Vector3 targetDirection = normalized;
			GoToInternal(targetPosition, targetDirection, velocity);
		}
	}

	private void Overshoot(Vector3 targetPosition, Vector3 normToTarget, float velocity)
	{
		if (!StartTargetOverride(0.5f))
		{
			GoToInternal(targetPosition, normToTarget, velocity);
			return;
		}
		Vector3 targetPosition2 = targetPosition + normToTarget * velocity * 3f;
		GoToInternal(targetPosition2, normToTarget, velocity);
	}

	private bool StartTargetOverride(float duration)
	{
		if (!overridingTarget && !BehaviourUpdateUtils.Register(this))
		{
			return false;
		}
		overridingTarget = true;
		timeEndOverride = Time.time + duration;
		return true;
	}

	private bool EndTargetOverride()
	{
		if (overridingTarget && !BehaviourUpdateUtils.Deregister(this))
		{
			return false;
		}
		overridingTarget = false;
		return true;
	}

	private void OnDisable()
	{
		BehaviourUpdateUtils.Deregister(this);
		overridingTarget = false;
		Idle();
	}

	private void OnDestroy()
	{
		BehaviourUpdateUtils.Deregister(this);
	}
}
