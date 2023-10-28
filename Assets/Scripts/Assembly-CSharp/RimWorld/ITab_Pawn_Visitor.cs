using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class ITab_Pawn_Visitor : ITab
	{
		private const float CheckboxInterval = 30f;

		private const float CheckboxMargin = 50f;

		private static List<PrisonerInteractionModeDef> tmpPrisonerInteractionModes = new List<PrisonerInteractionModeDef>();

		private static List<SlaveInteractionModeDef> tmpSlaveInteractionModes = new List<SlaveInteractionModeDef>();

		private const float SuppresionBarHeight = 30f;

		private const float SuppresionBarMargin = 7f;

		public ITab_Pawn_Visitor()
		{
			size = new Vector2(280f, 0f);
		}

		protected override void FillTab()
		{
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.PrisonerTab, KnowledgeAmount.FrameDisplayed);
			Text.Font = GameFont.Small;
			Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
			bool isPrisonerOfColony = base.SelPawn.IsPrisonerOfColony;
			bool isSlaveOfColony = base.SelPawn.IsSlaveOfColony;
			bool wildMan = base.SelPawn.IsWildMan();
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.maxOneColumn = true;
			listing_Standard.Begin(rect);
			if (!isSlaveOfColony)
			{
				Rect rect2 = listing_Standard.GetRect(28f);
				rect2.width = 140f;
				MedicalCareUtility.MedicalCareSetter(rect2, ref base.SelPawn.playerSettings.medCare);
				listing_Standard.Gap(4f);
			}
			if (isPrisonerOfColony)
			{
				if (!wildMan)
				{
					StringBuilder stringBuilder = new StringBuilder();
					int num = (int)PrisonBreakUtility.InitiatePrisonBreakMtbDays(base.SelPawn, stringBuilder, ignoreAsleep: true);
					string text = "PrisonBreakMTBDays".Translate() + ": ";
					if (PrisonBreakUtility.IsPrisonBreaking(base.SelPawn))
					{
						text += "CurrentlyPrisonBreaking".Translate();
					}
					else if (num < 0)
					{
						text += "Never".Translate();
						if (PrisonBreakUtility.GenePreventsPrisonBreaking(base.SelPawn, out var gene))
						{
							stringBuilder.AppendLineIfNotEmpty();
							stringBuilder.AppendTagged("PrisonBreakingDisabledDueToGene".Translate(gene.def.Named("GENE")).Colorize(ColorLibrary.RedReadable));
						}
					}
					else
					{
						text += "PeriodDays".Translate(num).ToString().Colorize(ColoredText.DateTimeColor);
					}
					Rect rect3 = listing_Standard.Label(text);
					string text2 = "PrisonBreakMTBDaysDescription".Translate();
					if (stringBuilder.Length > 0)
					{
						text2 = text2 + "\n\n" + stringBuilder.ToString();
					}
					TooltipHandler.TipRegion(rect3, text2);
					Widgets.DrawHighlightIfMouseover(rect3);
					if (base.SelPawn.guest.Recruitable)
					{
						Rect rect4 = listing_Standard.Label("RecruitmentResistance".Translate() + ": " + base.SelPawn.guest.resistance.ToString("F1"));
						if (Mouse.IsOver(rect4))
						{
							TaggedString taggedString = "RecruitmentResistanceDesc".Translate();
							FloatRange value = base.SelPawn.kindDef.initialResistanceRange.Value;
							taggedString += string.Format("\n\n{0}: {1}~{2}", "RecruitmentResistanceFromPawnKind".Translate(base.SelPawn.kindDef.LabelCap), value.min, value.max);
							if (base.SelPawn.royalty != null)
							{
								RoyalTitle mostSeniorTitle = base.SelPawn.royalty.MostSeniorTitle;
								if (mostSeniorTitle != null && mostSeniorTitle.def.recruitmentResistanceOffset != 0f)
								{
									string text3 = ((mostSeniorTitle.def.recruitmentResistanceOffset > 0f) ? "+" : "-");
									taggedString += "\n" + "RecruitmentResistanceRoyalTitleOffset".Translate(mostSeniorTitle.Label.CapitalizeFirst()) + (": " + text3) + mostSeniorTitle.def.recruitmentResistanceOffset.ToString();
								}
							}
							TooltipHandler.TipRegion(rect4, taggedString);
						}
						Widgets.DrawHighlightIfMouseover(rect4);
					}
					else
					{
						Rect rect5 = listing_Standard.Label("RecruitmentResistance".Translate() + ": " + "NonRecruitable".Translate());
						string text4 = "NonRecruitableTip".Translate();
						if (ModsConfig.IdeologyActive)
						{
							text4 += "\n\n" + "NonRecruitableTipCannotConvert".Translate();
						}
						TooltipHandler.TipRegion(rect5, text4);
						Widgets.DrawHighlightIfMouseover(rect5);
					}
					if (ModsConfig.IdeologyActive)
					{
						Rect rect6 = listing_Standard.Label("WillLevel".Translate() + ": " + base.SelPawn.guest.will.ToString("F1"));
						TaggedString taggedString2 = "WillLevelDesc".Translate(2.5f);
						if (!base.SelPawn.guest.EverEnslaved)
						{
							FloatRange value2 = base.SelPawn.kindDef.initialWillRange.Value;
							taggedString2 += string.Format("\n\n{0} : {1}~{2}", "WillFromPawnKind".Translate(base.SelPawn.kindDef.LabelCap), value2.min, value2.max);
						}
						TooltipHandler.TipRegion(rect6, taggedString2);
						Widgets.DrawHighlightIfMouseover(rect6);
					}
				}
				DoSlavePriceListing(listing_Standard, base.SelPawn);
				TaggedString taggedString3;
				if (base.SelPawn.Faction == null || base.SelPawn.Faction.IsPlayer || !base.SelPawn.Faction.CanChangeGoodwillFor(Faction.OfPlayer, 1))
				{
					taggedString3 = "None".Translate();
				}
				else
				{
					bool isHealthy;
					bool isInMentalState;
					int i = base.SelPawn.Faction.CalculateAdjustedGoodwillChange(Faction.OfPlayer, base.SelPawn.Faction.GetGoodwillGainForPrisonerRelease(base.SelPawn, out isHealthy, out isInMentalState));
					taggedString3 = ((isHealthy && !isInMentalState) ? (base.SelPawn.Faction.NameColored + " " + i.ToStringWithSign()) : ((!isHealthy) ? ("None".Translate() + " (" + "UntendedInjury".Translate() + ")") : ((!isInMentalState) ? "None".Translate() : ("None".Translate() + " (" + base.SelPawn.MentalState.InspectLine + ")"))));
				}
				Rect rect7 = listing_Standard.Label("PrisonerReleasePotentialRelationGains".Translate() + ": " + taggedString3);
				TooltipHandler.TipRegionByKey(rect7, "PrisonerReleaseRelationGainsDesc");
				Widgets.DrawHighlightIfMouseover(rect7);
				if (base.SelPawn.guilt.IsGuilty)
				{
					if (!base.SelPawn.InAggroMentalState)
					{
						listing_Standard.Label("ConsideredGuilty".Translate(base.SelPawn.guilt.TicksUntilInnocent.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor)));
					}
					else
					{
						listing_Standard.Label("ConsideredGuiltyNoTimer".Translate() + " (" + base.SelPawn.MentalStateDef.label + ")");
					}
				}
				if (ModsConfig.IdeologyActive && isPrisonerOfColony && base.SelPawn.guest.interactionMode == PrisonerInteractionModeDefOf.Convert)
				{
					Rect rect8 = listing_Standard.GetRect(32f);
					Rect rect9 = new Rect(rect8.xMax - 32f - 4f, rect8.y, 32f, 32f);
					rect8.xMax = rect9.xMin;
					Text.Anchor = TextAnchor.MiddleLeft;
					Widgets.Label(rect8, "IdeoConversionTarget".Translate());
					Text.Anchor = TextAnchor.UpperLeft;
					Widgets.DrawHighlightIfMouseover(rect8);
					TooltipHandler.TipRegionByKey(rect8, "IdeoConversionTargetDesc");
					base.SelPawn.guest.ideoForConversion.DrawIcon(rect9.ContractedBy(2f));
					if (Mouse.IsOver(rect9))
					{
						Widgets.DrawHighlight(rect9);
						TooltipHandler.TipRegion(rect9, base.SelPawn.guest.ideoForConversion.name);
					}
					if (Widgets.ButtonInvisible(rect9))
					{
						List<FloatMenuOption> list = new List<FloatMenuOption>();
						foreach (Ideo allIdeo in Faction.OfPlayer.ideos.AllIdeos)
						{
							Ideo newIdeo = allIdeo;
							string text5 = allIdeo.name;
							Action action = delegate
							{
								base.SelPawn.guest.ideoForConversion = newIdeo;
							};
							if (!ColonyHasAnyWardenOfIdeo(newIdeo, base.SelPawn.MapHeld))
							{
								text5 += " (" + "NoWardenOfIdeo".Translate(newIdeo.memberName.Named("MEMBERNAME")) + ")";
								action = null;
							}
							list.Add(new FloatMenuOption(text5, action, newIdeo.Icon, newIdeo.Color));
						}
						Find.WindowStack.Add(new FloatMenu(list));
					}
				}
				if (base.SelPawn.guest.finalResistanceInteractionData != null)
				{
					ResistanceInteractionData finalResistanceInteractionData = base.SelPawn.guest.finalResistanceInteractionData;
					Rect rect10 = listing_Standard.Label("LastRecruitment".Translate() + ": " + finalResistanceInteractionData.resistanceReduction.ToStringByStyle(ToStringStyle.FloatTwo));
					if (Mouse.IsOver(rect10))
					{
						Widgets.DrawHighlight(rect10);
						TaggedString taggedString4 = "LastRecruitmentDescription".Translate(base.SelPawn, finalResistanceInteractionData.initiatorName);
						taggedString4 += "\n\n";
						taggedString4 += "StatsReport_BaseValue".Translate() + ": " + 1f.ToStringByStyle(ToStringStyle.FloatTwo);
						taggedString4 += "\n\n" + "Mood".Translate() + ": x" + finalResistanceInteractionData.recruiteeMoodFactor.ToStringByStyle(ToStringStyle.FloatTwo);
						taggedString4 += "\n" + "RecruiterNegotiationAbility".Translate() + ": x" + finalResistanceInteractionData.initiatorNegotiationAbilityFactor.ToStringByStyle(ToStringStyle.FloatTwo);
						taggedString4 += "\n" + "OpinionOfRecruiter".Translate() + ": x" + finalResistanceInteractionData.recruiterOpinionFactor.ToStringByStyle(ToStringStyle.FloatTwo);
						taggedString4 += "\n" + "StatsReport_FinalValue".Translate() + ": " + finalResistanceInteractionData.resistanceReduction.ToStringByStyle(ToStringStyle.FloatTwo);
						TooltipHandler.TipRegion(rect10, taggedString4);
					}
				}
				tmpPrisonerInteractionModes.Clear();
				tmpPrisonerInteractionModes.AddRange(from m in DefDatabase<PrisonerInteractionModeDef>.AllDefs
					where CanUsePrisonerInteractionMode(base.SelPawn, m)
					select m into pim
					orderby pim.listOrder
					select pim);
				float height = 28f * (float)tmpPrisonerInteractionModes.Count + 20f;
				Rect rect11 = listing_Standard.GetRect(height).Rounded();
				Widgets.DrawMenuSection(rect11);
				Rect rect12 = rect11.ContractedBy(10f);
				Widgets.BeginGroup(rect12);
				Rect rect13 = new Rect(0f, 0f, rect12.width, 28f);
				foreach (PrisonerInteractionModeDef tmpPrisonerInteractionMode in tmpPrisonerInteractionModes)
				{
					if (Widgets.RadioButtonLabeled(rect13, tmpPrisonerInteractionMode.LabelCap, base.SelPawn.guest.interactionMode == tmpPrisonerInteractionMode))
					{
						PrisonerInteractionModeDef interactionMode = base.SelPawn.guest.interactionMode;
						base.SelPawn.guest.interactionMode = tmpPrisonerInteractionMode;
						InteractionModeChanged(interactionMode, tmpPrisonerInteractionMode);
					}
					if (!tmpPrisonerInteractionMode.description.NullOrEmpty() && Mouse.IsOver(rect13))
					{
						Widgets.DrawHighlight(rect13);
						TooltipHandler.TipRegion(rect13, tmpPrisonerInteractionMode.description);
					}
					rect13.y += 28f;
				}
				Widgets.EndGroup();
				tmpPrisonerInteractionModes.Clear();
			}
			if (isSlaveOfColony)
			{
				Need_Suppression need_Suppression = base.SelPawn.needs.TryGetNeed<Need_Suppression>();
				if (need_Suppression != null)
				{
					Rect rect14 = listing_Standard.Label("Suppression".Translate() + ": " + need_Suppression.CurLevel.ToStringPercent());
					Rect rect15 = listing_Standard.GetRect(30f);
					Rect rect16 = rect15.ContractedBy(7f);
					need_Suppression.DrawSuppressionBar(rect16);
					Rect rect17 = new Rect(rect14.x, rect14.y, rect14.width, rect14.height + rect15.height);
					Widgets.DrawHighlightIfMouseover(rect17);
					TaggedString taggedString5 = "SuppressionDesc".Translate();
					TooltipHandler.TipRegion(rect17, taggedString5);
					float statValue = base.SelPawn.GetStatValue(StatDefOf.SlaveSuppressionFallRate);
					string text6 = StatDefOf.SlaveSuppressionFallRate.ValueToString(statValue);
					Rect rect18 = listing_Standard.Label("SuppressionFallRate".Translate() + ": " + text6);
					if (Mouse.IsOver(rect18))
					{
						TaggedString taggedString6 = "SuppressionFallRateDesc".Translate(0.2f.ToStringPercent(), 0.3f.ToStringPercent(), 0.1f.ToStringPercent(), 0.15f.ToStringPercent(), 0.15f.ToStringPercent(), 0.05f.ToStringPercent(), 0.15f.ToStringPercent());
						string explanationForTooltip = ((StatWorker_SuppressionFallRate)StatDefOf.SlaveSuppressionFallRate.Worker).GetExplanationForTooltip(StatRequest.For(base.SelPawn));
						TooltipHandler.TipRegion(rect18, taggedString6 + ":\n\n" + explanationForTooltip);
						Widgets.DrawHighlight(rect18);
					}
					Rect rect19 = listing_Standard.Label(string.Format("{0}: {1}", "Terror".Translate(), base.SelPawn.GetStatValue(StatDefOf.Terror).ToStringPercent()));
					if (Mouse.IsOver(rect19))
					{
						IOrderedEnumerable<Thought_MemoryObservationTerror> source = from t in TerrorUtility.GetTerrorThoughts(base.SelPawn)
							orderby t.intensity descending
							select t;
						TaggedString taggedString7 = "TerrorDescription".Translate() + ":" + "\n\n" + TerrorUtility.SuppressionFallRateOverTerror.Points.Select((CurvePoint p) => string.Format("- {0} {1}: {2}", "Terror".Translate(), (p.x / 100f).ToStringPercent(), (p.y / 100f).ToStringPercent())).ToLineList();
						if (source.Count() > 0)
						{
							string text7 = source.Select((Thought_MemoryObservationTerror t) => $"{t.LabelCap}: {t.intensity}%").ToLineList("- ", capitalizeItems: true);
							taggedString7 += "\n\n" + "TerrorCurrentThoughts".Translate() + ":" + "\n\n" + text7;
						}
						TooltipHandler.TipRegion(rect19, taggedString7);
						Widgets.DrawHighlight(rect19);
					}
					float num2 = SlaveRebellionUtility.InitiateSlaveRebellionMtbDays(base.SelPawn);
					TaggedString label = "SlaveRebellionMTBDays".Translate() + ": ";
					if (!base.SelPawn.Awake())
					{
						label += "NotWhileAsleep".Translate();
					}
					else if (num2 < 0f)
					{
						label += "Never".Translate();
					}
					else
					{
						label += ((int)(num2 * 60000f)).ToStringTicksToPeriod();
					}
					Rect rect20 = listing_Standard.Label(label);
					TooltipHandler.TipRegion(rect20, delegate
					{
						TaggedString taggedString9 = "SlaveRebellionMTBDaysDescription".Translate();
						string text9 = SlaveRebellionUtility.GetSlaveRebellionMtbCalculationExplanation(base.SelPawn) + "\n" + SlaveRebellionUtility.GetAnySlaveRebellionExplanation(base.SelPawn);
						if (!text9.NullOrEmpty())
						{
							taggedString9 += "\n\n" + text9;
						}
						return taggedString9;
					}, "SlaveRebellionTooltip".GetHashCode());
					Widgets.DrawHighlightIfMouseover(rect20);
					DoSlavePriceListing(listing_Standard, base.SelPawn);
					Faction faction = base.SelPawn.SlaveFaction ?? base.SelPawn.Faction;
					TaggedString taggedString8;
					if (faction == null || faction.IsPlayer || !faction.CanChangeGoodwillFor(Faction.OfPlayer, 1))
					{
						taggedString8 = "None".Translate();
					}
					else
					{
						bool isHealthy2;
						bool isInMentalState2;
						int i2 = faction.CalculateAdjustedGoodwillChange(Faction.OfPlayer, faction.GetGoodwillGainForPrisonerRelease(base.SelPawn, out isHealthy2, out isInMentalState2));
						taggedString8 = ((isHealthy2 && !isInMentalState2) ? (faction.NameColored + " " + i2.ToStringWithSign()) : ((!isHealthy2) ? ("None".Translate() + " (" + "UntendedInjury".Translate() + ")") : ((!isInMentalState2) ? "None".Translate() : ("None".Translate() + " (" + base.SelPawn.MentalState.InspectLine + ")"))));
					}
					Rect rect21 = listing_Standard.Label("SlaveReleasePotentialRelationGains".Translate() + ": " + taggedString8);
					TooltipHandler.TipRegionByKey(rect21, "SlaveReleaseRelationGainsDesc");
					Widgets.DrawHighlightIfMouseover(rect21);
					tmpSlaveInteractionModes.Clear();
					tmpSlaveInteractionModes.AddRange(DefDatabase<SlaveInteractionModeDef>.AllDefs.OrderBy((SlaveInteractionModeDef pim) => pim.listOrder));
					int num3 = 32 * tmpSlaveInteractionModes.Count;
					Rect rect22 = listing_Standard.GetRect(num3).Rounded();
					Widgets.DrawMenuSection(rect22);
					Rect rect23 = rect22.ContractedBy(10f);
					Widgets.BeginGroup(rect23);
					SlaveInteractionModeDef currentSlaveIteractionMode = base.SelPawn.guest.slaveInteractionMode;
					Rect rect24 = new Rect(0f, 0f, rect23.width, 30f);
					foreach (SlaveInteractionModeDef tmpSlaveInteractionMode in tmpSlaveInteractionModes)
					{
						if (Widgets.RadioButtonLabeled(rect24, tmpSlaveInteractionMode.LabelCap, base.SelPawn.guest.slaveInteractionMode == tmpSlaveInteractionMode))
						{
							if (tmpSlaveInteractionMode == SlaveInteractionModeDefOf.Imprison && RestUtility.FindBedFor(base.SelPawn, base.SelPawn, checkSocialProperness: false, ignoreOtherReservations: false, GuestStatus.Prisoner) == null)
							{
								Messages.Message("CannotImprison".Translate() + ": " + "NoPrisonerBed".Translate(), base.SelPawn, MessageTypeDefOf.RejectInput, historical: false);
								continue;
							}
							base.SelPawn.guest.slaveInteractionMode = tmpSlaveInteractionMode;
							if (tmpSlaveInteractionMode == SlaveInteractionModeDefOf.Execute && base.SelPawn.SlaveFaction != null && !base.SelPawn.SlaveFaction.HostileTo(Faction.OfPlayer))
							{
								Dialog_MessageBox window = new Dialog_MessageBox("ExectueNeutralFactionSlave".Translate(base.SelPawn.Named("PAWN"), base.SelPawn.SlaveFaction.Named("FACTION")), "Confirm".Translate(), delegate
								{
								}, "Cancel".Translate(), delegate
								{
									base.SelPawn.guest.slaveInteractionMode = currentSlaveIteractionMode;
								});
								Find.WindowStack.Add(window);
							}
						}
						if (!tmpSlaveInteractionMode.description.NullOrEmpty() && Mouse.IsOver(rect24))
						{
							Widgets.DrawHighlight(rect24);
							string text8 = tmpSlaveInteractionMode.description;
							if (tmpSlaveInteractionMode == SlaveInteractionModeDefOf.Emancipate)
							{
								text8 = ((base.SelPawn.SlaveFaction == Faction.OfPlayer) ? ((string)(text8 + (" " + "EmancipateCololonistTooltip".Translate()))) : ((base.SelPawn.SlaveFaction == null) ? ((string)(text8 + (" " + "EmancipateNonCololonistWithoutFactionTooltip".Translate()))) : ((string)(text8 + (" " + "EmancipateNonCololonistWithFactionTooltip".Translate(base.SelPawn.SlaveFaction.NameColored))))));
							}
							TooltipHandler.TipRegion(rect24, text8);
						}
						rect24.y += 28f;
					}
					Widgets.EndGroup();
					tmpSlaveInteractionModes.Clear();
				}
			}
			listing_Standard.End();
			size = new Vector2(280f, listing_Standard.CurHeight + 10f + 24f);
			bool CanUsePrisonerInteractionMode(Pawn pawn, PrisonerInteractionModeDef mode)
			{
				if (!pawn.guest.Recruitable && mode.hideIfNotRecruitable)
				{
					return false;
				}
				if (wildMan && !mode.allowOnWildMan)
				{
					return false;
				}
				if (mode.hideIfNoBloodfeeders && pawn.MapHeld != null && !ColonyHasAnyBloodfeeder(pawn.MapHeld))
				{
					return false;
				}
				if (mode.hideOnHemogenicPawns && ModsConfig.BiotechActive && pawn.genes != null && pawn.genes.HasGene(GeneDefOf.Hemogenic))
				{
					return false;
				}
				if (!mode.allowInClassicIdeoMode && Find.IdeoManager.classicMode)
				{
					return false;
				}
				return true;
			}
		}

		private void InteractionModeChanged(PrisonerInteractionModeDef oldMode, PrisonerInteractionModeDef newMode)
		{
			if (ModsConfig.BiotechActive)
			{
				Bill bill = base.SelPawn.BillStack?.Bills?.FirstOrDefault((Bill x) => x.recipe == RecipeDefOf.ExtractHemogenPack);
				if (newMode == PrisonerInteractionModeDefOf.HemogenFarm)
				{
					if (bill == null)
					{
						HealthCardUtility.CreateSurgeryBill(base.SelPawn, RecipeDefOf.ExtractHemogenPack, null);
					}
				}
				else if (oldMode == PrisonerInteractionModeDefOf.HemogenFarm && bill != null)
				{
					base.SelPawn.BillStack.Bills.Remove(bill);
				}
			}
			if (newMode == PrisonerInteractionModeDefOf.Execution && base.SelPawn.MapHeld != null && !ColonyHasAnyWardenCapableOfViolence(base.SelPawn.MapHeld))
			{
				Messages.Message("MessageCantDoExecutionBecauseNoWardenCapableOfViolence".Translate(), base.SelPawn, MessageTypeDefOf.CautionInput, historical: false);
			}
			if (newMode == PrisonerInteractionModeDefOf.Enslave && base.SelPawn.MapHeld != null && !ColonyHasAnyWardenCapableOfEnslavement(base.SelPawn.MapHeld))
			{
				Messages.Message("MessageNoWardenCapableOfEnslavement".Translate(), base.SelPawn, MessageTypeDefOf.CautionInput, historical: false);
			}
			if (newMode == PrisonerInteractionModeDefOf.Convert && base.SelPawn.guest.ideoForConversion == null)
			{
				base.SelPawn.guest.ideoForConversion = Faction.OfPlayer.ideos.PrimaryIdeo;
			}
		}

		private void DoSlavePriceListing(Listing_Standard listing, Pawn pawn)
		{
			float statValue = base.SelPawn.GetStatValue(StatDefOf.MarketValue);
			Rect rect = listing.Label("SlavePrice".Translate() + ": " + statValue.ToStringMoney());
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
				TaggedString taggedString = "SlavePriceDescription".Translate() + "\n\n" + StatDefOf.MarketValue.Worker.GetExplanationFull(StatRequest.For(base.SelPawn), StatDefOf.MarketValue.toStringNumberSense, statValue);
				TooltipHandler.TipRegion(rect, taggedString);
			}
		}

		private bool ColonyHasAnyBloodfeeder(Map map)
		{
			if (ModsConfig.BiotechActive)
			{
				foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
				{
					if (item.IsBloodfeeder())
					{
						return true;
					}
				}
				foreach (Pawn item2 in map.mapPawns.PrisonersOfColony)
				{
					if (item2.IsBloodfeeder())
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool ColonyHasAnyWardenCapableOfViolence(Map map)
		{
			foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
			{
				if (item.workSettings.WorkIsActive(WorkTypeDefOf.Warden) && !item.WorkTagIsDisabled(WorkTags.Violent))
				{
					return true;
				}
			}
			return false;
		}

		private bool ColonyHasAnyWardenCapableOfEnslavement(Map map)
		{
			foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
			{
				if (item.workSettings.WorkIsActive(WorkTypeDefOf.Warden) && new HistoryEvent(HistoryEventDefOf.EnslavedPrisoner, item.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo())
				{
					return true;
				}
			}
			return false;
		}

		private bool ColonyHasAnyWardenOfIdeo(Ideo ideo, Map map)
		{
			foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
			{
				if (item.workSettings.WorkIsActive(WorkTypeDefOf.Warden) && item.Ideo == ideo)
				{
					return true;
				}
			}
			return false;
		}
	}
}
