using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Dialog_AssignBuildingOwner : Window
	{
		private CompAssignableToPawn assignable;

		private Vector2 scrollPosition;

		private const float EntryHeight = 35f;

		private const int IconSize = 24;

		private const int AssignButtonWidth = 165;

		private const int contextHash = 1279515574;

		private const int AssignedUnassignedSeparatorHeight = 7;

		private const float PawnPortraitCameraZoom = 1.2f;

		private static Dictionary<Pawn, string> tmpPawnName = new Dictionary<Pawn, string>(16);

		public override Vector2 InitialSize => new Vector2(520f, 500f);

		public Dialog_AssignBuildingOwner(CompAssignableToPawn assignable)
		{
			this.assignable = assignable;
			doCloseButton = true;
			doCloseX = true;
			closeOnClickedOutside = true;
			absorbInputAroundWindow = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Small;
			Rect outRect = new Rect(inRect);
			outRect.yMin += 20f;
			outRect.yMax -= 40f;
			outRect.width -= 16f;
			float num = Mathf.Max(24f, 35f);
			tmpPawnName.Clear();
			RectAggregator rectAggregator = new RectAggregator(Rect.zero, 1279515574);
			rectAggregator.NewCol(24f);
			rectAggregator.NewCol(165f);
			float width = rectAggregator.Rect.width;
			foreach (Pawn item in assignable.AssignedPawnsForReading)
			{
				tmpPawnName[item] = item.LabelCap;
			}
			foreach (Pawn assigningCandidate in assignable.AssigningCandidates)
			{
				if (!tmpPawnName.ContainsKey(assigningCandidate))
				{
					AcceptanceReport acceptanceReport = assignable.CanAssignTo(assigningCandidate);
					bool accepted = acceptanceReport.Accepted;
					tmpPawnName[assigningCandidate] = assigningCandidate.LabelCap + (accepted ? "" : (" (" + acceptanceReport.Reason.StripTags() + ")"));
				}
			}
			Pawn key;
			string value;
			foreach (KeyValuePair<Pawn, string> item2 in tmpPawnName)
			{
				GenCollection.Deconstruct(item2, out key, out value);
				Pawn key2 = key;
				num = Mathf.Max(num, Text.CalcHeight(tmpPawnName[key2], width));
			}
			foreach (KeyValuePair<Pawn, string> item3 in tmpPawnName)
			{
				GenCollection.Deconstruct(item3, out key, out value);
				rectAggregator.NewRow(num);
			}
			if (assignable.AssignedPawnsForReading.Count > 0)
			{
				rectAggregator.NewRow(7f);
			}
			float height = rectAggregator.Rect.height;
			RectDivider rectDivider = new RectDivider(new Rect(0f, 0f, outRect.width - 16f, height), 1279515574);
			Widgets.BeginScrollView(outRect, ref scrollPosition, rectDivider);
			try
			{
				int num2 = 0;
				foreach (Pawn item4 in assignable.AssignedPawnsForReading)
				{
					RectDivider rectDivider2 = rectDivider.NewRow(num);
					if (num2 % 2 == 0)
					{
						Widgets.DrawLightHighlight(rectDivider2);
					}
					num2++;
					Widgets.ThingIcon(rectDivider2.NewCol(24f), item4);
					if (Widgets.ButtonText(rectDivider2.NewCol(165f, HorizontalJustification.Right), "BuildingUnassign".Translate()))
					{
						assignable.TryUnassignPawn(item4);
						SoundDefOf.Click.PlayOneShotOnCamera();
						return;
					}
					Text.Anchor = TextAnchor.MiddleLeft;
					Widgets.Label(rectDivider2, tmpPawnName[item4]);
					Text.Anchor = TextAnchor.UpperLeft;
				}
				if (assignable.AssignedPawnsForReading.Count > 0)
				{
					Rect rect = rectDivider.NewRow(7f).Rect;
					GUI.color = Widgets.SeparatorLineColor;
					Widgets.DrawLineHorizontal(rect.x, rect.y + rect.height / 2f, rect.width);
					GUI.color = Color.white;
				}
				foreach (Pawn pawn in assignable.AssigningCandidates)
				{
					if (assignable.AssignedPawnsForReading.Contains(pawn))
					{
						continue;
					}
					bool accepted2 = assignable.CanAssignTo(pawn).Accepted;
					RectDivider rectDivider3 = rectDivider.NewRow(num);
					if (num2 % 2 == 0)
					{
						Widgets.DrawLightHighlight(rectDivider3);
					}
					num2++;
					if (!accepted2)
					{
						GUI.color = Color.gray;
					}
					Widgets.ThingIcon(rectDivider3.NewCol(24f), pawn);
					RectDivider rectDivider4 = rectDivider3.NewCol(165f, HorizontalJustification.Right);
					if (!Find.IdeoManager.classicMode && accepted2 && assignable.IdeoligionForbids(pawn))
					{
						RectDivider rectDivider5 = rectDivider4.NewCol(24f, HorizontalJustification.Right).NewRow(24f);
						Widgets.Label(rectDivider4, "IdeoligionForbids".Translate());
						IdeoUIUtility.DoIdeoIcon(rectDivider5, pawn.ideo.Ideo, doTooltip: true, delegate
						{
							IdeoUIUtility.OpenIdeoInfo(pawn.ideo.Ideo);
							Close();
						});
					}
					else if (accepted2)
					{
						TaggedString taggedString = (assignable.AssignedAnything(pawn) ? "BuildingReassign".Translate() : "BuildingAssign".Translate());
						if (Widgets.ButtonText(rectDivider4, taggedString, drawBackground: true, doMouseoverSound: true, accepted2))
						{
							assignable.TryAssignPawn(pawn);
							if (assignable.MaxAssignedPawnsCount == 1)
							{
								Close();
							}
							else
							{
								SoundDefOf.Click.PlayOneShotOnCamera();
							}
							break;
						}
					}
					Text.Anchor = TextAnchor.MiddleLeft;
					Widgets.Label(rectDivider3, tmpPawnName[pawn]);
					Text.Anchor = TextAnchor.UpperLeft;
					GUI.color = Color.white;
				}
			}
			finally
			{
				Widgets.EndScrollView();
			}
		}
	}
}
