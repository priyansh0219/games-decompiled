using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ShuttleCrawler : MonoBehaviour
{
	[AssertNotNull]
	public Animator animator;

	public float floatingDrag;

	public float jumpBaseForce;

	public float jumpVariableForce;

	public bool debugTriggerJump;

	private float timeLastJump;

	private float timeLastCollision;

	private Vector3 targetUpOrientation = Vector3.up;

	public float onGroundDistance;

	public float moveRotateRadiansPerSecond;

	public float minWalkTime;

	public float walkVarianceTime;

	public float walkChance;

	public Vector3 walkDirection;

	public float walkDuration;

	public float walkSpeedScalar;

	public float maxWalkSpeed;

	public float moveSpeedInterpSpeed;

	private void Start()
	{
	}

	private void Jump()
	{
		base.gameObject.GetComponent<Rigidbody>().drag = 0f;
		Vector3 vector = base.transform.up + new Vector3(UnityEngine.Random.Range(-0.1f, 0.1f), 0f, UnityEngine.Random.Range(-0.1f, 0.1f));
		vector.Normalize();
		GetComponent<Rigidbody>().AddForce(vector * (jumpBaseForce + UnityEngine.Random.value * jumpVariableForce));
		base.transform.up = vector;
		SetOnGround(newState: false);
		timeLastJump = Time.time;
	}

	private void SetOnGround(bool newState)
	{
		animator.SetBool(AnimatorHashID.on_ground, newState);
		base.gameObject.GetComponent<Rigidbody>().drag = (newState ? 0f : floatingDrag);
	}

	private bool CheckOnGround()
	{
		bool result = false;
		RaycastHit hitInfo = default(RaycastHit);
		Vector3 vector = base.transform.position + base.transform.up * 0.1f;
		float num = onGroundDistance;
		if (Physics.Raycast(vector, Vector3.down, out hitInfo, num))
		{
			base.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
			result = true;
			Debug.DrawLine(vector, vector + Vector3.down * num, Color.green, 0.1f);
		}
		else
		{
			Debug.DrawLine(vector, vector + Vector3.down * num, Color.red, 0.1f);
		}
		return result;
	}

	private void FixedUpdate()
	{
		if (debugTriggerJump && animator.GetBool(AnimatorHashID.on_ground))
		{
			Jump();
			debugTriggerJump = false;
		}
		SetOnGround(CheckOnGround());
	}

	private void SetMoveSpeedVertical()
	{
		float num = Mathf.Abs(new Vector3(base.gameObject.GetComponent<Rigidbody>().velocity.x, base.gameObject.GetComponent<Rigidbody>().velocity.y, base.gameObject.GetComponent<Rigidbody>().velocity.z).y);
		num = (Utils.NearlyEqual(num, 0f) ? 0f : num);
		SetMoveSpeedParam("move_speed_vertical", num);
	}

	private void SetMoveSpeedHorizontal()
	{
		if (animator.GetBool(AnimatorHashID.on_ground))
		{
			float newVal = 0f;
			if (walkDuration > 0f)
			{
				Debug.DrawLine(base.transform.position, base.transform.position + walkDirection, Color.yellow, 0.1f);
				base.transform.forward = Vector3.RotateTowards(base.transform.forward, walkDirection.normalized, moveRotateRadiansPerSecond * Time.deltaTime, 0f);
				Debug.DrawLine(base.transform.position, base.transform.position + base.transform.forward, Color.black, 0.1f);
				newVal = 10f;
			}
			SetMoveSpeedParam("move_speed_horizontal", newVal);
		}
	}

	private void SetMoveSpeedParam(string paramName, float newVal)
	{
		float @float = animator.GetFloat(paramName);
		float maxDelta = Time.deltaTime * moveSpeedInterpSpeed;
		SafeAnimator.SetFloat(animator, paramName, Mathf.MoveTowards(@float, newVal, maxDelta));
	}

	private void Update()
	{
		bool value = Time.time - timeLastJump < 2f;
		animator.SetBool(AnimatorHashID.jump, value);
		SetMoveSpeedVertical();
		SetMoveSpeedHorizontal();
		UpdateWalking();
		UpdateUpwardOrientation();
	}

	private void UpdateWalking()
	{
		if (!animator.GetBool(AnimatorHashID.on_ground))
		{
			return;
		}
		if (walkDuration == 0f)
		{
			if (UnityEngine.Random.value < walkChance)
			{
				walkDuration = minWalkTime + UnityEngine.Random.value * walkVarianceTime;
				float f = UnityEngine.Random.value * 2f * (float)Math.PI;
				walkDirection.x = Mathf.Cos(f);
				walkDirection.z = Mathf.Sin(f);
			}
		}
		else
		{
			base.gameObject.transform.position += walkDirection * Time.deltaTime * walkSpeedScalar;
			walkDuration -= Time.deltaTime;
			walkDuration = Mathf.Max(walkDuration, 0f);
		}
	}

	private void UpdateUpwardOrientation()
	{
		if (Time.time - timeLastCollision < 0.2f && !animator.GetBool(AnimatorHashID.jump) && animator.GetBool(AnimatorHashID.on_ground))
		{
			base.transform.up = Vector3.RotateTowards(base.transform.up, targetUpOrientation, Time.deltaTime, 0f);
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		targetUpOrientation = collision.transform.up;
		timeLastCollision = Time.time;
		Debug.DrawLine(collision.gameObject.transform.position, collision.gameObject.transform.position + collision.gameObject.transform.up * 2f, Color.black, 0.1f);
	}
}
