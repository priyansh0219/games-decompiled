using System;
using UnityEngine;

public class ParticlesInBiome : MonoBehaviour
{
	public string onlyInBiome = "";

	public ParticleSystem particles;

	private void Start()
	{
		InvokeRepeating("CheckBiome", 0f, 1f);
	}

	private void CheckBiome()
	{
		Player main = Player.main;
		if (!(main != null))
		{
			return;
		}
		bool flag = main.GetBiomeString().StartsWith(onlyInBiome, StringComparison.OrdinalIgnoreCase);
		if (flag != particles.isPlaying)
		{
			if (flag)
			{
				particles.Play();
			}
			else
			{
				particles.Stop();
			}
		}
	}
}
