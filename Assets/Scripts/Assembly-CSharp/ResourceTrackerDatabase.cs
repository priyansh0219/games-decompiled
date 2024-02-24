using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class ResourceTrackerDatabase : MonoBehaviour, IProtoEventListener, ICompileTimeCheckable, ILocalizationCheckable
{
	[ProtoContract]
	public class ResourceInfo
	{
		[ProtoMember(1)]
		public string uniqueId;

		[ProtoMember(2)]
		public TechType techType;

		[ProtoMember(3)]
		public Vector3 position;
	}

	public delegate void OnResourceDiscovered(ResourceInfo info);

	public delegate void OnResourceRemoved(ResourceInfo info);

	[Tooltip("Global settings and data required by the ResourceTracker system.")]
	[SerializeField]
	[AssertNotNull]
	public ResourceTrackerData globalData;

	private const int currentVersion = 2;

	[NonSerialized]
	[ProtoMember(1)]
	public int serializedVersion = 2;

	[NonSerialized]
	[ProtoMember(2)]
	public readonly List<ResourceInfo> savedResources = new List<ResourceInfo>();

	[NonSerialized]
	[ProtoMember(3)]
	public int storedChangeSet;

	private static readonly Dictionary<string, ResourceInfo>.ValueCollection emptyCollection = new Dictionary<string, ResourceInfo>().Values;

	private static readonly Dictionary<TechType, Dictionary<string, ResourceInfo>> resources = new Dictionary<TechType, Dictionary<string, ResourceInfo>>(TechTypeExtensions.sTechTypeComparer);

	private static readonly HashSet<TechType> detectableTechTypes = new HashSet<TechType>(TechTypeExtensions.sTechTypeComparer);

	private static readonly HashSet<TechType> undetectableTechTypes = new HashSet<TechType>(TechTypeExtensions.sTechTypeComparer);

	private static readonly Dictionary<TechType, string> scannedTechsTooltips = new Dictionary<TechType, string>();

	public static event OnResourceDiscovered onResourceDiscovered;

	public static event OnResourceRemoved onResourceRemoved;

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		savedResources.Clear();
		foreach (KeyValuePair<TechType, Dictionary<string, ResourceInfo>> resource in resources)
		{
			savedResources.AddRange(resource.Value.Values);
		}
		storedChangeSet = SNUtils.GetPlasticChangeSetOfBuild(0);
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		if (!BatchUpgrade.NeedsUpgrade(storedChangeSet))
		{
			int i = 0;
			for (int count = savedResources.Count; i < count; i++)
			{
				ResourceInfo resourceInfo = savedResources[i];
				if (!string.IsNullOrEmpty(resourceInfo.uniqueId))
				{
					resources.GetOrAddNew(resourceInfo.techType)[resourceInfo.uniqueId] = resourceInfo;
					if (!undetectableTechTypes.Contains(resourceInfo.techType))
					{
						detectableTechTypes.Add(resourceInfo.techType);
					}
				}
			}
		}
		else
		{
			Debug.Log("batches upgraded, wipe resource tracker database storedChangeSet: " + storedChangeSet);
		}
		savedResources.Clear();
	}

	private void Start()
	{
		detectableTechTypes.Clear();
		resources.Clear();
		undetectableTechTypes.Clear();
		undetectableTechTypes.UnionWith(globalData.undetectableTechTypes);
		InitializeTooltips();
	}

	private void InitializeTooltips()
	{
		scannedTechsTooltips.Clear();
		ResourceTrackerData.TechTooltip[] mineralDetectorScannedTooltips = globalData.mineralDetectorScannedTooltips;
		foreach (ResourceTrackerData.TechTooltip techTooltip in mineralDetectorScannedTooltips)
		{
			scannedTechsTooltips.Add(techTooltip.techType, techTooltip.text);
		}
	}

	public static void Register(string uniqueId, Vector3 resourcePosition, TechType resourceTechType)
	{
		if (resourceTechType == TechType.None || string.IsNullOrEmpty(uniqueId))
		{
			return;
		}
		Dictionary<string, ResourceInfo> orAddNew = resources.GetOrAddNew(resourceTechType);
		if (!orAddNew.TryGetValue(uniqueId, out var value))
		{
			value = new ResourceInfo
			{
				uniqueId = uniqueId,
				techType = resourceTechType,
				position = resourcePosition
			};
			orAddNew.Add(uniqueId, value);
			if (!undetectableTechTypes.Contains(resourceTechType))
			{
				detectableTechTypes.Add(resourceTechType);
			}
			if (ResourceTrackerDatabase.onResourceDiscovered != null)
			{
				ResourceTrackerDatabase.onResourceDiscovered(value);
			}
		}
		else
		{
			value.position = resourcePosition;
		}
	}

	public static void Unregister(string uniqueId, TechType techType)
	{
		if (techType == TechType.None || string.IsNullOrEmpty(uniqueId) || !resources.TryGetValue(techType, out var value))
		{
			return;
		}
		if (value.TryGetValue(uniqueId, out var value2))
		{
			value.Remove(uniqueId);
			if (ResourceTrackerDatabase.onResourceRemoved != null)
			{
				ResourceTrackerDatabase.onResourceRemoved(value2);
			}
		}
	}

	public static ICollection<ResourceInfo> GetNodes(TechType techType)
	{
		if (resources.TryGetValue(techType, out var value))
		{
			return value.Values;
		}
		return emptyCollection;
	}

	public static void GetNodes(Vector3 fromPosition, float distance, TechType techType, ICollection<ResourceInfo> outNodes)
	{
		float num = distance * distance;
		if (!resources.TryGetValue(techType, out var value))
		{
			return;
		}
		Dictionary<string, ResourceInfo>.Enumerator enumerator = value.GetEnumerator();
		while (enumerator.MoveNext())
		{
			ResourceInfo value2 = enumerator.Current.Value;
			if ((fromPosition - value2.position).sqrMagnitude <= num)
			{
				outNodes.Add(value2);
			}
		}
	}

	public static ICollection<TechType> GetTechTypes()
	{
		return resources.Keys;
	}

	public static void GetTechTypesInRange(Vector3 fromPosition, float distance, ICollection<TechType> outTechTypes)
	{
		Dictionary<TechType, Dictionary<string, ResourceInfo>>.Enumerator enumerator = resources.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<TechType, Dictionary<string, ResourceInfo>> current = enumerator.Current;
			if (HasNodeNearby(fromPosition, distance, current.Value))
			{
				outTechTypes.Add(current.Key);
			}
		}
	}

	public static bool HasTechTypeNearby(Vector3 fromPosition, float distance, TechType techType)
	{
		if (!resources.TryGetValue(techType, out var value))
		{
			return false;
		}
		float num = distance * distance;
		Dictionary<string, ResourceInfo>.Enumerator enumerator = value.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if ((fromPosition - enumerator.Current.Value.position).sqrMagnitude <= num)
			{
				return true;
			}
		}
		return false;
	}

	public static ICollection<TechType> GetDetectableTechTypes()
	{
		return detectableTechTypes;
	}

	public static bool IsDetectableTechType(TechType techType)
	{
		return detectableTechTypes.Contains(techType);
	}

	public static string GetTooltip(TechType techType)
	{
		string key = string.Empty;
		if (PDAScanner.ContainsCompleteEntry(techType))
		{
			key = scannedTechsTooltips.GetOrDefault(techType, string.Empty);
		}
		return Language.main.Get(key);
	}

	private static bool HasNodeNearby(Vector3 fromPosition, float distance, Dictionary<string, ResourceInfo> nodes)
	{
		float num = distance * distance;
		Dictionary<string, ResourceInfo>.Enumerator enumerator = nodes.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if ((fromPosition - enumerator.Current.Value.position).sqrMagnitude <= num)
			{
				return true;
			}
		}
		return false;
	}

	public string CompileTimeCheck()
	{
		if ((bool)globalData)
		{
			return globalData.CompileTimeCheck();
		}
		return null;
	}

	public string CompileTimeCheck(ILanguage language)
	{
		if ((bool)globalData)
		{
			return globalData.CompileTimeCheck(language);
		}
		return null;
	}
}
