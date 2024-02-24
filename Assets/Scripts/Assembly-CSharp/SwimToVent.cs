using UnityEngine;

public class SwimToVent : CreatureAction
{
	private PrecursorVentEntryTrigger target;

	[AssertNotNull]
	public Peeper peeper;

	public float swimVelocity = 4f;

	public float swimInterval = 1f;

	public float searchRange = 100f;

	public float searchInterval = 10f;

	private float timeNextSwim;

	private float timeNextSearch;

	private bool active;

	public override float Evaluate(Creature creature, float time)
	{
		if (time > timeNextSearch)
		{
			timeNextSearch = time + searchInterval;
			UpdateTarget();
		}
		if (!target)
		{
			return 0f;
		}
		return GetEvaluatePriority();
	}

	private bool IsTargetValid(PrecursorVentEntryTrigger candidate)
	{
		if (Physics.Linecast(base.transform.position, candidate.transform.position, Voxeland.GetTerrainLayerMask(), QueryTriggerInteraction.Ignore))
		{
			return false;
		}
		return true;
	}

	private void UpdateTarget()
	{
		if (active && (bool)target)
		{
			target.ReleaseExclusiveAccess(peeper);
		}
		PrecursorVentEntryTrigger nearestVentEntry = PrecursorVentEntryTrigger.GetNearestVentEntry(searchRange, peeper);
		target = (((bool)nearestVentEntry && IsTargetValid(nearestVentEntry)) ? nearestVentEntry : null);
		if (active && (bool)target)
		{
			target.AcquireExclusiveAccess(peeper);
		}
	}

	public override void StartPerform(Creature creature, float time)
	{
		active = true;
		if ((bool)target)
		{
			target.AcquireExclusiveAccess(peeper);
		}
	}

	public override void StopPerform(Creature creature, float time)
	{
		active = false;
		if ((bool)target)
		{
			target.ReleaseExclusiveAccess(peeper);
		}
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if ((bool)target && time > timeNextSwim)
		{
			timeNextSwim = time + swimInterval;
			base.swimBehaviour.SwimTo(target.transform.position, swimVelocity);
		}
	}

	private void OnDisable()
	{
		if (active && (bool)target)
		{
			target.ReleaseExclusiveAccess(peeper);
		}
	}

	public void OnReachBlockedVentEntry(PrecursorVentEntryTrigger entry)
	{
		if (active && entry == target)
		{
			target = null;
			Vector3 targetPosition = entry.transform.position + entry.transform.up * 15f;
			base.swimBehaviour.SwimTo(targetPosition, swimVelocity);
		}
	}
}
