using System;
using System.Collections.Generic;

public static class BiomeTypeExtensions
{
	public class BiomeTypeComparer : IEqualityComparer<BiomeType>
	{
		public bool Equals(BiomeType x, BiomeType y)
		{
			int num = (int)x;
			return num.Equals((int)y);
		}

		public int GetHashCode(BiomeType obj)
		{
			return (int)obj;
		}
	}

	public static readonly BiomeTypeComparer sBiomeTypeComparer;

	private static Dictionary<BiomeType, string> stringsNormal;

	private static Dictionary<BiomeType, string> stringsLowercase;

	private static Dictionary<string, BiomeType> biomeTypesNormal;

	private static Dictionary<string, BiomeType> biomeTypesIgnoreCase;

	static BiomeTypeExtensions()
	{
		sBiomeTypeComparer = new BiomeTypeComparer();
		Array values = Enum.GetValues(typeof(BiomeType));
		int length = values.Length;
		stringsNormal = new Dictionary<BiomeType, string>(length, sBiomeTypeComparer);
		stringsLowercase = new Dictionary<BiomeType, string>(length, sBiomeTypeComparer);
		biomeTypesNormal = new Dictionary<string, BiomeType>(length);
		biomeTypesIgnoreCase = new Dictionary<string, BiomeType>(length, StringComparer.InvariantCultureIgnoreCase);
		for (int i = 0; i < length; i++)
		{
			BiomeType biomeType = (BiomeType)values.GetValue(i);
			string text = biomeType.ToString();
			if (!stringsNormal.ContainsKey(biomeType))
			{
				stringsNormal[biomeType] = text;
				stringsLowercase[biomeType] = text.ToLower();
			}
			if (!biomeTypesNormal.ContainsKey(text))
			{
				biomeTypesNormal[text] = biomeType;
			}
			if (!biomeTypesIgnoreCase.ContainsKey(text))
			{
				biomeTypesIgnoreCase[text] = biomeType;
			}
		}
	}

	public static string AsString(this BiomeType biomeType, bool lowercase = false)
	{
		string value;
		if (lowercase)
		{
			if (stringsLowercase.TryGetValue(biomeType, out value))
			{
				return value;
			}
		}
		else if (stringsNormal.TryGetValue(biomeType, out value))
		{
			return value;
		}
		int num = (int)biomeType;
		return num.ToString();
	}

	public static bool FromString(string str, out BiomeType biomeType, bool ignoreCase)
	{
		if (ignoreCase)
		{
			return biomeTypesIgnoreCase.TryGetValue(str, out biomeType);
		}
		return biomeTypesNormal.TryGetValue(str, out biomeType);
	}
}
