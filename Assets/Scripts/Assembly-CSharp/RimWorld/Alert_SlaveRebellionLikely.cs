using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_SlaveRebellionLikely : Alert
	{
		private const float ReportAnySlaveMtbThreshold = 15f;

		public Alert_SlaveRebellionLikely()
		{
			defaultLabel = "AlertSlaveRebellionLikely".Translate();
		}

		public override TaggedString GetExplanation()
		{
			Map currentMap = Find.CurrentMap;
			if (currentMap == null)
			{
				return "";
			}
			_ = currentMap.mapPawns.SlavesOfColonySpawned.Count;
			int num = currentMap.mapPawns.SlavesOfColonySpawned.Where((Pawn pawn) => MTBMeetsRebelliousThreshold(SlaveRebellionUtility.InitiateSlaveRebellionMtbDays(pawn))).Count();
			Pawn mostRebelliousPawn = GetMostRebelliousPawn();
			int numTicks = (int)SlaveRebellionUtility.InitiateSlaveRebellionMtbDays(mostRebelliousPawn) * 60000;
			int numTicks2 = (int)SlaveRebellionUtility.RebellionForAnySlaveInMapMtbDays(currentMap) * 60000;
			StringBuilder stringBuilder = new StringBuilder();
			if (num >= 2)
			{
				stringBuilder.Append("AlertSlaveRebellionLikelyRebelliousCount".Translate(num.Named("REBELLIOUSCOUNT")) + " ");
			}
			stringBuilder.Append("AlertSlaveRebellionLikelyMostRebellious".Translate(numTicks2.ToStringTicksToPeriod().Named("COMBINEDTIME"), mostRebelliousPawn.Named("REBEL"), numTicks.ToStringTicksToPeriod().Named("INDIVIDUALTIME")));
			stringBuilder.Append("\n\n" + SlaveRebellionUtility.GetSlaveRebellionMtbCalculationExplanation(mostRebelliousPawn));
			return stringBuilder.ToString();
		}

		public override AlertReport GetReport()
		{
			if (!ModsConfig.IdeologyActive || Find.CurrentMap == null)
			{
				return false;
			}
			float mtb = SlaveRebellionUtility.RebellionForAnySlaveInMapMtbDays(Find.CurrentMap);
			if (!MTBMeetsRebelliousThreshold(mtb))
			{
				return false;
			}
			return AlertReport.CulpritIs(GetMostRebelliousPawn());
		}

		private Pawn GetMostRebelliousPawn()
		{
			IEnumerable<Pawn> source = Find.CurrentMap.mapPawns.SlavesOfColonySpawned.Where((Pawn pawn) => SlaveRebellionUtility.InitiateSlaveRebellionMtbDays(pawn) > 0f);
			if (source.Count() != 0)
			{
				return source.MinBy((Pawn pawn) => SlaveRebellionUtility.InitiateSlaveRebellionMtbDays(pawn));
			}
			return null;
		}

		private bool MTBMeetsRebelliousThreshold(float mtb)
		{
			if (15f > mtb)
			{
				return mtb > 0f;
			}
			return false;
		}
	}
}
