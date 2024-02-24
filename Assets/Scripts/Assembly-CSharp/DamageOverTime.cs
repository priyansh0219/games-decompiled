using UnityEngine;

public class DamageOverTime : MonoBehaviour
{
	public GameObject doer;

	public float totalDamage;

	public float damageRemaining;

	public float duration;

	public DamageType damageType;

	private float interval;

	private float startTime;

	private float dps;

	private void Start()
	{
	}

	public void ActivateInterval(float intervalToSet)
	{
		CancelInvoke("DoDamage");
		interval = intervalToSet;
		dps = totalDamage * (interval / duration);
		damageRemaining = totalDamage;
		startTime = Time.time;
		InvokeRepeating("DoDamage", 0f, interval);
	}

	public void DoDamage()
	{
		float num = Mathf.Min(damageRemaining, dps);
		damageRemaining = Mathf.Max(0f, damageRemaining - num);
		GetComponent<LiveMixin>().TakeDamage(num, base.transform.position, damageType);
		if (damageRemaining <= 0f || startTime + duration <= Time.time)
		{
			Object.Destroy(this);
		}
	}

	private void OnKill()
	{
		Object.Destroy(this);
	}
}
