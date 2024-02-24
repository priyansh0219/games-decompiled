using UnityEngine;

public class VFXPrecursorGunElevator : MonoBehaviour
{
	public Color wallLightsColor;

	public float wallLightsDistance = 35f;

	public float pointLightIntensity = 2f;

	[AssertNotNull]
	public MeshRenderer[] wallLightsRenderers;

	[AssertNotNull]
	public Light pointLight;

	[AssertNotNull]
	public Transform jointTransform;

	[AssertNotNull]
	public VFXController fxControl;

	private float pointLightAnimtTime;

	private float wallLightsAnimtTime;

	private bool pointLightIsFadingIn;

	private bool matsInstanciated;

	private bool wallLightsFadingOut;

	private MaterialPropertyBlock materialPropertyBlock;

	private void UpdateWallLights()
	{
		if (wallLightsFadingOut)
		{
			for (int i = 0; i < wallLightsRenderers.Length; i++)
			{
				MeshRenderer obj = wallLightsRenderers[i];
				obj.GetPropertyBlock(materialPropertyBlock);
				Color color = materialPropertyBlock.GetColor(ShaderPropertyID._Color);
				color = Color.Lerp(color, Color.clear, wallLightsAnimtTime);
				materialPropertyBlock.SetColor(ShaderPropertyID._Color, color);
				obj.SetPropertyBlock(materialPropertyBlock);
			}
			wallLightsAnimtTime += Time.deltaTime;
			if (wallLightsAnimtTime > 1f)
			{
				base.enabled = false;
				return;
			}
		}
		else
		{
			Vector3 localPlayerPos = Utils.GetLocalPlayerPos();
			for (int j = 0; j < wallLightsRenderers.Length; j++)
			{
				MeshRenderer obj2 = wallLightsRenderers[j];
				float num = Mathf.Abs(obj2.transform.position.y - localPlayerPos.y) / wallLightsDistance;
				Color value = Color.Lerp(t: Mathf.Clamp01(num * 5f), a: wallLightsColor, b: Color.clear);
				obj2.GetPropertyBlock(materialPropertyBlock);
				materialPropertyBlock.SetColor(ShaderPropertyID._Color, value);
				obj2.SetPropertyBlock(materialPropertyBlock);
			}
		}
		matsInstanciated = true;
	}

	private void UpdatePointLight()
	{
		pointLight.transform.position = new Vector3(pointLight.transform.position.x, jointTransform.position.y - 1.5f, pointLight.transform.position.z);
		if (pointLightIsFadingIn)
		{
			pointLightAnimtTime += Time.deltaTime * 2f;
		}
		else
		{
			pointLightAnimtTime -= Time.deltaTime;
		}
		pointLight.intensity = Mathf.Clamp01(pointLightAnimtTime) * pointLightIntensity;
	}

	private void Update()
	{
		UpdateWallLights();
		UpdatePointLight();
	}

	public void OnGunElevatorStart()
	{
		Debug.Log("Start Elevator Cinematic");
		wallLightsFadingOut = false;
		for (int i = 0; i < wallLightsRenderers.Length; i++)
		{
			wallLightsRenderers[i].gameObject.SetActive(value: true);
		}
		base.enabled = true;
	}

	public void OnPlayerCinematicModeEnd()
	{
		Debug.Log("Stop Elevator Cinematic");
		wallLightsFadingOut = true;
		pointLightAnimtTime = 1f;
		pointLightIsFadingIn = false;
	}

	public void OnGunElevatorAscendStart()
	{
		pointLight.gameObject.SetActive(value: true);
		pointLightAnimtTime = 0f;
		pointLightIsFadingIn = true;
		fxControl.Play(0);
	}

	public void OnGunElevatorDecendStart()
	{
		pointLight.gameObject.SetActive(value: true);
		pointLightAnimtTime = 0f;
		pointLightIsFadingIn = true;
	}

	private void Awake()
	{
		materialPropertyBlock = new MaterialPropertyBlock();
	}

	private void OnDisable()
	{
		wallLightsAnimtTime = 0f;
		wallLightsFadingOut = false;
		for (int i = 0; i < wallLightsRenderers.Length; i++)
		{
			wallLightsRenderers[i].gameObject.SetActive(value: false);
		}
		pointLight.gameObject.SetActive(value: false);
	}
}
