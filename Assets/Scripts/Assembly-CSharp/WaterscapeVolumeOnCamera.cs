using UWE;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class WaterscapeVolumeOnCamera : ImageEffectWithEvents
{
	public static bool doWaterFogCullingAdjustments = true;

	public WaterscapeVolume settings;

	private Camera camera;

	private bool visible = true;

	private float[] cullingDistances;

	public void SetVisible(bool _visible)
	{
		visible = _visible;
	}

	public bool GetVisible()
	{
		return visible;
	}

	private void Awake()
	{
		camera = GetComponent<Camera>();
		camera.depthTextureMode |= DepthTextureMode.Depth;
		cullingDistances = new float[32];
	}

	private void OnPreCull()
	{
		if (base.enabled)
		{
			if (GetShouldRender())
			{
				settings.PreRender(camera);
				_ = doWaterFogCullingAdjustments;
				ResetClippingPlane();
			}
			else
			{
				ResetClippingPlane();
			}
		}
	}

	private void OnPostRender()
	{
		settings.PostRender(camera);
	}

	private void ResetClippingPlane()
	{
		cullingDistances[0] = 0f;
		camera.layerCullDistances = cullingDistances;
	}

	public override bool CheckResources()
	{
		return true;
	}

	private bool GetShouldRender()
	{
		if (visible)
		{
			return ((1 << settings.gameObject.layer) & camera.cullingMask) != 0;
		}
		return false;
	}

	[ImageEffectOpaque]
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		using (new OnRenderImageWrapper(this, source, destination))
		{
			if (GetShouldRender())
			{
				bool cameraInside = Player.main != null && Player.main.IsInsideWalkable();
				if (MainCameraControl.main != null && MainCameraControl.main.GetComponent<FreecamController>().GetActive())
				{
					cameraInside = false;
				}
				settings.RenderImage(camera, cameraInside, source, destination);
			}
			else
			{
				Graphics.Blit(source, destination);
			}
		}
	}
}
