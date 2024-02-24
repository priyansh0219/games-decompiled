using UWE;
using UnityEngine;

public class BaseEntranceTrigger : MonoBehaviour
{
	private void HandleTrigger(Collider other)
	{
		Player component = other.gameObject.GetComponent<Player>();
		if ((bool)component)
		{
			if (other.gameObject.transform.position.y - base.transform.position.y < 0f)
			{
				component.SetCurrentSub(null);
			}
			else
			{
				component.SetCurrentSub(GetComponentInParent<SubRoot>());
			}
		}
		else if (!other.isTrigger)
		{
			GameObject entityRoot = UWE.Utils.GetEntityRoot(other.gameObject);
			if (!entityRoot)
			{
				entityRoot = other.gameObject;
			}
			Rigidbody component2 = entityRoot.GetComponent<Rigidbody>();
			if ((bool)component2)
			{
				component2.velocity = new Vector3(component2.velocity.x, Mathf.Min(-1f, component2.velocity.y), component2.velocity.z);
			}
		}
	}

	private void OnTriggerStay(Collider other)
	{
		HandleTrigger(other);
	}

	private void OnTriggerExit(Collider other)
	{
		HandleTrigger(other);
	}
}
