using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_DialogButton : MonoBehaviour
{
	[AssertNotNull]
	public RectTransform rectTransform;

	[AssertNotNull]
	public Button button;

	[AssertNotNull]
	public TextMeshProUGUI text;

	[NonSerialized]
	public int option = -1;

	[NonSerialized]
	public Action<int> action;

	public void OnClick()
	{
		if (action != null)
		{
			action(option);
		}
	}
}
