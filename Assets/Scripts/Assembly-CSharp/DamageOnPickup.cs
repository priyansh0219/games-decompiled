using System.Collections;
using UnityEngine;

public class DamageOnPickup : MonoBehaviour
{
	public float damageChance;

	public float damageAmount;

	public bool damageOnPickup = true;

	public bool damageOnKill = true;

	public DamageType damageType;

	private void OnEnable()
	{
		base.gameObject.GetComponent<Pickupable>().pickedUpEvent.AddHandler(base.gameObject, OnPickedUp);
	}

	private void OnPickedUp(Pickupable pickupable)
	{
		if (damageOnPickup && Random.value < damageChance)
		{
			Player.main.gameObject.GetComponent<LiveMixin>().TakeDamage(damageAmount, pickupable.gameObject.transform.position);
		}
	}

	private IEnumerator OnKill()
	{
		if (damageOnKill)
		{
			yield return null;
			DamageSystem.RadiusDamage(damageAmount, base.transform.position, 5f, damageType);
		}
	}
}
