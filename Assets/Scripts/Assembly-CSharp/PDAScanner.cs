using System;
using System.Collections.Generic;
using System.Text;
using Gendarme;
using ProtoBuf;
using Story;
using UnityEngine;

public static class PDAScanner
{
	public struct ScanTarget
	{
		public TechType techType;

		public GameObject gameObject;

		public string uid;

		public Transform transform;

		public float mass;

		public float progress;

		public bool isPlayer => techType == TechType.Player;

		public bool hasUID => !string.IsNullOrEmpty(uid);

		public bool isValid
		{
			get
			{
				if (techType != 0)
				{
					return gameObject != null;
				}
				return false;
			}
		}

		public void Invalidate()
		{
			techType = TechType.None;
			gameObject = null;
			uid = null;
			transform = null;
			mass = 1f;
			progress = 0f;
		}

		public void Initialize(GameObject candidate)
		{
			Invalidate();
			if (!Targeting.GetRoot(candidate, out techType, out gameObject))
			{
				return;
			}
			LiveMixin component = gameObject.GetComponent<LiveMixin>();
			if (component == null || component.IsAlive())
			{
				Rigidbody rigidbody = null;
				transform = gameObject.GetComponent<Transform>();
				rigidbody = gameObject.GetComponent<Rigidbody>();
				mass = ((rigidbody == null) ? 1f : rigidbody.mass);
				UniqueIdentifier component2 = gameObject.GetComponent<UniqueIdentifier>();
				if (component2 != null)
				{
					uid = component2.Id;
				}
			}
		}

		public Vector3 GetRandomLocalPointInBounds()
		{
			if (transform != null)
			{
				return new Vector3(UnityEngine.Random.value - 0.5f, UnityEngine.Random.value - 0.5f, UnityEngine.Random.value - 0.5f);
			}
			return Vector3.zero;
		}

		public Vector3 TransformPoint(Vector3 localPosition)
		{
			if (transform != null)
			{
				return transform.TransformPoint(localPosition);
			}
			return localPosition;
		}
	}

	public enum Result
	{
		None = 0,
		Processed = 1,
		Known = 2,
		Scan = 3,
		Done = 4,
		Researched = 5,
		NotInfected = 6,
		Infected = 7
	}

	public delegate void OnEntryEvent(Entry entry);

	[Serializable]
	public class EntryData
	{
		public const int minFragments = 1;

		public const int maxFragments = 30;

		public TechType key;

		public bool locked;

		public int totalFragments = 1;

		public bool destroyAfterScan;

		public string encyclopedia = "";

		public TechType blueprint;

		public float scanTime = 1f;

		public bool isFragment;
	}

	[ProtoContract]
	public class Entry : IComparable, IComparable<Entry>, IEquatable<Entry>
	{
		[ProtoMember(1)]
		public TechType techType;

		[ProtoMember(2)]
		public float progress;

		[ProtoMember(3)]
		public int unlocked;

		public int CompareTo(object other)
		{
			return CompareTo(other as Entry);
		}

		public int CompareTo(Entry other)
		{
			if (other == null)
			{
				return -1;
			}
			return techType.CompareTo(other.techType);
		}

		public override int GetHashCode()
		{
			return (int)techType;
		}

		public bool Equals(Entry other)
		{
			if (other == null)
			{
				return false;
			}
			return techType == other.techType;
		}
	}

	[ProtoContract]
	public class Data
	{
		[ProtoMember(1)]
		public int version = 2;

		[NonSerialized]
		[ProtoMember(2, OverwriteList = true)]
		public Dictionary<string, float> fragments;

		[NonSerialized]
		[ProtoMember(3, OverwriteList = true)]
		public List<Entry> partial;

		[NonSerialized]
		[ProtoMember(4, OverwriteList = true)]
		public HashSet<TechType> complete;
	}

	private const string infectionEncyclopediaEntry = "Infection";

	private const string healedInfectionEncyclopediaEntry = "CuredCreature";

	private const string heroPeeperEncyclopediaEntry = "HeroPeeper";

	private const string aquariumPeeperEncyclopediaEntry = "AquariumPeeper";

	private const float redundantScanTime = 2f;

	private const float selfScanTime = 1.7f;

	private const int currentVersion = 2;

	private static Dictionary<string, float> fragments;

	private static List<Entry> partial;

	private static HashSet<TechType> complete;

	[SuppressMessage("Gendarme.Rules.Concurrency", "NonConstantStaticFieldsShouldNotBeVisibleRule")]
	public static ScanTarget scanTarget = default(ScanTarget);

