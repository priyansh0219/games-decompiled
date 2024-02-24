using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LumenWorks.Framework.IO.Csv;
using UWE;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.XR;

public class GameSettings : MonoBehaviour
{
	public delegate void OnSaveDelegate(UserStorageUtils.SaveOperation saveOperation);

	public interface ISerializer
	{
		bool IsReading();

		bool Serialize(string name, bool value);

		int Serialize(string name, int value);

		float Serialize(string name, float value);

		string Serialize(string name, string value);

		Color32 Serialize(string name, Color32 value);
	}

	private class Serializer : ISerializer
	{
		private bool reading;

		private SaveLoadManager.OptionsCache options;

		public Serializer(SaveLoadManager.OptionsCache _options, bool _reading)
		{
			options = _options;
			reading = _reading;
		}

		public bool IsReading()
		{
			return reading;
		}

		public bool Serialize(string name, bool value)
		{
			if (reading)
			{
				value = options.GetBool(name, value);
			}
			else
			{
				options.SetBool(name, value);
			}
			return value;
		}

		public int Serialize(string name, int value)
		{
			if (reading)
			{
				value = options.GetInt(name, value);
			}
			else
			{
				options.SetInt(name, value);
			}
			return value;
		}

		public float Serialize(string name, float value)
		{
			if (reading)
			{
				value = options.GetFloat(name, value);
			}
			else
			{
				options.SetFloat(name, value);
			}
			return value;
		}

		public string Serialize(string name, string value)
		{
			if (reading)
			{
				value = options.GetString(name, value);
			}
			else
			{
				options.SetString(name, value);
			}
			return value;
		}

		public Color32 Serialize(string name, Color32 value)
		{
			int num = value.r | (value.g << 8) | (value.b << 16) | (value.a << 24);
			if (reading)
			{
				num = options.GetInt(name, num);
				value.r = (byte)((uint)num & 0xFFu);
				value.g = (byte)((num & 0xFF00) >> 8);
				value.b = (byte)((num & 0xFF0000) >> 16);
				value.a = (byte)((num & 0xFF000000u) >> 24);
			}
			else
			{
				options.SetInt(name, num);
			}
			return value;
		}
	}

	private GameSettings instance;

	private const int settingsVersion = 10;

	public const string optionsContainerName = "options";

	private const string optionsFileName = "options.bin";

	private static string machineOptionsFileName;

	private static SaveLoadManager.OptionsCache defaultOptions;

	private void Awake()
	{
		machineOptionsFileName = SystemInfo.deviceUniqueIdentifier + "-options.bin";
		if (instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			instance = this;
		}
	}

	private IEnumerator Start()
	{
		while (PlatformUtils.main.GetServices() == null)
		{
			yield return null;
		}
		ValidateOptions();
		GetDefaultOptions();
		SetupGraphicsSettingsForConsoles();
	}

	public static float GetMinVrRenderScale()
	{
		return 0.5f;
	}

	private static IEnumerator GetGpuScoreAsync(IOut<int> score)
	{
		AsyncOperationHandle<TextAsset> request = AddressablesUtility.LoadAsync<TextAsset>("gpu_scores.csv");
		yield return request;
		score.Set(-1);
		if (request.Status == AsyncOperationStatus.Succeeded)
		{
			TextAsset result = request.Result;
			string graphicsDeviceName = SystemInfo.graphicsDeviceName;
			using (CsvReader csvReader = new CsvReader(new StringReader(result.text), hasHeaders: false))
			{
				while (csvReader.ReadNextRecord())
				{
					if (csvReader[0] == graphicsDeviceName)
					{
						if (csvReader[1] == "medium")
						{
							score.Set(1);
						}
						else if (csvReader[1] == "high")
						{
							score.Set(2);
						}
						else
						{
							score.Set(0);
						}
						yield break;
					}
				}
			}
		}
		AddressablesUtility.QueueRelease(ref request);
	}

