using UWE;
using UnityEngine;

public class GrabcrabHome : MonoBehaviour
{
	public GameObject grabcrabPrefab;

	private Grabcrab grabcrab;

	public float kLootCheckRadius;

	private float timeLastCheck;

	private void Awake()
	{
		GameObject gameObject = Object.Instantiate(grabcrabPrefab, base.gameObject.transform.position, Quaternion.identity);
		grabcrab = gameObject.GetComponent<Grabcrab>();
		grabcrab.home = this;
	}

	private void Update()
	{
		if (!grabcrab.IsFree() || !(Time.time - timeLastCheck > 2f))
		{
			return;
		}
		int num = UWE.Utils.OverlapSphereIntoSharedBuffer(base.transform.position, kLootCheckRadius);
		for (int i = 0; i < num; i++)
		{
			Pickupable component = UWE.Utils.sharedColliderBuffer[i].gameObject.GetComponent<Pickupable>();
			if ((bool)component && component.isPickupable && new Vector3(component.gameObject.transform.position.x - base.transform.position.x, 0f, component.gameObject.transform.position.z - base.transform.position.z).magnitude > 3f)
			{
				grabcrab.AlertToItem(component);
				break;
			}
		}
		timeLastCheck = Time.time;
	}
}
