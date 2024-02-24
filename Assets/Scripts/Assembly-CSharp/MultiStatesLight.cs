using System;
using UnityEngine;

[Serializable]
public class MultiStatesLight
{
	public Light light;

	public float[] intensities = new float[3] { 1f, 1f, 1f };

	private float startIntensity = 1f;

	public void InitLerp()
	{
		if (light != null)
		{
			startIntensity = light.intensity;
		}
	}

	private void ToggleLightOnIntensity()
	{
		if (light.intensity > 0f && !light.gameObject.activeSelf)
		{
			light.gameObject.SetActive(value: true);
		}
		else if (light.intensity == 0f && light.gameObject.activeSelf)
		{
			light.gameObject.SetActive(value: false);
		}
	}

	public void UpdateIntensity(int targetState, float scalar)
	{
		if (light != null && intensities != null && targetState < intensities.Length)
		{
			light.intensity = Mathf.Lerp(startIntensity, intensities[targetState], scalar);
			ToggleLightOnIntensity();
		}
	}

	public void SetIntensity(int targetState)
	{
		if (light != null && intensities != null && targetState < intensities.Length)
		{
			light.intensity = intensities[targetState];
			ToggleLightOnIntensity();
		}
	}
}
