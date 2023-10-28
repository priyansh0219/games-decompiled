namespace RimWorld.BaseGen
{
	public class SymbolResolver_AncientComplex_Sketch : SymbolResolver
	{
		public override bool CanResolve(ResolveParams rp)
		{
			if (base.CanResolve(rp))
			{
				return rp.ancientComplexSketch != null;
			}
			return false;
		}

		public override void Resolve(ResolveParams rp)
		{
			rp.ancientComplexSketch.complexDef.Worker.Spawn(rp.ancientComplexSketch, BaseGen.globalSettings.map, rp.rect.BottomLeft, rp.threatPoints);
		}
	}
}
