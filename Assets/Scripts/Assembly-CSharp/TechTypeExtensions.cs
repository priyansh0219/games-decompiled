using System;
using System.Collections.Generic;
using System.Reflection;
using Gendarme;

public static class TechTypeExtensions
{
	public class TechTypeComparer : IEqualityComparer<TechType>
	{
		public bool Equals(TechType x, TechType y)
		{
			return x == y;
		}

		public int GetHashCode(TechType techType)
		{
			return (int)techType;
		}
	}

	public static readonly TechTypeComparer sTechTypeComparer;

	public static readonly List<TechType> sTechTypes;

	private static Dictionary<TechType, string> stringsNormal;

	private static Dictionary<TechType, string> stringsLowercase;

	private static Dictionary<string, TechType> techTypesNormal;

	private static Dictionary<string, TechType> techTypesIgnoreCase;

	private static Dictionary<TechType, string> techTypeKeys;

	private static Dictionary<string, TechType> keyTechTypes;

	[SuppressMessage("Subnautica.Rules", "AvoidTechTypeToStringRule")]
	static TechTypeExtensions()
	{
		sTechTypeComparer = new TechTypeComparer();
		Array values = Enum.GetValues(typeof(TechType));
		int length = values.Length;
		sTechTypes = new List<TechType>();
		stringsNormal = new Dictionary<TechType, string>(length, sTechTypeComparer);
		stringsLowercase = new Dictionary<TechType, string>(length, sTechTypeComparer);
		techTypesNormal = new Dictionary<string, TechType>(length);
		techTypesIgnoreCase = new Dictionary<string, TechType>(length, StringComparer.InvariantCultureIgnoreCase);
		techTypeKeys = new Dictionary<TechType, string>(length, sTechTypeComparer);
		keyTechTypes = new Dictionary<string, TechType>(length);
		for (int i = 0; i < length; i++)
		{
			TechType techType = (TechType)values.GetValue(i);
			string text = techType.ToString();
			if (!sTechTypes.Contains(techType))
			{
				sTechTypes.Add(techType);
			}
			if (!stringsNormal.ContainsKey(techType))
			{
				stringsNormal[techType] = text;
				stringsLowercase[techType] = text.ToLower();
			}
			if (!techTypesNormal.ContainsKey(text))
			{
				techTypesNormal[text] = techType;
			}
			if (!techTypesIgnoreCase.ContainsKey(text))
			{
				techTypesIgnoreCase[text] = techType;
			}
			int num = (int)techType;
			string text2 = num.ToString();
			techTypeKeys.Add(techType, text2);
			keyTechTypes.Add(text2, techType);
		}
	}

	public static string AsString(this TechType techType, bool lowercase = false)
	{
		string value;
		if (lowercase)
		{
			if (stringsLowercase.TryGetValue(techType, out value))
			{
				return value;
			}
		}
		else if (stringsNormal.TryGetValue(techType, out value))
		{
			return value;
		}
		int num = (int)techType;
		return num.ToString();
	}

	public static bool FromString(string str, out TechType techType, bool ignoreCase)
	{
		if (ignoreCase)
		{
			return techTypesIgnoreCase.TryGetValue(str, out techType);
		}
		return techTypesNormal.TryGetValue(str, out techType);
	}

	public static string EncodeKey(this TechType techType)
	{
		if (techTypeKeys.TryGetValue(techType, out var value))
		{
			return value;
		}
		return null;
	}

	public static TechType DecodeKey(this string key)
	{
		if (keyTechTypes.TryGetValue(key, out var value))
		{
			return value;
		}
		return TechType.None;
	}

	public static bool Contains(this Language language, TechType techType)
	{
		return language.Contains(techType.AsString());
	}

	public static bool TryGet(this Language language, TechType techType, out string result)
	{
		return language.TryGet(techType.AsString(), out result);
	}

	public static string Get(this ILanguage language, TechType techType)
	{
		return language.Get(techType.AsString());
	}

	public static string GetOrFallback(this Language language, TechType techType, TechType fallbackTechType)
	{
		return language.GetOrFallback(techType.AsString(), fallbackTechType.AsString());
	}

	public static string GetOrFallback(this Language language, string key, TechType fallbackTechType)
	{
		return language.GetOrFallback(key, fallbackTechType.AsString());
	}

	public static string GetOrFallback(this Language language, TechType techType, string fallbackKey)
	{
		return language.GetOrFallback(techType.AsString(), fallbackKey);
	}

	public static bool IsObsolete(this TechType techType)
	{
		FieldInfo field = typeof(TechType).GetField(techType.AsString());
		if (!(field == null))
		{
			return Attribute.IsDefined(field, typeof(ObsoleteAttribute));
		}
		return true;
	}

	public static IEnumerable<string> GetTechTypeNamesSuggestion(string techTypeString)
	{
		List<string> list = new List<string>();
		string[] names = Enum.GetNames(typeof(TechType));
		foreach (string text in names)
		{
			if (text.IndexOf(techTypeString, StringComparison.InvariantCultureIgnoreCase) != -1)
			{
				list.Add(text);
			}
		}
		return list;
	}
}
