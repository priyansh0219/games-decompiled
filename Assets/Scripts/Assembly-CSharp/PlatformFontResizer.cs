using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class PlatformFontResizer : MonoBehaviour
{
	public float fontSize;

	public bool resizeOnConsole;

	public bool resizeOnPS4;

	public bool resizeOnXBoxOne;

	public bool resizeOnSwitch;

	private void Start()
	{
		if (false)
		{
			GetComponent<TextMeshProUGUI>().fontSize = fontSize;
		}
		Object.Destroy(this);
	}
}
