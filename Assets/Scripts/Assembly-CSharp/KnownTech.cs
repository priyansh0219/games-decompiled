using System;
using System.Collections.Generic;
using Story;
using UnityEngine;

public static class KnownTech
{
	[Serializable]
	public class CompoundTech
	{
		public TechType techType;

		public List<TechType> dependencies;
	}

	[Serializable]
	public class AnalysisTech
	{
		public TechType techType;

		public string unlockMessage;

		public FMODAsset unlockSound;

		public Sprite unlockPopup;

		public List<TechType> unlockTechTypes;

		public List<StoryGoal> storyGoals;
	}

	public delegate void OnChanged();

	public delegate void OnAdd(TechType techType, bool verbose);

	public delegate void OnRemove(TechType techType);

	public delegate void OnCompoundAdd(TechType techType, int unlocked, int total);

	public delegate void OnCompoundRemove(TechType techType);

	public delegate void OnCompoundProgress(TechType techType, int unlocked, int total);

	public delegate void OnAnalyze(AnalysisTech analysis, bool verbose);

	private static bool initialized;

	private static HashSet<TechType> defaultTech;

	private static List<CompoundTech> compoundTech;

	private static List<AnalysisTech> analysisTech;

	private static HashSet<TechType> knownTech;

	private static HashSet<TechType> analyzedTech;

	private static Dictionary<TechType, int> knownCompoundTech;

	public static event OnChanged onChanged;

	public static event OnAdd onAdd;

	public static event OnRemove onRemove;

	public static event OnCompoundAdd onCompoundAdd;

	public static event OnCompoundRemove onCompoundRemove;

	public static event OnCompoundProgress onCompoundProgress;

	public static event OnAnalyze onAnalyze;

	public static void Initialize(PDAData data)
	{
		if (!initialized)
		{
			initialized = true;
			knownTech = new HashSet<TechType>(TechTypeExtensions.sTechTypeComparer);
			analyzedTech = new HashSet<TechType>(TechTypeExtensions.sTechTypeComparer);
			knownCompoundTech = new Dictionary<TechType, int>(TechTypeExtensions.sTechTypeComparer);
			compoundTech = ValidateCompoundTech(data.compoundTech);
			defaultTech = new HashSet<TechType>(data.defaultTech, TechTypeExtensions.sTechTypeComparer);
			analysisTech = ValidateAnalysisTech(data.analysisTech);
			AddRange(defaultTech, verbose: false);
		}
	}

	public static void Deinitialize()
	{
		defaultTech = null;
		compoundTech = null;
		analysisTech = null;
		knownTech = null;
		analyzedTech = null;
		knownCompoundTech = null;
		KnownTech.onChanged = null;
		KnownTech.onCompoundAdd = null;
		KnownTech.onCompoundRemove = null;
		KnownTech.onCompoundProgress = null;
		initialized = false;
	}

	public static IEnumerable<TechType> GetTech()
	{
		return knownTech;
	}

	public static bool Contains(TechType techType)
	{
		return knownTech.Contains(techType);
	}

	public static bool Add(TechType techType, bool verbose = true)
	{
		if (techType != 0 && knownTech.Add(techType))
		{
			NotifyAdd(techType, verbose);
			PDAScanner.CompleteAllEntriesWhichUnlocks(techType);
			NotifyChanged();
			UnlockCompoundTech(verbose);
			Analyze(techType, verbose);
			return true;
		}
		return false;
	}

	public static void AddRange(IEnumerable<TechType> newTech, bool verbose)
	{
		if (newTech == null)
		{
			return;
		}
		bool flag = false;
		IEnumerator<TechType> enumerator = newTech.GetEnumerator();
		while (enumerator.MoveNext())
		{
			TechType current = enumerator.Current;
			if (knownTech.Add(current))
			{
				flag = true;
				NotifyAdd(current, verbose);
				PDAScanner.CompleteAllEntriesWhichUnlocks(current);
				Analyze(current, verbose);
			}
		}
		if (flag)
		{
			NotifyChanged();
			UnlockCompoundTech(verbose);
		}
	}

