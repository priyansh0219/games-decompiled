using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{

//Note: "Storage" here means a cell-based storage (e.g. a shelf), it's not a container like graves where the item disappears.
//Maybe we should rename it to Building_CellStorage and add a Building_Storage as a base class for all haul destinations like shelves and graves?
[StaticConstructorOnStartup]
public class Building_Storage : Building, ISlotGroupParent, IStorageGroupMember
{
    //Working vars
    public StorageSettings settings;
    protected StorageGroup  storageGroup;

    //Working vars - unsaved
    public SlotGroup        slotGroup;
    private List<IntVec3>   cachedOccupiedCells = null;

    public Building_Storage()
    {
        slotGroup = new SlotGroup(this);
    }

    //=======================================================================
    //====================== IStorageGroupMember interface===================
    //=======================================================================

    StorageGroup IStorageGroupMember.Group
    {
        get => storageGroup;
        set => storageGroup = value;
    }
    bool IStorageGroupMember.DrawConnectionOverlay => Spawned;
    Map IStorageGroupMember.Map => MapHeld;
    string IStorageGroupMember.StorageGroupTag => def.building.storageGroupTag;
    StorageSettings IStorageGroupMember.StoreSettings => GetStoreSettings();
    StorageSettings IStorageGroupMember.ParentStoreSettings => GetParentStoreSettings();
    StorageSettings IStorageGroupMember.ThingStoreSettings => settings; //Our settings, not parent or storage group.
    bool IStorageGroupMember.DrawStorageTab => true;

    //=======================================================================
    //========================== SlotGroupParent interface===================
    //=======================================================================

    public bool StorageTabVisible => true;
    public bool IgnoreStoredThingsBeauty => def.building.ignoreStoredThingsBeauty;

    public SlotGroup GetSlotGroup() => slotGroup;

    public virtual void Notify_ReceivedThing(Thing newItem)
    {
        if( Faction == Faction.OfPlayer && newItem.def.storedConceptLearnOpportunity != null )
            LessonAutoActivator.TeachOpportunity(newItem.def.storedConceptLearnOpportunity, OpportunityType.GoodToKnow);
    }

    public virtual void Notify_LostThing(Thing newItem){/*Nothing by default*/}

    public virtual IEnumerable<IntVec3> AllSlotCells()
    {
        if (!Spawned)
            yield break;

        foreach( IntVec3 c in GenAdj.CellsOccupiedBy(this) )
        {
            yield return c;
        }
    }

    public List<IntVec3> AllSlotCellsList()
    {
        if( cachedOccupiedCells == null )
            cachedOccupiedCells = AllSlotCells().ToList();

        return cachedOccupiedCells;
    }

    public StorageSettings GetStoreSettings()
    {
        if (storageGroup != null)
            return storageGroup.GetStoreSettings();
        return settings;
    }

    public StorageSettings GetParentStoreSettings()
    {
        var parentSettings = def.building.fixedStorageSettings;
        if (parentSettings != null)
            return parentSettings;

        // if no given fixed config, only allow storable things in storage buildings by default
        return StorageSettings.EverStorableFixedSettings();
    }

    public void Notify_SettingsChanged()
    {
        if (Spawned && slotGroup != null)
            Map.listerHaulables.Notify_SlotGroupChanged(slotGroup);
    }

    public string SlotYielderLabel() => LabelCap;

    public bool Accepts(Thing t)
    {
        return GetStoreSettings().AllowedToAccept(t);
    }


    //=======================================================================
    //============================== Other stuff ============================
    //=======================================================================

    public override void PostMake()
    {
        base.PostMake();

        settings = new StorageSettings(this);

        if( def.building.defaultStorageSettings != null )
            settings.CopyFrom( def.building.defaultStorageSettings );
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        cachedOccupiedCells = null; // invalidate cache

        base.SpawnSetup(map, respawningAfterLoad);

        if (storageGroup != null && map != storageGroup.Map)
        {
            var oldSettings = storageGroup.GetStoreSettings();
            storageGroup.RemoveMember(this);
            storageGroup = null;
            settings.CopyFrom(oldSettings);
        }
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        base.DeSpawn(mode);
        cachedOccupiedCells = null; // invalidate cache
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        if (storageGroup != null)
        {
            storageGroup?.RemoveMember(this);
            storageGroup = null;
        }

        base.Destroy(mode);
    }

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Deep.Look(ref settings, "settings", this);
        Scribe_References.Look(ref storageGroup, "storageGroup");
    }

    public override void DrawExtraSelectionOverlays()
    {
        base.DrawExtraSelectionOverlays();
        StorageGroupUtility.DrawSelectionOverlaysFor(this);
    }

    public override string GetInspectString()
    {
        var s = base.GetInspectString();

        if( Spawned )
        {
            if ( storageGroup != null )
            {
                if ( !s.NullOrEmpty() )
                    s += "\n";
                s += "LinkedStorageSettings".Translate() + ": " + "NumBuildings".Translate(storageGroup.MemberCount).CapitalizeFirst();
            }

            if( slotGroup.HeldThings.Any() )
            {
                var sBuilder = new StringBuilder();

                if ( !s.NullOrEmpty() )
                    sBuilder.Append("\n");

                sBuilder.Append("StoresThings".Translate() + ": ");

                bool first = true;
                foreach ( var t in slotGroup.HeldThings )
                {
                    if( !first )
                        sBuilder.Append(", ");

                    sBuilder.Append(t.LabelShortCap);
                    first = false;
                }

                s += sBuilder + ".";
            }
        }

        return s;
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach( var g in base.GetGizmos() )
        {
            yield return g;
        }

        foreach( var g in StorageSettingsClipboard.CopyPasteGizmosFor(GetStoreSettings()) )
        {
            yield return g;
        }

        if (StorageTabVisible && MapHeld != null)
        {
            foreach (var g in StorageGroupUtility.StorageGroupMemberGizmos(this))
            {
                yield return g;
            }

            if( Find.Selector.NumSelected == 1 )
            {
                //Stored items
                foreach ( var t in slotGroup.HeldThings )
                {
                    yield return ContainingSelectionUtility.CreateSelectStorageGizmo("CommandSelectStoredThing".Translate(t), "CommandSelectStoredThingDesc".Translate() + "\n\n" + t.GetInspectString(), t, t, false);
                }
            }
        }
    }
}

}