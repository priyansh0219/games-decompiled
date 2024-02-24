using UnityEngine;

public class DamageModifier : MonoBehaviour
{
	public float multiplier;

	public DamageType damageType = DamageType.Undefined;

	public virtual float ModifyDamage(float damage, DamageType type)
	{
		if (type == damageType)
		{
			damage *= multiplier;
		}
		return damage;
	}
}
