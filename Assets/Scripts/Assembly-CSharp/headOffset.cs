using UnityEngine;

public class headOffset : MonoBehaviour
{
	public Transform pivotPoint;

	public Transform skinMesh;

	private void LateUpdate()
	{
		base.transform.localPosition = -skinMesh.InverseTransformPoint(pivotPoint.position);
	}

	private void FixedUpdate()
	{
		base.transform.localPosition = -skinMesh.InverseTransformPoint(pivotPoint.position);
	}
}
