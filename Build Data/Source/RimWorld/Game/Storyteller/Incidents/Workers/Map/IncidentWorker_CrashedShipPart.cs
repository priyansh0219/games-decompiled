using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
    public class IncidentWorker_CrashedShipPart : IncidentWorker
    {
        //Constants
        private const float ShipPointsFactor = 0.9f;
        private const int IncidentMinimumPoints = 300; //One centipede
		private const float DefendRadius = 28f;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            var map = (Map)parms.target;

            if (map.listerThings.ThingsOfDef(def.mechClusterBuilding).Count > 0)
                return false;

            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = (Map)parms.target;

            var lookTargets = new List<TargetInfo>();
            var shipPartDef = def.mechClusterBuilding;

            bool CanPlaceAt(IntVec3 loc)
            {
                var occupiedRect = GenAdj.OccupiedRect(loc, Rot4.North, shipPartDef.Size);
                if (loc.Fogged(map) || !occupiedRect.InBounds(map))
                    return false;

                if (!DropCellFinder.SkyfallerCanLandAt(loc, map, shipPartDef.Size))
                    return false;

                //Make sure the ship part doesn't punch through natural roofs
                foreach (var c in occupiedRect)
                {
                    var roof = c.GetRoof(map);
                    if (roof != null && roof.isNatural)
                        return false;
                }

                return GenConstruct.CanBuildOnTerrain(shipPartDef, loc, map, Rot4.North);
            }

            var cell = FindDropPodLocation(map, spot => CanPlaceAt(spot));
            if (cell == IntVec3.Invalid)
                return false;
    
            float points = Mathf.Max(parms.points * ShipPointsFactor, IncidentMinimumPoints);
            
            //Create mechs
            var groupParms = new PawnGroupMakerParms();
            groupParms.groupKind = PawnGroupKindDefOf.Combat;
            groupParms.tile = map.Tile;
            groupParms.faction = Faction.OfMechanoids;
            groupParms.points = points;
            
            var generatedPawns = PawnGroupMakerUtility.GeneratePawns(groupParms).ToList();
            
            //Create a ship part
            var shipPart = ThingMaker.MakeThing(shipPartDef);
            shipPart.SetFaction(Faction.OfMechanoids);
            
            LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_SleepThenMechanoidsDefend(new List<Thing>() { shipPart }, Faction.OfMechanoids, DefendRadius, cell, false, false), map, generatedPawns);
            DropPodUtility.DropThingsNear(cell, map, generatedPawns.Cast<Thing>());

            foreach (var p in generatedPawns)
            {
	            var canBeDormantComp = p.TryGetComp<CompCanBeDormant>();
	            if(canBeDormantComp != null)
		            canBeDormantComp.ToSleep();
            }
            
            lookTargets.AddRange(generatedPawns.Select(p => new TargetInfo(p)));

            //Create a skyfaller
            var skyfaller = SkyfallerMaker.MakeSkyfaller(ThingDefOf.CrashedShipPartIncoming, shipPart);
            GenSpawn.Spawn(skyfaller, cell, map);
            
            lookTargets.Add(new TargetInfo(cell, map));

            //Make letter
            SendStandardLetter(parms, lookTargets);
            return true;
        }
        
	    private static IntVec3 FindDropPodLocation(Map map, System.Predicate<IntVec3> validator)
	    {
            const int Tries = 200;

            for( int i = 0; i < Tries; i++ )
		    {
                var cell = DropCellFinder.FindRaidDropCenterDistant(map, true);
                var siegePos = RCellFinder.FindSiegePositionFrom(cell, map, true);

                if (validator(siegePos))
                    return siegePos;
		    }

		    return IntVec3.Invalid;
	    }
    }
}