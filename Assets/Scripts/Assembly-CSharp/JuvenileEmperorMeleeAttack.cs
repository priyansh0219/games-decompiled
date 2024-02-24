using UnityEngine;

public class JuvenileEmperorMeleeAttack : MonoBehaviour
{
	[AssertNotNull]
	public Transform leftClaw;

	[AssertNotNull]
	public Transform rightClaw;

	[AssertNotNull]
	public Animator animator;

	[AssertNotNull]
	public LiveMixin liveMixin;

	public float damage = 60f;

	public float attackInterval = 5f;

	public float attackDelay = 0.33f;

	public float attackDuration = 1.5f;

	private float lastAttackTime;

	private bool attacking;

	private bool frozen;

	private bool CanHit(GameObject target)
	{
		if (!liveMixin.IsAlive())
		{
			return false;
		}
		if (!target.CompareTag("Creature"))
		{
			return false;
		}
		if (CreatureData.GetBehaviourType(target) != BehaviourType.Leviathan)
		{
			return false;
		}
		return true;
	}

	private bool GetCanAttack(GameObject otherGameObject)
	{
		if (frozen || Time.time <= lastAttackTime + attackDuration + attackInterval)
		{
			return false;
		}
		return CanHit(otherGameObject);
	}

	private GameObject GetTarget(Collider collider)
	{
		GameObject gameObject = collider.gameObject;
		if (gameObject.GetComponent<LiveMixin>() == null && collider.attachedRigidbody != null)
		{
			gameObject = collider.attachedRigidbody.gameObject;
		}
		return gameObject;
	}

	public void OnTouchFront(Collider collider)
	{
		OnAttackTriggerEnter(collider, "attack_front");
	}

	public void OnTouchLeft(Collider collider)
	{
		OnAttackTriggerEnter(collider, "attack_left");
	}

	public void OnTouchRight(Collider collider)
	{
		OnAttackTriggerEnter(collider, "attack_right");
	}

	private void OnAttackTriggerEnter(Collider collider, string animParameter)
	{
		if (!attacking && GetCanAttack(GetTarget(collider)))
		{
			SafeAnimator.SetBool(animator, animParameter, value: true);
			lastAttackTime = Time.time;
			attacking = true;
		}
	}

	public void OnLeftClawTouch(Collider collider)
	{
		OnClawTouch(leftClaw, collider);
	}

	public void OnRightClawTouch(Collider collider)
	{
		OnClawTouch(rightClaw, collider);
	}

	private void OnClawTouch(Transform claw, Collider collider)
	{
		if (!attacking || !(Time.time > lastAttackTime + attackDelay))
		{
			return;
		}
		GameObject target = GetTarget(collider);
		if (CanHit(target))
		{
			Vector3 position = collider.ClosestPointOnBounds(claw.position);
			LiveMixin component = target.GetComponent<LiveMixin>();
			if (component != null && component.IsAlive())
			{
				component.TakeDamage(damage, position, DamageType.Normal, base.gameObject);
				component.NotifyCreatureDeathsOfCreatureAttack();
			}
			EndAttack();
		}
	}

	private void Update()
	{
		if (attacking && Time.time > lastAttackTime + attackDuration)
		{
			EndAttack();
		}
	}

	private void EndAttack()
	{
		attacking = false;
		SafeAnimator.SetBool(animator, "attack_front", value: false);
		SafeAnimator.SetBool(animator, "attack_left", value: false);
		SafeAnimator.SetBool(animator, "attack_right", value: false);
	}
}
