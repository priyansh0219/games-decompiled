using UnityEngine;

[RequireComponent(typeof(IDrownableCreature))]
public class Drowning : MonoBehaviour
{
	public float damage = 2f;

	public float damageInterval = 1f;

	public Animator animator;

	private bool drowning;

	private float timeNextDamage;

	private LiveMixin liveMixin;

	private IDrownableCreature creature;

	private void Start()
	{
		creature = GetComponent<IDrownableCreature>();
		liveMixin = GetComponent<LiveMixin>();
	}

	private void Update()
	{
		bool flag = base.transform.position.y < 0f;
		if (flag != drowning)
		{
			drowning = flag;
			creature.drowning = drowning;
			SafeAnimator.SetBool(animator, "drowning", drowning);
		}
		if (drowning && Time.time > timeNextDamage)
		{
			timeNextDamage = Time.time + damageInterval;
			if (liveMixin != null)
			{
				liveMixin.TakeDamage(damage);
			}
		}
	}
}
