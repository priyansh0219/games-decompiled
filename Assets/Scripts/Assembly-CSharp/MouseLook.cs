using UnityEngine;

[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLook : MonoBehaviour
{
	public enum RotationAxes
	{
		MouseXAndY = 0,
		MouseX = 1,
		MouseY = 2
	}

	public RotationAxes axes;

	public float sensitivityX = 15f;

	public float sensitivityY = 15f;

	public float minimumX = -360f;

	public float maximumX = 360f;

	public float minimumY = -60f;

	public float maximumY = 60f;

	public bool mouseLookEnabled = true;

	public bool invertY;

	private float rotationY;

	public void LayoutEscapeMenuGUI()
	{
		if (axes == RotationAxes.MouseY)
		{
			invertY = GUILayout.Toggle(invertY, "Invert Mouse Y");
		}
	}

	private void Update()
	{
		int num;
		float num2;
		if (mouseLookEnabled)
		{
			num = (AvatarInputHandler.main.IsEnabled() ? 1 : 0);
			if (num != 0)
			{
				num2 = Input.GetAxisRaw("Mouse X");
				goto IL_0029;
			}
		}
		else
		{
			num = 0;
		}
		num2 = 0f;
		goto IL_0029;
		IL_0029:
		float num3 = num2;
		float num4 = ((num != 0) ? Input.GetAxisRaw("Mouse Y") : 0f);
		if (invertY)
		{
			num4 *= -1f;
		}
		if (axes == RotationAxes.MouseXAndY)
		{
			float y = base.transform.localEulerAngles.y + num3 * sensitivityX;
			rotationY += num4 * sensitivityY;
			rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
			base.transform.localEulerAngles = new Vector3(0f - rotationY, y, 0f);
		}
		else if (axes == RotationAxes.MouseX)
		{
			base.transform.Rotate(0f, num3 * sensitivityX, 0f);
		}
		else
		{
			rotationY += num4 * sensitivityY;
			rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
			base.transform.localEulerAngles = new Vector3(0f - rotationY, base.transform.localEulerAngles.y, 0f);
		}
	}

	public void SetEnabled(bool val)
	{
		mouseLookEnabled = val;
	}

	private void Start()
	{
		if ((bool)GetComponent<Rigidbody>())
		{
			GetComponent<Rigidbody>().freezeRotation = true;
		}
	}
}
