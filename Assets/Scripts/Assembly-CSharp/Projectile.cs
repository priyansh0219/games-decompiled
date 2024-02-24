using UWE;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	public float speed = 10f;

	public float lifeTime = 4f;

	public bool destroyOnCollision;

	public DamageType damageType;

	public float damage = 10f;

	public bool deflectable;

	public Rigidbody myRigidbody;

	private Trail_v2 trailFX;

	private void Start()
	{
		Invoke("TimeUp", lifeTime);
	}

	public void Shoot(Vector3 direction)
	{
		myRigidbody.velocity = Vector3.Normalize(direction) * speed;
		trailFX = GetComponentInChildren<Trail_v2>();
	}

	private void OnCollisionEnter(Collision collision)
	{
		GameObject gameObject = collision.gameObject;
		bool flag = false;
		Player component = gameObject.GetComponent<Player>();
		if (deflectable && component != null && component.HasReinforcedSuit())
		{
			myRigidbody.velocity = Vector3.Normalize(-collision.relativeVelocity) * speed * 0.8f;
			return;
		}
		LiveMixin componentInHierarchy = UWE.Utils.GetComponentInHierarchy<LiveMixin>(gameObject);
		if (componentInHierarchy != null)
		{
			componentInHierarchy.TakeDamage(damage, base.transform.position, damageType);
			flag = true;
		}
		if (destroyOnCollision || flag)
		{
			if (trailFX != null)
			{
				trailFX.Stop();
				trailFX.transform.parent = null;
				Object.Destroy(trailFX.gameObject, 1f);
			}
			Object.Destroy(base.gameObject);
		}
	}

	private void TimeUp()
	{
		Object.Destroy(base.gameObject);
	}
}
