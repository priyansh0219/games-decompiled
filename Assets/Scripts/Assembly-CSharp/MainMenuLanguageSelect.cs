using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuLanguageSelect : MonoBehaviour
{
	public Text currentLanguage;

	public GameObject dropdownContent;

	public GameObject dropdownElementPrefab;

	private void Start()
	{
		currentLanguage.text = Language.main.GetCurrentLanguage().ToUpper();
		string[] files = Directory.GetFiles(SNUtils.InsideUnmanaged("LanguageFiles"));
		foreach (string path in files)
		{
			if (Path.GetExtension(path) == ".json")
			{
				GameObject obj = Object.Instantiate(dropdownElementPrefab);
				obj.transform.SetParent(dropdownContent.transform);
				obj.GetComponentsInChildren<Text>(includeInactive: true)[0].text = Path.GetFileNameWithoutExtension(path);
			}
		}
	}

	public void ToggleDropdown()
	{
		dropdownContent.SetActive(!dropdownContent.activeSelf);
	}
}
