using UnityEngine;

public class FixedBounds : MonoBehaviour
{
	public Bounds _bounds;

	public Bounds bounds => new Bounds(base.gameObject.transform.TransformPoint(_bounds.center), _bounds.size);

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(bounds.center, 0.1f);
		Gizmos.DrawWireCube(bounds.center, bounds.size);
	}
}
