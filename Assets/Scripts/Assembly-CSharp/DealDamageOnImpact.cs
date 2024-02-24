using System.Collections.Generic;
using UWE;
using UnityEngine;

public class DealDamageOnImpact : MonoBehaviour
{
	public bool damageBases = true;

	public float speedMinimumForSelfDamage = 1.6f;

	public float speedMinimumForDamage = 0.8f;

	public bool affectsEcosystem;

	public float minimumMassForDamage = 0.5f;

	public bool allowDamageToPlayer = true;

	public bool mirroredSelfDamage = true;

	public float mirroredSelfDamageFraction = 0.5f;

	public float capMirrorDamage = -1f;

	public float minDamageInterval;

	private float timeLastDamage;

	private float timeLastDamagedSelf;

	private Vector3 prevPosition;

	private Vector3 prevVelocity;

	private HashSet<GameObject> exceptions = new HashSet<GameObject>();

	public void AddException(GameObject target)
	{
		exceptions.Add(target);
	}

	public void RemoveException(GameObject target)
	{
		exceptions.Remove(target);
	}

	private void Start()
	{
		prevPosition = base.transform.position;
	}

	private void FixedUpdate()
	{
		prevVelocity = GetComponent<Rigidbody>().velocity;
	}

	private LiveMixin GetLiveMixin(GameObject go)
	{
		LiveMixin liveMixin = go.GetComponent<LiveMixin>();
		if (!liveMixin)
		{
			liveMixin = Utils.FindAncestorWithComponent<LiveMixin>(go);
		}
		return liveMixin;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!base.enabled || collision.contacts.Length == 0 || exceptions.Contains(collision.gameObject))
		{
			return;
		}
		float num = Mathf.Max(0f, Vector3.Dot(-collision.contacts[0].normal, prevVelocity));
		if (!(num > speedMinimumForDamage))
		{
			return;
		}
		if (!allowDamageToPlayer)
		{
			GameObject gameObject = collision.gameObject;
			GameObject entityRoot = UWE.Utils.GetEntityRoot(collision.gameObject);
			if ((bool)entityRoot)
			{
				gameObject = entityRoot;
			}
			if (gameObject.Equals(Player.main.gameObject))
			{
				return;
			}
		}
		if (!damageBases && (bool)UWE.Utils.GetComponentInHierarchy<Base>(collision.gameObject))
		{
			return;
		}
		LiveMixin liveMixin = GetLiveMixin(collision.contacts[0].otherCollider.gameObject);
		Vector3 point = collision.contacts[0].point;
		float num2 = Mathf.Clamp(1f + GetComponent<Rigidbody>().mass * 0.001f, 0f, 10f) * 3f;
		float num3 = num * num2;
		if (liveMixin != null && Time.time > timeLastDamage + minDamageInterval)
		{
			liveMixin.TakeDamage(num3, point, DamageType.Collide, base.gameObject);
			timeLastDamage = Time.time;
		}
		Rigidbody rigidbody = Utils.FindAncestorWithComponent<Rigidbody>(collision.gameObject);
		if (!mirroredSelfDamage || !(num >= speedMinimumForSelfDamage))
		{
			return;
		}
		LiveMixin liveMixin2 = GetLiveMixin(base.gameObject);
		if ((bool)liveMixin2 && Time.time > timeLastDamagedSelf + 1f && (rigidbody == null || rigidbody.mass > minimumMassForDamage))
		{
			float num4 = num3 * mirroredSelfDamageFraction;
			if (capMirrorDamage != -1f)
			{
				num4 = Mathf.Min(capMirrorDamage, num4);
			}
			liveMixin2.TakeDamage(num4, point, DamageType.Collide, base.gameObject);
			timeLastDamagedSelf = Time.time;
		}
	}
}
