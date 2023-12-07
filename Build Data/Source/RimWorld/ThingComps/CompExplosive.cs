using UnityEngine;
using System.Collections;
using Verse;
using Verse.Sound;
using System.Collections.Generic;
using System;

namespace RimWorld{
public class CompExplosive : ThingComp
{
	//Working vars
	public bool 			wickStarted = false;
	public int			    wickTicksLeft = 0;
	private Thing			instigator;
	private int				countdownTicksLeft = -1;
	public bool				destroyedThroughDetonation;
    private List<Thing>     thingsIgnoredByExplosion;
    public float?            customExplosiveRadius;
	
	//Components
	protected Sustainer		wickSoundSustainer = null;
	
	//Properties
	public CompProperties_Explosive Props { get { return (CompProperties_Explosive)props; } }
	protected int StartWickThreshold
	{
		get
		{
			return Mathf.RoundToInt(Props.startWickHitPointsPercent * parent.MaxHitPoints);
		}
	}
	private bool CanEverExplodeFromDamage
	{
		get
		{
			if( Props.chanceNeverExplodeFromDamage < 0.00001f )
				return true;
			else
			{
				Rand.PushState();
				Rand.Seed = parent.thingIDNumber.GetHashCode();
				bool result = Rand.Value > Props.chanceNeverExplodeFromDamage;
				Rand.PopState();
				return result;
			}
		}
	}

    public void AddThingsIgnoredByExplosion(List<Thing> things)
    {
        if(thingsIgnoredByExplosion == null)
            thingsIgnoredByExplosion = new List<Thing>();
        
        thingsIgnoredByExplosion.AddRange(things);
    }

	public override void PostExposeData()
	{
		base.PostExposeData();

		Scribe_References.Look(ref instigator, "instigator");
        Scribe_Collections.Look(ref thingsIgnoredByExplosion, "thingsIgnoredByExplosion", LookMode.Reference);
		Scribe_Values.Look( ref wickStarted, "wickStarted", false );
		Scribe_Values.Look( ref wickTicksLeft, "wickTicksLeft", 0 );
		Scribe_Values.Look( ref destroyedThroughDetonation, "destroyedThroughDetonation" );
		Scribe_Values.Look(ref countdownTicksLeft, "countdownTicksLeft");
        Scribe_Values.Look(ref customExplosiveRadius, "explosiveRadius");
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (Props.countdownTicks.HasValue)
			countdownTicksLeft = Props.countdownTicks.Value.RandomInRange;

        UpdateOverlays();
	}

	public override void CompTick()
	{
		if (countdownTicksLeft > 0)
		{
			countdownTicksLeft--;
			if (countdownTicksLeft == 0)
			{
				StartWick();
				countdownTicksLeft = -1;
			}
		}

		if( wickStarted )
		{
			if( wickSoundSustainer == null )
				StartWickSustainer(); //or sustainer is missing on load
			else
				wickSoundSustainer.Maintain();
			
            // Trigger wick messages if any configured
            if(Props.wickMessages != null)
            {
                foreach(var messageInfo in Props.wickMessages)
                {
                    if(messageInfo.ticksLeft == wickTicksLeft && messageInfo.wickMessagekey != null)
                        Messages.Message(messageInfo.wickMessagekey.Translate(parent.GetCustomLabelNoCount(includeHp: false), wickTicksLeft.ToStringSecondsFromTicks()), 
                            parent, messageInfo.messageType ?? MessageTypeDefOf.NeutralEvent, historical: false);
                }
            }

            wickTicksLeft--;

			if( wickTicksLeft <= 0 )
				Detonate(parent.MapHeld);
		}
	}
	
	private void StartWickSustainer()
	{
		SoundDefOf.MetalHitImportant.PlayOneShot(new TargetInfo(parent.PositionHeld, parent.MapHeld));
		SoundInfo info = SoundInfo.InMap(parent, MaintenanceType.PerTick);
		wickSoundSustainer = SoundDefOf.HissSmall.TrySpawnSustainer( info );
	}

