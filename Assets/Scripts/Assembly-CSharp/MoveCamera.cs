using System.Collections.Generic;
using UWE;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MoveCamera : MonoBehaviour
{
	public float mouseSensitivity = 1f;

	public float force = 1f;

	public List<GameObject> toolToggle = new List<GameObject>();

	private void UpdateTools()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			toolToggle[0].SetActive(!toolToggle[0].activeInHierarchy);
			if (toolToggle[0].activeInHierarchy)
			{
				toolToggle[1].SetActive(value: false);
			}
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			toolToggle[1].SetActive(!toolToggle[1].activeInHierarchy);
			if (toolToggle[1].activeInHierarchy)
			{
				toolToggle[0].SetActive(value: false);
			}
		}
	}

	private void UpdateCamera()
	{
		float num = 1f;
		if (Input.GetKeyDown(KeyCode.LeftShift))
		{
			num *= 10f;
		}
		float axisRaw = Input.GetAxisRaw("Forward");
		if (!Utils.NearlyEqual(axisRaw, 0f))
		{
			GetComponent<Rigidbody>().AddForce(base.transform.forward * axisRaw * force * num);
		}
		float axisRaw2 = Input.GetAxisRaw("Horizontal");
		if (!Utils.NearlyEqual(axisRaw2, 0f))
		{
			GetComponent<Rigidbody>().AddForce(base.transform.right * axisRaw2 * force * num);
		}
		UWE.Utils.lockCursor = !Input.GetMouseButton(1);
		if (UWE.Utils.lockCursor)
		{
			float axisRaw3 = Input.GetAxisRaw("Mouse X");
			float axisRaw4 = Input.GetAxisRaw("Mouse Y");
			base.transform.localEulerAngles = new Vector3(base.transform.localEulerAngles.x - axisRaw4 * mouseSensitivity, base.transform.localEulerAngles.y + axisRaw3 * mouseSensitivity, 0f);
		}
	}

	private void Update()
	{
		UpdateTools();
		UpdateCamera();
	}
}
