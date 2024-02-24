using UnityEngine;

public class ConstructableBounds : MonoBehaviour
{
	public OrientedBounds bounds;

	private void OnDrawGizmosSelected()
	{
		Color color = ((bounds.extents.x > 0f && bounds.extents.y > 0f && bounds.extents.z > 0f) ? new Color(1f, 1f, 0f, 0.5f) : new Color(1f, 0f, 0f, 0.5f));
		OrientedBounds.DrawGizmo(base.transform, bounds, color);
	}
}
