using System;
using UnityEngine;

namespace Verse
{
	public struct TextBlock : IDisposable
	{
		private GameFont oldFont;

		private TextAnchor oldAnchor;

		private bool oldWordWrap;

		public TextBlock(GameFont newFont)
			: this(newFont, null, null)
		{
		}

		public TextBlock(TextAnchor newAnchor)
			: this(null, newAnchor, null)
		{
		}

		public TextBlock(bool newWordWrap)
			: this(null, null, newWordWrap)
		{
		}

		public TextBlock(GameFont? newFont, TextAnchor? newAnchor, bool? newWordWrap)
		{
			oldFont = Text.Font;
			oldAnchor = Text.Anchor;
			oldWordWrap = Text.WordWrap;
			if (newFont.HasValue)
			{
				Text.Font = newFont.Value;
			}
			if (newAnchor.HasValue)
			{
				Text.Anchor = newAnchor.Value;
			}
			if (newWordWrap.HasValue)
			{
				Text.WordWrap = newWordWrap.Value;
			}
		}

		public static TextBlock Default()
		{
			return new TextBlock(GameFont.Small, TextAnchor.UpperLeft, false);
		}

		public void Dispose()
		{
			Text.Font = oldFont;
			Text.Anchor = oldAnchor;
			Text.WordWrap = oldWordWrap;
		}
	}
}
