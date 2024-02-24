using System;
using UnityEngine;

public class Stabilizer : MonoBehaviour
{
	public float uprightAccelerationStiffness = 10f;

	public bool stabilizerEnabled = true;

	public GameObject gyro;

	public float maxGyroSpinSpeed = 1440f;

	private float gyroSpinSpeed;

	[NonSerialized]
	public Rigidbody body;

	[NonSerialized]
	public SubRoot subRoot;

	public void Start()
	{
		body = base.gameObject.FindAncestor<Rigidbody>();
		subRoot = base.gameObject.FindAncestor<SubRoot>();
	}

	public void Update()
	{
		if (gyro != null && body != null && !body.isKinematic)
		{
			gyro.transform.Rotate(Vector3.up * gyroSpinSpeed * Time.deltaTime);
			gyroSpinSpeed += Time.deltaTime * 360f * (float)(stabilizerEnabled ? 1 : (-2));
			gyroSpinSpeed = Mathf.Clamp(gyroSpinSpeed, 0f, maxGyroSpinSpeed);
		}
	}

	public void FixedUpdate()
	{
		if (stabilizerEnabled && body != null && !body.isKinematic)
		{
			Vector3 vector = body.transform.position + body.transform.up;
			Vector3 vector2 = body.transform.position + Vector3.up;
			Vector3 force = uprightAccelerationStiffness * (vector2 - vector);
			body.AddForceAtPosition(force, vector, ForceMode.Acceleration);
			vector = body.transform.position - body.transform.up;
			vector2 = body.transform.position - Vector3.up;
			force = uprightAccelerationStiffness * (vector2 - vector);
			body.AddForceAtPosition(force, vector, ForceMode.Acceleration);
		}
	}

	public void Toggle()
	{
		stabilizerEnabled = !stabilizerEnabled;
	}
}
