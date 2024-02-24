using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BaseExitTrigger : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		Player component = other.gameObject.GetComponent<Player>();
		if ((bool)component)
		{
			component.SetCurrentSub(null);
		}
	}
}
