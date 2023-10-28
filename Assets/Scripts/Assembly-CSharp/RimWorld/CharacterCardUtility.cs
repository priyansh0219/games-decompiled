using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class CharacterCardUtility
	{
		private struct LeftRectSection
		{
			public Rect rect;

			public Action<Rect> drawer;

			public float calculatedSize;
		}

		private static Vector2 leftRectScrollPos = Vector2.zero;

		private static bool warnedChangingXenotypeWillRandomizePawn = false;

		private static Rect highlightRect;

		private const float NonArchiteBaselinerChance = 0.5f;

		public const int MainRectsY = 100;

		private const float MainRectsHeight = 355f;

		private const int ConfigRectTitlesHeight = 40;

		private const int FactionIconSize = 22;

		private const int IdeoIconSize = 22;

		private const int GenderIconSize = 22;

		private const float RowHeight = 22f;

		private const float LeftRectHeight = 250f;

		private const float RightRectHeight = 258f;

		public static Vector2 BasePawnCardSize = new Vector2(480f, 455f);

		private static readonly Color FavColorBoxColor = new Color(0.25f, 0.25f, 0.25f);

		public const int MaxNameLength = 12;

		public const int MaxNickLength = 16;

		public const int MaxTitleLength = 25;

		public const int QuestLineHeight = 20;

		public const float RandomizeButtonWidth = 200f;

		public const float HighlightMargin = 6f;

		private static readonly Texture2D QuestIcon = ContentFinder<Texture2D>.Get("UI/Icons/Quest");

		private static readonly Texture2D UnrecruitableIcon = ContentFinder<Texture2D>.Get("UI/Icons/UnwaveringlyLoyal");

		public static readonly Color StackElementBackground = new Color(1f, 1f, 1f, 0.1f);

		public static List<CustomXenotype> cachedCustomXenotypes;

		private static List<ExtraFaction> tmpExtraFactions = new List<ExtraFaction>();

		private static readonly Color TitleCausedWorkTagDisableColor = new Color(0.67f, 0.84f, 0.9f);

		private static List<GenUI.AnonymousStackElement> tmpStackElements = new List<GenUI.AnonymousStackElement>();

		private static StringBuilder tmpInspectStrings = new StringBuilder();

		public static Regex ValidNameRegex = new Regex("^[\\p{L}0-9 '\\-.]*$");

		private const int QuestIconSize = 24;

		private const int QuestIconExtraPaddingLeft = -7;

		private static List<CustomXenotype> CustomXenotypes
		{
			get
			{
				if (cachedCustomXenotypes == null)
				{
					cachedCustomXenotypes = new List<CustomXenotype>();
					foreach (FileInfo item in GenFilePaths.AllCustomXenotypeFiles.OrderBy((FileInfo f) => f.LastWriteTime))
					{
						string filePath = GenFilePaths.AbsFilePathForXenotype(Path.GetFileNameWithoutExtension(item.Name));
						PreLoadUtility.CheckVersionAndLoad(filePath, ScribeMetaHeaderUtility.ScribeHeaderMode.Xenotype, delegate
						{
							if (GameDataSaveLoader.TryLoadXenotype(filePath, out var xenotype))
							{
								cachedCustomXenotypes.Add(xenotype);
							}
						}, skipOnMismatch: true);
					}
				}
				return cachedCustomXenotypes;
			}
		}

		public static List<CustomXenotype> CustomXenotypesForReading => CustomXenotypes;

		public static void DrawCharacterCard(Rect rect, Pawn pawn, Action randomizeCallback = null, Rect creationRect = default(Rect), bool showName = true)
		{
			bool flag = randomizeCallback != null;
			Rect rect2 = (flag ? creationRect : rect);
			Widgets.BeginGroup(rect2);
			Rect rect3 = new Rect(0f, 0f, 300f, showName ? 30 : 0);
			if (showName)
			{
				NameTriple nameTriple = pawn.Name as NameTriple;
				if (flag && nameTriple != null)
				{
					Rect rect4 = new Rect(rect3);
					rect4.width *= 0.333f;
					Rect rect5 = new Rect(rect3);
					rect5.width *= 0.333f;
					rect5.x += rect5.width;
					Rect rect6 = new Rect(rect3);
					rect6.width *= 0.333f;
					rect6.x += rect5.width * 2f;
					string name = nameTriple.First;
					string name2 = nameTriple.Nick;
					string name3 = nameTriple.Last;
					DoNameInputRect(rect4, ref name, 12);
					if (nameTriple.Nick == nameTriple.First || nameTriple.Nick == nameTriple.Last)
					{
						GUI.color = new Color(1f, 1f, 1f, 0.5f);
					}
					DoNameInputRect(rect5, ref name2, 16);
					GUI.color = Color.white;
					DoNameInputRect(rect6, ref name3, 12);
					if (nameTriple.First != name || nameTriple.Nick != name2 || nameTriple.Last != name3)
					{
						pawn.Name = new NameTriple(name, string.IsNullOrEmpty(name2) ? name : name2, name3);
					}
					TooltipHandler.TipRegionByKey(rect4, "FirstNameDesc");
					TooltipHandler.TipRegionByKey(rect5, "ShortIdentifierDesc");
					TooltipHandler.TipRegionByKey(rect6, "LastNameDesc");
				}
				else
				{
					rect3.width = 999f;
					Text.Font = GameFont.Medium;
					string text = pawn.Name.ToStringFull.CapitalizeFirst();
					Widgets.Label(rect3, text);
					if (pawn.guilt != null && pawn.guilt.IsGuilty)
					{
						float x = Text.CalcSize(text).x;
						Rect rect7 = new Rect(x + 10f, 0f, 32f, 32f);
						GUI.DrawTexture(rect7, TexUI.GuiltyTex);
						TooltipHandler.TipRegion(rect7, () => pawn.guilt.Tip, 6321623);
					}
					Text.Font = GameFont.Small;
				}
			}
			bool allowsChildSelection = ScenarioUtility.AllowsChildSelection(Find.Scenario);
			if (ModsConfig.BiotechActive && flag)
			{
				Widgets.DrawHighlight(highlightRect.ExpandedBy(6f));
			}
			if (flag)
			{
				Rect rect8 = new Rect(creationRect.width - 200f - 6f, 6f, 200f, rect3.height);
				if (Widgets.ButtonText(rect8, "Randomize".Translate()))
				{
					SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
					randomizeCallback();
				}
				UIHighlighter.HighlightOpportunity(rect8, "RandomizePawn");
				if (ModsConfig.BiotechActive)
				{
					LifestageAndXenotypeOptions(pawn, rect8, flag, allowsChildSelection, randomizeCallback);
				}
			}
			if (flag)
			{
				Widgets.InfoCardButton(rect3.xMax + 4f, (rect3.height - 24f) / 2f, pawn);
			}
			else if (!pawn.health.Dead)
			{
				float num = PawnCardSize(pawn).x - 85f;
				if (pawn.IsFreeColonist && pawn.Spawned && !pawn.IsQuestLodger() && showName)
				{
					Rect rect9 = new Rect(num, 0f, 30f, 30f);
					if (Mouse.IsOver(rect9))
					{
						TooltipHandler.TipRegion(rect9, PawnBanishUtility.GetBanishButtonTip(pawn));
					}
					if (Widgets.ButtonImage(rect9, TexButton.Banish))
					{
						if (pawn.Downed)
						{
							Messages.Message("MessageCantBanishDownedPawn".Translate(pawn.LabelShort, pawn).AdjustedFor(pawn), pawn, MessageTypeDefOf.RejectInput, historical: false);
						}
						else
						{
							PawnBanishUtility.ShowBanishPawnConfirmationDialog(pawn);
						}
					}
					num -= 40f;
				}
				if ((pawn.IsColonist || DebugSettings.ShowDevGizmos) && showName)
				{
					Rect rect10 = new Rect(num, 0f, 30f, 30f);
					TooltipHandler.TipRegionByKey(rect10, "RenameColonist");
					if (Widgets.ButtonImage(rect10, TexButton.Rename))
					{
						Find.WindowStack.Add(pawn.NamePawnDialog());
					}
					num -= 40f;
				}
				if (pawn.IsFreeColonist && !pawn.IsQuestLodger() && pawn.royalty != null && pawn.royalty.AllTitlesForReading.Count > 0)
				{
					Rect rect11 = new Rect(num, 0f, 30f, 30f);
					TooltipHandler.TipRegionByKey(rect11, "RenounceTitle");
					if (Widgets.ButtonImage(rect11, TexButton.RenounceTitle))
					{
						FloatMenuUtility.MakeMenu(pawn.royalty.AllTitlesForReading, (RoyalTitle title) => "RenounceTitle".Translate() + ": " + "TitleOfFaction".Translate(title.def.GetLabelCapFor(pawn), title.faction.GetCallLabel()), delegate(RoyalTitle title)
						{
							return delegate
							{
								List<FactionPermit> list = pawn.royalty.PermitsFromFaction(title.faction);
								RoyalTitleUtility.FindLostAndGainedPermits(title.def, null, out var _, out var lostPermits);
								StringBuilder stringBuilder = new StringBuilder();
								if (lostPermits.Count > 0 || list.Count > 0)
								{
									stringBuilder.AppendLine("RenounceTitleWillLoosePermits".Translate(pawn.Named("PAWN")) + ":");
									foreach (RoyalTitlePermitDef item in lostPermits)
									{
										stringBuilder.AppendLine("- " + item.LabelCap + " (" + FirstTitleWithPermit(item).GetLabelFor(pawn) + ")");
									}
									foreach (FactionPermit item2 in list)
									{
										stringBuilder.AppendLine("- " + item2.Permit.LabelCap + " (" + item2.Title.GetLabelFor(pawn) + ")");
									}
									stringBuilder.AppendLine();
								}
								int permitPoints = pawn.royalty.GetPermitPoints(title.faction);
								if (permitPoints > 0)
								{
									stringBuilder.AppendLineTagged("RenounceTitleWillLosePermitPoints".Translate(pawn.Named("PAWN"), permitPoints.Named("POINTS"), title.faction.Named("FACTION")));
								}
								if (pawn.abilities.abilities.Any())
								{
									stringBuilder.AppendLine();
									stringBuilder.AppendLineTagged("RenounceTitleWillKeepPsylinkLevels".Translate(pawn.Named("PAWN")));
								}
								if (!title.faction.def.renounceTitleMessage.NullOrEmpty())
								{
									stringBuilder.AppendLine();
									stringBuilder.AppendLine(title.faction.def.renounceTitleMessage);
								}
								Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("RenounceTitleDescription".Translate(pawn.Named("PAWN"), "TitleOfFaction".Translate(title.def.GetLabelCapFor(pawn), title.faction.GetCallLabel()).Named("TITLE"), stringBuilder.ToString().TrimEndNewlines().Named("EFFECTS")), delegate
								{
									pawn.royalty.SetTitle(title.faction, null, grantRewards: false);
									pawn.royalty.ResetPermitsAndPoints(title.faction, title.def);
								}, destructive: true));
							};
							RoyalTitleDef FirstTitleWithPermit(RoyalTitlePermitDef permitDef)
							{
								return title.faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.First((RoyalTitleDef t) => t.permits != null && t.permits.Contains(permitDef));
							}
						});
					}
					num -= 40f;
				}
				if (pawn.guilt != null && pawn.guilt.IsGuilty && pawn.IsFreeColonist && !pawn.IsQuestLodger())
				{
					Rect rect12 = new Rect(num + 5f, 0f, 30f, 30f);
					TooltipHandler.TipRegionByKey(rect12, "ExecuteColonist");
					if (Widgets.ButtonImage(rect12, TexButton.ExecuteColonist))
					{
						pawn.guilt.awaitingExecution = !pawn.guilt.awaitingExecution;
						if (pawn.guilt.awaitingExecution)
						{
							Messages.Message("MessageColonistMarkedForExecution".Translate(pawn), pawn, MessageTypeDefOf.SilentInput, historical: false);
						}
					}
					if (pawn.guilt.awaitingExecution)
					{
						Rect position = default(Rect);
						position.x += rect12.x + 22f;
						position.width = 15f;
						position.height = 15f;
						GUI.DrawTexture(position, Widgets.CheckboxOnTex);
					}
				}
			}
			float num2 = rect3.height + 10f;
			float num3 = num2;
			num2 = DoTopStack(pawn, rect, flag, num2);
			if (num2 - num3 < 78f)
			{
				num2 += 15f;
			}
			Rect leftRect = new Rect(0f, num2, 250f, rect2.height - num2);
			DoLeftSection(rect, leftRect, pawn);
			Rect rect13 = new Rect(leftRect.xMax, num2, 258f, rect2.height - num2);
			Widgets.BeginGroup(rect13);
			SkillUI.DrawSkillsOf(mode: (Current.ProgramState != ProgramState.Playing) ? SkillUI.SkillDrawMode.Menu : SkillUI.SkillDrawMode.Gameplay, p: pawn, offset: Vector2.zero, container: rect13);
			Widgets.EndGroup();
			Widgets.EndGroup();
		}

		private static string GetTitleTipString(Pawn pawn, Faction faction, RoyalTitle title, int favor)
		{
			RoyalTitleDef def = title.def;
			TaggedString taggedString = "RoyalTitleTooltipHasTitle".Translate(pawn.Named("PAWN"), faction.Named("FACTION"), def.GetLabelCapFor(pawn).Named("TITLE"));
			taggedString += "\n\n" + faction.def.royalFavorLabel.CapitalizeFirst() + ": " + favor;
			RoyalTitleDef nextTitle = def.GetNextTitle(faction);
			if (nextTitle != null)
			{
				taggedString += "\n" + "RoyalTitleTooltipNextTitle".Translate() + ": " + nextTitle.GetLabelCapFor(pawn) + " (" + "RoyalTitleTooltipNextTitleFavorCost".Translate(nextTitle.favorCost.ToString(), faction.Named("FACTION")) + ")";
			}
			else
			{
				taggedString += "\n" + "RoyalTitleTooltipFinalTitle".Translate();
			}
			if (title.def.canBeInherited)
			{
				Pawn heir = pawn.royalty.GetHeir(faction);
				if (heir != null)
				{
					taggedString += "\n\n" + "RoyalTitleTooltipInheritance".Translate(pawn.Named("PAWN"), heir.Named("HEIR"));
					if (heir.Faction == null)
					{
						taggedString += " " + "RoyalTitleTooltipHeirNoFaction".Translate(heir.Named("HEIR"));
					}
					else if (heir.Faction != faction)
					{
						taggedString += " " + "RoyalTitleTooltipHeirDifferentFaction".Translate(heir.Named("HEIR"), heir.Faction.Named("FACTION"));
					}
				}
				else
				{
					taggedString += "\n\n" + "RoyalTitleTooltipNoHeir".Translate(pawn.Named("PAWN"));
				}
			}
			else
			{
				taggedString += "\n\n" + "LetterRoyalTitleCantBeInherited".Translate(title.def.Named("TITLE")).CapitalizeFirst() + " " + "LetterRoyalTitleNoHeir".Translate(pawn.Named("PAWN"));
			}
			taggedString += "\n\n" + (title.conceited ? "RoyalTitleTooltipConceited" : "RoyalTitleTooltipNonConceited").Translate(pawn.Named("PAWN"));
			taggedString += "\n\n" + RoyalTitleUtility.GetTitleProgressionInfo(faction, pawn);
			return (taggedString + ("\n\n" + "ClickToLearnMore".Translate().Colorize(ColoredText.SubtleGrayColor))).Resolve();
		}

		private static List<object> GetWorkTypeDisableCauses(Pawn pawn, WorkTags workTag)
		{
			List<object> list = new List<object>();
			if (pawn.story != null && pawn.story.Childhood != null && (pawn.story.Childhood.workDisables & workTag) != 0)
			{
				list.Add(pawn.story.Childhood);
			}
			if (pawn.story != null && pawn.story.Adulthood != null && (pawn.story.Adulthood.workDisables & workTag) != 0)
			{
				list.Add(pawn.story.Adulthood);
			}
			if (pawn.health != null && pawn.health.hediffSet != null)
			{
				foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
				{
					HediffStage curStage = hediff.CurStage;
					if (curStage != null && (curStage.disabledWorkTags & workTag) != 0)
					{
						list.Add(hediff);
					}
				}
			}
			if (pawn.story.traits != null)
			{
				for (int i = 0; i < pawn.story.traits.allTraits.Count; i++)
				{
					if (!pawn.story.traits.allTraits[i].Suppressed)
					{
						Trait trait = pawn.story.traits.allTraits[i];
						if ((trait.def.disabledWorkTags & workTag) != 0)
						{
							list.Add(trait);
						}
					}
				}
			}
			if (pawn.royalty != null)
			{
				foreach (RoyalTitle item in pawn.royalty.AllTitlesForReading)
				{
					if (item.conceited && (item.def.disabledWorkTags & workTag) != 0)
					{
						list.Add(item);
					}
				}
			}
			if (ModsConfig.IdeologyActive && pawn.Ideo != null)
			{
				Precept_Role role = pawn.Ideo.GetRole(pawn);
				if (role != null && (role.def.roleDisabledWorkTags & workTag) != 0)
				{
					list.Add(role);
				}
			}
			if (ModsConfig.BiotechActive && pawn.genes != null)
			{
				foreach (Gene item2 in pawn.genes.GenesListForReading)
				{
					if (item2.Active && (item2.def.disabledWorkTags & workTag) != 0)
					{
						list.Add(item2);
					}
				}
			}
			foreach (QuestPart_WorkDisabled item3 in QuestUtility.GetWorkDisabledQuestPart(pawn))
			{
				if ((item3.disabledWorkTags & workTag) != 0 && !list.Contains(item3.quest))
				{
					list.Add(item3.quest);
				}
			}
			return list;
		}

		private static Color GetDisabledWorkTagLabelColor(Pawn pawn, WorkTags workTag)
		{
			foreach (object workTypeDisableCause in GetWorkTypeDisableCauses(pawn, workTag))
			{
				if (workTypeDisableCause is RoyalTitleDef)
				{
					return TitleCausedWorkTagDisableColor;
				}
			}
			return Color.white;
		}

		private static void LifestageAndXenotypeOptions(Pawn pawn, Rect randomizeRect, bool creationMode, bool allowsChildSelection, Action randomizeCallback)
		{
			highlightRect = randomizeRect;
			highlightRect.yMax += randomizeRect.height + Text.LineHeight + 8f;
			int startingPawnIndex = StartingPawnUtility.PawnIndex(pawn);
			float width = (randomizeRect.width - 4f) / 2f;
			float x2 = randomizeRect.x;
			Rect rect = new Rect(x2, randomizeRect.y + randomizeRect.height + 4f, width, randomizeRect.height);
			x2 += rect.width + 4f;
			Text.Anchor = TextAnchor.MiddleCenter;
			Rect rect2 = rect;
			rect2.y += rect.height + 4f;
			rect2.height = Text.LineHeight;
			Widgets.Label(rect2, pawn.DevelopmentalStage.ToString().Translate().CapitalizeFirst());
			Text.Anchor = TextAnchor.UpperLeft;
			Rect rect3 = new Rect(rect.x, rect.y, rect.width, rect2.yMax - rect.yMin);
			if (Mouse.IsOver(rect3))
			{
				Widgets.DrawHighlight(rect3);
				if (Find.WindowStack.FloatMenu == null)
				{
					TaggedString taggedString = GetLabel().CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + "DevelopmentalAgeSelectionDesc".Translate();
					if (!allowsChildSelection)
					{
						taggedString += "\n\n" + "MessageDevelopmentalStageSelectionDisabledByScenario".Translate().Colorize(ColorLibrary.RedReadable);
					}
					TooltipHandler.TipRegion(rect3, taggedString.Resolve());
				}
			}
			if (Widgets.ButtonImageWithBG(rect, GetDevelopmentalStageIcon(), new Vector2(22f, 22f)))
			{
				if (allowsChildSelection)
				{
					int index2 = startingPawnIndex;
					PawnGenerationRequest existing2 = StartingPawnUtility.GetGenerationRequest(index2);
					List<FloatMenuOption> options = new List<FloatMenuOption>
					{
						new FloatMenuOption("Adult".Translate().CapitalizeFirst(), delegate
						{
							if (!existing2.AllowedDevelopmentalStages.Has(DevelopmentalStage.Adult))
							{
								existing2.AllowedDevelopmentalStages = DevelopmentalStage.Adult;
								existing2.AllowDowned = false;
								StartingPawnUtility.SetGenerationRequest(index2, existing2);
								randomizeCallback();
							}
						}, DevelopmentalStageExtensions.AdultTex.Texture, Color.white),
						new FloatMenuOption("Child".Translate().CapitalizeFirst(), delegate
						{
							if (!existing2.AllowedDevelopmentalStages.Has(DevelopmentalStage.Child))
							{
								existing2.AllowedDevelopmentalStages = DevelopmentalStage.Child;
								existing2.AllowDowned = false;
								StartingPawnUtility.SetGenerationRequest(index2, existing2);
								randomizeCallback();
							}
						}, DevelopmentalStageExtensions.ChildTex.Texture, Color.white),
						new FloatMenuOption("Baby".Translate().CapitalizeFirst(), delegate
						{
							if (!existing2.AllowedDevelopmentalStages.Has(DevelopmentalStage.Baby))
							{
								existing2.AllowedDevelopmentalStages = DevelopmentalStage.Baby;
								existing2.AllowDowned = true;
								StartingPawnUtility.SetGenerationRequest(index2, existing2);
								randomizeCallback();
							}
						}, DevelopmentalStageExtensions.BabyTex.Texture, Color.white)
					};
					Find.WindowStack.Add(new FloatMenu(options));
				}
				else
				{
					Messages.Message("MessageDevelopmentalStageSelectionDisabledByScenario".Translate(), null, MessageTypeDefOf.RejectInput, historical: false);
				}
			}
			Rect rect4 = new Rect(x2, randomizeRect.y + randomizeRect.height + 4f, width, randomizeRect.height);
			Text.Anchor = TextAnchor.MiddleCenter;
			Rect rect5 = rect4;
			rect5.y += rect4.height + 4f;
			rect5.height = Text.LineHeight;
			Widgets.Label(rect5, GetXenotypeLabel(startingPawnIndex).Truncate(rect5.width));
			Text.Anchor = TextAnchor.UpperLeft;
			Rect rect6 = new Rect(rect4.x, rect4.y, rect4.width, rect5.yMax - rect4.yMin);
			if (Mouse.IsOver(rect6))
			{
				Widgets.DrawHighlight(rect6);
				if (Find.WindowStack.FloatMenu == null)
				{
					TooltipHandler.TipRegion(rect6, GetXenotypeLabel(startingPawnIndex).Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + "XenotypeSelectionDesc".Translate());
				}
			}
			if (!Widgets.ButtonImageWithBG(rect4, GetXenotypeIcon(startingPawnIndex), new Vector2(22f, 22f)))
			{
				return;
			}
			int index = startingPawnIndex;
			List<FloatMenuOption> list = new List<FloatMenuOption>
			{
				new FloatMenuOption("AnyNonArchite".Translate().CapitalizeFirst(), delegate
				{
					List<XenotypeDef> allowedXenotypes = DefDatabase<XenotypeDef>.AllDefs.Where((XenotypeDef x) => !x.Archite && x != XenotypeDefOf.Baseliner).ToList();
					SetupGenerationRequest(index, null, null, allowedXenotypes, 0.5f, (PawnGenerationRequest existing) => existing.ForcedXenotype != null || existing.ForcedCustomXenotype != null, randomizeCallback, randomize: false);
				}),
				new FloatMenuOption("XenotypeEditor".Translate() + "...", delegate
				{
					Find.WindowStack.Add(new Dialog_CreateXenotype(index, delegate
					{
						cachedCustomXenotypes = null;
						randomizeCallback();
					}));
				})
			};
			foreach (XenotypeDef item in DefDatabase<XenotypeDef>.AllDefs.OrderBy((XenotypeDef x) => 0f - x.displayPriority))
			{
				XenotypeDef xenotype = item;
				list.Add(new FloatMenuOption(xenotype.LabelCap, delegate
				{
					SetupGenerationRequest(index, xenotype, null, null, 0f, (PawnGenerationRequest existing) => existing.ForcedXenotype != xenotype, randomizeCallback);
				}, xenotype.Icon, XenotypeDef.IconColor, MenuOptionPriority.Default, delegate(Rect r)
				{
					TooltipHandler.TipRegion(r, xenotype.descriptionShort ?? xenotype.description);
				}, null, 24f, (Rect r) => Widgets.InfoCardButton(r.x, r.y + 3f, xenotype) ? true : false, null, playSelectionSound: true, 0, HorizontalJustification.Left, extraPartRightJustified: true));
			}
			foreach (CustomXenotype customXenotype in CustomXenotypes)
			{
				CustomXenotype customInner = customXenotype;
				list.Add(new FloatMenuOption(customInner.name.CapitalizeFirst() + " (" + "Custom".Translate() + ")", delegate
				{
					SetupGenerationRequest(index, null, customInner, null, 0f, (PawnGenerationRequest existing) => existing.ForcedCustomXenotype != customInner, randomizeCallback);
				}, customInner.IconDef.Icon, XenotypeDef.IconColor, MenuOptionPriority.Default, null, null, 24f, delegate(Rect r)
				{
					if (Widgets.ButtonImage(new Rect(r.x, r.y + (r.height - r.width) / 2f, r.width, r.width), TexButton.DeleteX, GUI.color))
					{
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDelete".Translate(customInner.name.CapitalizeFirst()), delegate
						{
							string path = GenFilePaths.AbsFilePathForXenotype(customInner.name);
							if (File.Exists(path))
							{
								File.Delete(path);
								cachedCustomXenotypes = null;
							}
						}, destructive: true));
						return true;
					}
					return false;
				}, null, playSelectionSound: true, 0, HorizontalJustification.Left, extraPartRightJustified: true));
			}
			Find.WindowStack.Add(new FloatMenu(list));
			Texture2D GetDevelopmentalStageIcon()
			{
				return StartingPawnUtility.GetGenerationRequest(startingPawnIndex).AllowedDevelopmentalStages.Icon().Texture;
			}
			string GetLabel()
			{
				PawnGenerationRequest generationRequest = StartingPawnUtility.GetGenerationRequest(startingPawnIndex);
				if (generationRequest.AllowedDevelopmentalStages.Has(DevelopmentalStage.Baby))
				{
					return "Baby".Translate();
				}
				if (generationRequest.AllowedDevelopmentalStages.Has(DevelopmentalStage.Child))
				{
					return "Child".Translate();
				}
				return "Adult".Translate();
			}
		}

		private static void SetupGenerationRequest(int index, XenotypeDef xenotype, CustomXenotype customXenotype, List<XenotypeDef> allowedXenotypes, float forceBaselinerChance, Func<PawnGenerationRequest, bool> validator, Action randomizeCallback, bool randomize = true)
		{
			PawnGenerationRequest existing = StartingPawnUtility.GetGenerationRequest(index);
			if (!validator(existing))
			{
				return;
			}
			if (!warnedChangingXenotypeWillRandomizePawn && randomize)
			{
				Find.WindowStack.Add(new Dialog_MessageBox("WarnChangingXenotypeWillRandomizePawn".Translate(), "Yes".Translate(), delegate
				{
					warnedChangingXenotypeWillRandomizePawn = true;
					existing.ForcedXenotype = xenotype;
					existing.ForcedCustomXenotype = customXenotype;
					existing.AllowedXenotypes = allowedXenotypes;
					existing.ForceBaselinerChance = forceBaselinerChance;
					StartingPawnUtility.SetGenerationRequest(index, existing);
					randomizeCallback();
				}, "No".Translate()));
			}
			else
			{
				existing.ForcedXenotype = xenotype;
				existing.ForcedCustomXenotype = customXenotype;
				existing.AllowedXenotypes = allowedXenotypes;
				existing.ForceBaselinerChance = forceBaselinerChance;
				StartingPawnUtility.SetGenerationRequest(index, existing);
				if (randomize)
				{
					randomizeCallback();
				}
			}
		}

		private static string GetXenotypeLabel(int startingPawnIndex)
		{
			PawnGenerationRequest generationRequest = StartingPawnUtility.GetGenerationRequest(startingPawnIndex);
			if (generationRequest.ForcedCustomXenotype != null)
			{
				return generationRequest.ForcedCustomXenotype.name.CapitalizeFirst();
			}
			if (generationRequest.ForcedXenotype != null)
			{
				return generationRequest.ForcedXenotype.LabelCap;
			}
			return "AnyLower".Translate().CapitalizeFirst();
		}

		private static Texture2D GetXenotypeIcon(int startingPawnIndex)
		{
			PawnGenerationRequest generationRequest = StartingPawnUtility.GetGenerationRequest(startingPawnIndex);
			if (generationRequest.ForcedXenotype != null)
			{
				return generationRequest.ForcedXenotype.Icon;
			}
			if (generationRequest.ForcedCustomXenotype != null)
			{
				return generationRequest.ForcedCustomXenotype.IconDef.Icon;
			}
			return GeneUtility.UniqueXenotypeTex.Texture;
		}

		private static float DoTopStack(Pawn pawn, Rect rect, bool creationMode, float curY)
		{
			tmpStackElements.Clear();
			float num = rect.width - 10f;
			float width = (creationMode ? (num - 20f - Page_ConfigureStartingPawns.PawnPortraitSize.x) : num);
			Text.Font = GameFont.Small;
			bool flag = ModsConfig.BiotechActive && creationMode;
			string mainDesc = pawn.MainDesc(writeFaction: false, !flag);
			if (flag)
			{
				tmpStackElements.Add(new GenUI.AnonymousStackElement
				{
					drawer = delegate(Rect r)
					{
						GUI.DrawTexture(r, pawn.gender.GetIcon());
						if (Mouse.IsOver(r))
						{
							TooltipHandler.TipRegion(r, () => pawn.gender.GetLabel(pawn.AnimalOrWildMan()).CapitalizeFirst(), 7594764);
						}
					},
					width = 22f
				});
			}
			tmpStackElements.Add(new GenUI.AnonymousStackElement
			{
				drawer = delegate(Rect r)
				{
					Widgets.Label(r, mainDesc);
					if (Mouse.IsOver(r))
					{
						TooltipHandler.TipRegion(r, () => pawn.ageTracker.AgeTooltipString, 6873641);
					}
				},
				width = Text.CalcSize(mainDesc).x + 5f
			});
			if (ModsConfig.BiotechActive && pawn.genes != null && pawn.genes.GenesListForReading.Any())
			{
				float num2 = 22f;
				num2 += Text.CalcSize(pawn.genes.XenotypeLabelCap).x + 14f;
				tmpStackElements.Add(new GenUI.AnonymousStackElement
				{
					drawer = delegate(Rect r)
					{
						Rect rect11 = new Rect(r.x, r.y, r.width, r.height);
						GUI.color = StackElementBackground;
						GUI.DrawTexture(rect11, BaseContent.WhiteTex);
						GUI.color = Color.white;
						if (Mouse.IsOver(rect11))
						{
							Widgets.DrawHighlight(rect11);
						}
						Rect position5 = new Rect(r.x + 1f, r.y + 1f, 20f, 20f);
						GUI.color = XenotypeDef.IconColor;
						GUI.DrawTexture(position5, pawn.genes.XenotypeIcon);
						GUI.color = Color.white;
						Widgets.Label(new Rect(r.x + 22f + 5f, r.y, r.width + 22f - 1f, r.height), pawn.genes.XenotypeLabelCap);
						if (Mouse.IsOver(r))
						{
							TooltipHandler.TipRegion(r, () => ("Xenotype".Translate() + ": " + pawn.genes.XenotypeLabelCap).Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + pawn.genes.XenotypeDescShort + "\n\n" + "ViewGenesDesc".Translate(pawn.Named("PAWN")).ToString().StripTags()
								.Colorize(ColoredText.SubtleGrayColor), 883938493);
						}
						if (Widgets.ButtonInvisible(r))
						{
							if (Current.ProgramState == ProgramState.Playing && Find.WindowStack.WindowOfType<Dialog_InfoCard>() == null && Find.WindowStack.WindowOfType<Dialog_GrowthMomentChoices>() == null)
							{
								InspectPaneUtility.OpenTab(typeof(ITab_Genes));
							}
							else
							{
								Find.WindowStack.Add(new Dialog_ViewGenes(pawn));
							}
						}
					},
					width = num2
				});
				curY += GenUI.DrawElementStack(new Rect(0f, curY, width, 50f), 22f, tmpStackElements, delegate(Rect r, GenUI.AnonymousStackElement obj)
				{
					obj.drawer(r);
				}, (GenUI.AnonymousStackElement obj) => obj.width, 4f, 5f, allowOrderOptimization: false).height + 4f;
				tmpStackElements.Clear();
			}
			if (pawn.Faction != null && !pawn.Faction.Hidden)
			{
				tmpStackElements.Add(new GenUI.AnonymousStackElement
				{
					drawer = delegate(Rect r)
					{
						Rect rect9 = new Rect(r.x, r.y, r.width, r.height);
						Color color6 = GUI.color;
						GUI.color = StackElementBackground;
						GUI.DrawTexture(rect9, BaseContent.WhiteTex);
						GUI.color = color6;
						Widgets.DrawHighlightIfMouseover(rect9);
						Rect rect10 = new Rect(r.x, r.y, r.width, r.height);
						Rect position4 = new Rect(r.x + 1f, r.y + 1f, 20f, 20f);
						GUI.color = pawn.Faction.Color;
						GUI.DrawTexture(position4, pawn.Faction.def.FactionIcon);
						GUI.color = color6;
						Widgets.Label(new Rect(rect10.x + rect10.height + 5f, rect10.y, rect10.width - 10f, rect10.height), pawn.Faction.Name);
						if (Widgets.ButtonInvisible(rect9))
						{
							if (creationMode || Find.WindowStack.AnyWindowAbsorbingAllInput)
							{
								Find.WindowStack.Add(new Dialog_FactionDuringLanding());
							}
							else
							{
								Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Factions);
							}
						}
						if (Mouse.IsOver(rect9))
						{
							string text = "Faction".Translate().Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + "FactionDesc".Translate(pawn.Named("PAWN")).Resolve() + "\n\n" + "ClickToViewFactions".Translate().Colorize(ColoredText.SubtleGrayColor);
							TipSignal tip4 = new TipSignal(text, pawn.Faction.loadID * 37);
							TooltipHandler.TipRegion(rect9, tip4);
						}
					},
					width = Text.CalcSize(pawn.Faction.Name).x + 22f + 15f
				});
			}
			tmpExtraFactions.Clear();
			QuestUtility.GetExtraFactionsFromQuestParts(pawn, tmpExtraFactions);
			GuestUtility.GetExtraFactionsFromGuestStatus(pawn, tmpExtraFactions);
			foreach (ExtraFaction tmpExtraFaction in tmpExtraFactions)
			{
				if (pawn.Faction == tmpExtraFaction.faction)
				{
					continue;
				}
				ExtraFaction localExtraFaction = tmpExtraFaction;
				string factionName = localExtraFaction.faction.Name;
				bool drawExtraFactionIcon = localExtraFaction.factionType == ExtraFactionType.HomeFaction || localExtraFaction.factionType == ExtraFactionType.MiniFaction;
				tmpStackElements.Add(new GenUI.AnonymousStackElement
				{
					drawer = delegate(Rect r)
					{
						Rect rect6 = new Rect(r.x, r.y, r.width, r.height);
						Rect rect7 = (drawExtraFactionIcon ? rect6 : r);
						Color color5 = GUI.color;
						GUI.color = StackElementBackground;
						GUI.DrawTexture(rect7, BaseContent.WhiteTex);
						GUI.color = color5;
						Widgets.DrawHighlightIfMouseover(rect7);
						if (drawExtraFactionIcon)
						{
							Rect rect8 = new Rect(r.x, r.y, r.width, r.height);
							Rect position3 = new Rect(r.x + 1f, r.y + 1f, 20f, 20f);
							GUI.color = localExtraFaction.faction.Color;
							GUI.DrawTexture(position3, localExtraFaction.faction.def.FactionIcon);
							GUI.color = color5;
							Widgets.Label(new Rect(rect8.x + rect8.height + 5f, rect8.y, rect8.width - 10f, rect8.height), factionName);
						}
						else
						{
							Widgets.Label(new Rect(r.x + 5f, r.y, r.width - 10f, r.height), factionName);
						}
						if (Widgets.ButtonInvisible(rect6))
						{
							Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Factions);
						}
						if (Mouse.IsOver(rect7))
						{
							TipSignal tip3 = new TipSignal((localExtraFaction.factionType.GetLabel().CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + "ExtraFactionDesc".Translate(pawn.Named("PAWN")) + "\n\n" + "ClickToViewFactions".Translate().Colorize(ColoredText.SubtleGrayColor)).Resolve(), localExtraFaction.faction.loadID ^ 0x738AC053);
							TooltipHandler.TipRegion(rect7, tip3);
						}
					},
					width = Text.CalcSize(factionName).x + (float)(drawExtraFactionIcon ? 22 : 0) + 15f
				});
			}
			if (!Find.IdeoManager.classicMode && pawn.Ideo != null && ModsConfig.IdeologyActive)
			{
				float width2 = Text.CalcSize(pawn.Ideo.name).x + 22f + 15f;
				tmpStackElements.Add(new GenUI.AnonymousStackElement
				{
					drawer = delegate(Rect r)
					{
						GUI.color = StackElementBackground;
						GUI.DrawTexture(r, BaseContent.WhiteTex);
						GUI.color = Color.white;
						IdeoUIUtility.DrawIdeoPlate(r, pawn.Ideo, pawn);
					},
					width = width2
				});
			}
			if (ModsConfig.IdeologyActive)
			{
				Precept_Role role = pawn.Ideo?.GetRole(pawn);
				if (role != null)
				{
					string roleLabel = role.LabelForPawn(pawn);
					tmpStackElements.Add(new GenUI.AnonymousStackElement
					{
						drawer = delegate(Rect r)
						{
							Color color4 = GUI.color;
							Rect rect4 = new Rect(r.x, r.y, r.width, r.height);
							GUI.color = StackElementBackground;
							GUI.DrawTexture(rect4, BaseContent.WhiteTex);
							GUI.color = color4;
							if (Mouse.IsOver(rect4))
							{
								Widgets.DrawHighlight(rect4);
							}
							Rect rect5 = new Rect(r.x, r.y, r.width + 22f + 9f, r.height);
							Rect position2 = new Rect(r.x + 1f, r.y + 1f, 20f, 20f);
							GUI.color = pawn.Ideo.Color;
							GUI.DrawTexture(position2, role.Icon);
							GUI.color = Color.white;
							Widgets.Label(new Rect(rect5.x + 22f + 5f, rect5.y, rect5.width - 10f, rect5.height), roleLabel);
							if (Widgets.ButtonInvisible(rect4))
							{
								InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Social));
							}
							if (Mouse.IsOver(rect4))
							{
								TipSignal tip2 = new TipSignal(() => role.GetTip(), (int)curY * 39);
								TooltipHandler.TipRegion(rect4, tip2);
							}
						},
						width = Text.CalcSize(roleLabel).x + 22f + 14f
					});
				}
			}
			if (pawn.royalty != null && pawn.royalty.AllTitlesForReading.Count > 0)
			{
				foreach (RoyalTitle title in pawn.royalty.AllTitlesForReading)
				{
					RoyalTitle localTitle = title;
					string titleLabel = localTitle.def.GetLabelCapFor(pawn) + " (" + pawn.royalty.GetFavor(localTitle.faction) + ")";
					tmpStackElements.Add(new GenUI.AnonymousStackElement
					{
						drawer = delegate(Rect r)
						{
							Color color3 = GUI.color;
							Rect rect2 = new Rect(r.x, r.y, r.width, r.height);
							GUI.color = StackElementBackground;
							GUI.DrawTexture(rect2, BaseContent.WhiteTex);
							GUI.color = color3;
							int favor = pawn.royalty.GetFavor(localTitle.faction);
							if (Mouse.IsOver(rect2))
							{
								Widgets.DrawHighlight(rect2);
							}
							Rect rect3 = new Rect(r.x, r.y, r.width + 22f + 9f, r.height);
							Rect position = new Rect(r.x + 1f, r.y + 1f, 20f, 20f);
							GUI.color = title.faction.Color;
							GUI.DrawTexture(position, localTitle.faction.def.FactionIcon);
							GUI.color = color3;
							Widgets.Label(new Rect(rect3.x + 22f + 5f, rect3.y, rect3.width - 10f, rect3.height), titleLabel);
							if (Widgets.ButtonInvisible(rect2))
							{
								Find.WindowStack.Add(new Dialog_InfoCard(localTitle.def, localTitle.faction, pawn));
							}
							if (Mouse.IsOver(rect2))
							{
								TipSignal tip = new TipSignal(() => GetTitleTipString(pawn, localTitle.faction, localTitle, favor), (int)curY * 37);
								TooltipHandler.TipRegion(rect2, tip);
							}
						},
						width = Text.CalcSize(titleLabel).x + 22f + 14f
					});
				}
			}
			if (ModsConfig.IdeologyActive && !pawn.DevelopmentalStage.Baby() && pawn.story != null && pawn.story.favoriteColor.HasValue)
			{
				tmpStackElements.Add(new GenUI.AnonymousStackElement
				{
					drawer = delegate(Rect r)
					{
						string orIdeoColor = string.Empty;
						if (pawn.Ideo != null && !pawn.Ideo.hiddenIdeoMode)
						{
							orIdeoColor = "OrIdeoColor".Translate(pawn.Named("PAWN"));
						}
						Widgets.DrawRectFast(r, pawn.story.favoriteColor.Value);
						GUI.color = FavColorBoxColor;
						Widgets.DrawBox(r);
						GUI.color = Color.white;
						TooltipHandler.TipRegion(r, () => "FavoriteColorTooltip".Translate(pawn.Named("PAWN"), 0.6f.ToStringPercent().Named("PERCENTAGE"), orIdeoColor.Named("ORIDEO")).Resolve(), 837472764);
					},
					width = 22f
				});
			}
			if (pawn.guest != null && !pawn.guest.Recruitable)
			{
				tmpStackElements.Add(new GenUI.AnonymousStackElement
				{
					drawer = delegate(Rect r)
					{
						Color color2 = GUI.color;
						GUI.color = StackElementBackground;
						GUI.DrawTexture(r, BaseContent.WhiteTex);
						GUI.color = color2;
						GUI.DrawTexture(r, UnrecruitableIcon);
						if (Mouse.IsOver(r))
						{
							Widgets.DrawHighlight(r);
							TooltipHandler.TipRegion(r, () => "Unrecruitable".Translate().AsTipTitle().CapitalizeFirst() + "\n\n" + "UnrecruitableDesc".Translate(pawn.Named("PAWN")).Resolve(), 15877733);
						}
					},
					width = 22f
				});
			}
			QuestUtility.AppendInspectStringsFromQuestParts(delegate(string str, Quest quest)
			{
				tmpStackElements.Add(new GenUI.AnonymousStackElement
				{
					drawer = delegate(Rect r)
					{
						Color color = GUI.color;
						GUI.color = StackElementBackground;
						GUI.DrawTexture(r, BaseContent.WhiteTex);
						GUI.color = color;
						DoQuestLine(r, str, quest);
					},
					width = GetQuestLineSize(str, quest).x
				});
			}, pawn, out var _);
			curY += GenUI.DrawElementStack(new Rect(0f, curY, width, 50f), 22f, tmpStackElements, delegate(Rect r, GenUI.AnonymousStackElement obj)
			{
				obj.drawer(r);
			}, (GenUI.AnonymousStackElement obj) => obj.width, 4f, 5f, allowOrderOptimization: false).height;
			if (tmpStackElements.Any())
			{
				curY += 10f;
			}
			return curY;
		}

		private static void DoLeftSection(Rect rect, Rect leftRect, Pawn pawn)
		{
			Widgets.BeginGroup(leftRect);
			float num = 0f;
			Pawn pawnLocal = pawn;
			List<Ability> abilities = (from a in pawn.abilities.abilities
				orderby a.def.level, a.def.EntropyGain
				select a).ToList();
			int numSections = (abilities.Any() ? 5 : 4);
			float num2 = (float)Enum.GetValues(typeof(BackstorySlot)).Length * 22f;
			float stackHeight = 0f;
			if (pawn.story != null && pawn.story.title != null)
			{
				num2 += 22f;
			}
			List<LeftRectSection> list = new List<LeftRectSection>();
			list.Add(new LeftRectSection
			{
				rect = new Rect(0f, 0f, leftRect.width, num2),
				drawer = delegate(Rect sectionRect)
				{
					float num8 = sectionRect.y;
					Text.Font = GameFont.Small;
					foreach (BackstorySlot value6 in Enum.GetValues(typeof(BackstorySlot)))
					{
						BackstoryDef backstory = pawn.story.GetBackstory(value6);
						if (backstory != null)
						{
							Rect rect7 = new Rect(sectionRect.x, num8, leftRect.width, 22f);
							Text.Anchor = TextAnchor.MiddleLeft;
							Widgets.Label(rect7, (value6 == BackstorySlot.Adulthood) ? "Adulthood".Translate() : "Childhood".Translate());
							Text.Anchor = TextAnchor.UpperLeft;
							string text = backstory.TitleCapFor(pawn.gender);
							Rect rect8 = new Rect(rect7);
							rect8.x += 90f;
							rect8.width = Text.CalcSize(text).x + 10f;
							Color color4 = GUI.color;
							GUI.color = StackElementBackground;
							GUI.DrawTexture(rect8, BaseContent.WhiteTex);
							GUI.color = color4;
							Text.Anchor = TextAnchor.MiddleCenter;
							Widgets.Label(rect8, text.Truncate(rect8.width));
							Text.Anchor = TextAnchor.UpperLeft;
							if (Mouse.IsOver(rect8))
							{
								Widgets.DrawHighlight(rect8);
							}
							if (Mouse.IsOver(rect8))
							{
								TooltipHandler.TipRegion(rect8, backstory.FullDescriptionFor(pawn).Resolve());
							}
							num8 += rect7.height + 4f;
						}
					}
					if (pawn.story != null && pawn.story.title != null)
					{
						Rect rect9 = new Rect(sectionRect.x, num8, leftRect.width, 22f);
						Text.Anchor = TextAnchor.MiddleLeft;
						Widgets.Label(rect9, "BackstoryTitle".Translate() + ":");
						Text.Anchor = TextAnchor.UpperLeft;
						Rect rect10 = new Rect(rect9);
						rect10.x += 90f;
						rect10.width -= 90f;
						Widgets.Label(rect10, pawn.story.title);
						num8 += rect9.height;
					}
				}
			});
			num2 = 30f;
			List<Trait> traits = pawn.story.traits.allTraits;
			if (traits == null || traits.Count == 0)
			{
				num2 += 22f;
				stackHeight = 22f;
			}
			else
			{
				Rect rect2 = GenUI.DrawElementStack(new Rect(0f, 0f, leftRect.width - 5f, leftRect.height), 22f, pawn.story.traits.TraitsSorted, delegate
				{
				}, (Trait trait) => Text.CalcSize(trait.LabelCap).x + 10f, 4f, 5f, allowOrderOptimization: false);
				num2 += rect2.height;
				stackHeight = rect2.height;
			}
			list.Add(new LeftRectSection
			{
				rect = new Rect(0f, 0f, leftRect.width, num2),
				drawer = delegate(Rect sectionRect)
				{
					float currentY3 = sectionRect.y;
					Widgets.Label(new Rect(sectionRect.x, currentY3, 200f, 30f), "Traits".Translate().AsTipTitle());
					currentY3 += 24f;
					if (traits == null || traits.Count == 0)
					{
						Color color2 = GUI.color;
						GUI.color = Color.gray;
						Rect rect6 = new Rect(sectionRect.x, currentY3, leftRect.width, 24f);
						if (Mouse.IsOver(rect6))
						{
							Widgets.DrawHighlight(rect6);
						}
						Widgets.Label(rect6, pawn.DevelopmentalStage.Baby() ? "TraitsDevelopLaterBaby".Translate() : "None".Translate());
						currentY3 += rect6.height + 2f;
						TooltipHandler.TipRegionByKey(rect6, "None");
						GUI.color = color2;
					}
					else
					{
						GenUI.DrawElementStack(new Rect(sectionRect.x, currentY3, leftRect.width - 5f, stackHeight), 22f, pawn.story.traits.TraitsSorted, delegate(Rect r, Trait trait)
						{
							Color color3 = GUI.color;
							GUI.color = StackElementBackground;
							GUI.DrawTexture(r, BaseContent.WhiteTex);
							GUI.color = color3;
							if (Mouse.IsOver(r))
							{
								Widgets.DrawHighlight(r);
							}
							if (trait.Suppressed)
							{
								GUI.color = ColoredText.SubtleGrayColor;
							}
							else if (trait.sourceGene != null)
							{
								GUI.color = ColoredText.GeneColor;
							}
							Widgets.Label(new Rect(r.x + 5f, r.y, r.width - 10f, r.height), trait.LabelCap);
							GUI.color = Color.white;
							if (Mouse.IsOver(r))
							{
								Trait trLocal = trait;
								TooltipHandler.TipRegion(tip: new TipSignal(() => trLocal.TipString(pawn), (int)currentY3 * 37), rect: r);
							}
						}, (Trait trait) => Text.CalcSize(trait.LabelCap).x + 10f, 4f, 5f, allowOrderOptimization: false);
					}
				}
			});
			num2 = 30f;
			WorkTags disabledTags = pawn.CombinedDisabledWorkTags;
			List<WorkTags> disabledTagsList = WorkTagsFrom(disabledTags).ToList();
			bool allowWorkTagVerticalLayout = false;
			GenUI.StackElementWidthGetter<WorkTags> workTagWidthGetter = (WorkTags tag) => Text.CalcSize(tag.LabelTranslated().CapitalizeFirst()).x + 10f;
			if (disabledTags == WorkTags.None)
			{
				num2 += 22f;
			}
			else
			{
				disabledTagsList.Sort(delegate(WorkTags a, WorkTags b)
				{
					int num7 = (GetWorkTypeDisableCauses(pawn, a).Any((object c) => c is RoyalTitleDef) ? 1 : (-1));
					int value5 = (GetWorkTypeDisableCauses(pawn, b).Any((object c) => c is RoyalTitleDef) ? 1 : (-1));
					return num7.CompareTo(value5);
				});
				Rect rect3 = GenUI.DrawElementStack(new Rect(0f, 0f, leftRect.width - 5f, leftRect.height), 22f, disabledTagsList, delegate
				{
				}, workTagWidthGetter, 4f, 5f, allowOrderOptimization: false);
				num2 += rect3.height;
				stackHeight = rect3.height;
				num2 += 12f;
				allowWorkTagVerticalLayout = GenUI.DrawElementStackVertical(new Rect(0f, 0f, rect.width, stackHeight), 22f, disabledTagsList, delegate
				{
				}, workTagWidthGetter).width <= leftRect.width;
			}
			list.Add(new LeftRectSection
			{
				rect = new Rect(0f, 0f, leftRect.width, num2),
				drawer = delegate(Rect sectionRect)
				{
					float currentY2 = sectionRect.y;
					Widgets.Label(new Rect(sectionRect.x, currentY2, 200f, 24f), "IncapableOf".Translate(pawn).AsTipTitle());
					currentY2 += 24f;
					if (disabledTags == WorkTags.None)
					{
						GUI.color = Color.gray;
						Rect rect5 = new Rect(sectionRect.x, currentY2, leftRect.width, 24f);
						if (Mouse.IsOver(rect5))
						{
							Widgets.DrawHighlight(rect5);
						}
						Widgets.Label(rect5, "None".Translate());
						TooltipHandler.TipRegionByKey(rect5, "None");
					}
					else
					{
						GenUI.StackElementDrawer<WorkTags> drawer = delegate(Rect r, WorkTags tag)
						{
							Color color = GUI.color;
							GUI.color = StackElementBackground;
							GUI.DrawTexture(r, BaseContent.WhiteTex);
							GUI.color = color;
							GUI.color = GetDisabledWorkTagLabelColor(pawn, tag);
							if (Mouse.IsOver(r))
							{
								Widgets.DrawHighlight(r);
							}
							Widgets.Label(new Rect(r.x + 5f, r.y, r.width - 10f, r.height), tag.LabelTranslated().CapitalizeFirst());
							if (Mouse.IsOver(r))
							{
								WorkTags tagLocal = tag;
								TooltipHandler.TipRegion(tip: new TipSignal(() => GetWorkTypeDisabledCausedBy(pawnLocal, tagLocal) + "\n" + GetWorkTypesDisabledByWorkTag(tagLocal), (int)currentY2 * 32), rect: r);
							}
						};
						if (allowWorkTagVerticalLayout)
						{
							GenUI.DrawElementStackVertical(new Rect(sectionRect.x, currentY2, leftRect.width - 5f, leftRect.height / (float)numSections), 22f, disabledTagsList, drawer, workTagWidthGetter);
						}
						else
						{
							GenUI.DrawElementStack(new Rect(sectionRect.x, currentY2, leftRect.width - 5f, leftRect.height / (float)numSections), 22f, disabledTagsList, drawer, workTagWidthGetter, 5f);
						}
					}
					GUI.color = Color.white;
				}
			});
			if (abilities.Any())
			{
				num2 = 30f;
				Rect rect4 = GenUI.DrawElementStack(new Rect(0f, 0f, leftRect.width - 5f, leftRect.height), 32f, abilities, delegate
				{
				}, (Ability abil) => 32f);
				num2 += rect4.height;
				stackHeight = rect4.height;
				list.Add(new LeftRectSection
				{
					rect = new Rect(0f, 0f, leftRect.width, num2),
					drawer = delegate(Rect sectionRect)
					{
						float currentY = sectionRect.y;
						Widgets.Label(new Rect(sectionRect.x, currentY, 200f, 24f), "Abilities".Translate(pawn).AsTipTitle());
						currentY += 24f;
						GenUI.DrawElementStack(new Rect(sectionRect.x, currentY, leftRect.width - 5f, stackHeight), 32f, abilities, delegate(Rect r, Ability abil)
						{
							GUI.DrawTexture(r, BaseContent.ClearTex);
							if (Mouse.IsOver(r))
							{
								Widgets.DrawHighlight(r);
							}
							if (Widgets.ButtonImage(r, abil.def.uiIcon, doMouseoverSound: false))
							{
								Find.WindowStack.Add(new Dialog_InfoCard(abil.def));
							}
							if (Mouse.IsOver(r))
							{
								Ability abilCapture = abil;
								TipSignal tip = new TipSignal(() => abilCapture.Tooltip + "\n\n" + "ClickToLearnMore".Translate().Colorize(ColoredText.SubtleGrayColor), (int)currentY * 37);
								TooltipHandler.TipRegion(r, tip);
							}
						}, (Ability abil) => 32f);
						GUI.color = Color.white;
					}
				});
			}
			else
			{
				num2 += 12f;
			}
			float num3 = leftRect.height / (float)list.Count;
			float num4 = 0f;
			for (int i = 0; i < list.Count; i++)
			{
				LeftRectSection value = list[i];
				if (value.rect.height > num3)
				{
					num4 += value.rect.height - num3;
					value.calculatedSize = value.rect.height;
				}
				else
				{
					value.calculatedSize = num3;
				}
				list[i] = value;
			}
			bool flag = false;
			float num5 = 0f;
			if (num4 > 0f)
			{
				LeftRectSection value2 = list[0];
				float num6 = value2.rect.height + 12f;
				num4 -= value2.calculatedSize - num6;
				value2.calculatedSize = num6;
				list[0] = value2;
			}
			while (num4 > 0f)
			{
				bool flag2 = true;
				for (int j = 0; j < list.Count; j++)
				{
					LeftRectSection value3 = list[j];
					if (value3.calculatedSize - value3.rect.height > 0f)
					{
						value3.calculatedSize -= 1f;
						num4 -= 1f;
						flag2 = false;
					}
					list[j] = value3;
				}
				if (!flag2)
				{
					continue;
				}
				for (int k = 0; k < list.Count; k++)
				{
					LeftRectSection value4 = list[k];
					if (k > 0)
					{
						value4.calculatedSize = Mathf.Max(value4.rect.height, num3);
					}
					else
					{
						value4.calculatedSize = value4.rect.height + 22f;
					}
					num5 += value4.calculatedSize;
					list[k] = value4;
				}
				flag = true;
				break;
			}
			if (flag)
			{
				Widgets.BeginScrollView(new Rect(0f, 0f, leftRect.width, leftRect.height), ref leftRectScrollPos, new Rect(0f, 0f, leftRect.width - 16f, num5));
			}
			num = 0f;
			for (int l = 0; l < list.Count; l++)
			{
				LeftRectSection leftRectSection = list[l];
				leftRectSection.drawer(new Rect(0f, num, leftRect.width - 5f, leftRectSection.rect.height));
				num += leftRectSection.calculatedSize;
			}
			if (flag)
			{
				Widgets.EndScrollView();
			}
			Widgets.EndGroup();
		}

		private static string GetWorkTypeDisabledCausedBy(Pawn pawn, WorkTags workTag)
		{
			List<object> workTypeDisableCauses = GetWorkTypeDisableCauses(pawn, workTag);
			StringBuilder stringBuilder = new StringBuilder();
			foreach (object item in workTypeDisableCauses)
			{
				if (item is BackstoryDef)
				{
					stringBuilder.AppendLine("IncapableOfTooltipBackstory".Translate() + ": " + (item as BackstoryDef).TitleFor(pawn.gender).CapitalizeFirst());
				}
				else if (item is Trait)
				{
					stringBuilder.AppendLine("IncapableOfTooltipTrait".Translate() + ": " + (item as Trait).LabelCap);
				}
				else if (item is Hediff)
				{
					stringBuilder.AppendLine("IncapableOfTooltipHediff".Translate() + ": " + (item as Hediff).LabelCap);
				}
				else if (item is RoyalTitle)
				{
					stringBuilder.AppendLine("IncapableOfTooltipTitle".Translate() + ": " + (item as RoyalTitle).def.GetLabelFor(pawn));
				}
				else if (item is Quest)
				{
					stringBuilder.AppendLine("IncapableOfTooltipQuest".Translate() + ": " + (item as Quest).name);
				}
				else if (item is Precept_Role)
				{
					stringBuilder.AppendLine("IncapableOfTooltipRole".Translate() + ": " + (item as Precept_Role).LabelForPawn(pawn));
				}
				else if (item is Gene)
				{
					stringBuilder.AppendLine("IncapableOfTooltipGene".Translate() + ": " + (item as Gene).LabelCap);
				}
			}
			return stringBuilder.ToString();
		}

		private static string GetWorkTypesDisabledByWorkTag(WorkTags workTag)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("IncapableOfTooltipWorkTypes".Translate().Colorize(ColoredText.TipSectionTitleColor));
			foreach (WorkTypeDef allDef in DefDatabase<WorkTypeDef>.AllDefs)
			{
				if ((allDef.workTags & workTag) > WorkTags.None)
				{
					stringBuilder.Append("- ");
					stringBuilder.AppendLine(allDef.pawnLabel);
				}
			}
			return stringBuilder.ToString();
		}

		public static Vector2 PawnCardSize(Pawn pawn)
		{
			Vector2 basePawnCardSize = BasePawnCardSize;
			tmpInspectStrings.Length = 0;
			QuestUtility.AppendInspectStringsFromQuestParts(tmpInspectStrings, pawn, out var count);
			if (count >= 2)
			{
				basePawnCardSize.y += (count - 1) * 20;
			}
			return basePawnCardSize;
		}

		public static void DoNameInputRect(Rect rect, ref string name, int maxLength)
		{
			string text = Widgets.TextField(rect, name);
			if (text.Length <= maxLength && ValidNameRegex.IsMatch(text))
			{
				name = text;
			}
		}

		private static IEnumerable<WorkTags> WorkTagsFrom(WorkTags tags)
		{
			foreach (WorkTags allSelectedItem in tags.GetAllSelectedItems<WorkTags>())
			{
				if (allSelectedItem != 0)
				{
					yield return allSelectedItem;
				}
			}
		}

		private static Vector2 GetQuestLineSize(string line, Quest quest)
		{
			Vector2 vector = Text.CalcSize(line);
			return new Vector2(17f + vector.x + 10f, Mathf.Max(24f, vector.y));
		}

		private static void DoQuestLine(Rect rect, string line, Quest quest)
		{
			Rect rect2 = rect;
			rect2.xMin += 22f;
			rect2.height = Text.CalcSize(line).y;
			float x = Text.CalcSize(line).x;
			Rect rect3 = new Rect(rect.x, rect.y, Mathf.Min(x, rect2.width) + 24f + -7f + 5f, rect.height);
			if (!quest.hidden)
			{
				Widgets.DrawHighlightIfMouseover(rect3);
				TooltipHandler.TipRegionByKey(rect3, "ClickToViewInQuestsTab");
			}
			GUI.DrawTexture(new Rect(rect.x + -7f, rect.y - 2f, 24f, 24f), QuestIcon);
			Widgets.Label(rect2, line.Truncate(rect2.width));
			if (!quest.hidden && Widgets.ButtonInvisible(rect3))
			{
				Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests);
				((MainTabWindow_Quests)MainButtonDefOf.Quests.TabWindow).Select(quest);
			}
		}
	}
}
