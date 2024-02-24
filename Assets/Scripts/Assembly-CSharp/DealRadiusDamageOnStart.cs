using UnityEngine;

public class DealRadiusDamageOnStart : MonoBehaviour
{
	public float damage = 10f;

	public float radius = 6f;

	public DamageType type = DamageType.Heat;

	private void Start()
	{
		DamageSystem.RadiusDamage(damage, base.transform.position, radius, type, base.gameObject);
	}
}
