using UnityEngine;

public class VFXSurfaceTypeManager : MonoBehaviour
{
	private Quaternion defaultOrientation = Quaternion.Euler(-90f, 0f, 0f);

	public VFXSurfaceTypeDatabase database;

	private static VFXSurfaceTypeManager _main;

	public static VFXSurfaceTypeManager main
	{
		get
		{
			if (_main == null)
			{
				_main = Object.FindObjectOfType<VFXSurfaceTypeManager>();
			}
			return _main;
		}
	}

	public GameObject GetFXprefab(VFXSurfaceTypes surfaceType, VFXEventTypes eventType)
	{
		if (surfaceType == VFXSurfaceTypes.none)
		{
			return null;
		}
		if (database == null)
		{
			return null;
		}
		GameObject prefab = database.GetPrefab(surfaceType, eventType);
		if (prefab == null)
		{
			prefab = database.GetPrefab(VFXSurfaceTypes.fallback, eventType);
		}
		return prefab;
	}

	public ParticleSystem Play(VFXSurfaceTypes surfaceType, VFXEventTypes eventType, Vector3 position, Quaternion orientation, Transform parent)
	{
		ParticleSystem particleSystem = null;
		GameObject fXprefab = GetFXprefab(surfaceType, eventType);
		if (fXprefab != null)
		{
			GameObject gameObject = Object.Instantiate(fXprefab, position, orientation);
			if (eventType == VFXEventTypes.exoDrill)
			{
				gameObject.transform.parent = null;
				gameObject.GetComponent<VFXFakeParent>().Parent(parent, Vector3.zero, Vector3.zero);
				gameObject.GetComponent<VFXLateTimeParticles>().Play();
				particleSystem = gameObject.GetComponent<ParticleSystem>();
			}
			else
			{
				gameObject.transform.parent = parent;
				particleSystem = gameObject.GetComponent<ParticleSystem>();
				particleSystem.Play();
			}
		}
		return particleSystem;
	}

	public ParticleSystem Play(VFXSurfaceTypes surfaceType, VFXEventTypes eventType, Vector3 position, Transform parent)
	{
		return Play(surfaceType, eventType, position, defaultOrientation, parent);
	}

	public ParticleSystem Play(VFXSurfaceTypes surfaceType, VFXEventTypes eventType, Vector3 position)
	{
		return Play(surfaceType, eventType, position, defaultOrientation, null);
	}

	public ParticleSystem Play(VFXSurface surface, VFXEventTypes eventType, Vector3 position, Quaternion orientation, Transform parent)
	{
		VFXSurfaceTypes surfaceType = VFXSurfaceTypes.fallback;
		if (surface != null)
		{
			surfaceType = surface.surfaceType;
		}
		return Play(surfaceType, eventType, position, orientation, parent);
	}

	public ParticleSystem Play(VFXSurface surface, VFXEventTypes eventType, Vector3 position, Transform parent)
	{
		VFXSurfaceTypes surfaceType = VFXSurfaceTypes.fallback;
		if (surface != null)
		{
			surfaceType = surface.surfaceType;
		}
		return Play(surfaceType, eventType, position, defaultOrientation, parent);
	}

	public ParticleSystem Play(VFXSurface surface, VFXEventTypes eventType, Vector3 position)
	{
		VFXSurfaceTypes surfaceType = VFXSurfaceTypes.fallback;
		if (surface != null)
		{
			surfaceType = surface.surfaceType;
		}
		return Play(surfaceType, eventType, position, defaultOrientation, null);
	}
}
