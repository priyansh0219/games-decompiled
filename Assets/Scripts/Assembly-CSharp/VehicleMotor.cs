using System;
using UnityEngine;

public class VehicleMotor : MonoBehaviour
{
	public bool canControl = true;

	public float kSpeedScalar = 100f;

	public float kMaxSpeed = 100f;

	public float kWaterDrag = 0.001f;

	[NonSerialized]
	public Vector3 inputMoveDirection = new Vector3(0f, 0f, 0f);

	[NonSerialized]
	public bool inputJump;

	private Vector3 velocity = new Vector3(0f, 0f, 0f);

	private void UpdateFunction()
	{
		Debug.DrawLine(base.transform.position, base.transform.position + inputMoveDirection, Color.white, 2f);
		velocity += inputMoveDirection * Time.deltaTime * kSpeedScalar;
		velocity = Vector3.ClampMagnitude(velocity, kMaxSpeed);
		Vector3 vector = base.transform.position + velocity * Time.deltaTime;
		vector.y = Mathf.Min(vector.y, Ocean.GetOceanLevel());
		Vector3 motion = vector - base.transform.position;
		GetComponent<CharacterController>().Move(motion);
		velocity *= 1f - Time.deltaTime * kWaterDrag;
	}

	private void Update()
	{
		canControl = Player.main.motorMode == Player.MotorMode.Vehicle;
		if (canControl)
		{
			UpdateFunction();
		}
	}
}
