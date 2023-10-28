using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public static class DebugOutputsMisc
	{
		[DebugOutput]
		public static void DrawerTypes()
		{
			DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefs, new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("category", (ThingDef d) => d.category.ToString()), new TableDataGetter<ThingDef>("drawType", (ThingDef d) => d.drawerType.ToString()), new TableDataGetter<ThingDef>("thingClass", (ThingDef d) => d.thingClass.Name), new TableDataGetter<ThingDef>("draw() users (comps)", (ThingDef d) => GetDrawUsers(d)), new TableDataGetter<ThingDef>("potential issue", (ThingDef d) => (d.drawerType != DrawerType.MapMeshOnly || string.IsNullOrWhiteSpace(GetDrawUsers(d))) ? "No" : "Yes"));
			string GetDrawUsers(ThingDef def)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (CompProperties comp in def.comps)
				{
					MethodInfo method = comp.compClass.GetMethod("PostDraw");
					if (method.DeclaringType != typeof(ThingComp))
					{
						stringBuilder.AppendLine(method.DeclaringType.Name);
					}
				}
				return stringBuilder.ToString().TrimEndNewlines().Replace(Environment.NewLine, ",");
			}
		}

		[DebugOutput]
		public static void DynamicDrawnThingsNow()
		{
			DebugTables.MakeTablesDialog((from t in Find.CurrentMap.dynamicDrawManager.DrawThingsForReading
				group t by t.def).ToList(), new TableDataGetter<IGrouping<ThingDef, Thing>>("defName", (IGrouping<ThingDef, Thing> d) => d.Key.defName), new TableDataGetter<IGrouping<ThingDef, Thing>>("count", (IGrouping<ThingDef, Thing> d) => d.Count()));
		}

		[DebugOutput]
		public static void DynamicDrawnThingsByCategoryNow()
		{
			DebugTables.MakeTablesDialog((from t in Find.CurrentMap.dynamicDrawManager.DrawThingsForReading
				group t by t.def.category).ToList(), new TableDataGetter<IGrouping<ThingCategory, Thing>>("category", (IGrouping<ThingCategory, Thing> d) => d.Key.ToString()), new TableDataGetter<IGrouping<ThingCategory, Thing>>("count", (IGrouping<ThingCategory, Thing> d) => d.Count()));
		}

		[DebugOutput]
		public static void MiningResourceGeneration()
		{
			Func<ThingDef, ThingDef> mineable = delegate(ThingDef d)
			{
				List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
				for (int i = 0; i < allDefsListForReading.Count; i++)
				{
					if (allDefsListForReading[i].building != null && allDefsListForReading[i].building.mineableThing == d)
					{
						return allDefsListForReading[i];
					}
				}
				return null;
			};
			Func<ThingDef, float> mineableCommonality = (ThingDef d) => (mineable(d) != null) ? mineable(d).building.mineableScatterCommonality : 0f;
			Func<ThingDef, IntRange> mineableLumpSizeRange = (ThingDef d) => (mineable(d) != null) ? mineable(d).building.mineableScatterLumpSizeRange : IntRange.zero;
			Func<ThingDef, float> mineableYield = (ThingDef d) => (mineable(d) != null) ? ((float)mineable(d).building.EffectiveMineableYield) : 0f;
			DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.deepCommonality > 0f || mineableCommonality(d) > 0f), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("market value", (ThingDef d) => d.BaseMarketValue.ToString("F2")), new TableDataGetter<ThingDef>("stackLimit", (ThingDef d) => d.stackLimit), new TableDataGetter<ThingDef>("deep\ncommonality", (ThingDef d) => d.deepCommonality.ToString("F2")), new TableDataGetter<ThingDef>("deep\nlump size", (ThingDef d) => d.deepLumpSizeRange), new TableDataGetter<ThingDef>("deep lump\nvalue min", (ThingDef d) => ((float)d.deepLumpSizeRange.min * d.BaseMarketValue * (float)d.deepCountPerCell).ToStringMoney()), new TableDataGetter<ThingDef>("deep lump\nvalue avg", (ThingDef d) => (d.deepLumpSizeRange.Average * d.BaseMarketValue * (float)d.deepCountPerCell).ToStringMoney()), new TableDataGetter<ThingDef>("deep lump\nvalue max", (ThingDef d) => ((float)d.deepLumpSizeRange.max * d.BaseMarketValue * (float)d.deepCountPerCell).ToStringMoney()), new TableDataGetter<ThingDef>("deep count\nper cell", (ThingDef d) => d.deepCountPerCell), new TableDataGetter<ThingDef>("deep count\nper portion", (ThingDef d) => d.deepCountPerPortion), new TableDataGetter<ThingDef>("deep portion\nvalue", (ThingDef d) => ((float)d.deepCountPerPortion * d.BaseMarketValue).ToStringMoney()), new TableDataGetter<ThingDef>("mineable\ncommonality", (ThingDef d) => mineableCommonality(d).ToString("F2")), new TableDataGetter<ThingDef>("mineable\nlump size", (ThingDef d) => mineableLumpSizeRange(d)), new TableDataGetter<ThingDef>("mineable yield\nper cell", (ThingDef d) => mineableYield(d)));
		}

		[DebugOutput]
		public static void NaturalRocks()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
				where d.category == ThingCategory.Building && (d.building.isNaturalRock || d.building.isResourceRock) && !d.IsSmoothed
				select d into x
				orderby x.building.isNaturalRock descending, x.building.isResourceRock descending
				select x, new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("isNaturalRock", (ThingDef d) => d.building.isNaturalRock.ToStringCheckBlank()), new TableDataGetter<ThingDef>("isResourceRock", (ThingDef d) => d.building.isResourceRock.ToStringCheckBlank()), new TableDataGetter<ThingDef>("smoothed", (ThingDef d) => (d.building.smoothedThing == null) ? "" : d.building.smoothedThing.defName), new TableDataGetter<ThingDef>("mineableThing", (ThingDef d) => (d.building.mineableThing == null) ? "" : d.building.mineableThing.defName), new TableDataGetter<ThingDef>("mineableYield", (ThingDef d) => d.building.EffectiveMineableYield), new TableDataGetter<ThingDef>("mineableYieldWasteable", (ThingDef d) => d.building.mineableYieldWasteable), new TableDataGetter<ThingDef>("NaturalRockType\never possible", (ThingDef d) => d.IsNonResourceNaturalRock.ToStringCheckBlank()), new TableDataGetter<ThingDef>("NaturalRockType\nin CurrentMap", (ThingDef d) => (Find.CurrentMap == null) ? "" : Find.World.NaturalRockTypesIn(Find.CurrentMap.Tile).Contains(d).ToStringCheckBlank()));
		}

		[DebugOutput]
		public static void AncientJunk()
		{
			DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.defName.StartsWith("Ancient")), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("edifice", (ThingDef d) => d.IsEdifice()), new TableDataGetter<ThingDef>("scatterers", delegate(ThingDef d)
			{
				string text = null;
				foreach (GenStepDef item in DefDatabase<GenStepDef>.AllDefsListForReading)
				{
					if (item.genStep is GenStep_ScatterThings genStep_ScatterThings && genStep_ScatterThings.thingDef == d)
					{
						if (text != null)
						{
							text += ", ";
						}
						text = text + "ScatterThings " + item.defName;
					}
					if (item.genStep is GenStep_ScatterGroup genStep_ScatterGroup)
					{
						string text2 = null;
						for (int i = 0; i < genStep_ScatterGroup.groups.Count; i++)
						{
							GenStep_ScatterGroup.ScatterGroup scatterGroup = genStep_ScatterGroup.groups[i];
							for (int j = 0; j < scatterGroup.things.Count; j++)
							{
								GenStep_ScatterGroup.ThingWeight thingWeight = scatterGroup.things[j];
								if (thingWeight.thing == d)
								{
									if (text2 != null)
									{
										text2 += ", ";
									}
									text2 = text2 + i + "|" + thingWeight.weight;
								}
							}
						}
						if (text2 != null)
						{
							if (text != null)
							{
								text += ", ";
							}
							text = text + "ScatterGroup " + item.defName + " {" + text2 + "}";
						}
					}
				}
				return text;
			}), new TableDataGetter<ThingDef>("rotatable", (ThingDef d) => d.rotatable));
		}

		[DebugOutput]
		public static void MeditationFoci()
		{
			DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.StatBaseDefined(StatDefOf.MeditationFocusStrength)), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("base", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.MeditationFocusStrength).ToStringPercent()), new TableDataGetter<ThingDef>("max\ntotal", (ThingDef d) => TotalMax(d).ToStringPercent()), new TableDataGetter<ThingDef>("offset 0\nname", (ThingDef d) => GetOffsetClassName(d, 0)), new TableDataGetter<ThingDef>("offset 0\nmax", (ThingDef d) => GetOffsetMax(d, 0).ToStringPercentEmptyZero()), new TableDataGetter<ThingDef>("offset 1\nname", (ThingDef d) => GetOffsetClassName(d, 1)), new TableDataGetter<ThingDef>("offset 1\nmax", (ThingDef d) => GetOffsetMax(d, 1).ToStringPercentEmptyZero()), new TableDataGetter<ThingDef>("offset 2\nname", (ThingDef d) => GetOffsetClassName(d, 2)), new TableDataGetter<ThingDef>("offset 2\nmax", (ThingDef d) => GetOffsetMax(d, 2).ToStringPercentEmptyZero()), new TableDataGetter<ThingDef>("offset 3\nname", (ThingDef d) => GetOffsetClassName(d, 3)), new TableDataGetter<ThingDef>("offset 3\nmax", (ThingDef d) => GetOffsetMax(d, 3).ToStringPercentEmptyZero()), new TableDataGetter<ThingDef>("offset 4\nname", (ThingDef d) => GetOffsetClassName(d, 4)), new TableDataGetter<ThingDef>("offset 4\nmax", (ThingDef d) => GetOffsetMax(d, 4).ToStringPercentEmptyZero()));
			string GetOffsetClassName(ThingDef d, int index)
			{
				if (!TryGetOffset(d, index, out var result2))
				{
					return "";
				}
				return result2.GetType().Name;
			}
			float GetOffsetMax(ThingDef d, int index)
			{
				if (!TryGetOffset(d, index, out var result3))
				{
					return 0f;
				}
				return Mathf.Max(result3.MaxOffset(), 0f);
			}
			float TotalMax(ThingDef d)
			{
				float num = d.GetStatValueAbstract(StatDefOf.MeditationFocusStrength);
				for (int i = 0; i < 5; i++)
				{
					num += GetOffsetMax(d, i);
				}
				return num;
			}
			bool TryGetOffset(ThingDef d, int index, out FocusStrengthOffset result)
			{
				result = null;
				CompProperties_MeditationFocus compProperties = d.GetCompProperties<CompProperties_MeditationFocus>();
				if (compProperties == null)
				{
					return false;
				}
				if (compProperties.offsets.Count <= index)
				{
					return false;
				}
				result = compProperties.offsets[index];
				return true;
			}
		}

		[DebugOutput]
		public static void DefaultStuffs()
		{
			DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.MadeFromStuff && !d.IsFrame), new TableDataGetter<ThingDef>("category", (ThingDef d) => d.category.ToString()), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("default stuff", (ThingDef d) => GenStuff.DefaultStuffFor(d).defName), new TableDataGetter<ThingDef>("stuff categories", (ThingDef d) => d.stuffCategories.Select((StuffCategoryDef c) => c.defName).ToCommaList()));
		}

		[DebugOutput]
		public static void Beauties()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs.Cast<BuildableDef>().Concat(DefDatabase<TerrainDef>.AllDefs.Cast<BuildableDef>())
				where (d is ThingDef thingDef) ? BeautyUtility.BeautyRelevant(thingDef.category) : (d is TerrainDef)
				orderby (int)d.GetStatValueAbstract(StatDefOf.Beauty) descending
				select d, new TableDataGetter<BuildableDef>("category", (BuildableDef d) => (!(d is ThingDef)) ? "Terrain" : ((ThingDef)d).category.ToString()), new TableDataGetter<BuildableDef>("defName", (BuildableDef d) => d.defName), new TableDataGetter<BuildableDef>("beauty", (BuildableDef d) => d.GetStatValueAbstract(StatDefOf.Beauty).ToString()), new TableDataGetter<BuildableDef>("beauty outdoors", (BuildableDef d) => (!d.StatBaseDefined(StatDefOf.BeautyOutdoors)) ? "" : d.GetStatValueAbstract(StatDefOf.BeautyOutdoors).ToString()), new TableDataGetter<BuildableDef>("market value", (BuildableDef d) => d.GetStatValueAbstract(StatDefOf.MarketValue).ToString("F1")), new TableDataGetter<BuildableDef>("work to produce", (BuildableDef d) => DebugOutputsEconomy.WorkToProduceBest(d).ToString()), new TableDataGetter<BuildableDef>("beauty per market value", (BuildableDef d) => (!(d.GetStatValueAbstract(StatDefOf.Beauty) > 0f)) ? "" : (d.GetStatValueAbstract(StatDefOf.Beauty) / d.GetStatValueAbstract(StatDefOf.MarketValue)).ToString("F5")));
		}

		[DebugOutput]
		public static void StuffBeauty()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
				where d.IsStuff
				orderby getStatFactorVal(d, StatDefOf.Beauty) descending
				select d, new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("beauty factor", (ThingDef d) => getStatFactorVal(d, StatDefOf.Beauty).ToString()), new TableDataGetter<ThingDef>("beauty offset", (ThingDef d) => getStatOffsetVal(d, StatDefOf.Beauty).ToString()), new TableDataGetter<ThingDef>("market value", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.MarketValue).ToString("F1")), new TableDataGetter<ThingDef>("beauty factor per market value", (ThingDef d) => (!(getStatFactorVal(d, StatDefOf.Beauty) > 0f)) ? "" : (getStatFactorVal(d, StatDefOf.Beauty) / d.GetStatValueAbstract(StatDefOf.MarketValue)).ToString("F5")));
			float getStatFactorVal(ThingDef d, StatDef stat)
			{
				if (d.stuffProps.statFactors == null)
				{
					return 0f;
				}
				return d.stuffProps.statFactors.FirstOrDefault((StatModifier fa) => fa.stat == stat)?.value ?? 0f;
			}
			float getStatOffsetVal(ThingDef d, StatDef stat)
			{
				if (d.stuffProps.statOffsets == null)
				{
					return 0f;
				}
				return d.stuffProps.statOffsets.FirstOrDefault((StatModifier fa) => fa.stat == stat)?.value ?? 0f;
			}
		}

		[DebugOutput]
		public static void ThingsPowerAndHeat()
		{
			Func<ThingDef, CompProperties_HeatPusher> heatPusher = delegate(ThingDef d)
			{
				if (d.comps == null)
				{
					return null;
				}
				for (int i = 0; i < d.comps.Count; i++)
				{
					if (d.comps[i] is CompProperties_HeatPusher result)
					{
						return result;
					}
				}
				return null;
			};
			DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => (d.category == ThingCategory.Building || d.GetCompProperties<CompProperties_Power>() != null || heatPusher(d) != null) && !d.IsFrame), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("base\npower consumption", (ThingDef d) => (d.GetCompProperties<CompProperties_Power>() != null) ? d.GetCompProperties<CompProperties_Power>().PowerConsumption.ToString() : ""), new TableDataGetter<ThingDef>("short circuit\nin rain", (ThingDef d) => (d.GetCompProperties<CompProperties_Power>() != null) ? ((!d.GetCompProperties<CompProperties_Power>().shortCircuitInRain) ? "" : "rainfire") : ""), new TableDataGetter<ThingDef>("transmits\npower", (ThingDef d) => (d.GetCompProperties<CompProperties_Power>() != null) ? ((!d.GetCompProperties<CompProperties_Power>().transmitsPower) ? "" : "transmit") : ""), new TableDataGetter<ThingDef>("market\nvalue", (ThingDef d) => d.BaseMarketValue), new TableDataGetter<ThingDef>("cost list", (ThingDef d) => DebugOutputsEconomy.CostListString(d, divideByVolume: true, starIfOnlyBuyable: false)), new TableDataGetter<ThingDef>("heat pusher\ncompClass", (ThingDef d) => (heatPusher(d) != null) ? heatPusher(d).compClass.ToString() : ""), new TableDataGetter<ThingDef>("heat pusher\nheat per sec", (ThingDef d) => (heatPusher(d) != null) ? heatPusher(d).heatPerSecond.ToString() : ""), new TableDataGetter<ThingDef>("heat pusher\nmin temp", (ThingDef d) => (heatPusher(d) != null) ? heatPusher(d).heatPushMinTemperature.ToStringTemperature() : ""), new TableDataGetter<ThingDef>("heat pusher\nmax temp", (ThingDef d) => (heatPusher(d) != null) ? heatPusher(d).heatPushMaxTemperature.ToStringTemperature() : ""));
		}

		[DebugOutput]
		public static void FoodPoisonChances()
		{
			DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.IsIngestible), new TableDataGetter<ThingDef>("category", (ThingDef d) => d.category), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("food poison chance", delegate(ThingDef d)
			{
				if (d.GetCompProperties<CompProperties_FoodPoisonable>() != null)
				{
					return "poisonable by cook";
				}
				float statValueAbstract = d.GetStatValueAbstract(StatDefOf.FoodPoisonChanceFixedHuman);
				return (statValueAbstract != 0f) ? statValueAbstract.ToStringPercent() : "";
			}));
		}

		[DebugOutput]
		public static void TechLevels()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
				where d.category == ThingCategory.Building || d.category == ThingCategory.Item
				where !d.IsFrame && (d.building == null || !d.building.isNaturalRock)
				orderby (int)d.techLevel descending
				select d, new TableDataGetter<ThingDef>("category", (ThingDef d) => d.category.ToString()), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("tech level", (ThingDef d) => d.techLevel.ToString()));
		}

		[DebugOutput]
		public static void Stuffs()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
				where d.IsStuff
				orderby d.BaseMarketValue
				select d, new TableDataGetter<ThingDef>("fabric", (ThingDef d) => d.stuffProps.categories.Contains(StuffCategoryDefOf.Fabric).ToStringCheckBlank()), new TableDataGetter<ThingDef>("leather", (ThingDef d) => d.stuffProps.categories.Contains(StuffCategoryDefOf.Leathery).ToStringCheckBlank()), new TableDataGetter<ThingDef>("metal", (ThingDef d) => d.stuffProps.categories.Contains(StuffCategoryDefOf.Metallic).ToStringCheckBlank()), new TableDataGetter<ThingDef>("stony", (ThingDef d) => d.stuffProps.categories.Contains(StuffCategoryDefOf.Stony).ToStringCheckBlank()), new TableDataGetter<ThingDef>("woody", (ThingDef d) => d.stuffProps.categories.Contains(StuffCategoryDefOf.Woody).ToStringCheckBlank()), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("burnable", (ThingDef d) => d.burnableByRecipe.ToStringCheckBlank()), new TableDataGetter<ThingDef>("smeltable", (ThingDef d) => d.smeltable.ToStringCheckBlank()), new TableDataGetter<ThingDef>("base\nmarket\nvalue", (ThingDef d) => d.BaseMarketValue.ToStringMoney()), new TableDataGetter<ThingDef>("melee\ncooldown\nmultiplier", (ThingDef d) => getStatFactorString(d, StatDefOf.MeleeWeapon_CooldownMultiplier)), new TableDataGetter<ThingDef>("melee\nsharp\ndamage\nmultiplier", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.SharpDamageMultiplier).ToString("F2")), new TableDataGetter<ThingDef>("melee\nsharp\ndps factor\noverall", (ThingDef d) => meleeDpsSharpFactorOverall(d).ToString("F2")), new TableDataGetter<ThingDef>("melee\nblunt\ndamage\nmultiplier", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.BluntDamageMultiplier).ToString("F2")), new TableDataGetter<ThingDef>("melee\nblunt\ndps factor\noverall", (ThingDef d) => meleeDpsBluntFactorOverall(d).ToString("F2")), new TableDataGetter<ThingDef>("armor power\nsharp", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffPower_Armor_Sharp).ToString("F2")), new TableDataGetter<ThingDef>("armor power\nblunt", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffPower_Armor_Blunt).ToString("F2")), new TableDataGetter<ThingDef>("armor power\nheat", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffPower_Armor_Heat).ToString("F2")), new TableDataGetter<ThingDef>("insulation\ncold", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffPower_Insulation_Cold).ToString("F2")), new TableDataGetter<ThingDef>("insulation\nheat", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffPower_Insulation_Heat).ToString("F2")), new TableDataGetter<ThingDef>("flammability", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.Flammability).ToString("F2")), new TableDataGetter<ThingDef>("factor\nFlammability", (ThingDef d) => getStatFactorString(d, StatDefOf.Flammability)), new TableDataGetter<ThingDef>("factor\nWorkToMake", (ThingDef d) => getStatFactorString(d, StatDefOf.WorkToMake)), new TableDataGetter<ThingDef>("factor\nWorkToBuild", (ThingDef d) => getStatFactorString(d, StatDefOf.WorkToBuild)), new TableDataGetter<ThingDef>("factor\nMaxHp", (ThingDef d) => getStatFactorString(d, StatDefOf.MaxHitPoints)), new TableDataGetter<ThingDef>("factor\nBeauty", (ThingDef d) => getStatFactorString(d, StatDefOf.Beauty)), new TableDataGetter<ThingDef>("factor\nDoorspeed", (ThingDef d) => getStatFactorString(d, StatDefOf.DoorOpenSpeed)));
			string getStatFactorString(ThingDef d, StatDef stat)
			{
				if (d.stuffProps.statFactors == null)
				{
					return "";
				}
				StatModifier statModifier = d.stuffProps.statFactors.FirstOrDefault((StatModifier fa) => fa.stat == stat);
				if (statModifier == null)
				{
					return "";
				}
				return stat.ValueToString(statModifier.value);
			}
			float meleeDpsBluntFactorOverall(ThingDef d)
			{
				float statValueAbstract = d.GetStatValueAbstract(StatDefOf.BluntDamageMultiplier);
				float statFactorFromList = d.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.MeleeWeapon_CooldownMultiplier);
				return statValueAbstract / statFactorFromList;
			}
			float meleeDpsSharpFactorOverall(ThingDef d)
			{
				float statValueAbstract2 = d.GetStatValueAbstract(StatDefOf.SharpDamageMultiplier);
				float statFactorFromList2 = d.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.MeleeWeapon_CooldownMultiplier);
				return statValueAbstract2 / statFactorFromList2;
			}
		}

		[DebugOutput]
		public static void BurningAndSmeltingThings()
		{
			List<RecipeDef> burnRecipes = new List<RecipeDef>();
			foreach (RecipeDef item in DefDatabase<RecipeDef>.AllDefsListForReading)
			{
				if (item.defName.Substring(0, 4).ToLower().Equals("burn"))
				{
					burnRecipes.Add(item);
				}
			}
			List<RecipeDef> smeltRecipes = new List<RecipeDef>();
			foreach (RecipeDef item2 in DefDatabase<RecipeDef>.AllDefsListForReading)
			{
				if (item2.defName.Substring(0, 5).ToLower().Equals("smelt"))
				{
					smeltRecipes.Add(item2);
				}
			}
			DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefs, new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("flammability", (ThingDef d) => d.BaseFlammability), new TableDataGetter<ThingDef>("burn- or smeltable", (ThingDef d) => (smeltRecipes.Any((RecipeDef r) => r.IsIngredient(d)) || burnRecipes.Any((RecipeDef r) => r.IsIngredient(d))).ToStringCheckBlank()), new TableDataGetter<ThingDef>("burnable", (ThingDef d) => d.burnableByRecipe.ToStringCheckBlank()), new TableDataGetter<ThingDef>("smeltable", (ThingDef d) => d.smeltable.ToStringCheckBlank()), new TableDataGetter<ThingDef>("burn recipe", delegate(ThingDef d)
			{
				string[] array2 = (from r in burnRecipes
					where r.IsIngredient(d)
					select r.ToString()).ToArray();
				return (array2.Length != 0) ? string.Join(",", array2) : "NONE";
			}), new TableDataGetter<ThingDef>("smelt recipe", delegate(ThingDef d)
			{
				string[] array = (from r in smeltRecipes
					where r.IsIngredient(d)
					select r.ToString()).ToArray();
				return (array.Length != 0) ? string.Join(",", array) : "NONE";
			}), new TableDataGetter<ThingDef>("category", (ThingDef d) => (d.thingCategories != null) ? string.Join(",", d.thingCategories.Select((ThingCategoryDef c) => c.defName).ToArray()) : "NULL"));
		}

		[DebugOutput]
		public static void Medicines()
		{
			List<float> list = new List<float>();
			list.Add(0.3f);
			list.AddRange(from d in DefDatabase<ThingDef>.AllDefs
				where typeof(Medicine).IsAssignableFrom(d.thingClass)
				select d.GetStatValueAbstract(StatDefOf.MedicalPotency));
			SkillNeed_Direct skillNeed_Direct = (SkillNeed_Direct)StatDefOf.MedicalTendQuality.skillNeedFactors[0];
			TableDataGetter<float>[] array = new TableDataGetter<float>[21];
			array[0] = new TableDataGetter<float>("potency", (float p) => p.ToStringPercent());
			for (int i = 0; i < 20; i++)
			{
				float factor = skillNeed_Direct.valuesPerLevel[i];
				array[i + 1] = new TableDataGetter<float>((i + 1).ToString(), (float p) => (p * factor).ToStringPercent());
			}
			DebugTables.MakeTablesDialog(list, array);
		}

		[DebugOutput]
		public static void ShootingAccuracy()
		{
			StatDef stat = StatDefOf.ShootingAccuracyPawn;
			Func<int, float, int, float> accAtDistance = delegate(int level, float dist, int traitDegree)
			{
				float num = 1f;
				if (traitDegree != 0)
				{
					float value = TraitDef.Named("ShootingAccuracy").DataAtDegree(traitDegree).statOffsets.First((StatModifier so) => so.stat == stat).value;
					num += value;
				}
				foreach (SkillNeed skillNeedFactor in stat.skillNeedFactors)
				{
					SkillNeed_Direct skillNeed_Direct = skillNeedFactor as SkillNeed_Direct;
					num *= skillNeed_Direct.valuesPerLevel[level];
				}
				num = stat.postProcessCurve.Evaluate(num);
				return Mathf.Pow(num, dist);
			};
			List<int> list = new List<int>();
			for (int i = 0; i <= 20; i++)
			{
				list.Add(i);
			}
			DebugTables.MakeTablesDialog(list, new TableDataGetter<int>("No trait skill", (int lev) => lev.ToString()), new TableDataGetter<int>("acc at 1", (int lev) => accAtDistance(lev, 1f, 0).ToStringPercent("F2")), new TableDataGetter<int>("acc at 10", (int lev) => accAtDistance(lev, 10f, 0).ToStringPercent("F2")), new TableDataGetter<int>("acc at 20", (int lev) => accAtDistance(lev, 20f, 0).ToStringPercent("F2")), new TableDataGetter<int>("acc at 30", (int lev) => accAtDistance(lev, 30f, 0).ToStringPercent("F2")), new TableDataGetter<int>("acc at 50", (int lev) => accAtDistance(lev, 50f, 0).ToStringPercent("F2")), new TableDataGetter<int>("Careful shooter skill", (int lev) => lev.ToString()), new TableDataGetter<int>("acc at 1", (int lev) => accAtDistance(lev, 1f, 1).ToStringPercent("F2")), new TableDataGetter<int>("acc at 10", (int lev) => accAtDistance(lev, 10f, 1).ToStringPercent("F2")), new TableDataGetter<int>("acc at 20", (int lev) => accAtDistance(lev, 20f, 1).ToStringPercent("F2")), new TableDataGetter<int>("acc at 30", (int lev) => accAtDistance(lev, 30f, 1).ToStringPercent("F2")), new TableDataGetter<int>("acc at 50", (int lev) => accAtDistance(lev, 50f, 1).ToStringPercent("F2")), new TableDataGetter<int>("Trigger-happy skill", (int lev) => lev.ToString()), new TableDataGetter<int>("acc at 1", (int lev) => accAtDistance(lev, 1f, -1).ToStringPercent("F2")), new TableDataGetter<int>("acc at 10", (int lev) => accAtDistance(lev, 10f, -1).ToStringPercent("F2")), new TableDataGetter<int>("acc at 20", (int lev) => accAtDistance(lev, 20f, -1).ToStringPercent("F2")), new TableDataGetter<int>("acc at 30", (int lev) => accAtDistance(lev, 30f, -1).ToStringPercent("F2")), new TableDataGetter<int>("acc at 50", (int lev) => accAtDistance(lev, 50f, -1).ToStringPercent("F2")));
		}

		[DebugOutput(true)]
		public static void TemperatureData()
		{
			Find.CurrentMap.mapTemperature.DebugLogTemps();
		}

		[DebugOutput(false)]
		public static void TemperatureOverlayColors()
		{
			Find.CurrentMap.mapTemperature.DebugLogTemperatureOverlayColors();
		}

		[DebugOutput(true)]
		public static void WeatherChances()
		{
			Find.CurrentMap.weatherDecider.LogWeatherChances();
		}

		[DebugOutput(true)]
		public static void CelestialGlow()
		{
			GenCelestial.LogSunGlowForYear();
		}

		[DebugOutput(true)]
		public static void SunAngle()
		{
			GenCelestial.LogSunAngleForYear();
		}

		[DebugOutput(true)]
		public static void FallColor()
		{
			PlantUtility.LogFallColorForYear();
		}

		[DebugOutput(true)]
		public static void PawnsListAllOnMap()
		{
			Find.CurrentMap.mapPawns.LogListedPawns();
		}

		[DebugOutput(true)]
		public static void WindSpeeds()
		{
			Find.CurrentMap.windManager.LogWindSpeeds();
		}

		[DebugOutput(true)]
		public static void MapPawnsList()
		{
			Find.CurrentMap.mapPawns.LogListedPawns();
		}

		[DebugOutput]
		public static void Lords()
		{
			Find.CurrentMap.lordManager.LogLords();
		}

		[DebugOutput]
		public static void BodyPartTagGroups()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (BodyDef allDef in DefDatabase<BodyDef>.AllDefs)
			{
				BodyDef localBd = allDef;
				FloatMenuOption item = new FloatMenuOption(localBd.defName, delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine(localBd.defName + "\n----------------");
					foreach (BodyPartTagDef tag in localBd.AllParts.SelectMany((BodyPartRecord part) => part.def.tags).Distinct())
					{
						stringBuilder.AppendLine(tag.defName);
						foreach (BodyPartRecord item2 in from part in localBd.AllParts
							where part.def.tags.Contains(tag)
							orderby part.def.defName
							select part)
						{
							stringBuilder.AppendLine("  " + item2.def.defName);
						}
					}
					Log.Message(stringBuilder.ToString());
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugOutput]
		public static void MinifiableTags()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef.Minifiable)
				{
					stringBuilder.Append(allDef.defName);
					if (!allDef.tradeTags.NullOrEmpty())
					{
						stringBuilder.Append(" - ");
						stringBuilder.Append(allDef.tradeTags.ToCommaList());
					}
					stringBuilder.AppendLine();
				}
			}
			Log.Message(stringBuilder.ToString());
		}

		[DebugOutput]
		public static void ThingSetMakerTest()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (ThingSetMakerDef allDef in DefDatabase<ThingSetMakerDef>.AllDefs)
			{
				ThingSetMakerDef localDef = allDef;
				DebugMenuOption item = new DebugMenuOption(localDef.defName, DebugMenuOptionMode.Action, delegate
				{
					Action<ThingSetMakerParams> generate = delegate(ThingSetMakerParams parms)
					{
						StringBuilder stringBuilder = new StringBuilder();
						float num = 0f;
						float num2 = 0f;
						for (int i = 0; i < 50; i++)
						{
							List<Thing> list4 = localDef.root.Generate(parms);
							if (stringBuilder.Length > 0)
							{
								stringBuilder.AppendLine();
							}
							if (list4.NullOrEmpty())
							{
								stringBuilder.AppendLine("-(nothing generated)");
							}
							float num3 = 0f;
							float num4 = 0f;
							for (int j = 0; j < list4.Count; j++)
							{
								stringBuilder.AppendLine("-" + list4[j].LabelCap + " - $" + (list4[j].MarketValue * (float)list4[j].stackCount).ToString("F0"));
								num3 += list4[j].MarketValue * (float)list4[j].stackCount;
								if (!(list4[j] is Pawn))
								{
									num4 += list4[j].GetStatValue(StatDefOf.Mass) * (float)list4[j].stackCount;
								}
								list4[j].Destroy();
							}
							num += num3;
							num2 += num4;
							stringBuilder.AppendLine("   Total market value: $" + num3.ToString("F0"));
							stringBuilder.AppendLine("   Total mass: " + num4.ToStringMass());
						}
						StringBuilder stringBuilder2 = new StringBuilder();
						stringBuilder2.AppendLine("Default thing sets generated by: " + localDef.defName);
						string nonNullFieldsDebugInfo = Gen.GetNonNullFieldsDebugInfo(localDef.root.fixedParams);
						stringBuilder2.AppendLine("root fixedParams: " + (nonNullFieldsDebugInfo.NullOrEmpty() ? "none" : nonNullFieldsDebugInfo));
						string nonNullFieldsDebugInfo2 = Gen.GetNonNullFieldsDebugInfo(parms);
						if (!nonNullFieldsDebugInfo2.NullOrEmpty())
						{
							stringBuilder2.AppendLine("(used custom debug params: " + nonNullFieldsDebugInfo2 + ")");
						}
						stringBuilder2.AppendLine("Average market value: $" + (num / 50f).ToString("F1"));
						stringBuilder2.AppendLine("Average mass: " + (num2 / 50f).ToStringMass());
						stringBuilder2.AppendLine();
						stringBuilder2.Append(stringBuilder.ToString());
						Log.Message(stringBuilder2.ToString());
					};
					if (localDef == ThingSetMakerDefOf.TraderStock)
					{
						List<DebugMenuOption> list2 = new List<DebugMenuOption>();
						foreach (Faction allFaction in Find.FactionManager.AllFactions)
						{
							if (allFaction != Faction.OfPlayer)
							{
								Faction localF = allFaction;
								list2.Add(new DebugMenuOption(localF.Name + " (" + localF.def.defName + ")", DebugMenuOptionMode.Action, delegate
								{
									List<DebugMenuOption> list3 = new List<DebugMenuOption>();
									foreach (TraderKindDef item2 in DefDatabase<TraderKindDef>.AllDefs.Where((TraderKindDef x) => x.orbital).Concat(localF.def.caravanTraderKinds).Concat(localF.def.visitorTraderKinds)
										.Concat(localF.def.baseTraderKinds))
									{
										TraderKindDef localKind = item2;
										list3.Add(new DebugMenuOption(localKind.defName, DebugMenuOptionMode.Action, delegate
										{
											ThingSetMakerParams obj = default(ThingSetMakerParams);
											obj.makingFaction = localF;
											obj.traderDef = localKind;
											generate(obj);
										}));
									}
									Find.WindowStack.Add(new Dialog_DebugOptionListLister(list3));
								}));
							}
						}
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
					}
					else
					{
						generate(localDef.debugParams);
					}
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugOutput]
		public static void ThingSetMakerPossibleDefs()
		{
			Dictionary<ThingSetMakerDef, List<ThingDef>> generatableThings = new Dictionary<ThingSetMakerDef, List<ThingDef>>();
			foreach (ThingSetMakerDef allDef in DefDatabase<ThingSetMakerDef>.AllDefs)
			{
				ThingSetMakerDef thingSetMakerDef = allDef;
				generatableThings[allDef] = thingSetMakerDef.root.AllGeneratableThingsDebug(thingSetMakerDef.debugParams).ToList();
			}
			List<TableDataGetter<ThingDef>> list = new List<TableDataGetter<ThingDef>>();
			list.Add(new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName));
			list.Add(new TableDataGetter<ThingDef>("market\nvalue", (ThingDef d) => d.BaseMarketValue.ToStringMoney()));
			list.Add(new TableDataGetter<ThingDef>("mass", (ThingDef d) => d.BaseMass.ToStringMass()));
			list.Add(new TableDataGetter<ThingDef>("min\ncount", (ThingDef d) => (d.stackLimit == 1) ? "" : d.minRewardCount.ToString()));
			foreach (ThingSetMakerDef allDef2 in DefDatabase<ThingSetMakerDef>.AllDefs)
			{
				ThingSetMakerDef localDef = allDef2;
				list.Add(new TableDataGetter<ThingDef>(localDef.defName.Shorten(), (ThingDef d) => generatableThings[localDef].Contains(d).ToStringCheckBlank()));
			}
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
				where (d.category == ThingCategory.Item && !d.IsCorpse && !d.isUnfinishedThing) || (d.category == ThingCategory.Building && d.Minifiable) || d.category == ThingCategory.Pawn
				orderby d.BaseMarketValue descending
				select d, list.ToArray());
		}

		[DebugOutput]
		public static void ThingSetMakerSampled()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (ThingSetMakerDef allDef in DefDatabase<ThingSetMakerDef>.AllDefs)
			{
				ThingSetMakerDef localDef = allDef;
				DebugMenuOption item = new DebugMenuOption(localDef.defName, DebugMenuOptionMode.Action, delegate
				{
					Action<ThingSetMakerParams> generate = delegate(ThingSetMakerParams parms)
					{
						Dictionary<ThingDef, int> counts = new Dictionary<ThingDef, int>();
						for (int i = 0; i < 500; i++)
						{
							List<Thing> list4 = localDef.root.Generate(parms);
							foreach (ThingDef item2 in list4.Select((Thing th) => th.GetInnerIfMinified().def).Distinct())
							{
								if (!counts.ContainsKey(item2))
								{
									counts.Add(item2, 0);
								}
								counts[item2]++;
							}
							for (int j = 0; j < list4.Count; j++)
							{
								list4[j].Destroy();
							}
						}
						DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
							where counts.ContainsKey(d)
							orderby counts[d] descending
							select d, new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("market\nvalue", (ThingDef d) => d.BaseMarketValue.ToStringMoney()), new TableDataGetter<ThingDef>("mass", (ThingDef d) => d.BaseMass.ToStringMass()), new TableDataGetter<ThingDef>("appearance rate in " + localDef.defName, (ThingDef d) => ((float)counts[d] / 500f).ToStringPercent()));
					};
					if (localDef == ThingSetMakerDefOf.TraderStock)
					{
						List<DebugMenuOption> list2 = new List<DebugMenuOption>();
						foreach (Faction allFaction in Find.FactionManager.AllFactions)
						{
							if (allFaction != Faction.OfPlayer)
							{
								Faction localF = allFaction;
								list2.Add(new DebugMenuOption(localF.Name + " (" + localF.def.defName + ")", DebugMenuOptionMode.Action, delegate
								{
									List<DebugMenuOption> list3 = new List<DebugMenuOption>();
									foreach (TraderKindDef item3 in DefDatabase<TraderKindDef>.AllDefs.Where((TraderKindDef x) => x.orbital).Concat(localF.def.caravanTraderKinds).Concat(localF.def.visitorTraderKinds)
										.Concat(localF.def.baseTraderKinds))
									{
										TraderKindDef localKind = item3;
										list3.Add(new DebugMenuOption(localKind.defName, DebugMenuOptionMode.Action, delegate
										{
											ThingSetMakerParams obj = default(ThingSetMakerParams);
											obj.makingFaction = localF;
											obj.traderDef = localKind;
											generate(obj);
										}));
									}
									Find.WindowStack.Add(new Dialog_DebugOptionListLister(list3));
								}));
							}
						}
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
					}
					else
					{
						generate(localDef.debugParams);
					}
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugOutput]
		public static void RewardsGeneration()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Faction allFaction in Find.FactionManager.AllFactions)
			{
				if (allFaction == Faction.OfPlayer)
				{
					continue;
				}
				Faction localF = allFaction;
				list.Add(new DebugMenuOption(localF.Name + " (" + localF.def.defName + ")", DebugMenuOptionMode.Action, delegate
				{
					List<DebugMenuOption> list2 = new List<DebugMenuOption>();
					foreach (float item in DebugActionsUtility.PointsOptions(extended: false))
					{
						float localPoints = item;
						list2.Add(new DebugMenuOption(item.ToString("F0"), DebugMenuOptionMode.Action, delegate
						{
							StringBuilder stringBuilder = new StringBuilder();
							for (int i = 0; i < 30; i++)
							{
								RewardsGeneratorParams parms = default(RewardsGeneratorParams);
								parms.allowGoodwill = true;
								parms.allowRoyalFavor = true;
								parms.populationIntent = StorytellerUtilityPopulation.PopulationIntentForQuest;
								parms.giverFaction = localF;
								parms.rewardValue = localPoints;
								float generatedRewardValue;
								List<Reward> source = RewardsGenerator.Generate(parms, out generatedRewardValue);
								stringBuilder.AppendLine("giver: " + parms.giverFaction.Name + ", input value: " + parms.rewardValue.ToStringMoney() + ", output value: " + generatedRewardValue.ToStringMoney() + "\n" + source.Select((Reward x) => "-" + x.ToString()).ToLineList().Indented("  "));
								stringBuilder.AppendLine();
							}
							Log.Message(stringBuilder.ToString());
						}));
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugOutput]
		public static void RewardsGenerationSampled()
		{
			RewardsGeneratorParams parms = default(RewardsGeneratorParams);
			parms.allowGoodwill = true;
			parms.allowRoyalFavor = true;
			parms.populationIntent = StorytellerUtilityPopulation.PopulationIntentForQuest;
			parms.giverFaction = (from x in Find.FactionManager.GetFactions()
				where !x.Hidden && x.def.humanlikeFaction && !x.HostileTo(Faction.OfPlayer)
				select x).First();
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (float item in DebugActionsUtility.PointsOptions(extended: false))
			{
				float localPoints = item;
				list.Add(new DebugMenuOption(item.ToString("F0"), DebugMenuOptionMode.Action, delegate
				{
					parms.rewardValue = localPoints;
					Dictionary<Type, int> countByType = new Dictionary<Type, int>();
					Dictionary<ThingDef, int> countByThingDef = new Dictionary<ThingDef, int>();
					for (int i = 0; i < 1000; i++)
					{
						foreach (Reward item2 in RewardsGenerator.Generate(parms))
						{
							countByType.Increment(item2.GetType());
							if (item2 is Reward_Items reward_Items)
							{
								foreach (ThingDef item3 in reward_Items.items.Select((Thing x) => x.GetInnerIfMinified().def).Distinct())
								{
									countByThingDef.Increment(item3);
								}
							}
						}
					}
					Dictionary<ThingSetMakerDef, List<ThingDef>> dictionary = new Dictionary<ThingSetMakerDef, List<ThingDef>>();
					foreach (ThingSetMakerDef allDef in DefDatabase<ThingSetMakerDef>.AllDefs)
					{
						ThingSetMakerDef thingSetMakerDef = allDef;
						dictionary[allDef] = thingSetMakerDef.root.AllGeneratableThingsDebug(thingSetMakerDef.debugParams).ToList();
					}
					List<TableDataGetter<object>> list2 = new List<TableDataGetter<object>>
					{
						new TableDataGetter<object>("defName", (object d) => (!(d is Type)) ? ((ThingDef)d).defName : ("*" + ((Type)d).Name)),
						new TableDataGetter<object>("times appeared\nin " + 1000 + " rewards\nof value " + localPoints, (object d) => (!(d is Type)) ? countByThingDef.TryGetValue((ThingDef)d, 0) : countByType.TryGetValue((Type)d, 0)),
						new TableDataGetter<object>("reward tags", (object d) => (d is ThingDef thingDef2 && !thingDef2.thingSetMakerTags.NullOrEmpty() && thingDef2.thingSetMakerTags.Any((string x) => x.Contains("Reward"))) ? thingDef2.thingSetMakerTags.ToCommaList() : "-"),
						new TableDataGetter<object>("market value", (object d) => (d is ThingDef thingDef) ? thingDef.BaseMarketValue.ToStringMoney() : "-")
					};
					DebugTables.MakeTablesDialog(from d in typeof(Reward).AllSubclassesNonAbstract().Cast<object>().Union(countByThingDef.Keys.Cast<object>())
						orderby (!(d is Type)) ? countByThingDef.TryGetValue((ThingDef)d, 0) : countByType.TryGetValue((Type)d, 0) descending
						select d, list2.ToArray());
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugOutput]
		public static void WorkDisables()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (PawnKindDef item2 in DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef ki) => ki.RaceProps.Humanlike))
			{
				PawnKindDef pkInner = item2;
				Faction faction = FactionUtility.DefaultFactionFrom(pkInner.defaultFactionType);
				FloatMenuOption item = new FloatMenuOption(pkInner.defName, delegate
				{
					int num = 500;
					DefMap<WorkTypeDef, int> defMap = new DefMap<WorkTypeDef, int>();
					for (int i = 0; i < num; i++)
					{
						Pawn pawn = PawnGenerator.GeneratePawn(pkInner, faction);
						if (pawn.workSettings != null)
						{
							foreach (WorkTypeDef disabledWorkType in pawn.GetDisabledWorkTypes(permanentOnly: true))
							{
								defMap[disabledWorkType]++;
							}
						}
					}
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine("Generated " + num + " pawns of kind " + pkInner.defName + " on faction " + faction.ToStringSafe());
					stringBuilder.AppendLine("Work types disabled:");
					foreach (WorkTypeDef allDef in DefDatabase<WorkTypeDef>.AllDefs)
					{
						if (allDef.workTags != 0)
						{
							stringBuilder.AppendLine("   " + allDef.defName + ": " + defMap[allDef] + "        " + ((float)defMap[allDef] / (float)num).ToStringPercent());
						}
					}
					IEnumerable<BackstoryDef> allDefs = DefDatabase<BackstoryDef>.AllDefs;
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("Backstories WorkTypeDef disable rates (there are " + allDefs.Count() + " backstories):");
					foreach (WorkTypeDef wt in DefDatabase<WorkTypeDef>.AllDefs)
					{
						int num2 = 0;
						foreach (BackstoryDef item3 in allDefs)
						{
							if (item3.DisabledWorkTypes.Any((WorkTypeDef wd) => wt == wd))
							{
								num2++;
							}
						}
						stringBuilder.AppendLine("   " + wt.defName + ": " + num2 + "     " + ((float)num2 / (float)allDefs.Count()).ToStringPercent());
					}
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("Backstories WorkTag disable rates (there are " + allDefs.Count() + " backstories):");
					foreach (WorkTags value in Enum.GetValues(typeof(WorkTags)))
					{
						int num3 = 0;
						foreach (BackstoryDef item4 in allDefs)
						{
							if ((value & item4.workDisables) != 0)
							{
								num3++;
							}
						}
						stringBuilder.AppendLine(string.Concat("   ", value, ": ", num3, "     ", ((float)num3 / (float)allDefs.Count()).ToStringPercent()));
					}
					Log.Message(stringBuilder.ToString());
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugOutput]
		public static void FoodPreferability()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Food, ordered by preferability:");
			foreach (ThingDef item in from td in DefDatabase<ThingDef>.AllDefs
				where td.ingestible != null
				orderby td.ingestible.preferability
				select td)
			{
				stringBuilder.AppendLine($"  {item.ingestible.preferability}: {item.defName}");
			}
			Log.Message(stringBuilder.ToString());
		}

		[DebugOutput]
		public static void IngestibleMaxSatisfiedTitle()
		{
			RoyalTitleUtility.DoTable_IngestibleMaxSatisfiedTitle();
		}

		[DebugOutput]
		public static void AbilityCosts()
		{
			AbilityUtility.DoTable_AbilityCosts();
		}

		[DebugOutput]
		public static void SitePartDefs()
		{
			List<TableDataGetter<SitePartDef>> list = new List<TableDataGetter<SitePartDef>>();
			list.Add(new TableDataGetter<SitePartDef>("defName", (SitePartDef a) => a.defName));
			list.Add(new TableDataGetter<SitePartDef>("label", (SitePartDef a) => a.LabelCap));
			list.Add(new TableDataGetter<SitePartDef>("tags", (SitePartDef a) => string.Join(", ", a.tags)));
			list.Add(new TableDataGetter<SitePartDef>("excludeTags", (SitePartDef a) => string.Join(", ", a.excludesTags)));
			list.Add(new TableDataGetter<SitePartDef>("incompatible\nothers", (SitePartDef a) => string.Join(", ", from def in DefDatabase<SitePartDef>.AllDefs
				where !def.CompatibleWith(a)
				select def.LabelCap)));
			DebugTables.MakeTablesDialog(DefDatabase<SitePartDef>.AllDefsListForReading, list.ToArray());
		}

		[DebugOutput]
		public static void MapDanger()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Map danger status:");
			foreach (Map map in Find.Maps)
			{
				stringBuilder.AppendLine($"{map}: {map.dangerWatcher.DangerRating}");
			}
			Log.Message(stringBuilder.ToString());
		}

		[DebugOutput]
		public static void GenSteps()
		{
			DebugTables.MakeTablesDialog(from x in DefDatabase<GenStepDef>.AllDefsListForReading
				orderby x.order, x.index
				select x, new TableDataGetter<GenStepDef>("defName", (GenStepDef x) => x.defName), new TableDataGetter<GenStepDef>("order", (GenStepDef x) => x.order.ToString("0.##")), new TableDataGetter<GenStepDef>("class", (GenStepDef x) => x.genStep.GetType().Name), new TableDataGetter<GenStepDef>("site", (GenStepDef x) => (x.linkWithSite == null) ? "" : x.linkWithSite.defName));
		}

		[DebugOutput]
		public static void WorldGenSteps()
		{
			DebugTables.MakeTablesDialog(from x in DefDatabase<WorldGenStepDef>.AllDefsListForReading
				orderby x.order, x.index
				select x, new TableDataGetter<WorldGenStepDef>("defName", (WorldGenStepDef x) => x.defName), new TableDataGetter<WorldGenStepDef>("order", (WorldGenStepDef x) => x.order.ToString("0.##")), new TableDataGetter<WorldGenStepDef>("class", (WorldGenStepDef x) => x.worldGenStep.GetType().Name));
		}

		[DebugOutput]
		public static void ShuttleDefsToAvoid()
		{
			DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefsListForReading, new TableDataGetter<ThingDef>("defName", (ThingDef x) => x.defName), new TableDataGetter<ThingDef>("avoid", (ThingDef x) => (GenSpawn.SpawningWipes(ThingDefOf.ActiveDropPod, x) || (x.plant != null && x.plant.IsTree) || x.category == ThingCategory.Item || x.category == ThingCategory.Building).ToStringCheckBlank()));
		}

		[DebugOutput]
		public static void RitualDuration()
		{
			DebugTables.MakeTablesDialog(DefDatabase<RitualBehaviorDef>.AllDefsListForReading, new TableDataGetter<RitualBehaviorDef>("defName", (RitualBehaviorDef x) => x.defName), new TableDataGetter<RitualBehaviorDef>("duration", (RitualBehaviorDef x) => x.durationTicks.max.ToStringTicksToPeriod()));
		}

		[DebugOutput]
		public static void PlayerWealth()
		{
			Log.Message("Player wealth: " + WealthUtility.PlayerWealth.ToStringMoney());
		}

		[DebugOutput]
		public static void MemesAndPrecepts()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Memes:");
			foreach (MemeDef item in DefDatabase<MemeDef>.AllDefs.OrderBy((MemeDef x) => x.category))
			{
				stringBuilder.AppendLine($"{item.LabelCap}: \"{item.description}\"");
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Precepts:");
			foreach (IssueDef i in DefDatabase<IssueDef>.AllDefs)
			{
				stringBuilder.AppendLine(i.LabelCap);
				foreach (PreceptDef item2 in DefDatabase<PreceptDef>.AllDefs.Where((PreceptDef x) => x.issue == i && !x.label.NullOrEmpty() && !x.description.NullOrEmpty()))
				{
					stringBuilder.AppendLine($"\t{item2.LabelCap}: \"{item2.description}\"");
				}
			}
			Log.Message(GUIUtility.systemCopyBuffer = stringBuilder.ToString());
		}

		[DebugOutput]
		public static void GenerateGeneSetsX10()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < 10; i++)
			{
				GeneSet geneSet = GeneUtility.GenerateGeneSet();
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(geneSet.ToString());
			}
			Log.Message(stringBuilder.ToString());
		}

		[DebugOutput]
		public static void Genes()
		{
			DebugTables.MakeTablesDialog(DefDatabase<GeneDef>.AllDefs, new TableDataGetter<GeneDef>("defName", (GeneDef g) => g.defName), new TableDataGetter<GeneDef>("label", (GeneDef g) => g.label), new TableDataGetter<GeneDef>("labelShortAdj", (GeneDef g) => g.LabelShortAdj), new TableDataGetter<GeneDef>("CPX", (GeneDef g) => g.biostatCpx), new TableDataGetter<GeneDef>("MET", (GeneDef g) => g.biostatMet), new TableDataGetter<GeneDef>("ARC", (GeneDef g) => g.biostatArc), new TableDataGetter<GeneDef>("prereq", (GeneDef g) => (g.prerequisite != null) ? g.prerequisite.defName : string.Empty));
		}

		[DebugOutput]
		public static void Xenotypes()
		{
			DebugTables.MakeTablesDialog(DefDatabase<XenotypeDef>.AllDefs, new TableDataGetter<XenotypeDef>("defName", (XenotypeDef x) => x.defName), new TableDataGetter<XenotypeDef>("CPX", (XenotypeDef x) => x.AllGenes.Sum((GeneDef y) => y.biostatCpx)), new TableDataGetter<XenotypeDef>("MET", (XenotypeDef x) => x.AllGenes.Sum((GeneDef y) => y.biostatMet)), new TableDataGetter<XenotypeDef>("ARC", (XenotypeDef x) => x.AllGenes.Sum((GeneDef y) => y.biostatArc)));
		}

		[DebugOutput]
		public static void PreceptDefs()
		{
			DebugTables.MakeTablesDialog(DefDatabase<PreceptDef>.AllDefs.OrderBy((PreceptDef x) => x.defName), new TableDataGetter<PreceptDef>("defName", (PreceptDef x) => x.defName), new TableDataGetter<PreceptDef>("impact", (PreceptDef x) => x.impact), new TableDataGetter<PreceptDef>("duplicates", (PreceptDef x) => x.allowDuplicates.ToStringCheckBlank()), new TableDataGetter<PreceptDef>("defaultSelectionWeight", (PreceptDef x) => x.defaultSelectionWeight.ToString()), new TableDataGetter<PreceptDef>("noExpansion", (PreceptDef x) => x.classic.ToStringCheckBlank()), new TableDataGetter<PreceptDef>("exclusionTags", (PreceptDef x) => x.exclusionTags.ToCommaList(useAnd: false, emptyIfNone: true)), new TableDataGetter<PreceptDef>("effects", (PreceptDef x) => string.Join("\n", x.comps.SelectMany((PreceptComp y) => y.GetDescriptions()).Distinct())), new TableDataGetter<PreceptDef>("stats", (PreceptDef x) => ((x.statOffsets != null) ? x.statOffsets.Select((StatModifier y) => y.stat.defName + ": " + y.ValueToStringAsOffset) : Enumerable.Empty<string>()).Concat((x.statFactors != null) ? x.statFactors.Select((StatModifier y) => y.stat.defName + ": " + y.ToStringAsFactor) : Enumerable.Empty<string>()).ToCommaList(useAnd: false, emptyIfNone: true)));
		}

		[DebugOutput]
		public static void ThingTradeTags()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (string tag in from t in DefDatabase<ThingDef>.AllDefs.Where((ThingDef k) => k.tradeTags != null).SelectMany((ThingDef k) => k.tradeTags).Distinct()
				orderby t
				select t)
			{
				list.Add(new DebugMenuOption(tag, DebugMenuOptionMode.Action, delegate
				{
					List<TableDataGetter<ThingDef>> list2 = new List<TableDataGetter<ThingDef>>
					{
						new TableDataGetter<ThingDef>("defName", (ThingDef k) => k.defName)
					};
					DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefs.Where((ThingDef t) => t.tradeTags.NotNullAndContains(tag)), list2.ToArray());
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugOutput]
		public static void StructureColorDefs()
		{
			DebugTables.MakeTablesDialog(from x in DefDatabase<ColorDef>.AllDefs
				where x.colorType == ColorType.Structure
				orderby x.displayOrder
				select x, new TableDataGetter<ColorDef>("defName", (ColorDef x) => x.defName), new TableDataGetter<ColorDef>("label", (ColorDef x) => x.label), new TableDataGetter<ColorDef>("rgb", (ColorDef x) => $"({Mathf.RoundToInt(x.color.r * 255f)}, {Mathf.RoundToInt(x.color.r * 255f)}, {Mathf.RoundToInt(x.color.r * 2555f)})"), new TableDataGetter<ColorDef>("hsl", delegate(ColorDef x)
			{
				Color.RGBToHSV(x.color, out var H, out var S, out var V);
				return $"({Mathf.RoundToInt(H * 360f)}, {S.ToStringPercent()}, {V.ToStringPercent()})";
			}), new TableDataGetter<ColorDef>("order", (ColorDef x) => x.displayOrder));
		}
	}
}
