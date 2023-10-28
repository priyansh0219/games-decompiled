using Verse;

namespace RimWorld.BaseGen
{
	public abstract class SymbolResolver_AncientComplex_Base : SymbolResolver
	{
		protected abstract ComplexDef DefaultComplexDef { get; }

		public void ResolveComplex(ResolveParams rp)
		{
			if (rp.ancientComplexSketch == null)
			{
				rp.ancientComplexSketch = DefaultComplexDef.Worker.GenerateSketch(new IntVec2(rp.rect.Width, rp.rect.Height));
			}
			ResolveParams resolveParams = rp;
			resolveParams.ancientComplexSketch = rp.ancientComplexSketch;
			BaseGen.symbolStack.Push("ancientComplexSketch", resolveParams);
			ResolveParams resolveParams2 = rp;
			resolveParams2.floorDef = TerrainDefOf.Concrete;
			resolveParams2.allowBridgeOnAnyImpassableTerrain = true;
			resolveParams2.floorOnlyIfTerrainSupports = false;
			foreach (ComplexRoom room in rp.ancientComplexSketch.layout.Rooms)
			{
				foreach (CellRect rect in room.rects)
				{
					resolveParams2.rect = rect.MovedBy(rp.rect.BottomLeft);
					BaseGen.symbolStack.Push("floor", resolveParams2);
					BaseGen.symbolStack.Push("clear", resolveParams2);
				}
			}
		}
	}
}
