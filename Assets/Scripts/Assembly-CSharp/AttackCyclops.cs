using System.Collections.Generic;
using UnityEngine;

public class AttackCyclops : CreatureAction
{
	[AssertNotNull]
	public LastTarget lastTarget;

	public static readonly HashSet<GameObject> attackCyclopsCreatureHashSet = new HashSet<GameObject>();

	public float aggressPerSecond = 0.5f;

	public float attackAggressionThreshold = 0.75f;

	public float attackPause = 5f;

	public float maxDistToLeash = 30f;

	public float swimVelocity = 10f;

	public float swimInterval = 0.8f;

	[AssertNotNull]
	public CreatureTrait aggressiveToNoise;

	private const float updateAggressionInterval = 0.5f;

	private bool isActive;

	private GameObject currentTarget;

	private bool currentTargetIsDecoy;

	private float timeLastAttack;

	private float timeNextSwim;

	private Vector3 targetAttackPoint;

	private CyclopsNoiseManager forcedNoiseManager;

	private const float maxAttackCyclopsRange = 150f;

	private const float attackDecoyRange = 150f;

	private EcoRegion.TargetFilter isTargetValidFilter;

	private void Start()
	{
		DevConsole.RegisterConsoleCommand(this, "attacksub");
		isTargetValidFilter = IsTargetValid;
	}

	public override void OnEnable()
	{
		base.OnEnable();
		attackCyclopsCreatureHashSet.Add(base.gameObject);
		InvokeRepeating("UpdateAggression", Random.value * 0.5f, 0.5f);
	}

	private void OnDisable()
	{
		attackCyclopsCreatureHashSet.Remove(base.gameObject);
		aggressiveToNoise.Value = 0f;
		CancelInvoke();
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (currentTarget != null && aggressiveToNoise.Value > attackAggressionThreshold && time > timeLastAttack + attackPause)
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	public override void StartPerform(Creature creature, float time)
	{
		SafeAnimator.SetBool(creature.GetAnimator(), "attacking", value: true);
		UpdateAttackPoint();
		lastTarget.SetLockedTarget(currentTarget);
		isActive = true;
	}

	public override void StopPerform(Creature creature, float time)
	{
		SafeAnimator.SetBool(creature.GetAnimator(), "attacking", value: false);
		lastTarget.UnlockTarget();
		lastTarget.SetTarget(null);
		isActive = false;
		StopAttack();
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (time > timeNextSwim && currentTarget != null)
		{
			timeNextSwim = time + swimInterval;
			Vector3 targetPosition = (currentTargetIsDecoy ? currentTarget.transform.position : currentTarget.transform.TransformPoint(targetAttackPoint));
			base.swimBehaviour.SwimTo(targetPosition, swimVelocity);
		}
		creature.Aggression.Value = aggressiveToNoise.Value;
	}

	public void OnMeleeAttack(GameObject target)
	{
		if (target == currentTarget)
		{
			StopAttack();
		}
	}

	public bool IsAggressiveTowardsCyclops(GameObject cyclopsGameObject)
	{
		if (currentTarget == cyclopsGameObject)
		{
			return aggressiveToNoise.Value > 0.4f;
		}
		return false;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (Player.main != null && Player.main.currentSub != null && Player.main.currentSub.isCyclops && Player.main.currentSub.gameObject == collision.gameObject && !isActive && (!(currentTarget != null) || !currentTargetIsDecoy) && !(Vector3.Dot(collision.contacts[0].normal, collision.rigidbody.velocity) < 2.5f))
		{
			currentTarget = collision.gameObject;
			aggressiveToNoise.Value = 1f;
		}
	}

	protected void StopAttack()
	{
		aggressiveToNoise.Value = 0f;
		creature.Aggression.Value = 0f;
		timeLastAttack = Time.time;
	}

	private CyclopsDecoy GetClosestDecoy()
	{
		return EcoRegionManager.main.FindNearestTarget(EcoTargetType.SubDecoy, base.transform.position, isTargetValidFilter, 2)?.GetGameObject().GetComponent<CyclopsDecoy>();
	}

	private bool IsTargetValid(IEcoTarget target)
	{
		return Vector3.Distance(target.GetPosition(), base.transform.position) < 150f;
	}

	private void UpdateAggression()
	{
		aggressiveToNoise.UpdateTrait(0.5f);
		if (Time.time < timeLastAttack + attackPause)
		{
			return;
		}
		GameObject gameObject = null;
		CyclopsNoiseManager cyclopsNoiseManager = null;
		CyclopsDecoy closestDecoy = GetClosestDecoy();
		if (closestDecoy == null)
		{
			if (Player.main != null && Player.main.currentSub != null && Player.main.currentSub.isCyclops)
			{
				cyclopsNoiseManager = Player.main.currentSub.noiseManager;
			}
			else if (forcedNoiseManager != null)
			{
				cyclopsNoiseManager = forcedNoiseManager;
			}
		}
		if (closestDecoy != null || cyclopsNoiseManager != null)
		{
			gameObject = ((closestDecoy == null) ? cyclopsNoiseManager.gameObject : closestDecoy.gameObject);
			float num = 0f;
			Vector3 position;
			if (closestDecoy == null)
			{
				position = cyclopsNoiseManager.transform.position;
				float noisePercent = cyclopsNoiseManager.GetNoisePercent();
				num = Mathf.Lerp(0f, 150f, noisePercent);
			}
			else
			{
				position = closestDecoy.transform.position;
				num = 150f;
			}
			if (Vector3.Distance(position, base.transform.position) < num && Vector3.Distance(position, creature.leashPosition) < maxDistToLeash)
			{
				aggressiveToNoise.Add(aggressPerSecond * 0.5f);
			}
		}
		if (gameObject != null || !currentTargetIsDecoy)
		{
			SetCurrentTarget(gameObject, closestDecoy != null);
		}
	}

	private void SetCurrentTarget(GameObject target, bool isDecoy)
	{
		if (currentTarget != target)
		{
			currentTarget = target;
			currentTargetIsDecoy = isDecoy;
			if (isActive)
			{
				UpdateAttackPoint();
				lastTarget.SetLockedTarget(currentTarget);
			}
		}
	}

	private void UpdateAttackPoint()
	{
		targetAttackPoint = Vector3.zero;
		if (!currentTargetIsDecoy && currentTarget != null)
		{
			Vector3 vector = currentTarget.transform.InverseTransformPoint(base.transform.position);
			targetAttackPoint.z = Mathf.Clamp(vector.z, -26f, 26f);
		}
	}

	private void OnConsoleCommand_attacksub(NotificationCenter.Notification n)
	{
		CyclopsNoiseManager[] componentsInChildren = LargeWorldStreamer.main.globalRoot.GetComponentsInChildren<CyclopsNoiseManager>();
		forcedNoiseManager = null;
		float num = float.PositiveInfinity;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			float num2 = Vector3.Distance(base.transform.position, componentsInChildren[i].transform.position);
			if (num2 < num)
			{
				forcedNoiseManager = componentsInChildren[i];
				num = num2;
			}
		}
	}
}
