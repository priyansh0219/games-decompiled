using System;
using UnityEngine;

[DisallowMultipleComponent]
public class VFXMaterialsFlashesSlowdown : MonoBehaviour
{
	[Serializable]
	public class MaterialConfig
	{
		public Renderer renderer;

		public int materialIndex;

		public string[] shaderPropertiesVector4;
	}

	[SerializeField]
	private MaterialConfig[] materialConfigs;

	[SerializeField]
	private float slowdownScale = 0.01f;

	private FlashingLightHelpers.ShaderVector4ScalerToken[] tokens;

	private void Awake()
	{
		tokens = new FlashingLightHelpers.ShaderVector4ScalerToken[materialConfigs.Length];
		for (int i = 0; i < materialConfigs.Length; i++)
		{
			MaterialConfig materialConfig = materialConfigs[i];
			FlashingLightHelpers.ShaderVector4ScalerToken shaderVector4ScalerToken = new FlashingLightHelpers.ShaderVector4ScalerToken(materialConfig.renderer.sharedMaterials[materialConfig.materialIndex]);
			for (int j = 0; j < materialConfig.shaderPropertiesVector4.Length; j++)
			{
				int propertyId = Shader.PropertyToID(materialConfig.shaderPropertiesVector4[j]);
				shaderVector4ScalerToken.AddProperty(propertyId);
				tokens[i] = shaderVector4ScalerToken;
			}
		}
		MiscSettings.isFlashesEnabled.changedEvent.AddHandler(this, OnFlashesEnabled);
		UpdateSpeed();
	}

	private void OnDestroy()
	{
		MiscSettings.isFlashesEnabled.changedEvent.RemoveHandler(this, OnFlashesEnabled);
	}

	private void UpdateSpeed()
	{
		float scale = (MiscSettings.flashes ? 1f : slowdownScale);
		for (int i = 0; i < tokens.Length; i++)
		{
			tokens[i].SetScale(scale);
		}
	}

	private void OnFlashesEnabled(Utils.MonitoredValue<bool> isFlashesEnabled)
	{
		UpdateSpeed();
	}
}
