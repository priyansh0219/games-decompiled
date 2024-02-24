using System;
using UnityEngine;

public class CreatureFollowPlayer : MonoBehaviour, IScheduledUpdateBehaviour, IManagedBehaviour
{
	public float updateInterval = 1f;

	public float distanceToPlayer = 4f;

	public float maxYPos = -5f;

	[AssertNotNull]
	public Creature creature;

	private float timeNextUpdate;

	public int scheduledUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "CreatureFollowPlayer";
	}

	public void ScheduledUpdate()
	{
		if (Time.time > timeNextUpdate)
		{
			timeNextUpdate = Time.time + updateInterval;
			string biomeString = Player.main.GetBiomeString();
			if (!biomeString.StartsWith("precursor", StringComparison.OrdinalIgnoreCase) && !biomeString.StartsWith("prison", StringComparison.OrdinalIgnoreCase))
			{
				Transform transform = MainCamera.camera.transform;
				Vector3 vector = transform.position + transform.forward * distanceToPlayer;
				vector.y = Mathf.Min(vector.y, maxYPos);
				creature.leashPosition = vector;
				Debug.DrawLine(base.transform.position, vector, Color.magenta, updateInterval);
			}
		}
	}

	private void OnEnable()
	{
		UpdateSchedulerUtils.Register(this);
	}

	private void OnDisable()
	{
		UpdateSchedulerUtils.Deregister(this);
	}

	private void OnDestroy()
	{
		UpdateSchedulerUtils.Deregister(this);
	}
}
