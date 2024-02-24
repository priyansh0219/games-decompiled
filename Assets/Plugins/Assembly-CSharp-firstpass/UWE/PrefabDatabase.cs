using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UWE
{
	public static class PrefabDatabase
	{
		public static readonly Dictionary<string, string> prefabFiles = new Dictionary<string, string>();

		public static void SavePrefabDatabase(string fullFilename)
		{
			using (BinaryWriter writer = new BinaryWriter(FileUtils.CreateFile(fullFilename)))
			{
				Debug.Log($"PrefabDatabase::SavePrefabDatabase(count: {prefabFiles.Count})");
				writer.WriteInt32(prefabFiles.Count);
				foreach (KeyValuePair<string, string> prefabFile in prefabFiles)
				{
					if (string.IsNullOrEmpty(prefabFile.Key) || string.IsNullOrEmpty(prefabFile.Value))
					{
						Debug.LogWarningFormat("Invalid prefab '{0}' at '{1}' in prefab database.", prefabFile.Key, prefabFile.Value);
					}
					else
					{
						writer.WriteString(prefabFile.Key);
						writer.WriteString(prefabFile.Value);
					}
				}
			}
		}

		public static void LoadPrefabDatabase(string fullFilename)
		{
			prefabFiles.Clear();
			if (!File.Exists(fullFilename))
			{
				return;
			}
			using (FileStream input = File.OpenRead(fullFilename))
			{
				using (BinaryReader binaryReader = new BinaryReader(input))
				{
					int num = binaryReader.ReadInt32();
					Debug.Log($"PrefabDatabase::LoadPrefabDatabase(count: {num})");
					for (int i = 0; i < num; i++)
					{
						string key = binaryReader.ReadString();
						string value = binaryReader.ReadString();
						prefabFiles[key] = value;
					}
				}
			}
		}

		public static IPrefabRequest GetPrefabAsync(string classId)
		{
			if (ScenePrefabDatabase.TryGetScenePrefab(classId, out var prefab))
			{
				return new LoadedPrefabRequest(prefab);
			}
			if (!TryGetPrefabFilename(classId, out var filename))
			{
				Debug.LogWarningFormat("No filename for prefab {0} in database containing {1} entries", classId, prefabFiles.Count);
				return new LoadedPrefabRequest(null);
			}
			return GetPrefabForFilenameAsync(filename);
		}

		public static IPrefabRequest GetPrefabForFilenameAsync(string filename)
		{
			return new LoadingPrefabRequest(filename);
		}

		public static bool TryGetPrefabFilename(string classId, out string filename)
		{
			if (string.IsNullOrEmpty(classId))
			{
				filename = null;
				return false;
			}
			return prefabFiles.TryGetValue(classId, out filename);
		}

		public static int GetCacheSize()
		{
			return 0;
		}

		public static AsyncOperation UnloadUnusedAssets()
		{
			return Resources.UnloadUnusedAssets();
		}
	}
}
