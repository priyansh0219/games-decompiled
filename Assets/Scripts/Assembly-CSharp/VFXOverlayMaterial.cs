using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class VFXOverlayMaterial : MonoBehaviour
{
	private struct TrackedRenderer
	{
		public Renderer renderer;

		public int numSubMeshes;
	}

	public Material debugMat;

	private List<TrackedRenderer> trackedRenderers = new List<TrackedRenderer>();

	private float duration = -1f;

	private float lerpValue;

	private bool destroyMaterial;

	private Color initColor;

	private Color startColor;

	private Color targetColor;

	public Material material;

	private bool destroyOnFadeComplete;

	private void Update()
	{
		if (material != null && duration > 0f)
		{
			lerpValue += Time.deltaTime / duration;
			if (lerpValue > 1f)
			{
				Object.Destroy(this);
			}
			else
			{
				material.color = Color.Lerp(startColor, targetColor, lerpValue);
			}
		}
	}

	private void SetRenderers(Renderer[] rends)
	{
		trackedRenderers.Clear();
		TrackedRenderer item = default(TrackedRenderer);
		foreach (Renderer renderer in rends)
		{
			if (renderer.GetComponent<WaterClipProxy>() != null || renderer.GetComponent<VFXIgnoreOverlayMaterial>() != null)
			{
				continue;
			}
			item.renderer = renderer;
			if (renderer.GetType() == typeof(SkinnedMeshRenderer))
			{
				SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)renderer;
				if (skinnedMeshRenderer.sharedMesh != null)
				{
					item.numSubMeshes = skinnedMeshRenderer.sharedMesh.subMeshCount;
					trackedRenderers.Add(item);
				}
			}
			else
			{
				MeshFilter component = renderer.GetComponent<MeshFilter>();
				if (component != null && component.sharedMesh != null)
				{
					item.numSubMeshes = component.sharedMesh.subMeshCount;
					trackedRenderers.Add(item);
				}
			}
		}
	}

	public void OnDestroy()
	{
		WBOIT.UnregisterOverlay(this);
		if (destroyMaterial)
		{
			Object.Destroy(material);
		}
	}

	public void OnDisable()
	{
		WBOIT.UnregisterOverlay(this);
		trackedRenderers.Clear();
	}

	public bool FillBuffer(CommandBuffer buffer)
	{
		bool result = false;
		for (int num = trackedRenderers.Count - 1; num >= 0; num--)
		{
			TrackedRenderer trackedRenderer = trackedRenderers[num];
			Renderer renderer = trackedRenderer.renderer;
			if (renderer == null)
			{
				trackedRenderers.RemoveAt(num);
			}
			else if (renderer.gameObject.activeInHierarchy && renderer.isVisible)
			{
				int i = 0;
				for (int numSubMeshes = trackedRenderer.numSubMeshes; i < numSubMeshes; i++)
				{
					buffer.DrawRenderer(renderer, material, i, -1);
					result = true;
				}
			}
		}
		return result;
	}

	public void ApplyAndForgetOverlay(Material mat, string debugName, Color lerpToColor, float lifeTime, Renderer[] rends = null)
	{
		duration = lifeTime;
		startColor = mat.color;
		targetColor = lerpToColor;
		ApplyOverlay(mat, debugName, instantiateMaterial: true, rends);
	}

	public void ApplyOverlay(Material mat, string debugName, bool instantiateMaterial, Renderer[] rends = null)
	{
		if (rends == null)
		{
			rends = GetComponentsInChildren<Renderer>();
		}
		SetRenderers(rends);
		initColor = mat.color;
		if (instantiateMaterial)
		{
			material = new Material(mat);
			destroyMaterial = true;
		}
		else
		{
			material = mat;
		}
		WBOIT.RegisterOverlay(this);
	}

	public void RemoveOverlay()
	{
		Object.Destroy(this);
	}

	public void SkipTime(float seconds)
	{
		if (material != null && duration > 0f)
		{
			lerpValue += seconds / duration;
		}
	}

	[ContextMenu("ApplyDebugMaterial")]
	public void ApplyDebugMaterial()
	{
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
		ApplyOverlay(debugMat, "debugMat", instantiateMaterial: false, componentsInChildren);
	}
}
