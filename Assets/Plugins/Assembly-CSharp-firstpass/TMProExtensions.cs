using TMPro;
using UnityEngine;

public static class TMProExtensions
{
	public static TextAlignmentOptions ToAlignment(this TextAnchor anchor)
	{
		switch (anchor)
		{
		default:
			return TextAlignmentOptions.TopLeft;
		case TextAnchor.UpperCenter:
			return TextAlignmentOptions.Top;
		case TextAnchor.UpperRight:
			return TextAlignmentOptions.TopRight;
		case TextAnchor.MiddleLeft:
			return TextAlignmentOptions.Left;
		case TextAnchor.MiddleCenter:
			return TextAlignmentOptions.Center;
		case TextAnchor.MiddleRight:
			return TextAlignmentOptions.Right;
		case TextAnchor.LowerLeft:
			return TextAlignmentOptions.BottomLeft;
		case TextAnchor.LowerCenter:
			return TextAlignmentOptions.Bottom;
		case TextAnchor.LowerRight:
			return TextAlignmentOptions.BottomRight;
		}
	}

	public static void SetAlpha(this TextMeshProUGUI text, float alpha)
	{
		text.alpha = alpha;
	}
}
