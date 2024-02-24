using UnityEngine;

public class AlignWithMotion : MonoBehaviour
{
	public float rotationSpeed;

	public bool rotateXZOnly;

	private Vector3 prevPos;

	private void Start()
	{
		prevPos = base.transform.position;
	}

	private void Update()
	{
		Vector3 normalized = (base.transform.position - prevPos).normalized;
		if ((double)normalized.sqrMagnitude > 0.0001)
		{
			prevPos = base.transform.position;
			if (rotateXZOnly)
			{
				Vector3 forward = new Vector3(normalized.x, 0f, normalized.z);
				base.transform.rotation = Quaternion.Slerp(base.transform.rotation, Quaternion.LookRotation(forward), Time.deltaTime * rotationSpeed);
			}
			else
			{
				base.transform.rotation = Quaternion.Slerp(base.transform.rotation, Quaternion.LookRotation(normalized), Time.deltaTime * rotationSpeed);
			}
		}
	}
}
