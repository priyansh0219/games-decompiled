using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

public static class PDALog
{
	public delegate void OnAdd(Entry entry);

	public delegate void OnRemove(Entry entry);

	public delegate void OnSetTime();

	public enum EntryType
	{
		Default = 0,
		LowPriDefer = 1,
		NowOrNever = 2,
		Invalid = 3
	}

	public class EntryTypeComparer : IEqualityComparer<EntryType>
	{
		public bool Equals(EntryType x, EntryType y)
		{
			int num = (int)x;
			return num.Equals((int)y);
		}

		public int GetHashCode(EntryType obj)
		{
			return (int)obj;
		}
	}

	[Serializable]
	public class EntryData
	{
		public string key = "";

		public EntryType type;

		public Sprite icon;

		public FMODAsset sound;

		public bool doNotAutoPlay;
	}

	[ProtoContract]
	public class Entry
	{
		[ProtoMember(1)]
		public float timestamp;

		public EntryData data;
	}

	public static readonly EntryTypeComparer sEntryTypeComparer = new EntryTypeComparer();

	private const ManagedUpdate.Queue queue = ManagedUpdate.Queue.UpdateBeforeInput;

	private static Sprite iconDefault;

	private static readonly Dictionary<string, EntryData> mapping = new Dictionary<string, EntryData>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);

	private static float time;

	public static bool initialized { get; private set; }

	public static event OnAdd onAdd;

	public static event OnRemove onRemove;

	public static event OnSetTime onSetTime;

	public static void Initialize(PDAData pdaData)
	{
		if (initialized)
		{
			return;
		}
		initialized = true;
		entries.Clear();
		List<EntryData> log = pdaData.log;
		mapping.Clear();
		int i = 0;
		for (int count = log.Count; i < count; i++)
		{
			EntryData entryData = log[i];
			string key = entryData.key;
			if (string.IsNullOrEmpty(key))
			{
				Debug.LogError("PDALog : Initialize() : Empty key found at index " + i);
			}
			else if (mapping.ContainsKey(key))
			{
				Debug.LogErrorFormat("PDALog : Initialize() : Duplicate key '{0}' found at index {1}.", key, i);
			}
			else
			{
				mapping.Add(key, entryData);
			}
		}
		iconDefault = pdaData.defaultLogIcon;
		InitDataForEntries();
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.UpdateBeforeInput, OnUpdate);
	}

	public static void Deinitialize()
	{
		ManagedUpdate.Unsubscribe(OnUpdate);
		iconDefault = null;
		mapping.Clear();
		entries.Clear();
		PDALog.onAdd = null;
		PDALog.onRemove = null;
		PDALog.onSetTime = null;
		time = 0f;
		initialized = false;
	}

	private static void InitDataForEntries()
	{
		Dictionary<string, Entry>.Enumerator enumerator = entries.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, Entry> current = enumerator.Current;
			string key = current.Key;
			Entry value = current.Value;
			if (GetEntryData(key, out var entryData))
			{
				value.data = entryData;
			}
		}
	}

	private static void OnUpdate()
	{
		if (!(DayNightCycle.main != null))
		{
			return;
		}
		float num = time;
		time = DayNightCycle.main.timePassedAsFloat;
		foreach (KeyValuePair<string, Entry> entry in entries)
		{
			Entry value = entry.Value;
			if (!(value.timestamp <= num) && value.timestamp <= time)
			{
				NotificationManager.main.Add(NotificationManager.Group.Log, value.data.key, 3f);
				NotifyAdd(value);
			}
		}
	}

	public static EntryData Add(string key, bool playSound = true)
	{
		EntryData entryData = null;
		if (!entries.ContainsKey(key))
		{
			if (!GetEntryData(key, out entryData))
			{
				Debug.LogErrorFormat("PDALog : Add() : EntryData for key='{0}' is not found!", key);
				entryData = new EntryData();
				entryData.key = key;
				entryData.type = EntryType.Invalid;
			}
			Entry entry = new Entry();
			entry.data = entryData;
			entries.Add(entryData.key, entry);
			float num = 0f;
			if (playSound)
			{
				if (entryData.sound != null)
				{
					if (!entryData.doNotAutoPlay)
					{
						PDASounds.queue.PlayQueued(entryData.sound.id, entryData.key);
					}
					num = (float)FMODExtensions.GetLength(entryData.sound.path) * 0.001f;
				}
				else
				{
					Subtitles.Add(entryData.key);
				}
			}
			if (num <= 0f)
			{
				entry.timestamp = time;
				NotificationManager.main.Add(NotificationManager.Group.Log, entryData.key, 3f);
				NotifyAdd(entry);
			}
			else
			{
				entry.timestamp = time + num;
			}
		}
		return entryData;
	}

	public static void Remove(string key)
	{
		if (entries.ContainsKey(key))
		{
			Entry entry = entries[key];
			entries.Remove(key);
			NotificationManager.main.Remove(NotificationManager.Group.Log, key);
			NotifyRemove(entry);
		}
	}

	public static bool GetEntryData(string key, out EntryData entryData)
	{
		return mapping.TryGetValue(key, out entryData);
	}

	public static Dictionary<string, EntryData>.Enumerator GetMapping()
	{
		return mapping.GetEnumerator();
	}

	public static IEnumerable<KeyValuePair<string, Entry>> GetEntries(bool includeDelayed = false)
	{
		foreach (KeyValuePair<string, Entry> entry in entries)
		{
			if (includeDelayed || entry.Value.timestamp <= time)
			{
				yield return entry;
			}
		}
	}

	public static bool Contains(string key)
	{
		return entries.ContainsKey(key);
	}

	public static Sprite GetIcon(Sprite sprite)
	{
		if (sprite != null)
		{
			return sprite;
		}
		return iconDefault;
	}

	public static void SetTime(float value)
	{
		time = value;
		if (PDALog.onSetTime != null)
		{
			PDALog.onSetTime();
		}
	}

	private static void NotifyAdd(Entry entry)
	{
		if (PDALog.onAdd != null)
		{
			PDALog.onAdd(entry);
		}
	}

	private static void NotifyRemove(Entry entry)
	{
		if (PDALog.onRemove != null)
		{
			PDALog.onRemove(entry);
		}
	}

	public static Dictionary<string, Entry> Serialize()
	{
		OnUpdate();
		return entries;
	}

	public static void Deserialize(Dictionary<string, Entry> data)
	{
		if (!initialized)
		{
			Debug.LogError("PDALog : Deserialize() : Deserializing uninitialized PDALog!");
			return;
		}
		if (DayNightCycle.main != null)
		{
			time = DayNightCycle.main.timePassedAsFloat;
		}
		if (data != null)
		{
			Dictionary<string, Entry>.Enumerator enumerator = data.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, Entry> current = enumerator.Current;
				entries[current.Key] = current.Value;
			}
			InitDataForEntries();
		}
	}
}
