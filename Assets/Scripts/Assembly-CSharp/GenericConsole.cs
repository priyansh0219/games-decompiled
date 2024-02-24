using System;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

[ProtoContract]
public class GenericConsole : MonoBehaviour, IProtoEventListener
{
	[AssertNotNull]
	public HandTarget handTarget;

	[AssertNotNull]
	public Image iconImage;

	public Color colorUnused;

	public Color colorUsed;

	[NonSerialized]
	[ProtoMember(1)]
	public bool gotUsed;

	private void Start()
	{
		UpdateState();
	}

	private void OnStoryHandTarget()
	{
		gotUsed = true;
		UpdateState();
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		UpdateState();
	}

	private void UpdateState()
	{
		handTarget.isValidHandTarget = !gotUsed;
		Color color = (gotUsed ? colorUsed : colorUnused);
		iconImage.color = color;
	}
}
