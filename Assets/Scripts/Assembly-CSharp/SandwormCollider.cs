using UnityEngine;

public class SandwormCollider : MonoBehaviour
{
	public float damagePerSecond = 30f;

	private void OnTriggerStay(Collider collider)
	{
		Debug.Log("SandwormCollider.OnTriggerStay() - " + collider.gameObject.name);
		LiveMixin liveMixin = Utils.FindAncestorWithComponent<LiveMixin>(collider.gameObject);
		if ((bool)liveMixin)
		{
			liveMixin.TakeDamage(damagePerSecond * Time.deltaTime, base.transform.position, DamageType.Collide);
		}
	}
}
