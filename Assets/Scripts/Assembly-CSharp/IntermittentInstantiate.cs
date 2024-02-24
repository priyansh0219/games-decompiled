using System.Collections;
using UWE;
using UnityEngine;

public class IntermittentInstantiate : MonoBehaviour
{
	[AssertNotNull]
	public GameObject prefab;

	public Transform optEmitPoint;

	public float baseIntervalTime;

	public float randomIntervalTime;

	public int numToInstantiate;

	public float intraBurstPeriod = 0.1f;

	public bool registerToWorldStreamer = true;

	public FMOD_StudioEventEmitter onCreateSound;

	private float GetInterval()
	{
		return baseIntervalTime + Random.value * randomIntervalTime;
	}

	private void OnEnable()
	{
		Invoke("CreatePrefab", GetInterval());
	}

	private void OnDisable()
	{
		CancelInvoke("CreatePrefab");
	}

	private void CreatePrefab()
	{
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(CreateBatch());
			Invoke("CreatePrefab", GetInterval());
		}
	}

	private IEnumerator CreateBatch()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			yield break;
		}
		for (int i = 0; i < numToInstantiate; i++)
		{
			Vector3 position = ((optEmitPoint != null) ? optEmitPoint.position : base.gameObject.transform.position);
			GameObject gameObject = UWE.Utils.InstantiateWrap(prefab, position, Quaternion.identity);
			if (registerToWorldStreamer && (bool)LargeWorldStreamer.main)
			{
				LargeWorldStreamer.main.cellManager.RegisterEntity(gameObject);
			}
			if ((bool)onCreateSound)
			{
				Utils.PlayEnvSound(onCreateSound, gameObject.transform.position, 1f);
			}
			yield return new WaitForSeconds(intraBurstPeriod);
		}
	}

	private void OnKill()
	{
		Object.Destroy(this);
	}
}
