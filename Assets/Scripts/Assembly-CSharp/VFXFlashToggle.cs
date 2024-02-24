using UnityEngine;

public class VFXFlashToggle : MonoBehaviour
{
	private void Awake()
	{
		if (!MiscSettings.flashes)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
