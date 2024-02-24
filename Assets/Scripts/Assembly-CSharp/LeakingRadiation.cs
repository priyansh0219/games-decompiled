using System;
using System.Collections.Generic;
using ProtoBuf;
using Story;
using UnityEngine;

[ProtoContract]
public class LeakingRadiation : MonoBehaviour, IProtoEventListener
{
	[AssertNotNull]
	public DamagePlayerInRadius damagePlayerInRadius;

	[AssertNotNull]
	public RadiatePlayerInRange radiatePlayerInRange;

	[AssertNotNull]
	public PlayerDistanceTracker playerDistanceTracker;

	[AssertNotNull]
	public PDANotification leaksRemainingNotification;

	[AssertNotNull]
	public PDANotification leaksFixedNotification;

	[AssertNotNull]
	public StoryGoal leaksFixedGoal;

	public float kGrowRate;

	public float kStartRadius;

	public float kMaxRadius;

	public float kNaturalDissipation;

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public float currentRadius;

	[NonSerialized]
	[ProtoMember(3)]
	public bool radiationFixed;

	private bool deserialized;

	private List<RadiationLeak> leaks = new List<RadiationLeak>();

	public static LeakingRadiation main;

	private void Awake()
	{
		main = this;
	}

	private void Start()
	{
		DevConsole.RegisterConsoleCommand(this, "fixleaks");
		DevConsole.RegisterConsoleCommand(this, "decontaminate");
		if (!deserialized)
		{
			currentRadius = kStartRadius;
			damagePlayerInRadius.damageRadius = currentRadius;
			radiatePlayerInRange.radiateRadius = currentRadius;
		}
	}

	public void RegisterLeak(RadiationLeak newLeak)
	{
		leaks.Add(newLeak);
	}

	private int GetNumLeaks()
	{
		int num = 0;
		for (int i = 0; i < leaks.Count; i++)
		{
			if (leaks[i].IsLeaking())
			{
				num++;
			}
		}
		return num;
	}

	private void Update()
	{
		if (CrashedShipExploder.main.IsExploded())
		{
			float num = ((GetNumLeaks() > 0) ? kGrowRate : kNaturalDissipation);
			currentRadius = Mathf.Clamp(currentRadius + Time.deltaTime * num, 0f, kMaxRadius);
			damagePlayerInRadius.damageRadius = currentRadius;
			radiatePlayerInRange.radiateRadius = currentRadius;
		}
		else
		{
			radiationFixed = false;
		}
	}

	public void NotifyLeaksFixed()
	{
		int numLeaks = GetNumLeaks();
		if (numLeaks == 0)
		{
			if (!radiationFixed)
			{
				leaksFixedNotification.Play();
				leaksFixedGoal.Trigger();
				radiationFixed = true;
			}
		}
		else
		{
			leaksRemainingNotification.Play(numLeaks);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = (((bool)CrashedShipExploder.main && CrashedShipExploder.main.IsExploded()) ? Color.red : Color.blue);
		Gizmos.DrawWireSphere(base.transform.position, currentRadius);
	}

	private void OnConsoleCommand_fixleaks()
	{
		for (int i = 0; i < leaks.Count; i++)
		{
			LiveMixin component = leaks[i].GetComponent<LiveMixin>();
			if ((bool)component)
			{
				component.AddHealth(1000f);
			}
		}
	}

	private void OnConsoleCommand_decontaminate()
	{
		currentRadius = 1f;
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		deserialized = true;
	}
}
