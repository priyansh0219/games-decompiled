using System;
using ProtoBuf;
using Story;
using UnityEngine;

[ProtoContract]
public class PrecursorAquariumPlatformTrigger : MonoBehaviour
{
	[AssertNotNull]
	public Animator[] targetAnimators;

	[AssertNotNull]
	public string animationParameter;

	[AssertNotNull]
	public GameObject invisibleWalls;

	public float invisibleWallsDuration = 20f;

	public float maxStayDuration = 34f;

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public bool triggered;

	private void Start()
	{
		if (!triggered)
		{
			StoryGoalManager main = StoryGoalManager.main;
			if ((bool)main && main.IsGoalComplete("Precursor_Prison_Aquarium_EmperorLog1"))
			{
				Debug.Log("Restoring platform trigger state state after prison batch upgrade", this);
				triggered = true;
			}
		}
		UpdateWalls();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!triggered && !other.isTrigger && (bool)FindPlayer(other))
		{
			triggered = true;
			for (int i = 0; i < targetAnimators.Length; i++)
			{
				SafeAnimator.SetBool(targetAnimators[i], animationParameter, value: true);
			}
			Invoke("UpdateWalls", invisibleWallsDuration);
			Invoke("EndTrigger", maxStayDuration);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (triggered && !other.isTrigger && (bool)FindPlayer(other) && !invisibleWalls.activeSelf)
		{
			EndTrigger();
		}
	}

	private void UpdateWalls()
	{
		invisibleWalls.SetActive(!triggered);
	}

	private void EndTrigger()
	{
		for (int i = 0; i < targetAnimators.Length; i++)
		{
			SafeAnimator.SetBool(targetAnimators[i], animationParameter, value: false);
		}
	}

	private static Player FindPlayer(Collider other)
	{
		Player componentInParent = other.GetComponentInParent<Player>();
		if ((bool)componentInParent)
		{
			return componentInParent;
		}
		PrefabIdentifier componentInParent2 = other.GetComponentInParent<PrefabIdentifier>();
		if (!componentInParent2)
		{
			return null;
		}
		return componentInParent2.GetComponentInChildren<Player>(includeInactive: true);
	}
}
