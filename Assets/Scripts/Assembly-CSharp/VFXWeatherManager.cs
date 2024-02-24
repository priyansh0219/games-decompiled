using UnityEngine;

public class VFXWeatherManager : MonoBehaviour
{
	public float cloudScalar;

	public float sunScalar;

	public float horizonFogScalar = 0.5f;

	public float rainScalar;

	public float snowScalar;

	public float hailScalar;

	public float lightningsScalar;

	public float windScalar;

	public Color sunColor = new Color(1f, 1f, 1f, 1f);

	public Color cloudsColor = new Color(1f, 1f, 1f, 1f);

	public Color horizonColor = new Color(1f, 1f, 1f, 1f);

	public GameObject rainPrefab;

	public GameObject snowPrefab;

	public GameObject hailPrefab;

	public GameObject screenDropsPrefab;

	public GameObject[] lightningPrefabs;

	public GameObject glowSpritePrefab;

	private bool lightningSpawned = true;

	private GameObject rainInstance;

	private GameObject snowInstance;

	private GameObject hailInstance;

	private ParticleSystem hailPS;

	private GameObject screenDropsInstance;

	private ParticleSystem screenDropsPS;

	private void SpawnRain()
	{
		rainInstance = Object.Instantiate(rainPrefab);
		rainInstance.transform.position = base.transform.position;
		rainInstance.transform.parent = base.transform;
		rainInstance.GetComponent<Renderer>().enabled = true;
	}

	private void UpdateRain()
	{
		if (rainScalar > 0f)
		{
			if (rainInstance == null)
			{
				SpawnRain();
				return;
			}
			rainInstance.transform.position = base.transform.position;
			rainInstance.GetComponent<Renderer>().sharedMaterial.SetColor(ShaderPropertyID._SunColor, sunColor);
			rainInstance.GetComponent<Renderer>().sharedMaterial.SetColor(ShaderPropertyID._CloudsColor, cloudsColor);
			rainInstance.GetComponent<Renderer>().sharedMaterial.SetFloat(ShaderPropertyID._cloudAmount, Mathf.Clamp01(cloudScalar));
			rainInstance.GetComponent<Renderer>().sharedMaterial.SetFloat(ShaderPropertyID._sunAmount, Mathf.Clamp01(sunScalar));
			rainInstance.GetComponent<Renderer>().sharedMaterial.SetFloat(ShaderPropertyID._rainAmount, Mathf.Clamp01(rainScalar) * 50f);
			float num = Mathf.PerlinNoise(Time.time * 0.15f, 0f) * Mathf.Clamp01(windScalar) * 55f;
			rainInstance.transform.eulerAngles = new Vector3(num, 0f, 0f - num);
		}
		else if (rainInstance != null)
		{
			Object.Destroy(rainInstance);
		}
	}

	private void SpawnSnow()
	{
		snowInstance = Object.Instantiate(snowPrefab);
		snowInstance.transform.position = base.transform.position;
		snowInstance.transform.parent = base.transform;
		snowInstance.GetComponent<Renderer>().enabled = true;
	}

	private void UpdateSnow()
	{
		if (snowScalar > 0f)
		{
			if (snowInstance == null)
			{
				SpawnSnow();
				return;
			}
			snowInstance.transform.position = base.transform.position;
			snowInstance.GetComponent<Renderer>().sharedMaterial.SetColor(ShaderPropertyID._SunColor, sunColor);
			snowInstance.GetComponent<Renderer>().sharedMaterial.SetFloat(ShaderPropertyID._sunAmount, Mathf.Clamp01(sunScalar));
			snowInstance.GetComponent<Renderer>().sharedMaterial.SetFloat(ShaderPropertyID._snowAmount, Mathf.Clamp01(snowScalar));
			float num = Mathf.PerlinNoise(Time.time * 0.15f, 0f) * Mathf.Clamp01(windScalar) * 55f;
			snowInstance.transform.eulerAngles = new Vector3(num, 0f, 0f - num);
		}
		else if (snowInstance != null)
		{
			Object.Destroy(snowInstance);
		}
	}

	private void SpawnHail()
	{
		hailInstance = Object.Instantiate(hailPrefab);
		hailInstance.transform.position = base.transform.position + new Vector3(0f, 40f, 0f);
		hailInstance.transform.parent = base.transform;
		hailInstance.transform.localEulerAngles = new Vector3(180f, 0f, 0f);
		hailPS = hailInstance.GetComponent<ParticleSystem>();
		hailPS.Play();
	}

	private void UpdateHail()
	{
		if (hailScalar > 0f)
		{
			if (hailInstance == null)
			{
				SpawnHail();
			}
			else
			{
				hailPS.emissionRate = Mathf.Clamp01(hailScalar) * 100f;
			}
		}
		else if (hailInstance != null)
		{
			Object.Destroy(hailInstance, 3f);
		}
	}

