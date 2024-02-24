using UnityEngine;

[RequireComponent(typeof(SwimBehaviour))]
public class AttachToVehicle : CreatureAction, IProtoEventListener, IPropulsionCannonAmmo, IOnTakeDamage
{
	private enum State
	{
		None = 0,
		SwimTo = 1,
		Transition = 2,
		Attached = 3
	}

	[AssertNotNull]
	public Rigidbody usedRigidboby;

	[AssertNotNull]
	public Collider usedCollider;

	[AssertNotNull]
	public Collider attachedHitTrigger;

	public FMOD_StudioEventEmitter attachSound;

	public float transitionTime = 0.5f;

	public float consumeEnergySpeed = 0.05f;

	public float attachPositionYOffset;

	public float scanInterval = 5f;

	public float swimInterval = 1f;

	public float swimVelocity = 3f;

	private LavaLarvaTarget currTarget;

	private LavaLarvaAttachPoint currentAttachPoint;

	private Vector3 startPos;

	private Quaternion startRot;

	private float timeTargetSet;

	private float timeDetached;

	private float timeNextSwim;

	private float timeNextScan;

	private EcoRegion.TargetFilter isTargetValidFilter;

	private State state;

	private float accumulatedDamage;

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		SetDetached();
	}

	void IPropulsionCannonAmmo.OnShoot()
	{
		if (IsAttached())
		{
			SetDetached();
		}
	}

	void IPropulsionCannonAmmo.OnRelease()
	{
	}

	void IPropulsionCannonAmmo.OnImpact()
	{
	}

	bool IPropulsionCannonAmmo.GetAllowedToGrab()
	{
		return true;
	}

	bool IPropulsionCannonAmmo.GetAllowedToShoot()
	{
		return true;
	}

	void IPropulsionCannonAmmo.OnGrab()
	{
		if (IsAttached())
		{
			SetDetached();
		}
	}

	private void Start()
	{
		DevConsole.RegisterConsoleCommand(this, "lldetach");
		isTargetValidFilter = IsValidTarget;
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (GameModeUtils.IsInvisible())
		{
			return 0f;
		}
		if (time > timeNextScan)
		{
			UpdateCurrentTarget();
			timeNextScan = time + scanInterval;
		}
		if (timeDetached + 4f < time && currTarget != null && (currTarget.transform.position - base.transform.position).sqrMagnitude <= currTarget.distanceToStartAction * currTarget.distanceToStartAction)
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	public override void StartPerform(Creature creature, float time)
	{
		currentAttachPoint = currTarget.GetClosestAttachPoint(base.transform.position);
		if (currentAttachPoint == null)
		{
			SetDetached();
			return;
		}
		currentAttachPoint.occupied = true;
		currentAttachPoint.lavaLarva = base.gameObject;
		state = State.SwimTo;
	}

	public override void StopPerform(Creature creature, float time)
	{
		creature.Aggression.Add(-1f);
		creature.leashPosition = base.transform.position;
		SetDetached();
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (currentAttachPoint == null)
		{
			return;
		}
		switch (state)
		{
		case State.SwimTo:
		{
			Vector3 vector = currentAttachPoint.transform.position - currentAttachPoint.transform.forward * 2f;
			if (time > timeNextSwim && currentAttachPoint != null)
			{
				timeNextSwim = time + swimInterval;
				base.swimBehaviour.SwimTo(vector, swimVelocity);
			}
			if ((vector - base.transform.position).sqrMagnitude < 1f)
			{
				state = State.Transition;
				timeTargetSet = time;
				startPos = base.transform.position;
				startRot = base.transform.rotation;
				currentAttachPoint.attached = true;
				SetAttached();
			}
			break;
		}
		case State.Attached:
			ConsumeEnergy(currTarget, deltaTime);
			break;
		}
	}

	public void SetDetached()
	{
		if (currTarget != null && currTarget.IsCyclops())
		{
			currTarget.subControl.BroadcastMessage("DetachedLavaLarva", base.gameObject, SendMessageOptions.DontRequireReceiver);
		}
		timeDetached = Time.time;
		usedRigidboby.isKinematic = false;
		usedCollider.isTrigger = false;
		attachedHitTrigger.enabled = false;
		ClearCurrentTarget();
		if (state == State.Attached)
		{
			usedRigidboby.AddForce(-base.transform.forward * 2f, ForceMode.VelocityChange);
		}
		state = State.None;
		SetLayer(LayerMask.NameToLayer("Default"));
		if (creature != null)
		{
			SafeAnimator.SetBool(creature.GetAnimator(), "attacking", value: false);
		}
		if (LargeWorld.main != null && LargeWorld.main.streamer != null && LargeWorld.main.streamer.cellManager != null)
		{
			LargeWorld.main.streamer.cellManager.RegisterEntity(base.gameObject);
		}
		accumulatedDamage = 0f;
	}

	private void SetAttached()
	{
		SetLayer(LayerMask.NameToLayer("Viewmodel"));
		SafeAnimator.SetBool(creature.GetAnimator(), "attacking", value: true);
		if (attachSound != null)
		{
			Utils.PlayEnvSound(attachSound);
		}
		usedRigidboby.isKinematic = true;
		usedCollider.isTrigger = true;
		if (currTarget != null)
		{
			if (currTarget.IsCyclops())
			{
				currTarget.subControl.BroadcastMessage("AttachedLavaLarva", base.gameObject, SendMessageOptions.DontRequireReceiver);
				attachedHitTrigger.enabled = true;
			}
			base.transform.parent = currTarget.larvaePrefabRoot;
			if (LargeWorld.main != null && LargeWorld.main.streamer != null && LargeWorld.main.streamer.cellManager != null)
			{
				LargeWorld.main.streamer.cellManager.UnregisterEntity(base.gameObject);
			}
		}
	}

	private bool IsAttached()
	{
		if (state != State.Attached)
		{
			return state == State.Transition;
		}
		return true;
	}

	private void ConsumeEnergy(LavaLarvaTarget target, float deltaTime)
	{
		if (target != null)
		{
			target.ConsumeEnergy(consumeEnergySpeed * deltaTime);
		}
	}

	private void Update()
	{
		if (state != 0 && (currentAttachPoint == null || currTarget == null || !currTarget.GetAllowedToAttach()))
		{
			SetDetached();
		}
		else if (state == State.Transition)
		{
			float value = (Time.time - timeTargetSet) / transitionTime;
			value = Mathf.Clamp01(value);
			Vector3 b = currentAttachPoint.transform.TransformPoint(attachPositionYOffset * Vector3.back * base.transform.localScale.x);
			Quaternion b2 = Quaternion.LookRotation(currentAttachPoint.transform.forward, base.transform.up);
			base.transform.position = Vector3.Lerp(startPos, b, value);
			base.transform.rotation = Quaternion.Slerp(startRot, b2, value);
			if (value == 1f)
			{
				state = State.Attached;
			}
		}
	}

	private void SetLayer(int layer)
	{
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = layer;
		}
	}

	private void OnKill()
	{
		if (IsAttached())
		{
			SetDetached();
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (IsAttached())
		{
			SetDetached();
		}
	}

	private void OnDisable()
	{
		ClearCurrentTarget();
	}

	private void UpdateCurrentTarget()
	{
		if (state == State.None && EcoRegionManager.main != null)
		{
			IEcoTarget ecoTarget = EcoRegionManager.main.FindNearestTarget(EcoTargetType.HeatSource, base.transform.position, isTargetValidFilter);
			LavaLarvaTarget lavaLarvaTarget = null;
			if (ecoTarget != null)
			{
				lavaLarvaTarget = ecoTarget.GetGameObject().GetComponent<LavaLarvaTarget>();
			}
			if (lavaLarvaTarget != currTarget)
			{
				ClearCurrentTarget();
				currTarget = lavaLarvaTarget;
			}
		}
	}

	private bool IsValidTarget(IEcoTarget target)
	{
		GameObject gameObject = target.GetGameObject();
		if (gameObject == null)
		{
			return false;
		}
		LavaLarvaTarget component = gameObject.GetComponent<LavaLarvaTarget>();
		if (component != null)
		{
			return component.GetAllowedToAttach();
		}
		return false;
	}

	private void ClearCurrentTarget()
	{
		if (currentAttachPoint != null)
		{
			currentAttachPoint.Clear();
			currentAttachPoint = null;
		}
		currTarget = null;
	}

	private void OnConsoleCommand_lldetach(NotificationCenter.Notification n)
	{
		SetDetached();
	}

	public void OnTakeDamage(DamageInfo damageInfo)
	{
		if (state == State.Attached)
		{
			accumulatedDamage += damageInfo.damage;
			if (accumulatedDamage > 30f)
			{
				SetDetached();
			}
		}
	}
}
