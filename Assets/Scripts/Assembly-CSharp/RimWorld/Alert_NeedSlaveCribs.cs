using Verse;

namespace RimWorld
{
	public class Alert_NeedSlaveCribs : Alert
	{
		public Alert_NeedSlaveCribs()
		{
			defaultLabel = "AlertNeedSlaveCribs".Translate();
			defaultExplanation = "AlertNeedSlaveCribsDesc".Translate();
			defaultPriority = AlertPriority.High;
		}

		public override AlertReport GetReport()
		{
			if (!ModsConfig.BiotechActive || !ModsConfig.IdeologyActive)
			{
				return false;
			}
			foreach (Map map in Find.Maps)
			{
				if (map.IsPlayerHome)
				{
					Alert_NeedSlaveBeds.CheckSlaveBeds(map, out var _, out var enoughBabyCribs);
					if (!enoughBabyCribs)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
