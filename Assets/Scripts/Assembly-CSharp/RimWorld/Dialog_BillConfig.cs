using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Dialog_BillConfig : Window
	{
		private IntVec3 billGiverPos;

		protected Bill_Production bill;

		private ThingFilterUI.UIState thingFilterState = new ThingFilterUI.UIState();

		private string repeatCountEditBuffer;

		private string targetCountEditBuffer;

		private string unpauseCountEditBuffer;

		protected const float RecipeIconSize = 34f;

		[TweakValue("Interface", 0f, 400f)]
		private static int RepeatModeSubdialogHeight = 324;

		[TweakValue("Interface", 0f, 400f)]
		private static int StoreModeSubdialogHeight = 30;

		[TweakValue("Interface", 0f, 400f)]
		private static int WorkerSelectionSubdialogHeight = 85;

		[TweakValue("Interface", 0f, 400f)]
		private static int IngredientRadiusSubdialogHeight = 50;

		private static List<SpecialThingFilterDef> cachedHiddenSpecialThingFilters;

		public override Vector2 InitialSize => new Vector2(800f, 634f);

		private static IEnumerable<SpecialThingFilterDef> HiddenSpecialThingFilters
		{
			get
			{
				if (cachedHiddenSpecialThingFilters == null)
				{
					cachedHiddenSpecialThingFilters = new List<SpecialThingFilterDef>();
					if (ModsConfig.IdeologyActive)
					{
						cachedHiddenSpecialThingFilters.Add(SpecialThingFilterDefOf.AllowCarnivore);
						cachedHiddenSpecialThingFilters.Add(SpecialThingFilterDefOf.AllowVegetarian);
						cachedHiddenSpecialThingFilters.Add(SpecialThingFilterDefOf.AllowCannibal);
						cachedHiddenSpecialThingFilters.Add(SpecialThingFilterDefOf.AllowInsectMeat);
					}
				}
				return cachedHiddenSpecialThingFilters;
			}
		}

		public Dialog_BillConfig(Bill_Production bill, IntVec3 billGiverPos)
		{
			this.billGiverPos = billGiverPos;
			this.bill = bill;
			forcePause = true;
			doCloseX = true;
			doCloseButton = true;
			absorbInputAroundWindow = true;
			closeOnClickedOutside = true;
		}

		public override void PreOpen()
		{
			base.PreOpen();
			thingFilterState.quickSearch.Reset();
		}

		private void AdjustCount(int offset)
		{
			SoundDefOf.DragSlider.PlayOneShotOnCamera();
			bill.repeatCount += offset;
			if (bill.repeatCount < 1)
			{
				bill.repeatCount = 1;
			}
		}

		public override void WindowUpdate()
		{
			bill.TryDrawIngredientSearchRadiusOnMap(billGiverPos);
		}

		protected override void LateWindowOnGUI(Rect inRect)
		{
			Rect rect = new Rect(inRect.x, inRect.y, 34f, 34f);
			ThingStyleDef thingStyleDef = null;
			if (ModsConfig.IdeologyActive && bill.recipe.ProducedThingDef != null)
			{
				thingStyleDef = ((!bill.globalStyle) ? bill.style : Faction.OfPlayer.ideos.PrimaryIdeo.style.StyleForThingDef(bill.recipe.ProducedThingDef)?.styleDef);
			}
			Widgets.DefIcon(rect, bill.recipe, null, 1f, thingStyleDef, drawPlaceholder: true, null, null, bill.graphicIndexOverride);
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(40f, 0f, 400f, 34f), bill.LabelCap);
			float width = (int)((inRect.width - 34f) / 3f);
			Rect rect = new Rect(0f, 80f, width, inRect.height - 80f);
			Rect rect2 = new Rect(rect.xMax + 17f, 50f, width, inRect.height - 50f - Window.CloseButSize.y);
			Rect rect3 = new Rect(rect2.xMax + 17f, 50f, 0f, inRect.height - 50f - Window.CloseButSize.y);
			rect3.xMax = inRect.xMax;
			Text.Font = GameFont.Small;
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.Begin(rect2);
			Listing_Standard listing_Standard2 = listing_Standard.BeginSection(RepeatModeSubdialogHeight);
			if (listing_Standard2.ButtonText(bill.repeatMode.LabelCap))
			{
				BillRepeatModeUtility.MakeConfigFloatMenu(bill);
			}
			listing_Standard2.Gap();
			if (bill.repeatMode == BillRepeatModeDefOf.RepeatCount)
			{
				listing_Standard2.Label("RepeatCount".Translate(bill.repeatCount));
				listing_Standard2.IntEntry(ref bill.repeatCount, ref repeatCountEditBuffer);
			}
			else if (bill.repeatMode == BillRepeatModeDefOf.TargetCount)
			{
				string text = "CurrentlyHave".Translate() + ": ";
				text += bill.recipe.WorkerCounter.CountProducts(bill);
				text += " / ";
				text += ((bill.targetCount < 999999) ? bill.targetCount.ToString() : "Infinite".Translate().ToLower().ToString());
				string str = bill.recipe.WorkerCounter.ProductsDescription(bill);
				if (!str.NullOrEmpty())
				{
					text += "\n" + "CountingProducts".Translate() + ": " + str.CapitalizeFirst();
				}
				listing_Standard2.Label(text);
				int targetCount = bill.targetCount;
				listing_Standard2.IntEntry(ref bill.targetCount, ref targetCountEditBuffer, bill.recipe.targetCountAdjustment);
				bill.unpauseWhenYouHave = Mathf.Max(0, bill.unpauseWhenYouHave + (bill.targetCount - targetCount));
				ThingDef producedThingDef = bill.recipe.ProducedThingDef;
				if (producedThingDef != null)
				{
					if (producedThingDef.IsWeapon || producedThingDef.IsApparel)
					{
						listing_Standard2.CheckboxLabeled("IncludeEquipped".Translate(), ref bill.includeEquipped);
					}
					if (producedThingDef.IsApparel && producedThingDef.apparel.careIfWornByCorpse)
					{
						listing_Standard2.CheckboxLabeled("IncludeTainted".Translate(), ref bill.includeTainted);
					}
					Widgets.Dropdown(listing_Standard2.GetRect(30f), bill, (Bill_Production b) => b.includeFromZone, (Bill_Production b) => GenerateStockpileInclusion(), (bill.includeFromZone == null) ? "IncludeFromAll".Translate() : "IncludeSpecific".Translate(bill.includeFromZone.label));
					if (bill.recipe.products.Any((ThingDefCountClass prod) => prod.thingDef.useHitPoints))
					{
						Widgets.FloatRange(listing_Standard2.GetRect(28f), 975643279, ref bill.hpRange, 0f, 1f, "HitPoints", ToStringStyle.PercentZero);
						bill.hpRange.min = Mathf.Round(bill.hpRange.min * 100f) / 100f;
						bill.hpRange.max = Mathf.Round(bill.hpRange.max * 100f) / 100f;
					}
					if (producedThingDef.HasComp(typeof(CompQuality)))
					{
						Widgets.QualityRange(listing_Standard2.GetRect(28f), 1098906561, ref bill.qualityRange);
					}
					if (producedThingDef.MadeFromStuff)
					{
						listing_Standard2.CheckboxLabeled("LimitToAllowedStuff".Translate(), ref bill.limitToAllowedStuff);
					}
				}
			}
			if (bill.repeatMode == BillRepeatModeDefOf.TargetCount)
			{
				listing_Standard2.CheckboxLabeled("PauseWhenSatisfied".Translate(), ref bill.pauseWhenSatisfied);
				if (bill.pauseWhenSatisfied)
				{
					listing_Standard2.Label("UnpauseWhenYouHave".Translate() + ": " + bill.unpauseWhenYouHave.ToString("F0"));
					listing_Standard2.IntEntry(ref bill.unpauseWhenYouHave, ref unpauseCountEditBuffer, bill.recipe.targetCountAdjustment);
					if (bill.unpauseWhenYouHave >= bill.targetCount)
					{
						bill.unpauseWhenYouHave = bill.targetCount - 1;
						unpauseCountEditBuffer = bill.unpauseWhenYouHave.ToStringCached();
					}
				}
			}
			listing_Standard.EndSection(listing_Standard2);
			listing_Standard.Gap();
			Listing_Standard listing_Standard3 = listing_Standard.BeginSection(StoreModeSubdialogHeight);
			string text2 = string.Format(bill.GetStoreMode().LabelCap, (bill.GetStoreZone() != null) ? bill.GetStoreZone().SlotYielderLabel() : "");
			if (bill.GetStoreZone() != null && !bill.recipe.WorkerCounter.CanPossiblyStoreInStockpile(bill, bill.GetStoreZone()))
			{
				text2 += string.Format(" ({0})", "IncompatibleLower".Translate());
				Text.Font = GameFont.Tiny;
			}
			if (listing_Standard3.ButtonText(text2))
			{
				Text.Font = GameFont.Small;
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (BillStoreModeDef item in DefDatabase<BillStoreModeDef>.AllDefs.OrderBy((BillStoreModeDef bsm) => bsm.listOrder))
				{
					if (item == BillStoreModeDefOf.SpecificStockpile)
					{
						List<SlotGroup> allGroupsListInPriorityOrder = bill.billStack.billGiver.Map.haulDestinationManager.AllGroupsListInPriorityOrder;
						int count = allGroupsListInPriorityOrder.Count;
						for (int i = 0; i < count; i++)
						{
							SlotGroup group = allGroupsListInPriorityOrder[i];
							if (!(group.parent is Zone_Stockpile stockpile))
							{
								continue;
							}
							if (!bill.recipe.WorkerCounter.CanPossiblyStoreInStockpile(bill, stockpile))
							{
								list.Add(new FloatMenuOption(string.Format("{0} ({1})", string.Format(item.LabelCap, group.parent.SlotYielderLabel()), "IncompatibleLower".Translate()), null));
								continue;
							}
							list.Add(new FloatMenuOption(string.Format(item.LabelCap, group.parent.SlotYielderLabel()), delegate
							{
								bill.SetStoreMode(BillStoreModeDefOf.SpecificStockpile, (Zone_Stockpile)group.parent);
							}));
						}
					}
					else
					{
						BillStoreModeDef smLocal = item;
						list.Add(new FloatMenuOption(smLocal.LabelCap, delegate
						{
							bill.SetStoreMode(smLocal);
						}));
					}
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			Text.Font = GameFont.Small;
			listing_Standard.EndSection(listing_Standard3);
			listing_Standard.Gap();
			Listing_Standard listing_Standard4 = listing_Standard.BeginSection(WorkerSelectionSubdialogHeight);
			Widgets.Dropdown(buttonLabel: (bill.PawnRestriction != null) ? bill.PawnRestriction.LabelShortCap : ((ModsConfig.IdeologyActive && bill.SlavesOnly) ? ((string)"AnySlave".Translate()) : ((ModsConfig.BiotechActive && bill.recipe.mechanitorOnlyRecipe) ? ((string)"AnyMechanitor".Translate()) : ((ModsConfig.BiotechActive && bill.MechsOnly) ? ((string)"AnyMech".Translate()) : ((!ModsConfig.BiotechActive || !bill.NonMechsOnly) ? ((string)"AnyWorker".Translate()) : ((string)"AnyNonMech".Translate()))))), rect: listing_Standard4.GetRect(30f), target: bill, getPayload: (Bill_Production b) => b.PawnRestriction, menuGenerator: (Bill_Production b) => GeneratePawnRestrictionOptions());
			if (bill.PawnRestriction == null && bill.recipe.workSkill != null && !bill.MechsOnly)
			{
				listing_Standard4.Label("AllowedSkillRange".Translate(bill.recipe.workSkill.label));
				listing_Standard4.IntRange(ref bill.allowedSkillRange, 0, 20);
			}
			listing_Standard.EndSection(listing_Standard4);
			listing_Standard.End();
			float y = rect3.y;
			DoIngredientConfigPane(rect3.x, ref y, rect3.width, rect3.height);
			Listing_Standard listing_Standard5 = new Listing_Standard();
			listing_Standard5.Begin(rect);
			if (bill.suspended)
			{
				if (listing_Standard5.ButtonText("Suspended".Translate()))
				{
					bill.suspended = false;
					SoundDefOf.Click.PlayOneShotOnCamera();
				}
			}
			else if (listing_Standard5.ButtonText("NotSuspended".Translate()))
			{
				bill.suspended = true;
				SoundDefOf.Click.PlayOneShotOnCamera();
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (bill.recipe.description != null)
			{
				stringBuilder.AppendLine(bill.recipe.description);
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine("WorkAmount".Translate() + ": " + bill.recipe.WorkAmountTotal(null).ToStringWorkAmount());
			if (ModsConfig.BiotechActive && bill.recipe.products.Count == 1)
			{
				ThingDef thingDef = bill.recipe.products[0].thingDef;
				if (thingDef.IsApparel)
				{
					stringBuilder.AppendLine("WearableBy".Translate() + ": " + thingDef.apparel.developmentalStageFilter.ToCommaList().CapitalizeFirst());
				}
			}
			if (bill is Bill_Mech bill_Mech)
			{
				stringBuilder.AppendLine((string)("GestationCycles".Translate() + ": ") + bill.recipe.gestationCycles);
				ThingDef thingDef2 = bill_Mech.Gestator.GestatingMech?.def ?? bill_Mech.recipe.ProducedThingDef;
				if (thingDef2 != null)
				{
					stringBuilder.AppendLine((string)("Bandwidth".Translate() + ": ") + thingDef2.GetStatValueAbstract(StatDefOf.BandwidthCost));
				}
				if (!bill.recipe.mechResurrection)
				{
					float num = (float)(int)bill.recipe.ProducedThingDef.GetStatValueAbstract(StatDefOf.WastepacksPerRecharge) * bill.recipe.ProducedThingDef.GetStatValueAbstract(StatDefOf.BandwidthCost);
					stringBuilder.AppendLine((string)(Find.ActiveLanguageWorker.Pluralize(ThingDefOf.Wastepack.LabelCap) + " " + "ThingsProduced".Translate() + ": ") + num);
				}
			}
			stringBuilder.AppendLine("BillRequires".Translate() + ": ");
			for (int j = 0; j < bill.recipe.ingredients.Count; j++)
			{
				IngredientCount ingredientCount = bill.recipe.ingredients[j];
				if (!ingredientCount.filter.Summary.NullOrEmpty())
				{
					stringBuilder.AppendLine(" - " + bill.recipe.IngredientValueGetter.BillRequirementsDescription(bill.recipe, ingredientCount));
				}
			}
			stringBuilder.AppendLine();
			string text3 = bill.recipe.IngredientValueGetter.ExtraDescriptionLine(bill.recipe);
			if (text3 != null)
			{
				stringBuilder.AppendLine(text3);
				stringBuilder.AppendLine();
			}
			if (!bill.recipe.skillRequirements.NullOrEmpty())
			{
				stringBuilder.AppendLine("MinimumSkills".Translate());
				stringBuilder.AppendLine(bill.recipe.MinSkillString);
			}
			Text.Font = GameFont.Small;
			string text4 = stringBuilder.ToString();
			if (Text.CalcHeight(text4, rect.width) > rect.height)
			{
				Text.Font = GameFont.Tiny;
			}
			listing_Standard5.Label(text4);
			Text.Font = GameFont.Small;
			if (ModsConfig.IdeologyActive && Find.IdeoManager.classicMode && bill.recipe.ProducedThingDef != null)
			{
				listing_Standard5.Gap(rect.height - listing_Standard5.CurHeight - 90f);
				ThingDef producedThingDef2 = bill.recipe.ProducedThingDef;
				List<StyleCategoryDef> relevantStyleCategories = bill.recipe.ProducedThingDef.RelevantStyleCategories;
				if (relevantStyleCategories.Any())
				{
					StyleCategoryPair global = Faction.OfPlayer.ideos.PrimaryIdeo.style.StyleForThingDef(producedThingDef2);
					string text5 = ((global == null) ? "Basic".Translate().CapitalizeFirst() : global.category.LabelCap);
					string text6 = ((bill.style != null) ? bill.style.Category.LabelCap : "Basic".Translate().CapitalizeFirst());
					string text7 = (bill.globalStyle ? ("UseGlobalStyle".Translate().ToString() + " (" + text5 + ")") : text6);
					if (!bill.globalStyle && bill.style != null && bill.graphicIndexOverride.HasValue)
					{
						text7 = text7 + " " + (bill.graphicIndexOverride.Value + 1);
					}
					if (listing_Standard5.ButtonText(text7))
					{
						GenStuff.DefaultStuffFor(producedThingDef2);
						List<FloatMenuOption> list2 = new List<FloatMenuOption>();
						list2.Add(new FloatMenuOption("UseGlobalStyle".Translate() + " (" + text5 + ")", delegate
						{
							bill.style = global?.styleDef;
							bill.globalStyle = true;
							bill.graphicIndexOverride = null;
						}, bill.recipe.UIIconThing, global?.styleDef));
						list2.Add(new FloatMenuOption("Basic".Translate().CapitalizeFirst(), delegate
						{
							bill.style = null;
							bill.globalStyle = false;
							bill.graphicIndexOverride = null;
						}, bill.recipe.UIIconThing, null, forceBasicStyle: true));
						foreach (StyleCategoryDef item2 in relevantStyleCategories)
						{
							foreach (ThingDefStyle style in item2.thingDefStyles)
							{
								if (producedThingDef2 != style.ThingDef)
								{
									continue;
								}
								if (style.StyleDef.Graphic is Graphic_Random graphic_Random)
								{
									for (int k = 0; k < graphic_Random.SubGraphicsCount; k++)
									{
										int index = k;
										list2.Add(new FloatMenuOption((string)(item2.LabelCap + " ") + (index + 1), delegate
										{
											bill.style = style.StyleDef;
											bill.globalStyle = false;
											bill.graphicIndexOverride = index;
										}, bill.recipe.UIIconThing, style.StyleDef, forceBasicStyle: false, MenuOptionPriority.Default, null, null, 0f, null, null, playSelectionSound: true, 0, index));
									}
								}
								else
								{
									list2.Add(new FloatMenuOption(item2.LabelCap, delegate
									{
										bill.style = style.StyleDef;
										bill.globalStyle = false;
										bill.graphicIndexOverride = null;
									}, bill.recipe.UIIconThing, style.StyleDef));
								}
							}
						}
						Find.WindowStack.Add(new FloatMenu(list2));
					}
				}
				else
				{
					Rect rect4 = listing_Standard5.GetRect(30f);
					Widgets.DrawHighlight(rect4);
					Text.Anchor = TextAnchor.MiddleCenter;
					Widgets.Label(rect4, "NoStylesAvailable".Translate());
					Text.Anchor = TextAnchor.UpperLeft;
				}
			}
			listing_Standard5.End();
			if (bill.recipe.products.Count == 1)
			{
				ThingDef thingDef3 = bill.recipe.products[0].thingDef;
				Widgets.InfoCardButton(rect.x, rect3.y, thingDef3, GenStuff.DefaultStuffFor(thingDef3));
			}
		}

		protected virtual void DoIngredientConfigPane(float x, ref float y, float width, float height)
		{
			bool flag = true;
			for (int i = 0; i < bill.recipe.ingredients.Count; i++)
			{
				if (!bill.recipe.ingredients[i].IsFixedIngredient)
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				Rect rect = new Rect(x, y, width, height - (float)IngredientRadiusSubdialogHeight);
				bool num = bill.GetStoreZone() == null || bill.recipe.WorkerCounter.CanPossiblyStoreInStockpile(bill, bill.GetStoreZone());
				ThingFilterUI.DoThingFilterConfigWindow(rect, thingFilterState, bill.ingredientFilter, bill.recipe.fixedIngredientFilter, 4, null, HiddenSpecialThingFilters.ConcatIfNotNull(bill.recipe.forceHiddenSpecialFilters), forceHideHitPointsConfig: false, bill.recipe.GetPremultipliedSmallIngredients(), bill.Map);
				y += rect.height;
				bool flag2 = bill.GetStoreZone() == null || bill.recipe.WorkerCounter.CanPossiblyStoreInStockpile(bill, bill.GetStoreZone());
				if (num && !flag2)
				{
					Messages.Message("MessageBillValidationStoreZoneInsufficient".Translate(bill.LabelCap, bill.billStack.billGiver.LabelShort.CapitalizeFirst(), bill.GetStoreZone().label), bill.billStack.billGiver as Thing, MessageTypeDefOf.RejectInput, historical: false);
				}
			}
			Rect rect2 = new Rect(x, y, width, IngredientRadiusSubdialogHeight);
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.Begin(rect2);
			string text = "IngredientSearchRadius".Translate().Truncate(rect2.width * 0.6f);
			string text2 = ((bill.ingredientSearchRadius == 999f) ? "Unlimited".TranslateSimple().Truncate(rect2.width * 0.3f) : bill.ingredientSearchRadius.ToString("F0"));
			listing_Standard.Label(text + ": " + text2);
			bill.ingredientSearchRadius = listing_Standard.Slider((bill.ingredientSearchRadius > 100f) ? 100f : bill.ingredientSearchRadius, 3f, 100f);
			if (bill.ingredientSearchRadius >= 100f)
			{
				bill.ingredientSearchRadius = 999f;
			}
			listing_Standard.End();
			y += IngredientRadiusSubdialogHeight;
		}

		protected virtual IEnumerable<Widgets.DropdownMenuElement<Pawn>> GeneratePawnRestrictionOptions()
		{
			if (ModsConfig.BiotechActive && bill.recipe.mechanitorOnlyRecipe)
			{
				yield return new Widgets.DropdownMenuElement<Pawn>
				{
					option = new FloatMenuOption("AnyMechanitor".Translate(), delegate
					{
						bill.SetAnyPawnRestriction();
					}),
					payload = null
				};
				foreach (Widgets.DropdownMenuElement<Pawn> item in BillDialogUtility.GetPawnRestrictionOptionsForBill(bill, (Pawn p) => MechanitorUtility.IsMechanitor(p)))
				{
					yield return item;
				}
				yield break;
			}
			yield return new Widgets.DropdownMenuElement<Pawn>
			{
				option = new FloatMenuOption("AnyWorker".Translate(), delegate
				{
					bill.SetAnyPawnRestriction();
				}),
				payload = null
			};
			if (ModsConfig.IdeologyActive)
			{
				yield return new Widgets.DropdownMenuElement<Pawn>
				{
					option = new FloatMenuOption("AnySlave".Translate(), delegate
					{
						bill.SetAnySlaveRestriction();
					}),
					payload = null
				};
			}
			if (ModsConfig.BiotechActive && MechWorkUtility.AnyWorkMechCouldDo(bill.recipe))
			{
				yield return new Widgets.DropdownMenuElement<Pawn>
				{
					option = new FloatMenuOption("AnyMech".Translate(), delegate
					{
						bill.SetAnyMechRestriction();
					}),
					payload = null
				};
				yield return new Widgets.DropdownMenuElement<Pawn>
				{
					option = new FloatMenuOption("AnyNonMech".Translate(), delegate
					{
						bill.SetAnyNonMechRestriction();
					}),
					payload = null
				};
			}
			foreach (Widgets.DropdownMenuElement<Pawn> item2 in BillDialogUtility.GetPawnRestrictionOptionsForBill(bill))
			{
				yield return item2;
			}
		}

		private IEnumerable<Widgets.DropdownMenuElement<Zone_Stockpile>> GenerateStockpileInclusion()
		{
			yield return new Widgets.DropdownMenuElement<Zone_Stockpile>
			{
				option = new FloatMenuOption("IncludeFromAll".Translate(), delegate
				{
					bill.includeFromZone = null;
				}),
				payload = null
			};
			List<SlotGroup> groupList = bill.billStack.billGiver.Map.haulDestinationManager.AllGroupsListInPriorityOrder;
			int groupCount = groupList.Count;
			int i = 0;
			while (i < groupCount)
			{
				SlotGroup slotGroup = groupList[i];
				Zone_Stockpile stockpile = slotGroup.parent as Zone_Stockpile;
				if (stockpile != null)
				{
					if (!bill.recipe.WorkerCounter.CanPossiblyStoreInStockpile(bill, stockpile))
					{
						yield return new Widgets.DropdownMenuElement<Zone_Stockpile>
						{
							option = new FloatMenuOption(string.Format("{0} ({1})", "IncludeSpecific".Translate(slotGroup.parent.SlotYielderLabel()), "IncompatibleLower".Translate()), null),
							payload = stockpile
						};
					}
					else
					{
						yield return new Widgets.DropdownMenuElement<Zone_Stockpile>
						{
							option = new FloatMenuOption("IncludeSpecific".Translate(slotGroup.parent.SlotYielderLabel()), delegate
							{
								bill.includeFromZone = stockpile;
							}),
							payload = stockpile
						};
					}
				}
				int num = i + 1;
				i = num;
			}
		}
	}
}
