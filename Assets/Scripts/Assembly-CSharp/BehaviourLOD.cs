using System;
using UnityEngine;

[DisallowMultipleComponent]
public class BehaviourLOD : MonoBehaviour
{
	private static Vector3 cachedPlayerPos;

	private static Vector3 cachedCameraFwd;

	private static int cachedFrameId;

	private Renderer[] renderers;

	public GameObject visibilityRendererRoot;

	public float veryCloseThreshold = 10f;

	public float closeThreshold = 50f;

	public float farThreshold = 500f;

	private float veryCloseThresholdSq;

	private float closeThresholdSq;

	private float farThresholdSq;

	private float nextDistanceUpdateTime;

	private float timeBetweenDistanceUpdates = 1f;

	private bool useBoundsForDistanceChecks;

	private Bounds lodBounds;

	public Renderer boundsRenderer;

	private bool useBoundingBoxForVisibility;

	public float distToPlayerSq { get; private set; }

	public LODState current { get; private set; }

	public bool UseBoundingBoxForVisibility
	{
		get
		{
			return useBoundingBoxForVisibility;
		}
		set
		{
			useBoundingBoxForVisibility = value;
		}
	}

	public Vector3 LODCenter
	{
		get
		{
			if (useBoundsForDistanceChecks)
			{
				return lodBounds.center;
			}
			return base.transform.position;
		}
	}

	public bool IsFull()
	{
		return current == LODState.Full;
	}

	public bool IsMinimal()
	{
		return current == LODState.Minimal;
	}

	private void Awake()
	{
		veryCloseThresholdSq = veryCloseThreshold * veryCloseThreshold;
		closeThresholdSq = closeThreshold * closeThreshold;
		farThresholdSq = farThreshold * farThreshold;
		RefreshRenderers();
		nextDistanceUpdateTime = Time.time + UnityEngine.Random.value * timeBetweenDistanceUpdates;
		current = LODState.Full;
	}

	public void RefreshRenderers()
	{
		if (useBoundingBoxForVisibility)
		{
			renderers = null;
		}
		if (visibilityRendererRoot == null)
		{
			visibilityRendererRoot = base.gameObject;
		}
		renderers = visibilityRendererRoot.GetComponentsInChildren<Renderer>();
	}

	private static void UpdateCachedData()
	{
		if (Time.frameCount != cachedFrameId)
		{
			cachedFrameId = Time.frameCount;
			cachedPlayerPos = Player.main.transform.position;
			cachedCameraFwd = MainCamera.camera.transform.forward;
		}
	}

	private float GetDistanceToPlayerSquared()
	{
		if (useBoundsForDistanceChecks)
		{
			return lodBounds.SqrDistance(cachedPlayerPos);
		}
		return (LODCenter - cachedPlayerPos).sqrMagnitude;
	}

	public void SetUseBoundsForDistanceChecks(Bounds bounds)
	{
		lodBounds = bounds;
		useBoundsForDistanceChecks = true;
	}

	private void Update()
	{
		UpdateCachedData();
		if (boundsRenderer != null)
		{
			SetUseBoundsForDistanceChecks(boundsRenderer.bounds);
		}
		if (Time.time > nextDistanceUpdateTime)
		{
			for (distToPlayerSq = GetDistanceToPlayerSquared(); nextDistanceUpdateTime < Time.time; nextDistanceUpdateTime += timeBetweenDistanceUpdates)
			{
			}
		}
		if (distToPlayerSq < veryCloseThresholdSq)
		{
			current = LODState.Full;
		}
		else if (distToPlayerSq < closeThresholdSq)
		{
			if (CheckVisibility())
			{
				current = LODState.Full;
			}
			else
			{
				current = LODState.Minimal;
			}
		}
		else if (distToPlayerSq < farThresholdSq)
		{
			if (CheckVisibility())
			{
				current = LODState.Medium;
			}
			else
			{
				current = LODState.Minimal;
			}
		}
		else
		{
			current = LODState.Minimal;
		}
	}

