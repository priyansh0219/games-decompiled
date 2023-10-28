using System.Collections.Generic;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Study_Building : JobDriver_Study
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (pawn.Reserve(base.StudyThing, job, 1, -1, null, errorOnFailed))
			{
				if (base.StudyThing.def.hasInteractionCell)
				{
					return pawn.ReserveSittableOrSpot(base.StudyThing.InteractionCell, job, errorOnFailed);
				}
				return true;
			}
			return false;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOn(() => base.StudyThing.Map.designationManager.DesignationOn(base.StudyThing, DesignationDefOf.Study) == null || base.StudyComp == null || base.StudyComp.Completed);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			yield return StudyToil();
		}
	}
}
