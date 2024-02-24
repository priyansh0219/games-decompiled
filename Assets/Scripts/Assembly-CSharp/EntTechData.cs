using System;
using System.Collections.Generic;
using UnityEngine;

public class EntTechData : ScriptableObject, ICompileTimeCheckable
{
	[Serializable]
	public class Entry
	{
		public string prefabName;

		public TechType techType;
	}

	public Entry[] entTechMap;

	public string CompileTimeCheck()
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		Entry[] array = entTechMap;
		foreach (Entry obj in array)
		{
			string prefabName = obj.prefabName;
			if (obj.techType == TechType.None)
			{
				return $"TechType not set for entry '{prefabName}'";
			}
			if (prefabName != prefabName.ToLowerInvariant())
			{
				return $"Invalid entry '{prefabName}'. All entries must be lowercase!";
			}
			if (!hashSet.Add(prefabName))
			{
				return $"Duplicate entry '{prefabName}' in EntTechData";
			}
		}
		return null;
	}
}
