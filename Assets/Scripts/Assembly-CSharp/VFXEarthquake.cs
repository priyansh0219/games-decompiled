using System.Collections;
using UnityEngine;

public class VFXEarthquake : MonoBehaviour
{
	public float maxDistance = 100f;

	public int maxInstances = 10;

	public int maxSpawnPerFrame = 1;

	public GameObject[] dustFXprefab;

	public Transform[] spawnPoints;

	private int currentPointIndex;

	private int currentInstancesCount;

	private int spawnedThisFrame;

	private bool isShaking;

	public void Shake()
	{
		isShaking = true;
	}

	private void RandomizeSpawnPoints(Transform[] arr)
	{
		for (int num = arr.Length - 1; num > 0; num--)
		{
			int num2 = Random.Range(0, num);
			Transform transform = arr[num];
			arr[num] = arr[num2];
			arr[num2] = transform;
		}
	}

	private IEnumerator SpawnFX(Transform spawnPoint)
	{
		int num = Random.Range(0, dustFXprefab.Length);
		DeferredSpawner.Task task = DeferredSpawner.instance.InstantiateAsync(dustFXprefab[num], this, spawnPoint, spawnPoint.position, spawnPoint.rotation);
		yield return task;
		GameObject result = task.GetResult();
		if (!task.cancelled)
		{
			result.transform.GetComponent<ParticleSystem>().Play();
		}
	}

	private void Update()
	{
		if (!isShaking)
		{
			return;
		}
		Vector3 localPlayerPos = Utils.GetLocalPlayerPos();
		spawnedThisFrame = 0;
		if (currentPointIndex >= spawnPoints.Length || currentInstancesCount >= maxInstances)
		{
			RandomizeSpawnPoints(spawnPoints);
			currentPointIndex = 0;
			currentInstancesCount = 0;
			isShaking = false;
			return;
		}
		for (int i = currentPointIndex; i < spawnPoints.Length; i++)
		{
			if ((localPlayerPos - spawnPoints[currentPointIndex].transform.position).sqrMagnitude < maxDistance)
			{
				StartCoroutine(SpawnFX(spawnPoints[currentPointIndex]));
				currentInstancesCount++;
				spawnedThisFrame++;
			}
			currentPointIndex++;
		}
	}
}
