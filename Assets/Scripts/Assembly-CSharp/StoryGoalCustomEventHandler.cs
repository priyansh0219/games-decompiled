using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using Story;
using UnityEngine;

[ProtoContract]
public class StoryGoalCustomEventHandler : MonoBehaviour, ICompileTimeCheckable
{
	[Serializable]
	public sealed class SunbeamGoal : StoryGoal
	{
		public string trigger;
	}

	[NonSerialized]
	[ProtoMember(1)]
	public bool countdownActive;

	[NonSerialized]
	[ProtoMember(2)]
	public float countdownStartingTime;

	[NonSerialized]
	[ProtoMember(3)]
	public bool gunDisabled;

	public Vector3 landingSiteLocation;

	public float landingSiteRadius;

	public StoryGoal sunbeamDestroyEventInRange;

	public StoryGoal sunbeamDestroyEventOutOfRange;

	public StoryGoal gunDeactivate;

	public StoryGoal sunbeamCancel;

	public SunbeamGoal[] sunbeamGoals;

	private bool isShootingSequenceScheduled;

	public static StoryGoalCustomEventHandler main { get; private set; }

	public bool IsSunbeamAnimationActive
	{
		get
		{
			if (!isShootingSequenceScheduled)
			{
				if (VFXSunbeam.main != null)
				{
					return VFXSunbeam.main.isPlaying;
				}
				return false;
			}
			return true;
		}
	}

	public float endTime => countdownStartingTime + 2400f;

	private void Awake()
	{
		main = this;
		if (StoryGoalManager.main.IsGoalComplete(gunDeactivate.key))
		{
			gunDisabled = true;
		}
	}

	private void Start()
	{
		DevConsole.RegisterConsoleCommand(this, "startsunbeamstoryevent");
		DevConsole.RegisterConsoleCommand(this, "precursorgunaim");
		DevConsole.RegisterConsoleCommand(this, "sunbeamcountdownstart");
		DevConsole.RegisterConsoleCommand(this, "infectionreveal");
		DevConsole.RegisterConsoleCommand(this, "playerinfection");
	}

	public void DisableGun()
	{
		gunDisabled = true;
		gunDeactivate.Trigger();
	}

	public void NotifyGoalComplete(string key)
	{
		SunbeamGoal[] array = sunbeamGoals;
		foreach (SunbeamGoal sunbeamGoal in array)
		{
			if (string.Equals(key, sunbeamGoal.trigger, StringComparison.OrdinalIgnoreCase))
			{
				if (!gunDisabled)
				{
					sunbeamGoal.Trigger();
					continue;
				}
				sunbeamCancel.delay = sunbeamGoal.delay;
				sunbeamCancel.Trigger();
			}
		}
		if (string.Equals(key, "Goal_Disable_Gun", StringComparison.OrdinalIgnoreCase))
		{
			bool flag = false;
			List<string> pendingRadioMessages = StoryGoalManager.main.pendingRadioMessages;
			for (int num = pendingRadioMessages.Count - 1; num >= 0; num--)
			{
				if (string.Equals(pendingRadioMessages[num], "RadioSunbeam4", StringComparison.OrdinalIgnoreCase))
				{
					pendingRadioMessages.RemoveAt(num);
					flag = true;
				}
			}
			if (flag)
			{
				sunbeamCancel.Trigger();
			}
		}
		if (string.Equals(key, "OnPlayRadioSunbeam4", StringComparison.OrdinalIgnoreCase))
		{
			countdownActive = true;
			countdownStartingTime = DayNightCycle.main.timePassedAsFloat;
		}
		else if (string.Equals(key, "PrecursorGunAim", StringComparison.OrdinalIgnoreCase))
		{
			isShootingSequenceScheduled = true;
			PrecursorGunStoryEvents precursorGunStoryEvents = PrecursorGunStoryEvents.main;
			if ((bool)precursorGunStoryEvents)
			{
				precursorGunStoryEvents.GunTakeAim();
			}
		}
		else if (string.Equals(key, "SunbeamCheckPlayerRange", StringComparison.OrdinalIgnoreCase))
		{
			countdownActive = false;
			Invoke("StartSunbeamShootdownFX", 7f);
			bool flag2 = IsPlayerAtLandingSite();
			if (VFXSunbeam.main != null)
			{
				VFXSunbeam.main.PlaySFX(flag2);
			}
			else
			{
				Debug.LogError("VFXSunbeam.main can not be found");
			}
			(flag2 ? sunbeamDestroyEventInRange : sunbeamDestroyEventOutOfRange).Trigger();
		}
		else if (string.Equals(key, "SelfScan4", StringComparison.OrdinalIgnoreCase))
		{
			Invoke("TriggerSickAnim", 14f);
		}
		else if (string.Equals(key, "Infection_Progress1", StringComparison.OrdinalIgnoreCase))
		{
			Player.main.infectedMixin.IncreaseInfectedAmount(0.25f);
		}
		else if (string.Equals(key, "Infection_Progress2", StringComparison.OrdinalIgnoreCase))
		{
			Player.main.infectedMixin.IncreaseInfectedAmount(0.5f);
		}
		else if (string.Equals(key, "Infection_Progress3", StringComparison.OrdinalIgnoreCase))
		{
			Player.main.infectedMixin.IncreaseInfectedAmount(0.75f);
		}
		else if (string.Equals(key, "Infection_Progress4", StringComparison.OrdinalIgnoreCase))
		{
			Player.main.infectedMixin.IncreaseInfectedAmount(1f);
		}
		else if (string.Equals(key, "Infection_Progress5", StringComparison.OrdinalIgnoreCase))
		{
			Player.main.infectedMixin.RemoveInfection();
		}
		else if (string.Equals(key, "Emperor_Telepathic_Contact1", StringComparison.OrdinalIgnoreCase))
		{
			StartCoroutine(PlayTelepathy(12f, showModel: true));
		}
		else if (string.Equals(key, "Emperor_Telepathic_Contact2", StringComparison.OrdinalIgnoreCase))
		{
			StartCoroutine(PlayTelepathy(4f, showModel: true));
		}
		else if (string.Equals(key, "Emperor_Telepathic_Contact3", StringComparison.OrdinalIgnoreCase))
		{
			StartCoroutine(PlayTelepathy(13f, showModel: true));
		}
		else if (string.Equals(key, "Precursor_Prison_Aquarium_EmperorLog1", StringComparison.OrdinalIgnoreCase))
		{
			StartCoroutine(PlayTelepathy(31f, showModel: false));
		}
		else if (string.Equals(key, "Precursor_Prison_Aquarium_EmperorLog2", StringComparison.OrdinalIgnoreCase))
		{
			StartCoroutine(PlayTelepathy(31f, showModel: false));
		}
		else if (string.Equals(key, "Precursor_Prison_Aquarium_EmperorLog3", StringComparison.OrdinalIgnoreCase))
		{
			StartCoroutine(PlayTelepathy(22f, showModel: false));
		}
		else if (string.Equals(key, "Precursor_Prison_Aquarium_EmperorLog4", StringComparison.OrdinalIgnoreCase))
		{
			StartCoroutine(PlayTelepathy(26f, showModel: false));
		}
	}

