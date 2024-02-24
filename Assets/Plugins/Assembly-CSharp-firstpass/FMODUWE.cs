using System;
using System.Collections.Generic;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public static class FMODUWE
{
	public static readonly PARAMETER_ID invalidParameterId = new PARAMETER_ID
	{
		data1 = uint.MaxValue,
		data2 = uint.MaxValue
	};

	private static Dictionary<string, Guid> pathToGuidCache = new Dictionary<string, Guid>();

	private static readonly UWE_DSP_METERING_INFO outputMetering = new UWE_DSP_METERING_INFO();

	public static void PlayOneShot(FMODAsset asset, Vector3 position, float volume = 1f)
	{
		if (asset != null)
		{
			PlayOneShotImpl(asset.path, position, volume);
		}
		else
		{
			UnityEngine.Debug.LogError("Missing FMOD asset");
		}
	}

	[Obsolete("Use PlayOneShot(FMODAsset, ..) instead!")]
	public static void PlayOneShot(string eventPath, Vector3 position, float volume = 1f)
	{
		PlayOneShotImpl(eventPath, position, volume);
	}

	public static EventInstance GetEvent(FMODAsset asset)
	{
		return GetEventImpl(asset.path);
	}

	[Obsolete("Use GetEvent(FMODAsset) instead!")]
	public static EventInstance GetEvent(string eventPath)
	{
		return GetEventImpl(eventPath);
	}

	private static EventInstance GetEventImpl(string eventPath)
	{
		GetEventInstance(eventPath, out var instance);
		return instance;
	}

	private static void PlayOneShotImpl(string eventPath, Vector3 position, float volume)
	{
		EventInstance instance;
		RESULT eventInstance = GetEventInstance(eventPath, out instance);
		if (eventInstance != 0)
		{
			UnityEngine.Debug.LogErrorFormat("No FMOD event found for '{0}' ({1})", eventPath, Error.String(eventInstance));
		}
		else
		{
			instance.setVolume(volume);
			instance.set3DAttributes(position.To3DAttributes());
			instance.start();
			instance.release();
		}
	}

	private static RESULT GetEventInstance(string eventPath, out EventInstance instance)
	{
		instance = default(EventInstance);
		EventDescription eventDesc;
		try
		{
			RESULT eventDescription = GetEventDescription(eventPath, out eventDesc);
			if (eventDescription != 0)
			{
				return eventDescription;
			}
		}
		finally
		{
		}
		try
		{
			RESULT rESULT = eventDesc.createInstance(out instance);
			if (rESULT != 0)
			{
				return rESULT;
			}
		}
		finally
		{
		}
		return RESULT.OK;
	}

	private static RESULT GetEventDescription(string eventPath, out EventDescription eventDesc)
	{
		eventDesc = default(EventDescription);
		Guid guid;
		try
		{
			RESULT rESULT = PathToGUID(eventPath, out guid);
			if (rESULT != 0)
			{
				return rESULT;
			}
		}
		finally
		{
		}
		try
		{
			RESULT eventDescription = GetEventDescription(guid, out eventDesc);
			if (eventDescription != 0)
			{
				return eventDescription;
			}
		}
		finally
		{
		}
		return RESULT.OK;
	}

	private static RESULT PathToGUID(string path, out Guid guid)
	{
		if (pathToGuidCache.TryGetValue(path, out guid))
		{
			return RESULT.OK;
		}
		if (path.StartsWith("{"))
		{
			RESULT rESULT = Util.parseID(path, out guid);
			if (rESULT != 0)
			{
				return rESULT;
			}
		}
		else
		{
			RESULT rESULT2 = RuntimeManager.StudioSystem.lookupID(path, out guid);
			if (rESULT2 != 0)
			{
				return rESULT2;
			}
		}
		pathToGuidCache[path] = guid;
		return RESULT.OK;
	}

	private static RESULT GetEventDescription(Guid guid, out EventDescription eventDesc)
	{
		Dictionary<Guid, EventDescription> cachedDescriptions = RuntimeManager.CachedDescriptions;
		if (cachedDescriptions.TryGetValue(guid, out eventDesc))
		{
			if (eventDesc.isValid())
			{
				return RESULT.OK;
			}
			cachedDescriptions.Remove(guid);
		}
		RESULT eventByID = RuntimeManager.StudioSystem.getEventByID(guid, out eventDesc);
		if (eventByID != 0)
		{
			return eventByID;
		}
		cachedDescriptions[guid] = eventDesc;
		return RESULT.OK;
	}

	public static PARAMETER_ID GetParameterIndex(this StudioEventEmitter emitter, string paramName)
	{
		return GetEventInstanceParameterIndex(emitter.EventInstance, paramName);
	}

	public static PARAMETER_ID GetEventInstanceParameterIndex(EventInstance evt, string paramName)
	{
		if (evt.isValid())
		{
			if (evt.getDescription(out var description) != 0)
			{
				return invalidParameterId;
			}
			if (description.getParameterDescriptionByName(paramName, out var parameter) != 0)
			{
				return invalidParameterId;
			}
			return parameter.id;
		}
		return invalidParameterId;
	}

	public static bool IsValidParameterId(PARAMETER_ID id)
	{
		return !IsInvalidParameterId(id);
	}

	public static bool IsInvalidParameterId(PARAMETER_ID id)
	{
		if (id.data1 == invalidParameterId.data1)
		{
			return id.data2 == invalidParameterId.data2;
		}
		return false;
	}

	public static float GetMeteringVolume()
	{
		try
		{
			RuntimeManager.CoreSystem.getMasterChannelGroup(out var channelgroup).CheckResult();
			channelgroup.getDSP(-1, out var dsp).CheckResult();
			dsp.getMeteringEnabled(out var inputEnabled, out var outputEnabled).CheckResult();
			if (!outputEnabled)
			{
				dsp.setMeteringEnabled(inputEnabled, outputEnabled: true).CheckResult();
			}
			dsp.getMeteringInfo(null, outputMetering).CheckResult();
			float num = 0f;
			for (int i = 0; i < outputMetering.numchannels; i++)
			{
				num += outputMetering.rmslevel[i];
			}
			return num;
		}
		finally
		{
		}
	}
}
