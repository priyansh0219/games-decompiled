using UnityEngine;

[RequireComponent(typeof(Creature))]
public class StayNearFriend : MonoBehaviour, IScheduledUpdateBehaviour, IManagedBehaviour
{
	public float updateInterval = 1f;

	public float friendlinessThreshold = 0.5f;

	public float friendDistance = 4f;

	[SerializeField]
	[AssertNotNull]
	private Creature creature;

	private float timeNextUpdate;

	public int scheduledUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "StayNearFriend";
	}

	public void ScheduledUpdate()
	{
		if (!(Time.time > timeNextUpdate))
		{
			return;
		}
		timeNextUpdate = Time.time + updateInterval;
		GameObject friend = creature.GetFriend();
		if (friend != null && creature.Friendliness.Value > friendlinessThreshold)
		{
			Transform transform = friend.transform;
			if (friend == Player.main.gameObject)
			{
				transform = MainCamera.camera.transform;
			}
			Vector3 vector = transform.position + transform.forward * friendDistance;
			creature.leashPosition = vector;
			Debug.DrawLine(base.transform.position, vector, Color.magenta, updateInterval);
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
