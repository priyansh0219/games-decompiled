using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ScenPart_StartingThing_Defined : ScenPart_ThingCount
	{
		public const string PlayerStartWithTag = "PlayerStartsWith";

		public static string PlayerStartWithIntro => "ScenPart_StartWith".Translate();

		public override string Summary(Scenario scen)
		{
			return ScenSummaryList.SummaryWithList(scen, "PlayerStartsWith", PlayerStartWithIntro);
		}

		public override IEnumerable<string> GetSummaryListEntries(string tag)
		{
			if (tag == "PlayerStartsWith")
			{
				yield return GenLabel.ThingLabel(thingDef, stuff, count).CapitalizeFirst();
			}
		}

		public override IEnumerable<Thing> PlayerStartingThings()
		{
			Thing thing = ThingMaker.MakeThing(thingDef, stuff);
			if (quality.HasValue)
			{
				thing.TryGetComp<CompQuality>()?.SetQuality(quality.Value, ArtGenerationContext.Outsider);
			}
			if (thingDef.Minifiable)
			{
				thing = thing.MakeMinified();
			}
			if (thingDef.IsIngestible && thingDef.ingestible.IsMeal)
			{
				FoodUtility.GenerateGoodIngredients(thing, Faction.OfPlayer.ideos.PrimaryIdeo);
			}
			thing.stackCount = count;
			yield return thing;
		}
	}
}
