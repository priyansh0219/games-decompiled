using UnityEngine;

public class VFXExplosionPlayerFX : MonoBehaviour
{
	public bool PlayOnAwake = true;

	public bool destroyMaterial = true;

	public float duration = 5f;

	public AnimationCurve visbilityCurve;

	public AnimationCurve offsetSpeedCurve;

	public Vector3 offsetDirection = Vector3.zero;

	public Vector4 noiseOffset = Vector4.zero;

	public float noise4thDimensionSpeed = 0.1f;

	public GameObject wind;

	public float windDuration = 5f;

	public AnimationCurve screenFXcurve;

	public Transform crashedShipTrans;

	private float shipDistance;

	private int visibilityShaderID;

	private int offsetShaderID;

	private float animTime;

	private bool playing;

	private Material mat;

	private ExplosionScreenFX screenFX;

	public void Play()
	{
		animTime = 0f;
		playing = true;
		noiseOffset = Vector4.zero;
		SetMaterialProperties();
		GetComponent<Renderer>().enabled = true;
		Invoke("ActiveWind", 1f);
		screenFX = MainCamera.camera.GetComponent<ExplosionScreenFX>();
		if (screenFX != null)
		{
			screenFX.strength = 0.002f;
			screenFX.enabled = true;
		}
	}

	private void ActiveWind()
	{
		if (wind != null)
		{
			wind.SetActive(value: true);
		}
	}

	private void Awake()
	{
		GetComponent<Renderer>().enabled = false;
		mat = GetComponent<Renderer>().material;
		visibilityShaderID = Shader.PropertyToID("_Visibility");
		offsetShaderID = Shader.PropertyToID("_NoiseOffset");
	}

	private void Start()
	{
		if (PlayOnAwake)
		{
			UpdateDistanceAndVector();
			float time = shipDistance / 1000f;
			Invoke("DoPlay", time);
		}
	}

	private void DoPlay()
	{
		Play();
	}

	private void UpdateScreenFX()
	{
		if (screenFX != null)
		{
			screenFX.strength = screenFXcurve.Evaluate(animTime) * (1f - Mathf.Clamp(shipDistance / 4000f, 0f, 0.8f));
		}
	}

	private void UpdateDistanceAndVector()
	{
		if (crashedShipTrans != null)
		{
			Vector3 position = crashedShipTrans.position;
			Vector3 position2 = base.transform.position;
			shipDistance = Vector3.Distance(position, position2);
			offsetDirection = Vector3.Normalize(position - position2);
		}
	}

	private void SetMaterialProperties()
	{
		float f = visbilityCurve.Evaluate(animTime) + Mathf.Clamp(shipDistance / 500f, 0f, 100000f);
		float num = offsetSpeedCurve.Evaluate(animTime) * (2.5f - Mathf.Clamp(shipDistance / 1000f, 0.5f, 2.5f));
		Vector3 vector = num * offsetDirection;
		noiseOffset.x += vector.x;
		noiseOffset.y += vector.y;
		noiseOffset.z += vector.z;
		noiseOffset.w += num * noise4thDimensionSpeed;
		mat.SetFloat(visibilityShaderID, Mathf.Pow(f, 2f));
		mat.SetVector(offsetShaderID, noiseOffset);
	}

	private void Update()
	{
		if (!playing)
		{
			return;
		}
		animTime += Time.deltaTime / duration;
		if (animTime > 0.99f)
		{
			playing = false;
			return;
		}
		UpdateDistanceAndVector();
		SetMaterialProperties();
		UpdateScreenFX();
		base.transform.forward = -offsetDirection;
		if (animTime > windDuration / duration)
		{
			wind.SetActive(value: false);
		}
		if (!GetComponent<Renderer>().enabled)
		{
			GetComponent<Renderer>().enabled = true;
		}
	}

	private void OnDestroy()
	{
		if (destroyMaterial && animTime > 0f)
		{
			Object.DestroyImmediate(GetComponent<Renderer>().material);
		}
	}
}
