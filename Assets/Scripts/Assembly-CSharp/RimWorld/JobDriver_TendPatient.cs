using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_TendPatient : JobDriver
	{
		private bool usesMedicine;

		private const int BaseTendDuration = 600;

		private const int TicksBetweenSelfTendMotes = 100;

		private const TargetIndex MedicineHolderIndex = TargetIndex.C;

		protected Thing MedicineUsed => job.targetB.Thing;

		protected Pawn Deliveree => job.targetA.Pawn;

		protected bool IsMedicineInDoctorInventory
		{
			get
			{
				if (MedicineUsed != null)
				{
					return pawn.inventory.Contains(MedicineUsed);
				}
				return false;
			}
		}

		protected Pawn_InventoryTracker MedicineHolderInventory => MedicineUsed?.ParentHolder as Pawn_InventoryTracker;

		protected Pawn OtherPawnMedicineHolder => job.targetC.Pawn;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref usesMedicine, "usesMedicine", defaultValue: false);
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			usesMedicine = MedicineUsed != null;
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (Deliveree != pawn && !pawn.Reserve(Deliveree, job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			if (usesMedicine)
			{
				int num = pawn.Map.reservationManager.CanReserveStack(pawn, MedicineUsed, 10);
				if (num <= 0 || !pawn.Reserve(MedicineUsed, job, 10, Mathf.Min(num, Medicine.GetMedicineCountToFullyHeal(Deliveree)), null, errorOnFailed))
				{
					return false;
				}
			}
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOn(delegate
			{
				if (MedicineUsed != null && pawn.Faction == Faction.OfPlayer && Deliveree.playerSettings != null && !Deliveree.playerSettings.medCare.AllowsMedicine(MedicineUsed.def))
				{
					return true;
				}
				return (pawn == Deliveree && pawn.Faction == Faction.OfPlayer && pawn.playerSettings != null && !pawn.playerSettings.selfTend) ? true : false;
			});
			AddEndCondition(delegate
			{
				if (pawn.Faction == Faction.OfPlayer && HealthAIUtility.ShouldBeTendedNowByPlayer(Deliveree))
				{
					return JobCondition.Ongoing;
				}
				return ((job.playerForced || pawn.Faction != Faction.OfPlayer) && Deliveree.health.HasHediffsNeedingTend()) ? JobCondition.Ongoing : JobCondition.Succeeded;
			});
			this.FailOnAggroMentalState(TargetIndex.A);
			Toil reserveMedicine = null;
			PathEndMode interactionCell = PathEndMode.None;
			if (Deliveree == pawn)
			{
				interactionCell = PathEndMode.OnCell;
			}
			else if (Deliveree.InBed())
			{
				interactionCell = PathEndMode.InteractionCell;
			}
			else if (Deliveree != pawn)
			{
				interactionCell = PathEndMode.ClosestTouch;
			}
			Toil gotoToil = Toils_Goto.GotoThing(TargetIndex.A, interactionCell);
			if (usesMedicine)
			{
				reserveMedicine = Toils_Tend.ReserveMedicine(TargetIndex.B, Deliveree).FailOnDespawnedNullOrForbidden(TargetIndex.B);
				yield return Toils_Jump.JumpIf(gotoToil, () => IsMedicineInDoctorInventory);
				Toil goToMedicineHolder = Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch).FailOn(() => OtherPawnMedicineHolder != MedicineHolderInventory?.pawn || OtherPawnMedicineHolder.IsForbidden(pawn));
				yield return Toils_Haul.CheckItemCarriedByOtherPawn(MedicineUsed, TargetIndex.C, goToMedicineHolder);
				yield return reserveMedicine;
				yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B);
				yield return Toils_Tend.PickupMedicine(TargetIndex.B, Deliveree).FailOnDestroyedOrNull(TargetIndex.B);
				yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveMedicine, TargetIndex.B, TargetIndex.None, takeFromValidStorage: true);
				yield return Toils_Jump.Jump(gotoToil);
				yield return goToMedicineHolder;
				yield return Toils_General.Wait(25).WithProgressBarToilDelay(TargetIndex.C);
				yield return Toils_Haul.TakeFromOtherInventory(MedicineUsed, pawn.inventory.innerContainer, MedicineHolderInventory?.innerContainer, Medicine.GetMedicineCountToFullyHeal(Deliveree), TargetIndex.B);
			}
			yield return gotoToil;
			int ticks = (int)(1f / pawn.GetStatValue(StatDefOf.MedicalTendSpeed) * 600f);
			Toil waitToil;
			if (!job.draftedTend)
			{
				waitToil = Toils_General.Wait(ticks);
			}
			else
			{
				waitToil = Toils_General.WaitWith(TargetIndex.A, ticks, useProgressBar: false, maintainPosture: true);
				waitToil.AddFinishAction(delegate
				{
					if (Deliveree != null && Deliveree != pawn && Deliveree.CurJob != null && (Deliveree.CurJob.def == JobDefOf.Wait || Deliveree.CurJob.def == JobDefOf.Wait_MaintainPosture))
					{
						Deliveree.jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
				});
			}
			waitToil.FailOnCannotTouch(TargetIndex.A, interactionCell).WithProgressBarToilDelay(TargetIndex.A).PlaySustainerOrSound(SoundDefOf.Interact_Tend);
			waitToil.activeSkill = () => SkillDefOf.Medicine;
			waitToil.handlingFacing = true;
			waitToil.tickAction = delegate
			{
				if (pawn == Deliveree && pawn.Faction != Faction.OfPlayer && pawn.IsHashIntervalTick(100) && !pawn.Position.Fogged(pawn.Map))
				{
					FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.HealingCross);
				}
				if (pawn != Deliveree)
				{
					pawn.rotationTracker.FaceTarget(Deliveree);
				}
			};
			yield return Toils_Jump.JumpIf(waitToil, () => !usesMedicine || !IsMedicineInDoctorInventory);
			yield return Toils_Tend.PickupMedicine(TargetIndex.B, Deliveree).FailOnDestroyedOrNull(TargetIndex.B);
			yield return waitToil;
			yield return Toils_Tend.FinalizeTend(Deliveree);
			if (usesMedicine)
			{
				Toil toil = ToilMaker.MakeToil("MakeNewToils");
				toil.initAction = delegate
				{
					if (MedicineUsed.DestroyedOrNull())
					{
						Thing thing = HealthAIUtility.FindBestMedicine(pawn, Deliveree);
						if (thing != null)
						{
							job.targetB = thing;
							JumpToToil(reserveMedicine);
						}
					}
				};
				yield return toil;
			}
			yield return Toils_Jump.Jump(gotoToil);
		}

		public override void Notify_DamageTaken(DamageInfo dinfo)
		{
			base.Notify_DamageTaken(dinfo);
			if (dinfo.Def.ExternalViolenceFor(pawn) && pawn.Faction != Faction.OfPlayer && pawn == Deliveree)
			{
				pawn.jobs.CheckForJobOverride();
			}
		}
	}
}
