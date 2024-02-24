using System;
using System.Collections.Generic;
using Gendarme;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class CraftingAnalytics : MonoBehaviour, IProtoEventListener
{
	private sealed class Selector : List<EntryData>
	{
		private List<int> list = new List<int>();

		public IEnumerator<int> GetEnumerator(EntryProperty property)
		{
			list.Clear();
			for (int i = 0; i < base.Count; i++)
			{
				EntryData entryData = base[i];
				switch (property)
				{
				case EntryProperty.TechType:
					list.Add((int)entryData.techType);
					break;
				case EntryProperty.CraftCount:
					list.Add(entryData.craftCount);
					break;
				case EntryProperty.TimeCraftFirst:
					list.Add(entryData.timeCraftFirst);
					break;
				case EntryProperty.TimeCraftLast:
					list.Add(entryData.timeCraftLast);
					break;
				}
			}
			return list.GetEnumerator();
		}
	}

	private enum EntryProperty
	{
		None = 0,
		TechType = 1,
		TimeScanFirst = 2,
		TimeScanLast = 3,
		CraftCount = 4,
		TimeCraftFirst = 5,
		TimeCraftLast = 6
	}

	[ProtoContract]
	public struct EntryData
	{
		public TechType techType;

		[NonSerialized]
		[ProtoMember(1)]
		public int timeScanFirst;

		[NonSerialized]
		[ProtoMember(2)]
		public int timeScanLast;

		[NonSerialized]
		[ProtoMember(3)]
		public int craftCount;

		[NonSerialized]
		[ProtoMember(4)]
		public int timeCraftFirst;

		[NonSerialized]
		[ProtoMember(5)]
		public int timeCraftLast;
	}

	private const ManagedUpdate.Queue updateQueue = ManagedUpdate.Queue.LateUpdateAfterInput;

	private static CraftingAnalytics _main;

	private const int currentVersion = 2;

	[NonSerialized]
	[ProtoMember(1)]
	public int _serializedVersion = 2;

	[NonSerialized]
	[ProtoMember(2)]
	public bool _active;

	[NonSerialized]
	[ProtoMember(3)]
	public readonly Dictionary<TechType, EntryData> entries = new Dictionary<TechType, EntryData>(TechTypeExtensions.sTechTypeComparer);

	private bool _lastActive;

	private List<TechType> toUpdate = new List<TechType>();

	private Selector selector = new Selector();

	public static CraftingAnalytics main => _main;

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	private void Awake()
	{
		if (_main != null)
		{
			Debug.LogErrorFormat("Multiple {0} in scene!", GetType().ToString());
			UnityEngine.Object.Destroy(this);
		}
		else
		{
			_main = this;
			_active |= !Utils.GetContinueMode();
			Initialize();
		}
	}

	private void OnEnable()
	{
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.LateUpdateAfterInput, OnUpdate);
	}

	private void OnDisable()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.LateUpdateAfterInput, OnUpdate);
	}

	private void OnUpdate()
	{
		for (int num = toUpdate.Count - 1; num >= 0; num--)
		{
			TechType techType = toUpdate[num];
			toUpdate.RemoveAt(num);
			UpdateUnlock(techType);
		}
	}

	private void OnLockedChange(PDAScanner.Entry entry)
	{
		if (_active)
		{
			TechType techType = PDAScanner.GetEntryData(entry.techType)?.blueprint ?? entry.techType;
			if (techType != 0 && !toUpdate.Contains(techType))
			{
				toUpdate.Add(techType);
			}
		}
	}

	private void OnCompoundAdd(TechType techType, int unlocked, int total)
	{
		if (_active && techType != 0 && !toUpdate.Contains(techType))
		{
			toUpdate.Add(techType);
		}
	}

	private void OnCompoundRemove(TechType techType)
	{
		if (_active && techType != 0 && !toUpdate.Contains(techType))
		{
			toUpdate.Add(techType);
		}
	}

	public void OnConstruct(TechType techType, Vector3 position)
	{
		GameAnalytics.LegacyEvent(GameAnalytics.Event.LegacyConstruct, techType.AsString());
		UpdateCreate(GameAnalytics.Event.TechConstructed, techType, position);
	}

	public void OnCraft(TechType techType, Vector3 position)
	{
		UpdateCreate(GameAnalytics.Event.TechCrafted, techType, position);
	}

	public void AppendCraftingStatistics(GameAnalytics.EventData eventData)
	{
		if (!_active)
		{
			return;
		}
		selector.Clear();
		foreach (KeyValuePair<TechType, EntryData> entry in entries)
		{
			EntryData value = entry.Value;
			if (value.craftCount > 0)
			{
				value.techType = entry.Key;
				selector.Add(value);
			}
		}
		selector.Sort(CompareDescending);
		eventData.Add("tech_types", selector.GetEnumerator(EntryProperty.TechType));
		eventData.Add("craft_count", selector.GetEnumerator(EntryProperty.CraftCount));
		eventData.Add("time_craft_first", selector.GetEnumerator(EntryProperty.TimeCraftFirst));
		eventData.Add("time_craft_last", selector.GetEnumerator(EntryProperty.TimeCraftLast));
	}

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	private void Initialize()
	{
		if (_lastActive != _active)
		{
			_lastActive = _active;
			if (_active)
			{
				PDAScanner.onAdd = (PDAScanner.OnEntryEvent)Delegate.Combine(PDAScanner.onAdd, new PDAScanner.OnEntryEvent(OnLockedChange));
				PDAScanner.onRemove = (PDAScanner.OnEntryEvent)Delegate.Combine(PDAScanner.onRemove, new PDAScanner.OnEntryEvent(OnLockedChange));
				KnownTech.onCompoundAdd += OnCompoundAdd;
				KnownTech.onCompoundRemove += OnCompoundRemove;
			}
			else
			{
				toUpdate.Clear();
				PDAScanner.onAdd = (PDAScanner.OnEntryEvent)Delegate.Remove(PDAScanner.onAdd, new PDAScanner.OnEntryEvent(OnLockedChange));
				PDAScanner.onRemove = (PDAScanner.OnEntryEvent)Delegate.Remove(PDAScanner.onRemove, new PDAScanner.OnEntryEvent(OnLockedChange));
				KnownTech.onCompoundAdd -= OnCompoundAdd;
				KnownTech.onCompoundRemove -= OnCompoundRemove;
			}
		}
	}

	private EntryData EnsureEntry(TechType techType)
	{
		if (!entries.TryGetValue(techType, out var value))
		{
			EntryData entryData = default(EntryData);
			entryData.techType = techType;
			entryData.timeScanFirst = -1;
			entryData.timeScanLast = -1;
			entryData.craftCount = 0;
			entryData.timeCraftFirst = -1;
			entryData.timeCraftLast = -1;
			value = entryData;
			entries.Add(techType, value);
		}
		return value;
	}

	private void UpdateUnlock(TechType techType)
	{
		if (!_active)
		{
			return;
		}
		int timeNow = GameAnalytics.timeNow;
		EntryData value = EnsureEntry(techType);
		int unlocked;
		int total;
		TechUnlockState techUnlockState = KnownTech.GetTechUnlockState(techType, out unlocked, out total);
		if (value.timeScanFirst < 0)
		{
			value.timeScanFirst = timeNow;
		}
		if (techUnlockState == TechUnlockState.Available && value.timeScanLast < 0)
		{
			value.timeScanLast = timeNow;
			using (GameAnalytics.EventData eventData = GameAnalytics.CustomEvent(GameAnalytics.Event.BlueprintUnlocked))
			{
				eventData.Add("tech_type", (int)techType);
				eventData.Add("time", timeNow - value.timeScanFirst);
			}
		}
		entries[techType] = value;
	}

	private void UpdateCreate(GameAnalytics.Event eventId, TechType techType, Vector3 position)
	{
		if (!_active)
		{
			return;
		}
		int timeNow = GameAnalytics.timeNow;
		EntryData value = EnsureEntry(techType);
		value.craftCount++;
		value.timeCraftLast = timeNow;
		if (value.craftCount == 1)
		{
			value.timeCraftFirst = timeNow;
			if (value.timeScanLast > 0)
			{
				using (GameAnalytics.EventData eventData = GameAnalytics.CustomEvent(GameAnalytics.Event.FirstUnlockedCreate))
				{
					eventData.Add("tech_type", (int)techType);
					eventData.Add("time", timeNow - value.timeScanLast);
				}
			}
		}
		using (GameAnalytics.EventData eventData2 = GameAnalytics.CustomEvent(eventId))
		{
			eventData2.Add("tech_type", (int)techType);
			eventData2.AddPosition(position);
		}
		entries[techType] = value;
	}

	private bool IsCreated(TechType techType)
	{
		if (entries.TryGetValue(techType, out var value))
		{
			return value.craftCount > 0;
		}
		return false;
	}

	public int CompareDescending(EntryData x, EntryData y)
	{
		return y.craftCount.CompareTo(x.craftCount);
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		Initialize();
		List<TechType> list = new List<TechType>(entries.Count);
		foreach (KeyValuePair<TechType, EntryData> entry in entries)
		{
			list.Add(entry.Key);
		}
		for (int i = 0; i < list.Count; i++)
		{
			TechType techType = list[i];
			EntryData value = entries[techType];
			value.techType = techType;
			entries[techType] = value;
		}
		if (_serializedVersion >= 2)
		{
			return;
		}
		_serializedVersion = 2;
		for (int j = 0; j < list.Count; j++)
		{
			TechType key = list[j];
			EntryData value2 = entries[key];
			if (value2.timeScanFirst >= 0)
			{
				value2.timeScanFirst -= 480;
			}
			if (value2.timeScanLast >= 0)
			{
				value2.timeScanLast -= 480;
			}
			entries[key] = value2;
		}
	}
}
