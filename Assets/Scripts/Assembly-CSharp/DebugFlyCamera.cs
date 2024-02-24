using UWE;
using UnityEngine;

public sealed class DebugFlyCamera : MonoBehaviour
{
	public float mainSpeed = 50f;

	public float shiftAdd = 100f;

	public float maxShift = 1000f;

	public float camSens = 0.25f;

	public bool canLockMouse;

	private Vector3 lastMouse = new Vector3(255f, 255f, 255f);

	private float totalRun = 1f;

	private void Update()
	{
		if (canLockMouse && Input.GetMouseButtonDown(1))
		{
			UWE.Utils.lockCursor = true;
		}
		if (canLockMouse && Input.GetKeyDown(KeyCode.Escape))
		{
			UWE.Utils.lockCursor = false;
		}
		if (!UWE.Utils.lockCursor && Input.GetMouseButtonDown(1))
		{
			lastMouse = Input.mousePosition;
		}
		if (UWE.Utils.lockCursor)
		{
			float axisRaw = Input.GetAxisRaw("Mouse X");
			float axisRaw2 = Input.GetAxisRaw("Mouse Y");
			float y = base.transform.localEulerAngles.y + axisRaw * camSens;
			float x = base.transform.localEulerAngles.x;
			for (x -= axisRaw2 * camSens; x > 180f; x -= 360f)
			{
			}
			for (; x < -180f; x += 360f)
			{
			}
			x = Mathf.Clamp(x, -89f, 89f);
			base.transform.localEulerAngles = new Vector3(x, y, 0f);
		}
		else
		{
			if (Input.GetMouseButton(1))
			{
				lastMouse = Input.mousePosition - lastMouse;
				lastMouse = new Vector3((0f - lastMouse.y) * camSens, lastMouse.x * camSens, 0f);
				lastMouse = new Vector3(base.transform.eulerAngles.x + lastMouse.x, base.transform.eulerAngles.y + lastMouse.y, 0f);
				base.transform.eulerAngles = lastMouse;
			}
			lastMouse = Input.mousePosition;
		}
	}

	private void FixedUpdate()
	{
		Vector3 keyboardNetDirection = GetKeyboardNetDirection();
		if (Input.GetKey(KeyCode.LeftShift))
		{
			totalRun += Time.fixedDeltaTime;
			keyboardNetDirection = keyboardNetDirection * totalRun * shiftAdd;
			keyboardNetDirection.x = Mathf.Clamp(keyboardNetDirection.x, 0f - maxShift, maxShift);
			keyboardNetDirection.y = Mathf.Clamp(keyboardNetDirection.y, 0f - maxShift, maxShift);
			keyboardNetDirection.z = Mathf.Clamp(keyboardNetDirection.z, 0f - maxShift, maxShift);
		}
		else
		{
			totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
			keyboardNetDirection = keyboardNetDirection.normalized * mainSpeed;
		}
		if (Input.GetKey(KeyCode.Space))
		{
			keyboardNetDirection.y = 0f;
		}
		if ((bool)GetComponent<Rigidbody>())
		{
			Vector3 vector = base.transform.TransformDirection(keyboardNetDirection);
			GetComponent<Rigidbody>().AddForce(vector - GetComponent<Rigidbody>().velocity, ForceMode.VelocityChange);
		}
		else
		{
			base.transform.Translate(keyboardNetDirection * Time.fixedDeltaTime);
		}
	}

	private Vector3 GetKeyboardNetDirection()
	{
		Vector3 zero = Vector3.zero;
		if (Input.GetKey(KeyCode.W))
		{
			zero += new Vector3(0f, 0f, 1f);
		}
		if (Input.GetKey(KeyCode.S))
		{
			zero += new Vector3(0f, 0f, -1f);
		}
		if (Input.GetKey(KeyCode.A))
		{
			zero += new Vector3(-1f, 0f, 0f);
		}
		if (Input.GetKey(KeyCode.D))
		{
			zero += new Vector3(1f, 0f, 0f);
		}
		if (Input.GetKey(KeyCode.Q))
		{
			zero -= new Vector3(0f, 1f, 0f);
		}
		if (Input.GetKey(KeyCode.E))
		{
			zero += new Vector3(0f, 1f, 0f);
		}
		return zero;
	}
}
