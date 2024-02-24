using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WalkOnGround : CreatureAction
{
	public Animator animator;

	public float timeCollidedWithGround;

	public float timeOnGround;

	public bool _onGround;

	public Vector3 crawlDirection;

	public float minGroundStaytime = 5f;

	public float varyGroundStayTime = 4f;

	public float groundStayTime = 2f;

	public override float Evaluate(Creature creature, float time)
	{
		if (!_onGround && time < timeCollidedWithGround + 0.1f)
		{
			return 0.7f;
		}
		if (_onGround && time < groundStayTime)
		{
			return 0.2f;
		}
		return 0f;
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (!_onGround)
		{
			timeOnGround = time;
			Vector2 insideUnitCircle = Random.insideUnitCircle;
			GetComponent<Rigidbody>().velocity = Vector3.zero;
			crawlDirection = new Vector3(insideUnitCircle.x, insideUnitCircle.y);
			GetComponent<Rigidbody>().AddForce(crawlDirection);
			groundStayTime = minGroundStaytime + Random.value * varyGroundStayTime;
		}
	}

	private void HandleOnGround(Collision collision)
	{
		if (!_onGround)
		{
			timeCollidedWithGround = Time.time;
			_onGround = true;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		HandleOnGround(collision);
	}

	private void OnCollisionStay(Collision collision)
	{
		HandleOnGround(collision);
	}

	private void OnCollisionExit(Collision collisionInfo)
	{
		_onGround = false;
	}

	private void Update()
	{
		if (animator != null)
		{
			animator.SetBool(AnimatorHashID.on_ground, _onGround);
		}
	}
}
