using System.Collections.Generic;
using Gendarme;

public class IntStringCache
{
	private static Dictionary<int, string> intStringCacheDict;

	[SuppressMessage("Subnautica.Rules", "AvoidStringConcatenation")]
	public static string GetStringForInt(int val)
	{
		if (intStringCacheDict == null)
		{
			intStringCacheDict = new Dictionary<int, string>(2500);
			for (int i = 0; i < 2500; i++)
			{
				string value = i.ToString();
				intStringCacheDict.Add(i, value);
			}
		}
		if (!intStringCacheDict.TryGetValue(val, out var value2))
		{
			value2 = val.ToString();
			intStringCacheDict.Add(val, value2);
		}
		return value2;
	}
}
