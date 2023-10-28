using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_SlavesUnattended : Alert
	{
		private List<Pawn> targetsResult = new List<Pawn>();

		public List<Pawn> Targets
		{
			get
			{
				targetsResult.Clear();
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					if (SlaveRebellionUtility.IsUnattendedByColonists(maps[i]))
					{
						targetsResult.AddRange(maps[i].mapPawns.SlavesOfColonySpawned);
					}
				}
				return targetsResult;
			}
		}

		public Alert_SlavesUnattended()
		{
			defaultLabel = "SlaveUnattendedLabel".Translate();
			defaultPriority = AlertPriority.High;
		}

		public override string GetLabel()
		{
			if (Targets.Count == 1)
			{
				return "SlaveUnattendedLabel".Translate();
			}
			return "SlaveUnattendedMultipleLabel".Translate();
		}

		public override AlertReport GetReport()
		{
			if (!ModsConfig.IdeologyActive)
			{
				return false;
			}
			return AlertReport.CulpritsAre(Targets);
		}

		public override TaggedString GetExplanation()
		{
			Pawn pawn = Targets[0];
			return "SlavesUnattendedDesc".Translate(pawn);
		}
	}
}
