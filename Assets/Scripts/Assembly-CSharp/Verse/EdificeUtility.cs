namespace Verse
{
	public static class EdificeUtility
	{
		public static bool IsEdifice(this BuildableDef def)
		{
			if (def is ThingDef thingDef && thingDef.category == ThingCategory.Building)
			{
				return thingDef.building.isEdifice;
			}
			return false;
		}
	}
}
