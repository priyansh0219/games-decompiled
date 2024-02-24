using System;
using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class uGUI_BuildWatermark : MonoBehaviour
{
	[AssertLocalization(2)]
	private const string watermarkFormat = "EarlyAccessWatermarkFormat";

	[AssertNotNull]
	public TextMeshProUGUI text;

	private IEnumerator Start()
	{
		UpdateText();
		Language.OnLanguageChanged += OnLanguageChanged;
		while (!uGUI.isMainLevel || WaitScreen.IsWaiting)
		{
			yield return null;
		}
		base.gameObject.SetActive(value: true);
	}

	private void OnDestroy()
	{
		Language.OnLanguageChanged -= OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		UpdateText();
	}

	private void UpdateText()
	{
		string plasticChangeSetOfBuild = SNUtils.GetPlasticChangeSetOfBuild();
		DateTime dateTimeOfBuild = SNUtils.GetDateTimeOfBuild();
		text.text = Language.main.GetFormat("EarlyAccessWatermarkFormat", dateTimeOfBuild, plasticChangeSetOfBuild);
	}
}