	private void EndWickSustainer()
	{
		if( wickSoundSustainer != null )
		{
			wickSoundSustainer.End();
			wickSoundSustainer = null;
		}
	}

    private OverlayHandle? overlayBurningWick;
    private void UpdateOverlays()
    {
        if (!parent.Spawned || !Props.drawWick)
            return;
            
        parent.Map.overlayDrawer.Disable(parent, ref overlayBurningWick);

        if (wickStarted)
            overlayBurningWick = parent.Map.overlayDrawer.Enable(parent, OverlayTypes.BurningWick);
    }

	public override void PostDestroy(DestroyMode mode, Map previousMap)
	{
		if (mode == DestroyMode.KillFinalize && Props.explodeOnKilled)
			Detonate(previousMap, true);
	}

	public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
	{
		absorbed = false;

		if( CanEverExplodeFromDamage )
		{
			if( dinfo.Def.ExternalViolenceFor(parent) && dinfo.Amount >= parent.HitPoints && CanExplodeFromDamageType(dinfo.Def) )
			{
				//Explode immediately from excessive incoming damage
				//Must happen here, before I'm destroyed. I can't do it after because I lose my map reference.
				if( parent.MapHeld != null )
				{
                    instigator = dinfo.Instigator; // Carry the instigator of the external damage through to the explosion, this often happens when in a row of turrets one explodes.
					Detonate(parent.MapHeld);
					
					// if we haven't actually died, just let the standard damage code take care of it
					if( parent.Destroyed )
						absorbed = true;
				}
			}
			else if( !wickStarted && Props.startWickOnDamageTaken != null && Props.startWickOnDamageTaken.Contains(dinfo.Def) )
			{
				//Start wick for special damage type?
				StartWick(dinfo.Instigator);
			}
		}
	}
	
	public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
	{
		if( !CanEverExplodeFromDamage )
			return;
		
		if( !CanExplodeFromDamageType(dinfo.Def) )
			return;
		
		if( !parent.Destroyed )
		{
			if( wickStarted && dinfo.Def == DamageDefOf.Stun )	//Stop wick on stun damage
				StopWick();
			else if( !wickStarted && parent.HitPoints <= StartWickThreshold ) //Start wick on damage below threshold
			{
				if( dinfo.Def.ExternalViolenceFor(parent) || (!Props.startWickOnInternalDamageTaken.NullOrEmpty() && Props.startWickOnInternalDamageTaken.Contains(dinfo.Def)) )
					StartWick(dinfo.Instigator);
			}
		}
	}

	public void StartWick(Thing instigator = null)
	{
		if( wickStarted )
			return;
		
		if( ExplosiveRadius() <= 0 )
			return;

		this.instigator = instigator;

		wickStarted = true;
		wickTicksLeft = Props.wickTicks.RandomInRange;
		StartWickSustainer();

		GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(parent, Props.explosiveDamageType, instigator: instigator);
        UpdateOverlays();
	}
	
	public void StopWick()
	{
		wickStarted = false;
		instigator = null;
        UpdateOverlays();
	}

	public float ExplosiveRadius()
	{
		var props = Props;

		//Expand radius for stackcount
		float radius = customExplosiveRadius ?? Props.explosiveRadius;
		if( parent.stackCount > 1 && props.explosiveExpandPerStackcount > 0 )
			radius += Mathf.Sqrt((parent.stackCount-1) * props.explosiveExpandPerStackcount);
		if( props.explosiveExpandPerFuel > 0 && parent.GetComp<CompRefuelable>() != null )
			radius += Mathf.Sqrt(parent.GetComp<CompRefuelable>().Fuel * props.explosiveExpandPerFuel);

		return radius;
	}
	
