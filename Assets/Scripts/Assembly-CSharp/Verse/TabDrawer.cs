using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public static class TabDrawer
	{
		private const float MaxTabWidth = 200f;

		public const float TabHeight = 32f;

		public const float TabHoriztonalOverlap = 10f;

		private static List<TabRecord> tmpTabs = new List<TabRecord>();

		public static TabRecord DrawTabs(Rect baseRect, List<TabRecord> tabs, int rows)
		{
			if (rows <= 1)
			{
				return DrawTabs(baseRect, tabs);
			}
			int num = Mathf.FloorToInt(tabs.Count / rows);
			int num2 = 0;
			TabRecord result = null;
			Rect rect = baseRect;
			baseRect.yMin -= (float)(rows - 1) * 31f;
			Rect rect2 = baseRect;
			rect2.yMax = rect.y;
			Widgets.DrawMenuSection(rect2);
			for (int i = 0; i < rows; i++)
			{
				int num3 = ((i == 0) ? (tabs.Count - (rows - 1) * num) : num);
				tmpTabs.Clear();
				for (int j = num2; j < num2 + num3; j++)
				{
					tmpTabs.Add(tabs[j]);
				}
				TabRecord tabRecord = DrawTabs(baseRect, tmpTabs, baseRect.width);
				if (tabRecord != null)
				{
					result = tabRecord;
				}
				baseRect.yMin += 31f;
				num2 += num3;
			}
			tmpTabs.Clear();
			return result;
		}

		public static TTabRecord DrawTabs<TTabRecord>(Rect baseRect, List<TTabRecord> tabs, float maxTabWidth = 200f) where TTabRecord : TabRecord
		{
			TTabRecord val = null;
			TTabRecord val2 = tabs.Find((TTabRecord t) => t.Selected);
			float num = baseRect.width + (float)(tabs.Count - 1) * 10f;
			float tabWidth = num / (float)tabs.Count;
			if (tabWidth > maxTabWidth)
			{
				tabWidth = maxTabWidth;
			}
			Rect rect = new Rect(baseRect);
			rect.y -= 32f;
			rect.height = 9999f;
			Widgets.BeginGroup(rect);
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Small;
			Func<TTabRecord, Rect> func = (TTabRecord tab) => new Rect((float)tabs.IndexOf(tab) * (tabWidth - 10f), 1f, tabWidth, 32f);
			List<TTabRecord> list = tabs.ListFullCopy();
			if (val2 != null)
			{
				list.Remove(val2);
				list.Add(val2);
			}
			TabRecord tabRecord = null;
			List<TTabRecord> list2 = list.ListFullCopy();
			list2.Reverse();
			for (int i = 0; i < list2.Count; i++)
			{
				TTabRecord val3 = list2[i];
				Rect rect2 = func(val3);
				if (tabRecord == null && Mouse.IsOver(rect2))
				{
					tabRecord = val3;
				}
				MouseoverSounds.DoRegion(rect2, SoundDefOf.Mouseover_Tab);
				if (Widgets.ButtonInvisible(rect2))
				{
					val = val3;
				}
			}
			foreach (TTabRecord item in list)
			{
				Rect rect3 = func(item);
				item.Draw(rect3);
			}
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.EndGroup();
			if (val != null && val != val2)
			{
				SoundDefOf.RowTabSelect.PlayOneShotOnCamera();
				if (val.clickedAction != null)
				{
					val.clickedAction();
				}
			}
			return val;
		}
	}
}
