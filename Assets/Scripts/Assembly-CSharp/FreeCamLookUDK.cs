using UnityEngine;

public class FreeCamLookUDK : MonoBehaviour
{
	public float sensitivityX = 0.5f;

	public float sensitivityY = 0.5f;

	private float mHdg;

	private float mPitch;

	private void Start()
	{
	}

	private void Update()
	{
		if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
		{
			float aVal = Input.GetAxis("Mouse X") * sensitivityX;
			float num = Input.GetAxis("Mouse Y") * sensitivityY;
			if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
			{
				Strafe(aVal);
				ChangeHeight(num);
			}
			else if (Input.GetMouseButton(0))
			{
				MoveForwards(num);
				ChangeHeading(aVal);
			}
			else if (Input.GetMouseButton(1))
			{
				ChangeHeading(aVal);
				ChangePitch(0f - num);
			}
		}
	}

	private void MoveForwards(float aVal)
	{
		Vector3 forward = base.transform.forward;
		forward.y = 0f;
		forward.Normalize();
		base.transform.position += aVal * forward;
	}

	private void Strafe(float aVal)
	{
		base.transform.position += aVal * base.transform.right;
	}

	private void ChangeHeight(float aVal)
	{
		base.transform.position += aVal * Vector3.up;
	}

	private void ChangeHeading(float aVal)
	{
		mHdg += aVal;
		WrapAngle(ref mHdg);
		base.transform.localEulerAngles = new Vector3(mPitch, mHdg, 0f);
	}

	private void ChangePitch(float aVal)
	{
		mPitch += aVal;
		WrapAngle(ref mPitch);
		base.transform.localEulerAngles = new Vector3(mPitch, mHdg, 0f);
	}

	public static void WrapAngle(ref float angle)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
	}
}
