using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class VFXParticlesFlashesSlowdown : MonoBehaviour
{
	private class ParticleSystemData
	{
		public ParticleSystem particleSystem;

		public float originalSimulationSpeed;

		public bool originalIsPlaying;
	}

	[SerializeField]
	private float simulationSpeedMultiplier = 0.01f;

	[SerializeField]
	private bool turnOffCompletely;

	[SerializeField]
	private bool includeChildren;

	[SerializeField]
	private string[] includeChildrenIfNameContains = Array.Empty<string>();

	[SerializeField]
	private string[] excludeChildrenIfNameContains = Array.Empty<string>();

	private List<ParticleSystemData> particleSystems = new List<ParticleSystemData>();

	private void Awake()
	{
		ParticleSystem[] array = ((!includeChildren) ? GetComponents<ParticleSystem>() : GetComponentsInChildren<ParticleSystem>(includeInactive: true));
		foreach (ParticleSystem particleSystem in array)
		{
			string text = particleSystem.gameObject.name;
			if ((includeChildrenIfNameContains.Length == 0 || IsNameFromList(includeChildrenIfNameContains, text)) && (excludeChildrenIfNameContains.Length == 0 || !IsNameFromList(excludeChildrenIfNameContains, text)))
			{
				ParticleSystem.MainModule main = particleSystem.main;
				ParticleSystemData item = new ParticleSystemData
				{
					particleSystem = particleSystem,
					originalSimulationSpeed = main.simulationSpeed,
					originalIsPlaying = particleSystem.isPlaying
				};
				particleSystems.Add(item);
			}
		}
		MiscSettings.isFlashesEnabled.changedEvent.AddHandler(this, OnFlashesEnabled);
		UpdateSpeed(!MiscSettings.flashes);
	}

	private void OnDestroy()
	{
		MiscSettings.isFlashesEnabled.changedEvent.RemoveHandler(this, OnFlashesEnabled);
	}

	private void UpdateSpeed(bool slowdown)
	{
		for (int i = 0; i < particleSystems.Count; i++)
		{
			ParticleSystemData particleSystemData = particleSystems[i];
			UpdateSpeed(particleSystemData, slowdown);
		}
	}

	private void UpdateSpeed(ParticleSystemData particleSystemData, bool slowdown)
	{
		if (slowdown)
		{
			float num;
			if (turnOffCompletely)
			{
				num = 0f;
				if (particleSystemData.particleSystem.isPlaying)
				{
					particleSystemData.particleSystem.Stop(withChildren: false);
				}
			}
			else
			{
				num = simulationSpeedMultiplier;
			}
			ParticleSystem.MainModule main = particleSystemData.particleSystem.main;
			main.simulationSpeed = particleSystemData.originalSimulationSpeed * num;
		}
		else
		{
			if (turnOffCompletely && particleSystemData.originalIsPlaying && !particleSystemData.particleSystem.isPlaying)
			{
				particleSystemData.particleSystem.Play(withChildren: false);
			}
			ParticleSystem.MainModule main2 = particleSystemData.particleSystem.main;
			main2.simulationSpeed = particleSystemData.originalSimulationSpeed;
		}
	}

	private void OnFlashesEnabled(Utils.MonitoredValue<bool> isFlashesEnabled)
	{
		UpdateSpeed(!isFlashesEnabled.value);
	}

	private bool IsNameFromList(string[] list, string name)
	{
		foreach (string value in list)
		{
			if (name.Contains(value))
			{
				return true;
			}
		}
		return false;
	}
}
