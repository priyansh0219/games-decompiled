using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StorageGroup : IExposable, ILoadReferenceable, IStoreSettingsParent, ISlotGroup
	{
		public int loadID;

		private Map map;

		[LoadAlias("buildings")]
		public List<IStorageGroupMember> members = new List<IStorageGroupMember>();

		private StorageSettings settings;

		private static List<IntVec3> tmpCellsList = new List<IntVec3>(128);

		public Map Map => map;

		public int MemberCount => members.Count;

		public bool StorageTabVisible => true;

		StorageSettings ISlotGroup.Settings => GetStoreSettings();

		StorageGroup ISlotGroup.StorageGroup => this;

		public IEnumerable<Thing> HeldThings
		{
			get
			{
				foreach (IStorageGroupMember member in members)
				{
					if (!(member is ISlotGroupParent slotGroupParent))
					{
						continue;
					}
					foreach (Thing heldThing in slotGroupParent.GetSlotGroup().HeldThings)
					{
						yield return heldThing;
					}
				}
			}
		}

		public List<IntVec3> CellsList
		{
			get
			{
				tmpCellsList.Clear();
				foreach (IStorageGroupMember member in members)
				{
					if (member is ISlotGroupParent slotGroupParent)
					{
						tmpCellsList.AddRange(slotGroupParent.GetSlotGroup().CellsList);
					}
				}
				return tmpCellsList;
			}
		}

		public StorageGroup()
		{
		}

		public StorageGroup(Map map)
		{
			this.map = map;
			settings = new StorageSettings(this);
		}

		public void InitFrom(IStorageGroupMember member)
		{
			settings.CopyFrom(member.StoreSettings);
		}

		public void RemoveMember(IStorageGroupMember member, bool removeIfEmpty = true)
		{
			if (members.Remove(member))
			{
				if (member is IStoreSettingsParent storeSettingsParent)
				{
					storeSettingsParent.Notify_SettingsChanged();
				}
				if (removeIfEmpty)
				{
					map.storageGroups.Notify_MemberRemoved(this);
				}
			}
		}

		public StorageSettings GetStoreSettings()
		{
			return settings;
		}

		public StorageSettings GetParentStoreSettings()
		{
			if (members.Any())
			{
				return members[0].ParentStoreSettings;
			}
			return null;
		}

		public void Notify_SettingsChanged()
		{
			foreach (IStorageGroupMember member in members)
			{
				if (member is ISlotGroupParent slotGroupParent)
				{
					slotGroupParent.Notify_SettingsChanged();
				}
			}
		}

		public string GetUniqueLoadID()
		{
			return "StorageGroup_" + loadID;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref loadID, "loadID", 0);
			Scribe_References.Look(ref map, "map");
			Scribe_Collections.Look(ref members, "members", LookMode.Reference);
			Scribe_Deep.Look(ref settings, "settings", this);
			if (Scribe.mode != LoadSaveMode.PostLoadInit)
			{
				return;
			}
			if (settings == null)
			{
				settings = new StorageSettings(this);
				if (members.Count > 0)
				{
					settings.CopyFrom(members[0].ThingStoreSettings);
				}
				else
				{
					settings.CopyFrom(StorageSettings.EverStorableFixedSettings());
				}
			}
			members.RemoveAll((IStorageGroupMember x) => x == null);
		}
	}
}
