using System.Text;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SubtitlesSpeedTooltip : MonoBehaviour, ITooltip
{
	[AssertLocalization]
	private const string key = "SubtitlesSpeedTooltip";

	private int lastFrame;

	private float num;

	public bool showTooltipOnDrag => true;

	void ITooltip.GetTooltip(TooltipData data)
	{
		string text = Language.main.Get("SubtitlesSpeedTooltip");
		int frameCount = Time.frameCount;
		if (lastFrame != frameCount)
		{
			if (frameCount - 1 != lastFrame)
			{
				this.num = 0f;
			}
			else
			{
				int num = 0;
				for (int i = 0; i < text.Length; i++)
				{
					if (!Subtitles.ShouldSkipCharacter(text[i]))
					{
						num++;
					}
				}
				this.num += Time.unscaledDeltaTime * Subtitles.speed;
				this.num %= num + 1;
			}
			lastFrame = frameCount;
		}
		int num2 = Mathf.FloorToInt(this.num);
		for (int j = 0; j < num2; j++)
		{
			if (Subtitles.ShouldSkipCharacter(text[j]))
			{
				num2++;
			}
		}
		StringBuilder prefix = data.prefix;
		prefix.Append("<color=#FFFFFFFF>");
		prefix.Append(text, 0, num2);
		prefix.Append("</color>");
		prefix.Append("<color=#707070FF>");
		prefix.Append(text, num2, text.Length - num2);
		prefix.Append("</color>");
	}
}
