using System;
using System.Collections;
using UnityEngine;

public abstract class PrefabSpawnBase : MonoBehaviour
{
	public SpawnType spawnType;

	public float intermittentSpawnTime;

	private float spawnTime;

	public bool inheritLayer;

	public bool usePrefabTransformAsLocal;

	public bool useCurrentTransformAsLocal;

	public bool keepScale = true;

	public Transform attachToParent;

	public float spawnAtHealthPercent = 1f;

	public bool useSpawnAtHealth;

	public GameObject spawnedObj;

	private GameObject spawnedLastFrame;

	public string sendObjectMessageOnSpawn = "";

	public bool deactivateOnSpawn;

	private void Awake()
	{
		if (HasValidPrefab())
		{
			if (spawnType == SpawnType.OnAwake)
			{
				SpawnObj();
			}
		}
		else
		{
			Debug.Log(base.gameObject.name + ".PrefabSpawn() - No prefab specified, skipping.");
			base.gameObject.SetActive(value: false);
		}
		spawnTime = UnityEngine.Random.value * intermittentSpawnTime;
	}

	private void Start()
	{
		if (HasValidPrefab())
		{
			if (spawnType == SpawnType.OnStart)
			{
				SpawnObj();
			}
		}
		else
		{
			Debug.Log(base.gameObject.name + ".PrefabSpawn() - No prefab specified, skipping.");
			base.gameObject.SetActive(value: false);
		}
		if (spawnType == SpawnType.Intermittent)
		{
			StartCoroutine(IntermittentSpawnRoutine());
		}
	}

	public void SpawnManual(Action<GameObject> spawnCallback = null)
	{
		if (spawnedObj == null)
		{
			SpawnObj(spawnCallback);
		}
	}

	public virtual bool GetTimeToSpawn()
	{
		return UnityEngine.Random.value * intermittentSpawnTime < Time.deltaTime;
	}

	private IEnumerator IntermittentSpawnRoutine()
	{
		while (true)
		{
			if (spawnType == SpawnType.Intermittent && spawnedObj == null && GetTimeToSpawn())
			{
				SpawnObj();
			}
			spawnedLastFrame = spawnedObj;
			yield return null;
		}
	}

	private void OnNewBorn()
	{
		if (HasValidPrefab() && spawnType == SpawnType.OnNewBorn && spawnedObj == null)
		{
			SpawnObj();
		}
	}

	public abstract bool HasValidPrefab();

	protected virtual void SpawnObj(Action<GameObject> spawnCallback = null)
	{
		if (spawnedObj != null)
		{
			Debug.Log("WARNING: PrefabSpawn (" + base.gameObject.GetFullHierarchyPath() + ") already spawned its object! Object = " + spawnedObj.GetFullHierarchyPath());
			return;
		}
		Transform objParent = base.transform;
		if ((bool)attachToParent)
		{
			objParent = attachToParent;
		}
		Action<SpawnRequest> spawnCallback2 = delegate(SpawnRequest request)
		{
			spawnedObj = request.GetResultAndRelease();
			if (!usePrefabTransformAsLocal)
			{
				if (useCurrentTransformAsLocal)
				{
					spawnedObj.transform.localPosition = base.transform.localPosition;
					spawnedObj.transform.localRotation = base.transform.localRotation;
				}
				else if (keepScale)
				{
					spawnedObj.transform.localPosition = Vector3.zero;
					spawnedObj.transform.localRotation = Quaternion.identity;
				}
				else
				{
					Utils.ZeroTransform(spawnedObj.transform);
				}
			}
			if (inheritLayer)
			{
				spawnedObj.layer = base.gameObject.layer;
				Transform[] allComponentsInChildren = spawnedObj.GetAllComponentsInChildren<Transform>();
				for (int i = 0; i < allComponentsInChildren.Length; i++)
				{
					allComponentsInChildren[i].gameObject.layer = base.gameObject.layer;
				}
			}
			if (deactivateOnSpawn)
			{
				spawnedObj.SetActive(value: false);
			}
			if (useSpawnAtHealth)
			{
				LiveMixin componentInChildren = spawnedObj.GetComponentInChildren<LiveMixin>();
				if ((bool)componentInChildren)
				{
					componentInChildren.startHealthPercent = spawnAtHealthPercent;
				}
				else
				{
					Debug.LogWarningFormat(this, "Failed to set start health percent on spawned object {0}", spawnedObj);
				}
			}
			if (sendObjectMessageOnSpawn != "")
			{
				spawnedObj.SendMessage(sendObjectMessageOnSpawn);
			}
			if (spawnCallback != null)
			{
				spawnCallback(spawnedObj);
			}
		};
		SpawnObjInternal(objParent).RegisterCallback(spawnCallback2);
	}

	internal abstract SpawnRequest SpawnObjInternal(Transform objParent);
}
