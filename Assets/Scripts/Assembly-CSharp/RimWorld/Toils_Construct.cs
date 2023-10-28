using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class Toils_Construct
	{
		public static Toil MakeSolidThingFromBlueprintIfNecessary(TargetIndex blueTarget, TargetIndex targetToUpdate = TargetIndex.None)
		{
			Toil toil = ToilMaker.MakeToil("MakeSolidThingFromBlueprintIfNecessary");
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				if (curJob.GetTarget(blueTarget).Thing is Blueprint blueprint)
				{
					bool flag = targetToUpdate != 0 && curJob.GetTarget(targetToUpdate).Thing == blueprint;
					if (blueprint.TryReplaceWithSolidThing(actor, out var createdThing, out var _))
					{
						curJob.SetTarget(blueTarget, createdThing);
						if (flag)
						{
							curJob.SetTarget(targetToUpdate, createdThing);
						}
						if (createdThing is Frame)
						{
							actor.Reserve(createdThing, curJob);
						}
					}
				}
			};
			return toil;
		}

		public static Toil UninstallIfMinifiable(TargetIndex thingInd)
		{
			Toil uninstallIfMinifiable = ToilMaker.MakeToil("UninstallIfMinifiable").FailOnDestroyedNullOrForbidden(thingInd);
			uninstallIfMinifiable.initAction = delegate
			{
				Pawn actor2 = uninstallIfMinifiable.actor;
				JobDriver curDriver2 = actor2.jobs.curDriver;
				Thing thing3 = actor2.CurJob.GetTarget(thingInd).Thing;
				if (thing3.def.Minifiable)
				{
					curDriver2.uninstallWorkLeft = thing3.def.building.uninstallWork;
				}
				else
				{
					curDriver2.ReadyForNextToil();
				}
			};
			uninstallIfMinifiable.tickAction = delegate
			{
				Pawn actor = uninstallIfMinifiable.actor;
				JobDriver curDriver = actor.jobs.curDriver;
				Job curJob = actor.CurJob;
				float num = (StatDefOf.ConstructionSpeed.Worker.IsDisabledFor(actor) ? 0.1f : actor.GetStatValue(StatDefOf.ConstructionSpeed));
				curDriver.uninstallWorkLeft -= num * 1.7f;
				if (curDriver.uninstallWorkLeft <= 0f)
				{
					Thing thing = curJob.GetTarget(thingInd).Thing;
					bool num2 = Find.Selector.IsSelected(thing);
					MinifiedThing minifiedThing = thing.MakeMinified();
					Thing thing2 = GenSpawn.Spawn(minifiedThing, thing.Position, uninstallIfMinifiable.actor.Map);
					if (num2 && thing2 != null)
					{
						Find.Selector.Select(thing2, playSound: false, forceDesignatorDeselect: false);
					}
					curJob.SetTarget(thingInd, minifiedThing);
					actor.jobs.curDriver.ReadyForNextToil();
				}
			};
			uninstallIfMinifiable.defaultCompleteMode = ToilCompleteMode.Never;
			uninstallIfMinifiable.WithProgressBar(thingInd, () => 1f - uninstallIfMinifiable.actor.jobs.curDriver.uninstallWorkLeft / uninstallIfMinifiable.actor.CurJob.targetA.Thing.def.building.uninstallWork);
			return uninstallIfMinifiable;
		}
	}
}
