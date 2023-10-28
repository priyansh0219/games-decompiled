using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public static class GasUtility
	{
		public const float BlindingGasAccuracyPenalty = 0.7f;

		public const int RotStinkPerRawMeatRotting = 2;

		private const float RotStinkPerBodySize = 52f;

		private const float RotStinkHumanlikeFactor = 1.15f;

		private const int GasCheckInterval = 50;

		private const float ToxGasEffectOnExtremeBuildupFactor = 0.25f;

		public static string GetLabel(this GasType gasType)
		{
			switch (gasType)
			{
			case GasType.BlindSmoke:
				return "BlindSmoke".Translate();
			case GasType.ToxGas:
				return "ToxGas".Translate();
			case GasType.RotStink:
				return "RotStink".Translate();
			default:
				Log.ErrorOnce("Trying to get unknown gas type label.", 172091);
				return string.Empty;
			}
		}

		public static void AddGas(IntVec3 cell, Map map, GasType gasType, float radius)
		{
			int num = GenRadial.NumCellsInRadius(radius);
			map.gasGrid.AddGas(cell, gasType, 255 * num);
		}

		public static void AddGas(IntVec3 cell, Map map, GasType gasType, int amount)
		{
			map.gasGrid.AddGas(cell, gasType, amount);
		}

		public static byte GasDentity(this IntVec3 cell, Map map, GasType gasType)
		{
			return map.gasGrid.DensityAt(cell, gasType);
		}

		public static bool AnyGas(this IntVec3 cell, Map map, GasType gasType)
		{
			return map.gasGrid.DensityAt(cell, gasType) > 0;
		}

		public static int RotStinkToSpawnForCorpse(Corpse corpse)
		{
			if (GenTemperature.RotRateAtTemperature(Mathf.RoundToInt(corpse.AmbientTemperature)) <= 0f)
			{
				return 0;
			}
			if (corpse.GetRotStage() == RotStage.Rotting)
			{
				float num = corpse.InnerPawn.BodySize;
				if (corpse.InnerPawn.RaceProps.Humanlike)
				{
					num *= 1.15f;
				}
				return Mathf.CeilToInt(num * 52f);
			}
			return 0;
		}

		public static void PawnGasEffectsTick(Pawn pawn)
		{
			if (!ModsConfig.BiotechActive || !pawn.Spawned || !pawn.IsHashIntervalTick(50))
			{
				return;
			}
			byte b = pawn.Position.GasDentity(pawn.Map, GasType.ToxGas);
			if (b > 0)
			{
				float num = (float)(int)b / 255f;
				Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ToxicBuildup);
				if (firstHediffOfDef != null && firstHediffOfDef.CurStageIndex == firstHediffOfDef.def.stages.Count - 1)
				{
					num *= 0.25f;
				}
				if (ShouldGetGasExposureHediff(pawn))
				{
					pawn.health.AddHediff(HediffDefOf.ToxGasExposure);
				}
				GameCondition_ToxicFallout.DoPawnToxicDamage(pawn, protectedByRoof: false, num);
			}
			if (pawn.Spawned && pawn.Position.GasDentity(pawn.Map, GasType.RotStink) > 0 && (pawn.RaceProps.Animal || pawn.RaceProps.Humanlike) && !pawn.health.hediffSet.HasHediff(HediffDefOf.LungRotExposure) && GetLungRotAffectedBodyParts(pawn).Any())
			{
				pawn.health.AddHediff(HediffDefOf.LungRotExposure);
			}
		}

		public static IEnumerable<BodyPartRecord> GetLungRotAffectedBodyParts(Pawn pawn)
		{
			return from p in pawn.health.hediffSet.GetNotMissingParts()
				where p.def == BodyPartDefOf.Lung && !pawn.health.hediffSet.hediffs.Any((Hediff x) => x.Part == p && x.def.preventsLungRot)
				select p;
		}

		private static bool ShouldGetGasExposureHediff(Pawn pawn)
		{
			if (IsEffectedByExposure(pawn))
			{
				return !pawn.health.hediffSet.HasHediff(HediffDefOf.ToxGasExposure);
			}
			return false;
		}

		public static bool IsEffectedByExposure(Pawn pawn)
		{
			if (pawn.RaceProps.Humanlike || pawn.RaceProps.Humanlike)
			{
				if (pawn.apparel != null)
				{
					foreach (Apparel item in pawn.apparel.WornApparel)
					{
						if (item.def.apparel.immuneToToxGasExposure)
						{
							return false;
						}
					}
				}
				if (pawn.genes != null)
				{
					foreach (Gene item2 in pawn.genes.GenesListForReading)
					{
						if (item2.def.immuneToToxGasExposure)
						{
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}
	}
}
