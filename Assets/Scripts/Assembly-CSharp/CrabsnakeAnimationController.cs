using UnityEngine;

public class CrabsnakeAnimationController : MonoBehaviour
{
	public Vector3 enterAnimPosOffset = Vector3.zero;

	public Vector3 exitAnimPosOffset = Vector3.zero;

	public float enterAnimInterpolationTime = 0.25f;

	[AssertNotNull]
	public CrabSnake crabsnake;

	[AssertNotNull]
	public TrailManager trailManager;

	[AssertNotNull]
	public Animator animator;

	[AssertNotNull]
	public Transform rootTransform;

	private Transform target;

	private bool initialized;

	private bool interpolation;

	private float startTime;

	private Vector3 startPosition;

	private Vector3 targetPosition;

	private Quaternion startRotation;

	private Quaternion targetRotation;

	private Vector3 rootTransformLocPosition;

	private Vector3 defaultLocalPosition;

	private Vector3 mushroomPosition;

	private Vector3 lookDirection = Vector3.forward;

	private Vector3 upDirection = Vector3.up;

	public bool IsInTransition { get; private set; }

	public void Initialize(bool isInMushroom)
	{
		if (!initialized)
		{
			defaultLocalPosition = base.transform.localPosition;
			rootTransformLocPosition = base.transform.InverseTransformPoint(rootTransform.position);
			initialized = true;
		}
		if (isInMushroom)
		{
			mushroomPosition = crabsnake.mushroomPosition;
			rootTransform.position = mushroomPosition;
			rootTransform.rotation = crabsnake.mushroomRotation;
			base.transform.localPosition = defaultLocalPosition + exitAnimPosOffset;
			upDirection = crabsnake.mushroomUp;
			trailManager.SetEnabled(state: false);
		}
		SafeAnimator.SetBool(animator, "has_home_mushroom", isInMushroom);
		SafeAnimator.SetBool(animator, "chase_mode", !isInMushroom);
	}

	private void LateUpdate()
	{
		switch (crabsnake.state)
		{
		case CrabSnake.State.InMushroom:
			if (interpolation)
			{
				float num2 = Mathf.Clamp01((Time.time - startTime) / enterAnimInterpolationTime);
				base.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, num2);
				rootTransform.rotation = Quaternion.Lerp(startRotation, targetRotation, num2);
				if (num2 == 1f)
				{
					interpolation = false;
				}
			}
			break;
		case CrabSnake.State.Coil:
		{
			SetLookDirection(base.transform.position, target.position, Vector3.up);
			Quaternion quaternion = Quaternion.LookRotation(lookDirection);
			if (interpolation)
			{
				float num = Mathf.Clamp01((Time.time - startTime) / enterAnimInterpolationTime);
				base.transform.rotation = Quaternion.Lerp(startRotation, quaternion, num);
				if (num == 1f)
				{
					interpolation = false;
				}
			}
			else
			{
				base.transform.rotation = quaternion;
			}
			break;
		}
		}
	}

	public void EnterMushroom()
	{
		IsInTransition = true;
		mushroomPosition = crabsnake.mushroomPosition;
		upDirection = crabsnake.mushroomUp;
		SetLookDirection(rootTransform.position, mushroomPosition, upDirection);
		startTime = Time.time;
		Vector3 position = base.transform.position;
		rootTransform.position = mushroomPosition;
		base.transform.position = position;
		startPosition = base.transform.localPosition;
		targetPosition = defaultLocalPosition + enterAnimPosOffset;
		startRotation = rootTransform.rotation;
		targetRotation = Quaternion.LookRotation(lookDirection, upDirection);
		SafeAnimator.SetBool(animator, "has_home_mushroom", value: true);
		SafeAnimator.SetBool(animator, "chase_mode", value: false);
		interpolation = true;
	}

	public void OnEnterAnimationEnd()
	{
		IsInTransition = false;
		base.transform.localPosition = defaultLocalPosition + exitAnimPosOffset;
		trailManager.SetEnabled(state: false);
	}

	public void ExitMushroom(Vector3 targetPos)
	{
		IsInTransition = true;
		SetLookDirection(mushroomPosition, targetPos, upDirection);
		rootTransform.rotation = Quaternion.LookRotation(lookDirection, upDirection);
		SafeAnimator.SetBool(animator, "has_home_mushroom", value: true);
		SafeAnimator.SetBool(animator, "chase_mode", value: true);
	}

	public void OnExitAnimationEnd()
	{
		IsInTransition = false;
		rootTransform.position = base.transform.TransformPoint(rootTransformLocPosition);
		base.transform.localPosition = defaultLocalPosition;
		trailManager.SetEnabled(state: true);
		crabsnake.EndExitMushroom();
	}

	public void StartMushroomAttack(Vector3 targetPos, bool isHigh)
	{
		base.transform.localPosition = defaultLocalPosition + exitAnimPosOffset;
		SetLookDirection(mushroomPosition, targetPos, upDirection);
		rootTransform.rotation = Quaternion.LookRotation(lookDirection, upDirection);
		if (isHigh)
		{
			SafeAnimator.SetBool(animator, "mushroom_high_attack", value: true);
		}
		else
		{
			SafeAnimator.SetBool(animator, "mushroom_low_attack", value: true);
		}
	}

	public void EndMushroomAttack()
	{
		SafeAnimator.SetBool(animator, "mushroom_high_attack", value: false);
		SafeAnimator.SetBool(animator, "mushroom_low_attack", value: false);
	}

	public void OnMushroomAttackAnimationEnd()
	{
		base.transform.localPosition = defaultLocalPosition + exitAnimPosOffset;
	}

	public void EnterCoil(Transform target)
	{
		this.target = target.transform;
		SafeAnimator.SetBool(animator, "coil_mode", value: true);
		trailManager.SetEnabled(state: false);
		startRotation = base.transform.rotation;
		startTime = Time.time;
		interpolation = true;
	}

	public void ExitCoil()
	{
		SafeAnimator.SetBool(animator, "coil_mode", value: false);
		rootTransform.position = base.transform.TransformPoint(rootTransformLocPosition);
		rootTransform.rotation *= base.transform.localRotation;
		base.transform.localRotation = Quaternion.identity;
	}

	public void OnCoilAnimationEnd()
	{
		trailManager.SetEnabled(state: true);
	}

	private void SetLookDirection(Vector3 fromPosition, Vector3 toPosition, Vector3 up)
	{
		lookDirection = toPosition - fromPosition;
		if (up == Vector3.up)
		{
			lookDirection.y = 0f;
		}
		else
		{
			lookDirection = Vector3.ProjectOnPlane(lookDirection, upDirection);
		}
		lookDirection = lookDirection.normalized;
	}
}
