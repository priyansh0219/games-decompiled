using UnityEngine;

public class BlockSeamoth : MonoBehaviour
{
	private const float pushVelocity = 3f;

	private Rigidbody seamothRigidbody;

	private int enteredCollidersCount;

	private void FixedUpdate()
	{
		if ((bool)seamothRigidbody)
		{
			seamothRigidbody.AddForce(base.transform.forward * 3f, ForceMode.VelocityChange);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		SeaMoth seamoth = GetSeamoth(other);
		if ((bool)seamoth)
		{
			enteredCollidersCount++;
			seamothRigidbody = seamoth.GetComponent<Rigidbody>();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if ((bool)GetSeamoth(other))
		{
			enteredCollidersCount--;
			if (enteredCollidersCount <= 0)
			{
				seamothRigidbody = null;
			}
		}
	}

	private SeaMoth GetSeamoth(Collider other)
	{
		if (other.isTrigger)
		{
			return null;
		}
		return other.GetComponentInParent<SeaMoth>();
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawRay(base.transform.position, base.transform.forward * 10f);
	}
}
