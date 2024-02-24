using UnityEngine;

public class DescendToGround : CreatureAction, IManagedUpdateBehaviour, IManagedBehaviour
{
	private float lastExitTime;

	private float startTime;

	[AssertNotNull]
	public OnSurfaceTracker onGroundTracker;

	[AssertNotNull]
	public ConstantForce descendForce;

	[AssertNotNull]
	public bool checkGroundLoaded;

	public CreatureAction onGroundAction;

	public float forceValue = 10f;

	public float actionInterval = 5f;

	public float maxDuration = 5f;

	public int managedUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "DescendToGround";
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (time > lastExitTime + actionInterval && (startTime == 0f || startTime + maxDuration > time) && (!checkGroundLoaded || IsGroundLoaded()))
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	public override void StartPerform(Creature creature, float time)
	{
		startTime = time;
		descendForce.enabled = true;
		descendForce.force = new Vector3(0f, 0f - forceValue, 0f);
		if (onGroundAction != null)
		{
			BehaviourUpdateUtils.Register(this);
		}
	}

	public override void StopPerform(Creature creature, float time)
	{
		descendForce.enabled = false;
		lastExitTime = time;
		startTime = 0f;
		BehaviourUpdateUtils.Deregister(this);
	}

	private bool IsGroundLoaded()
	{
		if (LargeWorldStreamer.main != null)
		{
			return LargeWorldStreamer.main.IsRangeActiveAndBuilt(new Bounds(base.transform.position, Vector3.zero));
		}
		return false;
	}

	public void ManagedUpdate()
	{
		if (onGroundTracker.onSurface)
		{
			creature.TryStartAction(onGroundAction);
		}
	}

	private void OnDestroy()
	{
		BehaviourUpdateUtils.Deregister(this);
	}
}
