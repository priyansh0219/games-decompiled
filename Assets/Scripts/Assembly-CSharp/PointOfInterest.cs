using UnityEngine;

public class PointOfInterest : MonoBehaviour
{
	public new string name = "POI";

	public Transform target;

	public Collider collider;

	private void OnTriggerEnter(Collider other)
	{
		POIMemory component = other.gameObject.GetComponent<POIMemory>();
		if (component != null)
		{
			component.Add(this);
			Object.Destroy(collider);
			if (GetComponent<Rigidbody>() != null)
			{
				Object.Destroy(GetComponent<Rigidbody>());
			}
		}
	}
}
