using UnityEngine;

public class InteractionVolume : MonoBehaviour, ICompileTimeCheckable
{
	public string CompileTimeCheck()
	{
		Collider[] componentsInChildren = base.gameObject.GetComponentsInChildren<Collider>(includeInactive: true);
		foreach (Collider collider in componentsInChildren)
		{
			if (collider.isTrigger)
			{
				InteractionVolumeCollider component = collider.gameObject.GetComponent<InteractionVolumeCollider>();
				if (component == null)
				{
					return $"Collider '{collider.gameObject.name}' without and InteractionVolumeCollider";
				}
				if (component.owner != this)
				{
					return $"InteractionVolumeCollider '{collider.gameObject.name}' that doesn't have the owner set to '{base.gameObject.name}'";
				}
			}
		}
		return null;
	}
}
