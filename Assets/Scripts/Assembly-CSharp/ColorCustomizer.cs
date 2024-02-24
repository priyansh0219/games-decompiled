using System;
using UnityEngine;

public class ColorCustomizer : MonoBehaviour
{
	public enum ColorType
	{
		Main = 0,
		BlueStripe = 1,
		YellowStripe = 2
	}

	[Serializable]
	public struct ColorData
	{
		public Renderer renderer;

		public int materialIndex;

		private RendererMaterialsStorage rendererMaterialsStorage;

		public bool IsValid
		{
			get
			{
				if (renderer != null)
				{
					return rendererMaterialsStorage != null;
				}
				return false;
			}
		}

		public ColorData(Renderer renderer, int materialIndex)
		{
			this.renderer = renderer;
			this.materialIndex = materialIndex;
			rendererMaterialsStorage = null;
		}

		public void Init(object owner)
		{
			if (renderer != null)
			{
				rendererMaterialsStorage = RendererMaterialsStorageManager.GetRendererMaterialsStorage(renderer, owner);
			}
		}

		public Material GetCopiedMaterial()
		{
			return rendererMaterialsStorage.GetOrCreateCopiedMaterial(materialIndex);
		}

		public Material GetInitialMaterial()
		{
			return rendererMaterialsStorage.GetInitialMaterial(materialIndex);
		}

		public void DestroyCopiedAndRestoreInitialMaterials(object owner)
		{
			if (rendererMaterialsStorage != null)
			{
				RendererMaterialsStorageManager.TryDestroyCopiedAndRestoreInitialMaterials(rendererMaterialsStorage, owner);
			}
		}
	}

	private readonly Color specularDark = new Color(0.39f, 0.39f, 0.39f, 1f);

	private readonly Color specularLight = new Color(1f, 1f, 1f, 1f);

	public static readonly Color defaultMainColor = Color.white;

	public static readonly Color defaultStripe1Color = new Color(1f / 3f, 0.47843137f, 0.6039216f, 1f);

	public static readonly Color defaultStripe2Color = new Color(82f / 85f, 63f / 85f, 0.2627451f, 1f);

	public static readonly Color defaultInteriorColor = Color.white;

	public ColorData[] colorDatas;

	public bool isBase = true;

	public bool initializeFromParent;

	private void Awake()
	{
		for (int i = 0; i < colorDatas.Length; i++)
		{
			colorDatas[i].Init(this);
		}
	}

	public void Start()
	{
		if (!initializeFromParent)
		{
			return;
		}
		ICustomizeable componentInParent = GetComponentInParent<ICustomizeable>();
		if (componentInParent != null)
		{
			Vector3[] colors = componentInParent.GetColors();
			if (colors.Length != 0)
			{
				SetMainColor(colors[0]);
			}
			if (colors.Length > 1)
			{
				SetStripe1Color(colors[1]);
			}
			if (colors.Length > 2)
			{
				SetStripe2Color(colors[2]);
			}
		}
	}

	private void OnDestroy()
	{
		ColorData[] array = colorDatas;
		foreach (ColorData colorData in array)
		{
			colorData.DestroyCopiedAndRestoreInitialMaterials(this);
		}
	}

	private static Color EnsureMinColor(Color color)
	{
		color.r = Mathf.Max(color.r, 0.18f);
		color.g = Mathf.Max(color.g, 0.18f);
		color.b = Mathf.Max(color.b, 0.18f);
		return color;
	}

	public void SetColors(Color main, Color stripe1, Color stripe2)
	{
		if (!isBase)
		{
			stripe2 = EnsureMinColor(stripe2);
		}
		ColorData[] array = colorDatas;
		for (int i = 0; i < array.Length; i++)
		{
			ColorData colorData = array[i];
			if (colorData.IsValid)
			{
				Material obj = (Application.isPlaying ? colorData.GetCopiedMaterial() : colorData.GetInitialMaterial());
				float luminance = GetLuminance(main);
				Color b = Color.Lerp(specularDark, specularLight, luminance);
				obj.SetColor(ShaderPropertyID._Color, main);
				obj.SetColor(ShaderPropertyID._SpecColor, Color.Lerp(main, b, 1f - 0.5f * luminance));
				luminance = GetLuminance(stripe1);
				b = Color.Lerp(specularDark, specularLight, luminance);
				obj.SetColor(ShaderPropertyID._Color2, stripe1);
				obj.SetColor(ShaderPropertyID._SpecColor2, Color.Lerp(stripe1, b, 1f - 0.5f * luminance));
				luminance = GetLuminance(stripe2);
				b = Color.Lerp(specularDark, specularLight, luminance);
				obj.SetColor(ShaderPropertyID._Color3, stripe2);
				obj.SetColor(ShaderPropertyID._SpecColor3, Color.Lerp(stripe2, b, 1f - 0.5f * luminance));
			}
		}
	}

	public void SetMainColor(Vector3 hsb)
	{
		SetMainColor(uGUI_ColorPicker.HSBToColor(hsb));
	}

	public void SetMainColor(Color color)
	{
		ColorData[] array = colorDatas;
		for (int i = 0; i < array.Length; i++)
		{
			ColorData colorData = array[i];
			if (colorData.IsValid)
			{
				Material obj = (Application.isPlaying ? colorData.GetCopiedMaterial() : colorData.GetInitialMaterial());
				float luminance = GetLuminance(color);
				Color b = Color.Lerp(specularDark, specularLight, luminance);
				obj.SetColor(ShaderPropertyID._Color, color);
				obj.SetColor(ShaderPropertyID._SpecColor, Color.Lerp(color, b, 1f - 0.5f * luminance));
			}
		}
	}

	public void SetStripe1Color(Vector3 hsb)
	{
		SetStripe1Color(uGUI_ColorPicker.HSBToColor(hsb));
	}

	public void SetStripe1Color(Color color)
	{
		ColorData[] array = colorDatas;
		for (int i = 0; i < array.Length; i++)
		{
			ColorData colorData = array[i];
			if (colorData.IsValid)
			{
				Material obj = (Application.isPlaying ? colorData.GetCopiedMaterial() : colorData.GetInitialMaterial());
				float luminance = GetLuminance(color);
				Color b = Color.Lerp(specularDark, specularLight, luminance);
				obj.SetColor(ShaderPropertyID._Color2, color);
				obj.SetColor(ShaderPropertyID._SpecColor2, Color.Lerp(color, b, 1f - 0.5f * luminance));
			}
		}
	}

	public void SetStripe2Color(Vector3 hsb)
	{
		SetStripe2Color(uGUI_ColorPicker.HSBToColor(hsb));
	}

	public void SetStripe2Color(Color color)
	{
		if (!isBase)
		{
			color = EnsureMinColor(color);
		}
		ColorData[] array = colorDatas;
		for (int i = 0; i < array.Length; i++)
		{
			ColorData colorData = array[i];
			if (colorData.IsValid)
			{
				Material obj = (Application.isPlaying ? colorData.GetCopiedMaterial() : colorData.GetInitialMaterial());
				float luminance = GetLuminance(color);
				Color b = Color.Lerp(specularDark, specularLight, luminance);
				obj.SetColor(ShaderPropertyID._Color3, color);
				obj.SetColor(ShaderPropertyID._SpecColor3, Color.Lerp(color, b, 1f - 0.5f * luminance));
			}
		}
	}

	public void SetDefaultColors()
	{
		SetColors(defaultMainColor, defaultStripe1Color, defaultStripe2Color);
	}

	private float GetLuminance(Color color)
	{
		return 0.299f * color.r + 0.587f * color.g + 0.114f * color.b;
	}
}
