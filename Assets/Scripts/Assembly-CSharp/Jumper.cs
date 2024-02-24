using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class Jumper : Creature
{
	public enum State
	{
		Swim = 0,
		Walk = 1,
		Drift = 2
	}

	[AssertNotNull]
	public OnSurfaceTracker onSurfaceTracker;

	[AssertNotNull]
	public ConstantForce descendForce;

	[AssertNotNull]
	public Locomotion locomotion;

	[AssertNotNull]
	public CreatureAction swimRandomAction;

	[AssertNotNull]
	public GameObject jump_effect;

	[AssertNotNull]
	public GameObject trail;

	public FMODAsset jumpSound;

	public float jumpVelocity = 10f;

	public float walkGravity = 10f;

	public float jumpDuration = 2f;

	private float timeLastJump = -1f;

	private bool jumping;

	private State _state;

	public State state
	{
		get
		{
			return _state;
		}
		set
		{
			_state = value;
			if (_state == State.Walk)
			{
				descendForce.enabled = true;
				descendForce.force = new Vector3(0f, 0f - walkGravity, 0f);
			}
			else
			{
				descendForce.enabled = false;
			}
			Animator animator = GetAnimator();
			animator.SetBool("walk_mode", _state == State.Walk);
			animator.SetBool("dfift_mode", _state == State.Drift);
			locomotion.freezeHorizontalRotation = _state != 0 || jumping;
		}
	}

	public bool IsWalking()
	{
		if (state == State.Walk)
		{
			return !jumping;
		}
		return false;
	}

	public void Jump(float jumpVelocity)
	{
		if (!jumping && state == State.Walk)
		{
			locomotion.useRigidbody.AddForce(jumpVelocity * onSurfaceTracker.surfaceNormal, ForceMode.VelocityChange);
			timeLastJump = Time.time;
			GetAnimator().SetBool(AnimatorHashID.jump, value: true);
			Utils.SpawnPrefabAt(jump_effect, null, base.transform.position);
			Utils.SpawnPrefabAt(trail, base.transform, base.transform.position);
			if (jumpSound != null)
			{
				Utils.PlayFMODAsset(jumpSound, base.transform);
			}
			locomotion.freezeHorizontalRotation = true;
			jumping = true;
			Invoke("EndJump", jumpDuration);
		}
	}

	private void EndJump()
	{
		jumping = false;
		if (state == State.Swim)
		{
			locomotion.freezeHorizontalRotation = false;
			SwimRandom();
		}
		GetAnimator().SetBool(AnimatorHashID.jump, value: false);
	}

	public void SwimRandom()
	{
		TryStartAction(swimRandomAction);
	}
}
