using System;
using UnityEngine;

[Serializable]
public class WaterCausticsGenerator
{
	public Shader tilingShader;

	private Material tilingMaterial;

	public Shader updateCausticsShader;

	private Material updateCausticsMaterial;

	public float causticsSize = 10f;

	public int causticsTextureSize = 256;

	[Tooltip("Distance below the water surface where the caustics are computed")]
	public float causticsFloorDepth = 2f;

	[Range(1f, 2f)]
	public float causticsRefractionIndex = 1.33f;

	public float causticsColorDispersion;

	public RenderTexture causticsTexture;

	private Mesh causticsMesh;

	public void Create()
	{
		tilingMaterial = new Material(tilingShader);
		updateCausticsMaterial = new Material(updateCausticsShader);
		causticsMesh = CreateLatticeMesh(255, -0.3f, 1.3f);
		causticsTexture = new RenderTexture(causticsTextureSize, causticsTextureSize, 0, RenderTextureFormat.ARGBHalf);
		causticsTexture.antiAliasing = 8;
		causticsTexture.wrapMode = TextureWrapMode.Repeat;
		causticsTexture.autoGenerateMips = true;
		causticsTexture.useMipMap = true;
		causticsTexture.name = "Caustics";
	}

	private static Mesh CreateLatticeMesh(int sideNumVertices, float minValue = 0f, float maxValue = 1f)
	{
		Mesh mesh = new Mesh();
		Vector3[] array = new Vector3[sideNumVertices * sideNumVertices];
		for (int i = 0; i < sideNumVertices; i++)
		{
			for (int j = 0; j < sideNumVertices; j++)
			{
				int num = j + i * sideNumVertices;
				array[num].x = minValue + (maxValue - minValue) * (float)j / ((float)sideNumVertices - 1f);
				array[num].y = 0f;
				array[num].z = minValue + (maxValue - minValue) * (float)i / ((float)sideNumVertices - 1f);
			}
		}
		mesh.vertices = array;
		int[] array2 = new int[(sideNumVertices - 1) * (sideNumVertices - 1) * 2 * 3];
		for (int k = 0; k < sideNumVertices - 1; k++)
		{
			for (int l = 0; l < sideNumVertices - 1; l++)
			{
				int num2 = l + k * (sideNumVertices - 1);
				int num3 = (array2[num2 * 6 + 2] = l + k * sideNumVertices);
				array2[num2 * 6 + 1] = num3 + 1;
				array2[num2 * 6] = num3 + 1 + sideNumVertices;
				array2[num2 * 6 + 5] = num3;
				array2[num2 * 6 + 4] = num3 + 1 + sideNumVertices;
				array2[num2 * 6 + 3] = num3 + sideNumVertices;
			}
		}
		mesh.triangles = array2;
		return mesh;
	}

	private RenderTexture GetTilingTexture(RenderTexture srcTexture, float patchSize)
	{
		int num = Mathf.CeilToInt((float)srcTexture.width * causticsSize / patchSize);
		RenderTexture temporary = RenderTexture.GetTemporary(num, num, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		temporary.wrapMode = TextureWrapMode.Repeat;
		float value = (float)num / (float)srcTexture.width;
		tilingMaterial.SetPass(0);
		tilingMaterial.SetTexture(ShaderPropertyID._MainTex, srcTexture);
		tilingMaterial.SetFloat(ShaderPropertyID._WrapSize, value);
		Graphics.Blit(srcTexture, temporary, tilingMaterial);
		return temporary;
	}

	public void GenerateCaustics(RenderTexture fullDisplacementMap, RenderTexture fullNormalMap, float patchSize)
	{
		RenderTexture tilingTexture = GetTilingTexture(fullDisplacementMap, patchSize);
		RenderTexture tilingTexture2 = GetTilingTexture(fullNormalMap, patchSize);
		Graphics.SetRenderTarget(causticsTexture);
		GL.Clear(clearDepth: false, clearColor: true, Color.black);
		float num = causticsSize * 100f;
		updateCausticsMaterial.SetTexture(ShaderPropertyID._MainTex, tilingTexture);
		updateCausticsMaterial.SetFloat(ShaderPropertyID._WaterPatchLength, num);
		updateCausticsMaterial.SetTexture(ShaderPropertyID._NormalsTex, tilingTexture2);
		updateCausticsMaterial.SetFloat(ShaderPropertyID._TexelLength2, num / (float)tilingTexture.width * 2f);
		updateCausticsMaterial.SetFloat(ShaderPropertyID._ProjectionDepth, 0f - causticsFloorDepth);
		if (causticsColorDispersion > 0f)
		{
			updateCausticsMaterial.SetFloat(ShaderPropertyID._RefractionIndex, causticsRefractionIndex);
			updateCausticsMaterial.SetVector(ShaderPropertyID._WaveLength, new Vector3(1f, 0f, 0f));
			updateCausticsMaterial.SetPass(0);
			Graphics.DrawMeshNow(causticsMesh, Matrix4x4.identity);
			updateCausticsMaterial.SetFloat(ShaderPropertyID._RefractionIndex, causticsRefractionIndex + causticsColorDispersion);
			updateCausticsMaterial.SetVector(ShaderPropertyID._WaveLength, new Vector3(0f, 1f, 0f));
			updateCausticsMaterial.SetPass(0);
			Graphics.DrawMeshNow(causticsMesh, Matrix4x4.identity);
			updateCausticsMaterial.SetFloat(ShaderPropertyID._RefractionIndex, causticsRefractionIndex + causticsColorDispersion * 2f);
			updateCausticsMaterial.SetVector(ShaderPropertyID._WaveLength, new Vector3(0f, 0f, 1f));
			updateCausticsMaterial.SetPass(0);
			Graphics.DrawMeshNow(causticsMesh, Matrix4x4.identity);
		}
		else
		{
			updateCausticsMaterial.SetFloat(ShaderPropertyID._RefractionIndex, causticsRefractionIndex);
			updateCausticsMaterial.SetVector(ShaderPropertyID._WaveLength, new Vector3(1f, 1f, 1f));
			updateCausticsMaterial.SetPass(0);
			Graphics.DrawMeshNow(causticsMesh, Matrix4x4.identity);
		}
		RenderTexture.ReleaseTemporary(tilingTexture);
		RenderTexture.ReleaseTemporary(tilingTexture2);
	}

	public RenderTexture GetCausticsTexture()
	{
		return causticsTexture;
	}

	public int GetCausticsTextureSize()
	{
		return causticsTextureSize;
	}

	public float GetCausticsSize()
	{
		return causticsSize;
	}
}
