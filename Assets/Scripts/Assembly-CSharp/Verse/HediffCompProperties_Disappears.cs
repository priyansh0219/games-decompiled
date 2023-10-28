namespace Verse
{
	public class HediffCompProperties_Disappears : HediffCompProperties
	{
		public IntRange disappearsAfterTicks;

		public bool showRemainingTime;

		public bool canUseDecimalsShortForm;

		public MentalStateDef requiredMentalState;

		[MustTranslate]
		public string messageOnDisappear;

		public HediffCompProperties_Disappears()
		{
			compClass = typeof(HediffComp_Disappears);
		}
	}
}
