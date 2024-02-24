using UnityEngine;

public class ShipExteriorCull : MonoBehaviour
{
	[AssertNotNull]
	public BoxCollider[] colliders;

	private Transform camTransform;

	private void Start()
	{
		if ((bool)MainCameraV2.main)
		{
			camTransform = MainCamera.camera.transform;
		}
		if ((bool)ShipExteriorCullManager.main)
		{
			ShipExteriorCullManager.main.Register(this);
		}
	}

	private void OnDestroy()
	{
		if ((bool)ShipExteriorCullManager.main)
		{
			ShipExteriorCullManager.main.Deregister(this);
		}
	}

	private bool PointInOABB(Vector3 point, BoxCollider box)
	{
		point = box.transform.InverseTransformPoint(point) - box.center;
		float num = box.size.x * 0.5f;
		float num2 = box.size.y * 0.5f;
		float num3 = box.size.z * 0.5f;
		if (point.x < num && point.x > 0f - num && point.y < num2 && point.y > 0f - num2 && point.z < num3 && point.z > 0f - num3)
		{
			return true;
		}
		return false;
	}

	public bool IsInVolume()
	{
		if (camTransform == null)
		{
			return false;
		}
		bool flag = false;
		BoxCollider[] array = colliders;
		foreach (BoxCollider box in array)
		{
			flag = PointInOABB(camTransform.position, box);
			if (flag)
			{
				break;
			}
		}
		return flag;
	}
}
