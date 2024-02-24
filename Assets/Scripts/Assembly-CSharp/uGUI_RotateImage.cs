using UnityEngine;

public class uGUI_RotateImage : MonoBehaviour
{
	public GameObject image;

	public float rotationTime = 2f;

	private void Update()
	{
		if (rotationTime != 0f)
		{
			float num = Mathf.Sign(rotationTime);
			float num2 = Mathf.Abs(rotationTime);
			float num3 = Time.time % num2 / num2 * 360f;
			Vector3 localEulerAngles = image.transform.localEulerAngles;
			localEulerAngles.z = num3 * num;
			image.transform.localEulerAngles = localEulerAngles;
		}
	}
}
