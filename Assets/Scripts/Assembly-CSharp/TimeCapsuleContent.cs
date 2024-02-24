using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class TimeCapsuleContent
{
	[NonSerialized]
	[ProtoMember(1)]
	public string title;

	[NonSerialized]
	[ProtoMember(2)]
	public string text;

	[NonSerialized]
	[ProtoMember(3)]
	public string imageUrl;

	[NonSerialized]
	[ProtoMember(4, OverwriteList = true)]
	public List<TimeCapsuleItem> items;

	[NonSerialized]
	[ProtoMember(5)]
	public string updatedAt;

	[NonSerialized]
	[ProtoMember(6)]
	public bool isActive = true;

	[NonSerialized]
	[ProtoMember(7)]
	public int copiesFound;
}
