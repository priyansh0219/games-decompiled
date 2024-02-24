using UnityEngine;

public class VFXSurface : MonoBehaviour
{
	public VFXSurfaceTypes surfaceType;

	public void Play(VFXEventTypes eventType, Vector3 position, Quaternion orientation, Transform parent)
	{
		VFXSurfaceTypeManager.main.Play(surfaceType, eventType, position, orientation, parent);
	}

	public void Play(VFXEventTypes eventType, Vector3 position, Transform parent)
	{
		VFXSurfaceTypeManager.main.Play(surfaceType, eventType, position, parent);
	}

	public void Play(VFXEventTypes eventType, Vector3 position)
	{
		VFXSurfaceTypeManager.main.Play(surfaceType, eventType, position);
	}
}
