using UnityEngine;

public class Lava : MonoBehaviour
{
	public float kLavaDamage = 10f;

	private void OnTriggerStay(Collider collider)
	{
		if (!(collider.gameObject != null) || !(collider.gameObject.GetComponentInChildren<IgnoreTrigger>() != null))
		{
			LiveMixin component = collider.gameObject.GetComponent<LiveMixin>();
			if ((bool)component)
			{
				component.TakeDamage(kLavaDamage, base.gameObject.transform.position);
			}
		}
	}
}
