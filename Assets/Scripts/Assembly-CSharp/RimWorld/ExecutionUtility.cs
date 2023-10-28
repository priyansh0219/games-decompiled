using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class ExecutionUtility
	{
		public static void DoExecutionByCut(Pawn executioner, Pawn victim, int bloodPerWeight = 8, bool spawnBlood = true)
		{
			ExecutionInt(executioner, victim, huntingExecution: false, bloodPerWeight, spawnBlood);
		}

		public static void DoHuntingExecution(Pawn executioner, Pawn victim)
		{
			ExecutionInt(executioner, victim, huntingExecution: true);
		}

		private static void ExecutionInt(Pawn executioner, Pawn victim, bool huntingExecution = false, int bloodPerWeight = 8, bool spawnBlood = true)
		{
			if (spawnBlood)
			{
				int num = Mathf.Max(GenMath.RoundRandom(victim.BodySize * (float)bloodPerWeight), 1);
				for (int i = 0; i < num; i++)
				{
					victim.health.DropBloodFilth();
				}
			}
			if (!huntingExecution && victim.RaceProps.Animal && ModsConfig.IdeologyActive)
			{
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.SlaughteredAnimal, executioner.Named(HistoryEventArgsNames.Doer)));
			}
			BodyPartRecord bodyPartRecord = ExecuteCutPart(victim);
			int num2 = Mathf.Clamp((int)victim.health.hediffSet.GetPartHealth(bodyPartRecord) - 1, 1, 20);
			if (ModsConfig.BiotechActive && victim.genes != null && victim.genes.HasGene(GeneDefOf.Deathless))
			{
				num2 = 99999;
				if (victim.health.hediffSet.HasHediff(HediffDefOf.MechlinkImplant))
				{
					Hediff firstHediffOfDef = victim.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.MechlinkImplant);
					if (bodyPartRecord.GetPartAndAllChildParts().Contains(firstHediffOfDef.Part))
					{
						GenSpawn.Spawn(ThingDefOf.Mechlink, victim.Position, victim.Map);
					}
				}
			}
			DamageInfo damageInfo = new DamageInfo(DamageDefOf.ExecutionCut, num2, 999f, -1f, executioner, bodyPartRecord, null, DamageInfo.SourceCategory.ThingOrUnknown, null, instigatorGuilty: false, spawnBlood);
			victim.TakeDamage(damageInfo);
			if (!victim.Dead)
			{
				victim.Kill(damageInfo);
			}
			SoundDefOf.Execute_Cut.PlayOneShot(victim);
		}

		public static BodyPartRecord ExecuteCutPart(Pawn pawn)
		{
			BodyPartRecord bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Neck);
			if (bodyPartRecord != null)
			{
				return bodyPartRecord;
			}
			bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Head);
			if (bodyPartRecord != null)
			{
				return bodyPartRecord;
			}
			bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.InsectHead);
			if (bodyPartRecord != null)
			{
				return bodyPartRecord;
			}
			bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Body);
			if (bodyPartRecord != null)
			{
				return bodyPartRecord;
			}
			bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.SnakeBody);
			if (bodyPartRecord != null)
			{
				return bodyPartRecord;
			}
			bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Torso);
			if (bodyPartRecord != null)
			{
				return bodyPartRecord;
			}
			Log.Error("No good slaughter cut part found for " + pawn);
			return pawn.health.hediffSet.GetNotMissingParts().RandomElementByWeight((BodyPartRecord x) => x.coverageAbsWithChildren);
		}
	}
}
