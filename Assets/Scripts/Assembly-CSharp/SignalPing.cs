using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class SignalPing : MonoBehaviour, IProtoEventListener
{
	[AssertNotNull]
	public PingInstance pingInstance;

	public PDANotification vo;

	public bool disableOnEnter;

	private const int _currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public string descriptionKey;

	[NonSerialized]
	[ProtoMember(3)]
	public Vector3 pos;

	private void Start()
	{
		base.transform.position = pos;
		UpdateLabel();
	}

	public void PlayVO()
	{
		if (!(vo == null))
		{
			vo.Play();
		}
	}

	private void UpdateLabel()
	{
		pingInstance.SetLabel(Language.main.Get(descriptionKey));
	}

	public void OnTriggerEnter(Collider other)
	{
		if (disableOnEnter && other.gameObject.Equals(Player.main.gameObject))
		{
			pingInstance.SetVisible(value: false);
		}
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		UpdateLabel();
	}
}
