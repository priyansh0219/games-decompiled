using UnityEngine;

internal class VehicleAccelerationModifier : MonoBehaviour
{
	[Range(0f, 1f)]
	public float accelerationMultiplier = 0.5f;

	public virtual void ModifyAcceleration(ref Vector3 acceleration)
	{
		acceleration *= accelerationMultiplier;
	}
}
