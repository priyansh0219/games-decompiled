using UnityEngine;

[ExecuteInEditMode]
public class WarpScreenFX : MonoBehaviour
{
	public Material mat;

	public float amount;

	private FlashingLightHelpers.ShaderVector4ScalerToken textureSpeedToken;

	private void Awake()
	{
		MiscSettings.isFlashesEnabled.changedEvent.AddHandler(this, OnFlashesEnabled);
		textureSpeedToken = FlashingLightHelpers.CreateTeleportShaderVector4ScalerToken(mat);
		UpdateSpeed();
	}

	private void OnDestroy()
	{
		MiscSettings.isFlashesEnabled.changedEvent.RemoveHandler(this, OnFlashesEnabled);
	}

	private void Start()
	{
	}

	private void OnPreRender()
	{
		if (amount <= 0f)
		{
			base.enabled = false;
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		mat.SetFloat(ShaderPropertyID._Amount, amount);
		Graphics.Blit(source, destination, mat);
	}

	private void OnFlashesEnabled(Utils.MonitoredValue<bool> parms)
	{
		UpdateSpeed();
	}

	private void UpdateSpeed()
	{
		if (MiscSettings.flashes)
		{
			textureSpeedToken.RestoreScale();
		}
		else
		{
			textureSpeedToken.SetScale(0.1f);
		}
	}
}
