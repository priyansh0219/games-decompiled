using RadicalLibrary;
using UnityEngine;

[RequireComponent(typeof(Locomotion))]
[DisallowMultipleComponent]
public class SplineFollowing : MonoBehaviour, IManagedUpdateBehaviour, IManagedBehaviour
{
	public Vector3 targetPosition;

	public Vector3 targetDirection;

	public float targetRange = 1f;

	public float lookAhead = 1f;

	public float inertia = 1f;

	private Vector3 p0;

	private Vector3 p1;

	private Vector3 v0;

	private Vector3 v1;

	private Vector3 c0;

	private Vector3 c1;

	private float sentinelTime;

	private float medianSpeed;

	[AssertNotNull]
	public Locomotion locomotion;

	[AssertNotNull]
	public Rigidbody useRigidbody;

	private bool driving;

	[AssertNotNull]
	public BehaviourLOD levelOfDetail;

	public bool respectLOD = true;

	private float updateFrequency;

	private float nextUpdateTime;

	private float lastUpdateTime;

	public int managedUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "SplineFollowing";
	}

	public void Idle()
	{
		driving = false;
		locomotion.Idle();
	}

	public void GoTo(Vector3 targetPos, Vector3 targetDir, float velocity)
	{
		targetPosition = targetPos;
		targetDirection = targetDir;
		medianSpeed = velocity;
		UpdatePath();
		driving = true;
		locomotion.maxVelocity = velocity;
		BehaviourUpdateUtils.Register(this);
	}

	public void SetLookTarget(Transform target)
	{
		locomotion.SetLookTarget(target);
	}

	public void ManagedUpdate()
	{
		if (!driving)
		{
			BehaviourUpdateUtils.Deregister(this);
		}
		else if ((targetPosition - base.transform.position).sqrMagnitude < targetRange * targetRange)
		{
			driving = false;
			locomotion.Idle();
		}
		else
		{
			if (Time.time < nextUpdateTime)
			{
				return;
			}
			float num = Time.deltaTime;
			if (respectLOD)
			{
				switch (levelOfDetail.current)
				{
				case LODState.Full:
					updateFrequency = 0.033333f;
					break;
				case LODState.Medium:
					updateFrequency = 0.2f;
					break;
				case LODState.Minimal:
					updateFrequency = 0.5f;
					break;
				}
				num = Time.time - lastUpdateTime;
				for (lastUpdateTime = Time.time; nextUpdateTime <= Time.time; nextUpdateTime += updateFrequency)
				{
				}
			}
			Vector3 vector = CubicBez.Velocity(p0, p1, c0, c1, sentinelTime);
			float num2 = Mathf.Max(vector.magnitude, 1f);
			float num3 = medianSpeed / num2;
			sentinelTime = Mathf.Clamp01(sentinelTime + num * num3);
			Vector3 vector2 = CubicBez.Interp(p0, p1, c0, c1, sentinelTime);
			locomotion.GoTo(vector2, vector, lookAhead * 0.5f);
		}
	}

	public void OnDrawGizmosSelected()
	{
		DrawSpline(p0, p1, c0, c1, sentinelTime, 20);
	}

	private void UpdatePath()
	{
		p0 = base.transform.position;
		v0 = base.transform.forward;
		p1 = targetPosition;
		v1 = targetDirection;
		float num = Mathf.Clamp(ComputeLength(p0, p1, v0, v1), 1f, 50f);
		c0 = p0 + v0 * num / 3f;
		c1 = p1 - v1 * num / 3f;
		Vector3 velocity = useRigidbody.velocity;
		Vector3 vector = CubicBez.Velocity(p0, p1, c0, c1, 0f);
		float num2 = Mathf.Max(velocity.magnitude, 1f);
		float num3 = Mathf.Max(vector.magnitude, 1f);
		sentinelTime = Mathf.Clamp01(lookAhead * (num2 / num3));
	}

	public static float ComputeLength(Vector3 pos0, Vector3 pos1, Vector3 dir0, Vector3 dir1)
	{
		return (pos1 - pos0).magnitude;
	}

	public static void DrawSpline(Vector3 p0, Vector3 p1, Vector3 c0, Vector3 c1, float t, int numSections)
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(p0, c0);
		Gizmos.color = Color.green;
		Gizmos.DrawLine(c1, p1);
		Vector3 from = p0;
		for (int i = 0; i <= numSections; i++)
		{
			float t2 = (float)i / (float)numSections;
			Vector3 vector = CubicBez.Interp(p0, p1, c0, c1, t2);
			Gizmos.color = Color.white;
			Gizmos.DrawLine(from, vector);
			from = vector;
		}
		Vector3 vector2 = CubicBez.Interp(p0, p1, c0, c1, t);
		Vector3 vector3 = CubicBez.Velocity(p0, p1, c0, c1, t);
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(vector2, 0.1f);
		Gizmos.DrawLine(vector2, vector2 + vector3);
	}

	private void OnEnable()
	{
		nextUpdateTime = Time.time;
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
