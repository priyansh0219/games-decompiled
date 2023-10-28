using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class FoodRestrictionDatabase : IExposable
	{
		private List<FoodRestriction> foodRestrictions = new List<FoodRestriction>();

		public List<FoodRestriction> AllFoodRestrictions => foodRestrictions;

		public FoodRestrictionDatabase()
		{
			GenerateStartingFoodRestrictions();
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref foodRestrictions, "foodRestrictions", LookMode.Deep);
			BackCompatibility.PostExposeData(this);
		}

		public FoodRestriction DefaultFoodRestriction()
		{
			if (foodRestrictions.Count == 0)
			{
				MakeNewFoodRestriction();
			}
			return foodRestrictions[0];
		}

		public AcceptanceReport TryDelete(FoodRestriction foodRestriction)
		{
			foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
			{
				if (item.foodRestriction != null && item.foodRestriction.CurrentFoodRestriction == foodRestriction)
				{
					return new AcceptanceReport("FoodRestrictionInUse".Translate(item));
				}
			}
			foreach (Pawn item2 in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
			{
				if (item2.foodRestriction != null && item2.foodRestriction.CurrentFoodRestriction == foodRestriction)
				{
					item2.foodRestriction.CurrentFoodRestriction = null;
				}
			}
			foodRestrictions.Remove(foodRestriction);
			return AcceptanceReport.WasAccepted;
		}

		public FoodRestriction MakeNewFoodRestriction()
		{
			int id = ((!foodRestrictions.Any()) ? 1 : (foodRestrictions.Max((FoodRestriction o) => o.id) + 1));
			FoodRestriction foodRestriction = new FoodRestriction(id, "FoodRestriction".Translate() + " " + id.ToString());
			foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.GetStatValueAbstract(StatDefOf.Nutrition) > 0f))
			{
				foodRestriction.filter.SetAllow(item, allow: true);
			}
			foodRestrictions.Add(foodRestriction);
			if (ModsConfig.IdeologyActive)
			{
				foodRestriction.filter.SetAllow(SpecialThingFilterDefOf.AllowVegetarian, allow: true);
				foodRestriction.filter.SetAllow(SpecialThingFilterDefOf.AllowCarnivore, allow: true);
				foodRestriction.filter.SetAllow(SpecialThingFilterDefOf.AllowCannibal, allow: true);
				foodRestriction.filter.SetAllow(SpecialThingFilterDefOf.AllowInsectMeat, allow: true);
			}
			if (ModsConfig.BiotechActive)
			{
				foodRestriction.filter.SetAllow(ThingDefOf.HemogenPack, allow: false);
			}
			return foodRestriction;
		}

		private void GenerateStartingFoodRestrictions()
		{
			MakeNewFoodRestriction().label = "FoodRestrictionLavish".Translate();
			FoodRestriction foodRestriction = MakeNewFoodRestriction();
			foodRestriction.label = "FoodRestrictionFine".Translate();
			foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef.ingestible != null && (int)allDef.ingestible.preferability >= 10 && allDef != ThingDefOf.InsectJelly)
				{
					foodRestriction.filter.SetAllow(allDef, allow: false);
				}
			}
			if (ModsConfig.BiotechActive)
			{
				foodRestriction.filter.SetAllow(ThingDefOf.HemogenPack, allow: false);
			}
			FoodRestriction foodRestriction2 = MakeNewFoodRestriction();
			foodRestriction2.label = "FoodRestrictionSimple".Translate();
			foreach (ThingDef allDef2 in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef2.ingestible != null && (int)allDef2.ingestible.preferability >= 9 && allDef2 != ThingDefOf.InsectJelly)
				{
					foodRestriction2.filter.SetAllow(allDef2, allow: false);
				}
			}
			foodRestriction2.filter.SetAllow(ThingDefOf.MealSurvivalPack, allow: false);
			if (ModsConfig.BiotechActive)
			{
				foodRestriction2.filter.SetAllow(ThingDefOf.HemogenPack, allow: false);
			}
			FoodRestriction foodRestriction3 = MakeNewFoodRestriction();
			foodRestriction3.label = "FoodRestrictionPaste".Translate();
			foreach (ThingDef allDef3 in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef3.ingestible != null && (int)allDef3.ingestible.preferability >= 8 && allDef3 != ThingDefOf.MealNutrientPaste && allDef3 != ThingDefOf.InsectJelly && allDef3 != ThingDefOf.Pemmican)
				{
					foodRestriction3.filter.SetAllow(allDef3, allow: false);
				}
			}
			if (ModsConfig.BiotechActive)
			{
				foodRestriction3.filter.SetAllow(ThingDefOf.HemogenPack, allow: false);
			}
			FoodRestriction foodRestriction4 = MakeNewFoodRestriction();
			foodRestriction4.label = "FoodRestrictionRaw".Translate();
			foreach (ThingDef allDef4 in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef4.ingestible != null && (int)allDef4.ingestible.preferability >= 7)
				{
					foodRestriction4.filter.SetAllow(allDef4, allow: false);
				}
			}
			foodRestriction4.filter.SetAllow(ThingDefOf.Chocolate, allow: false);
			if (ModsConfig.BiotechActive)
			{
				foodRestriction4.filter.SetAllow(ThingDefOf.HemogenPack, allow: false);
			}
			FoodRestriction foodRestriction5 = MakeNewFoodRestriction();
			foodRestriction5.label = "FoodRestrictionNothing".Translate();
			foodRestriction5.filter.SetDisallowAll();
			CreateIdeologyFoodRestrictions();
		}

		public void CreateIdeologyFoodRestrictions()
		{
			if (!ModsConfig.IdeologyActive)
			{
				return;
			}
			TaggedString vegLabel = "FoodRestrictionVegetarian".Translate();
			if (foodRestrictions.FirstOrDefault((FoodRestriction fr) => fr.label == vegLabel) == null)
			{
				FoodRestriction foodRestriction = MakeNewFoodRestriction();
				foodRestriction.label = vegLabel;
				foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
				{
					if (FoodUtility.UnacceptableVegetarian(allDef))
					{
						foodRestriction.filter.SetAllow(allDef, allow: false);
					}
				}
				foodRestriction.filter.SetAllow(SpecialThingFilterDefOf.AllowCarnivore, allow: false);
				foodRestriction.filter.SetAllow(SpecialThingFilterDefOf.AllowCannibal, allow: false);
				foodRestriction.filter.SetAllow(SpecialThingFilterDefOf.AllowInsectMeat, allow: false);
				if (ModsConfig.BiotechActive)
				{
					foodRestriction.filter.SetAllow(ThingDefOf.HemogenPack, allow: false);
				}
			}
			TaggedString carnivoreLabel = "FoodRestrictionCarnivore".Translate();
			if (foodRestrictions.FirstOrDefault((FoodRestriction fr) => fr.label == carnivoreLabel) == null)
			{
				FoodRestriction foodRestriction2 = MakeNewFoodRestriction();
				foodRestriction2.label = carnivoreLabel;
				foreach (ThingDef allDef2 in DefDatabase<ThingDef>.AllDefs)
				{
					if (!FoodUtility.UnacceptableCarnivore(allDef2) && FoodUtility.GetMeatSourceCategory(allDef2) != MeatSourceCategory.Humanlike)
					{
						if (!allDef2.IsCorpse)
						{
							continue;
						}
						ThingDef sourceDef = allDef2.ingestible.sourceDef;
						if (sourceDef == null || sourceDef.race?.Humanlike != true)
						{
							continue;
						}
					}
					foodRestriction2.filter.SetAllow(allDef2, allow: false);
				}
				foodRestriction2.filter.SetAllow(SpecialThingFilterDefOf.AllowVegetarian, allow: false);
				foodRestriction2.filter.SetAllow(SpecialThingFilterDefOf.AllowCannibal, allow: false);
				foodRestriction2.filter.SetAllow(SpecialThingFilterDefOf.AllowInsectMeat, allow: false);
				if (ModsConfig.BiotechActive)
				{
					foodRestriction2.filter.SetAllow(ThingDefOf.HemogenPack, allow: false);
				}
			}
			TaggedString cannibalLabel = "FoodRestrictionCannibal".Translate();
			if (foodRestrictions.FirstOrDefault((FoodRestriction fr) => fr.label == cannibalLabel) == null)
			{
				FoodRestriction foodRestriction3 = MakeNewFoodRestriction();
				foodRestriction3.label = cannibalLabel;
				foreach (ThingDef allDef3 in DefDatabase<ThingDef>.AllDefs)
				{
					if (!FoodUtility.MaybeAcceptableCannibalDef(allDef3))
					{
						foodRestriction3.filter.SetAllow(allDef3, allow: false);
					}
				}
				foodRestriction3.filter.SetAllow(SpecialThingFilterDefOf.AllowVegetarian, allow: false);
				foodRestriction3.filter.SetAllow(SpecialThingFilterDefOf.AllowCarnivore, allow: false);
				foodRestriction3.filter.SetAllow(SpecialThingFilterDefOf.AllowInsectMeat, allow: false);
				if (ModsConfig.BiotechActive)
				{
					foodRestriction3.filter.SetAllow(ThingDefOf.HemogenPack, allow: false);
				}
			}
			TaggedString insectMeatLabel = "FoodRestrictionInsectMeat".Translate();
			if (foodRestrictions.FirstOrDefault((FoodRestriction fr) => fr.label == insectMeatLabel) != null)
			{
				return;
			}
			FoodRestriction foodRestriction4 = MakeNewFoodRestriction();
			foodRestriction4.label = insectMeatLabel;
			foreach (ThingDef allDef4 in DefDatabase<ThingDef>.AllDefs)
			{
				if (!FoodUtility.MaybeAcceptableInsectMeatEatersDef(allDef4))
				{
					foodRestriction4.filter.SetAllow(allDef4, allow: false);
				}
			}
			foodRestriction4.filter.SetAllow(SpecialThingFilterDefOf.AllowVegetarian, allow: false);
			foodRestriction4.filter.SetAllow(SpecialThingFilterDefOf.AllowCarnivore, allow: false);
			foodRestriction4.filter.SetAllow(SpecialThingFilterDefOf.AllowCannibal, allow: false);
			if (ModsConfig.BiotechActive)
			{
				foodRestriction4.filter.SetAllow(ThingDefOf.HemogenPack, allow: false);
			}
		}
	}
}
