using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCuller : MonoBehaviour
{
	[SerializeField]
	private int numSpheres = 1;

	[SerializeField]
	private Vector3 sphereStartOffset;

	[SerializeField]
	private int sphereRadius;

	[SerializeField]
	private int sphereSeparation;

	[SerializeField]
	private Vector3 sphereSeparationDirection = new Vector3(0f, -1f, 0f);

	private CullingGroup cullingGroup;

	private List<ParticleSystem> particleSystems = new List<ParticleSystem>();

	private List<ParticleSystemRenderer> particleRenderers = new List<ParticleSystemRenderer>();

	private BoundingSphere[] boundingSpheres;

	private bool positionSettled;

	private float cachedSphereRadius;

	public bool isCulled { get; private set; }

	private void Start()
	{
		if (cullingGroup == null && MainCameraV2.main != null)
		{
			cullingGroup = new CullingGroup();
			cullingGroup.targetCamera = MainCamera.camera;
			boundingSpheres = new BoundingSphere[numSpheres];
			UpdateBoundingSpheres();
			cullingGroup.SetBoundingSpheres(boundingSpheres);
			cullingGroup.SetBoundingSphereCount(numSpheres);
			CullingGroup obj = cullingGroup;
			obj.onStateChanged = (CullingGroup.StateChanged)Delegate.Combine(obj.onStateChanged, new CullingGroup.StateChanged(OnStateChanged));
			GetComponentsInChildren(particleSystems);
			GetComponentsInChildren(particleRenderers);
			Cull(AnySpheresVisible());
		}
		cullingGroup.enabled = true;
	}

	private void UpdateBoundingSpheres()
	{
		cachedSphereRadius = (float)sphereRadius * Mathf.Max(base.transform.lossyScale.x, base.transform.lossyScale.y, base.transform.lossyScale.z);
		for (int i = 0; i < numSpheres; i++)
		{
			boundingSpheres[i] = new BoundingSphere(base.transform.TransformPoint(sphereStartOffset + sphereSeparationDirection * i * sphereSeparation), cachedSphereRadius);
		}
	}

	private bool AnySpheresVisible()
	{
		for (int i = 0; i < numSpheres; i++)
		{
			if (cullingGroup.IsVisible(i))
			{
				return true;
			}
		}
		return false;
	}

	private void OnEnable()
	{
		if (cullingGroup != null)
		{
			cullingGroup.enabled = true;
			UpdateBoundingSpheres();
			Cull(AnySpheresVisible());
		}
	}

	private void OnDisable()
	{
		if (cullingGroup != null)
		{
			cullingGroup.enabled = false;
			Cull(visible: true);
		}
	}

	private void OnDestroy()
	{
		if (cullingGroup != null)
		{
			cullingGroup.Dispose();
		}
	}

	private void OnStateChanged(CullingGroupEvent sphere)
	{
		Cull(AnySpheresVisible());
	}

	private void Cull(bool visible)
	{
		isCulled = !visible;
		foreach (ParticleSystem particleSystem in particleSystems)
		{
			if (visible)
			{
				particleSystem.Play(withChildren: true);
			}
			else
			{
				particleSystem.Pause(withChildren: true);
			}
		}
		foreach (ParticleSystemRenderer particleRenderer in particleRenderers)
		{
			particleRenderer.enabled = visible;
		}
	}

	private void Update()
	{
		if (!positionSettled && Vector3.Distance(Vector3.zero, base.transform.position) > 0.1f)
		{
			positionSettled = true;
			UpdateBoundingSpheres();
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (base.enabled)
		{
			for (int i = 0; i < numSpheres; i++)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawWireSphere(base.transform.TransformPoint(sphereStartOffset + sphereSeparationDirection * i * sphereSeparation), cachedSphereRadius);
			}
		}
	}
}
