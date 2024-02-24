using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class UniqueIdentifier : MonoBehaviour
{
	private static int totalEntitiesEnabled;

	private bool isEnsureOnDestroyCalled;

	private string id;

	[HideInInspector]
	[SerializeField]
	private string classId;

	private static readonly Dictionary<string, UniqueIdentifier> identifiers = new Dictionary<string, UniqueIdentifier>(32768);

	public static bool IsTestingPlayMode = false;

	public string Id
	{
		get
		{
			if (string.IsNullOrEmpty(id))
			{
				Id = null;
			}
			return id;
		}
		set
		{
			Unregister();
			id = EnsureGuid(value);
			Register();
		}
	}

	public string ClassId
	{
		get
		{
			if (string.IsNullOrEmpty(classId))
			{
				ClassId = null;
			}
			return classId;
		}
		set
		{
			classId = EnsureGuid(value);
		}
	}

	public static IEnumerable<UniqueIdentifier> AllIdentifiers => identifiers.Values;

	private void Awake()
	{
		Register();
		totalEntitiesEnabled++;
	}

	public virtual void OnDestroy()
	{
		TryToCallEnsureOnDestroy();
	}

	public void BeforeDestroy()
	{
		TryToCallEnsureOnDestroy();
	}

	private void TryToCallEnsureOnDestroy()
	{
		if (!isEnsureOnDestroyCalled)
		{
			isEnsureOnDestroyCalled = true;
			EnsureOnDestroy();
		}
	}

	protected virtual void EnsureOnDestroy()
	{
		Unregister();
	}

	private void Register()
	{
		string text = id;
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		if (identifiers.TryGetValue(text, out var value))
		{
			if (!(value == this))
			{
				if ((bool)value)
				{
					Debug.LogErrorFormat(this, "Overwriting id '{0}' (old class '{1}', new class '{2}'), used to be '{3}' at {4} now '{5}' at {6}", text, value.classId, classId, value.name, value.transform.position, base.name, base.transform.position);
					identifiers[text] = this;
				}
				else
				{
					identifiers[text] = this;
				}
			}
		}
		else
		{
			identifiers.Add(text, this);
		}
	}

	private void Unregister()
	{
		string text = id;
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		if (identifiers.TryGetValue(text, out var value))
		{
			if (value == this)
			{
				identifiers.Remove(text);
			}
			else if ((bool)value)
			{
				Debug.LogErrorFormat("Unregistering id '{0}' (class '{1}', registered class '{2}') failed because it already changed to '{3}' at {4}, used to be '{5}' at {6}", text, classId, value.classId, value.name, value.transform.position, base.name, base.transform.position);
			}
			else
			{
				identifiers.Remove(text);
			}
		}
		else
		{
			Debug.LogErrorFormat(this, "Unregistering unique identifier '{0}' (class '{1}', name '{2}', at {3}) failed because it is not registered.", text, classId, base.name, base.transform.position);
		}
	}

	public abstract bool ShouldSerialize(Component comp);

	public abstract bool ShouldCreateEmptyObject();

	public abstract bool ShouldMergeObject();

	public abstract bool ShouldOverridePrefab();

	public abstract bool ShouldStoreClassId();

	public static string EnsureGuid(string guid)
	{
		if (string.IsNullOrEmpty(guid))
		{
			return Guid.NewGuid().ToString();
		}
		return guid;
	}

	public static bool TryGetIdentifier(string id, out UniqueIdentifier uid)
	{
		if (string.IsNullOrEmpty(id))
		{
			uid = null;
			return false;
		}
		return identifiers.TryGetValue(id, out uid);
	}

	public static IDictionary<string, UniqueIdentifier> DebugIdentifiers()
	{
		return identifiers;
	}
}
