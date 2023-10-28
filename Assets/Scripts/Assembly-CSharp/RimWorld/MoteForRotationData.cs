using Verse;

namespace RimWorld
{
	public class MoteForRotationData
	{
		public ThingDef north;

		public ThingDef south;

		public ThingDef east;

		public ThingDef west;

		public ThingDef GetForRotation(Rot4 rot)
		{
			switch (rot.AsInt)
			{
			case 0:
				return north;
			case 1:
				return east;
			case 2:
				return south;
			case 3:
				return west;
			default:
				return null;
			}
		}
	}
}