	private void SpawnScreenDrops()
	{
		GameObject fpParticleEmissionPoint = Utils.GetLocalPlayerComp().fpParticleEmissionPoint;
		if (fpParticleEmissionPoint != null)
		{
			Transform parent = fpParticleEmissionPoint.transform;
			screenDropsInstance = Object.Instantiate(screenDropsPrefab);
			screenDropsInstance.transform.parent = parent;
			screenDropsInstance.transform.localPosition = new Vector3(-0.16f, 0f, 0f);
			screenDropsInstance.transform.localEulerAngles = new Vector3(0f, 90f, 90f);
			screenDropsPS = screenDropsInstance.GetComponent<ParticleSystem>();
		}
	}

	private void UpdateScreenDrops(float playerDepth)
	{
		if (screenDropsInstance == null)
		{
			SpawnScreenDrops();
		}
		else if (snowScalar > 0f || rainScalar > 0f)
		{
			screenDropsPS.emissionRate = Mathf.Clamp01(snowScalar + rainScalar) * 30f;
			if (playerDepth > -1f)
			{
				if (!screenDropsPS.isPlaying)
				{
					screenDropsPS.Play();
				}
			}
			else if (screenDropsPS.isPlaying)
			{
				screenDropsPS.Stop();
				screenDropsPS.Clear();
			}
		}
		else if (screenDropsPS.isPlaying)
		{
			screenDropsPS.Stop();
		}
	}

	private void SpawnLightning()
	{
		int max = lightningPrefabs.Length;
		Vector3 position = base.transform.position + new Vector3(Random.Range(-500f, 500f), 256f, Random.Range(-500f, 500f));
		GameObject gameObject = lightningPrefabs[Random.Range(0, max)];
		if (gameObject != null)
		{
			GameObject obj = Object.Instantiate(gameObject);
			obj.transform.position = position;
			obj.transform.eulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
			obj.transform.localScale *= Random.Range(0.5f, 1.5f);
		}
		GameObject obj2 = Object.Instantiate(glowSpritePrefab);
		GameObject gameObject2 = Object.Instantiate(glowSpritePrefab);
		Vector3 localScale = new Vector3(Random.Range(400f, 800f), Random.Range(400f, 800f), 1f);
		obj2.transform.position = position;
		obj2.transform.LookAt(Utils.GetLocalPlayerPos());
		obj2.transform.localScale = localScale;
		obj2.GetComponent<Renderer>().material.SetFloat(ShaderPropertyID._EmissiveStrengh, Random.Range(0.5f, 3f));
		gameObject2.transform.position = new Vector3(position.x, -0.5f, position.z);
		gameObject2.transform.localScale = localScale;
		gameObject2.GetComponent<Renderer>().material.SetFloat(ShaderPropertyID._EmissiveStrengh, Random.Range(0.25f, 2f));
		lightningSpawned = true;
	}

	private void UpdateLightnings(float lightScalar)
	{
		if (lightScalar > 0f && lightningSpawned && MiscSettings.flashes)
		{
			lightningSpawned = false;
			Invoke("SpawnLightning", 1f / Random.Range(lightScalar, lightScalar * 15f));
		}
	}

	private void UpdateClouds()
	{
		GetComponent<Renderer>().sharedMaterial.SetColor(ShaderPropertyID._SunColor, sunColor);
		GetComponent<Renderer>().sharedMaterial.SetColor(ShaderPropertyID._CloudsColor, cloudsColor);
		GetComponent<Renderer>().sharedMaterial.SetFloat(ShaderPropertyID._horizonFogAmount, Mathf.Clamp01(horizonFogScalar));
		GetComponent<Renderer>().sharedMaterial.SetFloat(ShaderPropertyID._cloudAmount, Mathf.Clamp01(cloudScalar) * 9f);
		GetComponent<Renderer>().sharedMaterial.SetFloat(ShaderPropertyID._sunAmount, Mathf.Clamp01(sunScalar) * 8f);
	}

	private void Awake()
	{
		UpdateClouds();
		UpdateRain();
		UpdateHail();
		UpdateLightnings(Mathf.Clamp01(lightningsScalar));
	}

	private void Update()
	{
		Vector3 localPlayerPos = Utils.GetLocalPlayerPos();
		base.transform.position = new Vector3(localPlayerPos.x, -0.7f, localPlayerPos.z);
		UpdateClouds();
		UpdateRain();
		UpdateSnow();
		UpdateHail();
		UpdateScreenDrops(localPlayerPos.y);
		UpdateLightnings(Mathf.Clamp01(lightningsScalar));
	}
}
