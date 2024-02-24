using UWE;
using UnityEngine;

public class Bulletfish : MonoBehaviour
{
	public GameObject home;

	public Transform exitTransform;

	public float initialFireForce;

	public float returnHomeForce;

	public AudioClip hitClip;

	public bool launched;

	public float returnDelay;

	private float timeLaunched;

	public float xPokeoutOffset;

	public float playerForceScalar;

	private void Start()
	{
		Dock();
		Physics.IgnoreCollision(GetComponent<Collider>(), home.GetComponent<Collider>(), ignore: true);
	}

	private void Dock()
	{
		launched = false;
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(GetComponent<Rigidbody>(), isKinematic: true);
		base.transform.position = exitTransform.position + exitTransform.forward * xPokeoutOffset;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (hitClip != null)
		{
			AudioSource.PlayClipAtPoint(hitClip, base.transform.position);
		}
		OxygenManager component = collision.gameObject.GetComponent<OxygenManager>();
		if ((bool)component)
		{
			component.RemoveOxygen(10f);
		}
		PlayerPusher component2 = collision.gameObject.GetComponent<PlayerPusher>();
		if ((bool)component2)
		{
			component2.ApplyPlayerForce(GetComponent<Rigidbody>().velocity * playerForceScalar);
		}
	}

	public bool Fire(Transform exitTransform)
	{
		if (!launched)
		{
			UWE.Utils.SetIsKinematicAndUpdateInterpolation(GetComponent<Rigidbody>(), isKinematic: false);
			GetComponent<Rigidbody>().AddForce(exitTransform.forward * initialFireForce, ForceMode.Impulse);
			Debug.DrawLine(exitTransform.position, exitTransform.position + exitTransform.forward * initialFireForce, Color.blue, 10f);
			launched = true;
			timeLaunched = Time.time;
		}
		return launched;
	}

	private void FixedUpdate()
	{
		if (launched && Time.time > timeLaunched + returnDelay)
		{
			Vector3 vector = exitTransform.position - base.gameObject.transform.position;
			Debug.DrawLine(base.transform.position, base.transform.position + vector, Color.red, 0.5f);
			float magnitude = vector.magnitude;
			if (magnitude < 1f)
			{
				Dock();
			}
			else
			{
				GetComponent<Rigidbody>().AddForce(vector.normalized * magnitude * returnHomeForce, ForceMode.Impulse);
			}
		}
	}
}
