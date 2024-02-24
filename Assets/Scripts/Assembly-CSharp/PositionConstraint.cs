using UnityEngine;

public class PositionConstraint : MonoBehaviour
{
	public Vector3 localPosition;

	public Vector3 worldPosition;

	public Vector3 influence = Vector3.one;

	private void LateUpdate()
	{
		base.transform.localPosition = localPosition;
		Vector3 position = base.transform.position;
		position.x = Mathf.Lerp(base.transform.position.x, worldPosition.x, influence.x);
		position.y = Mathf.Lerp(base.transform.position.y, worldPosition.y, influence.y);
		position.z = Mathf.Lerp(base.transform.position.z, worldPosition.z, influence.z);
		base.transform.position = position;
	}
}
