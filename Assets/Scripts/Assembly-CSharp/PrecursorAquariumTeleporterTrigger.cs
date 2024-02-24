using System;
using Story;
using UWE;
using UnityEngine;

public class PrecursorAquariumTeleporterTrigger : MonoBehaviour, IStoryGoalListener
{
	[AssertNotNull]
	public Collider trigger;

	[AssertNotNull]
	public string listenForIncubatorActiveGoal = "PrecursorPrisonAquariumIncubatorActive";

	[AssertNotNull]
	public string listenForTeleporterUncoverGoal = "PrecursorPrisonAquariumFinalTeleporterUncover";

	[AssertNotNull]
	public string listenForTeleporterActiveGoal = "PrecursorPrisonAquariumFinalTeleporterActive";

	[AssertNotNull]
	public Animator targetAnimator;

	[AssertNotNull]
	public string atTeleporterParameterName = "";

	[AssertNotNull]
	public string blowTeleporterParameterName = "";

	public float minRange = 25f;

	public float maxRange = 50f;

	public float swimRange = 100f;

	private void Start()
	{
		StoryGoalManager main = StoryGoalManager.main;
		if (!main)
		{
			return;
		}
		if (main.IsGoalComplete(listenForTeleporterActiveGoal))
		{
			OnTeleporterActive();
			return;
		}
		if (main.IsGoalComplete(listenForTeleporterUncoverGoal))
		{
			OnTeleporterUncover();
		}
		else if (main.IsGoalComplete(listenForIncubatorActiveGoal))
		{
			OnIncubatorActive();
		}
		main.AddListener(this);
	}

	private void OnDestroy()
	{
		StoryGoalManager main = StoryGoalManager.main;
		if ((bool)main)
		{
			main.RemoveListener(this);
		}
	}

	public void NotifyGoalComplete(string key)
	{
		if (string.Equals(key, listenForIncubatorActiveGoal, StringComparison.OrdinalIgnoreCase))
		{
			OnIncubatorActive();
		}
		else if (string.Equals(key, listenForTeleporterUncoverGoal, StringComparison.OrdinalIgnoreCase))
		{
			OnTeleporterUncover();
		}
		else if (string.Equals(key, listenForTeleporterActiveGoal, StringComparison.OrdinalIgnoreCase))
		{
			OnTeleporterActive();
		}
	}

	private void OnIncubatorActive()
	{
		InvokeRepeating("CheckRange", UnityEngine.Random.value, 1f);
	}

	private void OnTeleporterUncover()
	{
		trigger.enabled = true;
		CancelInvoke();
		Player main = Player.main;
		Vector3 pos = ((main != null) ? main.transform.position : Vector3.zero);
		SafeAnimator.SetBool(targetAnimator, atTeleporterParameterName, UWE.Utils.IsInsideCollider(trigger, pos));
		SafeAnimator.SetBool(targetAnimator, blowTeleporterParameterName, value: false);
	}

	private void OnTeleporterActive()
	{
		SafeAnimator.SetBool(targetAnimator, atTeleporterParameterName, value: false);
		SafeAnimator.SetBool(targetAnimator, blowTeleporterParameterName, value: false);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void CheckRange()
	{
		Player main = Player.main;
		if ((bool)main)
		{
			float num = Vector3.Distance(main.transform.position, base.transform.position);
			bool value = num < swimRange;
			bool value2 = num < maxRange && num > minRange;
			SafeAnimator.SetBool(targetAnimator, atTeleporterParameterName, value);
			SafeAnimator.SetBool(targetAnimator, blowTeleporterParameterName, value2);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
		Gizmos.DrawSphere(base.transform.position, minRange);
		Gizmos.DrawSphere(base.transform.position, maxRange);
		Gizmos.DrawSphere(base.transform.position, swimRange);
	}
}
