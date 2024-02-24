using System;
using UnityEngine;

public class CullingOccluder : MonoBehaviour
{
	public MeshFilter meshFilter;

	public Renderer occluderRenderer;

	[NonSerialized]
	public int occluderListPosition = -1;

	private Bounds cachedWorldBounds;

	public Bounds worldBounds => cachedWorldBounds;

	private void Start()
	{
		cachedWorldBounds = occluderRenderer.bounds;
	}

	private void OnEnable()
	{
		if (CullingCamera.main != null)
		{
			CullingCamera.main.RegisterOccluder(this);
		}
	}

	private void OnDisable()
	{
		if (CullingCamera.main != null)
		{
			CullingCamera.main.DeregisterOccluder(this);
		}
	}

	private void OnDestroy()
	{
		if (CullingCamera.main != null)
		{
			CullingCamera.main.DeregisterOccluder(this);
		}
	}
}
