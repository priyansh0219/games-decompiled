using UnityEngine;

public class HideHUD : MonoBehaviour
{
	protected bool sshotHide;

	private void HideForScreenshots()
	{
		sshotHide = true;
	}

	private void UnhideForScreenshots()
	{
		sshotHide = false;
	}
}
