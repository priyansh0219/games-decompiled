using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class MainTabWindow_Research : MainTabWindow
	{
		private class ResearchTabRecord : TabRecord
		{
			public readonly ResearchTabDef def;

			public ResearchProjectDef firstMatch;

			public bool AnyMatches => firstMatch != null;

			public ResearchTabRecord(ResearchTabDef def, string label, Action clickedAction, Func<bool> selected)
				: base(label, clickedAction, selected)
			{
				this.def = def;
			}

			public void Reset()
			{
				firstMatch = null;
				labelColor = null;
			}
		}

		protected ResearchProjectDef selectedProject;

		private ScrollPositioner scrollPositioner = new ScrollPositioner();

		private bool requiredByThisFound;

		private Vector2 leftScrollPosition = Vector2.zero;

		private float leftScrollViewHeight;

		private Vector2 rightScrollPosition;

		private float rightViewWidth;

		private float rightViewHeight;

		private ResearchTabDef curTabInt;

		private QuickSearchWidget quickSearchWidget = new QuickSearchWidget();

		private bool editMode;

		private List<ResearchProjectDef> draggingTabs = new List<ResearchProjectDef>();

		private List<ResearchTabRecord> tabs = new List<ResearchTabRecord>();

		private List<ResearchProjectDef> cachedVisibleResearchProjects;

		private Dictionary<ResearchProjectDef, List<Pair<ResearchPrerequisitesUtility.UnlockedHeader, List<Def>>>> cachedUnlockedDefsGroupedByPrerequisites;

		private readonly HashSet<ResearchProjectDef> matchingProjects = new HashSet<ResearchProjectDef>();

		private float leftViewDebugHeight;

		private float leftStartAreaHeight = 68f;

		private const float leftAreaWidthPercent = 0.22f;

		private const float LeftAreaWidthMin = 200f;

		private const int ModeSelectButHeight = 40;

		private const float ProjectTitleHeight = 50f;

		private const float ProjectTitleLeftMargin = 0f;

		private const int ResearchItemW = 140;

		private const int ResearchItemH = 50;

		private const int ResearchItemPaddingW = 50;

		private const int ResearchItemPaddingH = 50;

		private const int ColumnMaxProjects = 6;

		private const float LineOffsetFactor = 0.48f;

		private const float IndentSpacing = 6f;

		private const float RowHeight = 24f;

		private const float LeftVerticalPadding = 10f;

		private const float LeftStartButHeight = 68f;

		private const float LeftProgressBarHeight = 35f;

		private const float SearchBoxHeight = 24f;

		private const int SearchHighlightMargin = 12;

		private const float TopCornerTexSize = 7f;

		private const KeyCode SelectMultipleKey = KeyCode.LeftShift;

		private const KeyCode DeselectKey = KeyCode.LeftControl;

		private static readonly Texture2D ResearchBarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.8f, 0.85f));

		private static readonly Texture2D ResearchBarBGTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.1f, 0.1f));

		private static readonly Texture2D TopCornerTex = ContentFinder<Texture2D>.Get("UI/Misc/TopCorner");

		private static readonly Color FulfilledPrerequisiteColor = Color.green;

		private static readonly Color MissingPrerequisiteColor = ColorLibrary.RedReadable;

		private static readonly Color ProjectWithMissingPrerequisiteLabelColor = Color.gray;

		private static readonly Color NoMatchTintColor = Widgets.MenuSectionBGFillColor;

		private static readonly Color SourceColor_Mod = new Color(0.31f, 0.31f, 0.31f);

		private const float NoMatchTintFactor = 0.4f;

		private static readonly CachedTexture TechprintRequirementTex = new CachedTexture("UI/Icons/Research/Techprint");

		private static readonly CachedTexture StudyRequirementTex = new CachedTexture("UI/Icons/Research/Study");

		private List<(BuildableDef, List<string>)> cachedDefsWithMissingMemes = new List<(BuildableDef, List<string>)>();

		private static Dictionary<string, string> labelsWithNewlineCached = new Dictionary<string, string>();

		private static Dictionary<Pair<int, int>, string> techprintsInfoCached = new Dictionary<Pair<int, int>, string>();

		private List<string> tmpSuffixesForUnlocked = new List<string>();

		private static List<Building> tmpAllBuildings = new List<Building>();

		private ResearchTabDef CurTab
		{
			get
			{
				return curTabInt;
			}
			set
			{
				if (value != curTabInt)
				{
					curTabInt = value;
					Vector2 vector = ViewSize(CurTab);
					rightViewWidth = vector.x;
					rightViewHeight = vector.y;
					rightScrollPosition = Vector2.zero;
				}
			}
		}

		private ResearchTabRecord CurTabRecord
		{
			get
			{
				foreach (ResearchTabRecord tab in tabs)
				{
					if (tab.def == CurTab)
					{
						return tab;
					}
				}
				return null;
			}
		}

		private bool ColonistsHaveResearchBench
		{
			get
			{
				bool result = false;
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					if (maps[i].listerBuildings.ColonistsHaveResearchBench())
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		public List<ResearchProjectDef> VisibleResearchProjects
		{
			get
			{
				if (cachedVisibleResearchProjects == null)
				{
					cachedVisibleResearchProjects = new List<ResearchProjectDef>(DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where((ResearchProjectDef d) => Find.Storyteller.difficulty.AllowedBy(d.hideWhen) || d == Find.ResearchManager.currentProj));
				}
				return cachedVisibleResearchProjects;
			}
		}

		public override Vector2 InitialSize
		{
			get
			{
				Vector2 initialSize = base.InitialSize;
				float b = UI.screenHeight - 35;
				float num = 0f;
				foreach (ResearchTabDef allDef in DefDatabase<ResearchTabDef>.AllDefs)
				{
					num = Mathf.Max(num, ViewSize(allDef).y);
				}
				float b2 = Margin + 10f + 32f + 10f + num + 10f + 10f + Margin;
				float a = Mathf.Max(initialSize.y, b2);
				initialSize.y = Mathf.Min(a, b);
				return initialSize;
			}
		}

		private Vector2 ViewSize(ResearchTabDef tab)
		{
			List<ResearchProjectDef> visibleResearchProjects = VisibleResearchProjects;
			float num = 0f;
			float num2 = 0f;
			Text.Font = GameFont.Small;
			for (int i = 0; i < visibleResearchProjects.Count; i++)
			{
				ResearchProjectDef researchProjectDef = visibleResearchProjects[i];
				if (researchProjectDef.tab == tab)
				{
					Rect rect = new Rect(0f, 0f, 140f, 0f);
					Widgets.LabelCacheHeight(ref rect, GetLabelWithNewlineCached(GetLabel(researchProjectDef)), renderLabel: false);
					num = Mathf.Max(num, PosX(researchProjectDef) + 140f);
					num2 = Mathf.Max(num2, PosY(researchProjectDef) + rect.height);
				}
			}
			return new Vector2(num + 20f + 12f, num2 + 20f + 12f);
		}

		public override void PreOpen()
		{
			base.PreOpen();
			selectedProject = Find.ResearchManager.currentProj;
			scrollPositioner.Arm();
			cachedVisibleResearchProjects = null;
			cachedUnlockedDefsGroupedByPrerequisites = null;
			quickSearchWidget.Reset();
			if (CurTab == null)
			{
				if (selectedProject != null)
				{
					CurTab = selectedProject.tab;
				}
				else
				{
					CurTab = ResearchTabDefOf.Main;
				}
			}
			UpdateSearchResults();
		}

		public override void DoWindowContents(Rect inRect)
		{
			windowRect.width = UI.screenWidth;
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			float width = Mathf.Max(200f, inRect.width * 0.22f);
			Rect leftOutRect = new Rect(0f, 0f, width, inRect.height - 24f - 10f);
			Rect searchRect = new Rect(0f, leftOutRect.yMax + 10f, width, 24f);
			Rect rightOutRect = new Rect(leftOutRect.xMax + 10f, 0f, inRect.width - leftOutRect.width - 10f, inRect.height);
			DrawSearchRect(searchRect);
			DrawLeftRect(leftOutRect);
			DrawRightRect(rightOutRect);
		}

		private void DrawSearchRect(Rect searchRect)
		{
			quickSearchWidget.OnGUI(searchRect, UpdateSearchResults);
		}

		private void DrawLeftRect(Rect leftOutRect)
		{
			float num = leftOutRect.height - (10f + leftStartAreaHeight) - 45f;
			Rect rect = leftOutRect;
			Widgets.BeginGroup(rect);
			if (selectedProject != null)
			{
				Rect outRect = new Rect(0f, 0f, rect.width, num - leftViewDebugHeight);
				Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, leftScrollViewHeight);
				Widgets.BeginScrollView(outRect, ref leftScrollPosition, viewRect);
				float num2 = 0f;
				Text.Font = GameFont.Medium;
				GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
				Rect rect2 = new Rect(0f, num2, viewRect.width - 0f, 50f);
				Widgets.LabelCacheHeight(ref rect2, selectedProject.LabelCap);
				GenUI.ResetLabelAlign();
				Text.Font = GameFont.Small;
				num2 += rect2.height;
				Rect rect3 = new Rect(0f, num2, viewRect.width, 0f);
				Widgets.LabelCacheHeight(ref rect3, selectedProject.description);
				num2 += rect3.height;
				Rect rect4 = new Rect(0f, num2, viewRect.width, 500f);
				num2 += DrawTechprintInfo(rect4, selectedProject);
				if ((int)selectedProject.techLevel > (int)Faction.OfPlayer.def.techLevel)
				{
					float num3 = selectedProject.CostFactor(Faction.OfPlayer.def.techLevel);
					Rect rect5 = new Rect(0f, num2, viewRect.width, 0f);
					string text = "TechLevelTooLow".Translate(Faction.OfPlayer.def.techLevel.ToStringHuman(), selectedProject.techLevel.ToStringHuman(), (1f / num3).ToStringPercent());
					if (num3 != 1f)
					{
						text += " " + "ResearchCostComparison".Translate(selectedProject.baseCost.ToString("F0"), selectedProject.CostApparent.ToString("F0"));
					}
					Widgets.LabelCacheHeight(ref rect5, text);
					num2 += rect5.height;
				}
				num2 += DrawResearchPrereqs(rect: new Rect(0f, num2, viewRect.width, 500f), project: selectedProject);
				num2 += DrawResearchBenchRequirements(rect: new Rect(0f, num2, viewRect.width, 500f), project: selectedProject);
				num2 += DrawStudyRequirements(rect: new Rect(0f, num2, viewRect.width, 500f), project: selectedProject);
				Rect rect9 = new Rect(0f, num2, viewRect.width, 500f);
				num2 += DrawUnlockableHyperlinks(rect9, selectedProject);
				Rect rect10 = new Rect(0f, num2, viewRect.width, 500f);
				num2 += DrawContentSource(rect10, selectedProject);
				num2 += 3f;
				leftScrollViewHeight = num2;
				Widgets.EndScrollView();
				Rect rect11 = new Rect(0f, outRect.yMax + 10f + leftViewDebugHeight, rect.width, leftStartAreaHeight);
				if (selectedProject.CanStartNow && selectedProject != Find.ResearchManager.currentProj)
				{
					leftStartAreaHeight = 68f;
					if (Widgets.ButtonText(rect11, "Research".Translate()))
					{
						AttemptBeginResearch(selectedProject);
					}
				}
				else
				{
					string text2 = "";
					if (selectedProject.IsFinished)
					{
						text2 = "Finished".Translate();
						Text.Anchor = TextAnchor.MiddleCenter;
					}
					else if (selectedProject == Find.ResearchManager.currentProj)
					{
						text2 = "InProgress".Translate();
						Text.Anchor = TextAnchor.MiddleCenter;
					}
					else
					{
						text2 = "";
						if (!selectedProject.PrerequisitesCompleted)
						{
							text2 += "\n  " + "PrerequisitesNotCompleted".Translate();
						}
						if (!selectedProject.TechprintRequirementMet)
						{
							text2 += "\n  " + "InsufficientTechprintsApplied".Translate(selectedProject.TechprintsApplied, selectedProject.TechprintCount);
						}
						if (!selectedProject.PlayerHasAnyAppropriateResearchBench)
						{
							text2 += "\n  " + "MissingRequiredResearchFacilities".Translate();
						}
						if (!selectedProject.PlayerMechanitorRequirementMet)
						{
							text2 += "\n  " + "MissingRequiredMechanitor".Translate();
						}
						if (!selectedProject.StudiedThingsRequirementsMet)
						{
							text2 = text2 + "\n" + selectedProject.requiredStudied.Select((ThingDef t) => "NotStudied".Translate(t.LabelCap).ToString()).ToLineList("  ");
						}
						if (text2.NullOrEmpty())
						{
							Log.ErrorOnce("Research " + selectedProject.defName + " locked but no reasons given", selectedProject.GetHashCode() ^ 0x5FE2BD1);
						}
						text2 = "Locked".Translate() + ":" + text2;
					}
					leftStartAreaHeight = Mathf.Max(Text.CalcHeight(text2, rect11.width - 10f) + 10f, 68f);
					Widgets.DrawHighlight(rect11);
					Widgets.Label(rect11.ContractedBy(5f), text2);
					Text.Anchor = TextAnchor.UpperLeft;
				}
				Rect rect12 = new Rect(0f, rect11.yMax + 10f, rect.width, 35f);
				Widgets.FillableBar(rect12, selectedProject.ProgressPercent, ResearchBarFillTex, ResearchBarBGTex, doBorder: true);
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(rect12, selectedProject.ProgressApparent.ToString("F0") + " / " + selectedProject.CostApparent.ToString("F0"));
				Text.Anchor = TextAnchor.UpperLeft;
				leftViewDebugHeight = 0f;
				if (Prefs.DevMode && selectedProject != Find.ResearchManager.currentProj && !selectedProject.IsFinished)
				{
					Text.Font = GameFont.Tiny;
					Rect rect13 = new Rect(rect11.x, outRect.yMax, 120f, 30f);
					if (Widgets.ButtonText(rect13, "Debug: Finish now"))
					{
						Find.ResearchManager.currentProj = selectedProject;
						Find.ResearchManager.FinishProject(selectedProject);
					}
					Text.Font = GameFont.Small;
					leftViewDebugHeight = rect13.height;
				}
				if (Prefs.DevMode && !selectedProject.TechprintRequirementMet)
				{
					Text.Font = GameFont.Tiny;
					Rect rect14 = new Rect(rect11.x + 120f, outRect.yMax, 120f, 30f);
					if (Widgets.ButtonText(rect14, "Debug: Apply techprint"))
					{
						Find.ResearchManager.ApplyTechprint(selectedProject, null);
						SoundDefOf.TechprintApplied.PlayOneShotOnCamera();
					}
					Text.Font = GameFont.Small;
					leftViewDebugHeight = rect14.height;
				}
			}
			Widgets.EndGroup();
		}

		private void AttemptBeginResearch(ResearchProjectDef projectToStart)
		{
			List<(BuildableDef, List<string>)> list = ComputeUnlockedDefsThatHaveMissingMemes(projectToStart);
			if (!list.Any())
			{
				DoBeginResearch(projectToStart);
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("ResearchProjectHasDefsWithMissingMemes".Translate(projectToStart.LabelCap)).Append(":");
			stringBuilder.AppendLine();
			foreach (var (buildableDef, items) in list)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("  - ").Append(buildableDef.LabelCap.Colorize(ColoredText.NameColor)).Append(" (")
					.Append(items.ToCommaList())
					.Append(")");
			}
			Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(stringBuilder.ToString(), delegate
			{
				DoBeginResearch(projectToStart);
			}));
			SoundDefOf.Tick_Low.PlayOneShotOnCamera();
		}

		private List<(BuildableDef, List<string>)> ComputeUnlockedDefsThatHaveMissingMemes(ResearchProjectDef project)
		{
			cachedDefsWithMissingMemes.Clear();
			if (!ModsConfig.IdeologyActive)
			{
				return cachedDefsWithMissingMemes;
			}
			if (Faction.OfPlayer.ideos?.PrimaryIdeo == null)
			{
				return cachedDefsWithMissingMemes;
			}
			foreach (Def unlockedDef in project.UnlockedDefs)
			{
				if (!(unlockedDef is BuildableDef buildableDef) || buildableDef.canGenerateDefaultDesignator)
				{
					continue;
				}
				List<string> list = null;
				foreach (MemeDef item in DefDatabase<MemeDef>.AllDefsListForReading)
				{
					if (!Faction.OfPlayer.ideos.HasAnyIdeoWithMeme(item) && item.AllDesignatorBuildables.Contains(buildableDef))
					{
						if (list == null)
						{
							list = new List<string>();
						}
						list.Add(item.LabelCap);
					}
				}
				if (list != null)
				{
					cachedDefsWithMissingMemes.Add((buildableDef, list));
				}
			}
			return cachedDefsWithMissingMemes;
		}

		private void DoBeginResearch(ResearchProjectDef projectToStart)
		{
			SoundDefOf.ResearchStart.PlayOneShotOnCamera();
			Find.ResearchManager.currentProj = projectToStart;
			TutorSystem.Notify_Event("StartResearchProject");
			if (!ColonistsHaveResearchBench)
			{
				Messages.Message("MessageResearchMenuWithoutBench".Translate(), MessageTypeDefOf.CautionInput);
			}
		}

		private float CoordToPixelsX(float x)
		{
			return x * 190f;
		}

		private float CoordToPixelsY(float y)
		{
			return y * 100f;
		}

		private float PixelsToCoordX(float x)
		{
			return x / 190f;
		}

		private float PixelsToCoordY(float y)
		{
			return y / 100f;
		}

		private float PosX(ResearchProjectDef d)
		{
			return CoordToPixelsX(d.ResearchViewX);
		}

		private float PosY(ResearchProjectDef d)
		{
			return CoordToPixelsY(d.ResearchViewY);
		}

		public override void PostOpen()
		{
			base.PostOpen();
			tabs.Clear();
			foreach (ResearchTabDef tabDef in DefDatabase<ResearchTabDef>.AllDefs)
			{
				tabs.Add(new ResearchTabRecord(tabDef, tabDef.LabelCap, delegate
				{
					CurTab = tabDef;
				}, () => CurTab == tabDef));
			}
		}

		private void DrawRightRect(Rect rightOutRect)
		{
			rightOutRect.yMin += 32f;
			Widgets.DrawMenuSection(rightOutRect);
			TabDrawer.DrawTabs(rightOutRect, tabs);
			if (Prefs.DevMode)
			{
				Rect rect = rightOutRect;
				rect.yMax = rect.yMin + 20f;
				rect.xMin = rect.xMax - 80f;
				Rect butRect = rect.RightPartPixels(30f);
				rect = rect.LeftPartPixels(rect.width - 30f);
				Widgets.CheckboxLabeled(rect, "Edit", ref editMode);
				if (Widgets.ButtonImageFitted(butRect, TexButton.Copy))
				{
					StringBuilder stringBuilder = new StringBuilder();
					foreach (ResearchProjectDef item in VisibleResearchProjects.Where((ResearchProjectDef def) => def.Debug_IsPositionModified()))
					{
						stringBuilder.AppendLine(item.defName);
						stringBuilder.AppendLine(string.Format("  <researchViewX>{0}</researchViewX>", item.ResearchViewX.ToString("F2")));
						stringBuilder.AppendLine(string.Format("  <researchViewY>{0}</researchViewY>", item.ResearchViewY.ToString("F2")));
						stringBuilder.AppendLine();
					}
					GUIUtility.systemCopyBuffer = stringBuilder.ToString();
					Messages.Message("Modified data copied to clipboard.", MessageTypeDefOf.SituationResolved, historical: false);
				}
			}
			else
			{
				editMode = false;
			}
			bool flag = false;
			Rect outRect = rightOutRect.ContractedBy(10f);
			Rect rect2 = new Rect(0f, 0f, rightViewWidth, rightViewHeight);
			rect2.ContractedBy(10f);
			rect2.width = rightViewWidth;
			Rect rect3 = rect2.ContractedBy(10f);
			Vector2 start = default(Vector2);
			Vector2 end = default(Vector2);
			scrollPositioner.ClearInterestRects();
			Widgets.ScrollHorizontal(outRect, ref rightScrollPosition, rect2);
			Widgets.BeginScrollView(outRect, ref rightScrollPosition, rect2);
			Widgets.BeginGroup(rect3);
			List<ResearchProjectDef> visibleResearchProjects = VisibleResearchProjects;
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < visibleResearchProjects.Count; j++)
				{
					ResearchProjectDef researchProjectDef = visibleResearchProjects[j];
					if (researchProjectDef.tab != CurTab)
					{
						continue;
					}
					start.x = PosX(researchProjectDef);
					start.y = PosY(researchProjectDef) + 25f;
					for (int k = 0; k < researchProjectDef.prerequisites.CountAllowNull(); k++)
					{
						ResearchProjectDef researchProjectDef2 = researchProjectDef.prerequisites[k];
						if (researchProjectDef2 == null || researchProjectDef2.tab != CurTab)
						{
							continue;
						}
						end.x = PosX(researchProjectDef2) + 140f;
						end.y = PosY(researchProjectDef2) + 25f;
						if (selectedProject == researchProjectDef || selectedProject == researchProjectDef2)
						{
							if (i == 1)
							{
								Widgets.DrawLine(start, end, TexUI.HighlightLineResearchColor, 4f);
							}
						}
						else if (i == 0)
						{
							Widgets.DrawLine(start, end, TexUI.DefaultLineResearchColor, 2f);
						}
					}
				}
			}
			Rect other = new Rect(rightScrollPosition.x, rightScrollPosition.y, outRect.width, outRect.height).ExpandedBy(10f);
			for (int l = 0; l < visibleResearchProjects.Count; l++)
			{
				ResearchProjectDef researchProjectDef3 = visibleResearchProjects[l];
				if (researchProjectDef3.tab != CurTab)
				{
					continue;
				}
				Rect rect4 = new Rect(PosX(researchProjectDef3), PosY(researchProjectDef3), 140f, 50f);
				Rect rect5 = new Rect(rect4);
				bool flag2 = quickSearchWidget.filter.Active && !matchingProjects.Contains(researchProjectDef3);
				bool flag3 = quickSearchWidget.filter.Active && matchingProjects.Contains(researchProjectDef3);
				if (flag3 || selectedProject == researchProjectDef3)
				{
					scrollPositioner.RegisterInterestRect(rect4);
				}
				string label = GetLabel(researchProjectDef3);
				Widgets.LabelCacheHeight(ref rect5, GetLabelWithNewlineCached(label));
				if (!rect5.Overlaps(other))
				{
					continue;
				}
				Color color = Widgets.NormalOptionColor;
				Color color2 = default(Color);
				Color color3 = default(Color);
				bool flag4 = !researchProjectDef3.IsFinished && !researchProjectDef3.CanStartNow;
				if (researchProjectDef3 == Find.ResearchManager.currentProj)
				{
					color2 = TexUI.ActiveResearchColor;
				}
				else if (researchProjectDef3.IsFinished)
				{
					color2 = TexUI.FinishedResearchColor;
				}
				else if (flag4)
				{
					color2 = TexUI.LockedResearchColor;
				}
				else if (researchProjectDef3.CanStartNow)
				{
					color2 = TexUI.AvailResearchColor;
				}
				if (editMode && draggingTabs.Contains(researchProjectDef3))
				{
					color3 = Color.yellow;
				}
				else if (selectedProject == researchProjectDef3)
				{
					color2 += TexUI.HighlightBgResearchColor;
					color3 = TexUI.HighlightBorderResearchColor;
				}
				else
				{
					color3 = TexUI.DefaultBorderResearchColor;
				}
				if (flag4)
				{
					color = ProjectWithMissingPrerequisiteLabelColor;
				}
				if (selectedProject != null)
				{
					if ((researchProjectDef3.prerequisites != null && researchProjectDef3.prerequisites.Contains(selectedProject)) || (researchProjectDef3.hiddenPrerequisites != null && researchProjectDef3.hiddenPrerequisites.Contains(selectedProject)))
					{
						color3 = TexUI.HighlightLineResearchColor;
					}
					if (!researchProjectDef3.IsFinished && ((selectedProject.prerequisites != null && selectedProject.prerequisites.Contains(researchProjectDef3)) || (selectedProject.hiddenPrerequisites != null && selectedProject.hiddenPrerequisites.Contains(researchProjectDef3))))
					{
						color3 = TexUI.DependencyOutlineResearchColor;
					}
				}
				if (requiredByThisFound)
				{
					for (int m = 0; m < researchProjectDef3.requiredByThis.CountAllowNull(); m++)
					{
						ResearchProjectDef researchProjectDef4 = researchProjectDef3.requiredByThis[m];
						if (selectedProject == researchProjectDef4)
						{
							color3 = TexUI.HighlightLineResearchColor;
						}
					}
				}
				Color color4 = (researchProjectDef3.TechprintRequirementMet ? FulfilledPrerequisiteColor : MissingPrerequisiteColor);
				Color color5 = (researchProjectDef3.StudiedThingsRequirementsMet ? FulfilledPrerequisiteColor : MissingPrerequisiteColor);
				if (flag2)
				{
					color = NoMatchTint(color);
					color2 = NoMatchTint(color2);
					color3 = NoMatchTint(color3);
					color4 = NoMatchTint(color4);
					color5 = NoMatchTint(color5);
				}
				Rect rect6 = rect5;
				Widgets.LabelCacheHeight(ref rect6, " ");
				if (flag3)
				{
					Widgets.DrawStrongHighlight(rect5.ExpandedBy(12f));
				}
				if (Widgets.CustomButtonText(ref rect5, "", color2, color, color3))
				{
					SoundDefOf.Click.PlayOneShotOnCamera();
					selectedProject = researchProjectDef3;
				}
				rect6.y = rect5.y + rect5.height - rect6.height;
				Rect rect7 = rect6;
				rect7.x += 10f;
				rect7.width = rect7.width / 2f - 10f;
				Color color6 = GUI.color;
				TextAnchor anchor = Text.Anchor;
				GUI.color = color;
				Text.Anchor = TextAnchor.UpperCenter;
				Widgets.Label(rect5, label);
				GUI.color = color;
				Text.Anchor = TextAnchor.MiddleLeft;
				Widgets.Label(rect7, researchProjectDef3.CostApparent.ToString());
				float num = rect4.xMax;
				if (researchProjectDef3.TechprintCount > 0)
				{
					string text = GetTechprintsInfoCached(researchProjectDef3.TechprintsApplied, researchProjectDef3.TechprintCount);
					Vector2 vector = Text.CalcSize(text);
					num -= vector.x + 10f;
					Rect rect8 = new Rect(num, rect7.y, vector.x, rect7.height);
					GUI.color = color4;
					Text.Anchor = TextAnchor.MiddleRight;
					Widgets.Label(rect8, text);
					num -= rect7.height;
					GUI.color = Color.white;
					GUI.DrawTexture(new Rect(num, rect7.y, rect7.height, rect7.height).ContractedBy(4f), TechprintRequirementTex.Texture);
					GUI.color = color6;
				}
				if (researchProjectDef3.RequiredStudiedThingCount > 0)
				{
					string text2 = GetTechprintsInfoCached(researchProjectDef3.StudiedThingsCompleted, researchProjectDef3.RequiredStudiedThingCount);
					Vector2 vector2 = Text.CalcSize(text2);
					num -= vector2.x + 10f;
					Rect rect9 = new Rect(num, rect7.y, vector2.x, rect7.height);
					GUI.color = color5;
					Text.Anchor = TextAnchor.MiddleRight;
					Widgets.Label(rect9, text2);
					num -= rect7.height;
					GUI.color = Color.white;
					GUI.DrawTexture(new Rect(num, rect7.y, rect7.height, rect7.height).ContractedBy(4f), StudyRequirementTex.Texture);
					GUI.color = color6;
				}
				GUI.color = GetSourceColor(researchProjectDef3);
				GUI.DrawTexture(new Rect(rect5.xMax - 7f, rect5.y, 7f, 7f), TopCornerTex);
				GUI.color = color6;
				Text.Anchor = anchor;
				if (Mouse.IsOver(rect5) && !editMode)
				{
					Widgets.DrawLightHighlight(rect5);
					TooltipHandler.TipRegion(rect5, researchProjectDef3.GetTip());
				}
				if (!editMode || !Mouse.IsOver(rect5) || !Input.GetMouseButtonDown(0))
				{
					continue;
				}
				flag = true;
				if (Input.GetKey(KeyCode.LeftShift))
				{
					if (!draggingTabs.Contains(researchProjectDef3))
					{
						draggingTabs.Add(researchProjectDef3);
					}
				}
				else if (!Input.GetKey(KeyCode.LeftControl) && !draggingTabs.Contains(researchProjectDef3))
				{
					draggingTabs.Clear();
					draggingTabs.Add(researchProjectDef3);
				}
				if (Input.GetKey(KeyCode.LeftControl) && draggingTabs.Contains(researchProjectDef3))
				{
					draggingTabs.Remove(researchProjectDef3);
				}
			}
			Widgets.EndGroup();
			Widgets.EndScrollView();
			scrollPositioner.ScrollHorizontally(ref rightScrollPosition, outRect.size);
			if (!editMode)
			{
				return;
			}
			if (!flag && Input.GetMouseButtonDown(0))
			{
				draggingTabs.Clear();
			}
			if (draggingTabs.NullOrEmpty())
			{
				return;
			}
			if (Input.GetMouseButtonUp(0))
			{
				for (int n = 0; n < draggingTabs.Count; n++)
				{
					draggingTabs[n].Debug_SnapPositionData();
				}
			}
			else if (Input.GetMouseButton(0) && !Input.GetMouseButtonDown(0) && Event.current.type == EventType.Layout)
			{
				for (int num2 = 0; num2 < draggingTabs.Count; num2++)
				{
					draggingTabs[num2].Debug_ApplyPositionDelta(new Vector2(PixelsToCoordX(Event.current.delta.x), PixelsToCoordY(Event.current.delta.y)));
				}
			}
		}

		private Color GetSourceColor(ResearchProjectDef project)
		{
			ModContentPack modContentPack = project.modContentPack;
			if (modContentPack == null)
			{
				return SourceColor_Mod;
			}
			if (modContentPack.IsOfficialMod)
			{
				ExpansionDef expansionWithIdentifier = ModLister.GetExpansionWithIdentifier(modContentPack.PackageId.ToLower());
				if (expansionWithIdentifier != null && !expansionWithIdentifier.isCore)
				{
					return expansionWithIdentifier.primaryColor;
				}
				return Color.clear;
			}
			return SourceColor_Mod;
		}

		private Color NoMatchTint(Color color)
		{
			return Color.Lerp(color, NoMatchTintColor, 0.4f);
		}

		private float DrawResearchPrereqs(ResearchProjectDef project, Rect rect)
		{
			if (project.prerequisites.NullOrEmpty())
			{
				return 0f;
			}
			float xMin = rect.xMin;
			float yMin = rect.yMin;
			Widgets.LabelCacheHeight(ref rect, "ResearchPrerequisites".Translate() + ":");
			rect.yMin += rect.height;
			rect.xMin += 6f;
			for (int i = 0; i < project.prerequisites.Count; i++)
			{
				SetPrerequisiteStatusColor(project.prerequisites[i].IsFinished, project);
				Widgets.LabelCacheHeight(ref rect, project.prerequisites[i].LabelCap);
				rect.yMin += rect.height;
			}
			if (project.hiddenPrerequisites != null)
			{
				for (int j = 0; j < project.hiddenPrerequisites.Count; j++)
				{
					SetPrerequisiteStatusColor(project.hiddenPrerequisites[j].IsFinished, project);
					Widgets.LabelCacheHeight(ref rect, project.hiddenPrerequisites[j].LabelCap);
					rect.yMin += rect.height;
				}
			}
			GUI.color = Color.white;
			rect.xMin = xMin;
			return rect.yMin - yMin;
		}

		private string GetLabelWithNewlineCached(string label)
		{
			if (!labelsWithNewlineCached.ContainsKey(label))
			{
				labelsWithNewlineCached.Add(label, label + "\n");
			}
			return labelsWithNewlineCached[label];
		}

		private string GetTechprintsInfoCached(int applied, int total)
		{
			Pair<int, int> key = new Pair<int, int>(applied, total);
			if (!techprintsInfoCached.ContainsKey(key))
			{
				techprintsInfoCached.Add(key, $"{applied.ToString()} / {total.ToString()}");
			}
			return techprintsInfoCached[key];
		}

		private float DrawResearchBenchRequirements(ResearchProjectDef project, Rect rect)
		{
			float xMin = rect.xMin;
			float yMin = rect.yMin;
			if (project.requiredResearchBuilding != null)
			{
				bool present = false;
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					if (maps[i].listerBuildings.allBuildingsColonist.Find((Building x) => x.def == project.requiredResearchBuilding) != null)
					{
						present = true;
						break;
					}
				}
				Widgets.LabelCacheHeight(ref rect, "RequiredResearchBench".Translate() + ":");
				rect.xMin += 6f;
				rect.yMin += rect.height;
				SetPrerequisiteStatusColor(present, project);
				rect.height = Text.CalcHeight(project.requiredResearchBuilding.LabelCap, rect.width - 24f - 6f);
				Widgets.HyperlinkWithIcon(rect, new Dialog_InfoCard.Hyperlink(project.requiredResearchBuilding));
				rect.yMin += rect.height + 4f;
				GUI.color = Color.white;
				rect.xMin = xMin;
			}
			if (!project.requiredResearchFacilities.NullOrEmpty())
			{
				Widgets.LabelCacheHeight(ref rect, "RequiredResearchBenchFacilities".Translate() + ":");
				rect.yMin += rect.height;
				Building_ResearchBench building_ResearchBench = FindBenchFulfillingMostRequirements(project.requiredResearchBuilding, project.requiredResearchFacilities);
				CompAffectedByFacilities bestMatchingBench = null;
				if (building_ResearchBench != null)
				{
					bestMatchingBench = building_ResearchBench.TryGetComp<CompAffectedByFacilities>();
				}
				rect.xMin += 6f;
				for (int j = 0; j < project.requiredResearchFacilities.Count; j++)
				{
					DrawResearchBenchFacilityRequirement(project.requiredResearchFacilities[j], bestMatchingBench, project, ref rect);
					rect.yMin += rect.height;
				}
				rect.yMin += 4f;
			}
			GUI.color = Color.white;
			rect.xMin = xMin;
			return rect.yMin - yMin;
		}

		private float DrawStudyRequirements(ResearchProjectDef project, Rect rect)
		{
			_ = rect.xMin;
			float yMin = rect.yMin;
			if (project.RequiredStudiedThingCount > 0)
			{
				Widgets.LabelCacheHeight(ref rect, "StudyRequirements".Translate() + ":");
				rect.xMin += 6f;
				rect.yMin += rect.height;
				foreach (ThingDef item in project.requiredStudied)
				{
					Rect rect2 = new Rect(rect.x, rect.yMin, rect.width, 24f);
					Color? color = null;
					if (quickSearchWidget.filter.Active)
					{
						if (MatchesUnlockedDef(item))
						{
							Widgets.DrawTextHighlight(rect2);
						}
						else
						{
							color = NoMatchTint(Widgets.NormalOptionColor);
						}
					}
					Dialog_InfoCard.Hyperlink hyperlink = new Dialog_InfoCard.Hyperlink(item);
					Widgets.HyperlinkWithIcon(rect2, hyperlink, null, 2f, 6f, color, truncateLabel: false, LabelSuffixForUnlocked(item));
					rect.yMin += 24f;
				}
			}
			return rect.yMin - yMin;
		}

		private float DrawUnlockableHyperlinks(Rect rect, ResearchProjectDef project)
		{
			List<Pair<ResearchPrerequisitesUtility.UnlockedHeader, List<Def>>> list = UnlockedDefsGroupedByPrerequisites(project);
			if (list.NullOrEmpty())
			{
				return 0f;
			}
			float yMin = rect.yMin;
			float x = rect.x;
			foreach (Pair<ResearchPrerequisitesUtility.UnlockedHeader, List<Def>> item in list)
			{
				ResearchPrerequisitesUtility.UnlockedHeader first = item.First;
				rect.x = x;
				if (!first.unlockedBy.Any())
				{
					Widgets.LabelCacheHeight(ref rect, "Unlocks".Translate() + ":");
				}
				else
				{
					Widgets.LabelCacheHeight(ref rect, string.Concat("UnlockedWith".Translate(), " ", HeaderLabel(first), ":"));
				}
				rect.x += 6f;
				rect.yMin += rect.height;
				foreach (Def item2 in item.Second)
				{
					Rect rect2 = new Rect(rect.x, rect.yMin, rect.width, 24f);
					Color? color = null;
					if (quickSearchWidget.filter.Active)
					{
						if (MatchesUnlockedDef(item2))
						{
							Widgets.DrawTextHighlight(rect2);
						}
						else
						{
							color = NoMatchTint(Widgets.NormalOptionColor);
						}
					}
					Dialog_InfoCard.Hyperlink hyperlink = new Dialog_InfoCard.Hyperlink(item2);
					Widgets.HyperlinkWithIcon(rect2, hyperlink, null, 2f, 6f, color, truncateLabel: false, LabelSuffixForUnlocked(item2));
					rect.yMin += 24f;
				}
			}
			return rect.yMin - yMin;
		}

		private float DrawContentSource(Rect rect, ResearchProjectDef project)
		{
			if (project.modContentPack == null || project.modContentPack.IsCoreMod)
			{
				return 0f;
			}
			float yMin = rect.yMin;
			TaggedString taggedString = "Stat_Source_Label".Translate() + ":  " + project.modContentPack.Name;
			Widgets.LabelCacheHeight(ref rect, taggedString.Colorize(Color.grey));
			ExpansionDef expansionDef = ModLister.AllExpansions.Find((ExpansionDef e) => e.linkedMod == project.modContentPack.PackageId);
			if (expansionDef != null)
			{
				GUI.DrawTexture(new Rect(Text.CalcSize(taggedString).x + 4f, rect.y, 20f, 20f), expansionDef.IconFromStatus);
			}
			return rect.yMax - yMin;
		}

		private string LabelSuffixForUnlocked(Def unlocked)
		{
			if (!ModLister.IdeologyInstalled)
			{
				return null;
			}
			tmpSuffixesForUnlocked.Clear();
			foreach (MemeDef allDef in DefDatabase<MemeDef>.AllDefs)
			{
				if (allDef.AllDesignatorBuildables.Contains(unlocked))
				{
					tmpSuffixesForUnlocked.AddDistinct(allDef.LabelCap);
				}
				if (allDef.thingStyleCategories.NullOrEmpty())
				{
					continue;
				}
				foreach (ThingStyleCategoryWithPriority thingStyleCategory in allDef.thingStyleCategories)
				{
					if (thingStyleCategory.category.AllDesignatorBuildables.Contains(unlocked))
					{
						tmpSuffixesForUnlocked.AddDistinct(allDef.LabelCap);
					}
				}
			}
			foreach (CultureDef allDef2 in DefDatabase<CultureDef>.AllDefs)
			{
				if (allDef2.thingStyleCategories.NullOrEmpty())
				{
					continue;
				}
				foreach (ThingStyleCategoryWithPriority thingStyleCategory2 in allDef2.thingStyleCategories)
				{
					if (thingStyleCategory2.category.AllDesignatorBuildables.Contains(unlocked))
					{
						tmpSuffixesForUnlocked.AddDistinct(allDef2.LabelCap);
					}
				}
			}
			if (!tmpSuffixesForUnlocked.Any())
			{
				return null;
			}
			return " (" + tmpSuffixesForUnlocked.ToCommaList() + ")";
		}

		private List<Pair<ResearchPrerequisitesUtility.UnlockedHeader, List<Def>>> UnlockedDefsGroupedByPrerequisites(ResearchProjectDef project)
		{
			if (cachedUnlockedDefsGroupedByPrerequisites == null)
			{
				cachedUnlockedDefsGroupedByPrerequisites = new Dictionary<ResearchProjectDef, List<Pair<ResearchPrerequisitesUtility.UnlockedHeader, List<Def>>>>();
			}
			if (!cachedUnlockedDefsGroupedByPrerequisites.TryGetValue(project, out var value))
			{
				value = ResearchPrerequisitesUtility.UnlockedDefsGroupedByPrerequisites(project);
				cachedUnlockedDefsGroupedByPrerequisites.Add(project, value);
			}
			return value;
		}

		private string HeaderLabel(ResearchPrerequisitesUtility.UnlockedHeader headerProject)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string value = "";
			for (int i = 0; i < headerProject.unlockedBy.Count; i++)
			{
				ResearchProjectDef researchProjectDef = headerProject.unlockedBy[i];
				string text = researchProjectDef.LabelCap;
				if (!researchProjectDef.IsFinished)
				{
					text = text.Colorize(MissingPrerequisiteColor);
				}
				stringBuilder.Append(text).Append(value);
				value = ", ";
			}
			return stringBuilder.ToString();
		}

		private float DrawTechprintInfo(Rect rect, ResearchProjectDef project)
		{
			if (selectedProject.TechprintCount == 0)
			{
				return 0f;
			}
			float xMin = rect.xMin;
			float yMin = rect.yMin;
			string text = "ResearchTechprintsFromFactions".Translate();
			float num = Text.CalcHeight(text, rect.width);
			Widgets.Label(new Rect(rect.x, yMin, rect.width, num), text);
			rect.x += 6f;
			if (selectedProject.heldByFactionCategoryTags != null)
			{
				foreach (string heldByFactionCategoryTag in selectedProject.heldByFactionCategoryTags)
				{
					foreach (Faction item in Find.FactionManager.AllFactionsInViewOrder)
					{
						if (item.def.categoryTag == heldByFactionCategoryTag)
						{
							string name = item.Name;
							Rect rect2 = new Rect(rect.x, yMin + num, rect.width, Mathf.Max(24f, Text.CalcHeight(name, rect.width - 24f - 6f)));
							Widgets.BeginGroup(rect2);
							Rect r = new Rect(0f, 0f, 24f, 24f).ContractedBy(2f);
							FactionUIUtility.DrawFactionIconWithTooltip(r, item);
							Rect rect3 = new Rect(r.xMax + 6f, 0f, rect2.width - r.width - 6f, rect2.height);
							Text.Anchor = TextAnchor.MiddleLeft;
							Text.WordWrap = false;
							Widgets.Label(rect3, item.Name);
							Text.Anchor = TextAnchor.UpperLeft;
							Text.WordWrap = true;
							Widgets.EndGroup();
							num += rect2.height;
						}
					}
				}
			}
			rect.xMin = xMin;
			return num;
		}

		private string GetLabel(ResearchProjectDef r)
		{
			return r.LabelCap;
		}

		private void SetPrerequisiteStatusColor(bool present, ResearchProjectDef project)
		{
			if (!project.IsFinished)
			{
				if (present)
				{
					GUI.color = FulfilledPrerequisiteColor;
				}
				else
				{
					GUI.color = MissingPrerequisiteColor;
				}
			}
		}

		private void DrawResearchBenchFacilityRequirement(ThingDef requiredFacility, CompAffectedByFacilities bestMatchingBench, ResearchProjectDef project, ref Rect rect)
		{
			Thing thing = null;
			Thing thing2 = null;
			if (bestMatchingBench != null)
			{
				thing = bestMatchingBench.LinkedFacilitiesListForReading.Find((Thing x) => x.def == requiredFacility);
				thing2 = bestMatchingBench.LinkedFacilitiesListForReading.Find((Thing x) => x.def == requiredFacility && bestMatchingBench.IsFacilityActive(x));
			}
			SetPrerequisiteStatusColor(thing2 != null, project);
			string text = requiredFacility.LabelCap;
			if (thing != null && thing2 == null)
			{
				text += " (" + "InactiveFacility".Translate() + ")";
			}
			rect.height = Text.CalcHeight(text, rect.width - 24f - 6f);
			Widgets.HyperlinkWithIcon(rect, new Dialog_InfoCard.Hyperlink(requiredFacility), text);
		}

		private Building_ResearchBench FindBenchFulfillingMostRequirements(ThingDef requiredResearchBench, List<ThingDef> requiredFacilities)
		{
			tmpAllBuildings.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				tmpAllBuildings.AddRange(maps[i].listerBuildings.allBuildingsColonist);
			}
			float num = 0f;
			Building_ResearchBench building_ResearchBench = null;
			for (int j = 0; j < tmpAllBuildings.Count; j++)
			{
				if (tmpAllBuildings[j] is Building_ResearchBench building_ResearchBench2 && (requiredResearchBench == null || building_ResearchBench2.def == requiredResearchBench))
				{
					float researchBenchRequirementsScore = GetResearchBenchRequirementsScore(building_ResearchBench2, requiredFacilities);
					if (building_ResearchBench == null || researchBenchRequirementsScore > num)
					{
						num = researchBenchRequirementsScore;
						building_ResearchBench = building_ResearchBench2;
					}
				}
			}
			tmpAllBuildings.Clear();
			return building_ResearchBench;
		}

		private float GetResearchBenchRequirementsScore(Building_ResearchBench bench, List<ThingDef> requiredFacilities)
		{
			float num = 0f;
			for (int i = 0; i < requiredFacilities.Count; i++)
			{
				CompAffectedByFacilities benchComp = bench.GetComp<CompAffectedByFacilities>();
				if (benchComp != null)
				{
					List<Thing> linkedFacilitiesListForReading = benchComp.LinkedFacilitiesListForReading;
					if (linkedFacilitiesListForReading.Find((Thing x) => x.def == requiredFacilities[i] && benchComp.IsFacilityActive(x)) != null)
					{
						num += 1f;
					}
					else if (linkedFacilitiesListForReading.Find((Thing x) => x.def == requiredFacilities[i]) != null)
					{
						num += 0.6f;
					}
				}
			}
			return num;
		}

		private void UpdateSearchResults()
		{
			quickSearchWidget.noResultsMatched = false;
			matchingProjects.Clear();
			foreach (ResearchTabRecord tab2 in tabs)
			{
				tab2.Reset();
			}
			if (!quickSearchWidget.filter.Active)
			{
				return;
			}
			foreach (ResearchProjectDef visibleResearchProject in VisibleResearchProjects)
			{
				if (quickSearchWidget.filter.Matches(GetLabel(visibleResearchProject)) || MatchesUnlockedDefs(visibleResearchProject))
				{
					matchingProjects.Add(visibleResearchProject);
				}
			}
			quickSearchWidget.noResultsMatched = !matchingProjects.Any();
			foreach (ResearchTabRecord tab in tabs)
			{
				tab.firstMatch = (from p in matchingProjects
					where tab.def == p.tab
					orderby p.ResearchViewX
					select p).FirstOrDefault();
				if (!tab.AnyMatches)
				{
					tab.labelColor = Color.grey;
				}
			}
			if (!CurTabRecord.AnyMatches)
			{
				foreach (ResearchTabRecord tab3 in tabs)
				{
					if (tab3.AnyMatches)
					{
						CurTab = tab3.def;
						break;
					}
				}
			}
			scrollPositioner.Arm();
			if (CurTabRecord.firstMatch != null)
			{
				selectedProject = CurTabRecord.firstMatch;
			}
			bool MatchesUnlockedDefs(ResearchProjectDef proj)
			{
				foreach (Pair<ResearchPrerequisitesUtility.UnlockedHeader, List<Def>> item in UnlockedDefsGroupedByPrerequisites(proj))
				{
					foreach (Def item2 in item.Second)
					{
						if (MatchesUnlockedDef(item2))
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		private bool MatchesUnlockedDef(Def unlocked)
		{
			return quickSearchWidget.filter.Matches(unlocked.label);
		}

		public override void Notify_ClickOutsideWindow()
		{
			base.Notify_ClickOutsideWindow();
			quickSearchWidget.Unfocus();
		}
	}
}
