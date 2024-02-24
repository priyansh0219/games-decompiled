using UnityEngine;

public class DamageInfo
{
	public float originalDamage;

	public float damage;

	public Vector3 position;

	public DamageType type;

	public GameObject dealer;

	public void Clear()
	{
		originalDamage = 0f;
		damage = 0f;
		position = Vector3.zero;
		type = DamageType.Normal;
	}
}
