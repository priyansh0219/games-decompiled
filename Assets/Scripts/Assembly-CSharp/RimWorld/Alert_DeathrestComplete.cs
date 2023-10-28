using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Alert_DeathrestComplete : Alert
	{
		private List<Pawn> targets = new List<Pawn>();

		public Alert_DeathrestComplete()
		{
			defaultLabel = "AlertDeathrestComplete".Translate();
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
			return "AlertDeathrestCompleteDesc".Translate() + ":\n\n" + targets.Select((Pawn p) => p.LabelCap.Colorize(ColoredText.NameColor)).ToLineList(" - ");
		}

		private void GetTargets()
		{
			targets.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				List<Pawn> list = maps[i].mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j].genes != null && list[j].Deathresting)
					{
						Gene_Deathrest firstGeneOfType = list[j].genes.GetFirstGeneOfType<Gene_Deathrest>();
						if (firstGeneOfType != null && firstGeneOfType.ShowWakeAlert)
						{
							targets.Add(list[j]);
						}
					}
				}
			}
		}
	}
}
