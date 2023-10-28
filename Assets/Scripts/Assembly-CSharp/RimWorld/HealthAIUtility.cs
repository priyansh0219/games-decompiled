using System;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class HealthAIUtility
	{
		public static bool ShouldSeekMedicalRestUrgent(Pawn pawn)
		{
			if ((!pawn.Downed || LifeStageUtility.AlwaysDowned(pawn)) && !pawn.health.HasHediffsNeedingTend() && !ShouldHaveSurgeryDoneNow(pawn))
			{
				return pawn.health.hediffSet.InLabor();
			}
			return true;
		}

		public static bool ShouldSeekMedicalRest(Pawn pawn)
		{
			if (!ShouldSeekMedicalRestUrgent(pawn) && !pawn.health.hediffSet.HasTendedAndHealingInjury())
			{
				return pawn.health.hediffSet.HasImmunizableNotImmuneHediff();
			}
			return true;
		}

		public static bool ShouldBeTendedNowByPlayerUrgent(Pawn pawn)
		{
			if (ShouldBeTendedNowByPlayer(pawn))
			{
				return HealthUtility.TicksUntilDeathDueToBloodLoss(pawn) < 45000;
			}
			return false;
		}

		public static bool ShouldBeTendedNowByPlayer(Pawn pawn)
		{
			if (pawn.playerSettings == null)
			{
				return false;
			}
			if (!ShouldEverReceiveMedicalCareFromPlayer(pawn))
			{
				return false;
			}
			return pawn.health.HasHediffsNeedingTendByPlayer();
		}

		public static bool ShouldEverReceiveMedicalCareFromPlayer(Pawn pawn)
		{
			Pawn_PlayerSettings playerSettings = pawn.playerSettings;
			if (playerSettings != null && playerSettings.medCare == MedicalCareCategory.NoCare)
			{
				return false;
			}
			if (pawn.guest?.interactionMode == PrisonerInteractionModeDefOf.Execution)
			{
				return false;
			}
			if (pawn.ShouldBeSlaughtered())
			{
				return false;
			}
			return true;
		}

		public static bool ShouldHaveSurgeryDoneNow(Pawn pawn)
		{
			if (pawn.health.surgeryBills.AnyShouldDoNow)
			{
				return WorkGiver_PatientGoToBedTreatment.AnyAvailableDoctorFor(pawn);
			}
			return false;
		}

		public static bool WantsToBeRescuedIfDowned(Pawn pawn)
		{
			if (LifeStageUtility.AlwaysDowned(pawn))
			{
				return ShouldSeekMedicalRest(pawn);
			}
			return true;
		}

		public static Thing FindBestMedicine(Pawn healer, Pawn patient, bool onlyUseInventory = false)
		{
			if (patient.playerSettings != null && (int)patient.playerSettings.medCare <= 1)
			{
				return null;
			}
			if (Medicine.GetMedicineCountToFullyHeal(patient) <= 0)
			{
				return null;
			}
			Predicate<Thing> validator = (Thing m) => (!m.IsForbidden(healer) && (patient.playerSettings == null || patient.playerSettings.medCare.AllowsMedicine(m.def)) && healer.CanReserve(m, 10, 1)) ? true : false;
			Thing thing = GetBestMedInInventory(healer.inventory.innerContainer);
			if (onlyUseInventory)
			{
				return thing;
			}
			Thing thing2 = GenClosest.ClosestThing_Global_Reachable(patient.Position, patient.Map, patient.Map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine), PathEndMode.ClosestTouch, TraverseParms.For(healer), 9999f, validator, PriorityOf);
			if (thing != null && thing2 != null)
			{
				if (!(PriorityOf(thing) >= PriorityOf(thing2)))
				{
					return thing2;
				}
				return thing;
			}
			if (thing == null && thing2 == null && healer.IsColonist && healer.Map != null)
			{
				Thing thing3 = null;
				foreach (Pawn spawnedColonyAnimal in healer.Map.mapPawns.SpawnedColonyAnimals)
				{
					thing3 = GetBestMedInInventory(spawnedColonyAnimal.inventory.innerContainer);
					if (thing3 != null && (thing2 == null || PriorityOf(thing2) < PriorityOf(thing3)) && !spawnedColonyAnimal.IsForbidden(healer) && healer.CanReach(spawnedColonyAnimal, PathEndMode.OnCell, Danger.Some))
					{
						thing2 = thing3;
					}
				}
			}
			return thing ?? thing2;
			Thing GetBestMedInInventory(ThingOwner inventory)
			{
				if (inventory.Count == 0)
				{
					return null;
				}
				return inventory.Where((Thing t) => t.def.IsMedicine && validator(t)).OrderByDescending(PriorityOf).FirstOrDefault();
			}
			float PriorityOf(Thing t)
			{
				return t.def.GetStatValueAbstract(StatDefOf.MedicalPotency);
			}
		}
	}
}
