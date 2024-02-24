using System;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UWE;
using UnityEngine;

[AddComponentMenu("")]
public class FMOD_StudioEventEmitter : MonoBehaviour, IManagedUpdateBehaviour, IManagedBehaviour
{
	[Serializable]
	public class Parameter
	{
		public string name;

		public float value;
	}

	[Header("This component is obsolete. Use FMODUnity.StudioEventEmitter instead")]
	public FMODAsset asset;

	public string path = "";

	public bool startEventOnAwake;

	public float minInterval;

	private float _lastTimePlayed;

	private EventInstance evt;

	private bool hasStarted;

	private Rigidbody cachedRigidBody;

	private static int numSlices = 5;

	private static int spawnCount = 0;

	private int updateSlice;

	private static bool isShuttingDown = false;

	public float lastTimePlayed => _lastTimePlayed;

	public int managedUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "FMOD_StudioEventEmitter";
	}

	public bool PlayUI()
	{
		bool result = false;
		if (evt.hasHandle())
		{
			if (_lastTimePlayed == 0f || Time.time > _lastTimePlayed + minInterval)
			{
				ERRCHECK(evt.start());
				_lastTimePlayed = Time.time;
				result = true;
			}
		}
		else
		{
			UnityEngine.Debug.Log("Tried to play event without a valid instance: " + path + " (" + asset.path + ")");
		}
		BehaviourUpdateUtils.Register(this);
		return result;
	}

	public static float GetLength(string soundName)
	{
		float result = -1f;
		EventDescription eventDescription = RuntimeManager.GetEventDescription(soundName);
		if (eventDescription.isValid())
		{
			int length = 0;
			eventDescription.getLength(out length);
			result = (float)length / 1000f;
		}
		return result;
	}

	public float GetLength()
	{
		if (asset == null)
		{
			return 0f;
		}
		float result = 0f;
		EventDescription eventDescription = RuntimeManager.GetEventDescription(asset.path);
		if (eventDescription.isValid())
		{
			int length = 0;
			eventDescription.getLength(out length);
			result = (float)length / 1000f;
		}
		return result;
	}

	public void PlayOneShotNoWorld(Vector3 position = default(Vector3), float volume = 1f)
	{
		if (_lastTimePlayed == 0f || Time.time > _lastTimePlayed + minInterval)
		{
			if (position == default(Vector3))
			{
				position = base.gameObject.transform.position;
			}
			FMODUWE.PlayOneShot(path, position, volume);
			_lastTimePlayed = Time.time;
			BehaviourUpdateUtils.Register(this);
		}
	}

	public void SetParameterValue(string paramname, float value)
	{
		if (evt.hasHandle())
		{
			evt.setParameterByName(paramname, value);
		}
	}

	public void SetParameterValue(PARAMETER_ID paramIndex, float value)
	{
		if (evt.hasHandle())
		{
			evt.setParameterByID(paramIndex, value);
		}
	}

	public PARAMETER_ID GetParameterIndex(string paramName)
	{
		try
		{
			return FMODUWE.GetEventInstanceParameterIndex(evt, paramName);
		}
		finally
		{
		}
	}

	public void Stop(bool allowFadeout = true)
	{
		if (evt.hasHandle())
		{
			if (allowFadeout)
			{
				ERRCHECK(evt.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT));
			}
			else
			{
				ERRCHECK(evt.stop(FMOD.Studio.STOP_MODE.IMMEDIATE));
			}
		}
		else
		{
			UnityEngine.Debug.Log("Tried to stop event without a valid instance: " + path + " (" + asset.path + ")");
		}
		BehaviourUpdateUtils.Deregister(this);
	}

	public PLAYBACK_STATE getPlaybackState()
	{
		if (!evt.isValid())
		{
			return PLAYBACK_STATE.STOPPED;
		}
		PLAYBACK_STATE state = PLAYBACK_STATE.STOPPED;
		if (ERRCHECK(evt.getPlaybackState(out state)) == RESULT.OK)
		{
			return state;
		}
		return PLAYBACK_STATE.STOPPED;
	}

	private void Start()
	{
		if (asset != null)
		{
			if (path == "")
			{
				UnityEngine.Debug.LogError(base.gameObject.name + ".FMOD_StudioEventEmitter.Start() - path is null. FMOD asset is \"" + asset.path + "\"");
			}
			CacheEventInstance();
		}
		cachedRigidBody = GetComponent<Rigidbody>();
		if (startEventOnAwake)
		{
			StartEvent();
		}
		updateSlice = Utils.Modulus(spawnCount++, numSlices);
		BehaviourUpdateUtils.Register(this);
	}

	public bool GetIsPlaying()
	{
		return getPlaybackState() == PLAYBACK_STATE.PLAYING;
	}

	public bool GetIsStartingOrPlaying()
	{
		PLAYBACK_STATE playbackState = getPlaybackState();
		if (playbackState != 0 && playbackState != PLAYBACK_STATE.STARTING)
		{
			return playbackState == PLAYBACK_STATE.SUSTAINING;
		}
		return true;
	}

	public bool CacheEventInstance()
	{
		bool result = false;
		if (asset != null)
		{
			evt = FMODUWE.GetEvent(asset);
			result = evt.hasHandle();
		}
		else if (!string.IsNullOrEmpty(path))
		{
			evt = FMODUWE.GetEvent(path);
			result = evt.hasHandle();
		}
		return result;
	}

	private void OnApplicationQuit()
	{
		isShuttingDown = true;
	}

	private void OnDisable()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	private void OnDestroy()
	{
		BehaviourUpdateUtils.Deregister(this);
		if (!isShuttingDown && evt.isValid())
		{
			if (getPlaybackState() != PLAYBACK_STATE.STOPPED)
			{
				ERRCHECK(evt.stop(FMOD.Studio.STOP_MODE.IMMEDIATE));
			}
			ERRCHECK(evt.release());
			evt.clearHandle();
		}
	}

	public void StartEvent()
	{
		if (!evt.isValid())
		{
			CacheEventInstance();
		}
		if (evt.isValid())
		{
			Update3DAttributes();
			ERRCHECK(evt.start());
			_lastTimePlayed = Time.time;
		}
		else
		{
			UnityEngine.Debug.LogError("Event retrieval failed: \"" + asset.path + "\" on gameObject " + base.gameObject.name);
		}
		hasStarted = true;
		BehaviourUpdateUtils.Register(this);
	}

	public bool HasFinished()
	{
		if (!hasStarted)
		{
			return false;
		}
		if (!evt.isValid())
		{
			return true;
		}
		return getPlaybackState() == PLAYBACK_STATE.STOPPED;
	}

	public void ManagedUpdate()
	{
		if (Time.frameCount % numSlices != updateSlice)
		{
			return;
		}
		if (evt.isValid())
		{
			if (GetIsPlaying())
			{
				Update3DAttributes();
			}
		}
		else
		{
			evt.clearHandle();
		}
		if (!GetIsPlaying())
		{
			BehaviourUpdateUtils.Deregister(this);
		}
	}

	private void Update3DAttributes()
	{
		if (evt.isValid())
		{
			ATTRIBUTES_3D attributes = RuntimeUtils.To3DAttributes(base.gameObject, cachedRigidBody);
			ERRCHECK(evt.set3DAttributes(attributes));
		}
	}

	private RESULT ERRCHECK(RESULT result)
	{
		if (result != 0)
		{
			UnityEngine.Debug.LogErrorFormat("FMOD Studio: Encounterd Error: {0} {1}", result, Error.String(result));
		}
		return result;
	}
}
