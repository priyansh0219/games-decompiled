using UnityEngine;

public class AlignWithVelocity : MonoBehaviour
{
	[SerializeField]
	[AssertNotNull]
	private Rigidbody rb;

	[SerializeField]
	private float rotationSpeed = 4f;

	[SerializeField]
	private bool rotateXZOnly;

	private void FixedUpdate()
	{
		Vector3 velocity = rb.velocity;
		if (velocity.sqrMagnitude > 0.04f)
		{
			if (rotateXZOnly)
			{
				velocity.y = 0f;
			}
			Quaternion rot = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(velocity), Time.fixedDeltaTime * rotationSpeed);
			rb.MoveRotation(rot);
		}
	}
}
