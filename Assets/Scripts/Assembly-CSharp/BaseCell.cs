using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class BaseCell : MonoBehaviour, IMovementPlatform
{
	[NonSerialized]
	public Int3 cell = Int3.zero;

	public bool IsPlatform()
	{
		return false;
	}
}
