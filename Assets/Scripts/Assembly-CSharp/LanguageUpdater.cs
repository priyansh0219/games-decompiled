using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class LanguageUpdater : MonoBehaviour, ILocalizationCheckable
{
	public bool debug;

	private TextMesh[] textMeshes;

	private GUIText[] guiTextStrings;

	private TextMeshProUGUI[] uguiTextStrings;

	private Dictionary<string, string> reverseStrings = new Dictionary<string, string>();

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
		ReverseChildrenTranslation(base.gameObject);
		TranslateChildren(base.gameObject);
	}

	public void TranslateChildren(GameObject parent)
	{
		textMeshes = parent.GetComponentsInChildren<TextMesh>(includeInactive: true);
		TextMesh[] array = textMeshes;
		foreach (TextMesh textMesh in array)
		{
			textMesh.text = GetReversible(textMesh.text);
		}
		guiTextStrings = parent.GetComponentsInChildren<GUIText>(includeInactive: true);
		GUIText[] array2 = guiTextStrings;
		foreach (GUIText gUIText in array2)
		{
			gUIText.text = GetReversible(gUIText.text);
		}
		uguiTextStrings = parent.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
		TextMeshProUGUI[] array3 = uguiTextStrings;
		foreach (TextMeshProUGUI textMeshProUGUI in array3)
		{
			textMeshProUGUI.text = GetReversible(textMeshProUGUI.text);
		}
	}

	public void ReverseChildrenTranslation(GameObject parent)
	{
		textMeshes = parent.GetComponentsInChildren<TextMesh>(includeInactive: true);
		TextMesh[] array = textMeshes;
		foreach (TextMesh textMesh in array)
		{
			textMesh.text = GetReverse(textMesh.text);
		}
		uguiTextStrings = parent.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
		TextMeshProUGUI[] array2 = uguiTextStrings;
		foreach (TextMeshProUGUI textMeshProUGUI in array2)
		{
			textMeshProUGUI.text = GetReverse(textMeshProUGUI.text);
		}
	}

	private string GetReversible(string key)
	{
		string text = Language.main.Get(key);
		reverseStrings[text] = key;
		return text;
	}

	private string GetReverse(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return "";
		}
		if (!reverseStrings.TryGetValue(key, out var value))
		{
			if (debug)
			{
				Debug.LogWarningFormat(this, "no reverse translation for key: '{0}'", key);
			}
			return key;
		}
		return value;
	}

	public string CompileTimeCheck(ILanguage language)
	{
		StringBuilder stringBuilder = new StringBuilder();
		textMeshes = base.gameObject.GetComponentsInChildren<TextMesh>(includeInactive: true);
		TextMesh[] array = textMeshes;
		foreach (TextMesh textMesh in array)
		{
			string text = language.CheckKey(textMesh.text, allowEmpty: true);
			if (text != null)
			{
				stringBuilder.AppendLine(text);
			}
		}
		guiTextStrings = base.gameObject.GetComponentsInChildren<GUIText>(includeInactive: true);
		GUIText[] array2 = guiTextStrings;
		foreach (GUIText gUIText in array2)
		{
			string text2 = language.CheckKey(gUIText.text, allowEmpty: true);
			if (text2 != null)
			{
				stringBuilder.AppendLine(text2);
			}
		}
		uguiTextStrings = base.gameObject.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
		TextMeshProUGUI[] array3 = uguiTextStrings;
		foreach (TextMeshProUGUI textMeshProUGUI in array3)
		{
			string text3 = language.CheckKey(textMeshProUGUI.text, allowEmpty: true);
			if (text3 != null)
			{
				stringBuilder.AppendLine(text3);
			}
		}
		if (stringBuilder.Length <= 0)
		{
			return null;
		}
		return stringBuilder.ToString();
	}
}
