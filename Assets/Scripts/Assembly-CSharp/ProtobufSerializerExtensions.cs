using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UWE;
using UnityEngine;

public static class ProtobufSerializerExtensions
{
	public static void SaveObjectTreeToFile(this ProtobufSerializer serializer, string filePath, GameObject root)
	{
		using (Stream stream = FileUtils.CreateFile(filePath))
		{
			serializer.SerializeStreamHeader(stream);
			serializer.SerializeObjectTree(stream, root);
		}
	}

	public static CoroutineTask<GameObject> LoadObjectTreeFromFileAsync(this ProtobufSerializer serializer, string filePath, int verbose)
	{
		TaskResult<GameObject> result = new TaskResult<GameObject>();
		return new CoroutineTask<GameObject>(serializer.LoadObjectTreeFromFileAsync(filePath, verbose, result), result);
	}

	public static CoroutineTask<GameObject> SerializeAndDeserializeAsync(this ProtobufSerializer serializer, GameObject source)
	{
		TaskResult<GameObject> result = new TaskResult<GameObject>();
		return new CoroutineTask<GameObject>(SerializeAndDeserializeAsync(serializer, source, result), result);
	}

	public static void SerializeObjectTree(this ProtobufSerializer serializer, Stream stream, GameObject root)
	{
		CoroutineUtils.PumpCoroutine(serializer.SerializeObjectTreeAsync(stream, root));
	}

	public static void SerializeObjects(this ProtobufSerializer serializer, Stream stream, IList<UniqueIdentifier> uids, bool storeParent)
	{
		CoroutineUtils.PumpCoroutine(serializer.SerializeObjectsAsync(stream, uids, storeParent));
	}

	public static IEnumerator DeserializeObjectTreeAsync(this ProtobufSerializer serializer, Stream stream, UniqueIdentifier uid, int verbose, bool allowSpawnRestrictions, IOut<GameObject> result)
	{
		CoroutineTask<GameObject> deserializeObjectsTask = serializer.DeserializeObjectsAsync(stream, uid, forceInactiveRoot: false, forceParent: false, null, allowSpawnRestrictions, verbose);
		yield return deserializeObjectsTask;
		GameObject result2 = deserializeObjectsTask.GetResult();
		if ((bool)result2)
		{
			IProtoTreeEventListener[] componentsInChildren = result2.GetComponentsInChildren<IProtoTreeEventListener>(includeInactive: true);
			foreach (IProtoTreeEventListener protoTreeEventListener in componentsInChildren)
			{
				UnityEngine.Object @object = protoTreeEventListener as UnityEngine.Object;
				if ((bool)@object)
				{
					try
					{
						protoTreeEventListener.OnProtoDeserializeObjectTree(serializer);
					}
					catch (Exception exception)
					{
						Debug.LogException(exception, @object);
					}
				}
			}
		}
		result.Set(result2);
	}

	private static IEnumerator LoadObjectTreeFromFileAsync(this ProtobufSerializer serializer, string filePath, int verbose, IOut<GameObject> result)
	{
		using (Stream file = FileUtils.ReadFile(filePath))
		{
			if (serializer.TryDeserializeStreamHeader(file))
			{
				CoroutineTask<GameObject> task = serializer.DeserializeObjectTreeAsync(file, forceInactiveRoot: false, allowSpawnRestrictions: false, verbose);
				yield return task;
				result.Set(task.GetResult());
				yield break;
			}
		}
		Debug.LogErrorFormat("Ignoring exception while fallback-deserializing file '{0}'. Carrying on.", filePath);
		GameObject value = new GameObject("fallback batch root");
		result.Set(value);
	}

	private static IEnumerator SerializeAndDeserializeAsync(ProtobufSerializer serializer, GameObject source, IOut<GameObject> result)
	{
		using (MemoryStream stream = new MemoryStream())
		{
			serializer.SerializeObjectTree(stream, source);
			stream.Seek(0L, SeekOrigin.Begin);
			CoroutineTask<GameObject> task = serializer.DeserializeObjectTreeAsync(stream, forceInactiveRoot: false, allowSpawnRestrictions: false, 0);
			yield return task;
			result.Set(task.GetResult());
		}
	}
}
