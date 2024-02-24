using System.Collections.Generic;
using UnityEngine;

public class MountingBounds : MonoBehaviour
{
	private static List<Collider> sCollidersList = new List<Collider>();

	public OrientedBounds bounds;

	public bool IsMounted()
	{
		bool result = false;
		OrientedBounds orientedBounds = bounds;
		if (orientedBounds.rotation.IsDistinguishedIdentity())
		{
			orientedBounds.rotation = Quaternion.identity;
		}
		orientedBounds.position = base.transform.position + base.transform.rotation * orientedBounds.position;
		orientedBounds.rotation = base.transform.rotation * orientedBounds.rotation;
		Builder.GetOverlappedColliders(orientedBounds.position, orientedBounds.rotation, orientedBounds.extents, sCollidersList);
		for (int i = 0; i < sCollidersList.Count; i++)
		{
			Collider collider = sCollidersList[i];
			if (IsMountTarget(collider))
			{
				result = true;
				break;
			}
		}
		sCollidersList.Clear();
		return result;
	}

	private bool IsMountTarget(Collider collider)
	{
		if (collider != null && collider.gameObject.layer == LayerID.TerrainCollider)
		{
			return true;
		}
		return false;
	}

	private void OnDrawGizmosSelected()
	{
		Color color = ((bounds.extents.x > 0f && bounds.extents.y > 0f && bounds.extents.z > 0f) ? new Color(0f, 1f, 1f, 0.5f) : new Color(1f, 0f, 0f, 0.5f));
		OrientedBounds.DrawGizmo(base.transform, bounds, color);
	}
}
