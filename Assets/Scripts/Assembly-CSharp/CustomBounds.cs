using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class CustomBounds : MonoBehaviour
{
	public Vector3 customCenter = new Vector3(0f, 0f, 0f);

	public Vector3 customBounds = new Vector3(10000f, 10000f, 10000f);

	private void Start()
	{
		SkinnedMeshRenderer component = base.gameObject.GetComponent<SkinnedMeshRenderer>();
		Bounds localBounds = new Bounds(customCenter, customBounds);
		component.localBounds = localBounds;
	}
}
