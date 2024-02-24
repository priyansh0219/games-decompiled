using System.Collections.Generic;
using System.Text;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class SoundQueue
{
	private struct Entry
	{
		public string sound;

		public string subtitles;

		public Entry(string sound, string subtitles)
		{
			this.sound = sound;
			this.subtitles = subtitles;
		}
	}

	private string _current;

	private int _position;

	private int _length;

	private float _positionSeconds;

	private float _lengthSeconds;

	private EventInstance eventInstance;

	private List<Entry> queue = new List<Entry>();

	public string current => _current;

	public float positionSeconds => _positionSeconds;

	public float lengthSeconds => _lengthSeconds;

	public int position
	{
		get
		{
			return _position;
		}
		set
		{
			if (eventInstance.hasHandle())
			{
				eventInstance.setTimelinePosition(Mathf.Clamp(value, 0, _length));
			}
		}
	}

	public int length => _length;

	public void Update()
	{
		if (queue.Count > 0 && !GetIsStartingOrPlaying(eventInstance))
		{
			Entry entry = queue[0];
			queue.RemoveAt(0);
			Play(entry.sound, entry.subtitles);
		}
		if (!string.IsNullOrEmpty(_current) && !GetIsStartingOrPlaying(eventInstance))
		{
			_current = null;
		}
		if (eventInstance.isValid())
		{
			ATTRIBUTES_3D attributes = Player.main.transform.To3DAttributes();
			eventInstance.set3DAttributes(attributes).CheckResult();
			eventInstance.getTimelinePosition(out _position).CheckResult();
			_positionSeconds = (float)_position * 0.001f;
		}
	}

	public void PlayQueued(FMODAsset asset)
	{
		PlayQueued(asset.id);
	}

	public void PlayIfFree(FMODAsset asset)
	{
		PlayIfFree(asset.id);
	}

	public void PlayImmediately(FMODAsset asset)
	{
		PlayImmediately(asset.id);
	}

	public void PlayQueued(string sound, string subtitles = null)
	{
		if (string.IsNullOrEmpty(sound) || _current == sound)
		{
			return;
		}
		for (int i = 0; i < queue.Count; i++)
		{
			if (queue[i].sound == sound)
			{
				return;
			}
		}
		queue.Add(new Entry(sound, subtitles));
	}

	public void PlayIfFree(string sound, string subtitles = null)
	{
		if (!string.IsNullOrEmpty(sound) && queue.Count <= 0 && !GetIsStartingOrPlaying(eventInstance))
		{
			Play(sound, subtitles);
		}
	}

	public void PlayImmediately(string sound, string subtitles = null)
	{
		if (string.IsNullOrEmpty(sound))
		{
			return;
		}
		for (int num = queue.Count - 1; num >= 0; num--)
		{
			if (queue[num].sound == sound)
			{
				queue.RemoveAt(num);
			}
		}
		Play(sound, subtitles);
	}

	public void Stop()
	{
		if (GetIsStartingOrPlaying(eventInstance))
		{
			eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE).CheckResult();
		}
		if (eventInstance.hasHandle())
		{
			eventInstance.release().CheckResult();
			eventInstance.clearHandle();
			_current = null;
		}
	}

	private void Play(string sound, string subtitles)
	{
		Stop();
		EventInstance eventInstance = ((!string.IsNullOrEmpty(sound)) ? FMODUWE.GetEvent(sound) : default(EventInstance));
		if (eventInstance.isValid())
		{
			this.eventInstance = eventInstance;
			_current = sound;
			this.eventInstance.getDescription(out var description).CheckResult();
			description.getLength(out _length).CheckResult();
			_lengthSeconds = (float)_length * 0.001f;
			this.eventInstance.setVolume(1f).CheckResult();
			this.eventInstance.start().CheckResult();
		}
		if (!string.IsNullOrEmpty(subtitles))
		{
			Subtitles.Add(subtitles);
		}
	}

	public static bool GetIsStartingOrPlaying(EventInstance evt)
	{
		if (evt.hasHandle())
		{
			PLAYBACK_STATE playbackState = GetPlaybackState(evt);
			if (playbackState != 0 && playbackState != PLAYBACK_STATE.STARTING)
			{
				return playbackState == PLAYBACK_STATE.SUSTAINING;
			}
			return true;
		}
		return false;
	}

	private static PLAYBACK_STATE GetPlaybackState(EventInstance evt)
	{
		PLAYBACK_STATE state = PLAYBACK_STATE.STOPPED;
		if (evt.isValid())
		{
			evt.getPlaybackState(out state).CheckResult();
		}
		return state;
	}

	public void Debug()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("SoundQueue ({0} queued):\n", queue.Count);
		PLAYBACK_STATE state = PLAYBACK_STATE.STOPPED;
		if (eventInstance.hasHandle())
		{
			eventInstance.getPlaybackState(out state);
		}
		stringBuilder.AppendFormat("current: {0} {1}\n", (current != null) ? current : "null", state);
		for (int i = 0; i < queue.Count; i++)
		{
			Entry entry = queue[i];
			stringBuilder.AppendFormat("\n{0} - {1} {2}", i, entry.sound, entry.subtitles);
		}
		UnityEngine.Debug.Log(stringBuilder.ToString());
	}
}
