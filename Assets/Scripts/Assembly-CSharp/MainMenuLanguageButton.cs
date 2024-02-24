using UnityEngine;
using UnityEngine.UI;

public class MainMenuLanguageButton : MonoBehaviour
{
	public void ChangeLanguage()
	{
		string text = GetComponentsInChildren<Text>()[0].text;
		Language.main.SetCurrentLanguage(ToTitleCase(text));
		base.transform.parent.gameObject.SetActive(value: false);
		GameObject.Find("Current language label").GetComponent<Text>().text = ToTitleCase(text);
	}

	private string ToTitleCase(string text)
	{
		string text2 = text[0].ToString();
		if (text.Length <= 0)
		{
			return text;
		}
		return text2.ToUpper() + text.Substring(1).ToLower();
	}
}
