namespace Verse
{
	public static class ThingCompUtility
	{
		public static T TryGetComp<T>(this Thing thing) where T : ThingComp
		{
			if (!(thing is ThingWithComps thingWithComps))
			{
				return null;
			}
			return thingWithComps.GetComp<T>();
		}
	}
}
