using UnityEngine;

[RequireComponent(typeof(Light))]
public class FadeLight : FadeLightBase
{
	private bool isPreviewing;

	private Color defaultColor = Color.white;

	private float defaultIntensity = -1f;

	private void Awake()
	{
		Light component = GetComponent<Light>();
		defaultColor = component.color;
		defaultIntensity = component.intensity;
	}

	public override void Fade(float fade)
	{
		Light component = GetComponent<Light>();
		component.intensity = defaultIntensity * Mathf.LinearToGammaSpace(fade);
		component.enabled = (double)fade > 0.0001;
	}

	public override Color EditorPreview(float fade, float dayLightScalar, Color color, float fraction)
	{
		Light component = GetComponent<Light>();
		if (!isPreviewing)
		{
			defaultColor = component.color;
			defaultIntensity = component.intensity;
		}
		component.color = Color.Lerp(defaultColor, color, fraction);
		component.intensity = defaultIntensity * Mathf.LinearToGammaSpace(fade);
		isPreviewing = true;
		return component.color;
	}

	public override void ResetPreview()
	{
		Light component = GetComponent<Light>();
		component.color = defaultColor;
		component.intensity = defaultIntensity;
		isPreviewing = false;
	}
}
