using System.Collections.Generic;
using RimWorld;

namespace Verse
{
	public class StorageGroupManager : IExposable
	{
		public Map map;

		private List<StorageGroup> groups = new List<StorageGroup>();

		public StorageGroupManager(Map map)
		{
			this.map = map;
		}

		public StorageGroup NewGroup()
		{
			StorageGroup storageGroup = new StorageGroup(map);
			storageGroup.loadID = Find.UniqueIDsManager.GetNextStorageGroupID();
			groups.Add(storageGroup);
			return storageGroup;
		}

		public void Notify_MemberRemoved(StorageGroup group)
		{
			if (group.MemberCount <= 1)
			{
				for (int num = group.MemberCount - 1; num >= 0; num--)
				{
					group.members[num].SetStorageGroup(null);
				}
				groups.Remove(group);
			}
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref groups, "groups", LookMode.Deep);
			Scribe_References.Look(ref map, "map");
		}
	}
}
