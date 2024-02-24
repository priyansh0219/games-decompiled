using UnityEngine;
using UnityEngine.XR;

public class HideForPlatform : MonoBehaviour
{
	public bool hideForDesktop;

	public bool hideForVr;

	public bool hideForConsole;

	public bool hideForPerfectWorldChina;

	public bool hideForPS4;

	public bool hideForPS5;

	public bool hideForXboxOne;

	public bool hideForGameCoreScarlett;

	public bool hideForSwitch;

	private void Start()
	{
		bool flag = false;
		if ((!XRSettings.enabled) ? (flag || hideForDesktop) : (flag || hideForVr))
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
