using UnityEngine;

public class PDACameraFOVControl : MonoBehaviour
{
	private const float speed = 3f;

	private void Update()
	{
		PDA pDA = Player.main.GetPDA();
		SNCameraRoot main = SNCameraRoot.main;
		float b = (pDA.isInUse ? 60f : MiscSettings.fieldOfView);
		if (!Mathf.Approximately(main.CurrentFieldOfView, b))
		{
			float fov = Mathf.Lerp(main.CurrentFieldOfView, b, Time.unscaledDeltaTime * 3f);
			main.SetFov(fov);
		}
	}

	private void OnDestroy()
	{
		SNCameraRoot.main.SyncFieldOfView();
	}
}
