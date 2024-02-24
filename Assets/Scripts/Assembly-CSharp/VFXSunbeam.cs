using System;
using UnityEngine;

public class VFXSunbeam : MonoBehaviour
{
	public static VFXSunbeam main;

	[AssertNotNull]
	public GameObject shipPrefab;

	[AssertNotNull]
	public GameObject warmupPrefab;

	[AssertNotNull]
	public GameObject levitatingRocksPrefab;

	[AssertNotNull]
	public GameObject muzzlePrefab;

	[AssertNotNull]
	public GameObject explosionPrefab;

	[AssertNotNull]
	public GameObject groundShockwavePrefab;

	[AssertNotNull]
	public GameObject beamPrefab;

	[AssertNotNull]
	public GameObject shootDownSFX;

	[AssertNotNull]
	public GameObject shootDownSFXNoVoice;

	[AssertNotNull]
	public GameObject[] burningChunkPrefabs;

	public int chunksAmount = 15;

	public float chunksSpawnDelay = 0.2f;

	public float chunkSpawnRadius = 25f;

	public float chunkForce = 10f;

	public AnimationCurve beamWidthCurve = new AnimationCurve();

	public AnimationCurve beamLengthCurve = new AnimationCurve();

	public float shipApproachDuration = 15f;

	public float warmupDuration = 3f;

	public float explosionDelay = 2f;

	public float beamDelay = 1f;

	public float beamDuration = 2f;

	public Vector3 shipInitPos = new Vector3(0f, 5000f, 6990f);

	public Vector3 targetInitPos = new Vector3(1294.2f, 3620.1f, 3899.9f);

	public Gradient cloudsColor;

	public float cloudsColorDuration = 15f;

	private Transform shipTransform;

	private GameObject gunSpawnPoint;

	private GameObject gunTarget;

	private GameObject rectifiedTarget;

	private int chunksToSpawn;

	private float animTime = -1f;

	private float shipAnimeTime;

	private float beamAnimTime;

	private float cloudsAnimTime;

	private bool warmedUp;

	private bool exploded;

	private Transform warmupTransform;

	private Transform muzzleTransform;

	private Transform explosionTransform;

	private GameObject beamInstance;

	private Material[] beamMats;

	private Color beamMatColor;

	private Vector3 originInitPos = new Vector3(489f, 129.7f, 1243.9f);

	private Vector3 realShipPos = new Vector3(0f, 5000f, 6990f);

	public bool isPlaying { get; private set; }

	public bool IsShooting()
	{
		return beamInstance != null;
	}

	public Color GetCloudsColor()
	{
		if (cloudsAnimTime <= 0f || cloudsAnimTime >= 1f || DayNightCycle.main == null)
		{
			return Color.black;
		}
		Color b = cloudsColor.Evaluate(cloudsAnimTime);
		float t = Mathf.Clamp01(DayNightCycle.main.GetLightScalar() + 0.1f);
		return Color.Lerp(Color.black, b, t);
	}

	private void CreateBeam()
	{
		Debug.Log("SUNBEAM: SPAWN BEAM");
		beamInstance = Utils.SpawnZeroedAt(beamPrefab, gunSpawnPoint.transform);
		beamInstance.transform.localScale = new Vector3(20f, 30f, 0f);
		beamInstance.transform.LookAt(gunSpawnPoint.transform);
		beamMats = beamInstance.GetComponent<Renderer>().materials;
		beamMatColor = beamMats[1].color;
	}

	private void UpdateBeam()
	{
		if (beamInstance != null)
		{
			if (beamAnimTime < 1f)
			{
				float z = Vector3.Distance(gunSpawnPoint.transform.position, rectifiedTarget.transform.position);
				float num = beamWidthCurve.Evaluate(beamAnimTime);
				Vector3 localScale = new Vector3(30f * num, 45f * num, z);
				beamInstance.transform.localScale = localScale;
				beamMats[0].SetTextureOffset(ShaderPropertyID._MainTex2, new Vector2(beamLengthCurve.Evaluate(beamAnimTime), 0.5f));
				beamAnimTime += Time.deltaTime / beamDuration;
			}
			else
			{
				UnityEngine.Object.Destroy(beamInstance);
			}
		}
	}

