using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VoxelandSerialData : MonoBehaviour
{
	public int version;

	[HideInInspector]
	public List<byte> octreeSerialBytes;

	public List<int> octreeSerialIndex;

	[HideInInspector]
	public byte[] octreeBytesArray;

	public int sizeX;

	public int sizeY;

	public int sizeZ;

	public int rootSize;
}
