using UnityEngine;

[RequireComponent(typeof(LiveMixin))]
public class TakeDamageOnCollide : MonoBehaviour
{
	public float damage;

	private void OnCollisionEnter(Collision collision)
	{
		base.gameObject.GetComponent<LiveMixin>().TakeDamage(damage);
	}
}
