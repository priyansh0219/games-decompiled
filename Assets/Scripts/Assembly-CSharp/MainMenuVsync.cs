using UnityEngine;
using UnityEngine.UI;

public class MainMenuVsync : MonoBehaviour
{
	public Toggle toggle;

	private void Start()
	{
		if (QualitySettings.vSyncCount == 0)
		{
			toggle.isOn = false;
		}
		else
		{
			toggle.isOn = true;
		}
	}

	public void ToggleVsync(bool value)
	{
		if (value)
		{
			toggle.isOn = true;
			QualitySettings.vSyncCount = 1;
		}
		else
		{
			toggle.isOn = false;
			QualitySettings.vSyncCount = 0;
		}
	}
}
