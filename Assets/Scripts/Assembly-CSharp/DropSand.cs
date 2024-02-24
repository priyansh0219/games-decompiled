using UnityEngine;

public class DropSand : MonoBehaviour
{
	public ParticleSystem dropSandVFXPrefab;

	private void OnEnable()
	{
		base.gameObject.GetComponent<Pickupable>().droppedEvent.AddHandler(base.gameObject, OnDropped);
	}

	private void OnDropped(Pickupable pickupable)
	{
		Vector3 position = MainCamera.camera.transform.position + MainCamera.camera.transform.forward;
		Utils.PlayOneShotPS(dropSandVFXPrefab.gameObject, position, Quaternion.identity);
		Object.Destroy(base.gameObject);
	}
}
