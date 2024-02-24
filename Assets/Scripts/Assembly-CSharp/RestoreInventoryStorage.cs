using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UWE;
using UnityEngine;

[Obsolete]
[ProtoContract]
public class RestoreInventoryStorage : MonoBehaviour, IProtoEventListener
{
	[NonSerialized]
	[ProtoMember(1, OverwriteList = true)]
	public byte[] serialData;

	[NonSerialized]
	[ProtoMember(2)]
	public float food;

	[NonSerialized]
	[ProtoMember(3)]
	public float water;

	[NonSerialized]
	[ProtoMember(4, OverwriteList = true)]
	public List<string> completedCustomGoals;

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		serialData = StorageHelper.Save(serializer, Inventory.main.storageRoot);
		food = Player.main.GetComponent<Survival>().food;
		Debug.Log("saving food = " + food);
		water = Player.main.GetComponent<Survival>().water;
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
	}

	private IEnumerator Start()
	{
		while (!Inventory.main)
		{
			yield return new WaitForSeconds(1f);
		}
		using (PooledObject<ProtobufSerializer> serializer = ProtobufSerializerPool.GetProxy())
		{
			Inventory.main.ResetInventory();
			yield return StorageHelper.RestoreItemsAsync(serializer, serialData, Inventory.main.container);
		}
		if (Utils.GetContinueMode())
		{
			Player.main.GetComponent<Survival>().food = food;
			Player.main.GetComponent<Survival>().water = water;
			Debug.Log("Restored food to " + food);
		}
		UnityEngine.Object.Destroy(this);
	}
}
