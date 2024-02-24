using UnityEngine;

public class CyclopsCompassHUD : MonoBehaviour
{
	[AssertNotNull]
	public GameObject compass;

	[AssertNotNull]
	public BehaviourLOD LOD;

	private void Update()
	{
		if (LOD.IsFull())
		{
			compass.transform.rotation = Quaternion.LookRotation(Vector3.forward);
		}
	}
}
