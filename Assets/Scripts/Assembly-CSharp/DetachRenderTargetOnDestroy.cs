using UnityEngine;

public class DetachRenderTargetOnDestroy : MonoBehaviour
{
	private void OnDestroy()
	{
		GetComponent<Camera>().targetTexture = null;
	}
}
