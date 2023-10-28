using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public static class GenRecipe
	{
		public static IEnumerable<Thing> MakeRecipeProducts(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient, IBillGiver billGiver, Precept_ThingStyle precept = null, ThingStyleDef style = null, int? overrideGraphicIndex = null)
		{
			float efficiency = ((recipeDef.efficiencyStat != null) ? worker.GetStatValue(recipeDef.efficiencyStat) : 1f);
			if (recipeDef.workTableEfficiencyStat != null && billGiver is Building_WorkTable thing)
			{
				efficiency *= thing.GetStatValue(recipeDef.workTableEfficiencyStat);
			}
			if (recipeDef.products != null)
			{
				for (int j = 0; j < recipeDef.products.Count; j++)
				{
					ThingDefCountClass thingDefCountClass = recipeDef.products[j];
					Thing thing2 = ThingMaker.MakeThing(stuff: (!thingDefCountClass.thingDef.MadeFromStuff) ? null : dominantIngredient.def, def: thingDefCountClass.thingDef);
					thing2.stackCount = Mathf.CeilToInt((float)thingDefCountClass.count * efficiency);
					if (dominantIngredient != null && recipeDef.useIngredientsForColor)
					{
						thing2.SetColor(dominantIngredient.DrawColor, reportFailure: false);
					}
					CompIngredients compIngredients = thing2.TryGetComp<CompIngredients>();
					if (compIngredients != null)
					{
						for (int l = 0; l < ingredients.Count; l++)
						{
							compIngredients.RegisterIngredient(ingredients[l].def);
						}
					}
					CompFoodPoisonable compFoodPoisonable = thing2.TryGetComp<CompFoodPoisonable>();
					if (compFoodPoisonable != null)
					{
						if (Rand.Chance(worker.GetRoom()?.GetStat(RoomStatDefOf.FoodPoisonChance) ?? RoomStatDefOf.FoodPoisonChance.roomlessScore))
						{
							compFoodPoisonable.SetPoisoned(FoodPoisonCause.FilthyKitchen);
						}
						else if (Rand.Chance(worker.GetStatValue(StatDefOf.FoodPoisonChance)))
						{
							compFoodPoisonable.SetPoisoned(FoodPoisonCause.IncompetentCook);
						}
					}
					yield return PostProcessProduct(thing2, recipeDef, worker, precept, style, overrideGraphicIndex);
				}
			}
			if (recipeDef.specialProducts == null)
			{
				yield break;
			}
			for (int j = 0; j < recipeDef.specialProducts.Count; j++)
			{
				for (int k = 0; k < ingredients.Count; k++)
				{
					Thing thing3 = ingredients[k];
					switch (recipeDef.specialProducts[j])
					{
					case SpecialProductType.Butchery:
						foreach (Thing item in thing3.ButcherProducts(worker, efficiency))
						{
							yield return PostProcessProduct(item, recipeDef, worker, precept, style, overrideGraphicIndex);
						}
						break;
					case SpecialProductType.Smelted:
						foreach (Thing item2 in thing3.SmeltProducts(efficiency))
						{
							yield return PostProcessProduct(item2, recipeDef, worker, precept, style, overrideGraphicIndex);
						}
						break;
					}
				}
			}
		}

		public static IEnumerable<Thing> FinalizeGestatedPawns(Bill_Mech bill, Pawn worker, ThingStyleDef style = null, int? overrideGraphicIndex = null)
		{
			yield return PostProcessProduct(bill.Gestator.GestatingMech, bill.recipe, worker, bill.precept, style, overrideGraphicIndex);
		}

		private static Thing PostProcessProduct(Thing product, RecipeDef recipeDef, Pawn worker, Precept_ThingStyle precept = null, ThingStyleDef style = null, int? overrideGraphicIndex = null)
		{
			CompQuality compQuality = product.TryGetComp<CompQuality>();
			if (compQuality != null)
			{
				if (recipeDef.workSkill == null)
				{
					Log.Error(string.Concat(recipeDef, " needs workSkill because it creates a product with a quality."));
				}
				QualityCategory q = QualityUtility.GenerateQualityCreatedByPawn(worker, recipeDef.workSkill);
				compQuality.SetQuality(q, ArtGenerationContext.Colony);
				QualityUtility.SendCraftNotification(product, worker);
			}
			CompArt compArt = product.TryGetComp<CompArt>();
			if (compArt != null)
			{
				compArt.JustCreatedBy(worker);
				if (compQuality != null && (int)compQuality.Quality >= 4)
				{
					TaleRecorder.RecordTale(TaleDefOf.CraftedArt, worker, product);
				}
			}
			if (worker.Ideo != null)
			{
				product.StyleDef = worker.Ideo.GetStyleFor(product.def);
			}
			if (precept != null)
			{
				product.StyleSourcePrecept = precept;
			}
			else if (style != null)
			{
				product.StyleDef = style;
			}
			product.overrideGraphicIndex = overrideGraphicIndex;
			if (product.def.Minifiable)
			{
				product = product.MakeMinified();
			}
			return product;
		}
	}
}
