using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class SceneObjectDataSet
{
	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public readonly Dictionary<string, SceneObjectData> items = new Dictionary<string, SceneObjectData>();

	public void Reset()
	{
		items.Clear();
	}

	public void Serialize(ProtobufSerializer serializer, SceneObjectIdentifier id)
	{
		SceneObjectData sceneObjectData = new SceneObjectData();
		sceneObjectData.SerializeFrom(serializer, id);
		items[sceneObjectData.uniqueName] = sceneObjectData;
	}

	public IEnumerator TryDeserializeAsync(ProtobufSerializer serializer, SceneObjectIdentifier id, IOut<bool> result)
	{
		result.Set(value: false);
		if (items.TryGetValue(id.uniqueName, out var value))
		{
			yield return value.DeserializeIntoAsync(serializer, id);
			result.Set(value: true);
		}
	}

	public IEnumerable<SceneObjectData> Items()
	{
		return items.Values;
	}
}
