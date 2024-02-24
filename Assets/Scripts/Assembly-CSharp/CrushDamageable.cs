using UWE;
using UnityEngine;

public class CrushDamageable : MonoBehaviour
{
	private float total;

	public Event<float> damagedEvent = new Event<float>();

	public float OnDamaged(float amount)
	{
		total += amount;
		damagedEvent.Trigger(amount);
		return amount;
	}

	public float GetTotalDamage()
	{
		return total;
	}
}
