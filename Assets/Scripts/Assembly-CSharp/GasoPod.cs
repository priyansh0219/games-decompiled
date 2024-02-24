using System.Collections;
using ProtoBuf;
using UnityEngine;
using UnityEngine.AddressableAssets;

[ProtoContract]
public class GasoPod : Creature
{
	public Transform gasPodSpawn;

	public AssetReferenceGameObject podPrefabReference;

	public GameObject gasFXprefab;

	private float podRandomForce = 1f;

	private float podBaseForce = 1f;

	private float timeLastGasPodDrop = -20000f;

	private float podSpawnDist = 0.27f;

	private float scaredTriggerValue = 0.75f;

	private float minTimeBetweenPayloads = 10f;

	private void DropGasPods()
	{
		int num = Random.Range(6, 10);
		for (int i = 1; i <= num; i++)
		{
			StartCoroutine(SpawnGasPodAsync());
		}
		if (gasFXprefab != null)
		{
			Utils.SpawnZeroedAt(gasFXprefab, gasPodSpawn);
		}
	}

	private IEnumerator SpawnGasPodAsync()
	{
		Vector3 randomDirection2 = Random.onUnitSphere;
		randomDirection2.z = (0f - Mathf.Abs(randomDirection2.z)) * 3f;
		Vector3 position = gasPodSpawn.TransformPoint(randomDirection2 * podSpawnDist);
		CoroutineTask<GameObject> task = AddressablesUtility.InstantiateAsync(podPrefabReference.RuntimeKey as string, null, position);
		yield return task;
		GameObject result = task.GetResult();
		if (!(result == null))
		{
			randomDirection2 = gasPodSpawn.TransformDirection(randomDirection2);
			result.GetComponent<Rigidbody>().AddForce(randomDirection2 * (podBaseForce + Random.value * podRandomForce), ForceMode.VelocityChange);
			if ((bool)LargeWorldStreamer.main)
			{
				LargeWorldStreamer.main.MakeEntityTransient(result);
			}
		}
	}

	public void Update()
	{
		Player main = Player.main;
		if (timeLastGasPodDrop + minTimeBetweenPayloads <= Time.time && Scared.Value >= scaredTriggerValue && (bool)main && main.CanBeAttacked())
		{
			timeLastGasPodDrop = Time.time;
			DropGasPods();
		}
	}
}
