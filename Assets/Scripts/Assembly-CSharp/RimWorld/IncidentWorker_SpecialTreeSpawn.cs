using Verse;

namespace RimWorld
{
	public class IncidentWorker_SpecialTreeSpawn : IncidentWorker
	{
		private GenStep_SpecialTrees genStep;

		protected GenStep_SpecialTrees GenStep
		{
			get
			{
				if (genStep == null)
				{
					genStep = (GenStep_SpecialTrees)def.treeGenStepDef.genStep;
				}
				return genStep;
			}
		}

		protected virtual bool SendLetter => true;

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
			Map map = (Map)parms.target;
			if (map.Biome.isExtremeBiome)
			{
				return false;
			}
			int num = GenStep.DesiredTreeCountForMap(map);
			if (map.listerThings.ThingsOfDef(def.treeDef).Count >= num)
			{
				return false;
			}
			IntVec3 cell;
			return TryFindRootCell(map, out cell);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!TryFindRootCell(map, out var cell))
			{
				return false;
			}
			if (!GenStep.TrySpawnAt(cell, map, def.treeGrowth, out var plant))
			{
				return false;
			}
			if (SendLetter)
			{
				SendStandardLetter(parms, plant);
			}
			return true;
		}

		protected virtual bool TryFindRootCell(Map map, out IntVec3 cell)
		{
			if (CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 x) => GenStep.CanSpawnAt(x, map), map, out cell))
			{
				return true;
			}
			return CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 x) => GenStep.CanSpawnAt(x, map, 10, 0, 18, 20), map, out cell);
		}
	}
}
