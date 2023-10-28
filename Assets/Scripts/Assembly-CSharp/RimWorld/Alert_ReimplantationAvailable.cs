using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class Alert_ReimplantationAvailable : Alert
	{
		private Pawn WaitingPawn
		{
			get
			{
				foreach (Map map in Find.Maps)
				{
					foreach (Lord lord in map.lordManager.lords)
					{
						if (lord.CurLordToil is LordToil_ReimplantXenogerm lordToil_ReimplantXenogerm)
						{
							return lordToil_ReimplantXenogerm.Data.target;
						}
					}
				}
				return null;
			}
		}

		public Alert_ReimplantationAvailable()
		{
			defaultPriority = AlertPriority.High;
			defaultLabel = "AlertReimplantationAvailable".Translate();
		}

		public override TaggedString GetExplanation()
		{
			return "AlertReimplantationAvailableDesc".Translate(WaitingPawn);
		}

		public override AlertReport GetReport()
		{
			if (!ModsConfig.BiotechActive)
			{
				return false;
			}
			return AlertReport.CulpritIs(WaitingPawn);
		}
	}
}
