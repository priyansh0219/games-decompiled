using UnityEngine;

public class CyclopsDecoyLauncher : MonoBehaviour
{
	[AssertNotNull]
	public GameObject decoyPrefab;

	public void LaunchDecoy()
	{
		CyclopsDecoy component = Object.Instantiate(decoyPrefab, base.transform.position, Quaternion.identity).GetComponent<CyclopsDecoy>();
		if ((bool)component)
		{
			component.launch = true;
		}
	}
}
