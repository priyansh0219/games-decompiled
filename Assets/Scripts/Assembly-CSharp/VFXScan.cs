using UnityEngine;

public class VFXScan : MonoBehaviour
{
	private bool scanActive;

	private float timeScanStarted;

	private float scanDuration = 1f;

	public Texture _EmissiveTex;

	private Renderer[] renderers;

	public void StartScan(float duration)
	{
		scanActive = true;
		scanDuration = duration;
		timeScanStarted = Time.time;
		renderers = base.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
		for (int i = 0; i < renderers.Length; i++)
		{
			Material[] materials = renderers[i].materials;
			foreach (Material material in materials)
			{
				if (material != null && material.shader != null)
				{
					material.EnableKeyword("FX_BUILDING");
					material.SetTexture(ShaderPropertyID._EmissiveTex, _EmissiveTex);
					material.SetFloat(ShaderPropertyID._Cutoff, 0.4f);
					material.SetColor(ShaderPropertyID._BorderColor, new Color(0.7f, 0.7f, 1f, 1f));
					material.SetFloat(ShaderPropertyID._Built, 0f);
					material.SetFloat(ShaderPropertyID._Cutoff, 0.42f);
					material.SetVector(ShaderPropertyID._BuildParams, new Vector4(2f, 0.7f, 3f, -0.25f));
					material.SetFloat(ShaderPropertyID._NoiseStr, 0.25f);
					material.SetFloat(ShaderPropertyID._NoiseThickness, 0.49f);
					material.SetFloat(ShaderPropertyID._BuildLinear, 1f);
					material.SetFloat(ShaderPropertyID._MyCullVariable, 0f);
				}
			}
		}
	}

	private void RestoreMaterials()
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			for (int j = 0; j < renderers[i].materials.Length; j++)
			{
				Material material = renderers[i].materials[j];
				if (material != null)
				{
					material.DisableKeyword("FX_BUILDING");
				}
			}
		}
	}

	public float GetCurrentYPos()
	{
		VFXFabricating componentInChildren = GetComponentInChildren<VFXFabricating>();
		if (scanActive)
		{
			float num = Mathf.Clamp01((Time.time - timeScanStarted) / scanDuration);
			return componentInChildren.minY + num * (componentInChildren.maxY - componentInChildren.minY);
		}
		return componentInChildren.maxY;
	}

	private void Update()
	{
		if (!scanActive)
		{
			return;
		}
		VFXFabricating componentInChildren = GetComponentInChildren<VFXFabricating>();
		for (int i = 0; i < renderers.Length; i++)
		{
			for (int j = 0; j < renderers[i].materials.Length; j++)
			{
				Material material = renderers[i].materials[j];
				if (material != null)
				{
					material.SetFloat(ShaderPropertyID._Built, Mathf.Clamp01((Time.time - timeScanStarted) / scanDuration));
					material.SetFloat(ShaderPropertyID._minYpos, componentInChildren.minY);
					material.SetFloat(ShaderPropertyID._maxYpos, componentInChildren.maxY);
				}
			}
		}
		if (timeScanStarted + scanDuration <= Time.time)
		{
			RestoreMaterials();
			scanActive = false;
		}
	}

	private void OnDestroy()
	{
		if (renderers == null)
		{
			return;
		}
		for (int i = 0; i < renderers.Length; i++)
		{
			Renderer renderer = renderers[i];
			if (!(renderer != null))
			{
				continue;
			}
			for (int j = 0; j < renderer.materials.Length; j++)
			{
				Material material = renderer.materials[j];
				if (material != null)
				{
					Object.Destroy(material);
				}
			}
		}
	}
}
