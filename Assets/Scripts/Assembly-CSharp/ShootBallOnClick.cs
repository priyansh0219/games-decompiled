using UWE;
using UnityEngine;

public class ShootBallOnClick : MonoBehaviour
{
	public float ballImpulse = 5f;

	public float ballLifetime = 5f;

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			obj.name = "Ball";
			obj.transform.parent = null;
			obj.transform.position = base.transform.position - base.transform.up;
			obj.transform.rotation = base.transform.rotation;
			obj.transform.localScale = Vector3.one;
			Rigidbody rigidbody = obj.AddComponent<Rigidbody>();
			rigidbody.useGravity = true;
			UWE.Utils.SetIsKinematicAndUpdateInterpolation(rigidbody, isKinematic: false);
			rigidbody.AddForce((base.transform.forward + base.transform.up) * ballImpulse, ForceMode.Impulse);
			Object.Destroy(obj, ballLifetime);
		}
	}
}
