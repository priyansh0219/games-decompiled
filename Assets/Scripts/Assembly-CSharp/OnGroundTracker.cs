using UnityEngine;

public class OnGroundTracker : OnSurfaceTracker
{
	public Vector3 lastSurfaceNormal => _surfaceNormal;

	protected override bool IsValidSurface(Collision collisionInfo)
	{
		bool result = false;
		ContactPoint[] contacts = collisionInfo.contacts;
		GameObject gameObject = collisionInfo.collider.gameObject;
		if (gameObject != null && gameObject.layer == LayerID.TerrainCollider)
		{
			for (int i = 0; i < contacts.Length; i++)
			{
				if (Vector3.Dot(Vector3.up, contacts[i].normal) >= minSurfaceCos)
				{
					result = true;
					_surfaceNormal = contacts[i].normal;
					_surfacePoint = contacts[i].point;
					break;
				}
			}
		}
		return result;
	}
}
