using Gendarme;
using UnityEngine;
using UnityEngine.XR;

[SuppressMessage("Gendarme.Rules.Concurrency", "NonConstantStaticFieldsShouldNotBeVisibleRule")]
public static class GraphicsUtil
{
	public delegate void OnQualityLevelChanged();

	public static int frameRateMin = 30;

	public static int frameRateMax = 500;

	private const bool isConsolePlatform = false;

	public const int frameRateDefault = 144;

	private static bool systemInfoCaptured = false;

	private static bool isOpenGL = false;

	private static bool isRadeon = false;

	public static OnQualityLevelChanged onQualityLevelChanged;

	private static void CaptureSystemInfo()
	{
		isOpenGL = SystemInfo.graphicsDeviceVersion.Contains("OpenGL");
		isRadeon = SystemInfo.graphicsDeviceName.Contains("Radeon");
		systemInfoCaptured = true;
	}

	public static bool IsOpenGL()
	{
		if (!systemInfoCaptured)
		{
			CaptureSystemInfo();
		}
		return isOpenGL;
	}

	public static bool IsRadeon()
	{
		if (!systemInfoCaptured)
		{
			CaptureSystemInfo();
		}
		return isRadeon;
	}

	public static bool GetUVStartsAtTop()
	{
		return !IsOpenGL();
	}

	public static Texture2D CopyRenderTextureToTexture(RenderTexture src)
	{
		RenderTexture active = RenderTexture.active;
		TextureFormat textureFormat = TextureFormat.ARGB32;
		switch (src.format)
		{
		case RenderTextureFormat.ARGB32:
			textureFormat = TextureFormat.ARGB32;
			break;
		case RenderTextureFormat.ARGBFloat:
			textureFormat = TextureFormat.RGBAFloat;
			break;
		case RenderTextureFormat.ARGBHalf:
			textureFormat = TextureFormat.RGBAHalf;
			break;
		}
		RenderTexture.active = src;
		Texture2D texture2D = new Texture2D(src.width, src.height, textureFormat, mipChain: false);
		texture2D.name = "GraphicsUtils.CopyRenderTextureToTexture";
		texture2D.ReadPixels(new Rect(0f, 0f, src.width, src.height), 0, 0);
		texture2D.Apply();
		RenderTexture.active = active;
		return texture2D;
	}

	public static Vector2Int GetScreenSize()
	{
		if (XRSettings.enabled)
		{
			return new Vector2Int(XRSettings.eyeTextureWidth, XRSettings.eyeTextureHeight);
		}
		return new Vector2Int(Screen.width, Screen.height);
	}

	public static bool GetVSyncEnabled()
	{
		return QualitySettings.vSyncCount != 0;
	}

	public static void SetVSyncEnabled(bool vsyncEnabled)
	{
		QualitySettings.vSyncCount = (vsyncEnabled ? 1 : 0);
	}

	public static int GetFrameRate()
	{
		return Application.targetFrameRate;
	}

	public static void SetFrameRate(int frameRate)
	{
		Application.targetFrameRate = Mathf.Clamp(frameRate, frameRateMin, frameRateMax);
	}

	[SuppressMessage("Subnautica.Rules", "AvoidDirectlySettingQuality")]
	public static void SetQualityLevel(int qualityLevel)
	{
		if (qualityLevel != QualitySettings.GetQualityLevel())
		{
			QualitySettings.SetQualityLevel(qualityLevel);
			if (onQualityLevelChanged != null)
			{
				onQualityLevelChanged();
			}
		}
	}

	public static Mesh CreateQuadMesh()
	{
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(-1f, -1f, 0f),
			new Vector3(1f, -1f, 0f),
			new Vector3(1f, 1f, 0f),
			new Vector3(-1f, 1f, 0f)
		};
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = new int[6] { 0, 1, 2, 0, 2, 3 };
		return mesh;
	}

	public static void GetClosestSupportedResolution(ref int xResolution, ref int yResolution)
	{
		Resolution desktopResolution = EditorModifications.desktopResolution;
		float num = (float)desktopResolution.width / (float)desktopResolution.height;
		float num2 = float.PositiveInfinity;
		int num3 = xResolution;
		int num4 = yResolution;
		Resolution[] resolutions = Screen.resolutions;
		for (int i = 0; i < resolutions.Length; i++)
		{
			Resolution resolution = resolutions[i];
			if (resolution.width == num3 && resolution.height == num4)
			{
				return;
			}
		}
		Resolution[] array = resolutions;
		for (int j = 0; j < array.Length; j++)
		{
			Resolution resolution2 = array[j];
			float num5 = (float)resolution2.width / (float)resolution2.height;
			float num6 = Mathf.Abs((float)resolution2.width - (float)num3) + Mathf.Abs(num5 - num) * 100f;
			if (num6 < num2)
			{
				num2 = num6;
				xResolution = resolution2.width;
				yResolution = resolution2.height;
			}
		}
	}
}