	public static bool Remove(TechType techType)
	{
		if (knownTech.Remove(techType))
		{
			PDAScanner.RemoveCompleteWhichUnlocks(techType);
			NotifyRemove(techType);
			NotifyChanged();
			return true;
		}
		return false;
	}

	private static List<AnalysisTech> ValidateAnalysisTech(List<AnalysisTech> src)
	{
		List<AnalysisTech> list = new List<AnalysisTech>();
		HashSet<TechType> hashSet = new HashSet<TechType>();
		int i = 0;
		for (int count = src.Count; i < count; i++)
		{
			AnalysisTech analysisTech = src[i];
			TechType techType = analysisTech.techType;
			if (techType == TechType.None)
			{
				continue;
			}
			List<TechType> unlockTechTypes = analysisTech.unlockTechTypes;
			int j = 0;
			for (int count2 = unlockTechTypes.Count; j < count2; j++)
			{
				TechType techType2 = unlockTechTypes[j];
				if (techType2 != 0)
				{
					hashSet.Add(techType2);
				}
			}
			AnalysisTech analysisTech2 = new AnalysisTech();
			analysisTech2.techType = techType;
			analysisTech2.unlockMessage = analysisTech.unlockMessage;
			analysisTech2.unlockSound = analysisTech.unlockSound;
			analysisTech2.unlockPopup = analysisTech.unlockPopup;
			analysisTech2.unlockTechTypes = new List<TechType>(hashSet);
			analysisTech2.storyGoals = new List<StoryGoal>(analysisTech.storyGoals);
			list.Add(analysisTech2);
			hashSet.Clear();
		}
		return list;
	}

	private static List<CompoundTech> ValidateCompoundTech(List<CompoundTech> src)
	{
		List<CompoundTech> list = new List<CompoundTech>();
		HashSet<TechType> hashSet = new HashSet<TechType>();
		int i = 0;
		for (int count = src.Count; i < count; i++)
		{
			CompoundTech compoundTech = src[i];
			TechType techType = compoundTech.techType;
			if (techType != 0)
			{
				if (!hashSet.Contains(techType))
				{
					List<TechType> list2 = new List<TechType>();
					List<TechType> dependencies = compoundTech.dependencies;
					int j = 0;
					for (int count2 = dependencies.Count; j < count2; j++)
					{
						TechType techType2 = dependencies[j];
						if (techType2 != 0)
						{
							if (!list2.Contains(techType2))
							{
								list2.Add(techType2);
							}
							else
							{
								Debug.LogError($"ValidateCompoundTech() : Duplicate TechType {techType2} at index {j} found in the list of dependencies for TechType {techType}!");
							}
						}
						else
						{
							Debug.LogError($"ValidateCompoundTech() : TechType.None found at index {j} in the list of dependencies for TechType {techType}!");
						}
					}
					if (list2.Count > 0)
					{
						CompoundTech compoundTech2 = new CompoundTech();
						compoundTech2.techType = techType;
						compoundTech2.dependencies = list2;
						list.Add(compoundTech2);
						hashSet.Add(techType);
					}
					else
					{
						Debug.LogError($"ValidateCompoundTech() : Validated list of dependencies for TechType {techType} is empty!");
					}
				}
				else
				{
					Debug.LogError($"ValidateCompoundTech() : Duplicate high-level TechType.{techType} found in compound tech list at index {i}.");
				}
			}
			else
			{
				Debug.LogError($"ValidateCompoundTech() : TechType.None in compound tech list found at index {i}");
			}
		}
		return list;
	}

