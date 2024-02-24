using System;
using System.Collections;
using System.IO;
using UWE;
using UnityEngine;
using WorldBuilding;
using WorldStreaming;

[RequireComponent(typeof(LargeWorldStreamer))]
[RequireComponent(typeof(Voxeland))]
public sealed class LargeWorld : MonoBehaviour
{
	public static LargeWorld main;

	[HideInInspector]
	public string dataDir;

	[NonSerialized]
	public LargeWorldStreamer streamer;

	[NonSerialized]
	public Voxeland land;

	[NonSerialized]
	public bool worldMounted;

	[NonSerialized]
	public string state = "uninit";

	public SignalDatabase signalDatabase;

	[NonSerialized]
	private BiomeMapData biomeMapData;

	public string BiomesCSVPath => Path.Combine(dataDir, "biomes.csv");

	public string BiomeMapPath => Path.Combine(dataDir, "biomeMap.bin");

	public int DebugBiomeMap(Int2 block)
	{
		biomeMapData.TryGetBiomeIndexForCoords(block, out var index);
		return index;
	}

	internal bool TryGetBiomeProperties(Int2 block, out BiomeProperties biomeProperties)
	{
		return biomeMapData.TryGetBiomeProperties(block, out biomeProperties);
	}

	public string GetBiome(Int3 block)
	{
		string overrideBiome = streamer.GetOverrideBiome(block);
		if (overrideBiome != null)
		{
			return overrideBiome;
		}
		if (!TryGetBiomeProperties(block.xz, out var biomeProperties))
		{
			return null;
		}
		return biomeProperties.name;
	}

	public bool IsMounted()
	{
		return state == "mounted";
	}

	public string GetBiome(Vector3 wsPos)
	{
		if (IsMounted())
		{
			Int3 block = Int3.Floor(land.transform.InverseTransformPoint(wsPos));
			return GetBiome(block);
		}
		return null;
	}

	private void Awake()
	{
		main = this;
	}

	private void OnDestroy()
	{
		main = null;
		biomeMapData.Destroy();
		streamer = null;
	}

	public static bool IsValidWorldDir(string dir)
	{
		if (!Directory.Exists(dir))
		{
			return false;
		}
		if (!File.Exists(GetWorldMetaPath(dir)))
		{
			return false;
		}
		return true;
	}

	private static string GetWorldMetaPath(string dir)
	{
		return Path.Combine(dir, "meta.txt");
	}

	private static bool CheckWorld(string dir, out string paletteDir)
	{
		paletteDir = null;
		string worldMetaPath = GetWorldMetaPath(dir);
		if (!IsValidWorldDir(dir))
		{
			Debug.Log("Valid world does not exist at '" + dir + "'! Should have a world meta info " + worldMetaPath + "!");
			return false;
		}
		try
		{
			using (StreamReader streamReader = FileUtils.ReadTextFile(worldMetaPath))
			{
				if (!int.TryParse(streamReader.ReadLine(), out var result))
				{
					Debug.Log("Valid world does not exist at '" + dir + "'! Should have a valid world meta info " + worldMetaPath + "!");
					return false;
				}
				Debug.Log("World in " + dir + " is from version " + result);
				paletteDir = streamReader.ReadLine()?.Trim();
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return false;
		}
		return true;
	}

	public CoroutineTask<Result> MountWorldAsync(string _dataDir, LargeWorldStreamer _streamer, WorldStreamer _streamerV2, Voxeland _land)
	{
		TaskResult<Result> result = new TaskResult<Result>();
		return new CoroutineTask<Result>(MountWorldAsync(_dataDir, _streamer, _streamerV2, _land, result), result);
	}

	private IEnumerator MountWorldAsync(string _dataDir, LargeWorldStreamer _streamer, WorldStreamer _streamerV2, Voxeland _land, IOut<Result> result)
	{
		if (state == "uninit")
		{
			if (!CheckWorld(_dataDir, out var paletteDir))
			{
				Debug.LogFormat(this, "CheckWorld failed. Frame {0}.", Time.frameCount);
				result.Set(Result.Failure("CheckWorldFailure"));
				yield break;
			}
			Debug.LogFormat(this, "LargeWorld: Loading world. Frame {0}.", Time.frameCount);
			main = this;
			dataDir = _dataDir;
			land = _land;
			streamer = _streamer;
			if (paletteDir != "")
			{
				land.paletteResourceDir = paletteDir;
			}
			Debug.LogFormat(this, "LargeWorld land '{0}'", _land);
			_streamer.Deinitialize();
			_land.data = ScriptableObject.CreateInstance<VoxelandData>();
			Timer.Begin("Streamer initialize");
			Result value;
			try
			{
				value = _streamer.Initialize(_streamerV2, _land, _dataDir);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex, this);
				value = Result.Failure(ex.Message);
			}
			Timer.End();
			_streamer.frozen = true;
			if (!value.success)
			{
				result.Set(value);
				yield break;
			}
			Timer.Begin("cellManager.ResetEntityDistributions");
			_streamer.cellManager.ResetEntityDistributions();
			Timer.End();
			yield return null;
			Timer.Begin("LoadSceneObjects");
			yield return _streamer.LoadSceneObjectsAsync();
			Timer.End();
			yield return null;
			Timer.Begin("LoadGlobalRoot");
			yield return _streamer.LoadGlobalRootAsync();
			Timer.End();
			yield return null;
			if (_land.paletteResourceDir != "")
			{
				Debug.LogFormat(this, "loading palette for LargeWorld at '{0}'.", _land.paletteResourceDir);
				Timer.Begin("Loading palette");
				_land.LoadPalette(_land.paletteResourceDir);
				Timer.End();
				yield return null;
			}
			else
			{
				Debug.Log("no custom palette dir specified - leaving palette alone.");
			}
			if (FileUtils.FileExists(BiomeMapPath))
			{
				Timer.Begin("Loading biome map");
				InitializeBiomeMap();
				Timer.End();
				yield return null;
			}
			_land.disableAutoSerialize = true;
			_land.readOnly = false;
			_land.dynamicRebuilding = true;
			Debug.LogFormat(this, "LargeWorld: calling land.Rebuild frame {0}", Time.frameCount);
			Timer.Begin("Rebuilding land");
			_land.Rebuild();
			Timer.End();
			yield return null;
			if ((bool)_streamerV2)
			{
				WorldStreamer.Settings settings = new WorldStreamer.Settings
				{
					worldPath = _dataDir,
					numOctrees = _land.data.GetNodeCount(),
					numOctreesPerBatch = _streamer.treesPerBatch.x,
					octreeSize = _land.data.biggestNode
				};
				_streamerV2.Start(_land.types, settings);
				_streamerV2.clipmapStreamer.RegisterListener(_streamer.cellManager);
				yield return null;
			}
			state = "mounted";
			worldMounted = true;
			result.Set(Result.Success());
		}
		else
		{
			Debug.LogWarningFormat(this, "Can not mount world in state {0}.", state);
			result.Set(Result.Failure("WorldAlreadyMounted"));
		}
	}

	public void InitializeBiomeMap()
	{
		biomeMapData.Load(BiomeMapPath, BiomesCSVPath, land.data.sizeX);
	}
}
