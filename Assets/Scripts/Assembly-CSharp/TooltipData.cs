using System.Collections.Generic;
using System.Text;

public class TooltipData
{
	public readonly StringBuilder prefix = new StringBuilder();

	public readonly List<TooltipIcon> icons = new List<TooltipIcon>();

	public readonly StringBuilder postfix = new StringBuilder();

	public void Reset()
	{
		prefix.Length = 0;
		icons.Clear();
		postfix.Length = 0;
	}
}