	private static void UnlockCompoundTech(bool verbose)
	{
		int i = 0;
		for (int count = KnownTech.compoundTech.Count; i < count; i++)
		{
			CompoundTech compoundTech = KnownTech.compoundTech[i];
			TechType techType = compoundTech.techType;
			if (knownTech.Contains(techType))
			{
				continue;
			}
			List<TechType> dependencies = compoundTech.dependencies;
			int count2 = dependencies.Count;
			int num = 0;
			for (int j = 0; j < count2; j++)
			{
				if (knownTech.Contains(dependencies[j]))
				{
					num++;
				}
			}
			if (num <= 0)
			{
				continue;
			}
			if (knownCompoundTech.TryGetValue(techType, out var value))
			{
				if (num != value)
				{
					if (num == count2)
					{
						knownCompoundTech.Remove(techType);
						NotifyCompoundRemove(techType);
					}
					else
					{
						knownCompoundTech[techType] = num;
						NotifyCompoundProgress(techType, num, count2);
					}
				}
			}
			else if (num < count2)
			{
				knownCompoundTech.Add(techType, num);
				NotifyCompoundAdd(techType, num, count2);
			}
			if (num == count2)
			{
				Add(techType, verbose);
			}
		}
	}

	public static void Analyze(TechType techType, bool verbose = true)
	{
		if (techType == TechType.None)
		{
			return;
		}
		int i = 0;
		for (int count = KnownTech.analysisTech.Count; i < count; i++)
		{
			AnalysisTech analysisTech = KnownTech.analysisTech[i];
			if (analysisTech.techType != techType || !analyzedTech.Add(techType))
			{
				continue;
			}
			NotifyAnalyze(analysisTech, verbose);
			List<TechType> unlockTechTypes = analysisTech.unlockTechTypes;
			int j = 0;
			for (int count2 = unlockTechTypes.Count; j < count2; j++)
			{
				Add(unlockTechTypes[j], verbose);
			}
			foreach (StoryGoal storyGoal in analysisTech.storyGoals)
			{
				storyGoal.Trigger();
			}
		}
	}

	public static List<CompoundTech>.Enumerator GetCompoundTech()
	{
		return compoundTech.GetEnumerator();
	}

	public static Dictionary<TechType, int>.Enumerator GetKnownCompoundTech()
	{
		return knownCompoundTech.GetEnumerator();
	}

	public static int GetCompoundDependenciesUnlocked(TechType techType)
	{
		if (knownCompoundTech.TryGetValue(techType, out var value))
		{
			return value;
		}
		return -1;
	}

	public static int GetCompoundDependenciesTotal(TechType techType)
	{
		int i = 0;
		for (int count = KnownTech.compoundTech.Count; i < count; i++)
		{
			CompoundTech compoundTech = KnownTech.compoundTech[i];
			if (compoundTech.techType == techType)
			{
				return compoundTech.dependencies.Count;
			}
		}
		return -1;
	}

	public static HashSet<TechType> GetAllUnlockables(bool filterAllowed = true)
	{
		HashSet<TechType> hashSet = new HashSet<TechType>();
		Dictionary<TechType, PDAScanner.EntryData>.Enumerator allEntriesData = PDAScanner.GetAllEntriesData();
		while (allEntriesData.MoveNext())
		{
			TechType blueprint = allEntriesData.Current.Value.blueprint;
			if (blueprint != 0)
			{
				hashSet.Add(blueprint);
			}
		}
		int i = 0;
		for (int count = analysisTech.Count; i < count; i++)
		{
			hashSet.Add(analysisTech[i].techType);
		}
		List<CompoundTech>.Enumerator enumerator = GetCompoundTech();
		while (enumerator.MoveNext())
		{
			hashSet.Add(enumerator.Current.techType);
		}
		if (!filterAllowed)
		{
			return hashSet;
		}
		return CraftData.FilterAllowed(hashSet);
	}

	public static void UnlockAll(bool verbose = true)
	{
		AddRange(GetAllUnlockables(), verbose);
	}

	private static void UnreadAdd(TechType techType)
	{
		string key = techType.EncodeKey();
		NotificationManager main = NotificationManager.main;
		main.Add(NotificationManager.Group.Blueprints, key, 3f);
		if (CraftTree.IsCraftable(techType))
		{
			main.Add(NotificationManager.Group.CraftTree, key);
		}
		if (TechData.GetBuildable(techType))
		{
			main.Add(NotificationManager.Group.Builder, key);
		}
	}

