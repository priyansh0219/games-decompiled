using System;
using UWE;
using UnityEngine;

public class SubFloodAlarm : MonoBehaviour
{
	private SubRoot sub;

	public GameObject insideRoot;

	public GameObject outsideRoot;

	public GameObject toggleWithAlarm;

	public Color redAlarm;

	public Color blueAlarm;

	private SubRoot oldSub;

	private bool updateLightbar;

	private void Awake()
	{
		sub = base.gameObject.FindAncestor<SubRoot>();
	}

	private void Update()
	{
		if (Player.main.currentSub != oldSub)
		{
			NewAlarmState();
			oldSub = Player.main.currentSub;
		}
	}

	public void NewAlarmState()
	{
		bool flag = false;
		if (sub.fireSuppressionState)
		{
			SetAlarmLightsActive(active: true);
			SetAlarmLightPulseState(state: true);
			SetAlarmLightColor(blueAlarm);
		}
		else if (sub.subWarning)
		{
			flag = true;
			SetAlarmLightsActive(active: true);
			SetAlarmLightPulseState(state: true);
			SetAlarmLightColor(redAlarm);
		}
		else if (sub.silentRunning)
		{
			SetAlarmLightsActive(active: true);
			SetAlarmLightPulseState(state: false);
			SetAlarmLightColor(redAlarm);
		}
		else
		{
			SetAlarmLightsActive(active: false);
		}
		if (flag)
		{
			if (Player.main.currentSub == sub)
			{
				Utils.StartAllFMODEvents(insideRoot);
				Utils.StopAllFMODEvents(outsideRoot);
			}
			else
			{
				Utils.StopAllFMODEvents(insideRoot);
				Utils.StartAllFMODEvents(outsideRoot);
			}
		}
		else
		{
			Utils.StopAllFMODEvents(insideRoot);
			Utils.StopAllFMODEvents(outsideRoot);
		}
	}

	private void SetAlarmLightColor(Color color)
	{
		foreach (Transform item in toggleWithAlarm.transform)
		{
			item.GetComponent<Light>().color = color;
		}
	}

	private void SetAlarmLightPulseState(bool state)
	{
		foreach (Transform item in toggleWithAlarm.transform)
		{
			LightAnimator component = item.GetComponent<LightAnimator>();
			component.enabled = state;
			component.DefaultIntensity();
		}
	}

	private void SetAlarmLightsActive(bool active)
	{
		foreach (Transform item in toggleWithAlarm.transform)
		{
			item.gameObject.SetActive(active);
		}
	}

	private void UpdateLightbar()
	{
		foreach (Transform item in toggleWithAlarm.transform)
		{
			Light component = item.GetComponent<Light>();
			if (component != null)
			{
				float num = UWE.Utils.Unlerp(Mathf.Sin((float)System.Math.PI * 2f * Time.time), -1f, 1f) * 2f - 1f;
				num = 1f + num * 0.66f;
				PlatformUtils.SetLightbarColor(component.color * num);
				break;
			}
		}
	}
}
