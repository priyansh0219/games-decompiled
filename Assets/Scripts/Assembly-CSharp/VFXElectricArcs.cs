using UnityEngine;

public class VFXElectricArcs : MonoBehaviour
{
	[AssertNotNull]
	public GameObject arcPrefab;

	[AssertNotNull]
	public Transform origin;

	[AssertNotNull]
	public Transform target;

	public bool autoStart = true;

	public bool isDynamic;

	public int arcsCount = 1;

	public float minLifeTime = 0.1f;

	public float maxLifeTime = 0.6f;

	public float minDelay = 0.1f;

	public float maxDelay = 1f;

	public Vector3 randomOrigin = Vector3.zero;

	public Vector3 randomTarget = Vector3.zero;

	private float delay = -1f;

	private VFXElectricLine[] lines;

	private float[] lifeTimes;

	private bool isPlaying;

	private Vector3 GetEndPos(Vector3 pos, Vector3 offset)
	{
		if (offset == Vector3.zero)
		{
			return pos;
		}
		Vector3 zero = Vector3.zero;
		zero.x = Random.Range(0f - offset.x, offset.x);
		zero.y = Random.Range(0f - offset.y, offset.y);
		zero.z = Random.Range(0f - offset.z, offset.z);
		return zero + pos;
	}

	private void Start()
	{
		lines = new VFXElectricLine[arcsCount];
		lifeTimes = new float[arcsCount];
		for (int i = 0; i < arcsCount; i++)
		{
			GameObject obj = Utils.SpawnZeroedAt(arcPrefab, base.transform, keepScale: true);
			VFXElectricLine component = obj.GetComponent<VFXElectricLine>();
			obj.SetActive(value: false);
			lines[i] = component;
			lifeTimes[i] = -1f;
		}
		if (autoStart)
		{
			Play();
		}
	}

	public void Play()
	{
		isPlaying = true;
		base.enabled = true;
	}

	public void Stop()
	{
		isPlaying = false;
		base.enabled = false;
	}

	private void ActivateArc()
	{
		for (int i = 0; i < arcsCount; i++)
		{
			if (lifeTimes[i] < 0f)
			{
				lines[i].origin = GetEndPos(origin.position, randomOrigin);
				lines[i].target = GetEndPos(target.position, randomTarget);
				lifeTimes[i] = Random.Range(minLifeTime, maxLifeTime);
				lines[i].gameObject.SetActive(value: true);
			}
		}
	}

	private void OnDisable()
	{
		for (int i = 0; i < arcsCount; i++)
		{
			lines[i].gameObject.SetActive(value: false);
			lifeTimes[i] = -1f;
		}
	}

	private void Update()
	{
		if (!isPlaying)
		{
			return;
		}
		delay -= Time.deltaTime;
		if (delay < 0f)
		{
			delay = Random.Range(minDelay, maxDelay);
			Invoke("ActivateArc", delay);
		}
		for (int i = 0; i < arcsCount; i++)
		{
			lifeTimes[i] -= Time.deltaTime;
			if (lifeTimes[i] < 0f)
			{
				lines[i].gameObject.SetActive(value: false);
			}
			if (isDynamic)
			{
				lines[i].origin = GetEndPos(origin.position, randomOrigin);
				lines[i].target = GetEndPos(target.position, randomTarget);
			}
		}
	}
}
