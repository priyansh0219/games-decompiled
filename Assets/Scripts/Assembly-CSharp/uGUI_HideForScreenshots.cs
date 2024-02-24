using UnityEngine;

public class uGUI_HideForScreenshots : MonoBehaviour
{
	public Canvas canvas;

	private void HideForScreenshots()
	{
		SetCanvasState(state: false);
	}

	private void UnhideForScreenshots()
	{
		SetCanvasState(state: true);
	}

	private void SetCanvasState(bool state)
	{
		if (canvas != null)
		{
			canvas.enabled = state;
		}
	}
}