	private void Awake()
	{
		main = this;
	}

	private void Start()
	{
		DevConsole.RegisterConsoleCommand(this, "playsunbeamfx");
	}

	public void PlaySequence()
	{
		Debug.Log("VFXSunBeam.PlaySequence");
		DestroyEffects();
		gunSpawnPoint = new GameObject("PrecursorGunFXSpawnPoint");
		gunSpawnPoint.transform.position = originInitPos;
		gunTarget = new GameObject("PrecursorGunFXTarget");
		gunTarget.transform.position = targetInitPos;
		rectifiedTarget = new GameObject("PrecursorGunFXRectifiedTarget");
		rectifiedTarget.transform.position = targetInitPos;
		animTime = 0f;
		shipAnimeTime = 0f;
		beamAnimTime = 0f;
		cloudsAnimTime = 0f;
		isPlaying = true;
		warmedUp = false;
		exploded = false;
		chunksToSpawn = chunksAmount;
		shipTransform = SpawnFXAndPlay(shipPrefab, base.transform);
		Invoke("PlayWarmupFX", shipApproachDuration);
	}

	private void PlayWarmupFX()
	{
		if (PrecursorGunStoryEvents.main != null)
		{
			warmupTransform = SpawnFXAndPlay(warmupPrefab, PrecursorGunStoryEvents.main.gunAim.gunArm);
			SpawnFXAndPlay(levitatingRocksPrefab, PrecursorGunStoryEvents.main.transform);
		}
	}

	public void PlaySFX(bool playerAtLandingSite)
	{
		UnityEngine.Object.Instantiate(playerAtLandingSite ? shootDownSFX : shootDownSFXNoVoice, new Vector3(411f, 0f, 1213f), Quaternion.identity);
	}

	private void DestroyEffects()
	{
		animTime = -1f;
		if (warmupTransform != null)
		{
			UnityEngine.Object.Destroy(warmupTransform.gameObject);
		}
		if (muzzleTransform != null)
		{
			UnityEngine.Object.Destroy(muzzleTransform.gameObject);
		}
		if (explosionTransform != null)
		{
			UnityEngine.Object.Destroy(explosionTransform.gameObject);
		}
		if (gunSpawnPoint != null)
		{
			UnityEngine.Object.Destroy(gunSpawnPoint.gameObject);
		}
		if (gunTarget != null)
		{
			UnityEngine.Object.Destroy(gunTarget.gameObject);
		}
		if (rectifiedTarget != null)
		{
			UnityEngine.Object.Destroy(rectifiedTarget.gameObject);
		}
		if (shipTransform != null)
		{
			UnityEngine.Object.Destroy(shipTransform.gameObject);
		}
		UnityEngine.Object.Destroy(beamInstance);
		CancelInvoke("UpdateDepth");
	}

	private void SpawnBurningChunks()
	{
		UnityEngine.Object.Instantiate(burningChunkPrefabs[UnityEngine.Random.Range(0, burningChunkPrefabs.Length)], gunTarget.transform.position + chunkSpawnRadius * UnityEngine.Random.insideUnitSphere, Quaternion.identity).GetComponent<ParticleSystem>().Play();
	}

	private Transform SpawnFXAndPlay(GameObject prefab, Transform parent)
	{
		GameObject obj = Utils.SpawnZeroedAt(prefab, parent);
		obj.GetComponent<ParticleSystem>().Play();
		return obj.transform;
	}

