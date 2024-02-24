using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class DiveReelAnchor : HandTarget
{
	[NonSerialized]
	[ProtoMember(1)]
	public string reelId;

	[NonSerialized]
	[ProtoMember(2)]
	public readonly List<Vector3> contactPts = new List<Vector3>();

	[NonSerialized]
	[ProtoMember(3)]
	public bool lastContactUnraveling;

	[NonSerialized]
	[ProtoMember(4)]
	public Vector3 prevLineEndPos;

	[NonSerialized]
	[ProtoMember(5)]
	public bool reelWasDropped;
}
