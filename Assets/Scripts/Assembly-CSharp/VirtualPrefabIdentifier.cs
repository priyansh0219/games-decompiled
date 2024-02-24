using System;
using System.Collections;
using UWE;
using UnityEngine;

[AddComponentMenu("")]
public class VirtualPrefabIdentifier : UniqueIdentifier
{
	[NonSerialized]
	public bool highPriority;

	public Action OnInstantiate;

	private Coroutine spawnCoroutine;

	private void Start()
	{
		spawnCoroutine = CoroutineHost.StartCoroutine(SpawnPrefab());
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (spawnCoroutine != null)
		{
			CoroutineHost.StopCoroutine(spawnCoroutine);
			spawnCoroutine = null;
		}
	}

	private IEnumerator SpawnPrefab()
	{
		if (!PrefabDatabase.TryGetPrefabFilename(base.ClassId, out var filename))
		{
			Debug.LogErrorFormat(this, "Failed to request prefab for '{0}'", base.ClassId);
			UnityEngine.Object.Destroy(base.gameObject);
			yield break;
		}
		Transform parent = base.transform.parent;
		DeferredSpawner.Task task = DeferredSpawner.instance.InstantiateAsync(filename, this, parent, base.transform.localPosition, base.transform.localRotation, instantiateActivated: false, highPriority);
		yield return task;
		GameObject result = task.GetResult();
		if (result != null)
		{
			result.transform.localScale = base.transform.localScale;
			result.SetActive(value: true);
			OnInstantiate?.Invoke();
		}
		UnityEngine.Object.Destroy(base.gameObject);
		spawnCoroutine = null;
	}

	public override bool ShouldSerialize(Component comp)
	{
		return comp is Transform;
	}

	public override bool ShouldCreateEmptyObject()
	{
		return false;
	}

	public override bool ShouldMergeObject()
	{
		return false;
	}

	public override bool ShouldOverridePrefab()
	{
		return true;
	}

	public override bool ShouldStoreClassId()
	{
		return true;
	}
}
