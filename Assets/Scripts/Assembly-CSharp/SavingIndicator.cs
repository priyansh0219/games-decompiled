using TMPro;
using UnityEngine;

public class SavingIndicator : MonoBehaviour
{
	[AssertNotNull]
	public CanvasGroup canvasGroup;

	[AssertNotNull]
	public TextMeshProUGUI text;

	[AssertLocalization]
	private const string savingGameMessage = "SavingGame";

	private void OnEnable()
	{
		OnLanguageChanged();
		Language.OnLanguageChanged += OnLanguageChanged;
	}

	private void OnDisable()
	{
		Language.OnLanguageChanged -= OnLanguageChanged;
	}

	private void Update()
	{
		canvasGroup.alpha = (SaveLoadManager.main.isSaving ? 1f : 0f);
	}

	private void OnLanguageChanged()
	{
		text.text = Language.main.Get("SavingGame");
	}
}
