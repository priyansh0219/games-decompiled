using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Alert_RoyalNoAcceptableFood : Alert
	{
		private List<Pawn> targetsResult = new List<Pawn>();

		public List<Pawn> Targets
		{
			get
			{
				targetsResult.Clear();
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					foreach (Pawn freeColonist in maps[i].mapPawns.FreeColonists)
					{
						if (freeColonist.Spawned && (freeColonist.story == null || !freeColonist.story.traits.HasTrait(TraitDefOf.Ascetic)))
						{
							RoyalTitle royalTitle = freeColonist.royalty?.MostSeniorTitle;
							if (royalTitle != null && royalTitle.conceited && royalTitle.def.foodRequirement.Defined && !FoodUtility.TryFindBestFoodSourceFor_NewTemp(freeColonist, freeColonist, desperate: false, out var _, out var _, canRefillDispenser: true, canUseInventory: true, canUsePackAnimalInventory: false, allowForbidden: false, allowCorpse: false, allowSociallyImproper: false, allowHarvest: false, forceScanWholeMap: false, ignoreReservations: true, calculateWantedStackCount: false, allowVenerated: false, FoodPreferability.DesperateOnly))
							{
								targetsResult.Add(freeColonist);
							}
						}
					}
				}
				return targetsResult;
			}
		}

		public Alert_RoyalNoAcceptableFood()
		{
			defaultLabel = "RoyalNoAcceptableFood".Translate();
			defaultExplanation = "RoyalNoAcceptableFoodDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			if (!ModsConfig.RoyaltyActive)
			{
				return false;
			}
			return AlertReport.CulpritsAre(Targets);
		}

		public override TaggedString GetExplanation()
		{
			return defaultExplanation + "\n" + Targets.Select(delegate(Pawn t)
			{
				RoyalTitle mostSeniorTitle = t.royalty.MostSeniorTitle;
				string text = t.LabelShort + " (" + mostSeniorTitle.def.GetLabelFor(t.gender) + "):\n" + mostSeniorTitle.def.SatisfyingMeals(includeDrugs: false).Select((Func<ThingDef, string>)((ThingDef m) => m.LabelCap)).ToLineList("- ");
				if (ModsConfig.IdeologyActive && t.Ideo != null && t.Ideo.VeneratedAnimals.Any())
				{
					text = text + "\n\n" + "AlertRoyalTitleNoVeneratedAnimalMeat".Translate(t.Named("PAWN"), t.Ideo.Named("IDEO"), t.Ideo.VeneratedAnimals.Select((ThingDef x) => x.label).ToCommaList().Named("ANIMALS")).Resolve();
				}
				return text;
			}).ToLineList("\n");
		}
	}
}
