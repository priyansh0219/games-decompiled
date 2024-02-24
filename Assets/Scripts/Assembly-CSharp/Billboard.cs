using UnityEngine;

public class Billboard : MonoBehaviour
{
	private void LateUpdate()
	{
		base.transform.rotation = Quaternion.LookRotation(-MainCamera.camera.transform.forward);
	}
}
