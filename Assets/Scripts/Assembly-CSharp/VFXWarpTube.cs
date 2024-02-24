using System.Collections.Generic;
using UnityEngine;

public class VFXWarpTube : MonoBehaviour
{
	[SerializeField]
	[AssertNotNull]
	private Renderer render;

	private List<FlashingLightHelpers.ShaderVector4ScalerToken> textureSpeedTokens;

	private void Awake()
	{
		MiscSettings.isFlashesEnabled.changedEvent.AddHandler(this, OnFlashesEnabled);
		textureSpeedTokens = FlashingLightHelpers.CreateShaderVector4ScalerTokens(render.materials[0], render.materials[1]);
		textureSpeedTokens.AddProperty(ShaderPropertyID._MainSpeed);
		textureSpeedTokens.AddProperty(ShaderPropertyID._DetailsSpeed);
		textureSpeedTokens.AddProperty(ShaderPropertyID._DeformSpeed);
		textureSpeedTokens.AddProperty(ShaderPropertyID._SinWaveSpeed);
		UpdateSpeed();
	}

	private void OnDestroy()
	{
		MiscSettings.isFlashesEnabled.changedEvent.RemoveHandler(this, OnFlashesEnabled);
	}

	private void OnFlashesEnabled(Utils.MonitoredValue<bool> parms)
	{
		UpdateSpeed();
	}

	private void UpdateSpeed()
	{
		if (MiscSettings.flashes)
		{
			textureSpeedTokens.RestoreScale();
		}
		else
		{
			textureSpeedTokens.SetScale(0.05f);
		}
	}
}
