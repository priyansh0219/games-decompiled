using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_ManageFoodRestrictions : Window
	{
		private ThingFilterUI.UIState thingFilterState = new ThingFilterUI.UIState();

		private FoodRestriction selFoodRestrictionInt;

		private const float TopAreaHeight = 40f;

		private const float TopButtonHeight = 35f;

		private const float TopButtonWidth = 150f;

		private static ThingFilter foodGlobalFilter;

		public static ThingFilter FoodGlobalFilter
		{
			get
			{
				if (foodGlobalFilter == null)
				{
					foodGlobalFilter = new ThingFilter();
					foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.GetStatValueAbstract(StatDefOf.Nutrition) > 0f))
					{
						foodGlobalFilter.SetAllow(item, allow: true);
					}
				}
				return foodGlobalFilter;
			}
		}

		private FoodRestriction SelectedFoodRestriction
		{
			get
			{
				return selFoodRestrictionInt;
			}
			set
			{
				CheckSelectedFoodRestrictionHasName();
				selFoodRestrictionInt = value;
			}
		}

		public override Vector2 InitialSize => new Vector2(700f, 700f);

		private void CheckSelectedFoodRestrictionHasName()
		{
			if (SelectedFoodRestriction != null && SelectedFoodRestriction.label.NullOrEmpty())
			{
				SelectedFoodRestriction.label = "Unnamed";
			}
		}

		public Dialog_ManageFoodRestrictions(FoodRestriction selectedFoodRestriction)
		{
			forcePause = true;
			doCloseX = true;
			doCloseButton = true;
			closeOnClickedOutside = true;
			absorbInputAroundWindow = true;
			SelectedFoodRestriction = selectedFoodRestriction;
		}

		public override void PreOpen()
		{
			base.PreOpen();
			thingFilterState.quickSearch.Reset();
		}

		public override void DoWindowContents(Rect inRect)
		{
			float num = 0f;
			Rect rect = new Rect(0f, 0f, 150f, 35f);
			num += 150f;
			if (Widgets.ButtonText(rect, "SelectFoodRestriction".Translate()))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (FoodRestriction allFoodRestriction in Current.Game.foodRestrictionDatabase.AllFoodRestrictions)
				{
					FoodRestriction localRestriction2 = allFoodRestriction;
					list.Add(new FloatMenuOption(localRestriction2.label, delegate
					{
						SelectedFoodRestriction = localRestriction2;
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			num += 10f;
			Rect rect2 = new Rect(num, 0f, 150f, 35f);
			num += 150f;
			if (Widgets.ButtonText(rect2, "NewFoodRestriction".Translate()))
			{
				SelectedFoodRestriction = Current.Game.foodRestrictionDatabase.MakeNewFoodRestriction();
			}
			num += 10f;
			Rect rect3 = new Rect(num, 0f, 150f, 35f);
			num += 150f;
			if (Widgets.ButtonText(rect3, "DeleteFoodRestriction".Translate()))
			{
				List<FloatMenuOption> list2 = new List<FloatMenuOption>();
				foreach (FoodRestriction allFoodRestriction2 in Current.Game.foodRestrictionDatabase.AllFoodRestrictions)
				{
					FoodRestriction localRestriction = allFoodRestriction2;
					list2.Add(new FloatMenuOption(localRestriction.label, delegate
					{
						AcceptanceReport acceptanceReport = Current.Game.foodRestrictionDatabase.TryDelete(localRestriction);
						if (!acceptanceReport.Accepted)
						{
							Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.RejectInput, historical: false);
						}
						else if (localRestriction == SelectedFoodRestriction)
						{
							SelectedFoodRestriction = null;
						}
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list2));
			}
			Rect rect4 = new Rect(0f, 40f, inRect.width, inRect.height - 40f - Window.CloseButSize.y).ContractedBy(10f);
			if (SelectedFoodRestriction == null)
			{
				GUI.color = Color.grey;
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(rect4, "NoFoodRestrictionSelected".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
			}
			else
			{
				Widgets.BeginGroup(rect4);
				DoNameInputRect(new Rect(0f, 0f, 200f, 30f), ref SelectedFoodRestriction.label);
				ThingFilterUI.DoThingFilterConfigWindow(new Rect(0f, 40f, 300f, rect4.height - 45f - 10f), thingFilterState, SelectedFoodRestriction.filter, FoodGlobalFilter, 1, null, HiddenSpecialThingFilters(), forceHideHitPointsConfig: true);
				Widgets.EndGroup();
			}
		}

		private IEnumerable<SpecialThingFilterDef> HiddenSpecialThingFilters()
		{
			yield return SpecialThingFilterDefOf.AllowFresh;
		}

		public override void PreClose()
		{
			base.PreClose();
			CheckSelectedFoodRestrictionHasName();
		}

		public static void DoNameInputRect(Rect rect, ref string name)
		{
			name = Widgets.TextField(rect, name, 30, Outfit.ValidNameRegex);
		}
	}
}
