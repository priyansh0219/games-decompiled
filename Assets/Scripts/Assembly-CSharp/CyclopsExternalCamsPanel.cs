using UnityEngine;

public class CyclopsExternalCamsPanel : MonoBehaviour
{
	public CyclopsExternalCams cyclopsExternalCams;

	public GameObject uiCameraPanel;

	private void Start()
	{
		Player.main.playerModeChanged.AddHandler(base.gameObject, OnPlayerModeChange);
	}

	public void OnPlayerModeChange(Player.Mode mode)
	{
		if (mode == Player.Mode.Piloting)
		{
			uiCameraPanel.SetActive(value: true);
		}
		else
		{
			uiCameraPanel.SetActive(value: false);
		}
	}

	public void CameraButtonActivated()
	{
		cyclopsExternalCams.SetActive(value: true);
	}
}
