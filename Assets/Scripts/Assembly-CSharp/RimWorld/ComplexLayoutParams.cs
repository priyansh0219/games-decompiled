using Verse;

namespace RimWorld
{
	public struct ComplexLayoutParams
	{
		public CellRect container;

		public int minRoomWidth;

		public int minRoomHeight;

		public float areaPrunePercent;

		public IntRange mergeRoomsRange;

		public int entranceCount;
	}
}
