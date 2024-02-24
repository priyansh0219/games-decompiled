using UnityEngine;

public class CreatureFlinch : MonoBehaviour, IOnTakeDamage
{
	[SerializeField]
	[AssertNotNull]
	private Animator animator;

	[SerializeField]
	private float interval = 1f;

	[SerializeField]
	private float damageThreshold = 10f;

	private float timeLastFlinch;

	public void OnTakeDamage(DamageInfo damageInfo)
	{
		float num = damageInfo.damage;
		if (damageInfo.type == DamageType.Electrical)
		{
			num *= 35f;
		}
		if (!(num < damageThreshold))
		{
			float time = Time.time;
			if (!(time < timeLastFlinch + interval))
			{
				timeLastFlinch = time;
				animator.SetFloat(AnimatorHashID.flinch_damage, num);
				animator.SetTrigger(AnimatorHashID.flinch);
			}
		}
	}
}
