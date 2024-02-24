using System;
using System.Collections.Generic;
using UnityEngine;

public static class Radical
{
	[Obsolete("Use System.Array.IndexOf or LINQ instead")]
	public static int IndexOf<T>(this IEnumerable<T> items, T item)
	{
		return items.FindIndex((T i) => EqualityComparer<T>.Default.Equals(item, i));
	}

	[Obsolete("Use System.Array.IndexOf or LINQ instead")]
	public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		int num = 0;
		foreach (T item in items)
		{
			if (predicate(item))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public static GameObject FindChild(this GameObject parent, string name)
	{
		for (int i = 0; i < parent.transform.childCount; i++)
		{
			Transform child = parent.transform.GetChild(i);
			if (child.name == name)
			{
				return child.gameObject;
			}
		}
		return null;
	}

	public static Component EnsureComponent(this GameObject obj, Type type)
	{
		Component component = obj.GetComponent(type);
		if ((bool)component)
		{
			return component;
		}
		return obj.AddComponent(type);
	}

	public static T EnsureComponent<T>(this GameObject obj) where T : Component
	{
		T component = obj.GetComponent<T>();
		if ((bool)component)
		{
			return component;
		}
		return obj.AddComponent<T>();
	}
}
