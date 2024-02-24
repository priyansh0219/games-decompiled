using UnityEngine;

public static class CameraExtensions
{
	public static Plane[] GetSharedFrustumPlanes(this Camera camera)
	{
		return CameraUtils.GetSharedFrustumPlanes(camera);
	}
}
