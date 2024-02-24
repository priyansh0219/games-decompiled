using System;
using UnityEngine;

public class OptimizedLight : MonoBehaviour
{
	public Light light;

	private LightShadows originalSetting;

	private void Start()
	{
		originalSetting = light.shadows;
		GraphicsUtil.onQualityLevelChanged = (GraphicsUtil.OnQualityLevelChanged)Delegate.Combine(GraphicsUtil.onQualityLevelChanged, new GraphicsUtil.OnQualityLevelChanged(OnQualityLevelChanged));
		UpdateForQuality();
	}

	private void OnQualityLevelChanged()
	{
		UpdateForQuality();
	}

	private void UpdateForQuality()
	{
		if (TryGetComponent<Light>(out var component))
		{
			if (QualitySettings.shadows == ShadowQuality.Disable)
			{
				component.shadows = LightShadows.None;
			}
			else
			{
				component.shadows = originalSetting;
			}
		}
	}

	private void OnDestroy()
	{
		GraphicsUtil.onQualityLevelChanged = (GraphicsUtil.OnQualityLevelChanged)Delegate.Remove(GraphicsUtil.onQualityLevelChanged, new GraphicsUtil.OnQualityLevelChanged(OnQualityLevelChanged));
	}
}
