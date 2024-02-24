using System.Collections.Generic;
using UWE;
using UnityEngine;

public class Creepvine : PlantBehaviour
{
	public FMOD_StudioEventEmitter cutEmitter;

	public bool isPlanted;

	public float segmentHeight = 6f;

	public Transform stamm;

	public GameObject seedPrefab;

	private float timeLastSeedCheck;

	private static TechType resourceOnCut = TechType.Creepvine;

	public float seedCreationChance = 1f;

	public float seedCheckInterval = 2f;

	private static float maturityPerSecond = 0.2f;

	private static float maturityUpdateInterval = 0.1f;

	private float timeLastMaturityUpdate;

	public GameObject[] topVariants;

	public GameObject[] segmentVariants;

	private List<GameObject> segments = new List<GameObject>();

	private Vector3 scale = new Vector3(1f, 1f, 1f);

	public float growTime;

	public float maxGrowTime = 180f;

	public float growSpeedMod = 0.1f;

	public bool fullyGrown;

	private float timeLastGrownUpdate;

	private int numFreeSeedPods;

	private float growMod = 1f;

	private float harvested;

	private void Awake()
	{
		growSpeedMod = 1f + (Random.value - 0.5f) * growSpeedMod;
	}

	private void OnKnifeHit(Knife knife)
	{
		if (cutEmitter != null)
		{
			Utils.PlayEnvSound(cutEmitter, knife.transform.position);
		}
		CraftData.AddToInventory(resourceOnCut);
		harvested += 10f;
		if (harvested >= growTime)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private GameObject CreateRandom(GameObject[] prefabs, float offset = 0f)
	{
		GameObject obj = Object.Instantiate(prefabs[Random.Range(0, prefabs.Length - 1)]);
		obj.transform.parent = stamm;
		UWE.Utils.ZeroTransform(obj.transform);
		obj.transform.localPosition = new Vector3(0f, offset, 0f);
		return obj;
	}

	private bool CreateSeed()
	{
		Transform transform = null;
		CreepvineSeedPod[] componentsInChildren = GetComponentsInChildren<CreepvineSeedPod>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].GetComponentInChildren<CreepvineSeed>() == null)
			{
				transform = componentsInChildren[i].transform;
				break;
			}
		}
		if (transform != null)
		{
			GameObject obj = Object.Instantiate(seedPrefab);
			obj.GetComponent<CreepvineSeed>().maturity = 0f;
			obj.GetComponent<CreepvineSeed>().isPickupable = false;
			obj.transform.parent = transform;
			UWE.Utils.ZeroTransform(obj.transform);
			return true;
		}
		return false;
	}

	private void CheckAndCreateSeed()
	{
		if (Time.time - timeLastSeedCheck > seedCheckInterval && numFreeSeedPods > 0)
		{
			if (Random.value < seedCreationChance && CreateSeed())
			{
				numFreeSeedPods--;
			}
			timeLastSeedCheck = Time.time;
		}
	}
}
