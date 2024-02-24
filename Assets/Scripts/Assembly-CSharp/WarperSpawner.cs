using UnityEngine;

public class WarperSpawner : MonoBehaviour
{
	[AssertNotNull]
	public GameObject warperPrefab;

	public float warpPause = 10f;

	public float warpInterval = 20f;

	private Warper warper;

	private float nextWarpTime;

	private void OnEnable()
	{
		nextWarpTime = Time.time + warpPause + Random.value * 3f;
	}

	private void OnDisable()
	{
		if (warper != null)
		{
			warper.WarpOut();
		}
	}

	private void Update()
	{
		if (warper == null && Time.time > nextWarpTime)
		{
			GameObject gameObject = Object.Instantiate(warperPrefab);
			if (gameObject != null)
			{
				gameObject.transform.position = GetSpawnPosition();
				Vector2 insideUnitCircle = Random.insideUnitCircle;
				gameObject.transform.LookAt(base.transform.position + new Vector3(insideUnitCircle.x, 0f, insideUnitCircle.y));
				warper = gameObject.GetComponent<Warper>();
				warper.WarpIn(this);
			}
		}
	}

	private Vector3 GetSpawnPosition()
	{
		Vector3 onUnitSphere = Random.onUnitSphere;
		float num = 3f;
		if (Physics.Raycast(base.transform.position, onUnitSphere, out var hitInfo, num))
		{
			return hitInfo.point - onUnitSphere.normalized;
		}
		return base.transform.position + onUnitSphere * num;
	}

	public void OnWarpOut()
	{
		nextWarpTime = Time.time + warpInterval + Random.value * 3f;
	}
}
