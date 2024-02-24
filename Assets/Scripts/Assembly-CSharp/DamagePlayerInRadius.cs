using UnityEngine;

[RequireComponent(typeof(PlayerDistanceTracker))]
public class DamagePlayerInRadius : MonoBehaviour
{
	public float updateInterval = 0.5f;

	public float damageRadius = 20f;

	public float damageAmount = 10f;

	public DamageType damageType;

	public bool doDebug;

	private PlayerDistanceTracker tracker;

	private void Start()
	{
		tracker = GetComponent<PlayerDistanceTracker>();
		InvokeRepeating("DoDamage", 0f, updateInterval);
	}

	private void DoDamage()
	{
		if (!base.enabled || !base.gameObject.activeInHierarchy || !(damageRadius > 0f))
		{
			return;
		}
		float distanceToPlayer = tracker.distanceToPlayer;
		if (distanceToPlayer <= damageRadius)
		{
			if (doDebug)
			{
				Debug.Log(base.gameObject.name + ".DamagePlayerInRadius() - dist/damageRadius: " + distanceToPlayer + "/" + damageRadius + " => damageAmount: " + damageAmount);
			}
			if (damageType != DamageType.Radiation || Player.main.radiationAmount != 0f)
			{
				if (doDebug)
				{
					Debug.Log("TakeDamage: " + damageAmount + " " + damageType.ToString());
				}
				Player.main.GetComponent<LiveMixin>().TakeDamage(damageAmount, base.transform.position, damageType);
			}
		}
		else if (doDebug)
		{
			Debug.Log(base.gameObject.name + ".DamagePlayerInRadius() - dist/damageRadius: " + distanceToPlayer + "/" + damageRadius + " => no damage");
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (doDebug)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(base.transform.position, damageRadius);
		}
	}
}
