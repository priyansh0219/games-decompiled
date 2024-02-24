using UnityEngine;

public class InvertMotion : MonoBehaviour
{
	public Transform center;

	[AssertNotNull]
	public Transform source;

	[AssertNotNull]
	public Transform destination;

	[HideInInspector]
	public Vector3 pos = Vector3.zero;

	[HideInInspector]
	public Quaternion rot = Quaternion.identity;

	private void LateUpdate()
	{
		pos = source.transform.InverseTransformPoint(center.position);
		rot = Quaternion.Inverse(source.rotation) * center.rotation;
		destination.localPosition = pos;
		destination.localRotation = rot;
	}
}
