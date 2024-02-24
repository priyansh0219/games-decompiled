using System;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class Reefback : Creature
{
	public enum Mode
	{
		Unknown = 0,
		Swimming = 1,
		Burrowing = 2,
		Burrowed = 3,
		Unburrowing = 4
	}

	public Mode mode = Mode.Swimming;

	public float maxMoveSpeed;

	public float moveForce;

	public float turningSpeed;

	public AnimationCurve heightHorizDistance;

	public float burrowSpeed;

	public float unburrowSpeed;

	public float burrowY;

	public int numCreaturesToDisturb;

	public Vector3 targetBurrowPos = new Vector3(0f, 0f, 0f);

	public float kAnimParamSpeedRate;

	public float kAnimParamTurnRate;

	public float kAnimParamLevelRate;

	public float targetPointDistance;

	private float targetBurrowHeight;

	private float distanceXZToTravel;

	public bool disableMovement;

	[AssertNotNull]
	public Rigidbody myRigidbody;

	private void UpdateMode()
	{
		if (mode == Mode.Burrowed && IsDisturbed())
		{
			Debug.Log("Disturbed!");
			SetBurrowPos();
			SetUnburrowing();
		}
		if (mode == Mode.Swimming && (base.transform.position - targetBurrowPos).magnitude < targetPointDistance)
		{
			SetBurrowing();
		}
	}

	private void SetUnburrowing()
	{
		Debug.Log("Set unburrowing");
		SetMode(Mode.Unburrowing);
		targetBurrowHeight = base.transform.position.y - burrowY;
	}

	private void SetBurrowing()
	{
		Debug.Log("Set burrowing");
		SetMode(Mode.Burrowing);
		targetBurrowHeight = base.transform.position.y + burrowY;
	}

	private void SetBurrowPos()
	{
		bool flag = false;
		RaycastHit hitInfo = default(RaycastHit);
		int num = 0;
		do
		{
			float num2 = (float)((double)(UnityEngine.Random.value * 2f) * System.Math.PI);
			float num3 = UnityEngine.Random.Range(75, 150);
			Vector3 vector = new Vector3(base.transform.position.x + (float)System.Math.Cos(num2) * num3, base.transform.position.y + 500f, base.transform.position.z + (float)System.Math.Sin(num2) * num3);
			flag = Physics.Raycast(vector, Vector3.down, out hitInfo, 1000f);
			if (flag)
			{
				Debug.DrawLine(vector, hitInfo.point, Color.red, 5f);
			}
			else
			{
				Debug.DrawLine(vector, vector + Vector3.down * 1000f, Color.white, 5f);
			}
			num++;
		}
		while (!flag && num < 10);
		if (!flag)
		{
			Debug.Log("Reefback.SetBurrowPos(): failed after 10 tries.");
		}
		targetBurrowPos = hitInfo.point;
		Vector3 position = base.transform.position;
		position -= targetBurrowPos;
		position.y = 0f;
		distanceXZToTravel = position.magnitude;
	}

	private bool IsDisturbed()
	{
		return false;
	}

	private bool SetMode(Mode m)
	{
		if (mode != m)
		{
			Debug.Log(string.Concat("Setting mode from ", mode, " to ", m));
			mode = m;
			return true;
		}
		return false;
	}

	private void UpdateMovement()
	{
		Rigidbody component = GetComponent<Rigidbody>();
		bool isKinematic = mode == Mode.Burrowing || mode == Mode.Unburrowing || mode == Mode.Burrowed;
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(component, isKinematic);
		if (mode == Mode.Swimming)
		{
			Vector3 vector = new Vector3(targetBurrowPos.x - base.transform.position.x, targetBurrowPos.y - base.transform.position.y, targetBurrowPos.z - base.transform.position.z);
			Vector3 vector2 = new Vector3(vector.x, 0f, vector.z);
			float time = 1f - vector2.magnitude / distanceXZToTravel;
			float num = heightHorizDistance.Evaluate(time);
			vector.y = targetBurrowPos.y + num * 100f - base.transform.position.y;
			base.gameObject.GetComponent<Rigidbody>().AddForce(vector * moveForce * Time.deltaTime);
			if (vector2.magnitude < 40f)
			{
				vector.y = 0f;
			}
			base.transform.forward = Vector3.Lerp(base.transform.forward, vector, Time.deltaTime * turningSpeed);
			if (base.gameObject.GetComponent<Rigidbody>().velocity.magnitude > maxMoveSpeed)
			{
				base.gameObject.GetComponent<Rigidbody>().velocity = Vector3.ClampMagnitude(base.gameObject.GetComponent<Rigidbody>().velocity, maxMoveSpeed);
			}
		}
		if (mode == Mode.Unburrowing)
		{
			base.transform.position = new Vector3(base.transform.position.x, Mathf.Lerp(base.transform.position.y, targetBurrowHeight, Time.deltaTime * unburrowSpeed), base.transform.position.z);
			if (Mathf.Abs(base.transform.position.y - targetBurrowHeight) < 0.1f)
			{
				base.transform.position = new Vector3(base.transform.position.x, targetBurrowHeight, base.transform.position.z);
				SetMode(Mode.Swimming);
			}
		}
		else if (mode == Mode.Burrowing)
		{
			base.transform.position = new Vector3(base.transform.position.x, Mathf.Lerp(base.transform.position.y, targetBurrowHeight, Time.deltaTime * burrowSpeed), base.transform.position.z);
			if (Mathf.Abs(base.transform.position.y - targetBurrowHeight) < 0.1f)
			{
				base.transform.position = new Vector3(base.transform.position.x, targetBurrowHeight, base.transform.position.z);
				SetMode(Mode.Burrowed);
			}
		}
	}

	private void UpdateAnimation()
	{
	}

	private void OnDrawGizmos()
	{
		if (mode == Mode.Swimming)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(targetBurrowPos, 0.25f);
		}
	}

	private void FixedUpdate()
	{
		if (myRigidbody.velocity.magnitude > 10f)
		{
			myRigidbody.velocity = myRigidbody.velocity.normalized * 10f;
		}
	}
}
