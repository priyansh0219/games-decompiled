using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Trident : MonoBehaviour
{
	public Transform[] movePath;

	public float travelTime;

	public float timeSpawned;

	public AudioClip leaveNestClip;

	public float playerPushConstant;

	public bool forwardDirection;

	public int numItemsToDrop;

	private void Awake()
	{
		forwardDirection = true;
	}

	public void Spawn()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			timeSpawned = Time.time;
			base.gameObject.SetActive(value: true);
			base.gameObject.GetComponent<AudioSource>().PlayOneShot(leaveNestClip, 1f);
		}
	}

	private void Dock()
	{
		if (base.gameObject.activeInHierarchy)
		{
			timeSpawned = 0f;
			base.gameObject.SetActive(value: false);
			forwardDirection = !forwardDirection;
		}
	}

	private void Update()
	{
		float num = Mathf.Clamp01((Time.time - timeSpawned) / travelTime);
		if (!forwardDirection)
		{
			num = 1f - num;
		}
		iTween.PutOnPath(base.gameObject, movePath, num);
		float num2 = (forwardDirection ? 0.05f : (-0.05f));
		float percent = Mathf.Clamp01(num + num2);
		base.transform.LookAt(iTween.PointOnPath(movePath, percent));
		if (Utils.NearlyEqual(num, forwardDirection ? 1f : 0f))
		{
			Dock();
		}
	}

	private void OnTouch(GameObject obj)
	{
		if ((bool)obj.GetComponent<Player>())
		{
			PlayerPusher component = obj.GetComponent<PlayerPusher>();
			if ((bool)component)
			{
				Vector3 vector = obj.transform.position - base.gameObject.transform.position;
				component.ApplyPlayerForce(vector.normalized * playerPushConstant);
				Debug.DrawRay(obj.transform.position, obj.transform.position + vector, Color.green, 8f);
			}
		}
	}
}
