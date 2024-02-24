using System.IO;
using UWE;
using UnityEngine;

[CreateAssetMenu(fileName = "NewVoxelandBrush.asset", menuName = "Voxeland/Create Brush Asset")]
public class VoxelandBrush : ScriptableObject
{
	private const int fileVersion = 2;

	public Mesh mesh;

	public TextAsset distanceField;

	public float[,,] distanceValues { get; private set; }

	public Bounds bounds { get; private set; }

	public void Load()
	{
		distanceValues = Load(distanceField, out var min, out var max, out var _);
		bounds = Utils.MinMaxBounds(min, max);
	}

	public DistanceFieldGrid CreateGrid(Vector3 position, Quaternion rotation, Vector3 scale, byte blockType)
	{
		if (distanceValues == null)
		{
			Load();
		}
		return DistanceFieldGrid.Create(distanceValues, bounds, blockType, position, rotation, scale);
	}

	public static float[,,] Load(TextAsset asset, out Vector3 min, out Vector3 max, out Vector3 meshBoundsSize)
	{
		using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(asset.bytes)))
		{
			int num = binaryReader.ReadInt32();
			int num2 = binaryReader.ReadInt32();
			int num3 = binaryReader.ReadInt32();
			int num4 = binaryReader.ReadInt32();
			min.x = binaryReader.ReadSingle();
			min.y = binaryReader.ReadSingle();
			min.z = binaryReader.ReadSingle();
			max.x = binaryReader.ReadSingle();
			max.y = binaryReader.ReadSingle();
			max.z = binaryReader.ReadSingle();
			meshBoundsSize = Vector3.zero;
			if (num > 1)
			{
				meshBoundsSize.x = binaryReader.ReadSingle();
				meshBoundsSize.y = binaryReader.ReadSingle();
				meshBoundsSize.z = binaryReader.ReadSingle();
			}
			float[,,] array = new float[num2, num3, num4];
			for (int i = 0; i < num4; i++)
			{
				for (int j = 0; j < num3; j++)
				{
					for (int k = 0; k < num2; k++)
					{
						array[k, j, i] = binaryReader.ReadSingle();
					}
				}
			}
			return array;
		}
	}

	public static void Save(string fileName, float[,,] distanceField, Vector3 min, Vector3 max, Vector3 meshBoundsSize)
	{
		using (BinaryWriter binaryWriter = new BinaryWriter(FileUtils.CreateFile(fileName)))
		{
			int length = distanceField.GetLength(0);
			int length2 = distanceField.GetLength(1);
			int length3 = distanceField.GetLength(2);
			binaryWriter.Write(2);
			binaryWriter.Write(length);
			binaryWriter.Write(length2);
			binaryWriter.Write(length3);
			binaryWriter.Write(min.x);
			binaryWriter.Write(min.y);
			binaryWriter.Write(min.z);
			binaryWriter.Write(max.x);
			binaryWriter.Write(max.y);
			binaryWriter.Write(max.z);
			binaryWriter.Write(meshBoundsSize.x);
			binaryWriter.Write(meshBoundsSize.y);
			binaryWriter.Write(meshBoundsSize.z);
			for (int i = 0; i < length3; i++)
			{
				for (int j = 0; j < length2; j++)
				{
					for (int k = 0; k < length; k++)
					{
						binaryWriter.Write(distanceField[k, j, i]);
					}
				}
			}
			binaryWriter.Close();
		}
	}
}
