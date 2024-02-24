using UWE;
using UnityEngine;

[RequireComponent(typeof(SwimBehaviour))]
public class MoveOnGround : CreatureAction, IManagedUpdateBehaviour, IManagedBehaviour
{
	public float swimVelocity = 5f;

	public float swimRadius = 10f;

	public float swimForward = 0.5f;

	public float swimInterval = 5f;

	public float targetYOffset;

	public CreatureAction leaveGroundAction;

	private float timeNextSwim;

	[AssertNotNull]
	public OnSurfaceTracker onGroundTracker;

	[AssertNotNull]
	public ConstantForce descendForce;

	public float descendForceValue = 10f;

	public int managedUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "MoveOnGround";
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (onGroundTracker.onSurface)
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	public override void Perform(Creature b, float time, float deltaTime)
	{
		if (time > timeNextSwim)
		{
			Vector3 insideUnitSphere = Random.insideUnitSphere;
			insideUnitSphere += base.transform.forward * swimForward;
			insideUnitSphere = Vector3.Scale(insideUnitSphere, new Vector3(swimRadius, 0f, swimRadius));
			Vector3 origin = base.transform.position + insideUnitSphere + Vector3.up;
			if (UWE.Utils.TraceForTerrain(new Ray(origin, Vector3.down), 30f, out var hitInfo))
			{
				base.swimBehaviour.SwimTo(hitInfo.point + targetYOffset * base.transform.localScale.y * Vector3.up, swimVelocity);
				timeNextSwim = time + swimInterval;
			}
			else
			{
				timeNextSwim = time + 0.1f;
			}
		}
	}

	public override void StartPerform(Creature creature, float time)
	{
		descendForce.enabled = true;
		descendForce.force = new Vector3(0f, 0f - descendForceValue, 0f);
		if (leaveGroundAction != null)
		{
			BehaviourUpdateUtils.Register(this);
		}
	}

	public override void StopPerform(Creature creature, float time)
	{
		descendForce.enabled = false;
		BehaviourUpdateUtils.Deregister(this);
	}

	public void ManagedUpdate()
	{
		if (!onGroundTracker.onSurface)
		{
			creature.TryStartAction(leaveGroundAction);
		}
	}

	private void OnDestroy()
	{
		BehaviourUpdateUtils.Deregister(this);
	}
}
