using System.Collections.Generic;
using UnityEngine;

namespace UWE
{
	public class WorldEntityDatabase
	{
		private static WorldEntityDatabase _main;

		private const string dataPath = "WorldEntities/WorldEntityData";

		private readonly Dictionary<string, WorldEntityInfo> infos;

		public static WorldEntityDatabase main
		{
			get
			{
				if (_main == null)
				{
					_main = new WorldEntityDatabase();
				}
				return _main;
			}
		}

		public static bool TryGetInfo(string classId, out WorldEntityInfo info)
		{
			return main.infos.TryGetValue(classId, out info);
		}

		public WorldEntityDatabase()
		{
			WorldEntityData worldEntityData = Resources.Load<WorldEntityData>("WorldEntities/WorldEntityData");
			if (!worldEntityData)
			{
				Debug.LogErrorFormat("Failed to load WorldEntityData at '{0}'", "WorldEntities/WorldEntityData");
				return;
			}
			infos = new Dictionary<string, WorldEntityInfo>(worldEntityData.infos.Length);
			WorldEntityInfo[] array = worldEntityData.infos;
			foreach (WorldEntityInfo worldEntityInfo in array)
			{
				infos[worldEntityInfo.classId] = worldEntityInfo;
			}
		}
	}
}
