using System;
using System.Collections.Generic;
using Gendarme;
using UnityEngine;

public static class PingManager
{
	public class PingTypeComparer : IEqualityComparer<PingType>
	{
		public bool Equals(PingType x, PingType y)
		{
			int num = (int)x;
			return num.Equals((int)y);
		}

		public int GetHashCode(PingType obj)
		{
			return (int)obj;
		}
	}

	public delegate void OnAdd(PingInstance instance);

	public delegate void OnRemove(string id);

	public delegate void OnRename(PingInstance instance);

	public delegate void OnIconChange(PingInstance instance);

	public delegate void OnColor(string id, Color color);

	public delegate void OnVisible(string id, bool visible);

	public delegate void OnVisit(PingInstance instance, float value);

	public static readonly PingTypeComparer sPingTypeComparer = new PingTypeComparer();

	public static readonly CachedEnumString<PingType> sCachedPingTypeStrings = new CachedEnumString<PingType>(sPingTypeComparer);

	public static readonly CachedEnumString<PingType> sCachedPingTypeTranslationStrings = new CachedEnumString<PingType>("Ping", sPingTypeComparer);

	public static readonly Color[] colorOptions = new Color[5]
	{
		new Color32(73, 190, byte.MaxValue, byte.MaxValue),
		new Color32(byte.MaxValue, 146, 71, byte.MaxValue),
		new Color32(219, 95, 64, byte.MaxValue),
		new Color32(93, 205, 200, byte.MaxValue),
		new Color32(byte.MaxValue, 209, 0, byte.MaxValue)
	};

	[SuppressMessage("Gendarme.Rules.Concurrency", "NonConstantStaticFieldsShouldNotBeVisibleRule")]
	public static OnAdd onAdd;

	[SuppressMessage("Gendarme.Rules.Concurrency", "NonConstantStaticFieldsShouldNotBeVisibleRule")]
	public static OnRemove onRemove;

	[SuppressMessage("Gendarme.Rules.Concurrency", "NonConstantStaticFieldsShouldNotBeVisibleRule")]
	public static OnRename onRename;

	[SuppressMessage("Gendarme.Rules.Concurrency", "NonConstantStaticFieldsShouldNotBeVisibleRule")]
	public static OnIconChange onIconChange;

	[SuppressMessage("Gendarme.Rules.Concurrency", "NonConstantStaticFieldsShouldNotBeVisibleRule")]
	public static OnColor onColor;

	[SuppressMessage("Gendarme.Rules.Concurrency", "NonConstantStaticFieldsShouldNotBeVisibleRule")]
	public static OnVisible onVisible;

	[SuppressMessage("Gendarme.Rules.Concurrency", "NonConstantStaticFieldsShouldNotBeVisibleRule")]
	public static OnVisit onVisit;

	private static Dictionary<string, PingInstance> pings = new Dictionary<string, PingInstance>();

	public static void Register(PingInstance instance)
	{
		if (instance == null)
		{
			return;
		}
		string id = instance.Id;
		if (!pings.ContainsKey(id))
		{
			pings.Add(id, instance);
			NotificationManager.main.RegisterTarget(NotificationManager.Group.Pings, id, instance);
			if (onAdd != null)
			{
				onAdd(instance);
			}
		}
	}

	public static void Unregister(PingInstance instance)
	{
		if (instance == null)
		{
			return;
		}
		string id = instance.Id;
		if (pings.Remove(id))
		{
			NotificationManager.main.UnregisterTarget(instance);
			if (onRemove != null)
			{
				onRemove(id);
			}
		}
	}

	public static void NotifyVisible(PingInstance instance)
	{
		if (!(instance == null) && onVisible != null)
		{
			onVisible(instance.Id, instance.visible);
		}
	}

	public static void NotifyVisit(PingInstance instance, float progress)
	{
		if (!(instance == null) && onVisit != null)
		{
			onVisit(instance, progress);
		}
	}

	public static void NotifyRename(PingInstance instance)
	{
		if (!(instance == null) && onRename != null)
		{
			onRename(instance);
		}
	}

	public static void NotifyIconChange(PingInstance instance)
	{
		if (!(instance == null) && onIconChange != null)
		{
			onIconChange(instance);
		}
	}

	public static void NotifyColor(PingInstance instance)
	{
		if (!(instance == null) && onColor != null)
		{
			int num = instance.colorIndex;
			if (num < 0 || num >= colorOptions.Length)
			{
				num = 0;
			}
			Color color = colorOptions[num];
			onColor(instance.Id, color);
		}
	}

	public static void SetVisible(string id, bool visible)
	{
		if (id != null && pings.TryGetValue(id, out var value))
		{
			value.SetVisible(visible);
		}
	}

	public static void SetColor(string id, int colorIndex)
	{
		if (id != null && pings.TryGetValue(id, out var value))
		{
			value.SetColor(colorIndex);
		}
	}

	public static Dictionary<string, PingInstance>.Enumerator GetEnumerator()
	{
		return pings.GetEnumerator();
	}

	public static PingInstance Get(string id)
	{
		if (id != null && pings.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	public static void Deinitialize()
	{
		pings.Clear();
	}

	public static string CompileTimeCheck(ILanguage language)
	{
		string text = null;
		foreach (PingType value in Enum.GetValues(typeof(PingType)))
		{
			text = language.CheckKey(sCachedPingTypeTranslationStrings.Get(value));
			if (text != null)
			{
				break;
			}
		}
		return text;
	}
}
