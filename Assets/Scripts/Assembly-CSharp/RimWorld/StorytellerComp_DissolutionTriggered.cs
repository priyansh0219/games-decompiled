using Verse;

namespace RimWorld
{
	public class StorytellerComp_DissolutionTriggered : StorytellerComp
	{
		private StorytellerCompProperties_DissolutionTriggered Props => (StorytellerCompProperties_DissolutionTriggered)props;

		public override void Notify_DissolutionEvent(Thing thing)
		{
			if (thing.def == Props.thing)
			{
				IncidentParms incidentParms = new IncidentParms();
				incidentParms.target = thing.MapHeld;
				incidentParms.spawnCenter = thing.Position;
				incidentParms.points = StorytellerUtility.DefaultThreatPointsNow(incidentParms.target);
				if (Rand.Chance(IncidentChanceFinal(Props.incident)) && Props.incident.Worker.CanFireNow(incidentParms))
				{
					new FiringIncident(Props.incident, this, incidentParms);
					Find.Storyteller.incidentQueue.Add(Props.incident, Find.TickManager.TicksGame + Props.delayTicks, incidentParms);
				}
			}
		}
	}
}
