using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class MenuTooltip : MonoBehaviour, ITooltip
{
	public string key = string.Empty;

	public bool showTooltipOnDrag => true;

	void ITooltip.GetTooltip(TooltipData data)
	{
		data.prefix.Append(Language.main.Get(key));
	}
}
