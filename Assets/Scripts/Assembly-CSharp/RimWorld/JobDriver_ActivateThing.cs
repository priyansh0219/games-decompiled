using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_ActivateThing : JobDriver
	{
		private CompActivable Activable => job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompActivable>();

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A)
				.FailOn(() => !Activable.CanActivate());
			yield return Toils_General.Do(delegate
			{
				Activable.Activate();
			});
		}
	}
}
