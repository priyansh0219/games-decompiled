using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public static class MaterialExtensions
{
	public const string keywordAlphaPremultiply = "ALPHA_PREMULTIPLY";

	private static List<Material> materials = new List<Material>();

	private static int GetMaterialCount(Renderer renderer)
	{
		renderer.GetSharedMaterials(materials);
		int count = materials.Count;
		materials.Clear();
		return count;
	}

	public static Renderer[] AssignMaterial(GameObject go, Material material, bool includeDisabled = false)
	{
		Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>(includeDisabled);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			int materialCount = GetMaterialCount(componentsInChildren[i]);
			Material[] array = new Material[materialCount];
			for (int j = 0; j < materialCount; j++)
			{
				array[j] = material;
			}
			componentsInChildren[i].materials = array;
		}
		return componentsInChildren;
	}

	public static void SetColor(Renderer[] renderers, int propertyID, Color color)
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			if (renderers[i] != null)
			{
				renderers[i].GetMaterials(materials);
				for (int j = 0; j < materials.Count; j++)
				{
					materials[j].SetColor(propertyID, color);
				}
			}
		}
		materials.Clear();
	}

	public static void SetFloat(Renderer[] renderers, int propertyID, float value)
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			if (renderers[i] != null)
			{
				renderers[i].GetMaterials(materials);
				for (int j = 0; j < materials.Count; j++)
				{
					materials[j].SetFloat(propertyID, value);
				}
			}
		}
		materials.Clear();
	}

	public static void SetTexture(Renderer[] renderers, int propertyID, Texture texture)
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			if (renderers[i] != null)
			{
				renderers[i].GetMaterials(materials);
				for (int j = 0; j < materials.Count; j++)
				{
					materials[j].SetTexture(propertyID, texture);
				}
			}
		}
		materials.Clear();
	}

	public static void SetBlending(Material material, Blending blending, bool alphaPremultiply)
	{
		if (!(material == null))
		{
			switch (blending)
			{
			case Blending.Additive:
				SetBlendMode(material, BlendMode.One, BlendMode.One, alphaPremultiply);
				break;
			case Blending.Multiplicative:
				SetBlendMode(material, BlendMode.DstColor, BlendMode.OneMinusSrcAlpha, alphaPremultiply);
				break;
			default:
				SetBlendMode(material, BlendMode.SrcAlpha, BlendMode.OneMinusSrcAlpha, alphaPremultiply);
				break;
			}
		}
	}

	public static void SetBlendMode(Material material, BlendMode srcFactor, BlendMode dstFactor, bool alphaPremultiply)
	{
		if (!(material == null))
		{
			material.SetInt(ShaderPropertyID._SrcFactor, (int)srcFactor);
			material.SetInt(ShaderPropertyID._DstFactor, (int)dstFactor);
			SetKeyword(material, "ALPHA_PREMULTIPLY", alphaPremultiply);
		}
	}

	public static void SetKeyword(Material material, string keyword, bool state)
	{
		if (!(material == null) && material.IsKeywordEnabled(keyword) != state)
		{
			if (state)
			{
				material.EnableKeyword(keyword);
			}
			else
			{
				material.DisableKeyword(keyword);
			}
		}
	}

	public static bool GetBarValue(RectTransform rt, BaseEventData eventData, Material material, bool horizontal, out float v)
	{
		if (eventData is PointerEventData pointerEventData)
		{
			return GetBarValue(rt, pointerEventData.position, pointerEventData.pressEventCamera, material, horizontal, out v);
		}
		v = 0f;
		return false;
	}

	public static bool GetBarValue(RectTransform rt, Vector2 screenPoint, Camera camera, Material material, bool horizontal, out float v)
	{
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPoint, camera, out var localPoint))
		{
			Vector2 size = rt.rect.size;
			float @float = material.GetFloat(ShaderPropertyID._TopBorder);
			float float2 = material.GetFloat(ShaderPropertyID._BottomBorder);
			if (horizontal)
			{
				v = localPoint.x / size.x + rt.pivot.x;
			}
			else
			{
				v = localPoint.y / size.y + rt.pivot.y;
			}
			v = (v - float2) / (1f - @float - float2);
			v = Mathf.Clamp01(v);
			return true;
		}
		v = 0f;
		return false;
	}
}
