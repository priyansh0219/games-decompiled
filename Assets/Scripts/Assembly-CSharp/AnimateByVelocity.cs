using UnityEngine;

[DisallowMultipleComponent]
public class AnimateByVelocity : MonoBehaviour, IManagedUpdateBehaviour, IManagedBehaviour, ICompileTimeCheckable
{
	[AssertNotNull]
	public Animator animator;

	public float animationMoveMaxSpeed = 4f;

	public float animationMaxPitch = 30f;

	public float animationMaxTilt = 45f;

	public bool useStrafeAnimation;

	[AssertNotNull]
	public GameObject rootGameObject;

	private Rigidbody rootRigidbody;

	public float dampTime = 0.5f;

	private Vector3 previousEulerHeading = Vector3.zero;

	private Vector3 previousPosition;

	private const float locomotionPitchFactor = 10f;

	private const float locomotionTiltFactor = 10f;

	private const float locomotionDampFactor = 0.4f;

	private float relativeVelocityX;

	private float relativeVelocityY;

	private float relativeVelocityZ;

	private float animPitchValue;

	private float animTiltValue;

	private float animSpeedValue;

	[AssertNotNull]
	public BehaviourLOD levelOfDetail;

	private float lastUpdateTime;

	private float timeBetweenMediumLODUpdates = 0.5f;

	private float animMoveMaxSpeedLastValue = 4f;

	public int managedUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "AnimateByVelocity";
	}

	private void OnValidate()
	{
		if (animationMoveMaxSpeed == 0f)
		{
			animationMoveMaxSpeed = animMoveMaxSpeedLastValue;
		}
		animMoveMaxSpeedLastValue = animationMoveMaxSpeed;
	}

	private void OnEnable()
	{
		BehaviourUpdateUtils.Register(this);
	}

	private void OnDisable()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	private void OnDestroy()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	private void Start()
	{
		if (rootGameObject == null)
		{
			rootGameObject = base.gameObject;
		}
		animator.logWarnings = true;
		animator.SetFloat(AnimatorHashID.speed, 0f);
		animator.SetFloat(AnimatorHashID.pitch, 0f);
		animator.SetFloat(AnimatorHashID.tilt, 0f);
		previousPosition = rootGameObject.transform.position;
		rootRigidbody = rootGameObject.GetComponent<Rigidbody>();
	}

	public void ManagedUpdate()
	{
		Vector3 position = rootGameObject.transform.position;
		float num = Time.deltaTime;
		switch (levelOfDetail.current)
		{
		case LODState.Minimal:
			previousPosition = position;
			lastUpdateTime = Time.time;
			return;
		case LODState.Medium:
		{
			float num2 = Time.time - lastUpdateTime;
			if (num2 < timeBetweenMediumLODUpdates)
			{
				UpdateAnimatorData(num);
				return;
			}
			num = num2;
			break;
		}
		}
		Vector3 vector = Vector3.zero;
		if (rootRigidbody != null)
		{
			vector = rootRigidbody.velocity;
		}
		else if (num > 0f)
		{
			vector = (position - previousPosition) / num;
		}
		float magnitude = vector.magnitude;
		float num3 = 0f;
		float num4 = 0f;
		if (magnitude > 0.0001f)
		{
			Vector3 eulerAngles = rootGameObject.transform.eulerAngles;
			float num5 = Mathf.Sign(base.transform.up.y);
			num3 = 0f - Mathf.DeltaAngle(previousEulerHeading.x, eulerAngles.x);
			num3 *= 10f * num5;
			num4 = Mathf.DeltaAngle(previousEulerHeading.y, eulerAngles.y);
			num4 *= 10f * num5;
			previousEulerHeading = eulerAngles;
		}
		animSpeedValue = Mathf.Clamp(magnitude / animationMoveMaxSpeed, 0f, 1f);
		if (useStrafeAnimation)
		{
			Vector3 vector2 = base.transform.InverseTransformVector(vector);
			relativeVelocityX = Mathf.Clamp(vector2.x / animationMoveMaxSpeed, -1f, 1f);
			relativeVelocityY = Mathf.Clamp(vector2.y / animationMoveMaxSpeed, -1f, 1f);
			relativeVelocityZ = Mathf.Clamp(vector2.z / animationMoveMaxSpeed, -1f, 1f);
		}
		if (animationMaxPitch > 0f)
		{
			animPitchValue = Mathf.Clamp(num3 / animationMaxPitch, -1f, 1f);
		}
		if (animationMaxTilt > 0f)
		{
			animTiltValue = Mathf.Clamp(num4 / animationMaxTilt, -1f, 1f);
		}
		UpdateAnimatorData(Time.deltaTime);
		lastUpdateTime = Time.time;
		previousPosition = position;
	}

	private void UpdateAnimatorData(float deltaTime)
	{
		float num = dampTime * 0.4f;
		animator.SetFloat(AnimatorHashID.speed, animSpeedValue, num, deltaTime);
		if (useStrafeAnimation)
		{
			animator.SetFloat(AnimatorHashID.speed_x, relativeVelocityX, num, deltaTime);
			animator.SetFloat(AnimatorHashID.speed_y, relativeVelocityY, num, deltaTime);
			animator.SetFloat(AnimatorHashID.speed_z, relativeVelocityZ, num, deltaTime);
		}
		if (animationMaxPitch > 0f)
		{
			animator.SetFloat(AnimatorHashID.pitch, animPitchValue, num, deltaTime);
		}
		if (animationMaxTilt > 0f)
		{
			animator.SetFloat(AnimatorHashID.tilt, animTiltValue, num, deltaTime);
		}
	}

	public void OnKill()
	{
		animator.SetFloat(AnimatorHashID.speed, 0f);
		animator.SetFloat(AnimatorHashID.pitch, 0f);
		animator.SetFloat(AnimatorHashID.tilt, 0f);
		animator.SetBool(AnimatorHashID.dead, value: true);
		base.enabled = false;
	}

	public void EvaluateRandom()
	{
		SafeAnimator.SetFloat(animator, "random", Random.value);
	}

	public void EvaluateFidget()
	{
		SafeAnimator.SetFloat(animator, "fidget", Random.value);
	}

	public string CompileTimeCheck()
	{
		return null;
	}
}
