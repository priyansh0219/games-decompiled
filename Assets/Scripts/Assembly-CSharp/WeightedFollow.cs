using UnityEngine;

public class WeightedFollow : MonoBehaviour
{
	[AssertNotNull]
	public Transform target;

	public float weight = 1f;

	private Vector3 prevPos;

	private void Start()
	{
		prevPos = target.position;
	}

	private void Update()
	{
		Vector3 vector = target.position - prevPos;
		base.transform.position = base.transform.position + vector * weight;
		prevPos = target.position;
	}
}
