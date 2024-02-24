using System;
using System.Collections.Generic;
using Gendarme;
using ProtoBuf;
using UnityEngine;

[SuppressMessage("Gendarme.Rules.Concurrency", "NonConstantStaticFieldsShouldNotBeVisibleRule")]
public static class PDAEncyclopedia
{
	public delegate void OnAdd(CraftNode node, bool verbose);

	public delegate void OnUpdate(CraftNode node);

	public delegate void OnRemove(CraftNode node);

	[Serializable]
	public class EntryData
	{
		public enum Kind
		{
			Encyclopedia = 0,
			Journal = 1,
			TimeCapsule = 2
		}

		public string key = "";

		public string path = "";

		[NonSerialized]
		public string[] nodes;

		public Kind kind;

		public bool unlocked;

		public Sprite popup;

		public Texture2D image;

		public FMODAsset sound;

		public FMODAsset audio;
	}

	[ProtoContract]
	public class Entry
	{
		[NonSerialized]
		[ProtoMember(1)]
		public float timestamp;

		[NonSerialized]
		[ProtoMember(2)]
		public string timeCapsuleId;
	}

	private const string timeCapsulePath = "TimeCapsules";

	private static readonly char[] sPathSplitChars = new char[2] { '/', '\\' };

	private static bool initialized = false;

	private static Dictionary<string, EntryData> mapping;

	private static Dictionary<string, Entry> entries;

	public static OnAdd onAdd;

	public static OnUpdate onUpdate;

	public static OnRemove onRemove;

	public static CraftNode tree { get; private set; }

	public static void Initialize(PDAData pdaData)
	{
		if (initialized)
		{
			return;
		}
		initialized = true;
		entries = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
		tree = new CraftNode("Root");
		List<EntryData> encyclopedia = pdaData.encyclopedia;
		mapping = new Dictionary<string, EntryData>(StringComparer.OrdinalIgnoreCase);
		int i = 0;
		for (int count = encyclopedia.Count; i < count; i++)
		{
			EntryData entryData = encyclopedia[i];
			string key = entryData.key;
			if (string.IsNullOrEmpty(key))
			{
				Debug.LogError("PDAEncyclopedia : Initialize() : Empty key found at index " + i);
			}
			else if (mapping.ContainsKey(key))
			{
				Debug.LogErrorFormat("PDAEncyclopedia : Initialize() : Duplicate key '{0}' found at index {1}.", key, i);
			}
			else
			{
				entryData.nodes = ParsePath(entryData.path);
				mapping.Add(key, entryData);
			}
		}
		int j = 0;
		for (int count2 = encyclopedia.Count; j < count2; j++)
		{
			EntryData entryData2 = encyclopedia[j];
			if (entryData2.unlocked)
			{
				Add(entryData2.key, null, verbose: false);
			}
		}
	}

	public static void Deinitialize()
	{
		mapping = null;
		entries = null;
		tree = null;
		onAdd = null;
		onUpdate = null;
		onRemove = null;
		initialized = false;
	}

	public static void OnLanguageChanged()
	{
		using (IEnumerator<CraftNode> enumerator = tree.Traverse(includeSelf: false))
		{
			Language main = Language.main;
			while (enumerator.MoveNext())
			{
				CraftNode current = enumerator.Current;
				if (!current.bool0)
				{
					current.string1 = main.Get(current.string0);
				}
			}
		}
	}

	public static EntryData AddAndPlaySound(string key)
	{
		EntryData entryData = Add(key, verbose: true);
		if (entryData != null && entryData.sound != null)
		{
			PDASounds.queue.PlayQueued(entryData.sound);
		}
		return entryData;
	}

	public static void AddTimeCapsule(string key, bool verbose)
	{
		Entry entry = new Entry();
		entry.timestamp = DayNightCycle.main.timePassedAsFloat;
		entry.timeCapsuleId = key;
		Add(key, entry, verbose);
	}

	public static void UpdateTimeCapsule(string key)
	{
		if (ContainsEntry(key) && GetEntryData(key, out var entryData) && entryData.kind == EntryData.Kind.TimeCapsule)
		{
			CraftNode parent = GetParent(entryData, create: false);
			if (parent != null && parent[key] is CraftNode craftNode)
			{
				craftNode.string1 = TimeCapsuleContentProvider.GetTitle(key);
				NotifyUpdate(craftNode);
			}
		}
	}

	public static void RemoveTimeCapsule(string key)
	{
		if (!ContainsEntry(key) || !GetEntryData(key, out var entryData) || entryData.kind != EntryData.Kind.TimeCapsule)
		{
			return;
		}
		CraftNode craftNode = GetParent(entryData, create: false);
		if (craftNode == null)
		{
			return;
		}
		CraftNode craftNode2 = craftNode[key] as CraftNode;
		if (craftNode2 != null)
		{
			NotificationManager.main.Remove(NotificationManager.Group.Encyclopedia, entryData.key);
			do
			{
				NotifyRemove(craftNode2);
				craftNode.RemoveNode(craftNode2);
				craftNode2 = craftNode;
				craftNode = craftNode2.parent as CraftNode;
			}
			while (craftNode != null && craftNode2.childCount == 0);
			entries.Remove(key);
			mapping.Remove(key);
		}
	}

	public static EntryData Add(string key, bool verbose)
	{
		if (!string.IsNullOrEmpty(key) && HasEntryData(key) && !ContainsEntry(key))
		{
			return Add(key, null, verbose);
		}
		return null;
	}

