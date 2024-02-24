using UnityEngine;

public class LightCircle : MonoBehaviour
{
	public GUITexture guiTexture;

	private float growTime = 0.1f;

	private float lifetime = 1f;

	public EcoEvent ecoEvent;

	private void Update()
	{
		float timeCreated = ecoEvent.GetTimeCreated();
		bool flag = Time.time >= timeCreated;
		guiTexture.enabled = flag;
		base.transform.position = ecoEvent.GetPosition();
		if (Time.time >= timeCreated + lifetime)
		{
			Object.Destroy(base.gameObject);
		}
		else if (flag)
		{
			Vector2 vector = MainCamera.camera.WorldToScreenPoint(ecoEvent.GetPosition());
			float x = vector.x / (float)Screen.width;
			float y = vector.y / (float)Screen.height;
			base.gameObject.transform.position = new Vector3(x, y, guiTexture.transform.position.z);
		}
	}
}
