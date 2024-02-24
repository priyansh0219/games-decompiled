using UnityEngine;

public class DistanceFieldGrid : IVoxelGrid
{
	private readonly Matrix4x4 transformation;

	private readonly Matrix4x4 inverseTransformation;

	private readonly float[,,] distanceField;

	private readonly Int3 distanceFieldSize;

	private readonly byte blockType;

	public static DistanceFieldGrid Create(float[,,] distanceField, Bounds distanceFieldBounds, byte blockType, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		return new DistanceFieldGrid(GetTransformation(new Int3(distanceField.GetLength(0), distanceField.GetLength(1), distanceField.GetLength(2)), distanceFieldBounds, position, rotation, scale), distanceField, blockType);
	}

	public static Matrix4x4 GetTransformation(Int3 fieldSize, Bounds fieldBounds, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		Vector3 s = Int3.Div(fieldBounds.size, fieldSize);
		Matrix4x4 matrix4x = Matrix4x4.TRS(fieldBounds.min, Quaternion.identity, s);
		return Matrix4x4.TRS(position, rotation, scale) * matrix4x;
	}

	public DistanceFieldGrid(Matrix4x4 transformation, float[,,] distanceField, byte blockType)
	{
		this.transformation = transformation;
		inverseTransformation = transformation.inverse;
		this.distanceField = distanceField;
		distanceFieldSize = new Int3(distanceField.GetLength(0), distanceField.GetLength(1), distanceField.GetLength(2));
		this.blockType = blockType;
	}

	public byte GetBlockType()
	{
		return blockType;
	}

	public VoxelandData.OctNode GetVoxel(int x, int y, int z)
	{
		Vector3 position = GetPosition(x, y, z);
		float num = 0f - GetDistance(position);
		return new VoxelandData.OctNode((byte)((num >= 0f) ? blockType : 0), VoxelandData.OctNode.EncodeDensity(num));
	}

	private Vector3 GetPosition(int x, int y, int z)
	{
		return inverseTransformation.MultiplyPoint3x4(new Vector3(x, y, z));
	}

	private float GetDistance(Vector3 pos)
	{
		Vector3 vector = pos - pos.Floor();
		Int3 @int = Int3.Floor(pos);
		Int3 int2 = @int + Int3.one;
		Int3 int3 = @int.Clamp(Int3.zero, distanceFieldSize - Int3.one);
		Int3 int4 = int2.Clamp(Int3.zero, distanceFieldSize - Int3.one);
		float a = distanceField[int3.x, int3.y, int3.z];
		float a2 = distanceField[int3.x, int3.y, int4.z];
		float a3 = distanceField[int3.x, int4.y, int3.z];
		float a4 = distanceField[int3.x, int4.y, int4.z];
		float b = distanceField[int4.x, int3.y, int3.z];
		float b2 = distanceField[int4.x, int3.y, int4.z];
		float b3 = distanceField[int4.x, int4.y, int3.z];
		float b4 = distanceField[int4.x, int4.y, int4.z];
		float a5 = Mathf.Lerp(a, b, vector.x);
		float a6 = Mathf.Lerp(a2, b2, vector.x);
		float b5 = Mathf.Lerp(a3, b3, vector.x);
		float b6 = Mathf.Lerp(a4, b4, vector.x);
		float a7 = Mathf.Lerp(a5, b5, vector.y);
		float b7 = Mathf.Lerp(a6, b6, vector.y);
		return Mathf.Lerp(a7, b7, vector.z);
	}

	public bool GetVoxelMask(int x, int y, int z)
	{
		Int3 @int = Int3.Floor(GetPosition(x, y, z));
		if (@int >= Int3.zero)
		{
			return @int < distanceFieldSize;
		}
		return false;
	}

	public Int3.Bounds GetBounds()
	{
		Vector3 vector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 vector2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		foreach (Int3 item in Int3.Range(Int3.zero, Int3.one))
		{
			Vector3 point = (item * distanceFieldSize).ToVector3();
			Vector3 rhs = transformation.MultiplyPoint3x4(point);
			vector = Vector3.Min(vector, rhs);
			vector2 = Vector3.Max(vector2, rhs);
		}
		return new Int3.Bounds(Int3.Floor(vector), Int3.Ceil(vector2));
	}
}
