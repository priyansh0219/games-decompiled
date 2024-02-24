public interface uGUI_IIconGridManager
{
	void GetTooltip(string id, TooltipData data);

	void OnPointerEnter(string id);

	void OnPointerExit(string id);

	void OnPointerClick(string id, int button);

	void OnSortRequested();
}
