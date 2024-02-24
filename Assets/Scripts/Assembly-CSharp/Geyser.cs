using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class Geyser : MonoBehaviour
{
	public GameObject warningSmokeParticles;

	public GameObject eruptionParticles;

	private ParticleSystem warningSmokeEmitter;

	private ParticleSystem eruptionEmitter;

	public float eruptionInterval;

	public float eruptionIntervalVariance;

	public float eruptionLength;

	private bool erupting;

	public float damage = 20f;

	private static bool consoleCmdRegged;

	private void Start()
	{
		GameObject gameObject = Utils.SpawnZeroedAt(warningSmokeParticles, base.transform);
		GameObject gameObject2 = Utils.SpawnZeroedAt(eruptionParticles, base.transform);
		warningSmokeEmitter = gameObject.GetComponent<ParticleSystem>();
		eruptionEmitter = gameObject2.GetComponent<ParticleSystem>();
		warningSmokeEmitter.Play();
		eruptionEmitter.Stop();
		float num = eruptionInterval + Random.value * eruptionIntervalVariance;
		InvokeRepeating("Erupt", Random.value * num, num);
		if (!consoleCmdRegged)
		{
			DevConsole.RegisterConsoleCommand(this, "erupt");
			consoleCmdRegged = true;
		}
	}

	private void OnConsoleCommand_erupt()
	{
		Geyser[] array = Object.FindObjectsOfType<Geyser>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Erupt();
		}
	}

	private void Erupt()
	{
		if (!erupting && base.gameObject.activeInHierarchy)
		{
			erupting = true;
			eruptionEmitter.Play();
			warningSmokeEmitter.Stop();
			Utils.PlayEnvSound("event:/env/geyser_erupt", base.transform.position);
			Invoke("EndErupt", eruptionLength);
		}
	}

	private void EndErupt()
	{
		erupting = false;
		eruptionEmitter.Stop();
		warningSmokeEmitter.Play();
	}

	private void OnTriggerStay(Collider other)
	{
		if (erupting)
		{
			LiveMixin component = other.gameObject.GetComponent<LiveMixin>();
			if (component != null)
			{
				component.TakeDamage(damage * Time.deltaTime, base.transform.position, DamageType.Fire);
			}
		}
	}
}
