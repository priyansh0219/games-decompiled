using System;
using UnityEngine;

public class PlayerMask : MonoBehaviour
{
	public Transform topLeft;

	public Transform topMiddle;

	public Transform topRight;

	public Transform bottomLeft;

	public Transform bottomMiddle;

	public Transform bottomRight;

	private Vector3 topLeftStartPosition;

	private Vector3 topRightStartPosition;

	private Vector3 topMiddleStartPosition;

	private Vector3 bottomLeftStartPosition;

	private Vector3 bottomRightStartPosition;

	private Vector3 bottomMiddleStartPosition;

	private Vector3 topLeftOffset;

	private Vector3 topRightOffset;

	private Vector3 topMiddleOffset;

	private Vector3 bottomLeftOffset;

	private Vector3 bottomRightOffset;

	private Vector3 bottomMiddleOffset;

	public float referenceDepth = 0.074f;

	public float referenceFov = 60f;

	public float referenceAspect = 1.7777778f;

	private float currentFov;

	private float currentAspect;

	private void OnValidate()
	{
		GetViewSpaceAnchors(referenceFov, referenceAspect, out topLeftOffset, out topMiddleOffset, out topRightOffset, out bottomLeftOffset, out bottomMiddleOffset, out bottomRightOffset);
		currentFov = 0f;
	}

	private void Start()
	{
		GetViewSpaceAnchors(referenceFov, referenceAspect, out topLeftOffset, out topMiddleOffset, out topRightOffset, out bottomLeftOffset, out bottomMiddleOffset, out bottomRightOffset);
		topLeftStartPosition = topLeft.localPosition;
		topMiddleStartPosition = topMiddle.localPosition;
		topRightStartPosition = topRight.localPosition;
		bottomLeftStartPosition = bottomLeft.localPosition;
		bottomMiddleStartPosition = bottomMiddle.localPosition;
		bottomRightStartPosition = bottomRight.localPosition;
	}

	private void GetViewSpaceAnchors(float fov, float aspect, out Vector3 topLeftAnchor, out Vector3 topMiddleAnchor, out Vector3 topRightAnchor, out Vector3 bottomLeftAnchor, out Vector3 bottomMiddleAnchor, out Vector3 bottomRightAnchor)
	{
		float num = Mathf.Tan(fov * ((float)Math.PI / 180f) * 0.5f) * referenceDepth;
		float num2 = num * aspect;
		topLeftAnchor = new Vector3(0f - num2, num, referenceDepth);
		topMiddleAnchor = new Vector3(0f, num, referenceDepth);
		topRightAnchor = new Vector3(num2, num, referenceDepth);
		bottomLeftAnchor = new Vector3(0f - num2, 0f - num, referenceDepth);
		bottomMiddleAnchor = new Vector3(0f, 0f - num, referenceDepth);
		bottomRightAnchor = new Vector3(num2, 0f - num, referenceDepth);
	}

	private void LateUpdate()
	{
		UpdateForCamera();
	}

	private void UpdateForCamera()
	{
		Camera mainCam = SNCameraRoot.main.mainCam;
		if (mainCam.fieldOfView != currentFov || mainCam.aspect != currentAspect)
		{
			GetViewSpaceAnchors(mainCam.fieldOfView, mainCam.aspect, out var topLeftAnchor, out var topMiddleAnchor, out var topRightAnchor, out var bottomLeftAnchor, out var bottomMiddleAnchor, out var bottomRightAnchor);
			topLeft.localPosition = topLeftAnchor - topLeftOffset + topLeftStartPosition;
			topMiddle.localPosition = topMiddleAnchor - topMiddleOffset + topMiddleStartPosition;
			topRight.localPosition = topRightAnchor - topRightOffset + topRightStartPosition;
			bottomLeft.localPosition = bottomLeftAnchor - bottomLeftOffset + bottomLeftStartPosition;
			bottomMiddle.localPosition = bottomMiddleAnchor - bottomMiddleOffset + bottomMiddleStartPosition;
			bottomRight.localPosition = bottomRightAnchor - bottomRightOffset + bottomRightStartPosition;
			currentFov = mainCam.fieldOfView;
			currentAspect = mainCam.aspect;
		}
	}
}
