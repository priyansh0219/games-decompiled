using UnityEngine;

public class CyclopsNoiseManager : MonoBehaviour
{
	[AssertNotNull]
	public SubRoot subRoot;

	[AssertNotNull]
	public CyclopsMotorMode cyclopsMotorMode;

	[AssertNotNull]
	public CyclopsLightingPanel lightingPanel;

	[AssertNotNull]
	public SubControl subControl;

	public float silentRunningDuration = 120f;

	public float noiseScalar { get; private set; }

	private void Start()
	{
		RecalculateNoiseValues();
	}

	public float GetNoisePercent()
	{
		if (subControl.appliedThrottle)
		{
			return noiseScalar;
		}
		return noiseScalar / 2f;
	}

	public void RigForSilentRunning()
	{
		subRoot.silentRunning = true;
		subRoot.voiceNotificationManager.PlayVoiceNotification(subRoot.silentRunningNotification);
		subRoot.BroadcastMessage("NewAlarmState", null, SendMessageOptions.DontRequireReceiver);
		RecalculateNoiseValues();
	}

	public void SecureFromSilentRunning()
	{
		subRoot.silentRunning = false;
		RecalculateNoiseValues();
		subRoot.BroadcastMessage("NewAlarmState", null, SendMessageOptions.DontRequireReceiver);
	}

	public float RecalculateNoiseValues()
	{
		noiseScalar = 0f;
		if (!cyclopsMotorMode.engineOn)
		{
			return 0f;
		}
		noiseScalar += cyclopsMotorMode.GetNoiseValue();
		if (subRoot.silentRunning)
		{
			noiseScalar *= 0.5f;
		}
		noiseScalar = Mathf.Clamp01(noiseScalar / 100f);
		return noiseScalar;
	}
}
