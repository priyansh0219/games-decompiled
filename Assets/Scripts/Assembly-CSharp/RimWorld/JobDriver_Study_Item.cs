using System.Collections.Generic;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Study_Item : JobDriver_Study
	{
		private const TargetIndex ResearchBenchInd = TargetIndex.B;

		private const TargetIndex HaulCell = TargetIndex.C;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (pawn.Reserve(base.StudyThing, job, 1, -1, null, errorOnFailed) && pawn.Reserve(base.TargetB, job, 1, -1, null, errorOnFailed))
			{
				return pawn.Reserve(base.TargetC, job, 1, -1, null, errorOnFailed);
			}
			return false;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
			this.FailOnBurningImmobile(TargetIndex.B);
			yield return Toils_General.DoAtomic(delegate
			{
				job.count = 1;
			});
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOnDestroyedNullOrForbidden(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, null, storageMode: false);
			yield return StudyToil();
		}
	}
}
