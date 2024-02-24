using UnityEngine;

public class ScreenFXWireFrame : MonoBehaviour
{
	private void OnPreRender()
	{
		GL.wireframe = true;
	}

	private void OnPostRender()
	{
		GL.wireframe = false;
	}
}
