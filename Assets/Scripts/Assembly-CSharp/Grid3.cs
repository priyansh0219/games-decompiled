using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class Grid3<T>
{
	public delegate T BlendFunction(T a, T b, float t);

	[NonSerialized]
	[ProtoMember(1)]
	public Grid3Shape shape;

	[NonSerialized]
	[ProtoMember(2, OverwriteList = true)]
	public T[] values;

	public T[] Values => values;

	public Grid3Shape Shape => shape;

	public int SizeX => shape.x;

	public int SizeY => shape.y;

	public int SizeZ => shape.z;

	public Grid3()
	{
	}

	public Grid3(Grid3Shape shape)
	{
		this.shape = shape;
		values = new T[shape.Size];
	}

	public T GetValue(int index, T defaultValue)
	{
		if (index == -1)
		{
			return defaultValue;
		}
		return values[index];
	}

	public T SampleAt(Vector3 pos, T defaultValue, BlendFunction blendFunction)
	{
		Int3 @int = Int3.Floor(pos);
		Int3 int2 = Int3.Ceil(pos);
		Vector3 vector = pos - @int.ToVector3();
		T value = GetValue(shape.GetIndex(@int.x, @int.y, @int.z), defaultValue);
		T value2 = GetValue(shape.GetIndex(int2.x, @int.y, @int.z), defaultValue);
		T value3 = GetValue(shape.GetIndex(@int.x, int2.y, @int.z), defaultValue);
		T value4 = GetValue(shape.GetIndex(int2.x, int2.y, @int.z), defaultValue);
		T value5 = GetValue(shape.GetIndex(@int.x, @int.y, int2.z), defaultValue);
		T value6 = GetValue(shape.GetIndex(int2.x, @int.y, int2.z), defaultValue);
		T value7 = GetValue(shape.GetIndex(@int.x, int2.y, int2.z), defaultValue);
		T value8 = GetValue(shape.GetIndex(int2.x, int2.y, int2.z), defaultValue);
		T a = blendFunction(value, value2, vector.x);
		T b = blendFunction(value3, value4, vector.x);
		T a2 = blendFunction(value5, value6, vector.x);
		T b2 = blendFunction(value7, value8, vector.x);
		T a3 = blendFunction(a, b, vector.y);
		T b3 = blendFunction(a2, b2, vector.y);
		return blendFunction(a3, b3, vector.z);
	}
}
