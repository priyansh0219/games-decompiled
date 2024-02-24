using UnityEngine;

public class VFXSpotlight : MonoBehaviour
{
	public enum InitialState
	{
		None = 0,
		Active = 1,
		Inactive = 2
	}

	[Tooltip("The state this should enter in Start. None will make no change to the state of the object.")]
	public InitialState initialState;

	[Tooltip("The color that _GlowColor will be set to when this is set to inactive.")]
	public Color inactiveColor = Color.black;

	public Renderer coneRenderer;

	public Renderer[] modelRenderers;

	private float lightIntensity = 1f;

	private Color[] initialModelColors;

	private Color initialConeColor;

	private bool initialized;

	private void Start()
	{
		switch (initialState)
		{
		case InitialState.Active:
			SetLightActive(active: true);
			break;
		case InitialState.Inactive:
			SetLightActive(active: false);
			break;
		}
	}

	private void Initialize()
	{
		if (initialized)
		{
			return;
		}
		initialModelColors = new Color[modelRenderers.Length];
		for (int i = 0; i < modelRenderers.Length; i++)
		{
			if (modelRenderers[i] != null)
			{
				initialModelColors[i] = modelRenderers[i].material.GetColor(ShaderPropertyID._GlowColor);
			}
		}
		if (coneRenderer != null)
		{
			initialConeColor = coneRenderer.sharedMaterial.GetColor(ShaderPropertyID._Color);
		}
		initialized = true;
	}

	public void SetLightActive(bool active)
	{
		Initialize();
		for (int i = 0; i < modelRenderers.Length; i++)
		{
			if (modelRenderers[i] != null)
			{
				Color value = (active ? initialModelColors[i] : inactiveColor);
				modelRenderers[i].material.SetColor(ShaderPropertyID._GlowColor, value);
			}
		}
	}
}
