using UnityEngine;

public class AnglesConstraint : MonoBehaviour
{
	public Vector3 desiredAngle;

	private void LateUpdate()
	{
		base.transform.eulerAngles = desiredAngle;
	}
}
