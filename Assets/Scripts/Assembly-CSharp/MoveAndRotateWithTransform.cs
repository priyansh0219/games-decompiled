using UnityEngine;

public class MoveAndRotateWithTransform : MonoBehaviour
{
	public Transform target;

	private void LateUpdate()
	{
		base.transform.rotation = target.rotation;
		base.transform.position = target.position;
	}
}
