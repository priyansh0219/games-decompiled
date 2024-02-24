using System;
using UnityEngine;

public class CullingOccludee : MonoBehaviour
{
	public Renderer[] occludeeRenderers;

	public bool isStatic;

	[NonSerialized]
	public int computeBufferPosition = -1;

	[NonSerialized]
	public int usedListPosition = -1;

	[NonSerialized]
	public int dynamicUsedListPosition = -1;

	private bool occludeeVisible = true;

	private void OnEnable()
	{
		if (CullingCamera.main != null)
		{
			CullingCamera.main.RegisterOccludee(this);
		}
	}

	private void OnDisable()
	{
		if (CullingCamera.main != null)
		{
			CullingCamera.main.DeregisterOccludee(this);
		}
	}

	private void OnDestroy()
	{
		if (CullingCamera.main != null)
		{
			CullingCamera.main.DeregisterOccludee(this);
		}
	}

	public void GetWorldBounds(out Vector3 min, out Vector3 max)
	{
		Bounds bounds = occludeeRenderers[0].bounds;
		min = bounds.min;
		max = bounds.max;
		for (int i = 1; i < occludeeRenderers.Length; i++)
		{
			bounds = occludeeRenderers[i].bounds;
			min = Vector3.Min(min, bounds.min);
			max = Vector3.Max(max, bounds.max);
		}
	}

	public void SetVisible(bool visible)
	{
		if (visible != occludeeVisible)
		{
			occludeeVisible = visible;
			Renderer[] array = occludeeRenderers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = visible;
			}
		}
	}

	public bool GetVisible()
	{
		return occludeeVisible;
	}
}
