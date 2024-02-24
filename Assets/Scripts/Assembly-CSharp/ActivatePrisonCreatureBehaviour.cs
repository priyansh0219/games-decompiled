using UWE;
using UnityEngine;

public class ActivatePrisonCreatureBehaviour : MonoBehaviour
{
	private void OnTriggerEnter(Collider col)
	{
		PrisonCreatureBehaviour componentInHierarchy = UWE.Utils.GetComponentInHierarchy<PrisonCreatureBehaviour>(col.gameObject);
		if (componentInHierarchy != null)
		{
			componentInHierarchy.Activate();
		}
	}
}
