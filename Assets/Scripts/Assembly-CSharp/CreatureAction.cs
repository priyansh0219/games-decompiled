using System;
using UnityEngine;

public abstract class CreatureAction : MonoBehaviour
{
	protected Creature creature;

	public float evaluatePriority = 0.4f;

	[SerializeField]
	private AnimationCurve priorityMultiplier;

	[SerializeField]
	private float minActionCheckInterval = -1f;

	[NonSerialized]
	public float timeLastChecked;

	private bool initialized;

	protected SwimBehaviour swimBehaviour { get; private set; }

	public virtual void Awake()
	{
		swimBehaviour = base.gameObject.GetComponent<SwimBehaviour>();
	}

	public virtual void OnEnable()
	{
		if (initialized)
		{
			return;
		}
		if (creature == null)
		{
			creature = base.gameObject.GetComponent<Creature>();
			if (!creature)
			{
				creature = base.gameObject.GetComponentInParent<Creature>();
			}
		}
		initialized = true;
	}

	public float GetEvaluatePriority()
	{
		return DayNightUtils.Evaluate(evaluatePriority, priorityMultiplier);
	}

	public virtual float Evaluate(Creature creature, float time)
	{
		return GetEvaluatePriority();
	}

	public virtual void StartPerform(Creature creature, float time)
	{
	}

	public virtual void StopPerform(Creature creature, float time)
	{
	}

	public virtual void Perform(Creature creature, float time, float deltaTime)
	{
	}

	public virtual string GetDebugString()
	{
		return GetType().ToString();
	}

	public float GetMaxEvaluatePriority()
	{
		return evaluatePriority;
	}

	public virtual bool NeedsToBeChecked(float time)
	{
		if (minActionCheckInterval <= 0f)
		{
			return false;
		}
		return time > timeLastChecked + minActionCheckInterval;
	}
}
