using System;
using System.Collections.Generic;

public class CachedEnumString<T>
{
	private Dictionary<T, string> valueToString;

	public ICollection<T> Values => valueToString.Keys;

	public ICollection<string> Names => valueToString.Values;

	public CachedEnumString(IEqualityComparer<T> comparer)
		: this(string.Empty, comparer)
	{
	}

	public CachedEnumString(string prefix, IEqualityComparer<T> comparer)
		: this(prefix, string.Empty, comparer)
	{
	}

	public CachedEnumString(string prefix, string postfix, IEqualityComparer<T> comparer)
	{
		Type typeFromHandle = typeof(T);
		if (!typeFromHandle.IsEnum)
		{
			throw new ArgumentException("T must be an enumerated type");
		}
		if (prefix == null)
		{
			throw new ArgumentException("prefix cannot be null");
		}
		if (postfix == null)
		{
			throw new ArgumentException("postfix cannot be null");
		}
		if (comparer == null)
		{
			throw new ArgumentException("comparer cannot be null");
		}
		Array values = Enum.GetValues(typeFromHandle);
		int length = values.Length;
		valueToString = new Dictionary<T, string>(length, comparer);
		for (int i = 0; i < length; i++)
		{
			T key = (T)values.GetValue(i);
			string value = prefix + key.ToString() + postfix;
			if (!valueToString.ContainsKey(key))
			{
				valueToString[key] = value;
			}
		}
	}

	public string Get(T value)
	{
		if (valueToString.TryGetValue(value, out var value2))
		{
			return value2;
		}
		return string.Empty;
	}

	public bool TryGet(T value, out string name)
	{
		return valueToString.TryGetValue(value, out name);
	}
}
