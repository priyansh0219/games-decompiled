using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class CaveCrawler : Creature
{
	public float animationMaxSpeed = 1f;

	public float animationMaxTilt = 10f;

	public float dampTime = 0.5f;

	public FMODAsset jumpSound;

	public FMOD_CustomLoopingEmitter walkingSound;

	[AssertNotNull]
	public Rigidbody rb;

	[AssertNotNull]
	public Collider aliveCollider;

	[AssertNotNull]
	public Collider deadCollider;

	[AssertNotNull]
	public OnSurfaceTracker onSurfaceTracker;

	private Vector3 moveDirection = Vector3.forward;

	private float timeLastJump;

	private float jumpMaxHeight;

	private float prevYAngle;

	public void OnJump()
	{
		if (jumpSound != null)
		{
			Utils.PlayFMODAsset(jumpSound, base.transform);
		}
		timeLastJump = Time.time;
	}

	public bool IsOnSurface()
	{
		return onSurfaceTracker.onSurface;
	}

	public Vector3 GetSurfaceNormal()
	{
		return onSurfaceTracker.surfaceNormal;
	}

	public void Update()
	{
		leashPosition.y = base.transform.position.y;
		if (!onSurfaceTracker.onSurface)
		{
			new Vector3(rb.velocity.x, 0f, rb.velocity.z);
		}
		else
		{
			_ = rb.velocity;
		}
		Vector3 vector = base.transform.InverseTransformVector(rb.velocity) / animationMaxSpeed;
		Animator animator = GetAnimator();
		animator.SetFloat(AnimatorHashID.move_speed_x, vector.x);
		animator.SetFloat(AnimatorHashID.move_speed_z, vector.z);
		animator.SetFloat(AnimatorHashID.speed, Mathf.Clamp01(Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z)));
		animator.SetBool(AnimatorHashID.jump, Time.time - timeLastJump < 0.2f);
		animator.SetFloat(AnimatorHashID.jump_height, jumpMaxHeight - base.transform.position.y);
		animator.SetBool(AnimatorHashID.on_ground, onSurfaceTracker.onSurface);
		float deltaTime = Time.deltaTime;
		if (deltaTime > 0f)
		{
			float num = Mathf.DeltaAngle(prevYAngle, base.transform.eulerAngles.y) / deltaTime;
			num = Mathf.Clamp(num / animationMaxTilt, -1f, 1f);
			prevYAngle = base.transform.eulerAngles.y;
			animator.SetFloat(AnimatorHashID.tilt, num, dampTime, deltaTime);
		}
		jumpMaxHeight = (onSurfaceTracker.onSurface ? base.transform.position.y : Mathf.Max(jumpMaxHeight, base.transform.position.y));
		if (onSurfaceTracker.onSurface && rb.velocity.sqrMagnitude > 0.01f)
		{
			walkingSound.Play();
		}
		else
		{
			walkingSound.Stop();
		}
	}

	public override void OnKill()
	{
		GetAnimator().SetBool(AnimatorHashID.dead, value: true);
		aliveCollider.enabled = false;
		deadCollider.enabled = true;
		deadCollider.isTrigger = false;
		base.OnKill();
	}
}