	private bool CheckBoundingBoxVisibility()
	{
		try
		{
			Vector3 rhs = lodBounds.center - cachedPlayerPos;
			rhs.Normalize();
			float num = Vector3.Dot(cachedCameraFwd, rhs);
			if (num < 0f)
			{
				return false;
			}
			float num2 = Mathf.Cos(MainCamera.camera.fieldOfView * ((float)Math.PI / 180f));
			if (num > num2)
			{
				return true;
			}
			Vector3 pos = lodBounds.center + lodBounds.extents;
			if (IsWithinThresh(pos, num2))
			{
				return true;
			}
			Vector3 pos2 = lodBounds.center - lodBounds.extents;
			if (IsWithinThresh(pos2, num2))
			{
				return true;
			}
			Vector3 pos3 = lodBounds.center + new Vector3(lodBounds.extents.x, lodBounds.extents.y, 0f - lodBounds.extents.z);
			if (IsWithinThresh(pos3, num2))
			{
				return true;
			}
			Vector3 pos4 = lodBounds.center + new Vector3(lodBounds.extents.x, 0f - lodBounds.extents.y, lodBounds.extents.z);
			if (IsWithinThresh(pos4, num2))
			{
				return true;
			}
			Vector3 pos5 = lodBounds.center + new Vector3(lodBounds.extents.x, 0f - lodBounds.extents.y, 0f - lodBounds.extents.z);
			if (IsWithinThresh(pos5, num2))
			{
				return true;
			}
			Vector3 pos6 = lodBounds.center + new Vector3(0f - lodBounds.extents.x, lodBounds.extents.y, lodBounds.extents.z);
			if (IsWithinThresh(pos6, num2))
			{
				return true;
			}
			Vector3 pos7 = lodBounds.center + new Vector3(0f - lodBounds.extents.x, lodBounds.extents.y, 0f - lodBounds.extents.z);
			if (IsWithinThresh(pos7, num2))
			{
				return true;
			}
			Vector3 pos8 = lodBounds.center + new Vector3(0f - lodBounds.extents.x, 0f - lodBounds.extents.y, lodBounds.extents.z);
			if (IsWithinThresh(pos8, num2))
			{
				return true;
			}
			return false;
		}
		finally
		{
		}
	}

	private bool IsWithinThresh(Vector3 pos, float dotThresh)
	{
		Vector3 rhs = pos - cachedPlayerPos;
		rhs.Normalize();
		return Vector3.Dot(cachedCameraFwd, rhs) > dotThresh;
	}

	private bool CheckVisibility()
	{
		if (useBoundingBoxForVisibility)
		{
			return CheckBoundingBoxVisibility();
		}
		if (renderers != null)
		{
			Renderer[] array = renderers;
			foreach (Renderer renderer in array)
			{
				if ((bool)renderer && renderer.isVisible)
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	[ContextMenu("HookupPrefabScripts")]
	private void HookupPrefabScripts()
	{
		AnimateByVelocity componentInChildren = base.gameObject.GetComponentInChildren<AnimateByVelocity>();
		if ((bool)componentInChildren)
		{
			componentInChildren.levelOfDetail = this;
		}
		TrailManager[] componentsInChildren = base.gameObject.GetComponentsInChildren<TrailManager>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].levelOfDetail = this;
		}
		Rotater[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<Rotater>(includeInactive: true);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].levelOfDetail = this;
		}
		SplineFollowing[] componentsInChildren3 = base.gameObject.GetComponentsInChildren<SplineFollowing>(includeInactive: true);
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			componentsInChildren3[i].levelOfDetail = this;
		}
		Locomotion[] componentsInChildren4 = base.gameObject.GetComponentsInChildren<Locomotion>(includeInactive: true);
		for (int i = 0; i < componentsInChildren4.Length; i++)
		{
			componentsInChildren4[i].levelOfDetail = this;
		}
	}
}