	protected void Detonate(Map map, bool ignoreUnspawned = false)
	{	
		if( !ignoreUnspawned && !parent.SpawnedOrAnyParentSpawned )
			return;

		var props = Props;
		float radius = ExplosiveRadius();

        if (radius <= 0)
            return;

		// Do this before destroying it so the fuel doesn't end up on the ground
		if( props.explosiveExpandPerFuel > 0 && parent.GetComp<CompRefuelable>() != null )
			parent.GetComp<CompRefuelable>().ConsumeFuel(parent.GetComp<CompRefuelable>().Fuel);
		
		if( props.destroyThingOnExplosionSize <= radius && !parent.Destroyed )
		{
			destroyedThroughDetonation = true;
			parent.Kill();
		}
		
		// Turn the wick off, in case we survive one way or another
		EndWickSustainer();
		wickStarted = false;

		if( map == null )
		{
			Log.Warning("Tried to detonate CompExplosive in a null map.");
			return;
		}

		if( props.explosionEffect != null )
		{
			var effect = props.explosionEffect.Spawn();
			effect.Trigger(new TargetInfo(parent.PositionHeld, map), new TargetInfo(parent.PositionHeld, map));
			effect.Cleanup();
		}
 
        //If the person who caused the explosion is not hostile, then he's to blame - unless its a player turret, in this case we always blame the instigator
        //Test cases:
        // - Colonist shoots enemy turret, it explodes and harms your ally - you shouldn't lose goodwill, instigator is your colonist
        // - Enemy shoots player turret and an ally is standing next to it - you lose shouldn't goodwill, instigator is the enemy
        // - Colonist shoots colonist turret, it explodes and harms your ally - you should lose goodwill, instigator is your colonist
        Thing toBlame;
        if( instigator != null && (!instigator.HostileTo(parent.Faction) || parent.Faction == Faction.OfPlayer) )
            toBlame = instigator;
        else //Otherwise it's this thing's fault - it was hostile so instigator was allowed to destroy it
            toBlame = parent;

		GenExplosion.DoExplosion(parent.PositionHeld,
			map,
			radius,
			props.explosiveDamageType,
			toBlame,
			damAmount: props.damageAmountBase,
			armorPenetration: props.armorPenetrationBase,
			explosionSound: props.explosionSound,
			postExplosionSpawnThingDef: props.postExplosionSpawnThingDef,
			postExplosionSpawnChance: props.postExplosionSpawnChance,
			postExplosionSpawnThingCount: props.postExplosionSpawnThingCount,
            postExplosionGasType: Props.postExplosionGasType,
			applyDamageToExplosionCellsNeighbors: props.applyDamageToExplosionCellsNeighbors,
			preExplosionSpawnThingDef: props.preExplosionSpawnThingDef,
			preExplosionSpawnChance: props.preExplosionSpawnChance,
			preExplosionSpawnThingCount: props.preExplosionSpawnThingCount,
			chanceToStartFire: props.chanceToStartFire,
			damageFalloff: props.damageFalloff,
            ignoredThings: thingsIgnoredByExplosion,
            doVisualEffects: props.doVisualEffects,
            propagationSpeed: props.propagationSpeed);
	}

	private bool CanExplodeFromDamageType(DamageDef damage)
	{
		return Props.requiredDamageTypeToExplode == null || Props.requiredDamageTypeToExplode == damage;
	}

	public override string CompInspectStringExtra()
	{
        string outString = "";
        
		if (countdownTicksLeft != -1)
			outString += "DetonationCountdown".Translate(countdownTicksLeft.TicksToDays().ToString("0.0"));
        
        if (Props.extraInspectStringKey != null)
            outString += (outString != "" ? "\n" : "") + Props.extraInspectStringKey.Translate();

		return outString;
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (countdownTicksLeft > 0)
		{
			Command_Action debugTriggerCmd = new Command_Action();
			debugTriggerCmd.defaultLabel = "DEV: Trigger countdown";
			debugTriggerCmd.action = () =>
			{
				countdownTicksLeft = 1;
			};
			yield return debugTriggerCmd;
		}
	}
}}