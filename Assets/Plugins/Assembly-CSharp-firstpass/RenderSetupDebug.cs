using UnityEngine;

[ExecuteInEditMode]
public class RenderSetupDebug : MonoBehaviour
{
	private void OnPreRender()
	{
		Debug.Log(Time.frameCount + " OnPreRender for " + base.gameObject.name + ", RenderTexture.active=" + RenderTexture.active.NullOrID() + ", camera.targetTexture=" + GetComponent<Camera>().targetTexture.NullOrID());
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		Debug.Log(Time.frameCount + " OnRenderImage for " + base.gameObject.name + ", RenderTexture.active=" + RenderTexture.active.NullOrID() + ", camera.targetTexture=" + GetComponent<Camera>().targetTexture.NullOrID());
	}
}
