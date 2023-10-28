using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StockGenerator_BuyTradeTag : StockGenerator
	{
		public string tag;

		public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
		{
			return Enumerable.Empty<Thing>();
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			if (thingDef.tradeTags != null)
			{
				return thingDef.tradeTags.Contains(tag);
			}
			return false;
		}
	}
}
