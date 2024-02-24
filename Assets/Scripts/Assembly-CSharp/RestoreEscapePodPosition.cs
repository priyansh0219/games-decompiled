using System;
using ProtoBuf;
using UnityEngine;

[Obsolete]
[ProtoContract]
public class RestoreEscapePodPosition : MonoBehaviour, IProtoEventListener
{
	[NonSerialized]
	[ProtoMember(1)]
	public Vector3 position;

	private bool hasPosition;

	private EscapePod pod;

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		position = pod.transform.position;
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		hasPosition = true;
	}

	private void Start()
	{
		if (hasPosition)
		{
			pod.StartAtPosition(position);
		}
		UnityEngine.Object.Destroy(this);
	}
}
