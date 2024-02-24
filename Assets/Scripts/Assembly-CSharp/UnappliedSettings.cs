using System;
using System.Collections.Generic;
using UnityEngine;

public class UnappliedSettings
{
	public enum Key
	{
		None = 0,
		Resolution = 1,
		VRRenderScale = 2,
		Language = 3,
		ControllerLayout = 4
	}

	private static Dictionary<Key, Action> toApply = new Dictionary<Key, Action>();

	private static Dictionary<Key, Action<uGUI_Dialog>> toRevert = new Dictionary<Key, Action<uGUI_Dialog>>();

	public static bool HasUnappliedSettings => toApply.Count > 0;

	public static void Add(Key key, Action applyAction)
	{
		toApply[key] = applyAction;
	}

	public static void Remove(Key key)
	{
		toApply.Remove(key);
	}

	public static void Revert(Key key, Action<uGUI_Dialog> revertAction)
	{
		toRevert[key] = revertAction;
	}

	public static void Update(uGUI_Dialog dialog)
	{
		if (toRevert.Count <= 0 || dialog.open)
		{
			return;
		}
		using (Dictionary<Key, Action<uGUI_Dialog>>.Enumerator enumerator = toRevert.GetEnumerator())
		{
			enumerator.MoveNext();
			KeyValuePair<Key, Action<uGUI_Dialog>> current = enumerator.Current;
			toRevert.Remove(current.Key);
			Action<uGUI_Dialog> value = current.Value;
			try
			{
				value(dialog);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public static void Apply()
	{
		using (ListPool<Action> listPool = Pool<ListPool<Action>>.Get())
		{
			List<Action> list = listPool.list;
			foreach (KeyValuePair<Key, Action> item in toApply)
			{
				list.Add(item.Value);
			}
			toApply.Clear();
			for (int i = 0; i < list.Count; i++)
			{
				Action action = list[i];
				try
				{
					action();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
	}

	public static void Clear()
	{
		toApply.Clear();
		toRevert.Clear();
	}
}
