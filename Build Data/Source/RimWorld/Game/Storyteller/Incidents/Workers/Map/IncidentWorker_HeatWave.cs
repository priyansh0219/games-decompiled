using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;


namespace RimWorld{
public class IncidentWorker_HeatWave : IncidentWorker_MakeGameCondition
{
	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if( !base.CanFireNowSub(parms) )
			return false;

		var map = (Map)parms.target;
		return IsTemperatureAppropriate(map);
	}

	public static bool IsTemperatureAppropriate(Map map)
	{
		return map.mapTemperature.SeasonalTemp >= 20;
	}
}

public class IncidentWorker_ColdSnap : IncidentWorker_MakeGameCondition
{
	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if( !base.CanFireNowSub(parms) )
			return false;

		var map = (Map)parms.target;
		return IsTemperatureAppropriate(map);
	}

	public static bool IsTemperatureAppropriate(Map map)
	{
		return map.mapTemperature.SeasonalTemp > 0 && map.mapTemperature.SeasonalTemp < 15;
	}
}
}