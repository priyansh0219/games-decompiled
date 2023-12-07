using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{

public class IncidentWorker_CropBlight : IncidentWorker
{
	//Constants
	private const float Radius = 11f;
    private static readonly SimpleCurve BlightChancePerRadius = new SimpleCurve()
    {
        new CurvePoint(0, 1f),
        new CurvePoint(8, 1f),
        new CurvePoint(Radius, 0.3f)
    };

    private static readonly SimpleCurve RadiusFactorPerPointsCurve = new SimpleCurve()
    {
        new CurvePoint(100,  0.6f),
        new CurvePoint(500,  1.0f),
        new CurvePoint(2000, 2.0f)
    };


	protected override bool CanFireNowSub(IncidentParms parms)
	{
		return TryFindRandomBlightablePlant((Map)parms.target, out _);
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		var map = (Map)parms.target;
		Plant rootPlant;
        float radiusFactor = RadiusFactorPerPointsCurve.Evaluate(parms.points);

		if( !TryFindRandomBlightablePlant(map, out rootPlant) )
			return false;

		var rootRoom = rootPlant.GetRoom();
		for( int i = 0, count = GenRadial.NumCellsInRadius(Radius * radiusFactor); i < count; i++ )
		{
			var c = rootPlant.Position + GenRadial.RadialPattern[i];

			if( !c.InBounds(map) || c.GetRoom(map) != rootRoom )
				continue;

			var plant = BlightUtility.GetFirstBlightableNowPlant(c, map);

			if( plant != null
            && plant.def == rootPlant.def
            && Rand.Chance( BlightChance(plant.Position, rootPlant.Position, radiusFactor)) )
				plant.CropBlighted();
		}

		SendStandardLetter("LetterLabelCropBlight".Translate( new NamedArgument(rootPlant.def, "PLANTDEF") ),
			"LetterCropBlight".Translate( new NamedArgument(rootPlant.def, "PLANTDEF") ),
			LetterDefOf.NegativeEvent,
			parms,
			new TargetInfo(rootPlant.Position, map));

		return true;
	}

	private bool TryFindRandomBlightablePlant(Map map, out Plant plant)
	{
		Thing plantThing;

		bool found = map.listerThings.ThingsInGroup(ThingRequestGroup.Plant)
			.Where(x => ((Plant)x).BlightableNow)
			.TryRandomElement(out plantThing);

		plant = (Plant)plantThing;

		return found;
	}

	private float BlightChance(IntVec3 c, IntVec3 root, float radiusFactor)
	{
        float radiusAdjusted = c.DistanceTo(root) / radiusFactor;
        return BlightChancePerRadius.Evaluate( radiusAdjusted );
	}
}

}
