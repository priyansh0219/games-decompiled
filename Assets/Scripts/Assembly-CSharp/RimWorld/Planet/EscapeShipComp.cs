using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class EscapeShipComp : WorldObjectComp
	{
		public override void PostMapGenerate()
		{
			Building building = ((MapParent)parent).Map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.Ship_Reactor).FirstOrDefault();
			if (building != null && building is Building_ShipReactor building_ShipReactor)
			{
				building_ShipReactor.charlonsReactor = true;
			}
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			foreach (FloatMenuOption floatMenuOption in CaravanArrivalAction_VisitEscapeShip.GetFloatMenuOptions(caravan, (MapParent)parent))
			{
				yield return floatMenuOption;
			}
		}
	}
}
