using System.Collections.Generic;
using UWE;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class GammaCorrection : MonoBehaviour
{
	public const float defaultGamma = 1f;

	public const float minGamma = 0.1f;

	public const float maxGamma = 2.8f;

	private static float _gamma = 1f;

	private static readonly List<GammaCorrection> list = new List<GammaCorrection>();

	private Material material;

	public static float gamma
	{
		get
		{
			return _gamma;
		}
		set
		{
			value = Mathf.Clamp(value, 0.1f, 2.8f);
			if (!Mathf.Approximately(_gamma, value))
			{
				_gamma = value;
				UpdateGamma();
			}
		}
	}

	private void Awake()
	{
		material = new Material(ShaderManager.preloadedShaders.GammaCorrection);
		list.Add(this);
		UpdateGamma();
	}

	private void OnDestroy()
	{
		list.Remove(this);
		UWE.Utils.DestroyWrap(material);
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		Graphics.Blit(src, dst, material);
	}

	private static void UpdateGamma()
	{
		Shader.SetGlobalFloat(ShaderPropertyID._Gamma, _gamma);
		Shader.SetGlobalFloat(ShaderPropertyID._InverseGamma, 1f / _gamma);
		bool flag = !Mathf.Approximately(_gamma, 1f);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].enabled = flag;
		}
	}
}
