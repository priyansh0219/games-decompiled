using UnityEngine;

public class Shaker : MonoBehaviour
{
	public Transform target;

	public float shakeDuration = 0.5f;

	public float shakeMagnitude = 1f;

	public float yOffset;

	private float current;

	private float velocity;

	private Vector3 origLocalPosition;

	private void Awake()
	{
		origLocalPosition = base.transform.localPosition;
	}

	public void Shake(float scale = 1f)
	{
		current = scale;
	}

	private void LateUpdate()
	{
		current = Mathf.SmoothDamp(current, 0f, ref velocity, shakeDuration);
		target.localPosition = origLocalPosition + Utils.SampleSphere(current * shakeMagnitude) + new Vector3(0f, yOffset, 0f);
	}

	public void ResetShaking()
	{
		current = 0f;
		velocity = 0f;
	}
}
