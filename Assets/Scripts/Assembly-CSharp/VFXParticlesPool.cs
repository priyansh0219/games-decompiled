using System.Collections.Generic;
using UnityEngine;

public class VFXParticlesPool : MonoBehaviour, IManagedUpdateBehaviour, IManagedBehaviour
{
	public enum PoolType
	{
		DeleteOldest = 0,
		Skip = 1
	}

	public class FXinstance
	{
		public GameObject go;

		public ParticleSystem ps;

		public float lastTimeUsed;
	}

	public GameObject fxPrefab;

	public int maxQuantity;

	public float fxDuration;

	public PoolType poolType;

	private Vector3 defaultAngles = new Vector3(-90f, 0f, 0f);

	private List<FXinstance> instancesInUse = new List<FXinstance>();

	private List<FXinstance> instancesFree = new List<FXinstance>();

	public int managedUpdateIndex { get; set; }

	private void OnDisable()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	private void OnDestroy()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	public string GetProfileTag()
	{
		return "VFXParticlesPool";
	}

	private void Init()
	{
		instancesInUse = new List<FXinstance>();
		instancesFree = new List<FXinstance>();
		for (int i = 0; i < maxQuantity; i++)
		{
			FXinstance fXinstance = new FXinstance();
			fXinstance.go = Object.Instantiate(fxPrefab, base.transform.position, Quaternion.identity);
			fXinstance.ps = fXinstance.go.GetComponent<ParticleSystem>();
			fXinstance.go.transform.parent = base.transform;
			fXinstance.go.SetActive(value: false);
			instancesFree.Add(fXinstance);
		}
	}

	private void EnableAndPlayAt(Vector3 position, Transform parent, Vector3 angles)
	{
		if (instancesFree.Count < 1 && poolType == PoolType.DeleteOldest)
		{
			instancesInUse[0].ps.Stop();
			instancesInUse[0].ps.Clear();
			instancesFree.Add(instancesInUse[0]);
			instancesInUse.RemoveAt(0);
		}
		if (instancesFree.Count > 0)
		{
			instancesFree[0].go.transform.position = position;
			instancesFree[0].go.transform.eulerAngles = angles;
			instancesFree[0].go.SetActive(value: true);
			instancesFree[0].ps.Play();
			instancesFree[0].lastTimeUsed = Time.time;
			instancesInUse.Add(instancesFree[0]);
			instancesFree.RemoveAt(0);
			BehaviourUpdateUtils.Register(this);
		}
	}

	public void Play(Vector3 position, Transform parent, Vector3 angles)
	{
		EnableAndPlayAt(position, parent, angles);
	}

	public void Play(Vector3 position, Transform parent)
	{
		EnableAndPlayAt(position, parent, defaultAngles);
	}

	public void Play(Vector3 position)
	{
		EnableAndPlayAt(position, null, defaultAngles);
	}

	private void CheckAndFreeUnused()
	{
		if (instancesInUse.Count > 0)
		{
			if (instancesInUse[0].lastTimeUsed + fxDuration < Time.time)
			{
				instancesInUse[0].ps.Stop();
				instancesInUse[0].ps.Clear();
				instancesInUse[0].go.transform.position = base.transform.position;
				instancesInUse[0].go.SetActive(value: false);
				instancesFree.Add(instancesInUse[0]);
				instancesInUse.RemoveAt(0);
			}
		}
		else
		{
			BehaviourUpdateUtils.Deregister(this);
		}
	}

	private void Awake()
	{
		Init();
	}

	public void ManagedUpdate()
	{
		CheckAndFreeUnused();
	}
}
