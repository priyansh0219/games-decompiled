using UnityEngine;

public interface IOxygenSource : ISecondaryTooltip
{
	GameObject gameObject { get; }

	bool IsPlayer();

	bool IsBreathable();

	float GetOxygenCapacity();

	float GetOxygenAvailable();

	float AddOxygen(float amount);

	float RemoveOxygen(float amount);
}
