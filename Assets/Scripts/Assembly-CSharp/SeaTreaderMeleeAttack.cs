using UnityEngine;

public class SeaTreaderMeleeAttack : MonoBehaviour
{
	public SeaTreader treader;

	public Animator animatior;

	public Transform attackTrigger;

	public Transform frontLeg;

	public SeaTreaderSounds soundsController;

	public float damage = 40f;

	public float attackInterval = 5f;

	public float frontAttackDelay = 1.3f;

	public float frontAttackDuration = 3.6f;

	public float downAttackDelay = 2f;

	public float downAttackDuration = 3f;

	private float lastAttackTime;

	private bool attacking;

	private float attackDelay;

	private float attackDuration;

	private bool frozen;

	private bool GetCanHit(GameObject otherGameObject)
	{
		if (!base.gameObject.GetComponent<LiveMixin>().IsAlive())
		{
			return false;
		}
		if (otherGameObject.GetComponent<LiveMixin>() == null)
		{
			return false;
		}
		Player component = otherGameObject.GetComponent<Player>();
		if (component == null || !component.CanBeAttacked())
		{
			return false;
		}
		return true;
	}

	private bool GetCanAttack(GameObject otherGameObject)
	{
		if (frozen || treader.cinematicMode || !treader.onSurfaceTracker.onSurface || Time.time <= lastAttackTime + attackInterval)
		{
			return false;
		}
		return GetCanHit(otherGameObject);
	}

	public void OnAttackTriggerEnter(Collider collider)
	{
		if (GetCanAttack(collider.gameObject))
		{
			attackDelay = frontAttackDelay;
			attackDuration = frontAttackDuration;
			Vector3 vector = attackTrigger.InverseTransformPoint(collider.transform.position);
			SafeAnimator.SetFloat(animatior, "attack_x", Mathf.Clamp(vector.x * 0.2f, -1f, 1f));
			SafeAnimator.SetFloat(animatior, "attack_y", Mathf.Clamp01(vector.y * 0.25f));
			attacking = true;
			lastAttackTime = Time.time;
			treader.cinematicMode = true;
			SafeAnimator.SetBool(animatior, "attacking", value: true);
			soundsController.OnAttack();
		}
	}

	public void OnDownAttackTriggerEnter(Collider collider)
	{
		if (GetCanAttack(collider.gameObject))
		{
			attackDelay = downAttackDelay;
			attackDuration = downAttackDuration;
			attacking = true;
			lastAttackTime = Time.time;
			treader.cinematicMode = true;
			SafeAnimator.SetBool(animatior, "attacking_down", value: true);
			soundsController.OnAttackDown();
		}
	}

	public void OnLegTouch(Collider collider)
	{
		GameObject gameObject = collider.gameObject;
		if (attacking && GetCanHit(gameObject) && Time.time > lastAttackTime + attackDelay)
		{
			Vector3 position = collider.ClosestPointOnBounds(frontLeg.position);
			LiveMixin component = gameObject.GetComponent<LiveMixin>();
			if (component.IsAlive())
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
		SafeAnimator.SetBool(animatior, "attacking", value: false);
		SafeAnimator.SetBool(animatior, "attacking_down", value: false);
		treader.cinematicMode = false;
	}

	private void OnFreezeByStasisSphere()
	{
		frozen = true;
	}

	private void OnUnfreezeByStasisSphere()
	{
		frozen = false;
	}
}
