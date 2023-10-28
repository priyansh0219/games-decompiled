using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Dialog_BeginRitual : Window
	{
		public delegate bool ActionCallback(RitualRoleAssignments assignments);

		private struct PawnPortraitIcon
		{
			public Color color;

			public Texture2D icon;

			public string tooltip;
		}

		private Precept_Ritual ritual;

		private TargetInfo target;

		private RitualObligation obligation;

		private RitualOutcomeEffectDef outcome;

		private string ritualExplanation;

		private List<string> extraInfos;

		protected ActionCallback action;

		protected Func<Pawn, bool, bool, bool> filter;

		protected Map map;

		protected string ritualLabel;

		protected string headerText;

		protected string okButtonText;

		protected string confirmText;

		protected Vector2 scrollPositionPawns;

		protected float listScrollViewHeight;

		protected Pawn organizer;

		protected Pawn selectedPawn;

		private RitualRoleAssignments assignments;

		private int pawnsListEdgeScrollDirection;

		private RitualRole highlightedRole;

		private Vector2 scrollPositionQualityDesc;

		private float qualityDescHeight;

		private List<Pawn> nonAssignablePawns = new List<Pawn>();

		private List<Pawn> nonParticipatingPawnCandidatesTmp = new List<Pawn>();

		private static List<Precept_Role> cachedRoles = new List<Precept_Role>();

		private static Texture2D prisonerIcon = ContentFinder<Texture2D>.Get("UI/Icons/Prisoner");

		private static Texture2D sleepingIcon = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Sleeping");

		private static readonly Texture2D questionMark = ContentFinder<Texture2D>.Get("UI/Overlays/QuestionMark");

		private static readonly Texture2D WarningIcon = Resources.Load<Texture2D>("Textures/UI/Widgets/Warning");

		private static readonly Texture2D QualityOffsetCheckOn = Resources.Load<Texture2D>("Textures/UI/Widgets/RitualQualityCheck_On");

		private static readonly Texture2D QualityOffsetCheckOff = Resources.Load<Texture2D>("Textures/UI/Widgets/RitualQualityCheck_Off");

		private string sleepingMessage;

		protected const float CategoryCaptionHeight = 32f;

		protected const float EntryHeight = 28f;

		protected const float ListWidth = 320f;

		protected const float TargetIconSize = 32f;

		protected const float QualityOffsetListWidth = 402f;

		private const int HeadlineIconSize = 20;

		private const int PawnPortraitHeightTotal = 70;

		private const int PawnPortraitHeight = 50;

		private const int PawnPortraitWidth = 50;

		private const int PawnPortraitLabelHeight = 20;

		private const int PawnPortraitMargin = 4;

		private const int PawnsListPadding = 4;

		private const int PawnsListHorizontalGap = 26;

		private const int PawnPortraitIconSize = 20;

		private const int EdgeScrollSpeedWhileDragging = 1000;

		private const float RoleHeight = 40f;

		private static Texture2D slaveryIcon;

		private List<ExpectedOutcomeDesc> expectedOutcomeEffects = new List<ExpectedOutcomeDesc>();

		private List<OutcomeChance> outcomeChances = new List<OutcomeChance>();

		private static List<PawnPortraitIcon> tmpPortraitIcons = new List<PawnPortraitIcon>();

		private static List<Action> tmpDelayedGuiCalls = new List<Action>();

		private int dragAndDropGroup;

		private Rect? lastHoveredDropArea;

		private static List<Pawn> tmpSelectedPawns = new List<Pawn>();

		private static List<Pawn> tmpAssignedPawns = new List<Pawn>();

		private static readonly object DropContextSpectator = new object();

		private static readonly object DropContextNotParticipating = new object();

		private List<IGrouping<string, RitualRole>> rolesGroupedTmp = new List<IGrouping<string, RitualRole>>();

		public override Vector2 InitialSize => new Vector2(845f, 740f);

		protected Vector2 ButSize => new Vector2(200f, 40f);

		protected string WarningText
		{
			get
			{
				string result = "";
				using (IEnumerator<string> enumerator = BlockingIssues().GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						result = enumerator.Current;
					}
				}
				return result;
			}
		}

		public string SleepingWarning
		{
			get
			{
				if (sleepingMessage.NullOrEmpty())
				{
					sleepingMessage = "RitualBeginSleepingWarning".Translate();
				}
				if (assignments.Participants.Any((Pawn p) => !p.Awake()))
				{
					return sleepingMessage;
				}
				return null;
			}
		}

		private static Texture2D SlaveryIcon
		{
			get
			{
				if (slaveryIcon == null)
				{
					slaveryIcon = ContentFinder<Texture2D>.Get("UI/Icons/Slavery");
				}
				return slaveryIcon;
			}
		}

		public Dialog_BeginRitual(string header, string ritualLabel, Precept_Ritual ritual, TargetInfo target, Map map, ActionCallback action, Pawn organizer, RitualObligation obligation, Func<Pawn, bool, bool, bool> filter = null, string okButtonText = null, List<Pawn> requiredPawns = null, Dictionary<string, Pawn> forcedForRole = null, string ritualName = null, RitualOutcomeEffectDef outcome = null, List<string> extraInfoText = null, Pawn selectedPawn = null, bool showQuality = true)
		{
			if (!ModLister.CheckRoyaltyOrIdeologyOrBiotech("Ritual"))
			{
				return;
			}
			this.ritual = ritual;
			this.target = target;
			this.obligation = obligation;
			extraInfos = extraInfoText;
			this.selectedPawn = selectedPawn;
			assignments = new RitualRoleAssignments(ritual);
			List<Pawn> list = new List<Pawn>(map.mapPawns.FreeColonistsAndPrisonersSpawned);
			nonAssignablePawns.Clear();
			for (int num = list.Count - 1; num >= 0; num--)
			{
				Pawn pawn = list[num];
				if (filter != null && !filter(pawn, arg2: true, arg3: true))
				{
					list.RemoveAt(num);
				}
				else
				{
					bool stillAddToPawnList;
					bool flag = RitualRoleAssignments.PawnNotAssignableReason(pawn, null, ritual, assignments, target, out stillAddToPawnList) == null || stillAddToPawnList;
					if (!flag && ritual != null)
					{
						if (pawn.DevelopmentalStage != DevelopmentalStage.Baby && pawn.DevelopmentalStage != DevelopmentalStage.Newborn)
						{
							nonAssignablePawns.AddDistinct(pawn);
						}
						foreach (RitualRole role in ritual.behavior.def.roles)
						{
							if ((RitualRoleAssignments.PawnNotAssignableReason(pawn, role, ritual, assignments, target, out stillAddToPawnList) == null || stillAddToPawnList) && (filter == null || filter(pawn, !(role is RitualRoleForced), role.allowOtherIdeos)) && (role.maxCount > 1 || forcedForRole == null || !forcedForRole.ContainsKey(role.id)))
							{
								flag = true;
								break;
							}
						}
					}
					if (!flag)
					{
						list.RemoveAt(num);
					}
				}
			}
			if (requiredPawns != null)
			{
				foreach (Pawn requiredPawn in requiredPawns)
				{
					list.AddDistinct(requiredPawn);
				}
			}
			if (forcedForRole != null)
			{
				foreach (KeyValuePair<string, Pawn> item in forcedForRole)
				{
					list.AddDistinct(item.Value);
				}
			}
			if (ritual != null)
			{
				foreach (RitualRole role2 in ritual.behavior.def.roles)
				{
					if (role2.Animal)
					{
						list.AddRange(map.mapPawns.SpawnedColonyAnimals.Where((Pawn p) => filter == null || filter(p, arg2: true, arg3: true)));
						break;
					}
				}
			}
			assignments.Setup(list, forcedForRole, requiredPawns, selectedPawn);
			ritualExplanation = ritual?.ritualExplanation;
			this.action = action;
			this.filter = filter;
			this.map = map;
			this.ritualLabel = ritualLabel;
			headerText = header;
			this.okButtonText = okButtonText;
			this.organizer = organizer;
			closeOnClickedOutside = true;
			absorbInputAroundWindow = true;
			forcePause = true;
			this.outcome = ((ritual != null && ritual.outcomeEffect != null) ? ritual.outcomeEffect.def : outcome);
			cachedRoles.Clear();
			if (ritual != null && ritual.ideo != null)
			{
				cachedRoles.AddRange(ritual.ideo.RolesListForReading.Where((Precept_Role r) => !r.def.leaderRole));
				Precept_Role precept_Role = Faction.OfPlayer.ideos.PrimaryIdeo.RolesListForReading.FirstOrDefault((Precept_Role p) => p.def.leaderRole);
				if (precept_Role != null)
				{
					cachedRoles.Add(precept_Role);
				}
				cachedRoles.SortBy((Precept_Role x) => x.def.displayOrderInImpact);
			}
		}

		public override void PostOpen()
		{
			assignments.FillPawns(filter, target);
			if (outcome == null || ritual == null || ritual.outcomeEffect == null)
			{
				return;
			}
			foreach (RitualOutcomeComp comp in outcome.comps)
			{
				comp.Notify_AssignementsChanged(assignments, ritual.outcomeEffect.DataForComp(comp));
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			float num = 0f;
			Text.Font = GameFont.Medium;
			Rect rect = new Rect(inRect);
			rect.height = Text.CalcHeight(ritualLabel, inRect.width) + 4f;
			Widgets.Label(rect, ritualLabel);
			Text.Font = GameFont.Small;
			if (ritual != null)
			{
				if (ritual.ideo != null && !Find.IdeoManager.classicMode)
				{
					Ideo ideo = ritual.ideo;
					float x = Text.CalcSize(ideo.name.CapitalizeFirst()).x;
					Rect r = new Rect(inRect);
					r.xMin = r.xMax - x - 30f;
					r.height = 22f;
					IdeoUIUtility.DrawIdeoPlate(r, ideo);
				}
				Text.Anchor = TextAnchor.LowerRight;
				GUI.color = Color.gray;
				if (!ritual.Label.EqualsIgnoreCase(ritual.UIInfoFirstLine))
				{
					Rect rect2 = new Rect(inRect);
					rect2.height = Text.CalcHeight(ritual.UIInfoFirstLine, inRect.width) + 4f;
					if (ritual.ideo != null)
					{
						rect2.height += 21f;
					}
					Widgets.Label(rect2, ritual.UIInfoFirstLine);
				}
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
			}
			num += rect.height;
			num += 4f;
			string str = ritual?.Description;
			if (ritual?.behavior?.descriptionOverride != null)
			{
				str = ritual?.behavior?.descriptionOverride;
			}
			if (!str.NullOrEmpty())
			{
				str = str.Formatted(organizer.Named("ORGANIZER"));
				float num2 = Text.CalcHeight(str, inRect.width);
				Rect rect3 = inRect;
				rect3.x += 10f;
				rect3.width -= 20f;
				rect3.yMin = num + 10f;
				rect3.height = num2;
				Widgets.Label(rect3, str);
				num += num2 + 17f;
			}
			float potentialMax;
			string text = ritual?.behavior?.GetExplanation(ritual, assignments, PredictedQuality(out potentialMax));
			if (!ritualExplanation.NullOrEmpty() || !text.NullOrEmpty())
			{
				string text2 = ritualExplanation;
				if (!text.NullOrEmpty())
				{
					if (!text2.NullOrEmpty())
					{
						text2 += "\n\n";
					}
					text2 += text;
				}
				float num3 = Text.CalcHeight(text2, inRect.width);
				Rect rect4 = inRect;
				rect4.x += 10f;
				rect4.width -= 20f;
				rect4.yMin = num + 10f;
				rect4.height = num3;
				Widgets.Label(rect4, text2);
				num += num3 + 17f;
			}
			Rect source = new Rect(inRect);
			source.yMin = num + 10f;
			source.yMax -= ButSize.y + 10f + 6f;
			Rect rect5 = new Rect(source);
			rect5.width = 320f;
			rect5.x += 20f;
			rect5.height -= 10f;
			Rect viewRect = new Rect(0f, 0f, rect5.width - ((listScrollViewHeight > rect5.height) ? 16f : 0f), listScrollViewHeight);
			Widgets.BeginScrollView(rect5, ref scrollPositionPawns, viewRect);
			try
			{
				DrawPawnList(viewRect, rect5);
			}
			finally
			{
				Widgets.EndScrollView();
			}
			float num4 = 0f;
			RitualRoleIdeoRoleChanger ritualRoleIdeoRoleChanger = assignments.AllRolesForReading.OfType<RitualRoleIdeoRoleChanger>().FirstOrDefault();
			if (ritualRoleIdeoRoleChanger != null && cachedRoles.Any())
			{
				Rect rect6 = new Rect(source.x + 320f + 20f + 4f, source.y, 320f, 40f);
				Pawn pawn = assignments.FirstAssignedPawn(ritualRoleIdeoRoleChanger);
				if (pawn != null)
				{
					DrawRoleSelection(pawn, rect6);
					num4 = 50f;
				}
			}
			int num5 = ((target.Thing != null) ? 28 : 0);
			Rect rect7 = new Rect(source);
			rect7.x = rect5.xMax + 28f;
			rect7.y += num4;
			rect7.width = 402f;
			rect7.height -= num5;
			if (ritual == null || ritual.outcomeEffect == null || ritual.outcomeEffect.ShowQuality)
			{
				DrawQualityFactors(rect7);
			}
			else
			{
				Rect rect8 = rect7;
				rect8.y += 17f;
				TaggedString taggedString = "RitualOutcomeNoQuality".Translate() + ":\n\n  - " + ritual.outcomeEffect.def.outcomeChances.MaxBy((OutcomeChance c) => c.positivityIndex).label.CapitalizeFirst() + " " + ritual.Label;
				Rect rect9 = rect8;
				rect9.width += 10f;
				rect9.height = Text.CalcHeight(taggedString, rect8.width);
				rect9 = rect9.ExpandedBy(9f);
				GUI.color = new Color(0.25f, 0.25f, 0.25f);
				Widgets.DrawBox(rect9, 2);
				GUI.color = Color.white;
				Widgets.Label(rect8, taggedString);
			}
			Rect rect10 = default(Rect);
			rect10.xMin = inRect.xMin;
			rect10.xMax = inRect.xMax;
			rect10.y = source.y + 17f - 2f;
			rect10.height = source.height;
			Rect rect11 = new Rect(rect10.xMax - ButSize.x - 250f - 10f, rect10.yMax, 250f, ButSize.y);
			GUI.color = ColorLibrary.RedReadable;
			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(rect11, WarningText);
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
			if (target.Thing != null)
			{
				TaggedString taggedString2 = "RitualTakesPlaceAt".Translate() + ": ";
				TaggedString taggedString3 = taggedString2 + target.Thing.LabelShortCap;
				float x2 = Text.CalcSize(taggedString2).x;
				float x3 = Text.CalcSize(target.Thing.LabelShortCap).x;
				float num6 = Text.CalcSize(taggedString3).x + 4f + 32f;
				Rect rect12 = new Rect(rect10.xMax - (x3 + 4f + 32f), rect10.yMax - 34f, 32f, 32f);
				Rect rect13 = new Rect(rect10.xMax - num6, rect10.yMax - (float)num5, x2, 24f);
				Rect rect14 = new Rect(rect12.xMax + 4f, rect10.yMax - (float)num5, x3, 24f);
				Widgets.Label(rect13, taggedString2);
				Widgets.Label(rect14, target.Thing.LabelShortCap);
				Widgets.ThingIcon(rect12, target.Thing);
				if (Mouse.IsOver(rect13) || Mouse.IsOver(rect14) || Mouse.IsOver(rect12))
				{
					Find.WindowStack.ImmediateWindow(738453, new Rect(0f, 0f, UI.screenWidth, UI.screenHeight), WindowLayer.Super, delegate
					{
						GenUI.DrawArrowPointingAtWorldspace(target.Cell.ToVector3(), Find.Camera);
					}, doBackground: false, absorbInputAroundWindow: false, 0f);
				}
			}
			Rect rect15 = new Rect(rect10.xMax - ButSize.x, rect10.yMax, ButSize.x, ButSize.y);
			Rect rect16 = new Rect(rect10.x, rect10.yMax, ButSize.x, ButSize.y);
			bool flag = !BlockingIssues().Any();
			if (!flag)
			{
				GUI.color = Color.gray;
			}
			if (Widgets.ButtonText(rect15, okButtonText ?? ((string)"OK".Translate()), drawBackground: true, doMouseoverSound: true, flag))
			{
				if (!confirmText.NullOrEmpty())
				{
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(confirmText, delegate
					{
						if (PredictedQuality(out var _) < 0.25f && outcome.warnOnLowQuality)
						{
							Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("RitualQualityLowWarning".Translate(ritualLabel, 0.25f.ToStringPercent()), Start, destructive: true));
						}
						else
						{
							Start();
						}
					}, destructive: true));
				}
				else
				{
					Start();
				}
			}
			GUI.color = Color.white;
			if (Widgets.ButtonText(rect16, "CancelButton".Translate()))
			{
				Close();
			}
			void Start()
			{
				ActionCallback actionCallback = action;
				if (actionCallback != null && actionCallback(assignments))
				{
					Close();
				}
			}
		}

		protected void DrawQualityFactors(Rect viewRect)
		{
			if (outcome == null)
			{
				return;
			}
			float y2 = viewRect.y;
			float totalInfoHeight = 0f;
			bool even = true;
			expectedOutcomeEffects.Clear();
			float startingQuality = outcome.startingQuality;
			float potentialMax;
			float num = PredictedQuality(out potentialMax);
			if (startingQuality > 0f)
			{
				expectedOutcomeEffects.Add(new ExpectedOutcomeDesc
				{
					label = "StartingQuality".Translate(),
					effect = "+" + startingQuality.ToStringPercent("F0"),
					quality = startingQuality,
					noMiddleColumnInfo = true,
					positive = true,
					priority = 5f
				});
			}
			if (ritual != null && ritual.RepeatPenaltyActive)
			{
				float repeatQualityPenalty = ritual.RepeatQualityPenalty;
				expectedOutcomeEffects.Add(new ExpectedOutcomeDesc
				{
					label = "RitualOutcomePerformedRecently".Translate(),
					effect = repeatQualityPenalty.ToStringPercent(),
					quality = repeatQualityPenalty,
					noMiddleColumnInfo = true,
					positive = false,
					priority = 5f
				});
			}
			Tuple<ExpectationDef, float> expectationsOffset = RitualOutcomeEffectWorker_FromQuality.GetExpectationsOffset(map, ritual?.def);
			if (expectationsOffset != null)
			{
				expectedOutcomeEffects.Add(new ExpectedOutcomeDesc
				{
					label = "RitualQualityExpectations".Translate(expectationsOffset.Item1.LabelCap),
					effect = "+" + expectationsOffset.Item2.ToStringPercent(),
					quality = expectationsOffset.Item2,
					noMiddleColumnInfo = true,
					positive = true,
					priority = 5f
				});
			}
			if (expectedOutcomeEffects.NullOrEmpty())
			{
				return;
			}
			Widgets.Label(new Rect(viewRect.x, y2 + 3f, viewRect.width, 32f), "QualityFactors".Translate());
			y2 += 32f;
			totalInfoHeight += 32f;
			foreach (ExpectedOutcomeDesc item in expectedOutcomeEffects.OrderByDescending((ExpectedOutcomeDesc e) => e.priority))
			{
				DrawQualityFactor(item, ref y2);
				even = !even;
			}
			y2 += 2f;
			if (num < 0.25f)
			{
				GUI.color = ColorLibrary.RedReadable;
			}
			Rect rect = new Rect(viewRect.x, y2 + 4f, viewRect.width, 25f);
			string text = ((ritual == null || ritual.outcomeEffect == null || ritual.outcomeEffect.ExpectedQualityLabel() == null) ? ((string)"ExpectedRitualQuality".Translate()) : ritual.outcomeEffect.ExpectedQualityLabel());
			Widgets.Label(rect, text + ":");
			Text.Font = GameFont.Medium;
			string text2 = ((num == potentialMax) ? num.ToStringPercent("F0") : (num.ToStringPercent("F0") + "-" + potentialMax.ToStringPercent("F0")));
			float x = Text.CalcSize(text2).x;
			Widgets.Label(new Rect(viewRect.xMax - x, y2 - 2f, viewRect.width, 32f), text2);
			Text.Font = GameFont.Small;
			y2 += 28f;
			totalInfoHeight += 28f;
			Rect rect2 = viewRect;
			rect2.width += 10f;
			rect2.height = totalInfoHeight;
			rect2 = rect2.ExpandedBy(9f);
			GUI.color = new Color(0.25f, 0.25f, 0.25f);
			Widgets.DrawBox(rect2, 2);
			GUI.color = Color.white;
			y2 += 28f;
			totalInfoHeight += 28f;
			Rect outRect = new Rect(viewRect.x, y2, viewRect.width, viewRect.height - totalInfoHeight);
			viewRect = viewRect.AtZero();
			viewRect.height = qualityDescHeight;
			viewRect.width = viewRect.width;
			bool flag = qualityDescHeight > outRect.height;
			if (flag)
			{
				viewRect.width -= 16f;
			}
			y2 = 0f;
			Widgets.BeginScrollView(outRect, ref scrollPositionQualityDesc, viewRect, flag);
			if (ritual != null)
			{
				string text3 = ritual.behavior.ExpectedDuration(ritual, assignments, num);
				TaggedString label = "{0}: {1}".Formatted("ExpectedRitualDuration".Translate(), text3);
				Widgets.Label(new Rect(viewRect.x, y2 - 4f, viewRect.width, 32f), label);
				y2 += 17f;
				totalInfoHeight += 17f;
			}
			if (!outcome.outcomeChances.NullOrEmpty())
			{
				outcomeChances.Clear();
				outcomeChances.AddRange(outcome.outcomeChances);
				if (ritual != null && ritual.outcomeEffect != null)
				{
					OutcomeChance forcedOutcome = ritual.outcomeEffect.GetForcedOutcome(ritual, target, obligation, assignments);
					if (forcedOutcome != null)
					{
						outcomeChances.Clear();
						outcomeChances.Add(forcedOutcome);
					}
				}
				Widgets.Label(new Rect(viewRect.x, y2, viewRect.width, 32f), "RitualOutcomeChances".Translate(text2) + ": ");
				y2 += 28f;
				totalInfoHeight += 28f;
				float num2 = 0f;
				foreach (OutcomeChance outcomeChance2 in outcomeChances)
				{
					num2 += (outcomeChance2.Positive ? (outcomeChance2.chance * num) : outcomeChance2.chance);
				}
				foreach (OutcomeChance outcomeChance3 in outcomeChances)
				{
					float f = (outcomeChance3.Positive ? (outcomeChance3.chance * num / num2) : (outcomeChance3.chance / num2));
					string text4 = "  - " + outcomeChance3.label + ": " + f.ToStringPercent();
					Rect rect3 = new Rect(viewRect.x, y2, Text.CalcSize(text4).x + 4f, 32f);
					Rect rect4 = new Rect(rect3);
					rect4.width = rect3.width + 8f;
					rect4.height = 22f;
					Rect rect5 = rect4;
					if (Mouse.IsOver(rect5))
					{
						string desc = outcome.OutcomeMoodBreakdown(outcomeChance3);
						if (!outcomeChance3.potentialExtraOutcomeDesc.NullOrEmpty())
						{
							if (!desc.NullOrEmpty())
							{
								desc += "\n\n";
							}
							desc += outcomeChance3.potentialExtraOutcomeDesc;
						}
						Widgets.DrawLightHighlight(rect5);
						if (!desc.NullOrEmpty())
						{
							TooltipHandler.TipRegion(rect5, () => desc, 231134347);
						}
					}
					Widgets.Label(rect3, text4);
					y2 += Text.LineHeight;
					totalInfoHeight += Text.LineHeight;
				}
			}
			if (outcome.extraOutcomeDescriptions != null)
			{
				y2 += Text.LineHeight;
				totalInfoHeight += Text.LineHeight;
				for (int i = 0; i < outcome.extraOutcomeDescriptions?.Count; i++)
				{
					RitualOutcomeEffectDef.ExtraOutcomeChanceDescription extraOutcomeChanceDescription = outcome.extraOutcomeDescriptions[i];
					float num3 = Mathf.Clamp01(extraOutcomeChanceDescription.qualityToChance(num));
					if (!(num3 <= 0f))
					{
						TaggedString taggedString = extraOutcomeChanceDescription.description.Formatted(num3);
						Vector2 vector = Text.CalcSize(taggedString);
						if (vector.x > viewRect.width)
						{
							vector = new Vector2(viewRect.width, Text.CalcHeight(taggedString, viewRect.width));
						}
						Widgets.Label(new Rect(viewRect.x, y2, vector.x + 4f, Mathf.Max(32f, vector.y)), taggedString);
						y2 += vector.y;
						totalInfoHeight += vector.y;
					}
				}
			}
			y2 += 10f;
			totalInfoHeight += 10f;
			if (extraInfos != null)
			{
				foreach (string extraInfo in extraInfos)
				{
					float num4 = Math.Max(Text.CalcHeight(extraInfo, viewRect.width) + 3f, 28f);
					Widgets.Label(new Rect(viewRect.x, y2 + 4f, viewRect.width, num4), extraInfo);
					y2 += num4;
					totalInfoHeight += num4;
				}
			}
			string sleepingWarning = SleepingWarning;
			if (!sleepingWarning.NullOrEmpty())
			{
				float num5 = Math.Max(Text.CalcHeight(sleepingWarning, viewRect.width) + 3f, 28f);
				Widgets.Label(new Rect(viewRect.x, y2 + 4f, viewRect.width, num5), sleepingWarning);
				y2 += num5;
			}
			if (!outcome.outcomeChances.NullOrEmpty() && ritual != null && ritual.ideo != null && ritual.ideo.Fluid && IdeoDevelopmentUtility.GetDevelopmentPointsOverOutcomeIndexCurveForRitual(ritual.ideo, ritual) != null)
			{
				y2 += 10f;
				totalInfoHeight += 10f;
				TaggedString taggedString2 = "RitualDevelopmentPointRewards".Translate() + ":\n";
				float num6 = Text.CalcHeight(taggedString2, viewRect.width);
				Widgets.Label(new Rect(viewRect.x, y2, viewRect.width, num6), taggedString2);
				y2 += num6;
				SimpleCurve developmentPointsOverOutcomeIndexCurveForRitual = IdeoDevelopmentUtility.GetDevelopmentPointsOverOutcomeIndexCurveForRitual(ritual.ideo, ritual);
				for (int j = 0; j < outcome.outcomeChances.Count; j++)
				{
					OutcomeChance outcomeChance = outcome.outcomeChances[j];
					string label2 = "  - " + outcomeChance.label + ": " + developmentPointsOverOutcomeIndexCurveForRitual.Evaluate(j).ToStringWithSign();
					Widgets.Label(new Rect(viewRect.x, y2, viewRect.width, 32f), label2);
					y2 += Text.LineHeight;
					totalInfoHeight += Text.LineHeight;
				}
			}
			GUI.color = Color.white;
			qualityDescHeight = y2;
			Widgets.EndScrollView();
			void DrawQualityFactor(ExpectedOutcomeDesc expectedOutcomeDesc, ref float y)
			{
				if (expectedOutcomeDesc != null)
				{
					Rect rect6 = new Rect(viewRect.x, y, viewRect.width, 25f);
					Rect rect7 = default(Rect);
					rect7.x = viewRect.x;
					rect7.width = viewRect.width + 10f;
					rect7.y = y - 3f;
					rect7.height = 28f;
					Rect rect8 = rect7;
					if (even)
					{
						Widgets.DrawLightHighlight(rect8);
					}
					GUI.color = (expectedOutcomeDesc.uncertainOutcome ? ColorLibrary.Yellow : (expectedOutcomeDesc.positive ? ColorLibrary.Green : ColorLibrary.RedReadable));
					Widgets.Label(rect6, "  " + expectedOutcomeDesc.label);
					Text.Anchor = TextAnchor.UpperRight;
					Widgets.Label(rect6, expectedOutcomeDesc.effect);
					Text.Anchor = TextAnchor.UpperLeft;
					if (!expectedOutcomeDesc.noMiddleColumnInfo)
					{
						if (!expectedOutcomeDesc.count.NullOrEmpty())
						{
							float x2 = Text.CalcSize(expectedOutcomeDesc.count).x;
							Rect rect9 = new Rect(rect6);
							rect9.xMin += 220f - x2 / 2f;
							rect9.width = x2;
							Widgets.Label(rect9, expectedOutcomeDesc.count);
						}
						else
						{
							GUI.color = Color.white;
							Texture2D image = (expectedOutcomeDesc.present ? QualityOffsetCheckOn : QualityOffsetCheckOff);
							if (expectedOutcomeDesc.uncertainOutcome)
							{
								image = questionMark;
							}
							Rect rect10 = new Rect(rect6);
							rect10.x += 208f;
							rect10.y -= 1f;
							rect10.width = 24f;
							rect10.height = 24f;
							if (!expectedOutcomeDesc.present)
							{
								if (expectedOutcomeDesc.uncertainOutcome)
								{
									TooltipHandler.TipRegion(rect10, () => "QualityFactorTooltipUncertain".Translate(), 238934347);
								}
								else
								{
									TooltipHandler.TipRegion(rect10, () => "QualityFactorTooltipNotFulfilled".Translate(), 238934347);
								}
							}
							GUI.DrawTexture(rect10, image);
						}
					}
					GUI.color = Color.white;
					if (expectedOutcomeDesc.tip != null && Mouse.IsOver(rect6))
					{
						Widgets.DrawHighlight(rect8);
						TooltipHandler.TipRegion(rect6, () => expectedOutcomeDesc.tip, 976091152);
					}
					y += 28f;
					totalInfoHeight += 28f;
				}
			}
		}

		private float PredictedQuality(out float potentialMax)
		{
			float num = outcome.startingQuality;
			potentialMax = 0f;
			foreach (RitualOutcomeComp comp in outcome.comps)
			{
				ExpectedOutcomeDesc expectedOutcomeDesc = comp.GetExpectedOutcomeDesc(ritual, target, obligation, assignments, ritual?.outcomeEffect?.DataForComp(comp));
				if (expectedOutcomeDesc != null)
				{
					if (!expectedOutcomeDesc.label.NullOrEmpty())
					{
						expectedOutcomeEffects.Add(expectedOutcomeDesc);
					}
					if (expectedOutcomeDesc.uncertainOutcome)
					{
						potentialMax += expectedOutcomeDesc.quality;
					}
					else
					{
						num += expectedOutcomeDesc.quality;
					}
				}
			}
			if (ritual != null && ritual.RepeatPenaltyActive)
			{
				num += ritual.RepeatQualityPenalty;
			}
			Tuple<ExpectationDef, float> expectationsOffset = RitualOutcomeEffectWorker_FromQuality.GetExpectationsOffset(map, ritual?.def);
			if (expectationsOffset != null)
			{
				num += expectationsOffset.Item2;
			}
			num = Mathf.Clamp(num, outcome.minQuality, outcome.maxQuality);
			potentialMax += num;
			potentialMax = Mathf.Clamp(potentialMax, outcome.minQuality, outcome.maxQuality);
			return num;
		}

		private void CalculatePawnPortraitIcons(Pawn pawn)
		{
			tmpPortraitIcons.Clear();
			Ideo ideo = pawn.Ideo;
			if (assignments.Required(pawn))
			{
				tmpPortraitIcons.Add(new PawnPortraitIcon
				{
					color = Color.white,
					icon = IdeoUIUtility.LockedTex,
					tooltip = "Required".Translate()
				});
			}
			if (!ModsConfig.IdeologyActive || ideo == null)
			{
				return;
			}
			if (!Find.IdeoManager.classicMode)
			{
				tmpPortraitIcons.Add(new PawnPortraitIcon
				{
					color = ideo.Color,
					icon = ideo.Icon,
					tooltip = ideo.memberName
				});
				Precept_Role role = ideo.GetRole(pawn);
				if (role != null)
				{
					tmpPortraitIcons.Add(new PawnPortraitIcon
					{
						color = ideo.Color,
						icon = role.Icon,
						tooltip = role.TipLabel
					});
				}
				GUI.color = Color.white;
			}
			Faction homeFaction = pawn.HomeFaction;
			if (homeFaction != null && !homeFaction.IsPlayer)
			{
				tmpPortraitIcons.Add(new PawnPortraitIcon
				{
					color = homeFaction.Color,
					icon = homeFaction.def.FactionIcon,
					tooltip = "Faction".Translate() + ": " + homeFaction.Name + "\n" + homeFaction.def.LabelCap
				});
			}
			if (pawn.IsSlave)
			{
				tmpPortraitIcons.Add(new PawnPortraitIcon
				{
					color = Color.white,
					icon = SlaveryIcon,
					tooltip = "RitualBeginSlaveDesc".Translate()
				});
			}
			if (pawn.IsPrisoner)
			{
				tmpPortraitIcons.Add(new PawnPortraitIcon
				{
					color = Color.white,
					icon = prisonerIcon,
					tooltip = null
				});
			}
			if (!pawn.Awake())
			{
				tmpPortraitIcons.Add(new PawnPortraitIcon
				{
					color = Color.white,
					icon = sleepingIcon,
					tooltip = "RitualBeginSleepingDesc".Translate(pawn)
				});
			}
		}

		private void DrawPawnPortrait(Rect rect, Pawn pawn, bool grayOutIfNoAssignableRole, Action clickHandler = null, Action rightClickHandler = null)
		{
			if (Mouse.IsOver(rect) && Event.current.type == EventType.MouseDown && Event.current.button == 1)
			{
				rightClickHandler();
			}
			Vector2 mp = Event.current.mousePosition;
			if (!assignments.Required(pawn) && DragAndDropWidget.Draggable(dragAndDropGroup, rect, pawn, clickHandler, delegate
			{
				lastHoveredDropArea = DragAndDropWidget.HoveringDropAreaRect(dragAndDropGroup, mp);
			}))
			{
				rect.position = Event.current.mousePosition;
				tmpDelayedGuiCalls.Add(delegate
				{
					DrawInternal(rect, pawn, 0.9f);
				});
			}
			else
			{
				DrawInternal(rect, pawn, 1f);
				Widgets.DrawHighlightIfMouseover(rect);
			}
			void DrawInternal(Rect r, Pawn p, float scale)
			{
				Rect rect2 = new Rect(r.x, r.y, r.width * scale, 50f * scale);
				Rect rect3 = new Rect(r.x, r.y + 50f * scale, r.width * scale, 20f * scale);
				bool flag = grayOutIfNoAssignableRole && !assignments.CanParticipate(pawn, target);
				Material material = (flag ? TexUI.GrayscaleGUI : null);
				GenUI.DrawTextureWithMaterial(rect2, ColonistBar.BGTex, material);
				if (highlightedRole != null && !DragAndDropWidget.Dragging && !assignments.AssignedPawns(highlightedRole).Contains(pawn) && highlightedRole.AppliesToPawn(pawn, out var _, target, null, assignments, ritual, skipReason: true))
				{
					Widgets.DrawHighlight(rect2.ContractedBy(3f));
				}
				RenderTexture texture = PortraitsCache.Get(p, new Vector2(100f, 100f), Rot4.South, new Vector3(0f, 0f, 0.1f), 1.5f);
				GenUI.DrawTextureWithMaterial(rect2, texture, material);
				Widgets.DrawRectFast(rect3, Widgets.WindowBGFillColor);
				Text.Font = GameFont.Tiny;
				Text.Anchor = TextAnchor.MiddleCenter;
				if (flag)
				{
					GUI.color = Color.gray;
				}
				string text = p.LabelShortCap;
				if (flag)
				{
					string text2 = assignments.PawnNotAssignableReason(p, null, target);
					if (!text2.NullOrEmpty())
					{
						text = text + "\n\n" + text2.Colorize(ColorLibrary.RedReadable);
					}
				}
				if (Widgets.LabelFit(rect3, p.LabelShortCap))
				{
					TooltipHandler.TipRegion(rect3, text);
				}
				GUI.color = Color.white;
				Text.Anchor = TextAnchor.UpperLeft;
				Text.Font = GameFont.Small;
				CalculatePawnPortraitIcons(p);
				float num = rect2.yMax;
				float num2 = rect2.xMax;
				float num3 = 20f * scale;
				foreach (PawnPortraitIcon tmpPortraitIcon in tmpPortraitIcons)
				{
					PawnPortraitIcon localIcon = tmpPortraitIcon;
					Rect rect4 = new Rect(num2 - num3, num - num3, num3, num3);
					num2 -= num3;
					if (num2 - num3 < rect2.x)
					{
						num2 = rect2.xMax;
						num -= num3 + 2f;
					}
					GUI.color = (flag ? tmpPortraitIcon.color.SaturationChanged(0f) : tmpPortraitIcon.color);
					Widgets.DrawTextureFitted(rect4, tmpPortraitIcon.icon, 1f);
					GUI.color = Color.white;
					if (tmpPortraitIcon.tooltip != null)
					{
						TooltipHandler.TipRegion(rect4, () => localIcon.tooltip, tmpPortraitIcon.icon.GetInstanceID() ^ p.thingIDNumber);
					}
				}
			}
		}

		private string ExtraPawnAssignmentInfo(IEnumerable<RitualRole> roleGroup, Pawn pawnToBeAssigned = null)
		{
			RitualRole role = roleGroup?.First();
			IEnumerable<Pawn> enumerable = assignments.AssignedPawns(role);
			if (pawnToBeAssigned != null)
			{
				enumerable = enumerable.Concat(new Pawn[1] { pawnToBeAssigned }).Distinct();
			}
			string text = ((pawnToBeAssigned == null) ? role.ExtraInfoForDialog(enumerable) : null);
			Pawn pawn = pawnToBeAssigned ?? enumerable.FirstOrDefault();
			PreceptDef preceptDef = pawn?.Ideo?.GetRole(pawn)?.def;
			if (pawn != null && preceptDef != role.precept && role.substitutable && role.precept != null)
			{
				if (text != null)
				{
					text += "\n\n";
				}
				Precept precept = ritual.ideo.PreceptsListForReading.FirstOrDefault((Precept p) => p.def == role.precept);
				if (precept != null)
				{
					text += "RitualRoleRequiresSocialRole".Translate(precept.Label);
				}
				string text2 = null;
				bool flag = false;
				if (ritual.outcomeEffect != null && !ritual.outcomeEffect.def.comps.NullOrEmpty())
				{
					foreach (RitualOutcomeComp comp in ritual.outcomeEffect.def.comps)
					{
						if (comp is RitualOutcomeComp_RolePresentNotSubstituted)
						{
							if (flag)
							{
								text2 += ", ";
							}
							text2 += comp.GetBonusDescShort();
							flag = true;
						}
					}
				}
				if (!Find.IdeoManager.classicMode)
				{
					text = text + ": " + (flag ? text2 : "None".Translate().CapitalizeFirst().Resolve());
				}
			}
			if (role.required && pawnToBeAssigned == null && assignments.FirstAssignedPawn(role) == null)
			{
				int num = 0;
				foreach (RitualRole item in roleGroup)
				{
					num += item.maxCount;
				}
				text = ((num <= 1) ? ((string)(text + "MessageRitualNeedsAtLeastOneRolePawn".Translate(role.Label))) : ((string)(text + "MessageRitualNeedsAtLeastNumRolePawn".Translate(Find.ActiveLanguageWorker.Pluralize(role.Label), num))));
			}
			return text;
		}

		private string CannotAssignReason(Pawn draggable, IEnumerable<RitualRole> roles, out RitualRole firstRole, out bool mustReplace, bool isReplacing = false)
		{
			int num = 0;
			int num2 = 0;
			firstRole = roles?.FirstOrDefault();
			mustReplace = false;
			string text = assignments.PawnNotAssignableReason(draggable, firstRole, target).CapitalizeFirst();
			if (text == null && firstRole == null && ritual != null && ritual.behavior.def.spectatorFilter != null && !ritual.behavior.def.spectatorFilter.Allowed(draggable))
			{
				text = ritual.behavior.def.spectatorFilter.description;
			}
			if (text == null && firstRole != null)
			{
				bool flag = true;
				foreach (RitualRole role in roles)
				{
					if (!assignments.AssignedPawns(role).Any())
					{
						flag = false;
						break;
					}
					foreach (Pawn item in assignments.AssignedPawns(role))
					{
						if (assignments.ForcedRole(item) != role.id)
						{
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					text = "RoleIsLocked".Translate(firstRole.Label);
				}
			}
			if (text == null && roles != null && !roles.Any((RitualRole r) => assignments.RoleForPawn(draggable) == r))
			{
				foreach (RitualRole role2 in roles)
				{
					if (role2.maxCount <= 0)
					{
						num = -1;
					}
					if (num != -1)
					{
						num += role2.maxCount;
					}
					num2 += assignments.AssignedPawns(role2).Count();
				}
				if (num >= 0 && num <= num2)
				{
					mustReplace = true;
					if (!isReplacing)
					{
						text = "MaxPawnsPerRole".Translate(firstRole.Label, num);
					}
				}
			}
			return text;
		}

		public override void WindowUpdate()
		{
			base.WindowUpdate();
			scrollPositionPawns.y += (float)(pawnsListEdgeScrollDirection * 1000) * Time.deltaTime;
		}

		private void UpdateRoleChangeTargetRole(Pawn p)
		{
			Precept_Role roleToChangeTo = null;
			if (p.Ideo?.GetRole(p) == null)
			{
				roleToChangeTo = RitualUtility.AllRolesForPawn(p).FirstOrDefault((Precept_Role r) => r.Active && r.RequirementsMet(p));
			}
			SetRoleToChangeTo(roleToChangeTo);
		}

		protected void DrawPawnList(Rect viewRect, Rect listRect)
		{
			rolesGroupedTmp.Clear();
			rolesGroupedTmp.AddRange(from r in assignments.AllRolesForReading
				group r by r.mergeId ?? r.id);
			try
			{
				int num = DragAndDropWidget.NewGroup();
				dragAndDropGroup = ((num == -1) ? dragAndDropGroup : num);
				int maxPawnsPerRow = Mathf.FloorToInt((viewRect.width - 8f) / 54f);
				float rowHeight = 0f;
				float curY = 0f;
				float curX = 0f;
				foreach (IGrouping<string, RitualRole> item in rolesGroupedTmp)
				{
					IGrouping<string, RitualRole> localRoleGroup = item;
					RitualRole ritualRole = item.First();
					int num2 = 0;
					foreach (RitualRole item2 in item)
					{
						num2 += item2.maxCount;
					}
					string extraInfo2 = ExtraPawnAssignmentInfo(localRoleGroup);
					IEnumerable<Pawn> enumerable = item.SelectMany((RitualRole r) => assignments.AssignedPawns(r));
					string headline2 = ritualRole.CategoryLabelCap ?? ritualRole.LabelCap;
					Vector2 mp = Event.current.mousePosition;
					DrawRoleGroup(enumerable, headline2, num2, delegate(Pawn p, Vector2 dropPos)
					{
						Pawn pawn3 = (Pawn)DragAndDropWidget.DraggableAt(dragAndDropGroup, mp);
						if (pawn3 != null)
						{
							TryAssignReplace(p, localRoleGroup, pawn3);
						}
						else
						{
							TryAssign(p, localRoleGroup, sendMessage: true, (Pawn)DragAndDropWidget.GetDraggableAfter(dragAndDropGroup, dropPos), doSound: true, insertLast: true);
						}
					}, item, extraInfo2, enumerable.Any() && enumerable.All((Pawn p) => assignments.Required(p)), WarningIcon, null, delegate(Pawn p)
					{
						if (!assignments.Required(p))
						{
							if (!assignments.TryAssignSpectate(p))
							{
								assignments.RemoveParticipant(p);
							}
							SoundDefOf.Tick_High.PlayOneShotOnCamera();
						}
					}, grayOutIfNoAssignableRole: false);
				}
				List<Pawn> spectatorsForReading = assignments.SpectatorsForReading;
				List<Pawn> allPawns = assignments.AllPawns;
				string spectatorLabel = ritual?.behavior?.def.spectatorsLabel ?? ((string)"Spectators".Translate());
				DrawRoleGroup(spectatorsForReading, spectatorLabel, allPawns.Count, delegate(Pawn p, Vector2 dropPos)
				{
					RitualRole firstRole7;
					bool mustReplace9;
					string text9 = CannotAssignReason(p, null, out firstRole7, out mustReplace9);
					if (text9 != null)
					{
						Messages.Message(text9, LookTargets.Invalid, MessageTypeDefOf.RejectInput, historical: false);
					}
					else
					{
						assignments.TryAssignSpectate(p, (Pawn)DragAndDropWidget.GetDraggableAfter(dragAndDropGroup, dropPos));
						SoundDefOf.DropElement.PlayOneShotOnCamera();
					}
				}, DropContextSpectator, null, locked: false, null, delegate(Pawn p)
				{
					TryAssignAnyRole(p);
				}, delegate(Pawn p)
				{
					if (!assignments.Required(p))
					{
						assignments.RemoveParticipant(p);
						SoundDefOf.Tick_High.PlayOneShotOnCamera();
					}
				}, grayOutIfNoAssignableRole: false);
				nonParticipatingPawnCandidatesTmp.Clear();
				nonParticipatingPawnCandidatesTmp.AddRange(allPawns);
				nonParticipatingPawnCandidatesTmp.AddRange(nonAssignablePawns);
				nonParticipatingPawnCandidatesTmp.RemoveDuplicates();
				DrawRoleGroup(nonParticipatingPawnCandidatesTmp.Where((Pawn p) => !assignments.PawnParticipating(p)), "NotParticipating".Translate(), nonParticipatingPawnCandidatesTmp.Count, delegate(Pawn p, Vector2 dropPos)
				{
					assignments.RemoveParticipant(p);
					SoundDefOf.DropElement.PlayOneShotOnCamera();
				}, DropContextNotParticipating, null, locked: false, null, delegate(Pawn p)
				{
					RitualRole firstRole6;
					bool mustReplace8;
					string text8 = CannotAssignReason(p, null, out firstRole6, out mustReplace8);
					if (text8 != null)
					{
						if (!TryAssignAnyRole(p))
						{
							Messages.Message(text8, LookTargets.Invalid, MessageTypeDefOf.RejectInput, historical: false);
						}
					}
					else
					{
						assignments.TryAssignSpectate(p);
						SoundDefOf.Tick_High.PlayOneShotOnCamera();
					}
				}, delegate(Pawn p)
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					string text7 = CannotAssignReason(p, null, out var firstRole5, out var _);
					Action action = ((text7 != null) ? null : ((Action)delegate
					{
						assignments.TryAssignSpectate(p);
					}));
					list.Add(new FloatMenuOption(PostProcessFloatLabel(spectatorLabel, text7, null), action));
					foreach (IGrouping<string, RitualRole> item3 in rolesGroupedTmp)
					{
						IGrouping<string, RitualRole> localRoleGroup2 = item3;
						RitualRole ritualRole3 = item3.First();
						text7 = CannotAssignReason(p, localRoleGroup2, out firstRole5, out var mustReplace6, isReplacing: true);
						Pawn replacing3 = (mustReplace6 ? localRoleGroup2.SelectMany((RitualRole role) => assignments.AssignedPawns(role)).Last() : null);
						action = ((text7 != null) ? null : ((Action)delegate
						{
							if (mustReplace6)
							{
								TryAssignReplace(p, localRoleGroup2, replacing3);
							}
							else
							{
								TryAssign(p, localRoleGroup2, sendMessage: true, null, doSound: true, insertLast: false);
							}
						}));
						list.Add(new FloatMenuOption(PostProcessFloatLabel(ritualRole3.LabelCap, text7, replacing3), action));
					}
					Find.WindowStack.Add(new FloatMenu(list));
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
				}, grayOutIfNoAssignableRole: true);
				highlightedRole = null;
				curY += rowHeight + 4f;
				Text.Font = GameFont.Tiny;
				GUI.color = ColorLibrary.Grey;
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(new Rect(viewRect.x, curY, viewRect.width, 20f), SteamDeck.IsSteamDeckInNonKeyboardMode ? "DragPawnsToRolesInfoController".Translate() : "DragPawnsToRolesInfo".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
				Text.Font = GameFont.Small;
				curY += 20f;
				if (Event.current.type == EventType.Layout)
				{
					listScrollViewHeight = curY;
				}
				foreach (Action tmpDelayedGuiCall in tmpDelayedGuiCalls)
				{
					tmpDelayedGuiCall();
				}
				object obj = DragAndDropWidget.CurrentlyDraggedDraggable();
				Pawn pawn2 = (Pawn)DragAndDropWidget.DraggableAt(dragAndDropGroup, Event.current.mousePosition);
				if (obj != null)
				{
					object obj2 = DragAndDropWidget.HoveringDropArea(dragAndDropGroup);
					if (obj2 != null)
					{
						Rect? rect = DragAndDropWidget.HoveringDropAreaRect(dragAndDropGroup);
						if (lastHoveredDropArea.HasValue && rect.HasValue && rect != lastHoveredDropArea)
						{
							SoundDefOf.DragSlider.PlayOneShotOnCamera();
						}
						lastHoveredDropArea = rect;
					}
					if (obj2 != null && obj2 != DropContextNotParticipating)
					{
						IGrouping<string, RitualRole> grouping = obj2 as IGrouping<string, RitualRole>;
						RitualRole firstRole;
						bool mustReplace;
						string text2 = CannotAssignReason((Pawn)obj, grouping, out firstRole, out mustReplace, grouping != null && pawn2 != null);
						string text3 = ((firstRole == null) ? null : ExtraPawnAssignmentInfo(grouping, (Pawn)obj));
						if (!string.IsNullOrWhiteSpace(text2) || !string.IsNullOrWhiteSpace(text3))
						{
							string text = (string.IsNullOrWhiteSpace(text2) ? text3 : text2);
							Color color = (string.IsNullOrWhiteSpace(text2) ? ColorLibrary.Yellow : ColorLibrary.RedReadable);
							Text.Font = GameFont.Small;
							Vector2 vector = Text.CalcSize(text);
							Rect r2 = new Rect(UI.MousePositionOnUI.x - vector.x / 2f, (float)UI.screenHeight - UI.MousePositionOnUI.y - vector.y - 10f, vector.x, vector.y).ExpandedBy(5f);
							Find.WindowStack.ImmediateWindow(47839543, r2, WindowLayer.Super, delegate
							{
								Text.Font = GameFont.Small;
								GUI.color = color;
								Widgets.Label(r2.AtZero().ContractedBy(5f), text);
								GUI.color = Color.white;
							});
						}
					}
				}
				void DrawRoleGroup(IEnumerable<Pawn> selectedPawns, string headline, int maxPawns, Action<Pawn, Vector2> assignAction, object dropAreaContext, string extraInfo, bool locked, Texture2D extraInfoIcon, Action<Pawn> clickHandler, Action<Pawn> rightClickHandler, bool grayOutIfNoAssignableRole)
				{
					tmpSelectedPawns.AddRange(selectedPawns);
					try
					{
						int num3 = Mathf.Min(maxPawns, tmpSelectedPawns.Count + 1);
						int num4 = Mathf.CeilToInt((float)num3 / (float)maxPawnsPerRow);
						int num5 = Mathf.Min(maxPawnsPerRow, num3);
						int num6 = num5 * 50 + (num5 - 1) * 4;
						int num7 = num4 * 70 + (num4 - 1) * 4;
						Vector2 vector2 = Text.CalcSize(headline);
						int num8 = 60;
						int num9 = Mathf.Max(num6, (int)vector2.x + num8 + ((num8 > 0) ? 10 : 0));
						int num10 = Mathf.FloorToInt((viewRect.width - (curX + 26f)) / 50f);
						bool flag2 = (num10 > 0 && num10 < maxPawns) || maxPawns >= maxPawnsPerRow;
						if (flag2)
						{
							num6 = maxPawnsPerRow * 50 + (maxPawnsPerRow - 1) * 4;
						}
						if (curX + (float)num9 + 26f > viewRect.width || flag2)
						{
							curY += rowHeight;
							rowHeight = 0f;
							curX = 0f;
						}
						float num11 = 0f;
						Rect rect4 = new Rect(viewRect.x + curX, viewRect.y + num11 + curY, vector2.x, vector2.y);
						Rect rect5 = new Rect(rect4.xMax + 10f, rect4.y + (vector2.y - 20f) / 2f, num8, 20f);
						num11 += vector2.y + 4f;
						Rect rect6 = new Rect(viewRect.x + curX, viewRect.y + num11 + curY, num6 + 8, num7 + 8);
						num11 += rect6.height + 10f;
						rowHeight = Mathf.Max(rowHeight, num11);
						curX += num9 + 26;
						GUI.color = (locked ? ColorLibrary.Grey : Color.white);
						Widgets.Label(rect4, headline);
						GUI.color = Color.white;
						Widgets.DrawRectFast(rect6, Widgets.MenuSectionBGFillColor);
						if (dropAreaContext is IGrouping<string, RitualRole> source && Mouse.IsOver(rect6))
						{
							highlightedRole = source.First();
						}
						if (locked)
						{
							Rect rect7 = new Rect(rect5.x, rect5.y, 20f, 20f);
							rect5.x += rect7.width;
							rect5.width -= rect7.height;
							Widgets.DrawTextureFitted(rect7, IdeoUIUtility.LockedTex, 1f);
							TooltipHandler.TipRegion(rect7, () => "Required".Translate(), 93457856);
						}
						if (extraInfo != null)
						{
							Rect rect8 = new Rect(rect5.x, rect5.y, 20f, 20f);
							rect5.x += rect8.width;
							rect5.width -= rect8.height;
							GUI.color = (Mouse.IsOver(rect8) ? Color.white : new Color(0.8f, 0.8f, 0.8f, 1f));
							Widgets.DrawTextureFitted(rect8, extraInfoIcon ?? WarningIcon, 1f);
							GUI.color = Color.white;
							TooltipHandler.TipRegion(rect8, () => extraInfo, 34899345);
						}
						Rect rect9 = rect6.ContractedBy(4f);
						DragAndDropWidget.DropArea(dragAndDropGroup, rect9, delegate(object pawn)
						{
							assignAction((Pawn)pawn, Event.current.mousePosition);
						}, dropAreaContext);
						if (Mouse.IsOver(rect9))
						{
							Widgets.DrawBoxSolidWithOutline(rect6, new Color(0.3f, 0.3f, 0.3f, 1f), new Color(0.5f, 0.5f, 0.5f, 1f), 3);
						}
						GenUI.DrawElementStack(rect9, 70f, tmpSelectedPawns, delegate(Rect r, Pawn p)
						{
							DrawPawnPortrait(r, p, grayOutIfNoAssignableRole, delegate
							{
								clickHandler?.Invoke(p);
							}, delegate
							{
								rightClickHandler?.Invoke(p);
							});
						}, (Pawn p) => 50f, 4f, 4f, allowOrderOptimization: false);
					}
					finally
					{
						tmpSelectedPawns.Clear();
					}
				}
			}
			finally
			{
				tmpDelayedGuiCalls.Clear();
			}
			pawnsListEdgeScrollDirection = 0;
			if (DragAndDropWidget.CurrentlyDraggedDraggable() != null)
			{
				Rect rect2 = new Rect(viewRect.x, scrollPositionPawns.y, viewRect.width, 30f);
				Rect rect3 = new Rect(viewRect.x, scrollPositionPawns.y + (listRect.height - 30f), viewRect.width, 30f);
				if (Mouse.IsOver(rect2))
				{
					pawnsListEdgeScrollDirection = -1;
				}
				else if (Mouse.IsOver(rect3))
				{
					pawnsListEdgeScrollDirection = 1;
				}
			}
			bool DoTryAssign(Pawn pawn, IEnumerable<RitualRole> roleGroup, bool sendMessage, Pawn insertBefore, bool doSound)
			{
				if (sendMessage && SendTryAssignMessages(pawn, roleGroup, isReplacing: false))
				{
					return false;
				}
				if (!sendMessage && CannotAssignReason(pawn, roleGroup, out var _, out var _) != null)
				{
					return false;
				}
				foreach (RitualRole item4 in roleGroup)
				{
					if (assignments.TryAssign(pawn, item4, target, out var _, null, insertBefore))
					{
						if (item4 is RitualRoleIdeoRoleChanger)
						{
							UpdateRoleChangeTargetRole(pawn);
							if (outcome != null)
							{
								foreach (RitualOutcomeComp comp in outcome.comps)
								{
									comp.Notify_AssignementsChanged(assignments, ritual?.outcomeEffect?.DataForComp(comp));
								}
							}
						}
						if (doSound)
						{
							SoundDefOf.DropElement.PlayOneShotOnCamera();
						}
						return true;
					}
				}
				return false;
			}
			string PostProcessFloatLabel(string label, string unavailableReason, Pawn replacing)
			{
				string text4 = label;
				if (unavailableReason != null)
				{
					text4 += " (" + "DisabledLower".Translate().CapitalizeFirst() + ": " + unavailableReason + ")";
				}
				if (replacing != null)
				{
					text4 += " (" + "RitualRoleReplaces".Translate(replacing.Named("PAWN")) + ")";
				}
				return "AssignToRole".Translate(text4);
			}
			bool SendTryAssignMessages(Pawn pawn, IEnumerable<RitualRole> roleGroup, bool isReplacing)
			{
				RitualRole firstRole4;
				bool mustReplace5;
				string text6 = CannotAssignReason(pawn, roleGroup, out firstRole4, out mustReplace5, isReplacing);
				if (text6 != null)
				{
					Messages.Message(text6, LookTargets.Invalid, MessageTypeDefOf.RejectInput, historical: false);
					return true;
				}
				return false;
			}
			bool TryAssign(Pawn pawn, IEnumerable<RitualRole> roleGroup, bool sendMessage, Pawn insertBefore, bool doSound, bool insertLast)
			{
				try
				{
					int num12 = 0;
					foreach (RitualRole item5 in roleGroup)
					{
						if (item5.maxCount == 0)
						{
							num12 = -1;
						}
						else if (item5.maxCount > 0)
						{
							num12 += item5.maxCount;
						}
						tmpAssignedPawns.AddRange(assignments.AssignedPawns(item5));
					}
					if ((num12 > 0 && tmpAssignedPawns.Count == num12) || (insertBefore != null && !tmpAssignedPawns.Contains(insertBefore)))
					{
						return DoTryAssign(pawn, roleGroup, sendMessage, null, doSound);
					}
					foreach (Pawn tmpAssignedPawn in tmpAssignedPawns)
					{
						assignments.TryUnassignAnyRole(tmpAssignedPawn);
					}
					if (insertBefore == null)
					{
						if (insertLast)
						{
							tmpAssignedPawns.Add(pawn);
						}
						else
						{
							tmpAssignedPawns.Insert(0, pawn);
						}
					}
					else
					{
						tmpAssignedPawns.Insert(tmpAssignedPawns.IndexOf(insertBefore), pawn);
					}
					bool result = false;
					foreach (Pawn tmpAssignedPawn2 in tmpAssignedPawns)
					{
						bool flag3 = DoTryAssign(tmpAssignedPawn2, roleGroup, sendMessage && tmpAssignedPawn2 == pawn, null, tmpAssignedPawn2 == pawn);
						if (tmpAssignedPawn2 == pawn)
						{
							result = flag3;
						}
					}
					return result;
				}
				finally
				{
					tmpAssignedPawns.Clear();
				}
			}
			bool TryAssignAnyRole(Pawn p)
			{
				string text5 = null;
				bool flag = rolesGroupedTmp.Count == 1;
				RitualRole firstRole2;
				foreach (IGrouping<string, RitualRole> item6 in rolesGroupedTmp)
				{
					text5 = CannotAssignReason(p, item6, out firstRole2, out var _, isReplacing: true);
					if (text5 == null && TryAssign(p, item6, sendMessage: false, null, doSound: true, insertLast: false))
					{
						return true;
					}
				}
				foreach (IGrouping<string, RitualRole> item7 in rolesGroupedTmp)
				{
					text5 = CannotAssignReason(p, item7, out firstRole2, out var mustReplace3, isReplacing: true);
					if (text5 == null)
					{
						Pawn replacing2 = (mustReplace3 ? item7.SelectMany((RitualRole role) => assignments.AssignedPawns(role)).Last() : null);
						if (TryAssignReplace(p, item7, replacing2))
						{
							return true;
						}
					}
				}
				if (flag && text5 != null)
				{
					Messages.Message(text5, LookTargets.Invalid, MessageTypeDefOf.RejectInput, historical: false);
				}
				SoundDefOf.ClickReject.PlayOneShotOnCamera();
				return false;
			}
			bool TryAssignReplace(Pawn pawn, IEnumerable<RitualRole> roleGroup, Pawn replacing)
			{
				if (!SendTryAssignMessages(pawn, roleGroup, isReplacing: true))
				{
					bool num13 = assignments.PawnSpectating(pawn);
					RitualRole ritualRole2 = assignments.RoleForPawn(pawn);
					Pawn insertBefore2 = roleGroup.SelectMany((RitualRole r) => assignments.AssignedPawns(r)).SkipWhile((Pawn p) => p != replacing).FirstOrDefault();
					assignments.RemoveParticipant(replacing);
					TryAssign(pawn, roleGroup, sendMessage: true, insertBefore2, doSound: true, insertLast: true);
					RitualRoleAssignments.FailReason failReason2;
					if (num13)
					{
						assignments.TryAssignSpectate(replacing);
					}
					else if (ritualRole2 != null && assignments.TryAssign(replacing, ritualRole2, target, out failReason2))
					{
						if (outcome != null)
						{
							foreach (RitualOutcomeComp comp2 in outcome.comps)
							{
								comp2.Notify_AssignementsChanged(assignments, ritual?.outcomeEffect?.DataForComp(comp2));
							}
						}
						UpdateRoleChangeTargetRole(pawn);
					}
				}
				return roleGroup.Contains(assignments.RoleForPawn(pawn));
			}
		}

		public void SetRoleToChangeTo(Precept_Role role)
		{
			assignments.SetRoleChangeSelection(role);
		}

		protected IEnumerable<string> BlockingIssues()
		{
			if (assignments.Participants.Count() == 0)
			{
				yield return "MessageRitualNeedsAtLeastOnePerson".Translate();
			}
			foreach (Pawn participant in assignments.Participants)
			{
				if (!participant.IsPrisoner && !participant.SafeTemperatureRange().IncludesEpsilon(target.Cell.GetTemperature(target.Map)))
				{
					yield return "CantJoinRitualInExtremeWeather".Translate();
					break;
				}
			}
			if (ritual == null)
			{
				yield break;
			}
			if (ritual.behavior.SpectatorsRequired() && assignments.SpectatorsForReading.Count == 0)
			{
				yield return "MessageRitualNeedsAtLeastOneSpectator".Translate();
			}
			if (ritual.outcomeEffect != null)
			{
				foreach (string item in ritual.outcomeEffect.BlockingIssues(ritual, target, assignments))
				{
					yield return item;
				}
			}
			if (ritual.obligationTargetFilter != null)
			{
				foreach (string blockingIssue in ritual.obligationTargetFilter.GetBlockingIssues(target, assignments))
				{
					yield return blockingIssue;
				}
			}
			if (!ritual.behavior.def.roles.NullOrEmpty())
			{
				foreach (IGrouping<string, RitualRole> item2 in from r in ritual.behavior.def.roles
					group r by r.mergeId ?? r.id)
				{
					RitualRole firstRole = item2.First();
					int requiredPawnCount = item2.Count((RitualRole r) => r.required);
					if (requiredPawnCount <= 0)
					{
						continue;
					}
					IEnumerable<Pawn> selectedPawns = item2.SelectMany((RitualRole r) => assignments.AssignedPawns(r));
					foreach (Pawn item3 in selectedPawns)
					{
						string text = assignments.PawnNotAssignableReason(item3, firstRole, target);
						if (text != null)
						{
							yield return text;
						}
					}
					if (requiredPawnCount == 1 && !selectedPawns.Any())
					{
						yield return "MessageRitualNeedsAtLeastOneRolePawn".Translate(firstRole.Label);
					}
					else if (requiredPawnCount > 1 && selectedPawns.Count() < requiredPawnCount)
					{
						yield return "MessageRitualNeedsAtLeastNumRolePawn".Translate(Find.ActiveLanguageWorker.Pluralize(firstRole.Label), requiredPawnCount);
					}
				}
				if (!assignments.ExtraRequiredPawnsForReading.NullOrEmpty())
				{
					foreach (Pawn item4 in assignments.ExtraRequiredPawnsForReading)
					{
						string text2 = assignments.PawnNotAssignableReason(item4, assignments.RoleForPawn(item4), target);
						if (text2 != null)
						{
							yield return text2;
						}
					}
				}
			}
			if (ritual.ritualOnlyForIdeoMembers && !assignments.Participants.Any((Pawn p) => p.Ideo == ritual.ideo))
			{
				yield return "MessageNeedAtLeastOneParticipantOfIdeo".Translate(ritual.ideo.memberName);
			}
		}

		private void DrawRoleSelection(Pawn pawn, Rect rect)
		{
			Precept_Role roleChangeSelection = assignments.RoleChangeSelection;
			Precept_Role currentRole = pawn?.Ideo?.GetRole(pawn);
			if (roleChangeSelection == null && currentRole == null)
			{
				UpdateRoleChangeTargetRole(pawn);
				roleChangeSelection = assignments.RoleChangeSelection;
			}
			if (roleChangeSelection != null || currentRole != null)
			{
				SocialCardUtility.DrawPawnRole(pawn, roleChangeSelection, (roleChangeSelection != null) ? roleChangeSelection.LabelCap : "RemoveRole".Translate(currentRole.Label).Resolve(), rect, drawLine: false);
			}
			Rect rect2 = new Rect(rect.x + 220f, rect.y + 2f, 140f, 32f);
			bool flag = pawn != null && pawn.Ideo != null;
			if (!flag)
			{
				GUI.color = Color.gray;
			}
			if (cachedRoles.Count > 1 && Widgets.ButtonText(rect2, "ChooseNewRole".Translate() + "...", drawBackground: true, doMouseoverSound: true, flag))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				if (currentRole != null)
				{
					list.Add(new FloatMenuOption("None".Translate(), delegate
					{
						confirmText = "ChooseRoleConfirmUnassign".Translate(currentRole.Named("ROLE"), pawn.Named("PAWN")) + "\n\n" + "ChooseRoleConfirmAssignPostfix".Translate();
						SetRoleToChangeTo(null);
					}, Widgets.PlaceholderIconTex, Color.white));
				}
				foreach (Precept_Role cachedRole in cachedRoles)
				{
					Precept_Role newRole = cachedRole;
					if (newRole == roleChangeSelection || newRole == currentRole || !newRole.Active || !newRole.RequirementsMet(pawn) || (newRole.def.leaderRole && pawn.Ideo != Faction.OfPlayer.ideos.PrimaryIdeo))
					{
						continue;
					}
					string text = newRole.LabelForPawn(pawn) + " (" + newRole.def.label + ")";
					TaggedString confirmTextLocal = "ChooseRoleConfirmAssign".Translate(newRole.Named("ROLE"), pawn.Named("PAWN"));
					string extraConfirmText = RitualUtility.RoleChangeConfirmation(pawn, currentRole, newRole);
					Pawn pawn2 = newRole.ChosenPawns().FirstOrDefault();
					if (pawn2 != null && newRole is Precept_RoleSingle)
					{
						text = text + ": " + pawn2.LabelShort;
					}
					if (!extraConfirmText.NullOrEmpty())
					{
						list.Add(new FloatMenuOption(text, delegate
						{
							confirmText = confirmTextLocal + "\n\n" + extraConfirmText + "\n\n" + "ChooseRoleConfirmAssignPostfix".Translate();
							SetRoleToChangeTo(newRole);
						}, newRole.Icon, newRole.ideo.Color, MenuOptionPriority.Default, DrawTooltip)
						{
							orderInPriority = newRole.def.displayOrderInImpact
						});
					}
					else
					{
						list.Add(new FloatMenuOption(text, delegate
						{
							newRole.Assign(pawn, addThoughts: true);
						}, newRole.Icon, newRole.ideo.Color, MenuOptionPriority.Default, DrawTooltip)
						{
							orderInPriority = newRole.def.displayOrderInImpact
						});
					}
					void DrawTooltip(Rect r)
					{
						TipSignal tip = new TipSignal(() => newRole.GetTip(), pawn.thingIDNumber * 39);
						TooltipHandler.TipRegion(r, tip);
					}
				}
				foreach (Precept_Role cachedRole2 in cachedRoles)
				{
					if ((cachedRole2 != roleChangeSelection && !cachedRole2.RequirementsMet(pawn)) || !cachedRole2.Active)
					{
						string text2 = cachedRole2.LabelForPawn(pawn) + " (" + cachedRole2.def.label + ")";
						if (cachedRole2.ChosenPawnSingle() != null)
						{
							text2 = text2 + ": " + cachedRole2.ChosenPawnSingle().LabelShort;
						}
						else if (!cachedRole2.RequirementsMet(pawn))
						{
							text2 = text2 + ": " + cachedRole2.GetFirstUnmetRequirement(pawn).GetLabel(cachedRole2).CapitalizeFirst();
						}
						else if (!cachedRole2.Active && cachedRole2.def.activationBelieverCount > cachedRole2.ideo.ColonistBelieverCountCached)
						{
							text2 += ": " + "InactiveRoleRequiresMoreBelievers".Translate(cachedRole2.def.activationBelieverCount, cachedRole2.ideo.memberName, cachedRole2.ideo.ColonistBelieverCountCached).CapitalizeFirst();
						}
						list.Add(new FloatMenuOption(text2, null, cachedRole2.Icon, cachedRole2.ideo.Color)
						{
							orderInPriority = cachedRole2.def.displayOrderInImpact
						});
					}
				}
				if (list.Any())
				{
					Find.WindowStack.Add(new FloatMenu(list));
				}
			}
			GUI.color = Color.white;
		}
	}
}