	public static void AddAllEntries(bool verbose = false)
	{
		Dictionary<string, EntryData>.Enumerator enumerator = mapping.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Add(enumerator.Current.Key, verbose);
		}
	}

	private static CraftNode GetParent(EntryData entryData, bool create)
	{
		if (entryData == null)
		{
			return null;
		}
		string[] nodes = entryData.nodes;
		Language main = Language.main;
		CraftNode craftNode = tree;
		int i = 0;
		for (int num = nodes.Length; i < num; i++)
		{
			string text = nodes[i];
			TreeNode treeNode = craftNode[text];
			if (treeNode == null)
			{
				if (!create)
				{
					return null;
				}
				string pathString = craftNode.GetPathString('/', includeSelf: true, "EncyPath_", text);
				treeNode = new CraftNode(text, TreeAction.Expand)
				{
					string0 = pathString,
					string1 = main.Get(pathString)
				};
				craftNode.AddNode(treeNode);
			}
			craftNode = treeNode as CraftNode;
		}
		return craftNode;
	}

	private static EntryData Add(string key, Entry entry, bool verbose)
	{
		if (!ContainsEntry(key))
		{
			if (entry == null)
			{
				entry = new Entry();
				entry.timestamp = DayNightCycle.main.timePassedAsFloat;
			}
			entries.Add(key, entry);
			if (GetEntryData(key, out var entryData))
			{
				entry.timeCapsuleId = null;
			}
			else if (!string.IsNullOrEmpty(entry.timeCapsuleId))
			{
				if (!string.IsNullOrEmpty(entry.timeCapsuleId))
				{
					entryData = new EntryData();
					entryData.key = entry.timeCapsuleId;
					entryData.path = "TimeCapsules";
					entryData.kind = EntryData.Kind.TimeCapsule;
					entryData.unlocked = false;
					entryData.popup = null;
					entryData.image = null;
					entryData.sound = null;
					entryData.nodes = ParsePath(entryData.path);
					mapping.Add(entry.timeCapsuleId, entryData);
				}
			}
			else
			{
				Debug.LogError("PDAEncyclopedia : Add() : Entry for key='" + key + "' is not found! It is either never existed in PDAData prefab or was removed.");
			}
			if (entryData != null)
			{
				CraftNode parent = GetParent(entryData, create: true);
				if (parent[entryData.key] == null)
				{
					string empty = string.Empty;
					string empty2 = string.Empty;
					if (entryData.kind == EntryData.Kind.TimeCapsule)
					{
						empty = entryData.key;
						empty2 = TimeCapsuleContentProvider.GetTitle(entryData.key);
					}
					else
					{
						empty = $"Ency_{entryData.key}";
						empty2 = Language.main.Get(empty);
					}
					CraftNode craftNode = new CraftNode(entryData.key, TreeAction.Craft)
					{
						string0 = empty,
						string1 = empty2,
						bool0 = (entryData.kind == EntryData.Kind.TimeCapsule)
					};
					parent.AddNode(craftNode);
					if (verbose)
					{
						NotificationManager.main.Add(NotificationManager.Group.Encyclopedia, entryData.key, 3f);
					}
					NotifyAdd(craftNode, verbose);
					return entryData;
				}
				Debug.LogError("PDAEncyclopedia : Add() : Node at path '" + parent.GetPathString('/', includeSelf: true) + "' already contains child node with identifier '" + entryData.key + "'.");
			}
		}
		return null;
	}

	public static bool ContainsEntry(string key)
	{
		return entries.ContainsKey(key);
	}

	public static Dictionary<string, Entry>.Enumerator GetEntries()
	{
		return entries.GetEnumerator();
	}

	public static bool HasEntryData(string key)
	{
		if (mapping != null)
		{
			return mapping.ContainsKey(key);
		}
		return false;
	}

	public static bool GetEntryData(string key, out EntryData entryData)
	{
		if (mapping != null && mapping.TryGetValue(key, out entryData))
		{
			return true;
		}
		entryData = null;
		return false;
	}

	public static Dictionary<string, Entry> Serialize()
	{
		return entries;
	}

	public static void Deserialize(Dictionary<string, Entry> data)
	{
		if (!initialized)
		{
			Debug.LogError("PDAEncyclopedia : Deserialize() : Deserializing uninitialized PDAEncyclopedia!");
		}
		else if (data != null)
		{
			Dictionary<string, Entry>.Enumerator enumerator = data.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, Entry> current = enumerator.Current;
				string key = current.Key;
				Entry value = current.Value;
				Add(key, value, verbose: false);
			}
		}
	}

	private static string[] ParsePath(string path)
	{
		return path.Split(sPathSplitChars, StringSplitOptions.RemoveEmptyEntries);
	}

	public static CraftNode GetNode(string key)
	{
		return tree.FindNodeById(key, ignoreCase: true) as CraftNode;
	}

	private static void NotifyAdd(CraftNode node, bool verbose)
	{
		if (onAdd != null)
		{
			onAdd(node, verbose);
		}
	}

	private static void NotifyUpdate(CraftNode node)
	{
		if (onUpdate != null)
		{
			onUpdate(node);
		}
	}

	private static void NotifyRemove(CraftNode node)
	{
		if (onRemove != null)
		{
			onRemove(node);
		}
	}
}
