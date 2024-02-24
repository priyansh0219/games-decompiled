using System;
using System.Collections.Generic;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class PrefabPlaceholdersGroup : MonoBehaviour, IProtoTreeEventListener, ICompileTimeCheckable, ICompileTimeSetupable
{
	[AssertNotNull]
	public PrefabPlaceholder[] prefabPlaceholders;

	public Action OnPrefabGroupSpawned;

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version;

	[NonSerialized]
	[ProtoMember(2)]
	public bool isInitialized;

	private int _remainingSpawns;

	private int remainingSpawns
	{
		get
		{
			return _remainingSpawns;
		}
		set
		{
			_remainingSpawns = value;
			OnRemainingSpawnsChanged();
		}
	}

	private void Start()
	{
		if (!isInitialized)
		{
			Spawn();
			isInitialized = true;
		}
		else
		{
			OnPrefabGroupSpawned?.Invoke();
		}
	}

	public void Spawn()
	{
		remainingSpawns = prefabPlaceholders.Length;
		for (int i = 0; i < prefabPlaceholders.Length; i++)
		{
			PrefabPlaceholder prefabPlaceholder = prefabPlaceholders[i];
			if ((bool)prefabPlaceholder)
			{
				if (OnPrefabGroupSpawned != null)
				{
					prefabPlaceholder.OnPlaceholderSpawn = OnPlaceholderSpawn;
				}
				prefabPlaceholder.Spawn();
			}
			else
			{
				int num = remainingSpawns - 1;
				remainingSpawns = num;
			}
		}
	}

	private void OnPlaceholderSpawn()
	{
		int num = remainingSpawns - 1;
		remainingSpawns = num;
	}

	private void OnRemainingSpawnsChanged()
	{
		if (OnPrefabGroupSpawned != null && remainingSpawns == 0)
		{
			OnPrefabGroupSpawned();
		}
	}

	public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
	{
		version = 1;
	}

	public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
	{
		if (version >= 1)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		PrefabIdentifier[] componentsInChildren = GetComponentsInChildren<PrefabIdentifier>(includeInactive: true);
		for (int i = 0; i < prefabPlaceholders.Length; i++)
		{
			PrefabPlaceholder prefabPlaceholder = prefabPlaceholders[i];
			if (PrefabDatabase.TryGetPrefabFilename(prefabPlaceholder.prefabClassId, out var filename) && filename.IndexOf("Slots", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				continue;
			}
			bool flag = false;
			foreach (PrefabIdentifier prefabIdentifier in componentsInChildren)
			{
				if (string.Equals(prefabPlaceholder.prefabClassId, prefabIdentifier.ClassId) && prefabPlaceholder.transform.parent == prefabIdentifier.transform.parent)
				{
					flag = true;
					break;
				}
			}
			num += (flag ? 1 : 0);
			num2 += ((!flag) ? 1 : 0);
		}
		isInitialized = num > num2;
	}

	public string CompileTimeCheck()
	{
		if (!GetComponent<PrefabIdentifier>())
		{
			return "PrefabPlaceholdersGroup must be placed on the prefab's root";
		}
		if (GetComponentsInChildren<PrefabPlaceholdersGroup>(includeInactive: true).Length != 1)
		{
			return "Only one PrefabPlaceholdersGroup allowed per prefab";
		}
		PrefabPlaceholder[] componentsInChildren = GetComponentsInChildren<PrefabPlaceholder>(includeInactive: true);
		PrefabPlaceholder[] array = componentsInChildren;
		foreach (PrefabPlaceholder prefabPlaceholder in array)
		{
			if (Array.IndexOf(prefabPlaceholders, prefabPlaceholder) < 0)
			{
				return $"Unreferenced prefab placeholder '{prefabPlaceholder.name}' found";
			}
		}
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			PrefabPlaceholder prefabPlaceholder2 = componentsInChildren[j];
			for (int k = j + 1; k < componentsInChildren.Length; k++)
			{
				PrefabPlaceholder prefabPlaceholder3 = componentsInChildren[k];
				if (string.Equals(prefabPlaceholder2.prefabClassId, prefabPlaceholder3.prefabClassId, StringComparison.OrdinalIgnoreCase))
				{
					float num = Quaternion.Angle(prefabPlaceholder2.transform.rotation, prefabPlaceholder3.transform.rotation);
					if ((prefabPlaceholder2.transform.position - prefabPlaceholder3.transform.position).sqrMagnitude < 0.001f && num < 1f)
					{
						return $"Potentially duplicate prefab placeholders {prefabPlaceholder2.name} and {prefabPlaceholder3.name} found.";
					}
				}
			}
		}
		return null;
	}

	public string CompileTimeSetup()
	{
		PrefabPlaceholder[] componentsInChildren = GetComponentsInChildren<PrefabPlaceholder>(includeInactive: true);
		HashSet<int> hashSet = new HashSet<int>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			PrefabPlaceholder prefabPlaceholder = componentsInChildren[i];
			for (int j = i + 1; j < componentsInChildren.Length; j++)
			{
				PrefabPlaceholder prefabPlaceholder2 = componentsInChildren[j];
				if (string.Equals(prefabPlaceholder.prefabClassId, prefabPlaceholder2.prefabClassId, StringComparison.OrdinalIgnoreCase))
				{
					float num = Quaternion.Angle(prefabPlaceholder.transform.rotation, prefabPlaceholder2.transform.rotation);
					if ((prefabPlaceholder.transform.position - prefabPlaceholder2.transform.position).sqrMagnitude < 0.001f && num < 1f)
					{
						hashSet.Add(j);
					}
				}
			}
		}
		foreach (int item in hashSet)
		{
			Debug.LogFormat(base.gameObject, "Destroying placeholder {0} '{1}' at {2}", item, componentsInChildren[item].name, componentsInChildren[item].gameObject.GetFullHierarchyPath());
			UnityEngine.Object.DestroyImmediate(componentsInChildren[item].gameObject, allowDestroyingAssets: true);
		}
		prefabPlaceholders = GetComponentsInChildren<PrefabPlaceholder>(includeInactive: true);
		return null;
	}
}
