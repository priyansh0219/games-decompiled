using System;
using System.Collections.Generic;

namespace Verse
{
	public sealed class ListerThings
	{
		private Dictionary<ThingDef, List<Thing>> listsByDef = new Dictionary<ThingDef, List<Thing>>(ThingDefComparer.Instance);

		private List<Thing>[] listsByGroup;

		private int[] stateHashByGroup;

		public ListerThingsUse use;

		private static readonly List<Thing> EmptyList = new List<Thing>();

		public List<Thing> AllThings => listsByGroup[2];

		public ListerThings(ListerThingsUse use)
		{
			this.use = use;
			listsByGroup = new List<Thing>[ThingListGroupHelper.AllGroups.Length];
			stateHashByGroup = new int[ThingListGroupHelper.AllGroups.Length];
			listsByGroup[2] = new List<Thing>();
		}

		public List<Thing> ThingsInGroup(ThingRequestGroup group)
		{
			return ThingsMatching(ThingRequest.ForGroup(group));
		}

		public int StateHashOfGroup(ThingRequestGroup group)
		{
			if (use == ListerThingsUse.Region && !group.StoreInRegion())
			{
				Log.ErrorOnce(string.Concat("Tried to get state hash of group ", group, " in a region, but this group is never stored in regions. Most likely a global query should have been used."), 1968738832);
				return -1;
			}
			return Gen.HashCombineInt(85693994, stateHashByGroup[(uint)group]);
		}

		public List<Thing> ThingsOfDef(ThingDef def)
		{
			return ThingsMatching(ThingRequest.ForDef(def));
		}

		public List<Thing> ThingsMatching(ThingRequest req)
		{
			if (req.singleDef != null)
			{
				if (!listsByDef.TryGetValue(req.singleDef, out var value))
				{
					return EmptyList;
				}
				return value;
			}
			if (req.group != 0)
			{
				if (use == ListerThingsUse.Region && !req.group.StoreInRegion())
				{
					Log.ErrorOnce(string.Concat("Tried to get things in group ", req.group, " in a region, but this group is never stored in regions. Most likely a global query should have been used."), 1968735132);
					return EmptyList;
				}
				return listsByGroup[(uint)req.group] ?? EmptyList;
			}
			throw new InvalidOperationException("Invalid ThingRequest " + req);
		}

		public bool Contains(Thing t)
		{
			return AllThings.Contains(t);
		}

		public void Add(Thing t)
		{
			if (!EverListable(t.def, use))
			{
				return;
			}
			if (!listsByDef.TryGetValue(t.def, out var value))
			{
				value = new List<Thing>();
				listsByDef.Add(t.def, value);
			}
			value.Add(t);
			ThingRequestGroup[] allGroups = ThingListGroupHelper.AllGroups;
			foreach (ThingRequestGroup thingRequestGroup in allGroups)
			{
				if ((use != ListerThingsUse.Region || thingRequestGroup.StoreInRegion()) && thingRequestGroup.Includes(t.def))
				{
					List<Thing> list = listsByGroup[(uint)thingRequestGroup];
					if (list == null)
					{
						list = new List<Thing>();
						listsByGroup[(uint)thingRequestGroup] = list;
						stateHashByGroup[(uint)thingRequestGroup] = 0;
					}
					list.Add(t);
					stateHashByGroup[(uint)thingRequestGroup]++;
				}
			}
		}

		public void Remove(Thing t)
		{
			if (!EverListable(t.def, use))
			{
				return;
			}
			listsByDef[t.def].Remove(t);
			ThingRequestGroup[] allGroups = ThingListGroupHelper.AllGroups;
			for (int i = 0; i < allGroups.Length; i++)
			{
				ThingRequestGroup thingRequestGroup = allGroups[i];
				if ((use != ListerThingsUse.Region || thingRequestGroup.StoreInRegion()) && thingRequestGroup.Includes(t.def))
				{
					listsByGroup[i].Remove(t);
					stateHashByGroup[(uint)thingRequestGroup]++;
				}
			}
		}

		public static bool EverListable(ThingDef def, ListerThingsUse use)
		{
			if (def.category == ThingCategory.Mote && (!def.drawGUIOverlay || use == ListerThingsUse.Region))
			{
				return false;
			}
			if (def.category == ThingCategory.Projectile && use == ListerThingsUse.Region)
			{
				return false;
			}
			return true;
		}

		public void Clear()
		{
			listsByDef.Clear();
			for (int i = 0; i < listsByGroup.Length; i++)
			{
				if (listsByGroup[i] != null)
				{
					listsByGroup[i].Clear();
				}
				stateHashByGroup[i] = 0;
			}
		}
	}
}
