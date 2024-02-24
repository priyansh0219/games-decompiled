using UnityEngine;

public class SeaEmperorTeleporterTrigger : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		SeaEmperorBaby componentInParent = other.GetComponentInParent<SeaEmperorBaby>();
		if ((bool)componentInParent)
		{
			componentInParent.Teleport();
		}
	}
}
