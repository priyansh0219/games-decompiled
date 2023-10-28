using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.AI.Group;

namespace Verse
{
	public class Hediff_LaborPushing : HediffWithParents
	{
		public int laborDuration;

		private Effecter progressBar;

		private OutcomeChance debugForceBirthOutcome;

		public float Progress
		{
			get
			{
				HediffComp_Disappears hediffComp_Disappears = this.TryGetComp<HediffComp_Disappears>();
				return 1f - (float)hediffComp_Disappears.ticksToDisappear / (float)Mathf.Max(1, hediffComp_Disappears.disappearsAfterTicks + laborDuration);
			}
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (DebugSettings.ShowDevGizmos)
			{
				yield return PregnancyUtility.BirthQualityGizmo(pawn);
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "DEV: Force stillborn";
				command_Action.action = delegate
				{
					ForceBirth(-1);
				};
				yield return command_Action;
				Command_Action command_Action2 = new Command_Action();
				command_Action2.defaultLabel = "DEV: Force infant illness";
				command_Action2.action = delegate
				{
					ForceBirth(0);
				};
				yield return command_Action2;
				Command_Action command_Action3 = new Command_Action();
				command_Action3.defaultLabel = "DEV: Force healthy";
				command_Action3.action = delegate
				{
					ForceBirth(1);
				};
				yield return command_Action3;
				Command_Action command_Action4 = new Command_Action();
				command_Action4.defaultLabel = "DEV: Force end";
				command_Action4.action = delegate
				{
					pawn.health.RemoveHediff(this);
				};
				yield return command_Action4;
			}
			void ForceBirth(int positivityIndex)
			{
				Precept_Ritual precept_Ritual = (Precept_Ritual)pawn.Ideo.GetPrecept(PreceptDefOf.ChildBirth);
				debugForceBirthOutcome = precept_Ritual.outcomeEffect.def.outcomeChances.FirstOrDefault((OutcomeChance o) => o.positivityIndex == positivityIndex);
				pawn.health.RemoveHediff(this);
			}
		}

		public override void PreRemoved()
		{
			base.PreRemoved();
			LordJob_Ritual lordJob_Ritual = pawn.GetLord()?.LordJob as LordJob_Ritual;
			Precept_Ritual precept_Ritual = (Precept_Ritual)pawn.Ideo.GetPrecept(PreceptDefOf.ChildBirth);
			if (lordJob_Ritual == null || lordJob_Ritual.Ritual == null || lordJob_Ritual.Ritual.def != PreceptDefOf.ChildBirth || lordJob_Ritual.assignments.FirstAssignedPawn("mother") != pawn)
			{
				float birthQualityFor = PregnancyUtility.GetBirthQualityFor(pawn);
				PregnancyUtility.ApplyBirthOutcome(debugForceBirthOutcome ?? ((RitualOutcomeEffectWorker_ChildBirth)precept_Ritual.outcomeEffect).GetOutcome(birthQualityFor, null), assignments: PregnancyUtility.RitualAssignmentsForBirth(precept_Ritual, pawn), quality: birthQualityFor, ritual: precept_Ritual, genes: geneSet?.GenesListForReading, geneticMother: base.Mother ?? pawn, birtherThing: pawn, father: base.Father);
			}
			else
			{
				lordJob_Ritual?.ApplyOutcome(1f);
			}
		}

		public override void Tick()
		{
			if (pawn.SpawnedOrAnyParentSpawned)
			{
				if (progressBar == null)
				{
					progressBar = EffecterDefOf.ProgressBarAlwaysVisible.Spawn();
				}
				progressBar.EffectTick(pawn, TargetInfo.Invalid);
				MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBar.children[0]).mote;
				if (mote != null)
				{
					mote.progress = Progress;
				}
			}
			else
			{
				progressBar?.Cleanup();
				progressBar = null;
			}
		}

		public override void PostRemoved()
		{
			base.PostRemoved();
			progressBar?.Cleanup();
			progressBar = null;
		}

		public override void Notify_PawnDied()
		{
			base.Notify_PawnDied();
			progressBar?.Cleanup();
			progressBar = null;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref laborDuration, "laborDuration", 0);
		}
	}
}
