using System;
using UnityEngine;

public class OnSurfaceTracker : MonoBehaviour
{
	protected bool _onSurface;

	protected Vector3 _surfacePoint;

	protected Vector3 _surfaceNormal;

	[Range(0f, 180f)]
	public float maxSurfaceAngle = 60f;

	protected float minSurfaceCos;

	public bool onSurface => _onSurface;

	public Vector3 lastSurfacePoint => _surfacePoint;

	public Vector3 surfaceNormal
	{
		get
		{
			if (!_onSurface)
			{
				return Vector3.up;
			}
			return _surfaceNormal;
		}
	}

	private void Awake()
	{
		minSurfaceCos = Mathf.Cos((float)Math.PI / 180f * maxSurfaceAngle);
	}

	private void FixedUpdate()
	{
		_onSurface = false;
	}

	private void OnCollisionStay(Collision collision)
	{
		if (base.enabled && IsValidSurface(collision))
		{
			_onSurface = true;
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		_onSurface = false;
	}

	private void OnDisable()
	{
		_onSurface = false;
	}

	protected virtual bool IsValidSurface(Collision collisionInfo)
	{
		if (collisionInfo.gameObject.CompareTag("Creature"))
		{
			return false;
		}
		if (collisionInfo.gameObject == Utils.GetLocalPlayer())
		{
			return false;
		}
		ContactPoint[] contacts = collisionInfo.contacts;
		for (int i = 0; i < contacts.Length; i++)
		{
			ContactPoint contactPoint = contacts[i];
			if (!(contactPoint.normal.y < minSurfaceCos))
			{
				_surfaceNormal = contactPoint.normal;
				_surfacePoint = contactPoint.point;
				return true;
			}
		}
		return false;
	}
}
