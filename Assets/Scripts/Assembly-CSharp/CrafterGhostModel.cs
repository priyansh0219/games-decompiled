using System.Collections;
using System.Collections.Generic;
using UWE;
using UnityEngine;

public sealed class CrafterGhostModel : MonoBehaviour
{
	private static List<Renderer> sGhostRenderers = new List<Renderer>();

	private const string dontRenderShaderName = "DontRender";

	public Transform itemSpawnPoint;

	public Texture _EmissiveTex;

	public Texture _NoiseTex;

	private GameObject ghostModel;

	private VFXFabricating boundsToVFX;

	private List<Material> ghostMaterials = new List<Material>();

	private bool ghostModelWasInactive;

	public void UpdateModel(TechType techType)
	{
		for (int num = ghostMaterials.Count - 1; num >= 0; num--)
		{
			Material material = ghostMaterials[num];
			if (material != null)
			{
				Object.Destroy(material);
			}
		}
		ghostMaterials.Clear();
		boundsToVFX = null;
		if (ghostModel != null)
		{
			Object.Destroy(ghostModel);
			ghostModel = null;
		}
		if (techType != 0)
		{
			StartCoroutine(SetupGhostModelAsync(techType));
		}
	}

	public void UpdateProgress(float progress)
	{
		if (ghostModel == null)
		{
			return;
		}
		float value = Mathf.Clamp01(progress);
		float value2 = 0f;
		float value3 = 0f;
		if (boundsToVFX != null)
		{
			value2 = boundsToVFX.minY;
			value3 = boundsToVFX.maxY;
		}
		for (int i = 0; i < ghostMaterials.Count; i++)
		{
			Material material = ghostMaterials[i];
			if (material.shader.name != "DontRender")
			{
				material.SetFloat(ShaderPropertyID._Built, value);
				material.SetFloat(ShaderPropertyID._minYpos, value2);
				material.SetFloat(ShaderPropertyID._maxYpos, value3);
			}
		}
	}

	private IEnumerator SetupGhostModelAsync(TechType techType)
	{
		bool isLootCube = false;
		CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType, verbose: false);
		yield return request;
		GameObject gameObject = request.GetResult();
		if (gameObject == null)
		{
			isLootCube = true;
		}
		if (isLootCube)
		{
			gameObject = Utils.genericLootPrefab;
		}
		Constructable component = gameObject.GetComponent<Constructable>();
		if (component != null && component.model != null)
		{
			ghostModel = Object.Instantiate(component.model);
		}
		else
		{
			VFXFabricating componentInChildren = gameObject.GetComponentInChildren<VFXFabricating>(includeInactive: true);
			if (componentInChildren != null)
			{
				ghostModel = Object.Instantiate(componentInChildren.gameObject);
			}
		}
		if (!(ghostModel != null))
		{
			yield break;
		}
		SkyApplier skyApplier = ghostModel.AddComponent<SkyApplier>();
		skyApplier.anchorSky = Skies.BaseInterior;
		ghostModel.SetActive(value: true);
		ghostModel.transform.parent = itemSpawnPoint;
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(ghostModel, isKinematic: true);
		UWE.Utils.ZeroTransform(ghostModel.transform);
		boundsToVFX = ghostModel.GetComponent<VFXFabricating>();
		if (boundsToVFX != null)
		{
			boundsToVFX.enabled = true;
			ghostModel.transform.localPosition = boundsToVFX.posOffset;
			ghostModel.transform.localEulerAngles = boundsToVFX.eulerOffset;
			ghostModel.transform.localScale *= boundsToVFX.scaleFactor;
		}
		ghostModel.GetComponentsInChildren(includeInactive: true, sGhostRenderers);
		skyApplier.renderers = sGhostRenderers.ToArray();
		for (int i = 0; i < sGhostRenderers.Count; i++)
		{
			Material[] materials = sGhostRenderers[i].materials;
			foreach (Material material in materials)
			{
				if (material != null)
				{
					ghostMaterials.Add(material);
					if (material.shader != null && material.shader.name != "DontRender")
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
		sGhostRenderers.Clear();
		Shader.SetGlobalFloat(ShaderPropertyID._SubConstructProgress, 0f);
	}
}
