using System.Collections.Generic;
using System.Linq;
using RimWorld.BaseGen;
using Verse;

namespace RimWorld.SketchGen
{
	public class SketchResolver_AncientUtilityBuilding : SketchResolver
	{
		protected override bool CanResolveInt(ResolveParams parms)
		{
			return parms.sketch != null;
		}

		protected override void ResolveInt(ResolveParams parms)
		{
			if (!ModLister.CheckIdeology("Ancient utility building"))
			{
				return;
			}
			Sketch sketch = new Sketch();
			IntVec2 intVec = parms.utilityBuildingSize ?? new IntVec2(10, 10);
			ComplexLayout complexLayout = ComplexLayoutGenerator.GenerateRandomLayout(new CellRect(0, 0, intVec.x, intVec.z), 4, 4, 0f, IntRange.zero);
			ThingDef stuff = BaseGenUtility.RandomCheapWallStuff(Faction.OfAncients, notVeryFlammable: true);
			for (int i = complexLayout.container.minX; i <= complexLayout.container.maxX; i++)
			{
				for (int j = complexLayout.container.minZ; j <= complexLayout.container.maxZ; j++)
				{
					IntVec3 intVec2 = new IntVec3(i, 0, j);
					if (complexLayout.IsWallAt(intVec2))
					{
						sketch.AddThing(ThingDefOf.Wall, intVec2, Rot4.North, stuff);
					}
					else if (complexLayout.IsDoorAt(intVec2))
					{
						sketch.AddThing(ThingDefOf.Door, intVec2, Rot4.North, stuff);
					}
				}
			}
			List<ComplexRoom> rooms = complexLayout.Rooms;
			rooms.SortByDescending((ComplexRoom a) => a.Area);
			ComplexRoom complexRoom = null;
			for (int k = 0; k < rooms.Count; k++)
			{
				if (!complexLayout.IsAdjacentToLayoutEdge(rooms[k]))
				{
					continue;
				}
				foreach (IntVec3 cell in rooms[k].Cells)
				{
					if (complexLayout.container.IsOnEdge(cell) && complexLayout.IsWallAt(cell))
					{
						sketch.AddThing(ThingDefOf.AncientFence, cell, Rot4.North);
					}
				}
				IEnumerable<IntVec3> cellsToCheck = rooms[k].rects.SelectMany((CellRect r) => r.Cells).InRandomOrder();
				if (TryFindThingPositionWithGap(ThingDefOf.AncientGenerator, cellsToCheck, sketch, out var position))
				{
					sketch.AddThing(ThingDefOf.AncientGenerator, position, ThingDefOf.AncientGenerator.defaultPlacingRot);
				}
				foreach (CellRect rect in rooms[k].rects)
				{
					foreach (IntVec3 item in rect)
					{
						sketch.AddTerrain(TerrainDefOf.Concrete, item);
					}
				}
				complexRoom = rooms[k];
				break;
			}
			for (int l = 0; l < rooms.Count; l++)
			{
				if (rooms[l] == complexRoom)
				{
					continue;
				}
				foreach (IntVec3 item2 in rooms[l].rects.SelectMany((CellRect r) => r.Cells))
				{
					sketch.AddTerrain(TerrainDefOf.Concrete, item2);
				}
			}
			parms.sketch.Merge(sketch);
			ResolveParams parms2 = parms;
			parms2.wallEdgeThing = ThingDefOf.Table1x2c;
			parms2.requireFloor = true;
			parms2.allowWood = false;
			SketchResolverDefOf.AddWallEdgeThings.Resolve(parms2);
			ResolveParams parms3 = parms;
			parms3.destroyChanceExp = 1.5f;
			SketchResolverDefOf.DamageBuildings.Resolve(parms3);
		}

		private bool TryFindThingPositionWithGap(ThingDef thingDef, IEnumerable<IntVec3> cellsToCheck, Sketch sketch, out IntVec3 position, int gap = 1)
		{
			foreach (IntVec3 item in cellsToCheck)
			{
				CellRect cellRect = GenAdj.OccupiedRect(item, thingDef.defaultPlacingRot, thingDef.size).ExpandedBy(gap);
				bool flag = true;
				foreach (IntVec3 cell in cellRect.Cells)
				{
					if (sketch.EdificeAt(cell) != null)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					position = item;
					return true;
				}
			}
			position = IntVec3.Invalid;
			return false;
		}
	}
}
