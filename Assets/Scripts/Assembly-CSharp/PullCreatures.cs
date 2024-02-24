using System.Collections.Generic;
using UWE;
using UnityEngine;

public class PullCreatures : CreatureAction
{
	[AssertNotNull]
	public LastTarget lastTarget;

	[AssertNotNull]
	public Rigidbody myRigidbody;

	public float minDistance = 2f;

	public float maxDistance = 20f;

	public float pauseInterval = 10f;

	public float swimVelocity = 0.1f;

	public float swimInterval = 0.5f;

	public float targetCreatureVelocity = 1f;

	public float pullPlayerSpeed = 1.5f;

	public float lerpCameraMinSpeed = 1f;

	public float lerpCameraMaxSpeed = 5f;

	[AssertNotNull]
	public AnimationCurve distanceHypnFactor;

	[AssertNotNull]
	public AnimationCurve lookAngleHypnFactor;

	private static List<GameObject> allTargets = new List<GameObject>();

	private bool isActive;

	private float timeNextSwim;

	private float lastActionTime;

	private Creature targetCreature;

	private SwimBehaviour targetSwimBehaviour;

	private MesmerizedScreenFXController screenFX;

	private GameObject _currentTarget;

	public FMOD_CustomLoopingEmitter loopingSound;

	private bool creatureWasEnabled;

	private bool targetLocomotionWasEnabled;

	private GameObject currentTarget
	{
		get
		{
			return _currentTarget;
		}
		set
		{
			if (_currentTarget != null)
			{
				ReleaseTarget(_currentTarget);
			}
			_currentTarget = value;
			if (_currentTarget != null)
			{
				CaptureTarget(_currentTarget);
			}
		}
	}

	private bool LooksAtMe(GameObject target)
	{
		Transform transform = ((!(Player.main.gameObject == target)) ? target.transform : MainCameraControl.main.transform);
		Vector3 forward = transform.forward;
		Vector3 vector = Vector3.Normalize(base.transform.position - target.transform.position);
		if (Vector3.Dot(vector, forward) < 0.65f)
		{
			return false;
		}
		int num = UWE.Utils.RaycastIntoSharedBuffer(transform.position, vector, Vector3.Distance(base.transform.position, transform.position), -1, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = UWE.Utils.sharedHitBuffer[i];
			GameObject gameObject = ((raycastHit.collider.attachedRigidbody == null) ? raycastHit.collider.gameObject : raycastHit.collider.attachedRigidbody.gameObject);
			if (gameObject != base.gameObject && gameObject != target)
			{
				return false;
			}
		}
		return true;
	}

