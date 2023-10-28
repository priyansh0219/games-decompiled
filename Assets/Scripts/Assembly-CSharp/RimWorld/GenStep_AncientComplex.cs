using System.Collections.Generic;
using RimWorld.BaseGen;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class GenStep_AncientComplex : GenStep_ScattererBestFit
	{
		private ComplexSketch sketch;

		private static readonly IntVec2 DefaultComplexSize = new IntVec2(80, 80);

		protected override IntVec2 Size => new IntVec2(sketch.layout.container.Width + 10, sketch.layout.container.Height + 10);

		public override int SeedPart => 235635649;

		public override bool CollisionAt(IntVec3 cell, Map map)
		{
			List<Thing> thingList = cell.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].def.IsBuildingArtificial)
				{
					return true;
				}
			}
			return false;
		}

		public override void Generate(Map map, GenStepParams parms)
		{
			count = 1;
			nearMapCenter = true;
			sketch = parms.sitePart.parms.ancientComplexSketch;
			if (sketch == null)
			{
				sketch = ComplexDefOf.AncientComplex.Worker.GenerateSketch(DefaultComplexSize);
				Log.Warning("No ancient complex found in sitepart parms, generating default ancient complex.");
			}
			base.Generate(map, parms);
		}

		protected override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
		{
			ResolveParams parms2 = default(ResolveParams);
			parms2.ancientComplexSketch = sketch;
			parms2.threatPoints = parms.sitePart.parms.threatPoints;
			parms2.rect = CellRect.CenteredOn(c, sketch.layout.container.Width, sketch.layout.container.Height);
			parms2.thingSetMakerDef = parms.sitePart.parms.ancientComplexRewardMaker;
			FormCaravanComp component = parms.sitePart.site.GetComponent<FormCaravanComp>();
			if (component != null)
			{
				component.foggedRoomsCheckRect = parms2.rect;
			}
			GenerateComplex(map, parms2);
		}

		protected virtual void GenerateComplex(Map map, ResolveParams parms)
		{
			RimWorld.BaseGen.BaseGen.globalSettings.map = map;
			RimWorld.BaseGen.BaseGen.symbolStack.Push("ancientComplex", parms);
			RimWorld.BaseGen.BaseGen.Generate();
		}
	}
}
