using UnityEngine;

public class TrailSegment : MonoBehaviour
{
	public Transform attachedTo;

	public float stiffness = 0.5f;

	public float rotateSpeed = 0.1f;

	private Vector3 targetPos;

	private Vector3 currentPos;

	private float distance;

	private void Start()
	{
		distance = (base.transform.position - attachedTo.position).magnitude;
		targetPos = attachedTo.position;
		currentPos = base.transform.position;
	}

	private void FixedUpdate()
	{
		Vector3 vector = (targetPos - currentPos) * stiffness * Time.deltaTime + currentPos;
		base.transform.position = targetPos + Vector3.Normalize(vector - targetPos) * distance;
		targetPos = attachedTo.position;
		currentPos = base.transform.position;
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, attachedTo.rotation, rotateSpeed);
	}
}
