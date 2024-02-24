using UnityEngine;
using UnityEngine.XR;

public class uGUI_SafeAreaScaler : MonoBehaviour
{
	public Rect vrSafeRect = new Rect(0.15f, 0.25f, 0.7f, 0.5f);

	public Rect consoleSafeRect = new Rect(0.01f, 0.05f, 0.98f, 0.9f);

	private Rect disableRect = new Rect(0f, 0f, 1f, 1f);

	public bool disabledOnPS4;

	public bool disabledOnXBoxOne;

	public bool disabledOnSwitch;

	private bool disableOverride;

	public void SetDisabledState(bool state)
	{
		disableOverride = state;
	}

	private void Update()
	{
		RectTransform component = GetComponent<RectTransform>();
		Rect rect = disableRect;
		if (disableOverride)
		{
			rect = disableRect;
		}
		else if (XRSettings.enabled)
		{
			rect = vrSafeRect;
		}
		component.anchorMin = rect.min;
		component.anchorMax = rect.max;
	}

	private bool IsDisabledOnCurrentConsolePlatform()
	{
		return false;
	}
}
