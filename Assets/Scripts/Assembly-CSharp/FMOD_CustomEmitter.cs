using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class FMOD_CustomEmitter : MonoBehaviour, IManagedUpdateBehaviour, IManagedBehaviour
{
	[AssertNotNull]
	public FMODAsset asset;

	public bool playOnAwake;

	public bool stopImmediatelyOnDisable;

	public bool followParent;

	public bool restartOnPlay;

	public bool debug;

	[SerializeField]
	private string subtitlesKey;

	[SerializeField]
	private bool playSubtitlesWithUnscaledTime = true;

	private EventInstance evt;

	private ATTRIBUTES_3D attributes;

	private bool _playing;

	private float length;

	private bool addedToUpdateManager;

	public bool playing => _playing;

	public int managedUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "FMOD_CustomEmitter";
	}

	protected virtual void OnPlay()
	{
		BehaviourUpdateUtils.Register(this);
	}

	protected virtual void OnStop()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	protected virtual void OnUpdate()
	{
	}

	protected virtual void OnSetEvent(EventInstance eventInstance)
	{
	}

	public void Play()
	{
		CacheEventInstance();
		if (debug && !evt.hasHandle())
		{
			UnityEngine.Debug.Log("FMOD: tried to start sound without any event assigned");
		}
		if ((!_playing || restartOnPlay) && evt.hasHandle())
		{
			bool flag = false;
			if (evt.getPlaybackState(out var state) == RESULT.OK)
			{
				_playing = state != PLAYBACK_STATE.STOPPED;
				flag = state == PLAYBACK_STATE.STOPPING;
			}
			if (_playing)
			{
				if (!flag)
				{
					Stop();
				}
				ReleaseEvent();
				CacheEventInstance();
			}
			UpdateEventAttributes();
			evt.start();
			if (!string.IsNullOrEmpty(subtitlesKey))
			{
				Subtitles.Add(subtitlesKey);
				evt.getDescription(out var description);
				description.getLength(out var _);
			}
			if (debug)
			{
				UnityEngine.Debug.Log("FMOD: starting sound " + asset.path);
			}
			_playing = true;
			OnPlay();
		}
		if (debug && !evt.hasHandle())
		{
			UnityEngine.Debug.Log("FMOD: tried to play but evt is null");
		}
	}

	public void Stop(FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
	{
		if (_playing && evt.hasHandle())
		{
			evt.stop(stopMode).CheckResult();
			if (debug)
			{
				UnityEngine.Debug.Log("FMOD: stopping sound " + asset.path);
			}
			_playing = false;
			OnStop();
		}
	}

	private bool ReleaseEvent()
	{
		if (evt.hasHandle())
		{
			Stop();
			evt.release();
			evt.clearHandle();
			return true;
		}
		return false;
	}

	protected virtual void OnDestroy()
	{
		ReleaseEvent();
		BehaviourUpdateUtils.Deregister(this);
	}

	private void Awake()
	{
		SetAsset(asset);
	}

	private void CacheEventInstance()
	{
		if (!evt.hasHandle())
		{
			SetAsset(asset);
		}
	}

	public void SetAsset(FMODAsset newAsset)
	{
		ReleaseEvent();
		asset = newAsset;
		if (newAsset != null)
		{
			evt = FMODUWE.GetEvent(asset);
			OnSetEvent(evt);
			if (!evt.hasHandle())
			{
				UnityEngine.Debug.LogError("FMOD: " + base.gameObject.name + ".FMOD_CustomEmitter: could not load fmod event: " + asset.path + " id: " + asset.id);
			}
			attributes = default(ATTRIBUTES_3D);
			attributes.velocity = Vector3.zero.ToFMODVector();
		}
	}

	protected virtual void Start()
	{
		if (playOnAwake)
		{
			Play();
		}
	}

	private void UpdateEventAttributes()
	{
		attributes = base.transform.To3DAttributes();
		evt.set3DAttributes(attributes);
	}

	public void SetParameterValue(string paramname, float value)
	{
		CacheEventInstance();
		if (evt.hasHandle())
		{
			evt.setParameterValue(paramname, value);
		}
	}

	public void SetParameterValue(PARAMETER_ID paramIndex, float value)
	{
		CacheEventInstance();
		if (evt.hasHandle())
		{
			evt.setParameterValueByIndex(paramIndex, value);
		}
	}

	public PARAMETER_ID GetParameterIndex(string paramName)
	{
		CacheEventInstance();
		return FMODUWE.GetEventInstanceParameterIndex(evt, paramName);
	}

	public void ManagedUpdate()
	{
		if (followParent && evt.hasHandle() && _playing)
		{
			UpdateEventAttributes();
		}
		OnUpdate();
	}

	private void OnEnable()
	{
		if (debug)
		{
			UnityEngine.Debug.Log("FMOD: enable event " + asset.path);
		}
		if (playOnAwake)
		{
			Play();
		}
	}

	private void OnDisable()
	{
		if (debug)
		{
			UnityEngine.Debug.Log("FMOD: disable event " + asset.path);
		}
		if (stopImmediatelyOnDisable)
		{
			Stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
		else
		{
			Stop();
		}
	}

	public EventInstance GetEventInstance()
	{
		return evt;
	}
}
