using System;

public static class LanguageUtils
{
	public static string CheckTechType(this ILanguage language, TechType techType)
	{
		if (techType.IsObsolete())
		{
			return null;
		}
		string key = techType.AsString();
		return language.CheckKey(key);
	}

	public static string CheckTechTypeTooltip(this ILanguage language, TechType techType)
	{
		if (techType.IsObsolete())
		{
			return null;
		}
		string key = TooltipFactory.techTypeTooltipStrings.Get(techType);
		return language.CheckKey(key);
	}

	public static string CheckFormat(this ILanguage language, string key, int args)
	{
		string text = language.CheckKey(key);
		if (string.IsNullOrEmpty(text))
		{
			string value = language.Get(key);
			if (language.GetFormat(key, new object[args]).Equals(value, StringComparison.Ordinal))
			{
				text = $"The localization key {key} does not have {args} format items in it.";
			}
		}
		return text;
	}

	public static string CheckKey(this ILanguage language, string key, bool allowEmpty = false)
	{
		if (allowEmpty && string.IsNullOrWhiteSpace(key))
		{
			return null;
		}
		if (!language.Contains(key))
		{
			return $"{key} is missing from English.json";
		}
		return null;
	}

	public static string CheckKeys(this ILanguage language, string[] keys, bool allowEmpty = false)
	{
		string text = null;
		foreach (string key in keys)
		{
			text = language.CheckKey(key, allowEmpty);
			if (text != null)
			{
				break;
			}
		}
		return text;
	}
}
