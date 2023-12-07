using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{

public class IncidentWorker_ResourcePodCrash : IncidentWorker
{
	protected override bool TryExecuteWorker( IncidentParms parms )
	{
		var map = (Map)parms.target;

		var thingsToDrop = ThingSetMakerDefOf.ResourcePod.root.Generate();
		var dropCenter = DropCellFinder.RandomDropSpot(map);

		DropPodUtility.DropThingsNear(dropCenter, map, thingsToDrop, leaveSlag: true, canRoofPunch: true);

		SendStandardLetter("LetterLabelCargoPodCrash".Translate(),
			"CargoPodCrash".Translate(),
			LetterDefOf.PositiveEvent,
			parms,
			new TargetInfo(dropCenter, map));

		return true;
	}
}

}
