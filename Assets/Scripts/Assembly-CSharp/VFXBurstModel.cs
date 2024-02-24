using UWE;
using UnityEngine;

public class VFXBurstModel : MonoBehaviour
{
	public Texture displaceTex;

	public float displaceStrength = 5f;

	private MeshRenderer[] meshRenderers;

	private SkinnedMeshRenderer[] skinRenderers;

	private void ApplyShaderToMat(Material mat)
	{
		mat.EnableKeyword("FX_BURST");
		mat.SetTexture(ShaderPropertyID._DispTex, displaceTex);
		mat.SetFloat(ShaderPropertyID._Displacement, displaceStrength);
		mat.SetFloat(ShaderPropertyID._startTime, Time.fixedTime);
	}

	private void OnKill()
	{
		meshRenderers = GetComponentsInChildren<MeshRenderer>();
		UWE.Utils.SetCollidersEnabled(base.gameObject, enabled: false);
		Object.Destroy(base.gameObject, 1f);
		for (int i = 0; i < meshRenderers.Length; i++)
		{
			Material[] materials = meshRenderers[i].materials;
			for (int j = 0; j < materials.Length; j++)
			{
				ApplyShaderToMat(materials[j]);
			}
			meshRenderers[i].materials = materials;
		}
		skinRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
		for (int k = 0; k < skinRenderers.Length; k++)
		{
			Material[] materials2 = skinRenderers[k].materials;
			for (int l = 0; l < materials2.Length; l++)
			{
				ApplyShaderToMat(materials2[l]);
			}
			skinRenderers[k].materials = materials2;
		}
	}

	private void OnDestroy()
	{
		if (meshRenderers != null)
		{
			for (int i = 0; i < meshRenderers.Length; i++)
			{
				for (int j = 0; j < meshRenderers[i].materials.Length; j++)
				{
					Object.Destroy(meshRenderers[i].materials[j]);
				}
			}
		}
		if (skinRenderers == null)
		{
			return;
		}
		for (int k = 0; k < skinRenderers.Length; k++)
		{
			for (int l = 0; l < skinRenderers[k].materials.Length; l++)
			{
				Object.Destroy(skinRenderers[k].materials[l]);
			}
		}
	}
}
