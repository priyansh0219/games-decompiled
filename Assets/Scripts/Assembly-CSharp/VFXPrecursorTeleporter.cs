using UnityEngine;

public class VFXPrecursorTeleporter : MonoBehaviour
{
	[AssertNotNull]
	public Renderer portalRenderer;

	[AssertNotNull]
	public Light portalLight;

	public VFXController fxControl;

	public float fadeInDuration = 1.5f;

	public float fadeOutDuration = 0.5f;

	private float fadefactor = 1f;

	private int fadeState;

	private int radialFadeID;

	private float defaultScrollSpeed;

	private float defaultRotationSpeed;

	private void Awake()
	{
		MiscSettings.isFlashesEnabled.changedEvent.AddHandler(this, OnFlashesEnabled);
		defaultScrollSpeed = portalRenderer.material.GetFloat(ShaderPropertyID._ScrollSpeed);
		defaultRotationSpeed = portalRenderer.material.GetFloat(ShaderPropertyID._RotationSpeed);
		UpdateSpeed();
	}

	private void Start()
	{
		radialFadeID = Shader.PropertyToID("_RadialFade");
	}

	public void Update()
	{
		float num = ((fadeState == 1) ? fadeInDuration : fadeOutDuration);
		fadefactor += (float)fadeState * Time.deltaTime / num;
		if (fadefactor < 0f)
		{
			fadeState = 0;
			portalLight.gameObject.SetActive(value: false);
			portalRenderer.gameObject.SetActive(value: false);
		}
		else if (fadefactor > 1f)
		{
			fadeState = 0;
		}
		fadefactor = Mathf.Clamp01(fadefactor);
		FlashingLightHelpers.SafeIntensityChangePerFrame(portalLight, portalLight.intensity * fadefactor);
		portalRenderer.material.SetFloat(radialFadeID, fadefactor);
	}

	public void FadeIn()
	{
		if (fxControl != null)
		{
			fxControl.Play(0);
		}
		portalLight.gameObject.SetActive(value: true);
		portalRenderer.gameObject.SetActive(value: true);
		fadeState = 1;
	}

	public void FadeOut()
	{
		fadeState = -1;
	}

	public void Toggle(bool open)
	{
		fadeState = 0;
		fadefactor = (open ? 1f : 0f);
		portalLight.gameObject.SetActive(open);
		portalRenderer.gameObject.SetActive(open);
		FlashingLightHelpers.SafeIntensityChangePerFrame(portalLight, portalLight.intensity * fadefactor);
		portalRenderer.material.SetFloat(radialFadeID, fadefactor);
	}

	private void OnDestroy()
	{
		Object.Destroy(portalRenderer.material);
		MiscSettings.isFlashesEnabled.changedEvent.RemoveHandler(this, OnFlashesEnabled);
	}

	private void OnFlashesEnabled(Utils.MonitoredValue<bool> isFlashesEnabled)
	{
		UpdateSpeed();
	}

	private void UpdateSpeed()
	{
		if (MiscSettings.flashes)
		{
			SetSpeedScale(1f);
		}
		else
		{
			SetSpeedScale(0.2f);
		}
	}

	private void SetSpeedScale(float scale)
	{
		Material material = portalRenderer.material;
		material.SetFloat(ShaderPropertyID._ScrollSpeed, defaultScrollSpeed * scale);
		material.SetFloat(ShaderPropertyID._RotationSpeed, defaultRotationSpeed * scale);
	}
}
