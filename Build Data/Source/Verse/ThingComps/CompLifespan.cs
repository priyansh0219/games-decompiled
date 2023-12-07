using UnityEngine;
using System.Collections;
using Verse;
using RimWorld;

namespace Verse{
public class CompLifespan : ThingComp
{
	public int age = -1;

	public CompProperties_Lifespan Props { get { return (CompProperties_Lifespan)props; } }

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref age, "age");
	}

	public override void CompTick()
	{
		age += 1;
		if( age >= Props.lifespanTicks )
            Expire();
	}

	public override void CompTickRare()
	{
		age += GenTicks.TickRareInterval;
		if( age >= Props.lifespanTicks )
            Expire();
	}

	public override string CompInspectStringExtra() 
	{
		string old = base.CompInspectStringExtra();
		string descStr = "";

		int ticksLeft = Props.lifespanTicks - age;
		if ( ticksLeft > 0 )
		{
            descStr = "LifespanExpiry".Translate() + " " + ticksLeft.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor);
			if( !old.NullOrEmpty() )
				descStr = "\n" + old;
		}

		return descStr; 
	}

    protected void Expire()
    {
        if (!parent.Spawned)
            return;

        if( Props.expireEffect != null )
            Props.expireEffect.Spawn(parent.Position, parent.Map).Cleanup();

        if (Props.plantDefToSpawn != null && PlantUtility.CanNowPlantAt(Props.plantDefToSpawn, parent.Position, parent.Map))
            GenSpawn.Spawn(Props.plantDefToSpawn, parent.Position, parent.Map);

        parent.Destroy(DestroyMode.KillFinalize);
    }
}}