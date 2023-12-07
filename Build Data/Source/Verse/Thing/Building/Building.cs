using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Verse.Sound;
using UnityEngine;
using RimWorld;
using Verse.AI.Group;
using System.Text;

namespace Verse{
public class Building : ThingWithComps
{
    //Working vars
    private Sustainer		sustainerAmbient = null;
    private ColorDef        paintColorDef;

    //Config
    public bool				canChangeTerrainOnDestroyed = true;

    //Constants
    private static readonly SimpleCurve ShakeAmountPerAreaCurve = new SimpleCurve()
    {
        new CurvePoint(1f, 0.07f),
        new CurvePoint(2f, 0.07f),
        new CurvePoint(4f, 0.1f),
        new CurvePoint(9f, 0.2f),
        new CurvePoint(16f, 0.5f)
    };
    private const float ChanceToGeneratePaintedFromTrader = 0.1f;

    //Properties
    public CompPower PowerComp		{get{ return GetComp<CompPower>(); }}
    public ColorDef PaintColorDef => paintColorDef;
    public override Color DrawColor
    {
        get
        {
            if (paintColorDef != null)
                return paintColorDef.color;
            return base.DrawColor;
        }
    }
    public virtual bool TransmitsPowerNow
    {
        get
        {
            //Designed to be overridden
            //In base game this always just returns the value in the powercomp's def
            CompPower pc = PowerComp;
            return pc != null && pc.Props.transmitsPower;
        }
    }
    public override int HitPoints
    {
        set
        {
            int oldHitPoints = HitPoints;
            base.HitPoints = value;

            BuildingsDamageSectionLayerUtility.Notify_BuildingHitPointsChanged(this, oldHitPoints);
        }
    }
    public virtual int MaxItemsInCell => def.building.maxItemsInCell;

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref canChangeTerrainOnDestroyed, "canChangeTerrainOnDestroyed", true);
        Scribe_Defs.Look(ref paintColorDef, "paintColorDef");
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        //Before base.SpawnSetup() so when regions are rebuilt this building can be accessed via edificeGrid
        if( def.IsEdifice() )
        {
            map.edificeGrid.Register(this);

            if (def.Fillage == FillCategory.Full)
                map.terrainGrid.Drawer.SetDirty();

            if (def.AffectsFertility)
                map.fertilityGrid.Drawer.SetDirty();
        }

        base.SpawnSetup(map, respawningAfterLoad);

        Map.listerBuildings.Add(this);
        
