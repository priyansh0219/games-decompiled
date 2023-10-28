using System.Collections.Generic;
using RimWorld.SketchGen;
using Verse;

namespace RimWorld
{
	public class ComplexRoomDef : Def
	{
		public SketchResolverDef sketchResolverDef;

		public float selectionWeight = 1f;

		public int maxCount = int.MaxValue;

		public int minArea = 25;

		public int maxArea = int.MaxValue;

		public bool requiresSingleRectRoom;

		public List<TerrainDef> floorTypes;

		public bool CanResolve(ComplexRoomParams parms)
		{
			int area = parms.room.Area;
			if (area >= minArea && area <= maxArea)
			{
				if (requiresSingleRectRoom)
				{
					return parms.room.rects.Count == 1;
				}
				return true;
			}
			return false;
		}

		public void ResolveSketch(ComplexRoomParams parms)
		{
			ResolveParams parms2 = default(ResolveParams);
			TerrainDef terrainDef = ((!floorTypes.NullOrEmpty()) ? floorTypes.RandomElement() : TerrainDefOf.Concrete);
			foreach (CellRect rect in parms.room.rects)
			{
				if (terrainDef != null)
				{
					foreach (IntVec3 item in rect)
					{
						parms.sketch.AddTerrain(terrainDef, item);
					}
				}
				parms2.rect = rect;
				parms2.sketch = parms.sketch;
				sketchResolverDef.Resolve(parms2);
			}
		}
	}
}
