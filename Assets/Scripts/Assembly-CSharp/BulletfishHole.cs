using UnityEngine;

public class BulletfishHole : MonoBehaviour
{
	public AudioClip fireClip;

	private float timeLastFired;

	public Vector3 scaleAmount = new Vector3(0f, 0f, 0f);

	public float scaleTime;

	public GameObject kBulletfishPrefab;

	public Bulletfish currentBulletfish;

	public Transform exitTransform;

	public bool animComplete = true;

	private void Awake()
	{
		GameObject gameObject = Object.Instantiate(kBulletfishPrefab, base.gameObject.transform.position, Quaternion.identity);
		currentBulletfish = gameObject.GetComponent<Bulletfish>();
		currentBulletfish.home = base.gameObject;
		currentBulletfish.exitTransform = exitTransform;
	}

	private void FireBulletfish()
	{
		if (currentBulletfish.Fire(exitTransform))
		{
			animComplete = false;
			iTween.PunchScale(base.gameObject, iTween.Hash("amount", scaleAmount, "time", scaleTime, "easetype", iTween.EaseType.easeInOutCubic, "oncomplete", "OnAnimComplete", "oncompletetarget", base.gameObject));
			AudioSource.PlayClipAtPoint(fireClip, base.transform.position);
			timeLastFired = Time.time;
		}
	}

	public void OnAnimComplete()
	{
		animComplete = true;
	}

	public void OnTriggerStay(Collider collider)
	{
		if ((bool)collider.gameObject.GetComponent<Rigidbody>())
		{
			Debug.Log("Bulletfish.OnTriggerEnter - " + collider.gameObject.name);
			if (Time.time > timeLastFired + 5f && animComplete)
			{
				FireBulletfish();
			}
		}
	}
}
