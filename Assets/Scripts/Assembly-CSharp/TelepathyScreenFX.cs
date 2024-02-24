using UnityEngine;

public class TelepathyScreenFX : MonoBehaviour
{
	[AssertNotNull]
	public Material mat;

	public float amount;

	[AssertNotNull]
	public GameObject leviathanGhostPrefab;

	public Vector3 posOffset = new Vector3(0.205f, -2.6f, 3f);

	public Vector3 eulerOffset = new Vector3(0f, 90f, 0f);

	public bool showGhostModel;

	public float rotationSpeed = 1f;

	public bool isFinalSequence;

	private static GameObject ghostGO;

	private static Renderer ghostRenderer;

	private Color[] initGhostColors;

	private Vector3 angleOffset = Vector3.zero;

	private float eulerAnim = 10f;

	private float eulerDuration = 0.1f;

	private Vector3 prevTargetEuler = Vector3.zero;

	private Vector3 targetEuler = Vector3.zero;

	private FlashingLightHelpers.ShaderVector4ScalerToken textureSpeedToken;

	private float defaultFlowIntensity;

	private float defaultEyesFresnelFade;

	private void Awake()
	{
		MiscSettings.isFlashesEnabled.changedEvent.AddHandler(this, OnFlashesEnabled);
		textureSpeedToken = new FlashingLightHelpers.ShaderVector4ScalerToken(mat);
		textureSpeedToken.AddProperty(ShaderPropertyID._MainScrollSpeed);
		textureSpeedToken.AddProperty(ShaderPropertyID._DetailScrollSpeed);
		textureSpeedToken.AddProperty(ShaderPropertyID._RefractScrollSpeed);
		textureSpeedToken.AddProperty(ShaderPropertyID._ColorCycleSpeed);
		defaultFlowIntensity = mat.GetFloat(ShaderPropertyID._FlowIntensity);
		UpdateSpeed();
	}

	private void OnDestroy()
	{
		MiscSettings.isFlashesEnabled.changedEvent.RemoveHandler(this, OnFlashesEnabled);
	}

	private void SpawnGhost()
	{
		ghostGO = Utils.SpawnZeroedAt(leviathanGhostPrefab, base.transform);
		ghostGO.transform.localPosition = posOffset;
		ghostGO.transform.localEulerAngles = eulerOffset;
		ghostRenderer = ghostGO.GetComponent<Renderer>();
		initGhostColors = new Color[ghostRenderer.materials.Length];
		for (int i = 0; i < ghostRenderer.materials.Length; i++)
		{
			initGhostColors[i] = ghostRenderer.materials[i].GetColor(ShaderPropertyID._Color);
		}
		UpdateGhostMaterials();
		targetEuler = eulerOffset + new Vector3(Random.Range(-20f, 20f), Random.Range(-25f, 25f), Random.Range(-20f, 10f));
		defaultEyesFresnelFade = ghostRenderer.materials[2].GetFloat(ShaderPropertyID._FresnelFade);
		UpdateSpeed();
	}

	private void UpdateGhostMaterials()
	{
		if (!(ghostGO == null) && !(ghostRenderer == null))
		{
			float t = amount;
			if (isFinalSequence)
			{
				t = Mathf.Clamp01(amount * 2f);
			}
			for (int i = 0; i < ghostRenderer.materials.Length; i++)
			{
				Color value = Color.Lerp(Color.clear, initGhostColors[i], t);
				ghostRenderer.materials[i].SetColor(ShaderPropertyID._Color, value);
			}
		}
	}

	private void Update()
	{
		if (!(ghostGO == null))
		{
			eulerAnim += Time.deltaTime / eulerDuration * rotationSpeed;
			if (eulerAnim > 1f)
			{
				eulerDuration = Random.Range(0.5f, 1f);
				eulerAnim = 0f;
				prevTargetEuler = targetEuler;
				targetEuler = eulerOffset + new Vector3(Random.Range(-20f, 20f), Random.Range(-25f, 25f), Random.Range(-20f, 10f));
			}
			ghostGO.transform.localEulerAngles = Vector3.Lerp(prevTargetEuler, targetEuler, Mathf.SmoothStep(0f, 1f, eulerAnim));
		}
	}

	private void OnPreRender()
	{
		if (amount <= 0f)
		{
			if (ghostGO != null)
			{
				for (int i = 0; i < ghostRenderer.materials.Length; i++)
				{
					Object.Destroy(ghostRenderer.materials[i]);
				}
			}
			Object.Destroy(ghostGO);
			base.enabled = false;
		}
		else if (ghostGO == null && showGhostModel)
		{
			SpawnGhost();
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		UpdateGhostMaterials();
		mat.SetFloat(ShaderPropertyID._Amount, showGhostModel ? amount : (amount * 0.75f));
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
			mat.SetFloat(ShaderPropertyID._FlowIntensity, defaultFlowIntensity);
			SetEyesFresnelFadeScale(1f);
		}
		else
		{
			textureSpeedToken.SetScale(0.1f);
			mat.SetFloat(ShaderPropertyID._FlowIntensity, defaultFlowIntensity * 0.01f);
			SetEyesFresnelFadeScale(0.05f);
		}
	}

	private void SetEyesFresnelFadeScale(float scale)
	{
		if (ghostRenderer != null)
		{
			ghostRenderer.materials[2].SetFloat(ShaderPropertyID._FresnelFade, defaultEyesFresnelFade * scale);
		}
	}
}
