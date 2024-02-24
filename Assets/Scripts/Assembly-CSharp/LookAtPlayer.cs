using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
	public enum RotateMode
	{
		None = 0,
		LookAt = 1,
		Copy = 2
	}

	public RotateMode rotateMode = RotateMode.Copy;

	public bool scale = true;

	public float scaleFactor = 1f;

	private Transform tr;

	private void Awake()
	{
		tr = GetComponent<Transform>();
	}

	private void Update()
	{
		if (SNCameraRoot.main == null)
		{
			return;
		}
		Transform aimingTransform = SNCameraRoot.main.GetAimingTransform();
		if (!(aimingTransform == null))
		{
			switch (rotateMode)
			{
			case RotateMode.LookAt:
				tr.LookAt(aimingTransform);
				break;
			case RotateMode.Copy:
				tr.rotation = aimingTransform.rotation;
				break;
			}
			if (scale)
			{
				float num = Vector3.Distance(aimingTransform.position, tr.position) * scaleFactor;
				tr.localScale = new Vector3(num, num, num);
			}
		}
	}
}
