using UnityEngine;

public interface IGroundMoveable
{
	Vector3 GetVelocity();

	bool IsOnGround();

	bool IsUnderwater();

	bool IsActive();

	VFXSurfaceTypes GetGroundSurfaceType();

	Vector3 GetGroundNormal();
}
