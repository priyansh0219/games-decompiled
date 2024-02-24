using UnityEngine;

public class VFXLateTimeParticles : MonoBehaviour
{
	private ParticleSystem ps;

	private ParticleSystem[] psChildren;

	private void Awake()
	{
		ps = GetComponent<ParticleSystem>();
		psChildren = GetComponentsInChildren<ParticleSystem>();
	}

	public void Play()
	{
		ps.Simulate(0f, withChildren: true, restart: true);
		ParticleSystem[] array = psChildren;
		for (int i = 0; i < array.Length; i++)
		{
			ParticleSystem.EmissionModule emission = array[i].emission;
			emission.enabled = true;
		}
	}

	public void Stop()
	{
		ParticleSystem[] array = psChildren;
		for (int i = 0; i < array.Length; i++)
		{
			ParticleSystem.EmissionModule emission = array[i].emission;
			emission.enabled = false;
		}
	}

	private void LateUpdate()
	{
		if (ps.IsAlive(withChildren: true))
		{
			ps.Simulate(Time.deltaTime, withChildren: true, restart: false, fixedTimeStep: false);
		}
	}
}
