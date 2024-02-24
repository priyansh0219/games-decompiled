using UnityEngine;

[RequireComponent(typeof(LastTarget))]
[RequireComponent(typeof(SwimBehaviour))]
public class AttackLastTarget : CreatureAction
{
	public float swimVelocity = 10f;

	public float swimInterval = 0.8f;

	public float aggressionThreshold = 0.75f;

	public float minAttackDuration = 3f;

	public float maxAttackDuration = 7f;

	public float pauseInterval = 20f;

	public float rememberTargetTime = 5f;

	public bool resetAggressionOnTime = true;

	[AssertNotNull]
	public LastTarget lastTarget;

	public FMOD_CustomEmitter attackStartSound;

	public VFXController attackStartFXcontrol;

	private float timeStartAttack;

	private float timeStopAttack;

	private float timeNextSwim;

	protected GameObject currentTarget;

	public override void StartPerform(Creature creature, float time)
	{
		timeStartAttack = time;
		if ((bool)attackStartSound)
		{
			attackStartSound.Play();
		}
		if (attackStartFXcontrol != null)
		{
			attackStartFXcontrol.Play();
		}
		SafeAnimator.SetBool(creature.GetAnimator(), "attacking", value: true);
	}

	public override void StopPerform(Creature creature, float time)
	{
		SafeAnimator.SetBool(creature.GetAnimator(), "attacking", value: false);
		if (attackStartFXcontrol != null)
		{
			attackStartFXcontrol.Stop();
		}
		currentTarget = null;
		timeStopAttack = time;
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (time > timeNextSwim && currentTarget != null)
		{
			timeNextSwim = time + swimInterval;
			Vector3 position = currentTarget.transform.position;
			Vector3 targetDirection = ((!(currentTarget.GetComponent<Player>() != null)) ? (currentTarget.transform.position - base.transform.position).normalized : (-MainCamera.camera.transform.forward));
			base.swimBehaviour.Attack(position, targetDirection, swimVelocity);
		}
		if (resetAggressionOnTime && time > timeStartAttack + maxAttackDuration)
		{
			StopAttack();
		}
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (((creature.Aggression.Value > aggressionThreshold) | (time < timeStartAttack + minAttackDuration)) & (time > timeStopAttack + pauseInterval))
		{
			if (lastTarget.target != null && time <= lastTarget.targetTime + rememberTargetTime && !lastTarget.targetLocked)
			{
				currentTarget = lastTarget.target;
			}
			if (!CanAttackTarget(currentTarget))
			{
				currentTarget = null;
			}
			if (currentTarget != null)
			{
				return GetEvaluatePriority();
			}
		}
		return 0f;
	}

	public void OnMeleeAttack(GameObject target)
	{
		if (target == currentTarget)
		{
			StopAttack();
		}
	}

	protected virtual bool CanAttackTarget(GameObject target)
	{
		if (target == null)
		{
			return false;
		}
		if (creature.IsFriendlyTo(target))
		{
			return false;
		}
		LiveMixin component = target.GetComponent<LiveMixin>();
		if (!component)
		{
			return false;
		}
		if (!component.IsAlive())
		{
			return false;
		}
		if (target == Player.main.gameObject && !Player.main.CanBeAttacked())
		{
			return false;
		}
		return true;
	}

	protected virtual void StopAttack()
	{
		creature.Aggression.Value = 0f;
		timeStopAttack = Time.time;
		lastTarget.SetTarget(null);
		if (attackStartFXcontrol != null)
		{
			attackStartFXcontrol.Stop();
		}
	}

	public override string GetDebugString()
	{
		string text = base.GetDebugString();
		if (currentTarget != null)
		{
			text = $"{text}: {currentTarget.name}";
		}
		return text;
	}
}
