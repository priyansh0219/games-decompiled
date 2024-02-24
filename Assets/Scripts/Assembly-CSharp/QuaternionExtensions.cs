using UnityEngine;

public static class QuaternionExtensions
{
	public static bool IsDistinguishedIdentity(this Quaternion q)
	{
		if (q.x == 0f && q.y == 0f && q.z == 0f)
		{
			return q.w == 0f;
		}
		return false;
	}
}
