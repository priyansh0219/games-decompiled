using System.Collections.Generic;
using UnityEngine;

public static class FlashingLightHelpers
{
	public class ShaderVector4ScalerToken
	{
		private struct Data
		{
			public int propertyId;

			public Vector4 defaultValue;
		}

		private Material material;

		private List<Data> properties;

		public ShaderVector4ScalerToken(Material material)
		{
			this.material = material;
			properties = new List<Data>();
		}

		public void AddProperty(int propertyId)
		{
			properties.Add(new Data
			{
				propertyId = propertyId,
				defaultValue = material.GetVector(propertyId)
			});
		}

		public void SetScale(float scale)
		{
			for (int i = 0; i < properties.Count; i++)
			{
				Data data = properties[i];
				material.SetVector(data.propertyId, data.defaultValue * scale);
			}
		}

		public void RestoreScale()
		{
			SetScale(1f);
		}
	}

	private const float defaultIntensitySpeed = 0.3f;

	private const float defaultMovementSpeed = 0.25f;

	public static ShaderVector4ScalerToken CreateUberShaderVector4ScalerToken(Material material)
	{
		ShaderVector4ScalerToken shaderVector4ScalerToken = new ShaderVector4ScalerToken(material);
		shaderVector4ScalerToken.AddProperty(ShaderPropertyID._MainTex_Speed);
		shaderVector4ScalerToken.AddProperty(ShaderPropertyID._MainTex2_Speed);
		shaderVector4ScalerToken.AddProperty(ShaderPropertyID._DeformMap_Speed);
		return shaderVector4ScalerToken;
	}

	public static List<ShaderVector4ScalerToken> CreateUberShaderVector4ScalerTokens(params Material[] materials)
	{
		List<ShaderVector4ScalerToken> list = new List<ShaderVector4ScalerToken>();
		foreach (Material material in materials)
		{
			list.Add(CreateUberShaderVector4ScalerToken(material));
		}
		return list;
	}

	public static List<ShaderVector4ScalerToken> CreateShaderVector4ScalerTokens(params Material[] materials)
	{
		List<ShaderVector4ScalerToken> list = new List<ShaderVector4ScalerToken>();
		foreach (Material material in materials)
		{
			list.Add(new ShaderVector4ScalerToken(material));
		}
		return list;
	}

	public static void AddProperty(this List<ShaderVector4ScalerToken> tokens, int propertyId)
	{
		for (int i = 0; i < tokens.Count; i++)
		{
			tokens[i].AddProperty(propertyId);
		}
	}

	public static void SetScale(this List<ShaderVector4ScalerToken> tokens, float scale)
	{
		for (int i = 0; i < tokens.Count; i++)
		{
			tokens[i].SetScale(scale);
		}
	}

	public static void RestoreScale(this List<ShaderVector4ScalerToken> tokens)
	{
		for (int i = 0; i < tokens.Count; i++)
		{
			tokens[i].RestoreScale();
		}
	}

	public static ShaderVector4ScalerToken CreateTeleportShaderVector4ScalerToken(Material material)
	{
		ShaderVector4ScalerToken shaderVector4ScalerToken = new ShaderVector4ScalerToken(material);
		shaderVector4ScalerToken.AddProperty(ShaderPropertyID._MainScrollSpeed);
		shaderVector4ScalerToken.AddProperty(ShaderPropertyID._DetailScrollSpeed);
		shaderVector4ScalerToken.AddProperty(ShaderPropertyID._NervesScrollSpeed);
		return shaderVector4ScalerToken;
	}

	public static void SafeIntensityChangePerFrame(Light light, float intensity, float epilepticsSpeed = 0.3f)
	{
		if (MiscSettings.flashes)
		{
			light.intensity = intensity;
		}
		else
		{
			light.intensity = LimitValueChangePreFrame(light.intensity, intensity, epilepticsSpeed);
		}
	}

	public static void SafeRangeChangePreFrame(Light light, float range, float epilepticsSpeed = 0.3f)
	{
		if (MiscSettings.flashes)
		{
			light.range = range;
		}
		else
		{
			light.range = LimitValueChangePreFrame(light.range, range, epilepticsSpeed);
		}
	}

	public static void SafePositionChangePreFrame(Transform transform, Vector3 position, float epilepticsSpeed = 0.25f)
	{
		if (MiscSettings.flashes)
		{
			transform.position = position;
		}
		else
		{
			transform.position = LimitPositionChangePreFrame(transform.position, position, epilepticsSpeed);
		}
	}

	private static float LimitValueChangePreFrame(float current, float target, float speed)
	{
		float num = Mathf.Abs(target - current);
		if (num > 0f)
		{
			return Mathf.Lerp(current, target, Time.deltaTime * speed / num);
		}
		return target;
	}

	private static Vector3 LimitPositionChangePreFrame(Vector3 current, Vector3 target, float speed)
	{
		float magnitude = (target - current).magnitude;
		if (magnitude > 0f)
		{
			return Vector3.Lerp(current, target, Time.deltaTime * speed / magnitude);
		}
		return target;
	}
}
