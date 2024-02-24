using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SimpleTooltip : MonoBehaviour, ITooltip, ILocalizationCheckable
{
	public bool translate = true;

	public string text = "Tooltip";

	public bool showTooltipOnDrag => false;

	void ITooltip.GetTooltip(TooltipData data)
	{
		data.prefix.Append(translate ? Language.main.Get(text) : text);
	}

	public string CompileTimeCheck(ILanguage language)
	{
		if (!translate)
		{
			return null;
		}
		return language.CheckKey(text, allowEmpty: true);
	}
}
