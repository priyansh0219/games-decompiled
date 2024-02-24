using UnityEngine;

public struct Coords
{
	public Vector3 origin;

	public Vector3 xAxis;

	public Vector3 yAxis;

	public Vector3 zAxis;

	public Coords(Vector3 _xAxis, Vector3 _yAxis, Vector3 _zAxis, Vector3 _origin)
	{
		xAxis = _xAxis;
		yAxis = _yAxis;
		zAxis = _zAxis;
		origin = _origin;
	}

	public Vector3 TransformPoint(Vector3 v)
	{
		Vector3 result = default(Vector3);
		result.x = v.x * xAxis.x + v.y * yAxis.x + v.z * zAxis.x + origin.x;
		result.y = v.x * xAxis.y + v.y * yAxis.y + v.z * zAxis.y + origin.y;
		result.z = v.x * xAxis.z + v.y * yAxis.z + v.z * zAxis.z + origin.z;
		return result;
	}

	public Vector3 TransformVector(Vector3 v)
	{
		Vector3 result = default(Vector3);
		result.x = v.x * xAxis.x + v.y * yAxis.x + v.z * zAxis.x;
		result.y = v.x * xAxis.y + v.y * yAxis.y + v.z * zAxis.y;
		result.z = v.x * xAxis.z + v.y * yAxis.z + v.z * zAxis.z;
		return result;
	}

	public static Coords operator *(Coords b, Coords c)
	{
		Coords result = default(Coords);
		result.xAxis.x = b.xAxis.x * c.xAxis.x + b.yAxis.x * c.xAxis.y + b.zAxis.x * c.xAxis.z;
		result.xAxis.y = b.xAxis.y * c.xAxis.x + b.yAxis.y * c.xAxis.y + b.zAxis.y * c.xAxis.z;
		result.xAxis.z = b.xAxis.z * c.xAxis.x + b.yAxis.z * c.xAxis.y + b.zAxis.z * c.xAxis.z;
		result.yAxis.x = b.xAxis.x * c.yAxis.x + b.yAxis.x * c.yAxis.y + b.zAxis.x * c.yAxis.z;
		result.yAxis.y = b.xAxis.y * c.yAxis.x + b.yAxis.y * c.yAxis.y + b.zAxis.y * c.yAxis.z;
		result.yAxis.z = b.xAxis.z * c.yAxis.x + b.yAxis.z * c.yAxis.y + b.zAxis.z * c.yAxis.z;
		result.zAxis.x = b.xAxis.x * c.zAxis.x + b.yAxis.x * c.zAxis.y + b.zAxis.x * c.zAxis.z;
		result.zAxis.y = b.xAxis.y * c.zAxis.x + b.yAxis.y * c.zAxis.y + b.zAxis.y * c.zAxis.z;
		result.zAxis.z = b.xAxis.z * c.zAxis.x + b.yAxis.z * c.zAxis.y + b.zAxis.z * c.zAxis.z;
		result.origin.x = b.xAxis.x * c.origin.x + b.yAxis.x * c.origin.y + b.zAxis.x * c.origin.z + b.origin.x;
		result.origin.y = b.xAxis.y * c.origin.x + b.yAxis.y * c.origin.y + b.zAxis.y * c.origin.z + b.origin.y;
		result.origin.z = b.xAxis.z * c.origin.x + b.yAxis.z * c.origin.y + b.zAxis.z * c.origin.z + b.origin.z;
		return result;
	}

	public Coords GetInverse()
	{
		Coords result = default(Coords);
		result.xAxis.x = xAxis.x;
		result.xAxis.y = yAxis.x;
		result.xAxis.z = zAxis.x;
		result.yAxis.x = xAxis.y;
		result.yAxis.y = yAxis.y;
		result.yAxis.z = zAxis.y;
		result.zAxis.x = xAxis.z;
		result.zAxis.y = yAxis.z;
		result.zAxis.z = zAxis.z;
		result.origin = -result.TransformVector(origin);
		return result;
	}
}
