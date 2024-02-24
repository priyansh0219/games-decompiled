using UnityEngine;

public class AggressiveToPilotingVehicle : MonoBehaviour
{
	[AssertNotNull]
	public LastTarget lastTarget;

	[AssertNotNull]
	public Creature creature;

	public float range = 20f;

	public float aggressionPerSecond = 0.5f;

	private float updateAggressionInterval = 1f;

	private void Start()
	{
		InvokeRepeating("UpdateAggression", Random.value * updateAggressionInterval, updateAggressionInterval);
	}

	private void UpdateAggression()
	{
		Player main = Player.main;
		if (!(main == null) && main.GetMode() == Player.Mode.LockedPiloting)
		{
			Vehicle vehicle = main.GetVehicle();
			if (!(vehicle == null) && !(Vector3.Distance(vehicle.transform.position, base.transform.position) > range))
			{
				lastTarget.SetTarget(vehicle.gameObject);
				creature.Aggression.Add(aggressionPerSecond * updateAggressionInterval);
			}
		}
	}
}
