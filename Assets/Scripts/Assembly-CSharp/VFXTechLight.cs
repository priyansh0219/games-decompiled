using UnityEngine;

public class VFXTechLight : MonoBehaviour
{
	public Light[] lights;

	public Renderer coneRenderer;

	public Renderer[] modelRenderers;

	public bool flicker;

	public bool placedByPlayer;

	public float minLightIntensity;

	public float maxLightIntensity = 0.7f;

	public float modelPow = 1f;

	public float minTime;

	public float maxTime = 0.03f;

	private float waittime;

	private float lightIntensity = 1f;

	private Color[] initialModelColors;

	private Color initialConeColor;

	private bool lightActive = true;

	private MaterialPropertyBlock coneRendererPropertyBlock;

	private MaterialPropertyBlock modelRendererPropertyBlock;

	private void UpdateLightFlickering()
	{
		if (waittime < 0f)
		{
			waittime = Random.Range(minTime, maxTime);
			float value = Random.value;
			lightIntensity = Mathf.SmoothStep(minLightIntensity, maxLightIntensity, value);
			float num = lightIntensity / maxLightIntensity;
			float num2 = Mathf.Pow(num, 1f / modelPow);
			for (int i = 0; i < lights.Length; i++)
			{
				lights[i].intensity = lightIntensity;
			}
			if (coneRenderer != null)
			{
				coneRenderer.GetPropertyBlock(coneRendererPropertyBlock, 0);
				coneRendererPropertyBlock.SetColor(ShaderPropertyID._Color, initialConeColor * num);
				coneRenderer.SetPropertyBlock(coneRendererPropertyBlock, 0);
			}
			for (int j = 0; j < modelRenderers.Length; j++)
			{
				Renderer renderer = modelRenderers[j];
				if (renderer != null)
				{
					renderer.GetPropertyBlock(modelRendererPropertyBlock, 0);
					modelRendererPropertyBlock.SetColor(ShaderPropertyID._GlowColor, initialModelColors[j] * num2);
					renderer.SetPropertyBlock(modelRendererPropertyBlock, 0);
				}
			}
		}
		waittime -= Time.deltaTime;
	}

	private void Awake()
	{
		initialModelColors = new Color[modelRenderers.Length];
		coneRendererPropertyBlock = new MaterialPropertyBlock();
		modelRendererPropertyBlock = new MaterialPropertyBlock();
		for (int i = 0; i < modelRenderers.Length; i++)
		{
			if (modelRenderers[i] != null)
			{
				initialModelColors[i] = modelRenderers[i].sharedMaterial.GetColor(ShaderPropertyID._GlowColor);
			}
		}
		if (coneRenderer != null)
		{
			initialConeColor = coneRenderer.sharedMaterial.GetColor(ShaderPropertyID._Color);
		}
		if (!(GetComponent<Constructable>() != null))
		{
			return;
		}
		for (int j = 0; j < modelRenderers.Length; j++)
		{
			Renderer renderer = modelRenderers[j];
			if (renderer != null)
			{
				renderer.GetPropertyBlock(modelRendererPropertyBlock, 0);
				modelRendererPropertyBlock.SetColor(ShaderPropertyID._GlowColor, Color.black);
				renderer.SetPropertyBlock(modelRendererPropertyBlock, 0);
			}
		}
	}

	private void Start()
	{
		if (placedByPlayer)
		{
			lightActive = false;
			SetPhysicalState(lightActive);
		}
		else
		{
			lightActive = true;
			SetPhysicalState(lightActive);
		}
	}

	private void Update()
	{
		if (lights != null && lights.Length != 0 && flicker && lightActive)
		{
			UpdateLightFlickering();
		}
	}

	private void SetPhysicalState(bool isOn)
	{
		if (coneRenderer != null)
		{
			coneRenderer.enabled = isOn;
		}
		for (int i = 0; i < modelRenderers.Length; i++)
		{
			Renderer renderer = modelRenderers[i];
			if (renderer != null)
			{
				renderer.GetPropertyBlock(modelRendererPropertyBlock, 0);
				Color value = (isOn ? initialModelColors[i] : Color.black);
				modelRendererPropertyBlock.SetColor(ShaderPropertyID._GlowColor, value);
				renderer.SetPropertyBlock(modelRendererPropertyBlock, 0);
			}
		}
	}

	public void SetLightOnOff(bool isOn)
	{
		if (lightActive != isOn)
		{
			lightActive = isOn;
			SetPhysicalState(lightActive);
		}
	}
}
