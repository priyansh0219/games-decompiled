using UnityEngine;

[RequireComponent(typeof(OnSurfaceTracker))]
public class OnSurfaceDownforce : MonoBehaviour
{
	public OnSurfaceTracker onSurfaceTracker;

	public Rigidbody useRigidbody;

	public WorldForces worldForces;

	public float force = 10f;

	public bool disableGravity = true;

	private bool useGravity;

	private void Awake()
	{
		useGravity = worldForces != null && worldForces.handleGravity;
	}

	private void FixedUpdate()
	{
		SetOnSurfaceState(onSurfaceTracker.onSurface);
		if (onSurfaceTracker.onSurface)
		{
			useRigidbody.AddForce(-onSurfaceTracker.surfaceNormal * force, ForceMode.Acceleration);
		}
	}

	private void SetOnSurfaceState(bool state)
	{
		if (useGravity && disableGravity)
		{
			worldForces.handleGravity = !state;
		}
	}

	public void OnKill()
	{
		base.enabled = false;
		SetOnSurfaceState(state: false);
	}
}
