using System.Collections.Generic;
using UWE;
using UnityEngine;

[RequireComponent(typeof(TriggerStayTracker))]
[RequireComponent(typeof(Collider))]
public class DamageSphere : MonoBehaviour
{
	public float maxDamagePerSecond = 100f;

	public DamageType damageType = DamageType.Cold;

	public float checkInterval = 3f;

	public string damageSound = "event:/player/cold_damage";

	private float minSoundInterval = 2f;

	private float lastSoundTime;

	private TriggerStayTracker tracker;

	private SphereCollider sphereCollider;

	private void Start()
	{
		tracker = base.gameObject.GetComponent<TriggerStayTracker>();
		sphereCollider = base.gameObject.GetComponent<SphereCollider>();
		InvokeRepeating("ApplyDamageEffects", checkInterval, checkInterval);
	}

	private void ApplyDamageEffects()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		HashSet<GameObject>.Enumerator enumerator = tracker.Get().GetEnumerator();
		while (enumerator.MoveNext())
		{
			GameObject current = enumerator.Current;
			if (!(current != null))
			{
				continue;
			}
			LiveMixin component = current.GetComponent<LiveMixin>();
			if (component != null)
			{
				float magnitude = (component.transform.position - base.transform.position).magnitude;
				float num = Mathf.Clamp01(1f - magnitude / sphereCollider.radius);
				float num2 = maxDamagePerSecond * checkInterval * num;
				component.TakeDamage(num2, base.transform.position, damageType);
				if (Time.time > lastSoundTime + minSoundInterval)
				{
					float volume = 0.3f + Mathf.Clamp01(num2 / 75f) * 0.7f;
					FMODUWE.PlayOneShot(damageSound, current.transform.position, volume);
					lastSoundTime = Time.time;
				}
			}
		}
	}
}
