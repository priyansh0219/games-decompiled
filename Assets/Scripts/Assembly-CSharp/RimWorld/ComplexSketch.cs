using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ComplexSketch : IExposable
	{
		public Sketch structure;

		public ComplexLayout layout;

		public List<Thing> thingsToSpawn = new List<Thing>();

		public string thingDiscoveredMessage;

		public ComplexDef complexDef;

		public void ExposeData()
		{
			Scribe_Deep.Look(ref structure, "structure");
			Scribe_Deep.Look(ref layout, "layout");
			Scribe_Collections.Look(ref thingsToSpawn, "thingsToSpawn", LookMode.Deep);
			Scribe_Defs.Look(ref complexDef, "complexDef");
			Scribe_Values.Look(ref thingDiscoveredMessage, "thingDiscoveredMessage");
		}
	}
}
