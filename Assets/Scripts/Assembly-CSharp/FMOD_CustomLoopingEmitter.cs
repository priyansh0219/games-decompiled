using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class FMOD_CustomLoopingEmitter : FMOD_CustomEmitter
{
	public FMODAsset assetStart;

	public FMODAsset assetStop;

	public float stopSoundInterval;

	private float timeLastStopSound;

	private EventInstance evtStop;

	protected override void OnPlay()
	{
		ReleaseStopEvent();
		if (assetStart != null)
		{
			EventInstance @event = FMODUWE.GetEvent(assetStart);
			@event.set3DAttributes(base.transform.To3DAttributes());
			@event.start();
			@event.release();
			timeLastStopSound = Time.time;
		}
		base.OnPlay();
	}

	protected override void OnStop()
	{
		if (stopSoundInterval == -1f || timeLastStopSound + stopSoundInterval < Time.time)
		{
			PlayStopSound();
		}
		base.OnStop();
	}

	private void PlayStopSound()
	{
		if (assetStop != null)
		{
			ReleaseStopEvent();
			evtStop = FMODUWE.GetEvent(assetStop);
			evtStop.set3DAttributes(base.transform.To3DAttributes());
			evtStop.start();
			timeLastStopSound = Time.time;
		}
	}

	private void ReleaseStopEvent()
	{
		if (evtStop.hasHandle())
		{
			evtStop.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			evtStop.release();
			evtStop.clearHandle();
		}
	}

	protected override void OnDestroy()
	{
		ReleaseStopEvent();
		base.OnDestroy();
	}
}
