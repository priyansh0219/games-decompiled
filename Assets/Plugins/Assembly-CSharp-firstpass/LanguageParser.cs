using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class LanguageParser
{
	public enum Tag
	{
		None = 0,
		Delay = 1,
		Duration = 2
	}

	public static readonly char[] messageSeparators;

	public static readonly Dictionary<string, Tag> tags;

	private static readonly Dictionary<Tag, string> tagNames;

	private StringBuilder sb = new StringBuilder();

	public float? delay;

	public float? duration;

	public string text;

	static LanguageParser()
	{
		messageSeparators = new char[1] { '\n' };
		tags = new Dictionary<string, Tag>(StringComparer.OrdinalIgnoreCase)
		{
			{
				"delay",
				Tag.Delay
			},
			{
				"duration",
				Tag.Duration
			}
		};
		tagNames = new Dictionary<Tag, string>();
		foreach (KeyValuePair<string, Tag> tag in tags)
		{
			if (!tagNames.ContainsKey(tag.Value))
			{
				tagNames.Add(tag.Value, tag.Key);
			}
		}
	}

	public static bool TryGetName(Tag tag, out string name)
	{
		return tagNames.TryGetValue(tag, out name);
	}

	private void AppendText(string text, int start, int length)
	{
		sb.Append(text, start, length);
	}

	private bool AppendTag(string key, string value)
	{
		if (tags.TryGetValue(key, out var value2))
		{
			int result;
			switch (value2)
			{
			case Tag.Delay:
				if (value != null && int.TryParse(value, out result))
				{
					delay = Mathf.Clamp((float)result / 1000f, 0f, 100f);
					return true;
				}
				break;
			case Tag.Duration:
				if (value != null && int.TryParse(value, out result))
				{
					duration = Mathf.Clamp((float)result / 1000f, 0.01f, 100f);
					return true;
				}
				break;
			}
		}
		return false;
	}

	public void Parse(string input)
	{
		delay = null;
		duration = null;
		text = null;
		sb.Length = 0;
		if (input != null)
		{
			ParseTags(input, AppendText, AppendTag);
			text = sb.ToString();
		}
	}

	private static void ParseTags(string input, Action<string, int, int> appendText, Func<string, string, bool> appendTag)
	{
		int num = 0;
		MatchCollection matchCollection = Regex.Matches(input, "<(\\S+?)(?:\\s*=\\s*(?:([^\\s>\"]+?)|\"([\\s\\S]+?)\"))?\\s*\\/?>");
		for (int i = 0; i < matchCollection.Count; i++)
		{
			GroupCollection groups = matchCollection[i].Groups;
			Group group = groups[0];
			Group group2 = groups[1];
			Group group3 = groups[2];
			Group group4 = groups[3];
			string value = group2.Value;
			string arg = (group3.Success ? group3.Value : (group4.Success ? group4.Value : null));
			int num2 = group.Index - num;
			if (num2 > 0)
			{
				appendText(input, num, num2);
			}
			num = group.Index;
			if (appendTag(value, arg))
			{
				num = group.Index + group.Length;
			}
		}
		appendText(input, num, input.Length - num);
	}
}
