using System;
using System.Collections;
using ProtoBuf;
using UWE;
using UnityEngine;

[Obsolete]
[ProtoContract]
public class RestoreEscapePodStorage : MonoBehaviour, IProtoEventListener
{
	[NonSerialized]
	[ProtoMember(1, OverwriteList = true)]
	public byte[] serialData;

	private StorageContainer podStorage;

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		serialData = StorageHelper.Save(serializer, podStorage.storageRoot.gameObject);
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
	}

	private IEnumerator Start()
	{
		EscapePod escapePod;
		while ((escapePod = UnityEngine.Object.FindObjectOfType<EscapePod>()) == null)
		{
			yield return new WaitForSeconds(1f);
		}
		podStorage = escapePod.GetComponentInChildren<StorageContainer>();
		using (PooledObject<ProtobufSerializer> serializer = ProtobufSerializerPool.GetProxy())
		{
			podStorage.ResetContainer();
			yield return StorageHelper.RestoreItemsAsync(serializer, serialData, podStorage.container);
		}
		UnityEngine.Object.Destroy(this);
	}
}
