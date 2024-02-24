using UnityEngine;

public static class ParticleSystemExtensions
{
	public static void EnableEmission(this ParticleSystem particleSystem, bool enabled)
	{
		ParticleSystem.EmissionModule emission = particleSystem.emission;
		emission.enabled = enabled;
	}

	public static float GetEmissionRate(this ParticleSystem particleSystem)
	{
		return particleSystem.emission.rateOverTime.constantMax;
	}

	public static void SetEmissionRate(this ParticleSystem particleSystem, float emissionRate)
	{
		ParticleSystem.EmissionModule emission = particleSystem.emission;
		ParticleSystem.MinMaxCurve rateOverTime = emission.rateOverTime;
		rateOverTime.constantMax = emissionRate;
		emission.rateOverTime = rateOverTime;
	}

	public static void SetPlaying(this ParticleSystem particleSystem, bool playing)
	{
		if (playing)
		{
			particleSystem.Play();
		}
		else
		{
			particleSystem.Stop();
		}
	}
}
