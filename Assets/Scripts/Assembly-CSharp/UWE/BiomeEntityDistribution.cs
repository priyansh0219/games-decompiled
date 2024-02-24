using System;
using System.IO;

namespace UWE
{
	[Serializable]
	public class BiomeEntityDistribution
	{
		public string entityName = "";

		public int count;

		private bool IsValidPrefab(string name)
		{
			if (!File.Exists("Assets/Prefabs/Natural/" + name + ".prefab") && !File.Exists("Assets/Prefabs/Creatures/" + name + ".prefab") && !File.Exists("Assets/Prefabs/Environment/" + name + ".prefab"))
			{
				return false;
			}
			return true;
		}

		public bool IsValid(out string errorString)
		{
			errorString = "";
			if (count > 0)
			{
				if (IsValidPrefab(entityName))
				{
					return true;
				}
				errorString = "\"" + entityName + "\" isn't a valid prefab";
			}
			else
			{
				errorString = "Count must be greater than 0.";
			}
			return false;
		}
	}
}
