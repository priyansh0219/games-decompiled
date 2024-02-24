using UnityEngine;

public class VoxelandIntersections : MonoBehaviour
{
	public Transform lineStartTfm;

	public Transform lineEndTfm;

	public Transform posTfm0;

	public Transform posTfm1;

	public Transform posTfm2;

	public Transform posTfm3;

	public static Matrix4x4 GetVectorMatrix(Vector3 vec)
	{
		Quaternion q = default(Quaternion);
		q.SetLookRotation(vec);
		Matrix4x4 matrix4x = default(Matrix4x4);
		matrix4x.SetTRS(new Vector3(0f, 0f, 0f), q, new Vector3(1f, 1f, 1f));
		return matrix4x.inverse;
	}

	public static bool IsOnLeft(Vector3 a, Vector3 b, Vector3 p)
	{
		return (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x) > 0f;
	}

	public static bool IsOnLeftXZ(Vector3 a, Vector3 b, Vector3 p)
	{
		return (b.z - a.z) * (p.x - a.x) - (b.x - a.x) * (p.z - a.z) > 0f;
	}

	public static bool IsZeroOnLeft(Vector3 a, Vector3 b)
	{
		return (b.x - a.x) * a.y - (b.y - a.y) * a.x > 0f;
	}

	public static Vector3 GetBaryCoords(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 from)
	{
		Vector3 result = new Vector3(((p2.y - p3.y) * (from.x - p3.x) + (p3.x - p2.x) * (from.y - p3.y)) / ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y)), ((p3.y - p1.y) * (from.x - p3.x) + (p1.x - p3.x) * (from.y - p3.y)) / ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y)), 0f);
		result.z = 1f - result.x - result.y;
		return result;
	}

	public void OnDrawGizmos()
	{
		Gizmos.DrawLine(lineStartTfm.position, lineEndTfm.position);
		Ray ray = new Ray(lineStartTfm.position, lineStartTfm.position - lineEndTfm.position);
		Matrix4x4 vectorMatrix = GetVectorMatrix(ray.direction.normalized);
		Vector3 vector = vectorMatrix.MultiplyPoint3x4(posTfm0.position - ray.origin);
		Vector3 vector2 = vectorMatrix.MultiplyPoint3x4(posTfm1.position - ray.origin);
		Vector3 vector3 = vectorMatrix.MultiplyPoint3x4(posTfm2.position - ray.origin);
		Vector3 vector4 = vectorMatrix.MultiplyPoint3x4(posTfm3.position - ray.origin);
		Gizmos.color = Color.red;
		if (IsZeroOnLeft(vector, vector2) && IsZeroOnLeft(vector2, vector3) && IsZeroOnLeft(vector3, vector4) && IsZeroOnLeft(vector4, vector))
		{
			Gizmos.color = Color.green;
		}
		Gizmos.DrawLine(posTfm0.position, posTfm1.position);
		Gizmos.DrawLine(posTfm1.position, posTfm2.position);
		Gizmos.DrawLine(posTfm2.position, posTfm3.position);
		Gizmos.DrawLine(posTfm3.position, posTfm0.position);
		Gizmos.DrawLine(posTfm0.position, posTfm2.position);
		Gizmos.color = Color.gray;
		Gizmos.DrawLine(vector, vector2);
		Gizmos.DrawLine(vector2, vector3);
		Gizmos.DrawLine(vector3, vector4);
		Gizmos.DrawLine(vector4, vector);
		Gizmos.DrawLine(vector, vector3);
	}
}
