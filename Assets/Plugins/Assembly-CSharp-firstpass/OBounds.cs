using UnityEngine;

public struct OBounds
{
	public Vector3 extents;

	public Vector3 center;

	public Vector3 xAxis;

	public Vector3 yAxis;

	public Vector3 zAxis;

	public OBounds(Transform transform, Bounds osBounds)
	{
		extents = osBounds.extents;
		center = transform.TransformPoint(osBounds.center);
		xAxis = transform.right;
		yAxis = transform.up;
		zAxis = transform.forward;
	}

	public bool Intersects(OBounds bounds)
	{
		Coords t = bounds.GetCoords().GetInverse() * GetCoords();
		return Intersects(bounds.extents, extents, t);
	}

	public bool Intersects(Bounds bounds)
	{
		Coords coords = GetCoords();
		coords.origin -= bounds.center;
		return Intersects(bounds.extents, extents, coords);
	}

	private Coords GetCoords()
	{
		return new Coords(xAxis, yAxis, zAxis, center);
	}

	private static bool Intersects(Vector3 AExtents, Vector3 BExtents, Coords T)
	{
		Vector3 origin = T.origin;
		Vector3 rhs = new Vector3(Mathf.Abs(T.xAxis.x) + Mathf.Epsilon, Mathf.Abs(T.xAxis.y) + Mathf.Epsilon, Mathf.Abs(T.xAxis.z) + Mathf.Epsilon);
		Vector3 rhs2 = new Vector3(Mathf.Abs(T.yAxis.x) + Mathf.Epsilon, Mathf.Abs(T.yAxis.y) + Mathf.Epsilon, Mathf.Abs(T.yAxis.z) + Mathf.Epsilon);
		Vector3 rhs3 = new Vector3(Mathf.Abs(T.zAxis.x) + Mathf.Epsilon, Mathf.Abs(T.zAxis.y) + Mathf.Epsilon, Mathf.Abs(T.zAxis.z) + Mathf.Epsilon);
		float x = origin.x;
		float x2 = AExtents.x;
		float num = BExtents.x * rhs.x + BExtents.y * rhs2.x + BExtents.z * rhs3.x;
		if (Mathf.Abs(x) > x2 + num)
		{
			return false;
		}
		float y = origin.y;
		x2 = AExtents.y;
		num = BExtents.x * rhs.y + BExtents.y * rhs2.y + BExtents.z * rhs3.y;
		if (Mathf.Abs(y) > x2 + num)
		{
			return false;
		}
		float z = origin.z;
		x2 = AExtents.z;
		num = BExtents.x * rhs.z + BExtents.y * rhs2.z + BExtents.z * rhs3.z;
		if (Mathf.Abs(z) > x2 + num)
		{
			return false;
		}
		float f = Vector3.Dot(origin, rhs);
		x2 = AExtents.x * rhs.x + AExtents.y * rhs.y + AExtents.z * rhs.z;
		num = BExtents.x;
		if (Mathf.Abs(f) > x2 + num)
		{
			return false;
		}
		float f2 = Vector3.Dot(origin, rhs2);
		x2 = AExtents.x * rhs2.x + AExtents.y * rhs2.y + AExtents.z * rhs2.z;
		num = BExtents.y;
		if (Mathf.Abs(f2) > x2 + num)
		{
			return false;
		}
		float f3 = Vector3.Dot(origin, rhs3);
		x2 = AExtents.x * rhs3.x + AExtents.y * rhs3.y + AExtents.z * rhs3.z;
		num = BExtents.z;
		if (Mathf.Abs(f3) > x2 + num)
		{
			return false;
		}
		float f4 = origin.z * rhs.y - origin.y * rhs.z;
		x2 = AExtents.y * rhs.z + AExtents.z * rhs.y;
		num = BExtents.y * rhs3.x + BExtents.z * rhs2.x;
		if (Mathf.Abs(f4) > x2 + num)
		{
			return false;
		}
		float f5 = origin.z * rhs2.y - origin.y * rhs2.z;
		x2 = AExtents.y * rhs2.z + AExtents.z * rhs2.y;
		num = BExtents.z * rhs.x + BExtents.x * rhs3.x;
		if (Mathf.Abs(f5) > x2 + num)
		{
			return false;
		}
		float f6 = origin.z * rhs3.y - origin.y * rhs3.z;
		x2 = AExtents.y * rhs3.z + AExtents.z * rhs3.y;
		num = BExtents.x * rhs2.x + BExtents.y * rhs.x;
		if (Mathf.Abs(f6) > x2 + num)
		{
			return false;
		}
		float f7 = origin.x * rhs.z - origin.z * rhs.x;
		x2 = AExtents.z * rhs.x + AExtents.x * rhs.z;
		num = BExtents.y * rhs3.y + BExtents.z * rhs2.y;
		if (Mathf.Abs(f7) > x2 + num)
		{
			return false;
		}
		float f8 = origin.x * rhs2.z - origin.z * rhs2.x;
		x2 = AExtents.z * rhs2.x + AExtents.x * rhs2.z;
		num = BExtents.z * rhs.y + BExtents.x * rhs3.y;
		if (Mathf.Abs(f8) > x2 + num)
		{
			return false;
		}
		float f9 = origin.x * rhs3.z - origin.z * rhs3.x;
		x2 = AExtents.z * rhs3.x + AExtents.x * rhs3.z;
		num = BExtents.x * rhs2.y + BExtents.y * rhs.y;
		if (Mathf.Abs(f9) > x2 + num)
		{
			return false;
		}
		float f10 = origin.y * rhs.x - origin.x * rhs.y;
		x2 = AExtents.x * rhs.y + AExtents.y * rhs.x;
		num = BExtents.y * rhs3.z + BExtents.z * rhs2.z;
		if (Mathf.Abs(f10) > x2 + num)
		{
			return false;
		}
		float f11 = origin.y * rhs2.x - origin.x * rhs2.y;
		x2 = AExtents.x * rhs2.y + AExtents.y * rhs2.x;
		num = BExtents.z * rhs.z + BExtents.x * rhs3.z;
		if (Mathf.Abs(f11) > x2 + num)
		{
			return false;
		}
		float f12 = origin.y * rhs3.x - origin.x * rhs3.y;
		x2 = AExtents.x * rhs3.y + AExtents.y * rhs3.x;
		num = BExtents.x * rhs2.z + BExtents.y * rhs.z;
		if (Mathf.Abs(f12) > x2 + num)
		{
			return false;
		}
		return true;
	}
}
