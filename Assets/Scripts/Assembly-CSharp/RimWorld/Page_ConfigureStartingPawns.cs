using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Page_ConfigureStartingPawns : Page
	{
		private Pawn curPawn;

		private bool renderClothes;

		private bool renderHeadgear;

		private int reorderableGroupID;

		private const float TabAreaWidth = 140f;

		private const float RightRectLeftPadding = 5f;

		private const float PawnEntryHeight = 60f;

		private const float SkillSummaryHeight = 127f;

		private const int SkillSummaryColumns = 4;

		private const int TeamSkillExtraInset = 10;

		public static readonly Vector2 PawnPortraitSize = new Vector2(92f, 128f);

		private static readonly Vector2 PawnSelectorPortraitSize = new Vector2(70f, 110f);

		private int SkillsPerColumn = -1;

		private float listScrollViewHeight;

		private Vector2 listScrollPosition;

		public override string PageTitle => "CreateCharacters".Translate();

		private bool StartingPawnsAllBabies
		{
			get
			{
				List<Pawn> startingAndOptionalPawns = Find.GameInitData.startingAndOptionalPawns;
				int num = 0;
				for (int i = 0; i < Find.GameInitData.startingPawnCount; i++)
				{
					if (startingAndOptionalPawns[i].DevelopmentalStage.Baby())
					{
						num++;
					}
				}
				return num >= Find.GameInitData.startingPawnCount;
			}
		}

		private AcceptanceReport ExtraCanDoNextReport
		{
			get
			{
				if (ModsConfig.BiotechActive && StartingPawnsAllBabies)
				{
					return "ChooseChildOrAdult".Translate();
				}
				if (!Find.GameInitData.startingPawnsRequired.NullOrEmpty())
				{
					IEnumerable<Pawn> source = Find.GameInitData.startingAndOptionalPawns.Take(Find.GameInitData.startingPawnCount);
					for (int i = 0; i < Find.GameInitData.startingPawnsRequired.Count; i++)
					{
						PawnKindCount required = Find.GameInitData.startingPawnsRequired[i];
						int num = source.Count((Pawn p) => p.kindDef == required.kindDef);
						if (required.count > num)
						{
							if (required.count <= 1 || required.kindDef.labelPlural.NullOrEmpty())
							{
								_ = required.kindDef.label;
							}
							else
							{
								_ = required.kindDef.labelPlural;
							}
							return "SelectedCharactersMustInclude".Translate(required.Summary.Named("SUMMARY"));
						}
					}
				}
				if (!Find.GameInitData.startingXenotypesRequired.NullOrEmpty())
				{
					IEnumerable<Pawn> source2 = Find.GameInitData.startingAndOptionalPawns.Take(Find.GameInitData.startingPawnCount);
					for (int j = 0; j < Find.GameInitData.startingXenotypesRequired.Count; j++)
					{
						XenotypeCount required2 = Find.GameInitData.startingXenotypesRequired[j];
						if (source2.Count((Pawn p) => p.genes.Xenotype == required2.xenotype && required2.allowedDevelopmentalStages.Has(p.DevelopmentalStage)) != required2.count)
						{
							return "SelectedCharactersMustInclude".Translate(required2.Summary.Named("SUMMARY"));
						}
					}
				}
				return true;
			}
		}

		public override void PreOpen()
		{
			base.PreOpen();
			if (Find.GameInitData.startingAndOptionalPawns.Count > 0)
			{
				curPawn = Find.GameInitData.startingAndOptionalPawns[0];
			}
			renderClothes = true;
			renderHeadgear = true;
		}

		public override void PostOpen()
		{
			base.PostOpen();
			TutorSystem.Notify_Event("PageStart-ConfigureStartingPawns");
		}

		public override void DoWindowContents(Rect rect)
		{
			DrawPageTitle(rect);
			DrawApparelOptions(rect);
			rect.yMin += 45f;
			DoBottomButtons(rect, "Start".Translate(), null, null, showNext: true, doNextOnKeypress: false);
			DrawXenotypeEditorButton(rect);
			AcceptanceReport extraCanDoNextReport = ExtraCanDoNextReport;
			if (!extraCanDoNextReport.Accepted && !extraCanDoNextReport.Reason.NullOrEmpty())
			{
				Rect rect2 = new Rect(rect.center.x + Page.BottomButSize.x / 2f + 4f, rect.y + rect.height - Page.BottomButSize.y, Page.BottomButSize.x, Page.BottomButSize.y);
				rect2.xMax = rect.xMax - Page.BottomButSize.x - 4f;
				string text = ExtraCanDoNextReport.Reason.TruncateHeight(rect2.width, rect2.height);
				GUI.color = Color.red;
				Text.Font = GameFont.Tiny;
				Widgets.Label(rect2, text);
				Text.Font = GameFont.Small;
				GUI.color = Color.white;
				if (ExtraCanDoNextReport.Reason != text && Mouse.IsOver(rect2))
				{
					Widgets.DrawHighlight(rect2);
					TooltipHandler.TipRegion(rect2, ExtraCanDoNextReport.Reason);
				}
			}
			rect.yMax -= 48f;
			Rect rect3 = rect;
			rect3.width = 140f;
			DrawPawnList(rect3);
			UIHighlighter.HighlightOpportunity(rect3, "ReorderPawn");
			Rect rect4 = rect;
			rect4.xMin += 140f;
			Rect rect5 = rect4.BottomPartPixels(127f);
			rect4.yMax = rect5.yMin;
			rect4 = rect4.ContractedBy(4f);
			rect5 = rect5.ContractedBy(4f);
			DrawPortraitArea(rect4);
			DrawSkillSummaries(rect5);
		}

		private void DrawPawnList(Rect rect)
		{
			Rect rect2 = rect;
			rect2.height = 60f;
			rect2 = rect2.ContractedBy(4f);
			if (Event.current.type == EventType.Repaint)
			{
				reorderableGroupID = ReorderableWidget.NewGroup(delegate(int from, int to)
				{
					if (TutorSystem.AllowAction("ReorderPawn"))
					{
						Pawn item = Find.GameInitData.startingAndOptionalPawns[from];
						Find.GameInitData.startingAndOptionalPawns.Insert(to, item);
						Find.GameInitData.startingAndOptionalPawns.RemoveAt((from < to) ? from : (from + 1));
						StartingPawnUtility.ReorderRequests(from, to);
						TutorSystem.Notify_Event("ReorderPawn");
						if (to < Find.GameInitData.startingPawnCount && from >= Find.GameInitData.startingPawnCount)
						{
							TutorSystem.Notify_Event("ReorderPawnOptionalToStarting");
						}
					}
				}, ReorderableDirection.Vertical, rect, -1f, null, playSoundOnStartReorder: false);
			}
			rect2.y += 15f;
			DrawPawnListLabelAbove(rect2, "StartingPawnsSelected".Translate());
			for (int i = 0; i < Find.GameInitData.startingAndOptionalPawns.Count; i++)
			{
				if (i == Find.GameInitData.startingPawnCount)
				{
					rect2.y += 30f;
					DrawPawnListLabelAbove(rect2, "StartingPawnsLeftBehind".Translate());
				}
				Pawn pawn = Find.GameInitData.startingAndOptionalPawns[i];
				Widgets.BeginGroup(rect2.ExpandedBy(4f));
				Rect rect3 = new Rect(new Vector2(4f, 4f), rect2.size);
				Widgets.DrawOptionBackground(rect3, curPawn == pawn);
				MouseoverSounds.DoRegion(rect3);
				Widgets.BeginGroup(rect3);
				GUI.color = new Color(1f, 1f, 1f, 0.2f);
				GUI.DrawTexture(new Rect(110f - PawnSelectorPortraitSize.x / 2f, 40f - PawnSelectorPortraitSize.y / 2f, PawnSelectorPortraitSize.x, PawnSelectorPortraitSize.y), PortraitsCache.Get(pawn, PawnSelectorPortraitSize, Rot4.South));
				GUI.color = Color.white;
				Widgets.Label(label: (!(pawn.Name is NameTriple nameTriple)) ? pawn.LabelShort : (string.IsNullOrEmpty(nameTriple.Nick) ? nameTriple.First : nameTriple.Nick), rect: rect3.TopPart(0.5f).Rounded());
				if (Text.CalcSize(pawn.story.TitleCap).x > rect3.width)
				{
					Widgets.Label(rect3.BottomPart(0.5f).Rounded(), pawn.story.TitleShortCap);
				}
				else
				{
					Widgets.Label(rect3.BottomPart(0.5f).Rounded(), pawn.story.TitleCap);
				}
				if (Event.current.type == EventType.MouseDown && Mouse.IsOver(rect3))
				{
					curPawn = pawn;
					SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
				}
				Widgets.EndGroup();
				Widgets.EndGroup();
				if (ReorderableWidget.Reorderable(reorderableGroupID, rect2.ExpandedBy(4f)))
				{
					Widgets.DrawRectFast(rect2, Widgets.WindowBGFillColor * new Color(1f, 1f, 1f, 0.5f));
				}
				if (Mouse.IsOver(rect2))
				{
					TooltipHandler.TipRegion(rect2, new TipSignal("DragToReorder".Translate(), pawn.GetHashCode() * 3499));
				}
				rect2.y += 60f;
			}
			rect2.y += 15f;
			DrawPawnListLabelAbove(rect2, "DragToReorder".Translate(), isGray: true);
		}

		private void DrawPawnListLabelAbove(Rect rect, string label, bool isGray = false)
		{
			rect.yMax = rect.yMin;
			rect.yMin -= 30f;
			rect.xMin -= 4f;
			Color color = Color.white;
			if (isGray)
			{
				color = GUI.color;
				GUI.color = Color.gray;
			}
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.LowerLeft;
			string text = label.Truncate(rect.width);
			Widgets.Label(rect, text);
			if (label != text)
			{
				TooltipHandler.TipRegion(new Rect(rect.x, rect.yMax - 18f, 120f, 18f), label);
			}
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			if (isGray)
			{
				GUI.color = color;
			}
		}

		private void DrawPortraitArea(Rect rect)
		{
			Widgets.DrawMenuSection(rect);
			rect = rect.ContractedBy(17f);
			Rect position = new Rect(rect.center.x - PawnPortraitSize.x / 2f, rect.yMin - 24f, PawnPortraitSize.x, PawnPortraitSize.y);
			RenderTexture image = PortraitsCache.Get(curPawn, PawnPortraitSize, Rot4.South, default(Vector3), 1f, supersample: true, compensateForUIScale: true, renderClothes: renderClothes, renderHeadgear: renderHeadgear, overrideApparelColors: null, overrideHairColor: null, stylingStation: true);
			GUI.DrawTexture(position, image);
			Rect rect2 = rect;
			rect2.width = 500f;
			CharacterCardUtility.DrawCharacterCard(rect2, curPawn, delegate
			{
				RandomizeCurPawn();
			}, rect);
			bool num2 = SocialCardUtility.AnyRelations(curPawn);
			List<ThingDefCount> list = Find.GameInitData.startingPossessions[curPawn];
			bool flag = list.Any();
			int num3 = 1;
			if (num2)
			{
				num3++;
			}
			if (flag)
			{
				num3++;
			}
			float height = (rect.height - 100f - (4f * (float)num3 - 1f)) / (float)num3;
			float y = rect.y;
			Rect rect3 = rect;
			rect3.yMin += 100f;
			rect3.xMin = rect2.xMax + 5f;
			rect3.height = height;
			if (!HealthCardUtility.AnyHediffsDisplayed(curPawn, showBloodLoss: true))
			{
				GUI.color = Color.gray;
			}
			Widgets.Label(rect3, "Health".Translate().AsTipTitle());
			GUI.color = Color.white;
			rect3.yMin += 35f;
			HealthCardUtility.DrawHediffListing(rect3, curPawn, showBloodLoss: true);
			y = rect3.yMax + 4f;
			if (num2)
			{
				Rect rect4 = new Rect(rect3.x, y, rect3.width, height);
				Widgets.Label(rect4, "Relations".Translate().AsTipTitle());
				rect4.yMin += 35f;
				SocialCardUtility.DrawRelationsAndOpinions(rect4, curPawn);
				y = rect4.yMax + 4f;
			}
			if (flag)
			{
				Rect rect5 = new Rect(rect3.x, y, rect3.width, height);
				Widgets.Label(rect5, "Possessions".Translate().AsTipTitle());
				rect5.yMin += 35f;
				DrawPossessions(rect5, curPawn, list);
			}
		}

		private void DrawApparelOptions(Rect rect)
		{
			if (ModsConfig.IdeologyActive)
			{
				string text = "ShowHeadgear".Translate();
				string text2 = "ShowApparel".Translate();
				float num = Mathf.Max(Text.CalcSize(text).x, Text.CalcSize(text2).x) + 4f + 24f;
				Rect rect2 = new Rect(rect.xMax - num, rect.y, num, Text.LineHeight * 2f);
				Widgets.CheckboxLabeled(new Rect(rect2.x, rect2.y, rect2.width, rect2.height / 2f), text, ref renderHeadgear);
				Widgets.CheckboxLabeled(new Rect(rect2.x, rect2.y + rect2.height / 2f, rect2.width, rect2.height / 2f), text2, ref renderClothes);
			}
		}

		private void DrawXenotypeEditorButton(Rect rect)
		{
			if (!ModsConfig.BiotechActive)
			{
				return;
			}
			Text.Font = GameFont.Small;
			float x = (rect.width - Page.BottomButSize.x) / 2f;
			float y = rect.y + rect.height - 38f;
			if (Widgets.ButtonText(new Rect(x, y, Page.BottomButSize.x, Page.BottomButSize.y), "XenotypeEditor".Translate()))
			{
				Find.WindowStack.Add(new Dialog_CreateXenotype(StartingPawnUtility.PawnIndex(curPawn), delegate
				{
					CharacterCardUtility.cachedCustomXenotypes = null;
					RandomizeCurPawn();
				}));
			}
		}

		private void DrawPossessions(Rect rect, Pawn selPawn, List<ThingDefCount> possessions)
		{
			GUI.BeginGroup(rect);
			Rect outRect = new Rect(0f, 0f, rect.width, rect.height);
			Rect viewRect = new Rect(0f, 0f, rect.width - 16f, listScrollViewHeight);
			Rect rect2 = rect;
			if (viewRect.height > outRect.height)
			{
				rect2.width -= 16f;
			}
			Widgets.BeginScrollView(outRect, ref listScrollPosition, viewRect);
			float num = 0f;
			_ = listScrollPosition;
			_ = listScrollPosition;
			_ = outRect.height;
			if (Find.GameInitData.startingPossessions.ContainsKey(selPawn))
			{
				for (int i = 0; i < possessions.Count; i++)
				{
					ThingDefCount thingDefCount = possessions[i];
					Rect rect3 = new Rect(0f, num, Text.LineHeight, Text.LineHeight);
					Widgets.DefIcon(rect3, thingDefCount.ThingDef);
					Rect rect4 = new Rect(rect3.xMax + 17f, num, rect.width - rect3.width - 17f - 24f, Text.LineHeight);
					Widgets.Label(rect4, thingDefCount.LabelCap);
					if (Mouse.IsOver(rect4))
					{
						Widgets.DrawHighlight(rect4);
						TooltipHandler.TipRegion(rect4, thingDefCount.ThingDef.LabelCap.ToString().Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + thingDefCount.ThingDef.description);
					}
					Widgets.InfoCardButton(rect4.xMax, num, thingDefCount.ThingDef);
					num += Text.LineHeight;
				}
			}
			if (Event.current.type == EventType.Layout)
			{
				listScrollViewHeight = num;
			}
			Widgets.EndScrollView();
			GUI.EndGroup();
		}

		private void DrawSkillSummaries(Rect rect)
		{
			rect.xMin += 10f;
			rect.xMax -= 10f;
			Widgets.DrawMenuSection(rect);
			rect = rect.ContractedBy(10f);
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(rect.min, new Vector2(rect.width, 45f)), "TeamSkills".Translate());
			Text.Font = GameFont.Small;
			rect.yMin += 45f;
			rect = rect.LeftPart(0.25f);
			rect.height = 27f;
			rect.y -= 4f;
			List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
			if (SkillsPerColumn < 0)
			{
				SkillsPerColumn = Mathf.CeilToInt((float)allDefsListForReading.Where((SkillDef sd) => sd.pawnCreatorSummaryVisible).Count() / 4f);
			}
			int num = 0;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				SkillDef skillDef = allDefsListForReading[i];
				if (skillDef.pawnCreatorSummaryVisible)
				{
					Rect r = rect;
					r.x = rect.x + rect.width * (float)(num / SkillsPerColumn);
					r.y = rect.y + rect.height * (float)(num % SkillsPerColumn);
					r.height = 24f;
					r.width -= 4f;
					Pawn pawn = FindBestSkillOwner(skillDef);
					SkillUI.DrawSkill(pawn.skills.GetSkill(skillDef), r.Rounded(), SkillUI.SkillDrawMode.Menu, pawn.Name.ToString().Colorize(ColoredText.TipSectionTitleColor));
					num++;
				}
			}
		}

		private Pawn FindBestSkillOwner(SkillDef skill)
		{
			Pawn pawn = Find.GameInitData.startingAndOptionalPawns[0];
			SkillRecord skillRecord = pawn.skills.GetSkill(skill);
			for (int i = 1; i < Find.GameInitData.startingPawnCount; i++)
			{
				SkillRecord skill2 = Find.GameInitData.startingAndOptionalPawns[i].skills.GetSkill(skill);
				if (skillRecord.TotallyDisabled || skill2.Level > skillRecord.Level || (skill2.Level == skillRecord.Level && (int)skill2.passion > (int)skillRecord.passion))
				{
					pawn = Find.GameInitData.startingAndOptionalPawns[i];
					skillRecord = skill2;
				}
			}
			return pawn;
		}

		private void RandomizeCurPawn()
		{
			if (TutorSystem.AllowAction("RandomizePawn"))
			{
				int num = 0;
				do
				{
					SpouseRelationUtility.Notify_PawnRegenerated(curPawn);
					curPawn = StartingPawnUtility.RandomizeInPlace(curPawn);
					num++;
				}
				while (num <= 20 && !StartingPawnUtility.WorkTypeRequirementsSatisfied());
				TutorSystem.Notify_Event("RandomizePawn");
			}
		}

		protected override bool CanDoNext()
		{
			if (!base.CanDoNext())
			{
				return false;
			}
			if (TutorSystem.TutorialMode)
			{
				WorkTypeDef workTypeDef = StartingPawnUtility.RequiredWorkTypesDisabledForEveryone().FirstOrDefault();
				if (workTypeDef != null)
				{
					Messages.Message("RequiredWorkTypeDisabledForEveryone".Translate() + ": " + workTypeDef.gerundLabel.CapitalizeFirst() + ".", MessageTypeDefOf.RejectInput, historical: false);
					return false;
				}
			}
			foreach (Pawn startingAndOptionalPawn in Find.GameInitData.startingAndOptionalPawns)
			{
				if (!startingAndOptionalPawn.Name.IsValid)
				{
					Messages.Message("EveryoneNeedsValidName".Translate(), MessageTypeDefOf.RejectInput, historical: false);
					return false;
				}
			}
			AcceptanceReport extraCanDoNextReport = ExtraCanDoNextReport;
			if (!extraCanDoNextReport.Reason.NullOrEmpty())
			{
				Messages.Message(extraCanDoNextReport.Reason, MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			PortraitsCache.Clear();
			return true;
		}

		protected override void DoNext()
		{
			CheckWarnRequiredWorkTypesDisabledForEveryone(delegate
			{
				foreach (Pawn startingAndOptionalPawn in Find.GameInitData.startingAndOptionalPawns)
				{
					if (startingAndOptionalPawn.Name is NameTriple nameTriple && string.IsNullOrEmpty(nameTriple.Nick))
					{
						startingAndOptionalPawn.Name = new NameTriple(nameTriple.First, nameTriple.First, nameTriple.Last);
					}
				}
				base.DoNext();
			});
		}

		private void CheckWarnRequiredWorkTypesDisabledForEveryone(Action nextAction)
		{
			IEnumerable<WorkTypeDef> enumerable = StartingPawnUtility.RequiredWorkTypesDisabledForEveryone();
			if (enumerable.Any())
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (WorkTypeDef item in enumerable)
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.Append("  - " + item.gerundLabel.CapitalizeFirst());
				}
				TaggedString text = "ConfirmRequiredWorkTypeDisabledForEveryone".Translate(stringBuilder.ToString());
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, nextAction));
			}
			else
			{
				nextAction();
			}
		}

		public void SelectPawn(Pawn c)
		{
			if (c != curPawn)
			{
				curPawn = c;
			}
		}
	}
}