	public static OnEntryEvent onAdd;

	public static OnEntryEvent onRemove;

	public static OnEntryEvent onProgress;

	private static bool initialized = false;

	private static Dictionary<TechType, EntryData> mapping;

	private static List<TechType> toRemove = new List<TechType>();

	private static Dictionary<string, float> cachedProgress = new Dictionary<string, float>();

	public static void Initialize(PDAData pdaData)
	{
		if (initialized)
		{
			return;
		}
		initialized = true;
		fragments = new Dictionary<string, float>();
		partial = new List<Entry>();
		complete = new HashSet<TechType>(TechTypeExtensions.sTechTypeComparer);
		mapping = new Dictionary<TechType, EntryData>(TechTypeExtensions.sTechTypeComparer);
		List<EntryData> scanner = pdaData.scanner;
		int i = 0;
		for (int count = scanner.Count; i < count; i++)
		{
			EntryData entryData = scanner[i];
			TechType key = entryData.key;
			if (key == TechType.None)
			{
				Debug.LogErrorFormat("PDAScanner : Initialize() : TechType.None key found at index {0}", i);
				continue;
			}
			if (mapping.ContainsKey(key))
			{
				Debug.LogErrorFormat("PDAScanner : Initialize() : Duplicate key '{0}' found at index {1}", key, i);
				continue;
			}
			if (entryData.totalFragments < 1 || entryData.totalFragments > 30)
			{
				entryData.totalFragments = Mathf.Clamp(entryData.totalFragments, 1, 30);
				Debug.LogErrorFormat("PDAScanner : Initialize() : Number of fragments cannot be less than {0} or greater than {1}! (TechType = {2}, index = {3}). Clamped to {4}", 1, 30, key, i, entryData.totalFragments);
			}
			string encyclopedia = entryData.encyclopedia;
			if (!string.IsNullOrEmpty(encyclopedia) && !PDAEncyclopedia.HasEntryData(encyclopedia))
			{
				entryData.encyclopedia = string.Empty;
				Debug.LogErrorFormat("PDAScanner : Initialize() : '{0}' is an unknown entry for PDAEncyclopedia! (index = {1})", encyclopedia, i);
			}
			mapping.Add(key, entryData);
		}
		Dictionary<TechType, EntryData>.Enumerator enumerator = mapping.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<TechType, EntryData> current = enumerator.Current;
			if (current.Value.locked)
			{
				Add(current.Key, 0);
			}
		}
	}

	public static void Deinitialize()
	{
		scanTarget.Invalidate();
		fragments = null;
		partial = null;
		complete = null;
		onAdd = null;
		onRemove = null;
		onProgress = null;
		mapping = null;
		cachedProgress.Clear();
		initialized = false;
	}

	public static Dictionary<TechType, EntryData>.Enumerator GetAllEntriesData()
	{
		return mapping.GetEnumerator();
	}

	public static EntryData GetEntryData(TechType key)
	{
		EntryData value = null;
		if (mapping != null)
		{
			mapping.TryGetValue(key, out value);
		}
		return value;
	}

	public static bool HasEntry(TechType key)
	{
		if (mapping != null)
		{
			return mapping.ContainsKey(key);
		}
		return false;
	}

	public static bool IsFragment(TechType techType)
	{
		if (techType != 0 && mapping.TryGetValue(techType, out var value))
		{
			return value.isFragment;
		}
		return false;
	}

	public static List<Entry>.Enumerator GetPartialEntries()
	{
		return partial.GetEnumerator();
	}

	public static void GetPartialEntriesWhichUnlocks(TechType techType, List<Entry> result, bool append = false)
	{
		if (!append)
		{
			result.Clear();
		}
		for (int i = 0; i < partial.Count; i++)
		{
			Entry entry = partial[i];
			if (IsEntryUnlocks(entry.techType, techType, out var _))
			{
				result.Add(entry);
			}
		}
	}

	public static bool ContainsPartialEntry(Entry entry)
	{
		return partial.Contains(entry);
	}

	private static bool IsEntryUnlocks(TechType key, TechType unlockable)
	{
		EntryData entryData;
		return IsEntryUnlocks(key, unlockable, out entryData);
	}

	private static bool IsEntryUnlocks(TechType key, TechType unlockable, out EntryData entryData)
	{
		return GetEntryUnlockable(key, out entryData) == unlockable;
	}

	public static TechType GetEntryUnlockable(TechType key, out EntryData entryData)
	{
		entryData = GetEntryData(key);
		if (entryData != null)
		{
			return entryData.blueprint;
		}
		return key;
	}

	public static bool GetPartialEntryByKey(TechType key, out Entry entry)
	{
		int i = 0;
		for (int count = partial.Count; i < count; i++)
		{
			Entry entry2 = partial[i];
			if (entry2.techType == key)
			{
				entry = entry2;
				return true;
			}
		}
		entry = null;
		return false;
	}

	public static bool AddByUnlockable(TechType techType, int unlocked = 0)
	{
		bool flag = false;
		Dictionary<TechType, EntryData>.Enumerator enumerator = mapping.GetEnumerator();
		while (enumerator.MoveNext())
		{
			EntryData value = enumerator.Current.Value;
			if (value.blueprint == techType)
			{
				flag = true;
				Add(value.key, unlocked);
			}
		}
		if (!flag)
		{
			Add(techType, unlocked);
		}
		return flag;
	}

	private static Entry Add(TechType techType, int unlocked)
	{
		if (unlocked < 0)
		{
			unlocked = 0;
		}
		if (techType == TechType.None || complete.Contains(techType))
		{
			return null;
		}
		if (GetPartialEntryByKey(techType, out var entry))
		{
			if (unlocked > entry.unlocked)
			{
				entry.unlocked = unlocked;
				NotifyProgress(entry);
			}
		}
		else
		{
			entry = new Entry();
			entry.techType = techType;
			entry.unlocked = unlocked;
			partial.Add(entry);
			NotifyAdd(entry);
		}
		return entry;
	}

	public static bool RemoveAllEntriesWhichUnlocks(TechType techType)
	{
		bool result = false;
		for (int num = partial.Count - 1; num >= 0; num--)
		{
			Entry entry = partial[num];
			if (IsEntryUnlocks(entry.techType, techType))
			{
				partial.RemoveAt(num);
				NotifyRemove(entry);
				result = true;
			}
		}
		toRemove.Clear();
		HashSet<TechType>.Enumerator enumerator = complete.GetEnumerator();
		while (enumerator.MoveNext())
		{
			TechType current = enumerator.Current;
			if (IsEntryUnlocks(current, techType))
			{
				toRemove.Add(current);
			}
		}
		for (int i = 0; i < toRemove.Count; i++)
		{
			TechType item = toRemove[i];
			complete.Remove(item);
			result = true;
		}
		toRemove.Clear();
		return result;
	}

	public static void CompleteAllEntriesWhichUnlocks(TechType techType)
	{
		for (int num = partial.Count - 1; num >= 0; num--)
		{
			Entry entry = partial[num];
			if (IsEntryUnlocks(entry.techType, techType))
			{
				partial.Remove(entry);
				complete.Add(entry.techType);
				NotifyRemove(entry);
			}
		}
		Dictionary<TechType, EntryData>.Enumerator enumerator = mapping.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<TechType, EntryData> current = enumerator.Current;
			if (current.Value.blueprint == techType)
			{
				complete.Add(current.Key);
			}
		}
	}

	public static bool ContainsCompleteEntry(TechType techType)
	{
		return complete.Contains(techType);
	}

	public static void RemoveCompleteWhichUnlocks(TechType techType)
	{
		toRemove.Clear();
		HashSet<TechType>.Enumerator enumerator = complete.GetEnumerator();
		while (enumerator.MoveNext())
		{
			TechType current = enumerator.Current;
			if (IsEntryUnlocks(current, techType))
			{
				toRemove.Add(current);
			}
		}
		int i = 0;
		for (int count = toRemove.Count; i < count; i++)
		{
			complete.Remove(toRemove[i]);
		}
		toRemove.Clear();
	}

	public static void UpdateTarget(float distance, bool self)
	{
		ScanTarget scanTarget = default(ScanTarget);
		scanTarget.Invalidate();
		GameObject result;
		if (self)
		{
			result = ((Player.main != null) ? Player.main.gameObject : null);
		}
		else
		{
			Targeting.AddToIgnoreList(Player.main.gameObject);
			Targeting.GetTarget(distance, out result, out var _);
		}
		scanTarget.Initialize(result);
		if (PDAScanner.scanTarget.techType != scanTarget.techType || PDAScanner.scanTarget.gameObject != scanTarget.gameObject || PDAScanner.scanTarget.uid != scanTarget.uid)
		{
			if (PDAScanner.scanTarget.isPlayer && PDAScanner.scanTarget.hasUID && cachedProgress.ContainsKey(PDAScanner.scanTarget.uid))
			{
				cachedProgress[PDAScanner.scanTarget.uid] = 0f;
			}
			if (scanTarget.hasUID && cachedProgress.TryGetValue(scanTarget.uid, out var value))
			{
				scanTarget.progress = value;
			}
			PDAScanner.scanTarget = scanTarget;
		}
	}

	public static Result CanScan(ScanTarget scanTarget)
	{
		TechType techType = scanTarget.techType;
		if (techType == TechType.None)
		{
			return Result.None;
		}
		if (scanTarget.isPlayer)
		{
			return Result.Scan;
		}
		Constructable constructable = ((scanTarget.gameObject != null) ? scanTarget.gameObject.GetComponent<Constructable>() : null);
		if ((bool)constructable && !constructable.constructed)
		{
			return Result.None;
		}
		InfectedMixin infectedMixin = ((scanTarget.gameObject != null) ? scanTarget.gameObject.GetComponent<InfectedMixin>() : null);
		bool num = infectedMixin != null && infectedMixin.IsInfected();
		bool flag = infectedMixin != null && infectedMixin.IsHealedByPeeper();
		if (num && !PDAEncyclopedia.ContainsEntry("Infection"))
		{
			return Result.Scan;
		}
		if (flag && !PDAEncyclopedia.ContainsEntry("CuredCreature"))
		{
			return Result.Scan;
		}
		if (techType == TechType.Peeper)
		{
			Peeper peeper = ((scanTarget.gameObject != null) ? scanTarget.gameObject.GetComponent<Peeper>() : null);
			if (peeper != null && peeper.isHero && !PDAEncyclopedia.ContainsEntry("HeroPeeper"))
			{
				return Result.Scan;
			}
			if (peeper != null && peeper.isInPrisonAquarium && !PDAEncyclopedia.ContainsEntry("AquariumPeeper"))
			{
				return Result.Scan;
			}
		}
		EntryData entryData = GetEntryData(techType);
		if (entryData != null)
		{
			if (complete.Contains(techType))
			{
				if (entryData.destroyAfterScan)
				{
					return Result.Scan;
				}
				if (!string.IsNullOrEmpty(entryData.encyclopedia) && !PDAEncyclopedia.ContainsEntry(entryData.encyclopedia))
				{
					return Result.Scan;
				}
				return Result.Known;
			}
			if (scanTarget.hasUID && fragments.ContainsKey(scanTarget.uid))
			{
				return Result.Processed;
			}
			return Result.Scan;
		}
		return Result.None;
	}

	public static bool CanScan(GameObject go)
	{
		UniqueIdentifier component = go.GetComponent<UniqueIdentifier>();
		if (component != null)
		{
			TechType techType = CraftData.GetTechType(go);
			string id = component.Id;
			if (!fragments.ContainsKey(id) && !complete.Contains(techType))
			{
				return true;
			}
		}
		return false;
	}

	public static Result Scan()
	{
		TechType techType = scanTarget.techType;
		GameObject gameObject = scanTarget.gameObject;
		InfectedMixin infectedMixin = ((gameObject != null) ? gameObject.GetComponent<InfectedMixin>() : null);
		bool flag = infectedMixin != null;
		bool flag2 = flag && infectedMixin.IsInfected();
		bool flag3 = techType == TechType.Peeper;
		string uid = scanTarget.uid;
		bool hasUID = scanTarget.hasUID;
		Result result = CanScan(scanTarget);
		if (result != Result.Scan)
		{
			return result;
		}
		float num = 2f;
		bool flag4 = complete.Contains(techType);
		bool flag5 = false;
		bool flag6 = false;
		bool flag7 = false;
		EntryData entryData = GetEntryData(techType);
		if (entryData != null)
		{
			num = entryData.scanTime;
		}
		if (flag4)
		{
			num = 2f;
		}
		if (scanTarget.isPlayer)
		{
			num = 1.7f;
		}
		if (NoCostConsoleCommand.main.fastScanCheat)
		{
			num = 1f;
		}
		scanTarget.progress += Time.deltaTime / num;
		if (scanTarget.progress >= 1f)
		{
			scanTarget.progress = 0f;
			if (hasUID && !fragments.ContainsKey(uid))
			{
				fragments.Add(uid, 1f);
			}
			if (entryData != null)
			{
				flag5 = entryData.destroyAfterScan;
				flag7 = true;
				if (flag4)
				{
					if (flag5)
					{
						ErrorMessage.AddError(Language.main.Get("ScannerRedundantScanned"));
						CraftData.AddToInventory(TechType.Titanium, 2);
						result = Result.Done;
					}
					else
					{
						result = Result.Researched;
					}
				}
				else
				{
					if (!GetPartialEntryByKey(techType, out var entry))
					{
						entry = Add(techType, 0);
					}
					if (entry != null)
					{
						entry.unlocked++;
						if (entry.unlocked >= entryData.totalFragments)
						{
							result = Result.Researched;
							flag6 = true;
							partial.Remove(entry);
							complete.Add(entry.techType);
							NotifyRemove(entry);
						}
						else
						{
							result = Result.Done;
							int totalFragments = entryData.totalFragments;
							if (totalFragments > 1)
							{
								float arg = Mathf.RoundToInt((float)entry.unlocked / (float)totalFragments * 100f);
								ErrorMessage.AddError(Language.main.GetFormat("ScannerInstanceScanned", Language.main.Get(techType.AsString()), arg, entry.unlocked, totalFragments));
							}
							NotifyProgress(entry);
						}
					}
				}
			}
			if (gameObject != null)
			{
				gameObject.SendMessage("OnScanned", entryData, SendMessageOptions.DontRequireReceiver);
			}
			if (flag5 && !scanTarget.isPlayer && gameObject != null)
			{
				UnityEngine.Object.Destroy(gameObject);
				scanTarget.Invalidate();
				if (hasUID)
				{
					fragments.Remove(uid);
				}
			}
			if (flag6 || flag7)
			{
				Unlock(entryData, flag6, flag7);
			}
			if (flag)
			{
				result = (flag2 ? Result.Infected : Result.NotInfected);
				if (scanTarget.isPlayer)
				{
					float infectedAmount = infectedMixin.GetInfectedAmount();
					StoryGoal storyGoal = new StoryGoal();
					storyGoal.goalType = Story.GoalType.PDA;
					storyGoal.key = "SelfScan5";
					if (infectedAmount > 0.8f)
					{
						storyGoal.key = "SelfScan4";
					}
					else if (infectedAmount > 0.6f)
					{
						storyGoal.key = "SelfScan3b";
					}
					else if (infectedAmount > 0.4f)
					{
						storyGoal.key = "SelfScan3";
					}
					else if (infectedAmount > 0.2f)
					{
						storyGoal.key = "SelfScan2";
					}
					else if (infectedAmount > 0f)
					{
						storyGoal.key = "SelfScan1";
						result = Result.NotInfected;
					}
					storyGoal.Trigger();
				}
				else if (flag2)
				{
					PDAEncyclopedia.Add("Infection", verbose: true);
					if (infectedMixin.IsHealedByPeeper())
					{
						PDAEncyclopedia.Add("CuredCreature", verbose: true);
					}
				}
			}
			if (flag3)
			{
				Peeper peeper = ((gameObject != null) ? gameObject.GetComponent<Peeper>() : null);
				if (peeper != null && peeper.isHero)
				{
					PDAEncyclopedia.Add("HeroPeeper", verbose: true);
				}
				if (peeper != null && peeper.isInPrisonAquarium)
				{
					PDAEncyclopedia.Add("AquariumPeeper", verbose: true);
				}
			}
		}
		if (scanTarget.hasUID)
		{
			cachedProgress[scanTarget.uid] = scanTarget.progress;
		}
		return result;
	}

	private static void Unlock(EntryData entryData, bool unlockBlueprint, bool unlockEncyclopedia, bool verbose = true)
	{
		if (entryData != null)
		{
			if (unlockBlueprint)
			{
				KnownTech.Add(entryData.blueprint, verbose);
			}
			if (unlockEncyclopedia)
			{
				PDAEncyclopedia.Add(entryData.encyclopedia, verbose);
			}
		}
	}

	private static void NotifyAdd(Entry entry)
	{
		if (onAdd != null)
		{
			onAdd(entry);
		}
	}

	private static void NotifyRemove(Entry entry)
	{
		if (onRemove != null)
		{
			onRemove(entry);
		}
	}

	private static void NotifyProgress(Entry entry)
	{
		if (onProgress != null)
		{
			onProgress(entry);
		}
	}

	public static Data Serialize()
	{
		return new Data
		{
			version = 2,
			fragments = fragments,
			partial = partial,
			complete = complete
		};
	}

	public static void Deserialize(Data serialData)
	{
		if (!initialized)
		{
			Debug.LogException(new InvalidOperationException("PDAScanner : Deserialize() : Attempt to deserialize uninitialized PDAScanner!"));
		}
		else
		{
			if (serialData == null)
			{
				return;
			}
			if (serialData.complete != null)
			{
				HashSet<TechType>.Enumerator enumerator = serialData.complete.GetEnumerator();
				while (enumerator.MoveNext())
				{
					TechType current = enumerator.Current;
					complete.Add(current);
				}
			}
			if (serialData.version < 2)
			{
				if (serialData.partial != null)
				{
					int i = 0;
					for (int count = serialData.partial.Count; i < count; i++)
					{
						Entry entry = serialData.partial[i];
						if (entry.progress >= 0.99f)
						{
							complete.Add(entry.techType);
							continue;
						}
						EntryData entryData = GetEntryData(entry.techType);
						if (entryData != null)
						{
							int unlocked = Mathf.FloorToInt(entry.progress * (float)entryData.totalFragments + 0.001f);
							Add(entry.techType, unlocked);
						}
					}
				}
			}
			else if (serialData.partial != null)
			{
				int j = 0;
				for (int count2 = serialData.partial.Count; j < count2; j++)
				{
					Entry entry2 = serialData.partial[j];
					Add(entry2.techType, entry2.unlocked);
				}
			}
			if (serialData.version < 2)
			{
				if (serialData.fragments != null)
				{
					Dictionary<string, float>.Enumerator enumerator2 = serialData.fragments.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						KeyValuePair<string, float> current2 = enumerator2.Current;
						if (current2.Value >= 1f)
						{
							fragments.Add(current2.Key, 1f);
						}
					}
				}
			}
			else if (serialData.fragments != null)
			{
				Dictionary<string, float>.Enumerator enumerator3 = serialData.fragments.GetEnumerator();
				while (enumerator3.MoveNext())
				{
					KeyValuePair<string, float> current3 = enumerator3.Current;
					fragments.Add(current3.Key, 1f);
				}
			}
			List<TechType> list = new List<TechType>(complete);
			int k = 0;
			for (int count3 = list.Count; k < count3; k++)
			{
				TechType techType = list[k];
				EntryData entryData2 = GetEntryData(techType);
				if (entryData2 != null)
				{
					Unlock(entryData2, unlockBlueprint: true, unlockEncyclopedia: true, verbose: false);
					continue;
				}
				entryData2 = new EntryData();
				entryData2.key = techType;
				entryData2.locked = false;
				entryData2.totalFragments = 1;
				entryData2.destroyAfterScan = false;
				entryData2.encyclopedia = "";
				entryData2.blueprint = techType;
				entryData2.scanTime = 1f;
				Unlock(entryData2, unlockBlueprint: true, unlockEncyclopedia: false, verbose: false);
			}
		}
	}

	public static string GetDebugString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("ScanTarget:\n");
		stringBuilder.AppendFormat("techType {0}\n", scanTarget.techType);
		stringBuilder.AppendFormat("uid {0}\n", scanTarget.uid);
		stringBuilder.AppendFormat("progress {0}\n", scanTarget.progress);
		stringBuilder.AppendFormat("infected {0}\n", scanTarget.gameObject != null && scanTarget.gameObject.GetComponent<InfectedMixin>() != null && scanTarget.gameObject.GetComponent<InfectedMixin>().IsInfected());
		stringBuilder.Append("\nCached Progress (non-serialized):\n");
		foreach (KeyValuePair<string, float> item in cachedProgress)
		{
			stringBuilder.AppendFormat("{0} - {1}\n", item.Key, item.Value);
		}
		stringBuilder.Append("\nScanned:\n");
		foreach (KeyValuePair<string, float> fragment in fragments)
		{
			stringBuilder.AppendFormat("{0} - {1}\n", fragment.Key, fragment.Value);
		}
		stringBuilder.Append("\nPartial:\n");
		foreach (Entry item2 in partial)
		{
			stringBuilder.AppendFormat("{0} - {1}\n", item2.techType, item2.unlocked);
		}
		stringBuilder.Append("\nComplete:\n");
		foreach (TechType item3 in complete)
		{
			stringBuilder.AppendLine(item3.AsString());
		}
		stringBuilder.Append("\nKnown:\n");
		int num = 0;
		foreach (TechType item4 in KnownTech.GetTech())
		{
			stringBuilder.AppendFormat("{0}, ", item4.AsString());
			if (num > 5)
			{
				num = 0;
				stringBuilder.AppendLine();
			}
			num++;
		}
		return stringBuilder.ToString();
	}
}
