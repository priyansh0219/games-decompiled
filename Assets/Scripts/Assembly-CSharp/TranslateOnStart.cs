using TMPro;
using UnityEngine;

public class TranslateOnStart : MonoBehaviour, ILocalizationCheckable
{
	private void Start()
	{
		using (ListPool<TextMeshProUGUI> listPool = Pool<ListPool<TextMeshProUGUI>>.Get())
		{
			GetComponentsInChildren(includeInactive: true, listPool.list);
			foreach (TextMeshProUGUI item in listPool.list)
			{
				item.text = Language.main.Get(item.text);
			}
		}
	}

	public string CompileTimeCheck(ILanguage language)
	{
		string text = null;
		using (ListPool<TextMeshProUGUI> listPool = Pool<ListPool<TextMeshProUGUI>>.Get())
		{
			GetComponentsInChildren(includeInactive: true, listPool.list);
			foreach (TextMeshProUGUI item in listPool.list)
			{
				if (item.gameObject.activeInHierarchy && !string.IsNullOrEmpty(item.text))
				{
					text = language.CheckKey(item.text, allowEmpty: true);
					if (text != null)
					{
						break;
					}
				}
			}
		}
		return text;
	}
}
