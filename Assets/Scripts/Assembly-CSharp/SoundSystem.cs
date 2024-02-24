using System;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

internal class SoundSystem : MonoBehaviour
{
	private static SoundSystem instance;

	public const float defaultMasterVolume = 1f;

	public const float defaultMusicVolume = 1f;

	public const float defaultVoiceVolume = 1f;

	public const float defaultAmbientVolume = 1f;

	private static float masterVolume = 1f;

	private static float musicVolume = 1f;

	private static float voiceVolume = 1f;

	private static float ambientVolume = 1f;

	private static VCA masterVCA;

	private static VCA musicVCA;

	private static VCA voiceVCA;

	private static VCA sfxVCA;

	private static void Initialize()
	{
		RuntimeUtils.EnforceLibraryOrder();
		int defaultDevice = GetDefaultDevice();
		if (defaultDevice != -1)
		{
			SetDevice(defaultDevice);
		}
	}

	public static void SetupDefaultSettings()
	{
		SetMasterVolume(1f);
		SetMusicVolume(1f);
		SetVoiceVolume(1f);
		SetAmbientVolume(1f);
	}

	private static void GetBuses()
	{
		FMOD.Studio.System studioSystem = RuntimeManager.StudioSystem;
		studioSystem.getVCAByID(new Guid("ab3de586-1658-48da-9f2b-ec696e5e68e7"), out masterVCA);
		studioSystem.getVCAByID(new Guid("0d0dd1e2-9d46-4a5b-aae8-ae412d827831"), out musicVCA);
		studioSystem.getVCAByID(new Guid("634d9305-7695-40df-999a-cc1d049e1ebe"), out sfxVCA);
		studioSystem.getVCAByID(new Guid("3c4e7775-5fbe-4b00-83e0-35bc9add106a"), out voiceVCA);
		if (masterVCA.hasHandle())
		{
			masterVCA.setVolume(masterVolume);
		}
		if (musicVCA.hasHandle())
		{
			musicVCA.setVolume(musicVolume);
		}
		if (voiceVCA.hasHandle())
		{
			voiceVCA.setVolume(voiceVolume);
		}
		if (sfxVCA.hasHandle())
		{
			sfxVCA.setVolume(ambientVolume);
		}
	}

	public static string[] GetDeviceOptions(out int currentIndex)
	{
		FMOD.System coreSystem = RuntimeManager.CoreSystem;
		coreSystem.getNumDrivers(out var numdrivers);
		string[] array = new string[numdrivers];
		for (int i = 0; i < numdrivers; i++)
		{
			coreSystem.getDriverInfo(i, out var text, 256, out var _, out var _, out var _, out var _);
			array[i] = text;
		}
		coreSystem.getDriver(out currentIndex);
		return array;
	}

	public static void SetDevice(int deviceIndex)
	{
		FMOD.System coreSystem = RuntimeManager.CoreSystem;
		if (deviceIndex == -1)
		{
			deviceIndex = GetDefaultDevice();
		}
		if (deviceIndex == -1)
		{
			deviceIndex = 0;
		}
		coreSystem.setDriver(deviceIndex);
	}

	private static int GetDeviceByGuid(Guid guid)
	{
		FMOD.System coreSystem = RuntimeManager.CoreSystem;
		coreSystem.getNumDrivers(out var numdrivers);
		for (int i = 0; i < numdrivers; i++)
		{
			coreSystem.getDriverInfo(i, out var _, 256, out var guid2, out var _, out var _, out var _);
			if (guid2 == guid)
			{
				return i;
			}
		}
		return -1;
	}

	private static Guid GetDeviceGuid(int deviceIndex)
	{
		RuntimeManager.CoreSystem.getDriverInfo(deviceIndex, out var _, 256, out var guid, out var _, out var _, out var _);
		return guid;
	}

	public static int GetDefaultDevice()
	{
		if (VRUtil.GetAudioDeviceGuid(out var guid))
		{
			return GetDeviceByGuid(guid);
		}
		return -1;
	}

	public static void SetMasterVolume(float value)
	{
		masterVolume = value;
		if (masterVCA.hasHandle())
		{
			masterVCA.setVolume(value);
		}
	}

	public static float GetMasterVolume()
	{
		return masterVolume;
	}

	public static void SetMusicVolume(float value)
	{
		musicVolume = value;
		if (musicVCA.hasHandle())
		{
			musicVCA.setVolume(value);
		}
	}

	public static float GetMusicVolume()
	{
		return musicVolume;
	}

	public static void SetVoiceVolume(float value)
	{
		voiceVolume = value;
		if (voiceVCA.hasHandle())
		{
			voiceVCA.setVolume(value);
		}
	}

	public static float GetVoiceVolume()
	{
		return voiceVolume;
	}

	public static void SetAmbientVolume(float value)
	{
		ambientVolume = value;
		if (sfxVCA.hasHandle())
		{
			sfxVCA.setVolume(value);
		}
	}

	public static float GetAmbientVolume()
	{
		return ambientVolume;
	}

	private void Awake()
	{
		if (instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		instance = this;
		Initialize();
	}

	private void Update()
	{
		if (!masterVCA.hasHandle())
		{
			GetBuses();
		}
	}
}
