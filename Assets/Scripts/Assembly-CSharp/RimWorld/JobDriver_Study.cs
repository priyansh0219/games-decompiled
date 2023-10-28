using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class JobDriver_Study : JobDriver
	{
		protected const TargetIndex StudiableInd = TargetIndex.A;

		private const float DefaultResearchSpeed = 0.08f;

		private const int JobEndInterval = 4000;

		protected Thing StudyThing => base.TargetThingA;

		protected CompStudiable StudyComp => StudyThing.TryGetComp<CompStudiable>();

		protected Toil StudyToil()
		{
			Toil study = ToilMaker.MakeToil("StudyToil");
			study.tickAction = delegate
			{
				Pawn actor = study.actor;
				float num = 0.08f;
				if (!actor.WorkTypeIsDisabled(WorkTypeDefOf.Research))
				{
					num = actor.GetStatValue(StatDefOf.ResearchSpeed);
				}
				num *= base.TargetThingA.GetStatValue(StatDefOf.ResearchSpeedFactor);
				StudyComp.Study(num, actor);
				if (StudyComp.Completed)
				{
					pawn.jobs.curDriver.ReadyForNextToil();
				}
			};
			study.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			study.WithProgressBar(TargetIndex.A, () => StudyComp.ProgressPercent);
			study.defaultCompleteMode = ToilCompleteMode.Delay;
			study.defaultDuration = 4000;
			study.activeSkill = () => SkillDefOf.Intellectual;
			return study;
		}
	}
}
