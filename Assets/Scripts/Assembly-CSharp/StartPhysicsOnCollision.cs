using UWE;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StartPhysicsOnCollision : MonoBehaviour
{
	private void Awake()
	{
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(GetComponent<Rigidbody>(), isKinematic: true);
	}

	private void OnCollisionEnter(Collision collision)
	{
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(GetComponent<Rigidbody>(), isKinematic: false);
	}
}
