using System;
using System.Collections.Generic;
using Gendarme;
using UWE;
using UnityEngine;

[SuppressMessage("Gendarme.Rules.Portability", "DoNotHardcodePathsRule")]
public class MaterialDatabase : MonoBehaviour
{
	private static readonly List<BlockTypeClassification> emptyList = new List<BlockTypeClassification>();

	private static readonly Dictionary<byte, List<BlockTypeClassification>> blocks = new Dictionary<byte, List<BlockTypeClassification>>();

	private const string lavaMaterial = "lava";

	[AssertNotNull]
	public TextAsset blockTypeClassifications;

	public void Start()
	{
		try
		{
			foreach (BlockTypeClassification item in CSVUtils.Read<BlockTypeClassification>(blockTypeClassifications.bytes))
			{
				blocks.GetOrAddNew((byte)item.blockType).Add(item);
			}
		}
		catch
		{
		}
	}

	public static string GetTerrainMaterial(Vector3 position, Vector3 normal)
	{
		string result = null;
		LargeWorldStreamer main = LargeWorldStreamer.main;
		if (main != null && main.IsReady())
		{
			byte b = 0;
			for (int i = 0; i < 4; i++)
			{
				b = main.GetBlockType(position);
				if (b != 0)
				{
					break;
				}
				position -= normal * 0.5f;
			}
			if (b != 0)
			{
				List<BlockTypeClassification> orDefault = blocks.GetOrDefault(b, emptyList);
				float inclination = Vector3.Angle(normal, Vector3.up);
				foreach (BlockTypeClassification item in orDefault)
				{
					if (item.ContainsInclination(inclination))
					{
						result = item.material;
						break;
					}
				}
			}
		}
		return result;
	}

	public static bool IsLava(Vector3 position, Vector3 normal)
	{
		return string.Equals(GetTerrainMaterial(position, normal), "lava", StringComparison.OrdinalIgnoreCase);
	}
}
