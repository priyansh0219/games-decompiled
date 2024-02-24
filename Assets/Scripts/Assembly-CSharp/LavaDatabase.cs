using System;
using System.Collections.Generic;
using UWE;
using UnityEngine;

public class LavaDatabase : ScriptableObject
{
	[NonSerialized]
	private static readonly List<BlockTypeClassification> emptyList = new List<BlockTypeClassification>();

	[NonSerialized]
	private readonly Dictionary<byte, List<BlockTypeClassification>> lavaBlocks = new Dictionary<byte, List<BlockTypeClassification>>();

	[NonSerialized]
	private bool isInitialized;

	[AssertNotNull]
	public TextAsset blockTypeClassifications;

	[AssertNotNull]
	public string lavaMaterial;

	public Dictionary<byte, List<BlockTypeClassification>> _lavaBlocks => lavaBlocks;

	public bool _isInitialized => isInitialized;

	private void LazyInitialize()
	{
		if (isInitialized)
		{
			return;
		}
		foreach (BlockTypeClassification item in CSVUtils.Read<BlockTypeClassification>(blockTypeClassifications.bytes))
		{
			if (string.Equals(lavaMaterial, item.material, StringComparison.OrdinalIgnoreCase))
			{
				lavaBlocks.GetOrAddNew((byte)item.blockType).Add(item);
			}
		}
		isInitialized = true;
	}

	public bool IsLava(byte blockType, float inclination)
	{
		LazyInitialize();
		foreach (BlockTypeClassification item in lavaBlocks.GetOrDefault(blockType, emptyList))
		{
			if (item.ContainsInclination(inclination))
			{
				return true;
			}
		}
		return false;
	}
}
