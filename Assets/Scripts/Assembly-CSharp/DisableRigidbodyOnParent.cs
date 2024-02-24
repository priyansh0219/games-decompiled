using UnityEngine;

public class DisableRigidbodyOnParent : MonoBehaviour
{
	private bool isEnabled = true;

	private void FixedUpdate()
	{
		bool flag = base.transform.parent == null;
		if (flag != isEnabled)
		{
			isEnabled = flag;
			GetComponent<Rigidbody>().detectCollisions = flag;
		}
	}
}
