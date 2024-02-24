using UnityEngine;

public class VFXPushBodiesFromCenter : MonoBehaviour
{
	public float force = 10f;

	private bool exploded;

	private Rigidbody[] bodies;

	private Vector3 center;

	private void Start()
	{
		center = base.transform.position;
		bodies = GetComponentsInChildren<Rigidbody>();
	}

	private void Update()
	{
		if (exploded)
		{
			Object.Destroy(this);
			return;
		}
		for (int i = 0; i < bodies.Length; i++)
		{
			Rigidbody obj = bodies[i];
			obj.transform.parent = null;
			Vector3 vector = Vector3.Normalize(obj.transform.position - center);
			obj.AddForce(force * vector, ForceMode.Force);
		}
		exploded = true;
	}
}
