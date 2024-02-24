using UWE;
using UnityEngine;

public class TrailManager : MonoBehaviour, IManagedLateUpdateBehaviour, IManagedBehaviour
{
	public Transform rootTransform;

	public Transform rootSegment;

	public Transform[] trails;

	[AssertNotNull]
	public AnimationCurve pitchMultiplier;

	[AssertNotNull]
	public AnimationCurve rollMultiplier;

	[AssertNotNull]
	public AnimationCurve yawMultiplier;

	public float segmentSnapSpeed = 5f;

	public float maxSegmentOffset = -1f;

	public bool debugDraw;

	private static float lerpRotationSpeed = 1f;

	private static readonly ArrayPool<float> floatPool = new ArrayPool<float>(4, 1);

	private static readonly ArrayPool<Vector3> vectorPool = new ArrayPool<Vector3>(12, 1);

	private static readonly ArrayPool<Quaternion> quatPool = new ArrayPool<Quaternion>(16, 1);

	private Vector3[] trailStartPositions;

	private Quaternion[] trailStartRotations;

	private Vector3[] prevPositions;

	private Quaternion[] prevRotation;

	private Vector3[] prevUpDirection;

	private Vector3[] trailSpaceForward;

	private Vector3[] trailSpaceUp;

	private Quaternion[] trailSpaceRotOffset;

	private Vector3[] rotationMultipliers;

	private float[] distances;

	private bool initialized;

	private bool isDisabled;

	public bool allowDisableOnScreen = true;

	[AssertNotNull]
	public BehaviourLOD levelOfDetail;

	private float lastUpdateTime;

	private float timeBetweenMediumLODUpdates = 0.1f;

	private float nextUpdateTime;

