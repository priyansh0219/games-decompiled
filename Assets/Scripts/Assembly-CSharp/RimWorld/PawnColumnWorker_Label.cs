using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_Label : PawnColumnWorker
	{
		private const int LeftMargin = 3;

		private const float PortraitCameraZoom = 1.2f;

		private static Dictionary<string, string> labelCache = new Dictionary<string, string>();

		private static float labelCacheForWidth = -1f;

		protected virtual TextAnchor LabelAlignment => TextAnchor.MiddleLeft;

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			Rect rect2 = new Rect(rect.x, rect.y, rect.width, Mathf.Min(rect.height, def.groupable ? rect.height : ((float)GetMinCellHeight(pawn))));
			Rect rect3 = rect2;
			rect3.xMin += 3f;
			if (def.showIcon)
			{
				rect3.xMin += rect2.height;
				Widgets.ThingIcon(new Rect(rect2.x, rect2.y, rect2.height, rect2.height), pawn);
			}
			if (pawn.health.summaryHealth.SummaryHealthPercent < 0.99f)
			{
				Rect rect4 = new Rect(rect3.x - 3f, rect3.y, rect3.width + 3f, rect3.height);
				rect4.yMin += 4f;
				rect4.yMax -= 6f;
				Widgets.FillableBar(rect4, pawn.health.summaryHealth.SummaryHealthPercent, GenMapUI.OverlayHealthTex, BaseContent.ClearTex, doBorder: false);
			}
			if (Mouse.IsOver(rect2))
			{
				GUI.DrawTexture(rect2, TexUI.HighlightTex);
			}
			string text = ((!pawn.RaceProps.Humanlike && !pawn.RaceProps.Animal && pawn.Name != null && !pawn.Name.Numerical) ? (pawn.Name.ToStringShort.CapitalizeFirst() + ", " + pawn.KindLabel.Colorize(ColoredText.SubtleGrayColor)) : ((!def.useLabelShort) ? pawn.LabelNoCount.CapitalizeFirst() : pawn.LabelShortCap));
			if (rect3.width != labelCacheForWidth)
			{
				labelCacheForWidth = rect3.width;
				labelCache.Clear();
			}
			if (Text.CalcSize(text.StripTags()).x > rect3.width)
			{
				text = text.StripTags().Truncate(rect3.width, labelCache);
			}
			if (pawn.IsSlave || pawn.IsColonyMech)
			{
				text = text.Colorize(PawnNameColorUtility.PawnNameColorOf(pawn));
			}
			Text.Font = GameFont.Small;
			Text.Anchor = LabelAlignment;
			Text.WordWrap = false;
			Widgets.Label(rect3, text);
			Text.WordWrap = true;
			Text.Anchor = TextAnchor.UpperLeft;
			if (Widgets.ButtonInvisible(rect2))
			{
				CameraJumper.TryJumpAndSelect(pawn);
				if (Current.ProgramState == ProgramState.Playing && Event.current.button == 0)
				{
					Find.MainTabsRoot.EscapeCurrentTab(playSound: false);
				}
			}
			else if (Mouse.IsOver(rect2))
			{
				TipSignal tooltip = pawn.GetTooltip();
				tooltip.text = "ClickToJumpTo".Translate() + "\n\n" + tooltip.text;
				TooltipHandler.TipRegion(rect2, tooltip);
			}
		}

		public override int GetMinWidth(PawnTable table)
		{
			return Mathf.Max(base.GetMinWidth(table), 80);
		}

		public override int GetOptimalWidth(PawnTable table)
		{
			return Mathf.Clamp(165, GetMinWidth(table), GetMaxWidth(table));
		}
	}
}