	private bool IsValidTarget(GameObject target)
	{
		if (target == null)
		{
			return false;
		}
		if (target != currentTarget && allTargets.Contains(target))
		{
			return false;
		}
		if (!target.CompareTag("Creature") && !target.CompareTag("Player"))
		{
			return false;
		}
		if (Player.main.gameObject == target && (!Player.main.CanBeAttacked() || !Player.main.IsUnderwater()))
		{
			return false;
		}
		if (target.GetComponent<PropulseCannonAmmoHandler>() != null)
		{
			return false;
		}
		float sqrMagnitude = (target.transform.position - base.transform.position).sqrMagnitude;
		if (sqrMagnitude > maxDistance * maxDistance || sqrMagnitude < minDistance * minDistance)
		{
			return false;
		}
		LiveMixin component = target.GetComponent<LiveMixin>();
		if (component == null || !component.IsAlive())
		{
			return false;
		}
		return LooksAtMe(target);
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (!isActive)
		{
			if (time > lastActionTime + pauseInterval && IsValidTarget(lastTarget.target))
			{
				return GetEvaluatePriority();
			}
			return 0f;
		}
		if (currentTarget != null && !IsValidTarget(currentTarget))
		{
			currentTarget = null;
		}
		if (currentTarget != null)
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	private void Start()
	{
		screenFX = MainCamera.camera.GetComponent<MesmerizedScreenFXController>();
	}

	public override void StartPerform(Creature behavior, float time)
	{
		currentTarget = lastTarget.target;
		isActive = true;
		timeNextSwim = time;
		if (currentTarget != null && currentTarget.CompareTag("Player"))
		{
			if (loopingSound != null)
			{
				loopingSound.Play();
			}
			if (screenFX != null)
			{
				screenFX.StartHypnose();
			}
		}
	}

	public override void StopPerform(Creature behavior, float time)
	{
		currentTarget = null;
		isActive = false;
		lastActionTime = time;
		if (loopingSound != null)
		{
			loopingSound.Stop();
		}
		if (screenFX != null)
		{
			screenFX.StopHypnose();
		}
	}

	private void Update()
	{
		if (!isActive || !(currentTarget != null))
		{
			return;
		}
		Vector3 vector = currentTarget.transform.position - base.transform.position;
		float magnitude = vector.magnitude;
		vector = vector.normalized;
		if (Time.time >= timeNextSwim)
		{
			base.swimBehaviour.SwimTo(currentTarget.transform.position, vector, swimVelocity);
			if (targetSwimBehaviour != null)
			{
				targetSwimBehaviour.SwimTo(base.transform.position, -vector, targetCreatureVelocity);
			}
		}
		Quaternion quaternion = Quaternion.LookRotation(-vector);
		if (currentTarget.gameObject == Player.main.gameObject)
		{
			float num = distanceHypnFactor.Evaluate(Mathf.InverseLerp(minDistance, maxDistance, magnitude));
			float value = Vector3.Dot(-vector, MainCameraControl.main.transform.forward);
			float num2 = lookAngleHypnFactor.Evaluate(Mathf.InverseLerp(1f, 0.65f, value));
			float num3 = Mathf.Clamp01(num * num2);
			Player.main.mesmerizedSpeedMultiplier = 1f - num3;
			float t = Mathf.Lerp(lerpCameraMinSpeed, lerpCameraMaxSpeed, num3) * Time.deltaTime;
			Vector3 eulerAngles = quaternion.eulerAngles;
			MainCameraControl.main.rotationX = Mathf.LerpAngle(MainCameraControl.main.rotationX, eulerAngles.y - Player.main.transform.localEulerAngles.y, t);
			MainCameraControl.main.rotationY = Mathf.LerpAngle(MainCameraControl.main.rotationY, 0f - eulerAngles.x, t);
			if (magnitude > minDistance)
			{
				currentTarget.transform.position = currentTarget.transform.position - vector * pullPlayerSpeed * Time.deltaTime;
			}
		}
	}

	public void OnMeleeAttack(GameObject gameObject)
	{
		currentTarget = null;
	}

	private void OnDisable()
	{
		if (isActive)
		{
			StopPerform(creature, Time.time);
		}
	}

	private void OnKill()
	{
		if (isActive)
		{
			StopPerform(creature, Time.time);
		}
	}

	private void CaptureTarget(GameObject target)
	{
		Creature component = target.GetComponent<Creature>();
		if (component != null)
		{
			targetCreature = component;
			creatureWasEnabled = component.enabled;
			component.enabled = false;
		}
		targetSwimBehaviour = target.GetComponent<SwimBehaviour>();
		allTargets.Add(target);
	}

	private void ReleaseTarget(GameObject target)
	{
		if (creatureWasEnabled)
		{
			LiveMixin component = target.GetComponent<LiveMixin>();
			if (component != null && component.IsAlive())
			{
				PropulseCannonAmmoHandler component2 = target.GetComponent<PropulseCannonAmmoHandler>();
				if (component2 != null)
				{
					component2.behaviorWasEnabled = true;
				}
				else if (targetCreature != null)
				{
					targetCreature.enabled = true;
				}
			}
			creatureWasEnabled = false;
		}
		targetCreature = null;
		if ((bool)Player.main && target.gameObject == Player.main.gameObject)
		{
			Player.main.mesmerizedSpeedMultiplier = 1f;
			VRUtil.Recenter();
		}
		allTargets.Remove(target);
	}
}
