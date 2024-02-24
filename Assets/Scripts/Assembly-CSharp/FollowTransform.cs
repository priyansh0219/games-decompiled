using UnityEngine;

public class FollowTransform : MonoBehaviour
{
	public Transform parent;

	public bool keepOffset;

	public bool keepRotation;

	private Transform tr;

	private Vector3 offsetPosition;

	private Quaternion offsetRotation;

	private void Awake()
	{
		tr = GetComponent<Transform>();
		if (parent != null && keepOffset)
		{
			Quaternion quaternion = Quaternion.Inverse(parent.rotation);
			offsetPosition = quaternion * (tr.position - parent.position);
			offsetRotation = quaternion * tr.rotation;
		}
	}

	private void LateUpdate()
	{
		if (!(parent != null))
		{
			return;
		}
		if (keepOffset)
		{
			Quaternion rotation = parent.rotation;
			tr.position = parent.position + rotation * offsetPosition;
			if (!keepRotation)
			{
				tr.rotation = rotation * offsetRotation;
			}
		}
		else
		{
			tr.position = parent.position;
			if (!keepRotation)
			{
				tr.rotation = parent.rotation;
			}
		}
	}
}
