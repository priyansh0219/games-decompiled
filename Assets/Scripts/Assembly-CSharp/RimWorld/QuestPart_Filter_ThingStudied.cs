using Verse;

namespace RimWorld
{
	public class QuestPart_Filter_ThingStudied : QuestPart_Filter
	{
		public ThingDef thingDef;

		protected override bool Pass(SignalArgs args)
		{
			return Find.StudyManager.StudyComplete(thingDef);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref thingDef, "thingDef");
		}
	}
}
