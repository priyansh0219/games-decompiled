using UnityEngine;

public class VFXFakeParent : MonoBehaviour
{
	public Transform parent;

	private bool parented;

	public Vector3 localPos;

	public Vector3 localEuler;

	public Vector3 localScale;

	public void Parent(Transform newParent, Vector3 posOffset, Vector3 eulerOffset)
	{
		parented = true;
		parent = newParent;
		localPos = posOffset;
		localEuler = eulerOffset;
		base.transform.parent = null;
	}

	public void Unparent()
	{
		parented = false;
		parent = null;
	}

	private void LateUpdate()
	{
		if (parented)
		{
			if (parent != null)
			{
				base.transform.position = parent.TransformPoint(localPos);
				base.transform.rotation = parent.rotation * Quaternion.Euler(localEuler);
			}
			else
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