	private IEnumerator PlayTelepathy(float duration, bool showModel)
	{
		Camera camera = MainCamera.camera;
		if (!camera)
		{
			Debug.LogWarningFormat(this, "Could not find main camera");
			yield break;
		}
		TelepathyScreenFXController screenFX = camera.GetComponent<TelepathyScreenFXController>();
		if (!screenFX)
		{
			Debug.LogWarningFormat(this, "Could not find telepathy fx on camera");
			yield break;
		}
		screenFX.StartTelepathy(showModel);
		yield return new WaitForSeconds(duration);
		screenFX.StopTelepathy();
	}

	public bool IsPlayerAtLandingSite()
	{
		Player player = Player.main;
		if (!player)
		{
			return false;
		}
		return Vector3.Distance(player.transform.position, landingSiteLocation) <= landingSiteRadius;
	}

	private void StartSunbeamShootdownFX()
	{
		isShootingSequenceScheduled = false;
		if (VFXSunbeam.main != null)
		{
			VFXSunbeam.main.PlaySequence();
		}
		else
		{
			Debug.LogError("VFXSunbeam.main can not be found");
		}
	}

	private void TriggerSickAnim()
	{
		Player.main.infectedMixin.infectedAmount = 1f;
		StartCoroutine(Player.main.TriggerInfectionRevealAsync());
	}

	private void OnConsoleCommand_startsunbeamstoryevent()
	{
		new StoryGoal("RadioSunbeamStart", Story.GoalType.Radio, 0f).Trigger();
	}

	private void OnConsoleCommand_precursorgunaim()
	{
		StoryGoalManager.main.OnGoalComplete("PrecursorGunAim");
	}

	private void OnConsoleCommand_sunbeamcountdownstart()
	{
		StoryGoalManager.main.OnGoalComplete("OnPlayRadioSunbeam4");
	}

	private void OnConsoleCommand_infectionreveal()
	{
		Player.main.infectionRevealed = false;
		TriggerSickAnim();
	}

	private void OnConsoleCommand_playerinfection(NotificationCenter.Notification n)
	{
		string text;
		switch (int.Parse((string)n.data[0]))
		{
		case 1:
			text = "Infection_Progress1";
			break;
		case 2:
			text = "Infection_Progress2";
			break;
		case 3:
			text = "Infection_Progress3";
			break;
		case 4:
			text = "Infection_Progress4";
			break;
		default:
			text = "Infection_Progress5";
			break;
		}
		Debug.Log("triggering story goal: " + text);
		new StoryGoal(text, Story.GoalType.Story, 0f).Trigger();
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(landingSiteLocation, landingSiteRadius);
	}

	public string CompileTimeCheck()
	{
		return StoryGoalUtils.CheckStoryGoals(new StoryGoal[2] { sunbeamDestroyEventInRange, sunbeamDestroyEventOutOfRange });
	}
}