	public static IEnumerator SaveAsync(OnSaveDelegate onSave = null)
	{
		Debug.Log("Saving game settings...");
		SaveLoadManager.OptionsCache optionsCache = new SaveLoadManager.OptionsCache();
		Serializer serializer = new Serializer(optionsCache, _reading: false);
		SerializeSettings(serializer);
		((ISerializer)serializer).Serialize("Version", 10);
		UserStorage userStorage = PlatformUtils.main.GetUserStorage();
		using (MemoryStream stream = new MemoryStream())
		{
			using (PooledObject<ProtobufSerializer> pooledObject = ProtobufSerializerPool.GetProxy())
			{
				pooledObject.Value.Serialize(stream, optionsCache);
			}
			byte[] value = stream.ToArray();
			Dictionary<string, byte[]> files = new Dictionary<string, byte[]>
			{
				{ "options.bin", value },
				{ machineOptionsFileName, value }
			};
			UserStorageUtils.SaveOperation saveOperation = userStorage.SaveFilesAsync("options", files);
			yield return saveOperation;
			if (saveOperation.GetSuccessful())
			{
				Debug.Log("Saving game settings complete!");
			}
			else if (saveOperation.result == UserStorageUtils.Result.InvalidFormat)
			{
				UserStorageUtils.AsyncOperation deleteOperation = userStorage.DeleteContainerAsync("options");
				yield return deleteOperation;
				if (deleteOperation.GetSuccessful())
				{
					saveOperation = userStorage.SaveFilesAsync("options", files);
					yield return saveOperation;
					Debug.LogFormat("Saving game settings complete after deletion of corrupted save data with result [{0}] {1}", saveOperation.result, saveOperation.errorMessage);
				}
			}
			else
			{
				Debug.LogFormat("Saving game settings failed: [{0}] {1}", saveOperation.result, saveOperation.errorMessage);
			}
			onSave?.Invoke(saveOperation);
		}
	}

	public static CoroutineTask<bool> LoadAsync()
	{
		TaskResult<bool> taskResult = new TaskResult<bool>();
		return new CoroutineTask<bool>(LoadAsync(taskResult), taskResult);
	}

	public static void ApplyDefaultConsoleSettings()
	{
	}

	public static IEnumerator LoadAsync(IOut<bool> success)
	{
		Debug.Log("Loading game settings");
		SaveLoadManager.OptionsCache options = new SaveLoadManager.OptionsCache();
		UserStorage userStorage = PlatformUtils.main.GetUserStorage();
		List<string> files = new List<string> { machineOptionsFileName };
		UserStorageUtils.LoadOperation loadOperation = userStorage.LoadFilesAsync("options", files);
		yield return loadOperation;
		if (!loadOperation.GetSuccessful() || !loadOperation.files.ContainsKey(machineOptionsFileName))
		{
			files.Clear();
			files.Add("options.bin");
			if (PlatformUtils.isWindowsStore && PlatformUtils.main.GetServices() is PlatformServicesWindows platformServicesWindows)
			{
				UserStorage localStorage = platformServicesWindows.GetLocalStorage();
				if (localStorage != null)
				{
					userStorage = localStorage;
				}
			}
			loadOperation = userStorage.LoadFilesAsync("options", files);
			yield return loadOperation;
		}
		if (loadOperation.GetSuccessful() && loadOperation.files.TryGetValue(files[0], out var value))
		{
			using (MemoryStream stream = new MemoryStream(value))
			{
				using (PooledObject<ProtobufSerializer> pooledObject = ProtobufSerializerPool.GetProxy())
				{
					pooledObject.Value.Deserialize(stream, options, verbose: false);
				}
			}
		}
		else
		{
			TaskResult<int> result = new TaskResult<int>();
			yield return GetGpuScoreAsync(result);
			int num = result.Get();
			if (num == -1)
			{
				num = 2;
			}
			int xResolution = 1920;
			int yResolution = 1080;
			if (num == 0)
			{
				xResolution = 1280;
				yResolution = 720;
			}
			GraphicsUtil.GetClosestSupportedResolution(ref xResolution, ref yResolution);
			Screen.SetResolution(xResolution, yResolution, fullscreen: true);
			GraphicsPreset.GetPresets()[num].Apply();
		}
		Serializer serializer = new Serializer(options, _reading: true);
		SerializeSettings(serializer);
		UpgradeSettings(serializer);
		ValidateOptions();
		success.Set(value: true);
	}

	private static SaveLoadManager.OptionsCache GetDefaultOptions()
	{
		if (defaultOptions == null)
		{
			defaultOptions = new SaveLoadManager.OptionsCache();
			Serializer serializer = new Serializer(defaultOptions, _reading: false);
			SerializeSettings(serializer);
			((ISerializer)serializer).Serialize("Version", 10);
		}
		return defaultOptions;
	}

