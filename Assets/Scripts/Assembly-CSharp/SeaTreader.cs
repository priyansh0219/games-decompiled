using System;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class SeaTreader : Creature
{
	[AssertNotNull]
	public Rigidbody myRigidbody;

	[AssertNotNull]
	public OnSurfaceTracker onSurfaceTracker;

	[AssertNotNull]
	public Collider aliveCollider;

	[AssertNotNull]
	public Collider deadCollider;

	public TreaderPath[] treaderPaths;

	public float minLeashDistance = 5f;

	private const float turnSpeedFactor = 11f / 150f;

	private const float maxVelocity = 1.6f;

	private const float acceleration = 4f;

	private const float turnLerpSpeed = 0.1f;

	private const float minTurnAngle = 5f;

	private bool isMoving;

	private float currentVelocity;

	private Vector3 currentTarget;

	private float prevMoveSpeed;

	private float prevTurnSpeed;

	private TreaderPath path;

	private int currentPathPointIndex = -1;

	private bool _cinematicMode;

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int treader_version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public float grazingTimeLeft = -1f;

	[NonSerialized]
	[ProtoMember(3)]
	public bool reverseDirection;

	private bool grazing;

	public float leashDistance { get; private set; }

	public bool cinematicMode
	{
		get
		{
			return _cinematicMode;
		}
		set
		{
			_cinematicMode = value;
			UWE.Utils.SetIsKinematicAndUpdateInterpolation(myRigidbody, value);
		}
	}

	protected override void InitializeOnce()
	{
		base.InitializeOnce();
		FindClosestPathPoint();
	}

	protected override void InitializeAgain()
	{
		base.InitializeAgain();
		RestorePathPoint();
	}

	private void FixedUpdate()
	{
		Rigidbody rigidbody = myRigidbody;
		if (isMoving && onSurfaceTracker.onSurface && !cinematicMode)
		{
			currentTarget.y = base.transform.position.y;
			if (Vector3.SqrMagnitude(currentTarget - base.transform.position) < 0.5f)
			{
				Idle();
			}
			else if (rigidbody.velocity.magnitude < currentVelocity)
			{
				Vector3 vector = currentTarget - base.transform.position;
				rigidbody.AddForce(Vector3.ProjectOnPlane(vector, onSurfaceTracker.surfaceNormal).normalized * 4f, ForceMode.Acceleration);
			}
		}
		float num = GetMaxVelocity();
		if (rigidbody.velocity.magnitude > num)
		{
			rigidbody.velocity = rigidbody.velocity.normalized * num;
		}
	}

	public void Update()
	{
		leashPosition.y = base.transform.position.y;
		UpdatePath(out var grazingAnimation);
		UpdateTurning(out var turningAnimation);
		bool isKinematic = cinematicMode || grazingAnimation || turningAnimation;
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(myRigidbody, isKinematic);
		float b = 0f;
		if (onSurfaceTracker.onSurface && !myRigidbody.isKinematic)
		{
			b = Vector3.ProjectOnPlane(myRigidbody.velocity, onSurfaceTracker.surfaceNormal).magnitude / GetMaxVelocity();
		}
		b = Mathf.Lerp(prevMoveSpeed, b, 1f * Time.deltaTime);
		SafeAnimator.SetFloat(GetAnimator(), "move_speed_z", b);
		prevMoveSpeed = b;
		AllowCreatureUpdates(onSurfaceTracker.onSurface || cinematicMode);
	}

	private void UpdateTurning(out bool turningAnimation)
	{
		Vector3 forward = Vector3.ProjectOnPlane(onSurfaceTracker.onSurface ? (currentTarget - base.transform.position) : base.transform.forward, onSurfaceTracker.surfaceNormal);
		Vector3 forward2 = base.transform.forward;
		Quaternion b = Quaternion.LookRotation(forward, onSurfaceTracker.surfaceNormal);
		myRigidbody.MoveRotation(Quaternion.Lerp(base.transform.rotation, b, Time.deltaTime * 0.1f));
		float num = 0f;
		turningAnimation = false;
		if (onSurfaceTracker.onSurface && !cinematicMode)
		{
			num = Vector3.Angle(forward2, base.transform.forward) / Time.deltaTime;
			turningAnimation = num > 5f;
		}
		GetAnimator().SetBool("turning", turningAnimation);
		if (turningAnimation)
		{
			if (base.transform.InverseTransformDirection(forward2).x > 0f)
			{
				num = 0f - num;
			}
			float b2 = num * (11f / 150f);
			b2 = Mathf.Lerp(prevTurnSpeed, b2, 1f * Time.deltaTime);
			SafeAnimator.SetFloat(GetAnimator(), "turn_speed", b2);
			prevTurnSpeed = b2;
		}
	}

	private void UpdatePath(out bool grazingAnimation)
	{
		grazingAnimation = false;
		if (cinematicMode)
		{
			return;
		}
		if (grazing)
		{
			grazingTimeLeft -= Time.deltaTime;
			if (grazingTimeLeft <= 0f)
			{
				grazing = false;
				SetNextPathPoint();
			}
			grazingAnimation = grazing && onSurfaceTracker.onSurface && !isMoving;
			GetAnimator().SetBool("grazing", grazingAnimation);
			UWE.Utils.SetIsKinematicAndUpdateInterpolation(myRigidbody, grazingAnimation);
		}
		else if (Vector3.SqrMagnitude(leashPosition - base.transform.position) <= leashDistance * leashDistance)
		{
			if (grazingTimeLeft > 0f)
			{
				grazing = true;
			}
			else
			{
				SetNextPathPoint();
			}
		}
	}

	public void MoveTo(Vector3 target, float velocity = -1f)
	{
		isMoving = true;
		currentTarget = target;
		currentTarget.y = base.transform.position.y;
		float num = GetMaxVelocity();
		currentVelocity = ((velocity > 0f && velocity < num) ? velocity : num);
	}

	public void Idle()
	{
		isMoving = false;
	}

	public float GetMaxVelocity()
	{
		return 1.6f * base.transform.localScale.x;
	}

	private void FindClosestPathPoint()
	{
		int num = -1;
		int num2 = -1;
		float num3 = float.PositiveInfinity;
		for (int i = 0; i < treaderPaths.Length; i++)
		{
			for (int j = 0; j < treaderPaths[i].pathPoints.Count; j++)
			{
				float num4 = Vector3.SqrMagnitude(base.transform.position - treaderPaths[i].pathPoints[j].position);
				if (num4 < num3)
				{
					num3 = num4;
					num = i;
					num2 = j;
				}
			}
		}
		path = treaderPaths[num];
		int num5 = (reverseDirection ? (num2 - 1) : (num2 + 1));
		if (num5 >= 0 && num5 < path.pathPoints.Count)
		{
			float num6 = Vector3.Angle(base.transform.forward, base.transform.position - path.pathPoints[num2].position);
			float num7 = Vector3.Angle(base.transform.forward, base.transform.position - path.pathPoints[num5].position);
			if (num6 > 90f && num7 <= 90f)
			{
				num2 = num5;
			}
		}
		SetPathPoint(num2);
		grazing = grazingTimeLeft > 0f;
	}

	private void RestorePathPoint()
	{
		for (int i = 0; i < treaderPaths.Length; i++)
		{
			for (int j = 0; j < treaderPaths[i].pathPoints.Count; j++)
			{
				if (treaderPaths[i].pathPoints[j].position.x == leashPosition.x && treaderPaths[i].pathPoints[j].position.z == leashPosition.z)
				{
					path = treaderPaths[i];
					SetPathPoint(j, setGrazingTime: false);
					return;
				}
			}
		}
		FindClosestPathPoint();
	}

	private void SetNextPathPoint()
	{
		if ((float)currentPathPointIndex == 0f || currentPathPointIndex == path.pathPoints.Count - 1)
		{
			reverseDirection = !reverseDirection;
		}
		SetPathPoint(reverseDirection ? (currentPathPointIndex - 1) : (currentPathPointIndex + 1));
	}

	private TreaderPath.PathPoint GetCurrentPathPoint()
	{
		return path.pathPoints[currentPathPointIndex];
	}

	private void SetPathPoint(int pointIndex, bool setGrazingTime = true)
	{
		if (!(path == null) && pointIndex >= 0 && pointIndex < path.pathPoints.Count)
		{
			TreaderPath.PathPoint pathPoint = path.pathPoints[pointIndex];
			leashPosition = pathPoint.position;
			leashDistance = Mathf.Max(pathPoint.grazingRange, minLeashDistance);
			if (setGrazingTime)
			{
				grazingTimeLeft = pathPoint.grazingTime;
			}
			currentPathPointIndex = pointIndex;
		}
	}

	public override void OnKill()
	{
		Animator animator = GetAnimator();
		animator.SetBool("grazing", value: false);
		animator.SetBool("turning", value: false);
		animator.SetBool("pooping", value: false);
		animator.SetBool(AnimatorHashID.dead, value: true);
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(myRigidbody, isKinematic: false);
		aliveCollider.enabled = false;
		deadCollider.enabled = true;
		deadCollider.isTrigger = false;
		base.OnKill();
	}

	public bool IsWalking()
	{
		if (isMoving && onSurfaceTracker.onSurface && !cinematicMode)
		{
			return myRigidbody.velocity.sqrMagnitude > 0.01f;
		}
		return false;
	}
}
