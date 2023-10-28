using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Building_Storage : Building, ISlotGroupParent, IStoreSettingsParent, IHaulDestination, IStorageGroupMember
	{
		public StorageSettings settings;

		protected StorageGroup storageGroup;

		public SlotGroup slotGroup;

		private List<IntVec3> cachedOccupiedCells;

		StorageGroup IStorageGroupMember.Group
		{
			get
			{
				return storageGroup;
			}
			set
			{
				storageGroup = value;
			}
		}

		bool IStorageGroupMember.DrawConnectionOverlay => base.Spawned;

		Map IStorageGroupMember.Map => base.MapHeld;

		string IStorageGroupMember.StorageGroupTag => def.building.storageGroupTag;

		StorageSettings IStorageGroupMember.StoreSettings => GetStoreSettings();

		StorageSettings IStorageGroupMember.ParentStoreSettings => GetParentStoreSettings();

		StorageSettings IStorageGroupMember.ThingStoreSettings => settings;

		bool IStorageGroupMember.DrawStorageTab => true;

		public bool StorageTabVisible => true;

		public bool IgnoreStoredThingsBeauty => def.building.ignoreStoredThingsBeauty;

		public Building_Storage()
		{
			slotGroup = new SlotGroup(this);
		}

		public SlotGroup GetSlotGroup()
		{
			return slotGroup;
		}

		public virtual void Notify_ReceivedThing(Thing newItem)
		{
			if (base.Faction == Faction.OfPlayer && newItem.def.storedConceptLearnOpportunity != null)
			{
				LessonAutoActivator.TeachOpportunity(newItem.def.storedConceptLearnOpportunity, OpportunityType.GoodToKnow);
			}
		}

		public virtual void Notify_LostThing(Thing newItem)
		{
		}

		public virtual IEnumerable<IntVec3> AllSlotCells()
		{
			if (!base.Spawned)
			{
				yield break;
			}
			foreach (IntVec3 item in GenAdj.CellsOccupiedBy(this))
			{
				yield return item;
			}
		}

		public List<IntVec3> AllSlotCellsList()
		{
			if (cachedOccupiedCells == null)
			{
				cachedOccupiedCells = AllSlotCells().ToList();
			}
			return cachedOccupiedCells;
		}

		public StorageSettings GetStoreSettings()
		{
			if (storageGroup != null)
			{
				return storageGroup.GetStoreSettings();
			}
			return settings;
		}

		public StorageSettings GetParentStoreSettings()
		{
			StorageSettings fixedStorageSettings = def.building.fixedStorageSettings;
			if (fixedStorageSettings != null)
			{
				return fixedStorageSettings;
			}
			return StorageSettings.EverStorableFixedSettings();
		}

		public void Notify_SettingsChanged()
		{
			if (base.Spawned && slotGroup != null)
			{
				base.Map.listerHaulables.Notify_SlotGroupChanged(slotGroup);
			}
		}

		public string SlotYielderLabel()
		{
			return LabelCap;
		}

		public bool Accepts(Thing t)
		{
			return GetStoreSettings().AllowedToAccept(t);
		}

		public override void PostMake()
		{
			base.PostMake();
			settings = new StorageSettings(this);
			if (def.building.defaultStorageSettings != null)
			{
				settings.CopyFrom(def.building.defaultStorageSettings);
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			cachedOccupiedCells = null;
			base.SpawnSetup(map, respawningAfterLoad);
			if (storageGroup != null && map != storageGroup.Map)
			{
				StorageSettings storeSettings = storageGroup.GetStoreSettings();
				storageGroup.RemoveMember(this);
				storageGroup = null;
				settings.CopyFrom(storeSettings);
			}
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			base.DeSpawn(mode);
			cachedOccupiedCells = null;
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
			string text = base.GetInspectString();
			if (base.Spawned)
			{
				if (storageGroup != null)
				{
					if (!text.NullOrEmpty())
					{
						text += "\n";
					}
					text += "LinkedStorageSettings".Translate() + ": " + "NumBuildings".Translate(storageGroup.MemberCount).CapitalizeFirst();
				}
				if (slotGroup.HeldThings.Any())
				{
					StringBuilder stringBuilder = new StringBuilder();
					if (!text.NullOrEmpty())
					{
						stringBuilder.Append("\n");
					}
					stringBuilder.Append("StoresThings".Translate() + ": ");
					bool flag = true;
					foreach (Thing heldThing in slotGroup.HeldThings)
					{
						if (!flag)
						{
							stringBuilder.Append(", ");
						}
						stringBuilder.Append(heldThing.LabelShortCap);
						flag = false;
					}
					text = string.Concat(text, stringBuilder, ".");
				}
			}
			return text;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			foreach (Gizmo item in StorageSettingsClipboard.CopyPasteGizmosFor(GetStoreSettings()))
			{
				yield return item;
			}
			if (!StorageTabVisible || base.MapHeld == null)
			{
				yield break;
			}
			foreach (Gizmo item2 in StorageGroupUtility.StorageGroupMemberGizmos(this))
			{
				yield return item2;
			}
			if (Find.Selector.NumSelected != 1)
			{
				yield break;
			}
			foreach (Thing heldThing in slotGroup.HeldThings)
			{
				yield return ContainingSelectionUtility.CreateSelectStorageGizmo("CommandSelectStoredThing".Translate(heldThing), "CommandSelectStoredThingDesc".Translate() + "\n\n" + heldThing.GetInspectString(), heldThing, heldThing, groupable: false);
			}
		}
	}
}
