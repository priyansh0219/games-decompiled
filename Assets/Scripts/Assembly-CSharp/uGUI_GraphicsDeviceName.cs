using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class uGUI_GraphicsDeviceName : MonoBehaviour
{
	[AssertNotNull]
	public TextMeshProUGUI text;

	private void Start()
	{
		text.text = SystemInfo.graphicsDeviceName;
	}
}