	public int managedLateUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "TrailManager";
	}

	private void OnValidate()
	{
		if (initialized)
		{
			InitializeRotationMultipliers();
		}
	}

	private void Awake()
	{
		Initialize();
	}

	private void Initialize()
	{
		try
		{
			if (!initialized && GetScale() != 0f)
			{
				trailStartPositions = vectorPool.Get(trails.Length);
				prevPositions = vectorPool.Get(trails.Length);
				prevRotation = quatPool.Get(trails.Length);
				prevUpDirection = vectorPool.Get(trails.Length);
				trailStartRotations = quatPool.Get(trails.Length);
				trailSpaceForward = vectorPool.Get(trails.Length);
				trailSpaceUp = vectorPool.Get(trails.Length);
				trailSpaceRotOffset = quatPool.Get(trails.Length);
				distances = floatPool.Get(trails.Length);
				InitializePositions();
				InitializeRotationMultipliers();
				InitializeSegments();
				initialized = true;
			}
		}
		finally
		{
		}
	}

	private void InitializePositions()
	{
		float scale = GetScale();
		for (int i = 0; i < trails.Length; i++)
		{
			trailStartPositions[i] = trails[i].localPosition;
			trailStartRotations[i] = trails[i].localRotation;
			Vector3 position = rootSegment.position;
			if (i > 0)
			{
				position = trails[i - 1].position;
			}
			distances[i] = (position - trails[i].position).magnitude / scale;
			trails[i].localRotation = Quaternion.identity;
			Vector3 normalized = (position - trails[i].position).normalized;
			Vector3 normalized2 = Vector3.ProjectOnPlane(rootTransform.up, normalized).normalized;
			trailSpaceForward[i] = trails[i].InverseTransformDirection(normalized);
			trailSpaceUp[i] = trails[i].InverseTransformDirection(normalized2);
			trailSpaceRotOffset[i] = Quaternion.LookRotation(trailSpaceForward[i], trailSpaceUp[i]);
			trails[i].localRotation = trailStartRotations[i];
		}
	}

	private void InitializeRotationMultipliers()
	{
		rotationMultipliers = vectorPool.Get(trails.Length);
		for (int i = 0; i < trails.Length; i++)
		{
			rotationMultipliers[i] = new Vector3(pitchMultiplier.Evaluate(Mathf.InverseLerp(0f, trails.Length - 1, i)), yawMultiplier.Evaluate(Mathf.InverseLerp(0f, trails.Length - 1, i)), rollMultiplier.Evaluate(Mathf.InverseLerp(0f, trails.Length - 1, i)));
		}
	}

	private void InitializeSegments()
	{
		for (int i = 0; i < trails.Length; i++)
		{
			prevPositions[i] = trails[i].position;
			prevRotation[i] = trails[i].localRotation;
			prevUpDirection[i] = trails[i].up;
		}
	}

	private void ResetSegments()
	{
		for (int i = 0; i < trails.Length; i++)
		{
			trails[i].localPosition = trailStartPositions[i];
			trails[i].localRotation = trailStartRotations[i];
		}
	}

	private void Disable()
	{
		if (!isDisabled)
		{
			ResetSegments();
			isDisabled = true;
		}
	}

	private void ReEnable()
	{
		if (isDisabled)
		{
			isDisabled = false;
			InitializeSegments();
		}
	}

	public void ManagedLateUpdate()
	{
		float deltaTime = Time.deltaTime;
		switch (levelOfDetail.current)
		{
		case LODState.Minimal:
			Disable();
			return;
		case LODState.Medium:
			if (allowDisableOnScreen)
			{
				Disable();
				return;
			}
			break;
		}
		if (isDisabled)
		{
			ReEnable();
		}
		UpdateTrails(deltaTime);
		lastUpdateTime = Time.time;
	}

	private void UpdateTrails(float deltaTime)
	{
		float scale = GetScale();
		Vector3 vector = rootSegment.position;
		for (int i = 0; i < trails.Length; i++)
		{
			trails[i].localRotation = Quaternion.identity;
			trails[i].localPosition = trailStartPositions[i];
			Vector3 position = trails[i].position;
			Vector3 vector2 = position - prevPositions[i];
			Vector3 vector3 = Vector3.Slerp(prevPositions[i], position, deltaTime * segmentSnapSpeed * scale);
			if (maxSegmentOffset > 0f && (position - vector3).sqrMagnitude > maxSegmentOffset * maxSegmentOffset)
			{
				vector3 = position - vector2.normalized * maxSegmentOffset;
			}
			Vector3 normalized = trails[i].InverseTransformVector(vector - vector3).normalized;
			Vector3 normalized2 = Vector3.ProjectOnPlane(trails[i].InverseTransformVector(prevUpDirection[i]), normalized).normalized;
			Vector3 eulerAngles = Quaternion.LookRotation(normalized, normalized2).eulerAngles;
			eulerAngles = LerpAngle((prevRotation[i] * trailSpaceRotOffset[i]).eulerAngles, eulerAngles, rotationMultipliers[i] * Time.timeScale);
			Quaternion quaternion = Quaternion.Slerp(Quaternion.Euler(eulerAngles) * Quaternion.Inverse(trailSpaceRotOffset[i]), trailStartRotations[i], lerpRotationSpeed * deltaTime);
			trails[i].localRotation = quaternion;
			Vector3 vector4 = -trails[i].TransformDirection(trailSpaceForward[i]) * distances[i] * scale + vector;
			trails[i].position = vector4;
			prevPositions[i] = vector4;
			prevRotation[i] = quaternion;
			prevUpDirection[i] = trails[i].TransformDirection(trailSpaceUp[i]);
			vector = vector4;
		}
	}

	public void SetEnabled(bool state)
	{
		if (state == base.enabled)
		{
			return;
		}
		if (initialized)
		{
			if (state)
			{
				InitializeSegments();
			}
			else
			{
				ResetSegments();
			}
		}
		base.enabled = state;
	}

	private float GetScale()
	{
		if (!(rootTransform == null))
		{
			return rootTransform.localScale.x;
		}
		return 1f;
	}

	private static Vector3 LerpAngle(Vector3 a, Vector3 b, Vector3 t)
	{
		Vector3 result = default(Vector3);
		result.x = Mathf.LerpAngle(a.x, b.x, t.x);
		result.y = Mathf.LerpAngle(a.y, b.y, t.y);
		result.z = Mathf.LerpAngle(a.z, b.z, t.z);
		return result;
	}

	public void OnDrawGizmos()
	{
		if (debugDraw && initialized)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(rootSegment.position, 0.5f);
			Vector3 position = rootSegment.position;
			for (int i = 0; i < trails.Length; i++)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere(trails[i].position, 0.5f);
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(position, trails[i].position);
				position = trails[i].position;
			}
		}
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
		vectorPool.Return(trailStartPositions);
		vectorPool.Return(prevPositions);
		vectorPool.Return(prevUpDirection);
		vectorPool.Return(trailSpaceForward);
		vectorPool.Return(trailSpaceUp);
		vectorPool.Return(rotationMultipliers);
		quatPool.Return(prevRotation);
		quatPool.Return(trailStartRotations);
		quatPool.Return(trailSpaceRotOffset);
		floatPool.Return(distances);
		BehaviourUpdateUtils.Deregister(this);
	}
}
