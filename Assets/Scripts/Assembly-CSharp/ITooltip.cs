public interface ITooltip
{
	bool showTooltipOnDrag { get; }

	void GetTooltip(TooltipData tooltip);
}
