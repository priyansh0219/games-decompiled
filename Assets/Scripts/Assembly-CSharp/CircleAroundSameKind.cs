using UWE;
using UnityEngine;

[RequireComponent(typeof(SwimBehaviour))]
public class CircleAroundSameKind : CreatureAction
{
	public float circleRadius = 2f;

	public float swimVelocity = 3f;

	public float swimInterval = 2f;

	public float maxDistance = 20f;

	public float friendTimeOut = 30f;

	private GameObject friend;

	private float timeFriendFound = -100f;

	private float timeLastUpdate;

	private float timeNextSwim;

	private void Update()
	{
		if (!(friend == null) || !(timeLastUpdate + 2f < Time.time) || !(timeFriendFound + friendTimeOut * 2f <= Time.time))
		{
			return;
		}
		int num = UWE.Utils.OverlapSphereIntoSharedBuffer(base.transform.position, maxDistance);
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = UWE.Utils.sharedColliderBuffer[i].gameObject;
			if (gameObject != base.gameObject && gameObject.name == base.gameObject.name)
			{
				friend = gameObject;
				timeFriendFound = Time.time;
				break;
			}
		}
		timeLastUpdate = Time.time;
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (friend != null)
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	public override void StartPerform(Creature creature, float time)
	{
		creature.Happy.Add(1f);
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (timeFriendFound + friendTimeOut <= time)
		{
			friend = null;
		}
		if (time > timeNextSwim && friend != null)
		{
			Vector3 vector = Vector3.Normalize(friend.transform.position - base.transform.position);
			Vector3 vector2 = Vector3.Cross(Vector3.up, vector);
			base.swimBehaviour.SwimTo(friend.transform.position + vector2 * circleRadius, vector, swimVelocity);
		}
	}
}
