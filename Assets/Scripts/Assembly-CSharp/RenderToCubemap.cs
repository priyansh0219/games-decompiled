using UnityEngine;

public class RenderToCubemap : MonoBehaviour
{
	public delegate void CapureDoneAction();

	public Camera camera;

	public Cubemap cubemap;

	public int faceSize = 512;

	public TextureFormat texFormat = TextureFormat.RGBAFloat;

	private RenderTexture rt;

	private Rect prevRect;

	private float prevFov;

	private float prevRatio;

	private Quaternion prevRot;

	private bool isRendering;

	private int currentFace = -1;

	private Quaternion[] rotations = new Quaternion[6]
	{
		Quaternion.Euler(-90f, 0f, 0f),
		Quaternion.Euler(0f, 90f, 0f),
		Quaternion.Euler(0f, 0f, 0f),
		Quaternion.Euler(90f, 0f, 0f),
		Quaternion.Euler(0f, -90f, 0f),
		Quaternion.Euler(0f, 180f, 0f)
	};

	private CubemapFace[] faces = new CubemapFace[6]
	{
		CubemapFace.PositiveY,
		CubemapFace.PositiveX,
		CubemapFace.PositiveZ,
		CubemapFace.NegativeY,
		CubemapFace.NegativeX,
		CubemapFace.NegativeZ
	};

	private RenderTexture[] allFacesRT;

	private Texture2D[] allFaces;

	private Rect captureZone;

	public static event CapureDoneAction OnCaptureDone;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination);
		if (currentFace != -1)
		{
			Graphics.Blit(source, rt);
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = rt;
			captureZone = new Rect(0f, 0f, faceSize, faceSize);
			allFaces[currentFace] = new Texture2D(Mathf.RoundToInt(captureZone.width), Mathf.RoundToInt(captureZone.height), texFormat, mipChain: false, linear: true);
			allFaces[currentFace].name = "RenderToCubemap";
			allFaces[currentFace].anisoLevel = 9;
			allFaces[currentFace].filterMode = FilterMode.Point;
			allFaces[currentFace].wrapMode = TextureWrapMode.Clamp;
			allFaces[currentFace].ReadPixels(captureZone, 0, 0, recalculateMipMaps: false);
			allFaces[currentFace].Apply();
			RenderTexture.active = active;
			currentFace++;
			if (currentFace > 5)
			{
				currentFace = -1;
				FacesToCubemap();
				RevertCameraSettings();
			}
			else
			{
				camera.transform.rotation = rotations[currentFace];
			}
		}
	}

	private Texture2D Resize(Texture2D sourceTex, int Width, int Height)
	{
		Texture2D texture2D = new Texture2D(Width, Height, sourceTex.format, mipChain: false, linear: true);
		texture2D.name = "RenderToCubemapResize";
		texture2D.anisoLevel = 9;
		texture2D.filterMode = FilterMode.Point;
		Color[] array = new Color[Width * Height];
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				float u = (float)j * 1f / (float)Width;
				float v = (float)i * 1f / (float)Height;
				array[i * Width + j] = sourceTex.GetPixelBilinear(u, v);
			}
		}
		texture2D.SetPixels(array);
		texture2D.Apply();
		return texture2D;
	}

	private Texture2D Flip(Texture2D sourceTex)
	{
		Texture2D texture2D = new Texture2D(sourceTex.width, sourceTex.height, sourceTex.format, mipChain: false, linear: true);
		texture2D.name = "RenderToCubemapFlip";
		texture2D.anisoLevel = 9;
		texture2D.filterMode = FilterMode.Point;
		for (int i = 0; i < sourceTex.height; i++)
		{
			for (int j = 0; j < sourceTex.width; j++)
			{
				Color pixel = sourceTex.GetPixel(sourceTex.width + j, sourceTex.height - 1 - i);
				texture2D.SetPixel(j, i, pixel);
			}
		}
		return texture2D;
	}

	public static Texture2D CropTexture(Texture2D originalTexture, Rect cropRect)
	{
		cropRect.x = Mathf.Clamp(cropRect.x, 0f, originalTexture.width);
		cropRect.width = Mathf.Clamp(cropRect.width, 0f, (float)originalTexture.width - cropRect.x);
		cropRect.y = Mathf.Clamp(cropRect.y, 0f, originalTexture.height);
		cropRect.height = Mathf.Clamp(cropRect.height, 0f, (float)originalTexture.height - cropRect.y);
		if (cropRect.height <= 0f || cropRect.width <= 0f)
		{
			return null;
		}
		Texture2D texture2D = new Texture2D((int)cropRect.width, (int)cropRect.height, originalTexture.format, mipChain: false, linear: true);
		texture2D.name = "RenderToCubemapCrop";
		texture2D.anisoLevel = 9;
		texture2D.filterMode = FilterMode.Point;
		Color[] pixels = originalTexture.GetPixels((int)cropRect.x, (int)cropRect.y, (int)cropRect.width, (int)cropRect.height, 0);
		texture2D.SetPixels(pixels);
		texture2D.Apply();
		return texture2D;
	}

	private void FacesToCubemap()
	{
		Texture2D texture2D = new Texture2D(faceSize, faceSize, texFormat, mipChain: false, linear: true);
		texture2D.name = "FacesToCubemap";
		texture2D.anisoLevel = 9;
		texture2D.filterMode = FilterMode.Point;
		texture2D.wrapMode = TextureWrapMode.Clamp;
		cubemap = new Cubemap(faceSize, texFormat, mipChain: false);
		for (int i = 0; i < 6; i++)
		{
			texture2D = allFaces[i];
			texture2D = Resize(texture2D, faceSize, faceSize);
			texture2D = Flip(texture2D);
			Color[] pixels = texture2D.GetPixels();
			cubemap.SetPixels(pixels, faces[i], 0);
			Object.Destroy(allFacesRT[i]);
		}
		cubemap.Apply();
		if (RenderToCubemap.OnCaptureDone != null)
		{
			RenderToCubemap.OnCaptureDone();
		}
	}

	public void RenderCubemap()
	{
		if (camera != null)
		{
			prevRect = camera.rect;
			prevFov = camera.fieldOfView;
			prevRot = camera.transform.rotation;
			camera.fieldOfView = 90f;
			prevRatio = camera.aspect;
			camera.aspect = 1f;
			camera.transform.rotation = rotations[0];
			currentFace = 0;
			allFacesRT = new RenderTexture[6];
			allFaces = new Texture2D[6];
			RenderTextureFormat format = ((texFormat == TextureFormat.RGBAFloat) ? PlatformUtils.defaultHDRFormat : RenderTextureFormat.ARGB32);
			rt = new RenderTexture(faceSize, faceSize, 0, format);
			rt.name = "RenderCubemap";
			rt.anisoLevel = 9;
			rt.filterMode = FilterMode.Point;
		}
	}

	private void RevertCameraSettings()
	{
		camera.fieldOfView = prevFov;
		camera.aspect = prevRatio;
		isRendering = false;
		currentFace = -1;
		camera.transform.rotation = prevRot;
		Object.Destroy(this);
	}
}
