using UnityEngine;

public class ItemPrefabData : MonoBehaviour
{
	public Vector3 localPosition = Vector3.zero;

	public Vector3 localRotation = Vector3.zero;

	private Pickupable pickupable;

	private void Awake()
	{
		pickupable = GetComponent<Pickupable>();
	}
}