	private static void ValidateOptions()
	{
		if (XRSettings.eyeTextureResolutionScale < GetMinVrRenderScale())
		{
			XRSettings.eyeTextureResolutionScale = 1f;
		}
	}

	private static void UpgradeSettings(ISerializer serializer)
	{
		int num = serializer.Serialize("Version", 0);
		GameInput.UpgradeSettings(serializer);
		if (num < 5)
		{
			MiscSettings.fieldOfView = serializer.Serialize("Graphics/FOV", MiscSettings.fieldOfView);
			GraphicsPreset.GetPresets()[QualitySettings.GetQualityLevel()].Apply();
			UwePostProcessingManager.SetAaMode(serializer.Serialize("Graphics/AntiAliasingMode", UwePostProcessingManager.GetAaMode()));
			UwePostProcessingManager.SetAaQuality(serializer.Serialize("Graphics/AntiAliasingQuality", UwePostProcessingManager.GetAaQuality()));
			UwePostProcessingManager.SetAoQuality(serializer.Serialize("Graphics/AmbientOcclusionQuality", UwePostProcessingManager.GetAoQuality()));
			UwePostProcessingManager.SetSsrQuality(serializer.Serialize("Graphics/ScreenSpaceReflectionsQuality", UwePostProcessingManager.GetSsrQuality()));
			UwePostProcessingManager.ToggleBloom(serializer.Serialize("Graphics/Bloom", UwePostProcessingManager.GetBloomEnabled()));
			UwePostProcessingManager.ToggleBloomLensDirt(serializer.Serialize("Graphics/BloomLensDirt", UwePostProcessingManager.GetBloomLensDirtEnabled()));
			UwePostProcessingManager.ToggleDof(serializer.Serialize("Graphics/DepthOfField", UwePostProcessingManager.GetDofEnabled()));
			UwePostProcessingManager.ToggleDithering(serializer.Serialize("Graphics/Dithering", UwePostProcessingManager.GetDitheringEnabled()));
			UwePostProcessingManager.SetMotionBlurQuality(serializer.Serialize("Graphics/MotionBlurQuality", UwePostProcessingManager.GetMotionBlurQuality()));
		}
		if (num < 10 && Enum.TryParse<WaterSurface.Quality>(serializer.Serialize("Graphics/WaterQuality", WaterSurface.GetQuality().ToString()), out var result))
		{
			WaterSurface.SetQuality(result);
		}
	}

	private static void SerializeSettings(ISerializer serializer)
	{
		SerializeGraphicsSettings(serializer);
		GameInput.SerializeSettings(serializer);
		SerializeSoundSettings(serializer);
		SerializeVRSettings(serializer);
		SerializeLocaleSettings(serializer);
		SerializeMiscSettings(serializer);
	}

	private static void SerializeGraphicsSettings(ISerializer serializer)
	{
		WaterSurface.SetQuality((WaterSurface.Quality)serializer.Serialize("Graphics/WaterSurfaceQuality", (int)WaterSurface.GetQuality()));
		GraphicsUtil.SetQualityLevel(serializer.Serialize("Graphics/Quality", QualitySettings.GetQualityLevel()));
		GraphicsUtil.SetVSyncEnabled(serializer.Serialize("Graphics/VSync", GraphicsUtil.GetVSyncEnabled()));
		UwePostProcessingManager.SetAaMode(serializer.Serialize("Graphics/AntiAliasingMode", UwePostProcessingManager.GetAaMode()));
		UwePostProcessingManager.SetAaQuality(serializer.Serialize("Graphics/AntiAliasingQuality", UwePostProcessingManager.GetAaQuality()));
		UwePostProcessingManager.SetAoQuality(serializer.Serialize("Graphics/AmbientOcclusionQuality", UwePostProcessingManager.GetAoQuality()));
		UwePostProcessingManager.SetSsrQuality(serializer.Serialize("Graphics/ScreenSpaceReflectionsQuality", UwePostProcessingManager.GetSsrQuality()));
		UwePostProcessingManager.ToggleBloom(serializer.Serialize("Graphics/Bloom", UwePostProcessingManager.GetBloomEnabled()));
		UwePostProcessingManager.ToggleBloomLensDirt(serializer.Serialize("Graphics/BloomLensDirt", UwePostProcessingManager.GetBloomLensDirtEnabled()));
		UwePostProcessingManager.ToggleDof(serializer.Serialize("Graphics/DepthOfField", UwePostProcessingManager.GetDofEnabled()));
		UwePostProcessingManager.SetMotionBlurQuality(serializer.Serialize("Graphics/MotionBlurQuality", UwePostProcessingManager.GetMotionBlurQuality()));
		UwePostProcessingManager.ToggleDithering(serializer.Serialize("Graphics/Dithering", UwePostProcessingManager.GetDitheringEnabled()));
		GraphicsUtil.SetFrameRate(serializer.Serialize("Graphics/FrameRate", GraphicsUtil.GetFrameRate()));
		GammaCorrection.gamma = serializer.Serialize("Graphics/Gamma", GammaCorrection.gamma);
		UwePostProcessingManager.SetColorGradingMode(serializer.Serialize("Graphics/ColorGrading", UwePostProcessingManager.GetColorGradingMode()));
	}

