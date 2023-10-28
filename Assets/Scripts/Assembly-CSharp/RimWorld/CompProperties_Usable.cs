using Verse;

namespace RimWorld
{
	public class CompProperties_Usable : CompProperties
	{
		public JobDef useJob;

		[MustTranslate]
		public string useLabel;

		public int useDuration = 100;

		public HediffDef userMustHaveHediff;

		public MenuOptionPriority floatMenuOptionPriority = MenuOptionPriority.Default;

		public FactionDef floatMenuFactionIcon;

		public ThingDef warmupMote;

		public ThingDef finalizeMote;

		public bool ignoreOtherReservations;

		public CompProperties_Usable()
		{
			compClass = typeof(CompUsable);
		}
	}
}