	private static void UnreadRemove(TechType techType)
	{
		string key = techType.EncodeKey();
		NotificationManager main = NotificationManager.main;
		main.Remove(NotificationManager.Group.Blueprints, key);
		main.Remove(NotificationManager.Group.Builder, key);
		main.Remove(NotificationManager.Group.CraftTree, key);
	}

	public static TechUnlockState GetTechUnlockState(TechType techType)
	{
		int unlocked;
		int total;
		return GetTechUnlockState(techType, out unlocked, out total);
	}

	public static TechUnlockState GetTechUnlockState(TechType techType, out int unlocked, out int total)
	{
		TechUnlockState techUnlockState = TechUnlockState.Hidden;
		unlocked = -1;
		total = -1;
		if (Contains(techType))
		{
			return TechUnlockState.Available;
		}
		List<PDAScanner.Entry> list = new List<PDAScanner.Entry>();
		PDAScanner.GetPartialEntriesWhichUnlocks(techType, list);
		for (int i = 0; i < list.Count; i++)
		{
			PDAScanner.Entry entry = list[i];
			PDAScanner.EntryData entryData = PDAScanner.GetEntryData(entry.techType);
			int num = 1;
			if (entryData != null)
			{
				num = entryData.totalFragments;
			}
			if (num > total)
			{
				total = num;
				techUnlockState = TechUnlockState.Locked;
				unlocked = entry.unlocked;
			}
		}
		if (techUnlockState != 0)
		{
			return techUnlockState;
		}
		total = GetCompoundDependenciesTotal(techType);
		if (total > 0)
		{
			unlocked = GetCompoundDependenciesUnlocked(techType);
			if (unlocked > 0)
			{
				techUnlockState = ((unlocked < total) ? TechUnlockState.Locked : TechUnlockState.Available);
			}
		}
		return techUnlockState;
	}

	private static void NotifyAdd(TechType techType, bool verbose)
	{
		if (verbose)
		{
			UnreadAdd(techType);
		}
		if (KnownTech.onAdd != null)
		{
			KnownTech.onAdd(techType, verbose);
		}
	}

	private static void NotifyRemove(TechType techType)
	{
		UnreadRemove(techType);
		if (KnownTech.onRemove != null)
		{
			KnownTech.onRemove(techType);
		}
	}

	private static void NotifyChanged()
	{
		if (KnownTech.onChanged != null)
		{
			KnownTech.onChanged();
		}
	}

	private static void NotifyCompoundAdd(TechType techType, int unlocked, int total)
	{
		if (KnownTech.onCompoundAdd != null)
		{
			KnownTech.onCompoundAdd(techType, unlocked, total);
		}
	}

	private static void NotifyCompoundRemove(TechType techType)
	{
		if (KnownTech.onCompoundRemove != null)
		{
			KnownTech.onCompoundRemove(techType);
		}
	}

	private static void NotifyCompoundProgress(TechType techType, int unlocked, int total)
	{
		if (KnownTech.onCompoundProgress != null)
		{
			KnownTech.onCompoundProgress(techType, unlocked, total);
		}
	}

	private static void NotifyAnalyze(AnalysisTech analysis, bool verbose)
	{
		if (KnownTech.onAnalyze != null)
		{
			KnownTech.onAnalyze(analysis, verbose);
		}
	}

	public static void Serialize(out List<TechType> known, out HashSet<TechType> analyzed)
	{
		known = new List<TechType>();
		HashSet<TechType>.Enumerator enumerator = knownTech.GetEnumerator();
		while (enumerator.MoveNext())
		{
			TechType current = enumerator.Current;
			if (!defaultTech.Contains(current))
			{
				known.Add(current);
			}
		}
		analyzed = new HashSet<TechType>(analyzedTech);
	}

	public static void Deserialize(List<TechType> known, HashSet<TechType> analyzed)
	{
		AddRange(known, verbose: false);
		if (analyzed != null)
		{
			HashSet<TechType>.Enumerator enumerator = analyzed.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Analyze(enumerator.Current, verbose: false);
			}
		}
	}
}
