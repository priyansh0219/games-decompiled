using UnityEngine;

public class TemperatureDamage : MonoBehaviour
{
	[AssertNotNull]
	public LavaDatabase lavaDatabase;

	[AssertNotNull]
	public LiveMixin liveMixin;

	public float minDamageTemperature = 49f;

	public float baseDamagePerSecond = 0.2f;

	public bool onlyLavaDamage;

	private const float lavaDamageTimeOut = 0.2f;

	private const float lavaDamageInterval = 0.1f;

	private const float lavaDamagePerSecond = 25f;

	private float timeDamageStarted = -1000f;

	private float timeLastDamage;

	private Player player;

	private void Start()
	{
		if (!onlyLavaDamage)
		{
			InvokeRepeating("UpdateDamage", 1f, 1f);
		}
		player = GetComponent<Player>();
	}

	private float GetTemperature()
	{
		WaterTemperatureSimulation main = WaterTemperatureSimulation.main;
		if (!(main != null))
		{
			return 0f;
		}
		return main.GetTemperature(base.transform.position);
	}

	private void UpdateDamage()
	{
		bool flag = true;
		if ((bool)player && (player.IsInsideWalkable() || !player.IsSwimming()))
		{
			flag = false;
		}
		float temperature = GetTemperature();
		if (flag && temperature >= minDamageTemperature)
		{
			float num = temperature / minDamageTemperature;
			num *= baseDamagePerSecond;
			liveMixin.TakeDamage(num, base.transform.position, DamageType.Heat);
		}
	}

	private void StartLavaDamage()
	{
		if (timeDamageStarted + 0.2f < Time.time)
		{
			InvokeRepeating("ApplyLavaDamage", 0f, 0.1f);
		}
		timeDamageStarted = Time.time;
	}

	private void ApplyLavaDamage()
	{
		if (timeDamageStarted + 0.2f <= Time.time)
		{
			CancelInvoke("ApplyLavaDamage");
		}
		else
		{
			liveMixin.TakeDamage(2.5f, base.transform.position, DamageType.Heat);
		}
	}

	private void OnCollisionStay(Collision colinfo)
	{
		try
		{
			for (int i = 0; i < colinfo.contacts.Length; i++)
			{
				ContactPoint contactPoint = colinfo.contacts[i];
				if (lavaDatabase.IsLava(contactPoint.point, contactPoint.normal))
				{
					StartLavaDamage();
					break;
				}
			}
		}
		finally
		{
		}
	}
}
