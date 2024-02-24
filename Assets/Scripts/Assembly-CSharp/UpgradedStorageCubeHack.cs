using UnityEngine;

public class UpgradedStorageCubeHack : MonoBehaviour
{
	private void Start()
	{
		BoxCollider component = base.gameObject.GetComponent<BoxCollider>();
		if ((bool)component)
		{
			Object.Destroy(component);
		}
		Object.Destroy(this);
	}
}
