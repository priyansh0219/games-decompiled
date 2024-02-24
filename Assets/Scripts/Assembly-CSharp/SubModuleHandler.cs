using UnityEngine;

public class SubModuleHandler : MonoBehaviour
{
	private void Start()
	{
		CheckRigidBody();
	}

	private void CheckRigidBody()
	{
		if (GetComponentInParent<SubRoot>() != null)
		{
			Rigidbody component = GetComponent<Rigidbody>();
			if (component != null)
			{
				Object.Destroy(component);
			}
		}
	}
}
