using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Platform.IO;
using UnityEngine;

public static class SNUtils
{
	public enum RenderQueues
	{
		Background = 1000,
		Geometry = 2000,
		VehicleActual = 2301,
		ViewModelDepthClear = 2400,
		ViewModelActual = 2401,
		AlphaTest = 2450,
		Transparent = 3000,
		FloodWaterClipper = 3500,
		FloodWater = 3501,
		Overlay = 4000,
		NumQueues = 4001
	}

	public const string EngineDeveloperCheckFile = "__engine_developer_flag__.ignore";

	public static readonly string[] PrefabsResources = new string[4] { "WorldEntities", "Submarine", "Base", "Misc" };

	public const string EntityPrefabsResources = "WorldEntities";

	public const string WorldTilesPrefabsResources = "WorldTiles";

	public const string GlobalVoxelandPaletteResDir = "BlockPrefabs";

	public const string EntitySlotPrefabsResources = "WorldEntities/Slots";

	public const string MainSceneName = "Main";

	public const string MainScenePath = "Assets/Scenes/Main.unity";

	public const string resourcesPrefix = "Assets/AddressableResources/";

	public const string prefabSuffix = ".prefab";

	private static string _unmanagedDataDir = null;

	private static string _unmanagedSourceDataDir = null;

	private static string _buildTimeFile = null;

	private static string _buildNumberFile = null;

	public const string testDataDir = "SNTestData";

	public const string defaultWorldDir = "Build18";

	private static string _plasticChangeSetCache = null;

	private static string _devTempPath = null;

	private static bool _VerboseQueried = false;

	private static bool _VerboseDebug = false;

	public static string unmanagedDataDir
	{
		get
		{
			if (_unmanagedDataDir == null)
			{
				_unmanagedDataDir = Path.Combine(Application.streamingAssetsPath, "SNUnmanagedData");
			}
			return _unmanagedDataDir;
		}
	}

	public static string UnmanagedSourceDataDir
	{
		get
		{
			if (_unmanagedSourceDataDir == null)
			{
				_unmanagedSourceDataDir = Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, "..\\StreamingAssetsSource~\\SNUnmanagedData"));
			}
			return _unmanagedSourceDataDir;
		}
	}

	public static string BuildTimeFile
	{
		get
		{
			if (_buildTimeFile == null)
			{
				_buildTimeFile = Path.Combine(Application.streamingAssetsPath, "__buildtime.txt");
			}
			return _buildTimeFile;
		}
	}

	public static string BuildNumberFile
	{
		get
		{
			if (_buildNumberFile == null)
			{
				_buildNumberFile = Path.Combine(Application.streamingAssetsPath, "__buildnumber.txt");
			}
			return _buildNumberFile;
		}
	}

	public static string prefabDatabaseFilename => InsideUnmanaged("prefabs.db");

	public static string smokeOutDir => Environment.GetEnvironmentVariable("SN_SMOKE_OUTDIR");

	public static string plasticStatusFile => InsideUnmanaged("plastic_status.ignore");

	public static string buildDateTimeFile => BuildTimeFile;

	public static bool VerboseDebug
	{
		get
		{
			if (!_VerboseQueried)
			{
				_VerboseDebug = Environment.GetEnvironmentVariable("SN_VERBOSE_DEBUG") == "1";
				_VerboseQueried = true;
			}
			return _VerboseDebug;
		}
	}

	public static bool IsSmokeTesting()
	{
		if (smokeOutDir != null)
		{
			return smokeOutDir != "";
		}
		return false;
	}

	public static string GetPlasticChangeSetOfBuild()
	{
		if (_plasticChangeSetCache == null)
		{
			_plasticChangeSetCache = GetPlasticChangeSetOfBuildImpl();
		}
		return _plasticChangeSetCache;
	}

	private static string GetPlasticChangeSetOfBuildImpl()
	{
		if (!File.Exists(plasticStatusFile))
		{
			return string.Empty;
		}
		string text = File.ReadAllText(plasticStatusFile).Trim();
		if (int.TryParse(text, out var _))
		{
			return text;
		}
		return Regex.Match(text, "\\d+").Value;
	}

	public static int GetPlasticChangeSetOfBuild(int fallbackValue)
	{
		if (int.TryParse(GetPlasticChangeSetOfBuild(), out var result))
		{
			return result;
		}
		return fallbackValue;
	}

	public static DateTime GetDateTimeOfBuild()
	{
		DateTime result = DateTime.MinValue;
		if (File.Exists(buildDateTimeFile))
		{
			DateTime.TryParse(File.ReadAllText(buildDateTimeFile).Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
		}
		return result;
	}

	public static string InsideUnmanaged(string path)
	{
		return Path.Combine(unmanagedDataDir, path);
	}

	public static string InsideUnmanagedSourceFolder(string path)
	{
		return Path.Combine(UnmanagedSourceDataDir, path);
	}

	public static string GetDevTempPath()
	{
		if (!string.IsNullOrEmpty(_devTempPath))
		{
			return _devTempPath;
		}
		if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
		{
			return "sndevtemp/";
		}
		return "C:/sndevtemp";
	}

	public static void SetDevTempPath(string path)
	{
		_devTempPath = path;
	}

	public static void EnsureCustomLogPathExists()
	{
		string devTempPath = GetDevTempPath();
		if (!Directory.Exists(devTempPath))
		{
			Directory.CreateDirectory(devTempPath);
		}
	}

	public static string InsideCustomLogs(string path)
	{
		return Path.Combine(GetDevTempPath(), path);
	}

	public static string InsideDevTemp(string path)
	{
		return Path.Combine(GetDevTempPath(), path);
	}

	public static string InsideAssets(string path)
	{
		return Path.Combine(Application.dataPath, path);
	}

	public static bool IsEngineDeveloper()
	{
		return File.Exists("__engine_developer_flag__.ignore");
	}
}
