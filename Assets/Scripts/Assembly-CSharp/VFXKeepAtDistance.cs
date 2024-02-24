using System;
using UnityEngine;

public class VFXKeepAtDistance : MonoBehaviour
{
	[AssertNotNull]
	public Transform realPositionTransform;

	public float minDistance;

	public float maxDistance = 1700f;

	public float minScale;

	public float maxScale = 1f;

	private void LateUpdate()
	{
		Vector3 position = MainCamera.camera.transform.position;
		float num = Vector3.Distance(realPositionTransform.position, position);
		float maxDistanceDelta = 0f;
		if (num > maxDistance)
		{
			maxDistanceDelta = num - maxDistance;
		}
		else if (num < minDistance)
		{
			maxDistanceDelta = num - minDistance;
		}
		base.transform.position = Vector3.MoveTowards(realPositionTransform.position, position, maxDistanceDelta);
		float num2 = Vector3.Distance(base.transform.position, position);
		float num3 = 2f * Mathf.Tan(MainCamera.camera.fieldOfView * 0.5f * ((float)Math.PI / 180f));
		float num4 = num3 * num;
		float num5 = num3 * num2;
		float num6 = (num4 - num5) / num4;
		base.transform.localScale = Vector3.one * Mathf.Clamp(1f - num6, minScale, maxScale);
	}
}