        //Remake terrain meshes with new underwall under me
        if( def.coversFloor )
            Map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Terrain, true, false);

        var occRect = this.OccupiedRect();
        for( int z=occRect.minZ; z<=occRect.maxZ; z++ )
        {
            for( int x=occRect.minX; x<=occRect.maxX; x++ )
            {
                var c = new IntVec3(x,0,z);
                Map.mapDrawer.MapMeshDirty( c, MapMeshFlag.Buildings );
                Map.glowGrid.MarkGlowGridDirty(c);
                if( !SnowGrid.CanCoexistWithSnow(def) )
                    Map.snowGrid.SetDepth(c, 0);
            }
        }

        if( Faction == Faction.OfPlayer )
        {
            if( def.building != null && def.building.spawnedConceptLearnOpportunity != null )
            {
                LessonAutoActivator.TeachOpportunity( def.building.spawnedConceptLearnOpportunity, OpportunityType.GoodToKnow );
            }
        }

        AutoHomeAreaMaker.Notify_BuildingSpawned( this );

        if( def.building != null && !def.building.soundAmbient.NullOrUndefined() )
        {
            LongEventHandler.ExecuteWhenFinished(() =>
                {
                    SoundInfo info = SoundInfo.InMap(this, MaintenanceType.None);
                    sustainerAmbient = SoundStarter.TrySpawnSustainer(def.building.soundAmbient, info);
                });
        }

        Map.listerBuildingsRepairable.Notify_BuildingSpawned(this);
        Map.listerArtificialBuildingsForMeditation.Notify_BuildingSpawned(this);
        Map.listerBuldingOfDefInProximity.Notify_BuildingSpawned(this);
        Map.listerBuildingWithTagInProximity.Notify_BuildingSpawned(this);

        if( !this.CanBeSeenOver() )
            Map.exitMapGrid.Notify_LOSBlockerSpawned();

        SmoothSurfaceDesignatorUtility.Notify_BuildingSpawned(this);

        //Must go after adding to buildings list
        map.avoidGrid.Notify_BuildingSpawned(this);
        map.lordManager.Notify_BuildingSpawned(this);
        map.animalPenManager.Notify_BuildingSpawned(this);
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        var map = Map; // before DeSpawn!

        base.DeSpawn(mode);

        if( def.IsEdifice() )
        {
            map.edificeGrid.DeRegister(this);

            if (def.Fillage == FillCategory.Full)
            {
                map.terrainGrid.Drawer.SetDirty();

                if (ModsConfig.BiotechActive)
                    map.pollutionGrid.Drawer.SetDirty();
            }

            if (def.AffectsFertility)
                map.fertilityGrid.Drawer.SetDirty();
        }

        if( mode != DestroyMode.WillReplace )
        {
            if( def.MakeFog )
                map.fogGrid.Notify_FogBlockerRemoved(Position);

            if( def.holdsRoof )
                RoofCollapseCellsFinder.Notify_RoofHolderDespawned(this, map);
        
            if( def.IsSmoothable )
                SmoothSurfaceDesignatorUtility.Notify_BuildingDespawned(this, map);
        }

        if( sustainerAmbient != null )
            sustainerAmbient.End();

        CellRect occRect = GenAdj.OccupiedRect(this);
        for( int z=occRect.minZ; z<=occRect.maxZ; z++ )
        {
            for( int x=occRect.minX; x<=occRect.maxX; x++ )
            {
                IntVec3 c = new IntVec3(x,0,z);

                MapMeshFlag changeType = MapMeshFlag.Buildings;

                if( def.coversFloor )
                    changeType |= MapMeshFlag.Terrain;

                if( def.Fillage == FillCategory.Full )
                {
                    changeType |= MapMeshFlag.Roofs;
                    changeType |= MapMeshFlag.Snow;
                }

                map.mapDrawer.MapMeshDirty( c, changeType );

                map.glowGrid.MarkGlowGridDirty(c);
            }
        }

        map.listerBuildings.Remove(this);
        map.listerBuildingsRepairable.Notify_BuildingDeSpawned(this);
        map.listerArtificialBuildingsForMeditation.Notify_BuildingDeSpawned(this);
        map.listerBuldingOfDefInProximity.Notify_BuildingDeSpawned(this);
        map.listerBuildingWithTagInProximity.Notify_BuildingDeSpawned(this);

        if( def.building.leaveTerrain != null && Current.ProgramState == ProgramState.Playing && canChangeTerrainOnDestroyed )
        {
            foreach (var tile in GenAdj.OccupiedRect(this))
            {
                map.terrainGrid.SetTerrain(tile, def.building.leaveTerrain);
            }
        }

        //Mining, planning, etc
        map.designationManager.Notify_BuildingDespawned(this);

        if( !this.CanBeSeenOver() )
            map.exitMapGrid.Notify_LOSBlockerDespawned();

        if( def.building.hasFuelingPort )
        {
            var fuelingPortCell = FuelingPortUtility.GetFuelingPortCell(Position, Rotation);
            var launchable = FuelingPortUtility.LaunchableAt(fuelingPortCell, map);

            if( launchable != null )
                launchable.Notify_FuelingPortSourceDeSpawned();
        }

        //Must go after removing from buildings list
        map.avoidGrid.Notify_BuildingDespawned(this);
        map.lordManager.Notify_BuildingDespawned(this);
        map.animalPenManager.Notify_BuildingDespawned(this);

        //Scatter items if there were multiple items per cell
        if( MaxItemsInCell >= 2 )
        {
            foreach( var c in this.OccupiedRect() )
            {
                int items = c.GetItemCount(map);
                if( items <= 1 || items <= c.GetMaxItemsAllowedInCell(map) )
                    continue;

                for( int i = 0; i < items - 1; i++ )
                {
                    var firstItem = c.GetFirstItem(map);
                    if( firstItem != null )
                    {
                        firstItem.DeSpawn();
                        GenPlace.TryPlaceThing(firstItem, c, map, ThingPlaceMode.Near);

                        if( c.GetItemCount(map) <= c.GetMaxItemsAllowedInCell(map) )
                            break;
                    }
                    else
                        break;
                }
            }
        }
    }
    
    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        bool wasSpawned = Spawned;
        var map = Map; // before Destroy()!

        // before Destroy(); the math is easier to do here
        SmoothableWallUtility.Notify_BuildingDestroying(this, mode);

        var lord = this.GetLord();
        if( lord != null )
            lord.Notify_BuildingLost(this);

        base.Destroy(mode);

        // (buildings can be reinstalled)
        InstallBlueprintUtility.CancelBlueprintsFor(this);

        if (wasSpawned)
        {
            if (mode == DestroyMode.Deconstruct)
                SoundDefOf.Building_Deconstructed.PlayOneShot(new TargetInfo(Position, map));
            else if (mode == DestroyMode.KillFinalize)
                DoDestroyEffects(map);
        }
        
        if( wasSpawned )
            ThingUtility.CheckAutoRebuildOnDestroyed(this, mode, map, def);
    }

    public override void SetFaction( Faction newFaction, Pawn recruiter = null )
    {
        if( Spawned )
        {
            Map.listerBuildingsRepairable.Notify_BuildingDeSpawned(this);
            Map.listerBuildingWithTagInProximity.Notify_BuildingDeSpawned(this);
            Map.listerBuildings.Remove(this);
        }

        base.SetFaction(newFaction, recruiter);

        if( Spawned )
        {
            Map.listerBuildingsRepairable.Notify_BuildingSpawned(this);
            Map.listerArtificialBuildingsForMeditation.Notify_BuildingSpawned(this);
            Map.listerBuildingWithTagInProximity.Notify_BuildingSpawned(this);
            Map.listerBuildings.Add(this);
            Map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.PowerGrid, true, false);

            if( newFaction == Faction.OfPlayer )
                AutoHomeAreaMaker.Notify_BuildingClaimed(this);
        }
    }

    public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
    {
        if( Faction != null && Spawned && Faction != Faction.OfPlayer )
        {
            for( int i=0; i<Map.lordManager.lords.Count; i++ )
            {
                var lord = Map.lordManager.lords[i];
                if( lord.faction == Faction )
                    lord.Notify_BuildingDamaged(this, dinfo);
            }
        }

        base.PreApplyDamage(ref dinfo, out absorbed);

        if (!absorbed && Faction != null)
            Faction.Notify_BuildingTookDamage(this, dinfo);
    }

    public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
    {
        base.PostApplyDamage(dinfo, totalDamageDealt);

        if( Spawned )
            Map.listerBuildingsRepairable.Notify_BuildingTookDamage(this);
    }

    public override void DrawExtraSelectionOverlays()
    {
        base.DrawExtraSelectionOverlays();

        var ebp = InstallBlueprintUtility.ExistingBlueprintFor(this);

        if( ebp != null )
            GenDraw.DrawLineBetween(this.TrueCenter(), ebp.TrueCenter());
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach( var c in base.GetGizmos() )
        {
            yield return c;
        }

        if( ((def.BuildableByPlayer && def.passability != Traversability.Impassable && !def.IsDoor) || def.building.forceShowRoomStats)
            && Gizmo_RoomStats.GetRoomToShowStatsFor(this) != null
            && Find.Selector.SingleSelectedObject == this )
        {
            yield return new Gizmo_RoomStats(this);
        }
        
        var selectMonumentMarker = QuestUtility.GetSelectMonumentMarkerGizmo(this);
        if( selectMonumentMarker != null )
            yield return selectMonumentMarker;

        if( def.Minifiable && Faction == Faction.OfPlayer )
            yield return InstallationDesignatorDatabase.DesignatorFor(def);

        ColorInt? glowColorOverride = null;
        if (GetComp<CompGlower>() is CompGlower glower && glower.HasGlowColorOverride)
            glowColorOverride = glower.GlowColor;

        var buildCopy = BuildCopyCommandUtility.BuildCopyCommand(def, Stuff, StyleSourcePrecept as Precept_Building, StyleDef, true, glowColorOverride);
        if( buildCopy != null )
            yield return buildCopy;
            
        if( Faction == Faction.OfPlayer || def.building.alwaysShowRelatedBuildCommands )
        {
            foreach( var facility in BuildRelatedCommandUtility.RelatedBuildCommands(def) )
            {
                yield return facility;
            }
        }
    }

    public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
    {
        foreach (var s in base.SpecialDisplayStats())
        {
            yield return s;
        }

        if (PaintColorDef != null && !PaintColorDef.label.NullOrEmpty())
            yield return new StatDrawEntry(StatCategoryDefOf.Building, "Stat_Building_PaintColor".Translate(), PaintColorDef.LabelCap, "Stat_Building_PaintColorDesc".Translate(), StatDisplayOrder.Thing_Paintable);
    }

    public override bool ClaimableBy(Faction by, StringBuilder reason = null)
    {
        if( !def.Claimable )
            return false;

        // Note if Faction is null we don't want to check Map.ParentFaction because we want players to be able to claim
        // abandoned structures on the their own map.
        if (Faction == by)
            return false;

        if (FactionPreventsClaimingOrAdopting(Faction ?? Map?.ParentFaction, true, reason))
            return false;

        for ( int i = 0; i < AllComps.Count; i++ )
        {
            if( AllComps[i].CompPreventClaimingBy(by) )
                return false;
        }

        return true;
    }

    public virtual bool DeconstructibleBy(Faction faction)
    {
        if( !def.building.IsDeconstructible )
            return false;

        if( DebugSettings.godMode )
            return true;

        return Faction == faction
            || ClaimableBy(faction)
            || def.building.alwaysDeconstructible;
    }

    public virtual ushort PathWalkCostFor(Pawn p)
    {
        return 0;
    }

    public virtual bool IsDangerousFor(Pawn p)
    {
        return false;
    }

    private void DoDestroyEffects(Map map)
    {
        if( def.building.destroyEffecter != null )
        {
            var effecter = def.building.destroyEffecter.Spawn(Position, map);
            effecter.Trigger(new TargetInfo(Position, map), TargetInfo.Invalid);
            effecter.Cleanup();
            return; // if there is a custom effecter, let it handle everything
        }

        if (!def.IsEdifice())
            return;

        var destroySound = GetDestroySound();
        if (destroySound != null)
            destroySound.PlayOneShot(new TargetInfo(Position, map));

        foreach (var c in this.OccupiedRect())
        {
            int dustAmount = def.building.isNaturalRock ? 1 : Rand.RangeInclusive(3, 5);
            for (int i = 0; i < dustAmount; i++)
            {
                FleckMaker.ThrowDustPuffThick(c.ToVector3Shifted(), map, Rand.Range(1.5f, 2f), Color.white);
            }
        }

        if (Find.CurrentMap == map)
        {
            float shake = def.building.destroyShakeAmount;

            if (shake < 0f)
                shake = ShakeAmountPerAreaCurve.Evaluate(def.Size.Area);

            //Don't shake for expired buildings
            var lifespan = this.TryGetComp<CompLifespan>();
            if (lifespan == null || lifespan.age < lifespan.Props.lifespanTicks)
                Find.CameraDriver.shaker.DoShake(shake);
        }
    }

    private SoundDef GetDestroySound()
    {
        const int SizeSmall = 1;
        const int SizeMedium = 4;

        //Explicitly defined
        if (!def.building.destroySound.NullOrUndefined())
            return def.building.destroySound;

        StuffCategoryDef stuffCategory;
        if (def.MadeFromStuff && Stuff != null && !Stuff.stuffProps.categories.NullOrEmpty())
            stuffCategory = Stuff.stuffProps.categories[0];
        else if (!def.CostList.NullOrEmpty() && def.CostList[0].thingDef.IsStuff && !def.CostList[0].thingDef.stuffProps.categories.NullOrEmpty())
            stuffCategory = def.CostList[0].thingDef.stuffProps.categories[0];
        else
            return null;

        switch (def.building.buildingSizeCategory)
        {
            case BuildingSizeCategory.Small:
                if (!stuffCategory.destroySoundSmall.NullOrUndefined())
                    return stuffCategory.destroySoundSmall;
                break;
            case BuildingSizeCategory.Medium:
                if (!stuffCategory.destroySoundMedium.NullOrUndefined())
                    return stuffCategory.destroySoundMedium;
                break;
            case BuildingSizeCategory.Large:
                if (!stuffCategory.destroySoundLarge.NullOrUndefined())
                    return stuffCategory.destroySoundLarge;
                break;
            case BuildingSizeCategory.None: //Based on actual building size
                var area = def.Size.Area;

                if (area <= SizeSmall && !stuffCategory.destroySoundSmall.NullOrUndefined())
                    return stuffCategory.destroySoundSmall;
                else if (area <= SizeMedium && !stuffCategory.destroySoundMedium.NullOrUndefined())
                    return stuffCategory.destroySoundMedium;
                else if (!stuffCategory.destroySoundLarge.NullOrUndefined())
                    return stuffCategory.destroySoundLarge;
                break;
        }

        return null;
    }

    public override void PostGeneratedForTrader(TraderKindDef trader, int forTile, Faction forFaction)
    {
        if (def.building.paintable && Rand.Value < ChanceToGeneratePaintedFromTrader)
            ChangePaint(DefDatabase<ColorDef>.AllDefs.Where(x => x.colorType == ColorType.Structure).RandomElement());
        else if (def.colorGeneratorInTraderStock != null)
            this.SetColor(def.colorGeneratorInTraderStock.NewRandomizedColor(), reportFailure: true);
    }

    public override string GetInspectStringLowPriority()
    {
        var str = base.GetInspectStringLowPriority();

        if(def.IsNonDeconstructibleAttackableBuilding)
        {
            if(!str.NullOrEmpty())
                str += "\n";

            str += "AttackToDestroy".Translate(); 
        }

        return str;
    }

    public void ChangePaint(ColorDef colorDef)
    {
        paintColorDef = colorDef;
        Notify_ColorChanged();
    }

    public static Gizmo SelectContainedItemGizmo(Thing container, Thing item)
    {
        if (!container.Faction.IsPlayerSafe())
            return null;

        return ContainingSelectionUtility.SelectCarriedThingGizmo(container, item);
    }
}

public enum BuildingSizeCategory
{
    None,
    Small,
    Medium,
    Large
}
}
