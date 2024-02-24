using System.Collections.Generic;
using UnityEngine;

public class MagmaPillar : MonoBehaviour
{
	public GameObject[] prefabs;

	public GameObject smokeParticlePrefab;

	public GameObject smokeOffset;

	public int maxBlobs;

	public int minBlobs;

	private int quantity;

	public float minStartDelay;

	public float maxStartDelay;

	private float randomStartDelay;

	public float blobGrowTime;

	public float pillarLifeTime;

	public float blobDestroyTime;

	public float minDelayBetweenBlobs;

	public float maxDelayBetweenBlobs;

	private float delayBetweenBlobs;

	public float scaleFactor;

	public float scaleVariance;

	public Vector3 offsets = new Vector3(0.3f, 0.4f, 0.3f);

	public AnimationCurve BlobOffset;

	public AnimationCurve moveCurve;

	public AnimationCurve GrowCurve;

	public AnimationCurve freezeCurve;

	public AnimationCurve destructCurve;

	public float timeScale = 1f;

	private ParticleSystem smokeParticles;

	private float timeLived;

	private float timeToLive;

	private List<MagmaBlob> blobs = new List<MagmaBlob>();

	private void Awake()
	{
		GameObject gameObject = Object.Instantiate(smokeParticlePrefab.gameObject, Vector3.zero, Quaternion.identity);
		gameObject.transform.parent = smokeOffset.transform;
		gameObject.transform.localPosition = Vector3.zero;
		smokeParticles = gameObject.GetComponent<ParticleSystem>();
		quantity = Random.Range(minBlobs, maxBlobs);
		randomStartDelay = Random.Range(minStartDelay, maxStartDelay);
		delayBetweenBlobs = Random.Range(minDelayBetweenBlobs, maxDelayBetweenBlobs);
		timeToLive = randomStartDelay + pillarLifeTime + (float)quantity * (blobGrowTime + blobDestroyTime + 2f * delayBetweenBlobs);
		scaleFactor += Random.value * scaleVariance;
	}

	private void CreateBlobs()
	{
		Vector3 localStartPos = Vector3.zero;
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < quantity; i++)
		{
			GameObject obj = Utils.SpawnZeroedAt(prefabs[Random.Range(0, prefabs.Length)], base.transform);
			obj.transform.localScale = new Vector3(0f, 0f, 0f);
			MagmaBlob magmaBlob = obj.AddComponent<MagmaBlob>();
			float num = 1f + scaleFactor * ((float)(quantity - i) / (float)quantity);
			magmaBlob.SetScaleRatio(num);
			magmaBlob.SetLocalStartPos(localStartPos);
			magmaBlob.SetLocalEndPos(zero);
			localStartPos = zero;
			Vector3 vector = new Vector3(Random.Range(-1f, 1f), Random.Range(0.3f, 1f), Random.Range(-1f, 1f));
			vector.Normalize();
			vector *= BlobOffset.Evaluate(Random.value);
			Vector3 vector2 = new Vector3(vector.x * offsets.x, vector.y * offsets.y, vector.z * offsets.z);
			zero += vector2 * num;
			obj.transform.Rotate(Vector3.up * Random.Range(0, 360));
			obj.transform.Rotate(Vector3.right * Random.Range(0, 30));
			obj.transform.Rotate(Vector3.left * Random.Range(0, 30));
			obj.SetActive(value: false);
			blobs.Add(magmaBlob);
		}
	}

	private void Start()
	{
		CreateBlobs();
	}

	private bool UpdateBlobs()
	{
		if (timeLived < randomStartDelay || Utils.NearlyEqual(timeLived - randomStartDelay, 0f))
		{
			return false;
		}
		float num = timeLived - randomStartDelay;
		foreach (MagmaBlob blob in blobs)
		{
			float num2 = blobGrowTime;
			if (num < blobGrowTime)
			{
				num2 = num;
			}
			float growCurveValue = GrowCurve.Evaluate(num2 / blobGrowTime);
			float num3 = moveCurve.Evaluate(num2 / blobGrowTime);
			float freezeCurveValue = freezeCurve.Evaluate(num2 / blobGrowTime);
			blob.UpdateGrowingBlob(num2, blobGrowTime, growCurveValue, num3, freezeCurveValue);
			smokeOffset.transform.localPosition = Vector3.Lerp(blob.GetLocalStartPos(), blob.GetLocalEndPos(), num3);
			num -= blobGrowTime;
			if (num <= 0f)
			{
				return true;
			}
			num -= delayBetweenBlobs;
			if (num <= 0f)
			{
				return false;
			}
		}
		num -= pillarLifeTime;
		if (num <= 0f)
		{
			return false;
		}
		for (int num4 = blobs.Count - 1; num4 >= 0; num4--)
		{
			MagmaBlob magmaBlob = blobs[num4];
			float num5 = blobDestroyTime;
			if (num < blobDestroyTime)
			{
				num5 = num;
			}
			magmaBlob.UpdateDestroyingBlob(num5, blobDestroyTime, destructCurve.Evaluate(num5 / blobDestroyTime));
			num -= blobDestroyTime + delayBetweenBlobs;
			if (num <= 0f)
			{
				return false;
			}
		}
		return false;
	}

	public void SetRandomTimeLived()
	{
		timeLived = Random.value * timeToLive;
	}

	private void Update()
	{
		timeLived += Time.deltaTime * timeScale;
		bool flag = UpdateBlobs();
		if (flag && !smokeParticles.isPlaying)
		{
			smokeParticles.Play();
		}
		else if (!flag && smokeParticles.isPlaying)
		{
			smokeParticles.Stop();
		}
		if (timeLived > timeToLive)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
