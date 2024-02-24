using UnityEngine;

public class CyclopsDecoy : MonoBehaviour
{
	public float noiseScalar = 1f;

	public float lifeTime;

	public bool sonarDetectable = true;

	public bool launch;

	public float launchSpeed = 2f;

	[AssertNotNull]
	public GameObject despawnVFX;

	private void Start()
	{
		Invoke("Despawn", lifeTime);
		CyclopsDecoyManager.AddDecoyToGlobalHashSet(base.gameObject);
	}

	private void Despawn()
	{
		Object.Instantiate(despawnVFX, base.transform.position, Quaternion.identity);
		Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		if (launch)
		{
			base.transform.Translate(new Vector3(0f, launchSpeed, 0f), Space.World);
			launchSpeed = Mathf.MoveTowards(launchSpeed, 0f, Time.deltaTime);
			if (Mathf.Approximately(launchSpeed, 0f))
			{
				launch = false;
			}
		}
	}

	private void OnDestroy()
	{
		CyclopsDecoyManager.RemoveDecoyFromGlobalHashSet(base.gameObject);
	}

	public void OnHandHover()
	{
	}

	public void OnHandClick()
	{
	}
}
