using TMPro;
using UnityEngine;

public class MainMenuPlayerName : MonoBehaviour
{
	public TextMeshProUGUI text;

	private void Start()
	{
		if (!PlatformUtils.main.GetServices().GetSupportsDynamicLogOn())
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			text.text = PlatformUtils.main.GetLoggedInUserName();
		}
	}

	private void Update()
	{
		if (!PlatformUtils.main.GetServices().GetSupportsDynamicLogOn())
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			text.text = PlatformUtils.main.GetLoggedInUserName();
		}
	}
}
