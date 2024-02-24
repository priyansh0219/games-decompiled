using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class ProtobufSerializerCornerCases
{
	[NonSerialized]
	[ProtoMember(1, OverwriteList = true)]
	public List<SceneObjectData> ListOfClassInstances;

	[NonSerialized]
	[ProtoMember(2, OverwriteList = true)]
	public Dictionary<int, SceneObjectData> DictionaryOfClassInstances;

	[NonSerialized]
	[ProtoMember(3)]
	public Vector3? NullableStruct;

	[NonSerialized]
	[ProtoMember(4)]
	public CubemapFace? NullableEnum;

	[NonSerialized]
	[ProtoMember(5)]
	public Grid3<float> FloatGrid;

	[NonSerialized]
	[ProtoMember(6)]
	public Grid3<Vector3> Vector3Grid;

	[NonSerialized]
	[ProtoMember(7, OverwriteList = true)]
	public int[] EmptyArray;

	[NonSerialized]
	[ProtoMember(8)]
	public readonly HashSet<string> InitializedSet = new HashSet<string>(new string[2] { "foo", "bar" }, StringComparer.OrdinalIgnoreCase);
}
