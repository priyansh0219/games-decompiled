using UnityEngine;

public class InteractionVolumeCollider : MonoBehaviour
{
	public InteractionVolume owner;

	private void Start()
	{
		if (GetComponent<Collider>() == null)
		{
			Debug.LogError("No collider for " + base.gameObject.name + " with an InteractionVolume");
		}
		else if (!GetComponent<Collider>().isTrigger)
		{
			Debug.LogError("Collider for " + base.gameObject.name + " is not a trigger!");
		}
		base.gameObject.layer = LayerMask.NameToLayer("Useable");
	}

	private void OnTriggerEnter(Collider other)
	{
		InteractionVolumeUser interactionVolumeUser = other.gameObject.FindAncestor<InteractionVolumeUser>();
		if (interactionVolumeUser != null)
		{
			interactionVolumeUser.OnEnterVolume(owner);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		InteractionVolumeUser interactionVolumeUser = other.gameObject.FindAncestor<InteractionVolumeUser>();
		if (interactionVolumeUser != null)
		{
			interactionVolumeUser.OnExitVolume(owner);
		}
	}
}
