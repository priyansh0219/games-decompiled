using System;
using System.Collections.Generic;
using UnityEngine;

public class ScheduledUpdateBehaviourSet : BehaviourUpdateManager.IBehaviourSet
{
	private readonly List<IScheduledUpdateBehaviour> behaviours = new List<IScheduledUpdateBehaviour>();

	private readonly List<IScheduledUpdateBehaviour> toAdd = new List<IScheduledUpdateBehaviour>();

	private readonly List<IScheduledUpdateBehaviour> toRemove = new List<IScheduledUpdateBehaviour>();

	private int lastEndPosition;

	private readonly List<IScheduledUpdateBehaviour> backfill = new List<IScheduledUpdateBehaviour>();

	private bool iterating;

	public int Count => behaviours.Count;

	public bool Add(IScheduledUpdateBehaviour behaviour)
	{
		if (behaviour.scheduledUpdateIndex > 0)
		{
			return false;
		}
		if (!iterating)
		{
			behaviour.scheduledUpdateIndex = behaviours.Count + 1;
			behaviours.Add(behaviour);
		}
		else
		{
			toAdd.Add(behaviour);
		}
		return true;
	}

	public bool Remove(IScheduledUpdateBehaviour behaviour)
	{
		int scheduledUpdateIndex = behaviour.scheduledUpdateIndex;
		if (scheduledUpdateIndex <= 0)
		{
			return false;
		}
		if (!iterating)
		{
			IScheduledUpdateBehaviour scheduledUpdateBehaviour = behaviours[behaviours.Count - 1];
			int scheduledUpdateIndex2 = scheduledUpdateBehaviour.scheduledUpdateIndex;
			behaviours[scheduledUpdateIndex - 1] = scheduledUpdateBehaviour;
			scheduledUpdateBehaviour.scheduledUpdateIndex = scheduledUpdateIndex;
			behaviours.RemoveAt(behaviours.Count - 1);
			behaviour.scheduledUpdateIndex = 0;
			if (scheduledUpdateIndex2 >= lastEndPosition && scheduledUpdateBehaviour.scheduledUpdateIndex < lastEndPosition)
			{
				backfill.Add(scheduledUpdateBehaviour);
			}
		}
		else
		{
			toRemove.Add(behaviour);
		}
		return true;
	}

	public void Print()
	{
		foreach (IScheduledUpdateBehaviour behaviour in behaviours)
		{
			Debug.Log(behaviour.GetProfileTag(), behaviour as UnityEngine.Object);
		}
	}

	public void CallUpdate(int startIndex, int numItems)
	{
		StartIterating();
		try
		{
			startIndex = Mathf.Max(0, startIndex);
			int num = (lastEndPosition = Mathf.Min(startIndex + numItems, behaviours.Count));
			for (int i = startIndex; i < num; i++)
			{
				IScheduledUpdateBehaviour scheduledUpdateBehaviour = behaviours[i];
				StartProfiling(scheduledUpdateBehaviour);
				try
				{
					scheduledUpdateBehaviour.ScheduledUpdate();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception, scheduledUpdateBehaviour as UnityEngine.Object);
				}
				StopProfiling();
			}
			ResolveBackfill();
		}
		finally
		{
			StopIterating();
		}
	}

	private void ResolveBackfill()
	{
		for (int i = 0; i < backfill.Count; i++)
		{
			IScheduledUpdateBehaviour scheduledUpdateBehaviour = backfill[i];
			if (scheduledUpdateBehaviour != null && scheduledUpdateBehaviour.scheduledUpdateIndex > 0)
			{
				StartProfiling(scheduledUpdateBehaviour);
				try
				{
					scheduledUpdateBehaviour.ScheduledUpdate();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception, scheduledUpdateBehaviour as UnityEngine.Object);
				}
				StopProfiling();
			}
		}
		backfill.Clear();
	}

	private void StartIterating()
	{
		iterating = true;
	}

	private void StopIterating()
	{
		iterating = false;
		foreach (IScheduledUpdateBehaviour item in toAdd)
		{
			Add(item);
		}
		foreach (IScheduledUpdateBehaviour item2 in toRemove)
		{
			Remove(item2);
		}
		toAdd.Clear();
		toRemove.Clear();
	}

	private static void StartProfiling(IScheduledUpdateBehaviour beh)
	{
		beh.GetProfileTag();
	}

	private static void StopProfiling()
	{
	}
}
