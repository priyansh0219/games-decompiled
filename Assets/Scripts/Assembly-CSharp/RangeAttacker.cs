using UnityEngine;

[RequireComponent(typeof(RangeTargeter))]
public class RangeAttacker : MonoBehaviour
{
	public float attackRate = 1f;

	public float damage = 10f;

	public DamageType damageType;

	public GameObject projectilePrefab;

	public float projectileSpawnOffset = 0.5f;

	[AssertNotNull]
	public Animator animator;

	[AssertNotNull]
	public RangeTargeter targeter;

	private float timeLastAttack;

	private void Update()
	{
		if ((bool)targeter.target && timeLastAttack + attackRate < Time.time)
		{
			if (projectilePrefab == null)
			{
				targeter.target.GetComponent<LiveMixin>().TakeDamage(damage, base.transform.position, damageType);
			}
			else
			{
				GameObject obj = Object.Instantiate(projectilePrefab);
				Vector3 vector = Vector3.Normalize(targeter.target.transform.position - targeter.eye.position);
				Projectile component = obj.GetComponent<Projectile>();
				component.transform.position = vector * projectileSpawnOffset + targeter.eye.position;
				component.Shoot(vector);
				component.damageType = damageType;
				component.damage = damage;
			}
			timeLastAttack = Time.time;
		}
		SafeAnimator.SetBool(animator, "attack", Time.time < timeLastAttack + 0.5f);
	}
}
