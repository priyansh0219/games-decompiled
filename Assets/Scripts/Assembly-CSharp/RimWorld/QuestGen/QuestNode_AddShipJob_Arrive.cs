using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_AddShipJob_Arrive : QuestNode_AddShipJob
	{
		public SlateRef<Map> map;

		public SlateRef<IntVec3?> landingCell;

		protected override ShipJobDef DefaultShipJobDef => ShipJobDefOf.Arrive;

		protected override void AddJobVars(ShipJob shipJob, Slate slate)
		{
			if (shipJob is ShipJob_Arrive shipJob_Arrive)
			{
				Map map = this.map.GetValue(slate) ?? slate.Get<Map>("map");
				shipJob_Arrive.mapParent = map.Parent;
				if (landingCell.GetValue(slate).HasValue)
				{
					shipJob_Arrive.cell = landingCell.GetValue(slate).Value;
				}
			}
		}
	}
}
