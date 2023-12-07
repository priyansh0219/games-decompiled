using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Verse.AI{
public static class ToilJumpConditions
{
	public static Toil JumpIf( this Toil toil, Func<bool> jumpCondition, Toil jumpToil )
	{
		toil.AddPreTickAction( ()=>
			{
				if( jumpCondition() )
				{
					toil.actor.jobs.curDriver.JumpToToil(jumpToil);
					return;
				}
			}
		);

		return toil;
	}

	public static Toil JumpIfDespawnedOrNull( this Toil toil, TargetIndex ind, Toil jumpToil )
	{
		return toil.JumpIf( ()=>
			{
				var thing = toil.actor.jobs.curJob.GetTarget(ind).Thing;

				return thing == null || !thing.Spawned;
			},
			jumpToil );
	}

	public static Toil JumpIfDespawnedOrNullOrForbidden( this Toil toil, TargetIndex ind, Toil jumpToil )
	{
		return toil.JumpIf( ()=>
			{
				var thing = toil.actor.jobs.curJob.GetTarget(ind).Thing;

				return thing == null || !thing.Spawned || thing.IsForbidden(toil.actor);
			},
			jumpToil );
	}

	public static Toil JumpIfOutsideHomeArea( this Toil toil, TargetIndex ind, Toil jumpToil )
	{
		return toil.JumpIf( ()=>
			{
				var thing = toil.actor.jobs.curJob.GetTarget(ind).Thing;

				if( !toil.actor.Map.areaManager.Home[thing.Position] )
					return true;
			
				return false;
			},
			jumpToil );
	}

    public static Toil JumpIfThingMissingDesignation(this Toil toil, TargetIndex ind, DesignationDef desDef, Toil jumpToil)
	{
        return toil.JumpIf(() => {
            var actor = toil.GetActor();
            var job = actor.jobs.curJob;

            if (job.ignoreDesignations)
                return false;

            var targ = job.GetTarget(ind).Thing;

            if (targ == null || actor.Map.designationManager.DesignationOn(targ, desDef) == null)
                return true;

            return false;
        }, jumpToil);
	}
}
}
