using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RocketPreflightCheckScreenElement : MonoBehaviour
{
	[AssertNotNull]
	public RocketPreflightCheckManager preflightCheckManager;

	[AssertNotNull]
	public TextMeshProUGUI localizedCheckText;

	[AssertNotNull]
	public GameObject activeLight;

	[AssertNotNull]
	public Image icon;

	private Color baseTextColor;

	public PreflightCheck preflightCheck;

	private bool complete;

	private void Start()
	{
		localizedCheckText.text = preflightCheckManager.ReturnLocalizedPreflightCheckName(preflightCheck);
		baseTextColor = localizedCheckText.color;
		complete = preflightCheckManager.GetPreflightComplete(preflightCheck);
		if (!complete)
		{
			localizedCheckText.color = Color.red;
			icon.color = Color.red;
		}
		activeLight.SetActive(complete);
	}

	public void SetPreflightCheckComplete(PreflightCheck setPreflightComplete)
	{
		if (setPreflightComplete == preflightCheck)
		{
			complete = true;
			activeLight.SetActive(value: true);
			icon.color = Color.white;
			localizedCheckText.color = baseTextColor;
		}
	}

	public void SetPreflightCheckIncomplete(PreflightCheck setPreflightComplete)
	{
		if (setPreflightComplete == preflightCheck)
		{
			complete = false;
			activeLight.SetActive(value: false);
			icon.color = Color.red;
			localizedCheckText.color = Color.red;
		}
	}
}