	private void UpdateSequence()
	{
		float num = shipApproachDuration + warmupDuration;
		float num2 = num + explosionDelay;
		if (shipTransform != null)
		{
			targetInitPos = shipTransform.position;
			if (shipAnimeTime < 1f)
			{
				shipAnimeTime += Time.deltaTime / num;
				if (shipAnimeTime >= 1f)
				{
					UnityEngine.Object.Destroy(shipTransform.gameObject, 5f);
				}
			}
		}
		if (!warmedUp && animTime >= num)
		{
			Debug.Log("SUNBEAM: WARMED UP");
			if (warmupTransform != null)
			{
				warmupTransform.GetComponent<ParticleSystem>().Stop();
			}
			gunSpawnPoint.transform.localScale = Vector3.one;
			Invoke("CreateBeam", beamDelay);
			if (PrecursorGunStoryEvents.main != null)
			{
				muzzleTransform = SpawnFXAndPlay(muzzlePrefab, gunSpawnPoint.transform);
			}
			warmedUp = true;
		}
		else if (!exploded && animTime >= num2)
		{
			Debug.Log("SUNBEAM: EXPLODING");
			if (beamMats != null)
			{
				if (beamMats.Length > 1)
				{
					float t = Mathf.Clamp01((animTime - num2) * 3f);
					beamMats[1].SetColor(ShaderPropertyID._Color, Color.Lerp(Color.clear, beamMatColor, t));
				}
				else
				{
					Debug.Log("VFXSunbeam.beamMats[1] is null");
				}
			}
			rectifiedTarget.transform.localScale = Vector3.one;
			explosionTransform = SpawnFXAndPlay(explosionPrefab, rectifiedTarget.transform);
			explosionTransform.eulerAngles = new Vector3(-90f, 0f, 0f);
			if (PrecursorGunStoryEvents.main != null)
			{
				SpawnFXAndPlay(groundShockwavePrefab, PrecursorGunStoryEvents.main.transform);
			}
			for (int i = 0; i < chunksAmount; i++)
			{
				Invoke("SpawnBurningChunks", chunksSpawnDelay * (float)i + 2f);
			}
			exploded = true;
		}
		if (exploded)
		{
			cloudsAnimTime += Time.deltaTime / cloudsColorDuration;
		}
		animTime += Time.deltaTime;
	}

	private void RectifyPosition(ref GameObject go, Vector3 targetPos, Vector3 playerPos)
	{
		float num = Vector3.Distance(targetPos, playerPos);
		float maxDistanceDelta = Mathf.Max(0f, num - 1000f);
		go.transform.position = Vector3.MoveTowards(targetPos, playerPos, maxDistanceDelta);
	}

	private void RectifyPositionAndScale(ref GameObject go, Vector3 targetPos, Vector3 playerPos, Vector3 currentScale)
	{
		float num = Vector3.Distance(targetPos, playerPos);
		float maxDistanceDelta = Mathf.Max(0f, num - 1000f);
		float num2 = Vector3.Distance(base.transform.position, playerPos);
		float num3 = 2f * Mathf.Tan(MainCamera.camera.fieldOfView * 0.5f * ((float)Math.PI / 180f));
		float num4 = num3 * num;
		float num5 = num3 * num2;
		float num6 = (num4 - num5) / num4;
		go.transform.localScale = currentScale * Mathf.Clamp(1f - num6, 0f, 1f);
		go.transform.position = Vector3.MoveTowards(targetPos, playerPos, maxDistanceDelta);
	}

	private void Update()
	{
		if (isPlaying)
		{
			UpdateSequence();
			Vector3 position = MainCamera.camera.transform.position;
			RectifyPosition(ref rectifiedTarget, targetInitPos, position);
			gunSpawnPoint.transform.position = originInitPos;
			gunSpawnPoint.transform.localScale = Vector3.one;
			gunSpawnPoint.transform.LookAt(rectifiedTarget.transform);
			UpdateBeam();
		}
		isPlaying = animTime > -1f && (animTime <= shipApproachDuration + warmupDuration + explosionDelay + 20f || explosionTransform != null);
	}

	private void OnConsoleCommand_playsunbeamfx()
	{
		PlaySequence();
	}

	private void OnDestroy()
	{
		if (beamMats != null)
		{
			for (int i = 0; i < beamMats.Length; i++)
			{
				UnityEngine.Object.Destroy(beamMats[i]);
			}
		}
	}
}
