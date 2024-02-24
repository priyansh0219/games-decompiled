using TMPro;
using UnityEngine;

public class TranslationLiveUpdate : MonoBehaviour
{
	[AssertLocalization(AssertLocalizationAttribute.Options.AllowEmptyString)]
	public string translationKey;

	[AssertNotNull]
	public TextMeshProUGUI textComponent;

	private void OnEnable()
	{
		Language.OnLanguageChanged += OnLanguageChanged;
		OnLanguageChanged();
	}

	private void OnDisable()
	{
		Language.OnLanguageChanged -= OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		if (!string.IsNullOrEmpty(textComponent.text))
		{
			if (string.IsNullOrEmpty(translationKey))
			{
				translationKey = textComponent.text;
			}
			textComponent.text = Language.main.Get(translationKey);
		}
	}
}
