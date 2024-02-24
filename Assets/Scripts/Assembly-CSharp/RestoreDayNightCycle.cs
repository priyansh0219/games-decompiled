using System;
using ProtoBuf;
using UnityEngine;

[Obsolete]
[ProtoContract]
public class RestoreDayNightCycle : MonoBehaviour, IProtoEventListener
{
	[NonSerialized]
	[ProtoMember(1)]
	public float timePassed = -1f;

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		if ((bool)DayNightCycle.main)
		{
			timePassed = DayNightCycle.main.timePassedAsFloat;
		}
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
	}

	private void Start()
	{
		if ((bool)DayNightCycle.main && timePassed >= 0f)
		{
			DayNightCycle.main.SetTimePassed(timePassed);
		}
		UnityEngine.Object.Destroy(this);
	}
}
