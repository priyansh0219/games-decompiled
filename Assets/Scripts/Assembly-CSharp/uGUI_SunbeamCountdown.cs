using System;
using TMPro;
using UnityEngine;

public class uGUI_SunbeamCountdown : MonoBehaviour
{
	[AssertNotNull]
	public GameObject sunbeamSignalPrefab;

	[AssertNotNull]
	public GameObject countdownHolder;

	[AssertNotNull]
	public TextMeshProUGUI countdownTitle;

	[AssertNotNull]
	public TextMeshProUGUI countdownText;

	[AssertNotNull]
	public string sunbeamArrivalCountdown;

	public Vector3 rendevouzLocation;

	private bool showing;

	private GameObject signal;

	public static uGUI_SunbeamCountdown main { get; private set; }

	private void Start()
	{
		main = this;
		InvokeRepeating("UpdateInterface", 0f, 1f);
	}

	private void ShowInterface()
	{
		if (!showing)
		{
			signal = UnityEngine.Object.Instantiate(sunbeamSignalPrefab, rendevouzLocation, Quaternion.identity);
			SignalPing component = signal.GetComponent<SignalPing>();
			if ((bool)component)
			{
				component.pos = rendevouzLocation;
				component.descriptionKey = "SunbeamRendezvousLocation";
			}
			countdownTitle.text = Language.main.Get(sunbeamArrivalCountdown);
			countdownHolder.SetActive(value: true);
			showing = true;
		}
	}

	private void HideInterface()
	{
		if (showing)
		{
			countdownHolder.SetActive(value: false);
			UnityEngine.Object.Destroy(signal);
			signal = null;
			showing = false;
		}
	}

	private void UpdateInterface()
	{
		StoryGoalCustomEventHandler storyGoalCustomEventHandler = StoryGoalCustomEventHandler.main;
		if ((bool)storyGoalCustomEventHandler)
		{
			if (!storyGoalCustomEventHandler.countdownActive)
			{
				HideInterface();
				return;
			}
			ShowInterface();
			TimeSpan timeSpan = TimeSpan.FromSeconds(storyGoalCustomEventHandler.endTime - DayNightCycle.main.timePassedAsFloat);
			string text = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
			countdownText.text = text;
		}
	}
}
