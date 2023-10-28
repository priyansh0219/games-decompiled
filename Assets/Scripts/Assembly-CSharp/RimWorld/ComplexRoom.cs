using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ComplexRoom : IExposable
	{
		public List<CellRect> rects = new List<CellRect>();

		public ComplexRoomDef def;

		public int Area => rects.Sum((CellRect r) => r.Area);

		public IEnumerable<IntVec3> Corners => rects.SelectMany((CellRect r) => r.Corners);

		public IEnumerable<IntVec3> Cells
		{
			get
			{
				for (int i = 0; i < rects.Count; i++)
				{
					foreach (IntVec3 cell in rects[i].Cells)
					{
						yield return cell;
					}
				}
			}
		}

		public ComplexRoom()
		{
		}

		public ComplexRoom(List<CellRect> rects)
		{
			this.rects = rects;
		}

		public bool IsCorner(IntVec3 position)
		{
			for (int i = 0; i < rects.Count; i++)
			{
				if (rects[i].IsCorner(position))
				{
					return true;
				}
			}
			return false;
		}

		public bool IsAdjacentTo(ComplexRoom room, int minAdjacencyScore = 1)
		{
			foreach (CellRect rect in rects)
			{
				foreach (CellRect rect2 in room.rects)
				{
					if (rect.GetAdjacencyScore(rect2) >= minAdjacencyScore)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool Contains(IntVec3 position)
		{
			for (int i = 0; i < rects.Count; i++)
			{
				if (rects[i].Contains(position))
				{
					return true;
				}
			}
			return false;
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref rects, "rects", LookMode.Value);
			Scribe_Defs.Look(ref def, "def");
		}
	}
}
