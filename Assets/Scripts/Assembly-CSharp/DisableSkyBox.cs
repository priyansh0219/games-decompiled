using UnityEngine;

public class DisableSkyBox : MonoBehaviour
{
	public float threshold = 1f;

	private Material skyMaterial;

	private void Update()
	{
		if (MainCamera.camera.transform.position.y < 0f - threshold)
		{
			if (RenderSettings.skybox != null)
			{
				skyMaterial = RenderSettings.skybox;
				RenderSettings.skybox = null;
			}
		}
		else if (RenderSettings.skybox == null)
		{
			RenderSettings.skybox = skyMaterial;
		}
	}
}
