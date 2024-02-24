using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class CrabSquid : Creature, IManagedFixedUpdateBehaviour, IManagedBehaviour
{
	[AssertNotNull]
	[SerializeField]
	private Rigidbody rb;

	[AssertNotNull]
	[SerializeField]
	private OnSurfaceTracker onGroundTracker;

	[AssertNotNull]
	[SerializeField]
	private MoveOnGround moveOnGroundAction;

	[AssertNotNull]
	[SerializeField]
	private Locomotion locomotion;

	[SerializeField]
	private float groundDownforce = 10f;

	[SerializeField]
	private float jumpForce = 6f;

	private const float locomotionDampFactor = 0.4f;

	private const float dampTime = 0.5f;

	[SerializeField]
	private float upRotationSpeedWalk = 0.5f;

	[SerializeField]
	private float upRotationSpeedSwim = 3f;

	private bool wasOnGround;

	public int managedFixedUpdateIndex { get; set; }

	public new string GetProfileTag()
	{
		return "CrabSquid";
	}

	public void Update()
	{
		bool flag = moveOnGroundAction == GetBestAction() && onGroundTracker.onSurface;
		SetOnGround(flag);
		Animator animator = GetAnimator();
		animator.SetBool(AnimatorHashID.on_ground, flag);
		if (flag)
		{
			Vector3 direction = Vector3.ProjectOnPlane(rb.velocity, onGroundTracker.surfaceNormal);
			float deltaTime = Time.deltaTime;
			float num = 0.2f;
			Vector3 vector = base.transform.InverseTransformDirection(direction);
			animator.SetFloat(AnimatorHashID.move_speed_x, vector.x, num, deltaTime);
			animator.SetFloat(AnimatorHashID.move_speed_z, vector.z, num, deltaTime);
		}
	}

	public void ManagedFixedUpdate()
	{
		rb.AddForce(-onGroundTracker.surfaceNormal * groundDownforce, ForceMode.Acceleration);
	}

	public override void OnDestroy()
	{
		BehaviourUpdateUtils.Deregister(this);
		base.OnDestroy();
	}

	private void SetOnGround(bool onGround)
	{
		if (onGround != wasOnGround)
		{
			if (onGround)
			{
				BehaviourUpdateUtils.Register(this);
			}
			else
			{
				BehaviourUpdateUtils.Deregister(this);
				Animator animator = GetAnimator();
				animator.SetFloat(AnimatorHashID.move_speed_x, 0f);
				animator.SetFloat(AnimatorHashID.move_speed_z, 0f);
				Jump();
			}
			locomotion.rotateToSurfaceNormal = onGround;
			locomotion.upRotationSpeed = (onGround ? upRotationSpeedWalk : upRotationSpeedSwim);
			wasOnGround = onGround;
		}
	}

	private void Jump()
	{
		rb.AddForce(jumpForce * base.transform.up, ForceMode.VelocityChange);
	}
}
