using UnityEngine;

public class VFXFallingRocks : MonoBehaviour
{
	public GameObject startPrefab;

	public GameObject[] prefabs;

	public int minQuantity = 2;

	public int maxQuantity = 10;

	public float emitterRadius = 1f;

	public float minLifetime = 15f;

	public float maxLifetime = 25f;

	public float minDelay = 15f;

	public float maxDelay = 45f;

	public float minSpawnDuration = 1.5f;

	public float maxSpawnDuration = 0.5f;

	public float minSize = 0.25f;

	public float maxSize = 0.5f;

	public bool playOnAwake = true;

	public bool loop = true;

	public GameObject rockTrailPrefab;

	private float timer;

	private float rate;

	private float delayTimer;

	private float spawnDuration;

	private bool isEmitting;

	private int quantitySpawned;

	private int quantity = 2;

	private GameObject SpawnRandomPrefab()
	{
		int num = Random.Range(0, prefabs.Length);
		Vector3 pos = base.transform.position + Random.insideUnitSphere * emitterRadius;
		GameObject gameObject = Utils.SpawnPrefabAt(prefabs[num], null, pos);
		gameObject.transform.Rotate(Vector3.one * Random.value * 360f, Space.World);
		VFXAnimator component = gameObject.GetComponent<VFXAnimator>();
		if (component != null)
		{
			component.initScale = new Vector3(Random.Range(minSize, maxSize), Random.Range(minSize, maxSize), Random.Range(minSize, maxSize));
		}
		else
		{
			gameObject.transform.localScale = new Vector3(Random.Range(minSize, maxSize), Random.Range(minSize, maxSize), Random.Range(minSize, maxSize));
		}
		gameObject.SetActive(value: true);
		Utils.SpawnPrefabAt(rockTrailPrefab, gameObject.transform, gameObject.transform.position);
		return gameObject;
	}

	private void SpawnPrefabs()
	{
		isEmitting = true;
		quantity = Random.Range(minQuantity, maxQuantity);
		spawnDuration = Random.Range(minSpawnDuration, maxSpawnDuration);
		rate = spawnDuration / (float)quantity;
		delayTimer = Random.Range(minDelay, maxDelay) + spawnDuration;
		quantitySpawned = 0;
		if (startPrefab != null)
		{
			Utils.SpawnPrefabAt(startPrefab, null, base.transform.position);
		}
	}

	private void Start()
	{
		if (playOnAwake)
		{
			SpawnPrefabs();
		}
	}

	private void Update()
	{
		delayTimer -= Time.deltaTime;
		timer -= Time.deltaTime;
		if (delayTimer < 0f)
		{
			if (loop)
			{
				SpawnPrefabs();
			}
		}
		else if (timer < 0f && quantitySpawned < quantity)
		{
			quantitySpawned++;
			SpawnRandomPrefab();
			timer = spawnDuration / (float)quantity;
		}
	}
}
