using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Alert_MechDamaged : Alert
	{
		private List<Pawn> targets = new List<Pawn>();

		private void GetTargets()
		{
			targets.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				List<Pawn> list = maps[i].mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j].IsColonyMech && MechRepairUtility.CanRepair(list[j]))
					{
						targets.Add(list[j]);
					}
				}
			}
		}

		public Alert_MechDamaged()
		{
			defaultLabel = "AlertMechNeedsRepair".Translate();
		}

		public override AlertReport GetReport()
		{
			if (!ModsConfig.BiotechActive)
			{
				return false;
			}
			GetTargets();
			return AlertReport.CulpritsAre(targets);
		}

		public override TaggedString GetExplanation()
		{
			return "AlertMechNeedsRepairDescPrefix".Translate() + ":\n\n" + targets.Select((Pawn p) => p.LabelCap).ToLineList(" - ") + "\n\n" + "AlertMechNeedsRepairDesc".Translate();
		}
	}
}
