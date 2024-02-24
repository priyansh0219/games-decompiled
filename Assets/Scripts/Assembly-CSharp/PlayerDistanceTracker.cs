using UnityEngine;

public class PlayerDistanceTracker : MonoBehaviour, IScheduledUpdateBehaviour, IManagedBehaviour
{
	public float maxDistance = 25f;

	private float timeLastUpdate;

	private bool _playerNearby;

	private float _distanceToPlayer = float.PositiveInfinity;

	private float timeBetweenUpdates = 0.4f;

	public float distanceToPlayer => _distanceToPlayer;

	public bool playerNearby => _playerNearby;

	public int scheduledUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "PlayerDistanceTracker";
	}

	private void Start()
	{
		timeLastUpdate = Time.time - Random.value * timeBetweenUpdates;
	}

	public void OnEnable()
	{
		timeLastUpdate = Time.time - Random.value * timeBetweenUpdates;
		UpdateSchedulerUtils.Register(this);
	}

	public void OnDisable()
	{
		UpdateSchedulerUtils.Deregister(this);
	}

	public void OnDestroy()
	{
		UpdateSchedulerUtils.Deregister(this);
	}

	public void ScheduledUpdate()
	{
		if (timeLastUpdate + timeBetweenUpdates < Time.time)
		{
			float magnitude = (base.transform.position - Player.main.transform.position).magnitude;
			_playerNearby = magnitude < maxDistance;
			_distanceToPlayer = (_playerNearby ? magnitude : float.PositiveInfinity);
			timeLastUpdate += timeBetweenUpdates;
		}
	}
}
