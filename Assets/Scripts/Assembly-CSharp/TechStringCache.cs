using System;
using System.Collections.Generic;

public class TechStringCache
{
	private struct Key
	{
		public TechType techType;

		public Type behaviour;

		public string name;

		public override bool Equals(object obj)
		{
			if (!(obj is Key key))
			{
				return false;
			}
			if (techType == key.techType && behaviour == key.behaviour)
			{
				return name == key.name;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return techType.GetHashCode() ^ behaviour.GetHashCode() ^ name.GetHashCode();
		}
	}

	private Dictionary<Key, string> master = new Dictionary<Key, string>();

	public bool GetValueExists(TechType techType, Type behaviour, string name)
	{
		string value;
		return TryGetValue(techType, behaviour, name, out value);
	}

	public string GetValue(TechType techType, Type behaviour, string name)
	{
		if (!TryGetValue(techType, behaviour, name, out var value))
		{
			return "";
		}
		return value;
	}

	public bool TryGetValue(TechType techType, Type behaviour, string name, out string value)
	{
		if (techType != 0)
		{
			Key key = default(Key);
			key.techType = techType;
			key.behaviour = behaviour;
			key.name = name;
			if (master.TryGetValue(key, out value))
			{
				return true;
			}
		}
		value = null;
		return false;
	}

	public bool SetValue(TechType techType, Type behaviour, string name, string value)
	{
		bool result = false;
		if (techType != 0)
		{
			Key key = default(Key);
			key.techType = techType;
			key.behaviour = behaviour;
			key.name = name;
			if (!master.ContainsKey(key))
			{
				master.Add(key, value);
				result = true;
			}
		}
		return result;
	}
}