	private static void SerializeSoundSettings(ISerializer serializer)
	{
		SoundSystem.SetMasterVolume(serializer.Serialize("Sound/MasterVolume", SoundSystem.GetMasterVolume()));
		SoundSystem.SetMusicVolume(serializer.Serialize("Sound/MusicVolume", SoundSystem.GetMusicVolume()));
		SoundSystem.SetVoiceVolume(serializer.Serialize("Sound/VoiceVolume", SoundSystem.GetVoiceVolume()));
		SoundSystem.SetAmbientVolume(serializer.Serialize("Sound/AmbientVolume", SoundSystem.GetAmbientVolume()));
	}

	private static void SerializeVRSettings(ISerializer serializer)
	{
		XRSettings.eyeTextureResolutionScale = serializer.Serialize("VR/RenderScale", XRSettings.eyeTextureResolutionScale);
		VROptions.gazeBasedCursor = serializer.Serialize("VR/GazeBasedCursor", VROptions.gazeBasedCursor);
	}

	private static void SerializeLocaleSettings(ISerializer serializer)
	{
		Language.main.SetCurrentLanguage(serializer.Serialize("Locale/Language", Language.main.GetCurrentLanguage()));
		Language.main.showSubtitles = serializer.Serialize("Locale/Subtitles", Language.main.showSubtitles);
		Subtitles.speed = serializer.Serialize("Locale/SubtitlesSpeed", Subtitles.speed);
	}

	private static void SerializeMiscSettings(ISerializer serialize)
	{
		MiscSettings.fieldOfView = serialize.Serialize("Misc/FieldOfView", MiscSettings.fieldOfView);
		MiscSettings.consoleHistory = serialize.Serialize("Misc/ConsoleHistory", MiscSettings.consoleHistory);
		MiscSettings.cameraBobbing = serialize.Serialize("Misc/CameraBobbing", MiscSettings.cameraBobbing);
		MiscSettings.email = serialize.Serialize("Misc/Email", MiscSettings.email);
		MiscSettings.rememberEmail = serialize.Serialize("Misc/RememberEmail", MiscSettings.rememberEmail);
		MiscSettings.hideEmailBox = serialize.Serialize("Misc/HideEmailBox", MiscSettings.hideEmailBox);
		MiscSettings.newsEnabled = serialize.Serialize("Misc/NewsEnabled", MiscSettings.newsEnabled);
		MiscSettings.flashes = serialize.Serialize("Misc/Flashes", MiscSettings.flashes);
		MiscSettings.pdaPause = serialize.Serialize("Misc/PDAPause", MiscSettings.pdaPause);
		MiscSettings.runInBackground = serialize.Serialize("Misc/RunInBackground", MiscSettings.runInBackground);
		MiscSettings.SetUIScale(serialize.Serialize("Misc/UIScale", MiscSettings.GetUIScale(DisplayOperationMode.Default)), DisplayOperationMode.Default);
		MiscSettings.SetUIScale(serialize.Serialize("Misc/UIScaleHandheld", MiscSettings.GetUIScale(DisplayOperationMode.Handheld)), DisplayOperationMode.Handheld);
	}

	private static void SetupGraphicsSettingsForConsoles()
	{
	}
}
