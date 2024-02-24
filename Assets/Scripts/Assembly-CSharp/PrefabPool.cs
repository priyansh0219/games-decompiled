using System;
using System.Collections.Generic;
using UnityEngine;

public class PrefabPool<T> where T : Component
{
	private GameObject prefab;

	private Transform parent;

	private int chunkSize;

	private Action<T> create;

	private Action<T> release;

	private List<T> pool = new List<T>();

	public PrefabPool(GameObject prefab, Transform parent, int initialSize, int chunkSize, Action<T> create, Action<T> release)
	{
		this.prefab = prefab;
		this.parent = parent;
		this.chunkSize = Mathf.Max(1, chunkSize);
		this.create = create;
		this.release = release;
		if (initialSize > 0)
		{
			Allocate(initialSize);
		}
	}

	private void Allocate(int amount)
	{
		for (int i = 0; i < amount; i++)
		{
			T component = UnityEngine.Object.Instantiate(prefab, parent).GetComponent<T>();
			pool.Add(component);
			if (create != null)
			{
				create(component);
			}
		}
	}

	public T Get()
	{
		if (pool.Count == 0)
		{
			Allocate(chunkSize);
		}
		int index = pool.Count - 1;
		T result = pool[index];
		pool.RemoveAt(index);
		return result;
	}

	public void Release(T entry)
	{
		if (!(entry == null))
		{
			if (release != null)
			{
				release(entry);
			}
			pool.Add(entry);
		}
	}
}
