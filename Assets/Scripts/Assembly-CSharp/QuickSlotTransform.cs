using System;
using UnityEngine;

public class QuickSlotTransform : MonoBehaviour
{
	public Transform handSlot;

	public float swaySpeed = 5f;

	public float swayMaxAngle = 10f;

	public float slotOffset = 0.7f;

	public float slotDepth = 1f;

	public AnimationCurve swayEasing = new AnimationCurve(new Keyframe(0f, 0f, 2f, 2f), new Keyframe(1f, 1f, 0f, 0f));

	private Transform tr;

	private Camera refCam;

	private Vector3 oldSlotPos;

	private void Start()
	{
		refCam = Player.main.viewModelCamera;
		tr = GetComponent<Transform>();
		if (handSlot == null)
		{
			Debug.LogError("QuickSlotTransform : Awake() : handSlot is not assigned!");
			return;
		}
		handSlot.parent = tr;
		handSlot.localPosition = Vector3.zero;
		handSlot.localRotation = Quaternion.identity;
	}

	private void LateUpdate()
	{
		Vector3 vector = tr.rotation * GetSlotPosition();
		Vector3 vector2 = Vector3.Slerp(oldSlotPos, vector, swaySpeed * Time.deltaTime);
		float num = Vector3.Angle(vector, vector2);
		float num2 = Mathf.Abs(num);
		Vector3 n = Vector3.Cross(vector2, vector);
		if (num2 > swayMaxAngle)
		{
			num = Mathf.Sign(num) * swayMaxAngle;
			vector2 = MathExtensions.RotateVectorAroundAxisAngle(n, vector, (0f - num) * ((float)Math.PI / 180f));
		}
		oldSlotPos = vector2;
		float num3 = swayEasing.Evaluate(num2 / swayMaxAngle);
		num = Mathf.Sign(num) * num3 * swayMaxAngle;
		vector2 = MathExtensions.RotateVectorAroundAxisAngle(n, vector, (0f - num) * ((float)Math.PI / 180f));
		handSlot.localPosition = Quaternion.Inverse(tr.rotation) * vector2;
	}

	private Vector3 GetSlotPosition()
	{
		slotOffset = Mathf.Clamp01(slotOffset);
		float num = Mathf.Tan(refCam.fieldOfView * 0.5f * ((float)Math.PI / 180f));
		float x = refCam.aspect * num;
		return Vector3.Slerp(Vector3.forward, new Vector3(x, 0f - num, 1f), slotOffset).normalized * slotDepth;
	}
}
