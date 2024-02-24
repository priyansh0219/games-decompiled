using UnityEngine;

public class LookAt : MonoBehaviour
{
	public GameObject target;

	public float minDistance = 10f;

	public float cameraMoveSpeed = 5f;

	private void Update()
	{
		if (target != null)
		{
			Vector3 vector = target.transform.position - base.transform.position;
			if (vector.magnitude > minDistance)
			{
				vector.Normalize();
				base.transform.Translate(vector * cameraMoveSpeed * Time.deltaTime);
			}
			base.transform.LookAt(target.transform.position);
		}
	}
}
