using UnityEngine;

public class CollisionLogger : MonoBehaviour
{
	private void OnCollisionEnter(Collision collision)
	{
		Debug.Log("CollisionLogger: " + base.gameObject.name + " OnCollisionEnter with " + collision.gameObject.GetFullHierarchyPath());
	}

	private void OnCollisionExit(Collision collision)
	{
		Debug.Log("CollisionLogger: " + base.gameObject.name + " OnCollisionExit with " + collision.gameObject.GetFullHierarchyPath());
	}

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log("CollisionLogger: " + base.gameObject.name + " OnTriggerEnter with " + other.gameObject.GetFullHierarchyPath());
	}

	private void OnTriggerExit(Collider other)
	{
		Debug.Log("CollisionLogger: " + base.gameObject.name + " OnTriggerExit with " + other.gameObject.GetFullHierarchyPath());
	}
}
