using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using Gendarme;
using Platform.IO;
using UWE;
using UnityEngine;
using WorldStreaming;

[RequireComponent(typeof(LargeWorld))]
public sealed class LargeWorldStreamer : MonoBehaviour, VoxelandEventHandler
{
	[Serializable]
	public sealed class Settings
	{
		public int maxFrameMs = 10;

		public int maxSubFrameMs = 10;

		public int maxInsideFrameMs = 10;

		public int maxLoadingFrameMs = 100;

		public bool overrideDebugSkipEntities;

		public float budgetMBs = 200f;

		public bool warmupAllShaders;

		public int batchLoadRings = 1;

		public BatchOctreesStreamer.Settings octreesSettings;

		public BatchOctreesStreamer.Settings lowDetailOctreesSettings;

		public bool disableFarColliders;

		public int GetMaxFrameMs(Player player)
		{
			if (WaitScreen.IsWaiting)
			{
				return maxLoadingFrameMs;
			}
			int num = maxFrameMs;
			if ((bool)player)
			{
				if (player.IsPiloting())
				{
					num = maxSubFrameMs;
				}
				if (player.IsInsideWalkable())
				{
					num = maxInsideFrameMs;
				}
			}
			if (QualitySettings.vSyncCount < 2)
			{
				num /= 2;
			}
			return num;
		}

		public override string ToString()
		{
			return $"max frame ms {maxFrameMs}, skip ents {overrideDebugSkipEntities}, budget {budgetMBs}";
		}
	}

	public struct CompactOctreeSaveItem
	{
		public readonly Int3 index;

		public readonly CompactOctree octree;

		public CompactOctreeSaveItem(Int3 _index, CompactOctree _octree)
		{
			index = _index;
			octree = _octree;
		}
	}

	private sealed class UpdateBatchStreamingCoroutine : StateMachineBase<LargeWorldStreamer>
	{
		private Int3 best;

		private Int3 worst;

		public override bool MoveNext()
		{
			try
			{
				switch (state)
				{
				case 0:
				{
					if (IngameMenu.IsQuitting())
					{
						host.isIdle = true;
						return false;
					}
					if (host.frozen)
					{
						return false;
					}
					host.loadedMBsOut = (float)VoxelandData.OctNode.GetPoolBytesUsed() / 1024f / 1024f;
					Vector3 vector = new Vector3(0f, -8f, 0f);
					Vector3 vector2 = host.cachedCameraPosition + vector;
					Int3 containingBatch = host.GetContainingBatch(vector2);
					host.isIdle = false;
					Int3.Bounds effectiveBounds = host.GetEffectiveBounds(containingBatch);
					if (host.TryGetWorstBatch(vector2, effectiveBounds, out worst))
					{
						if (host.TryUnloadBatch(worst))
						{
							if (host.streamerV2 != null)
							{
								host.streamerV2.IncreaseFreezeCount();
							}
							current = host.cellManager.IncreaseFreezeCount();
							state = 1;
							host.debugNumBatchesUnloading++;
							return true;
						}
					}
					else
					{
						if (host.TryGetBestBatch(vector2, effectiveBounds, out best))
						{
							BatchCells batchCells = host.cellManager.InitializeBatchCells(best);
							current = host.LoadBatchTaskedAsync(batchCells, _editMode: false);
							state = 3;
							host.debugNumBatchesLoading++;
							return true;
						}
						host.isIdle = true;
					}
					return false;
				}
				case 1:
					current = host.SaveBatchTmpAsync(worst);
					state = 2;
					return true;
				case 2:
					host.UnloadBatch(worst);
					host.cellManager.DecreaseFreezeCount();
					if (host.streamerV2 != null)
					{
						host.streamerV2.DecreaseFreezeCount();
					}
					host.debugNumBatchesUnloading--;
					return false;
				case 3:
					current = host.FinalizeLoadBatchAsync(best, _editMode: false);
					state = 4;
					host.debugNumBatchesLoading--;
					return true;
				case 4:
					return false;
				default:
					UnityEngine.Debug.LogErrorFormat(host, "Unexpected state {0} in UpdateBatchStreamingCoroutine", state);
					return false;
				}
			}
			finally
			{
			}
		}

		public override void Reset()
		{
			best = Int3.zero;
			worst = Int3.zero;
		}
	}

	private sealed class LoadBatchTask : IWorkerTask, IAsyncOperation
	{
		private readonly LargeWorldStreamer streamer;

		private readonly BatchCells batchCells;

		private const bool editMode = false;

		private bool done;

		public bool isDone => done;

		public LoadBatchTask(LargeWorldStreamer _streamer, BatchCells _batchCells, bool _editMode)
		{
			streamer = _streamer;
			batchCells = _batchCells;
		}

		public void Execute()
		{
			try
			{
				CoroutineUtils.PumpCoroutine(streamer.LoadBatchThreadedAsync(batchCells, _editMode: false));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception, streamer);
			}
			finally
			{
				done = true;
			}
		}

		public override string ToString()
		{
			return $"LoadBatchTask {batchCells.batch}";
		}
	}

	public class RasterProxy : VoxelandRasterizer
	{
		private readonly LargeWorldStreamer streamer;

		public RasterProxy(LargeWorldStreamer _streamer)
		{
			streamer = _streamer;
		}

		public void LayoutDebugGUI()
		{
		}

		public void Rasterize(Voxeland land, Array3<byte> typesGrid, Array3<byte> densityGrid, Int3 size, int wx, int wy, int wz, int downsamples)
		{
			int x = wx + (size.x << downsamples) - 1;
			int y = wy + (size.y << downsamples) - 1;
			int z = wz + (size.z << downsamples) - 1;
			Int3 @int = new Int3(wx, wy, wz);
			Int3 int2 = new Int3(x, y, z);
			foreach (Int3 item in Int3.Range(Int3.Max(Int3.zero, @int / 32), Int3.Min(streamer.compactTrees.Dims() - 1, int2 / 32)))
			{
				streamer.compactTrees.Get(item)?.RasterizeNative(0, typesGrid, densityGrid, size, wx >> downsamples, wy >> downsamples, wz >> downsamples, item.x * 32 >> downsamples, item.y * 32 >> downsamples, item.z * 32 >> downsamples, 32 >> downsamples + 1);
			}
		}

		public bool IsRangeUniform(Int3.Bounds blockRange)
		{
			foreach (Int3 item in Int3.Range(blockRange.mins / streamer.blocksPerTree, blockRange.maxs / streamer.blocksPerTree))
			{
				if (streamer.compactTrees.CheckBounds(item) && streamer.compactTrees.Get(item) != null && !streamer.compactTrees.Get(item).IsEmpty())
				{
					return false;
				}
			}
			return true;
		}

		public bool IsRangeLoaded(Int3.Bounds blockRange, int downsamples)
		{
			return true;
		}

		public void OnPreBuildRange(Int3.Bounds blockRange)
		{
		}
	}

	private delegate float DistanceField(Vector3 wsPos);

	[AssertNotNull]
	public GameObject cellRootPrefab;

	[AssertNotNull]
	public GameObject batchRootPrefab;

	[AssertNotNull]
	public GameObject entitySlotsPlaceholderPrefab;

	[AssertNotNull]
	public TerrainPoolManager terrainPoolManager;

	private const int CompiledOctreesVersion = 4;

	public const string CompiledCacheFolder = "CompiledOctreesCache";

	public const string BatchObjectsCacheFolder = "BatchObjectsCache";

	[NonSerialized]
	public CellManager cellManager;

	private const bool debugSkipTerrain = true;

	public static LargeWorldStreamer main;

	[NonSerialized]
	public LargeWorld world;

	public float loadedMBsOut;

	private const bool Verbose = false;

	public const bool DebugBatchCells = false;

	public const bool DebugDisableSlotEnts = false;

	private const bool DebugKeepGroupRoots = false;

	private const bool DebugDisableLowDetailTerrain = false;

	public const bool DebugDisableAllEnts = false;

	private const bool DebugSkipEntityLoad = false;

	private const bool DebugOverrideDisableGrass = false;

	public int debugNumBatchesLoading;

	public int debugNumBatchesUnloading;

	private const bool DebugDisableFastRangeLookup = false;

	private const bool DebugSkipCppFaceScan = false;

	private bool inited;

	private bool isIdle;

	[NonSerialized]
	public bool frozen;

	private Array3<float> batchSizeMBs;

	private Array3<bool> batchOctreesCached;

	private readonly HashSet<Int3> loadedBatches = new HashSet<Int3>(Int3.equalityComparer);

	[NonSerialized]
	private GameObject transientRoot;

	[NonSerialized]
	public GameObject globalRoot;

	[AssertNotNull]
	public Transform batchesRoot;

	[AssertNotNull]
	public Transform cellsRoot;

	[AssertNotNull]
	public Transform waitersRoot;

	[NonSerialized]
	public readonly Dictionary<Int3, LargeWorldBatchRoot> batch2root = new Dictionary<Int3, LargeWorldBatchRoot>(Int3.equalityComparer);

	[HideInInspector]
	public int maxInstanceLayer;

	[HideInInspector]
	public int minInstanceLayer;

	[NonSerialized]
	public VoxelandData data;

	[NonSerialized]
	public VoxelandData compiledVoxels;

	[NonSerialized]
	public VoxelandData bakedVoxels;

	[HideInInspector]
	public Voxeland land;

	private RasterProxy proxy;

	[NonSerialized]
	public WorldStreamer streamerV2;

	public Array3<CompactOctree> compactTrees;

	private WorkerThread workerThread;

	public Int3 treesPerBatch;

	private Int3 nodeCount;

	[NonSerialized]
	private Settings settings;

	private readonly List<CompactOctreeSaveItem> compactOctreeBuffer = new List<CompactOctreeSaveItem>();

	private readonly ObjectPool<CompactOctree> compactOctreePool = ObjectPoolHelper.CreatePool<CompactOctree>(20000);

	private static readonly StateMachinePool<UpdateBatchStreamingCoroutine, LargeWorldStreamer> updateBatchStreamingCoroutinePool = new StateMachinePool<UpdateBatchStreamingCoroutine, LargeWorldStreamer>();

	private static Int3 cachedRequestedIndex = Int3.negativeOne;

	private static string cachedPathIndexString = string.Empty;

	private static readonly Dictionary<string, string> combinedOctreeCachePrefix = new Dictionary<string, string>();

	private IAlloc<byte> batchObjectsBuffer;

	private int batchLoadRings => settings.batchLoadRings;

	public float budgetMBsOut => settings.budgetMBs;

	public int blocksPerTree => data.biggestNode;

	public Int3 blocksPerBatch => blocksPerTree * treesPerBatch;

	public Int3 worldSize => data.GetSize();

	internal Int3 batchCount => land.data.GetNodeCount().CeilDiv(treesPerBatch);

	private bool showLowDetailTerrain => true;

	[HideInInspector]
	public string tmpPathPrefix { get; private set; }

	[HideInInspector]
	public string pathPrefix { get; private set; }

	internal Vector3 cachedCameraPosition { get; private set; }

	internal Vector3 cachedCameraForward { get; private set; }

	internal float cachedTime { get; private set; }

	public static event EventHandler onLoadActivity;

	public bool IsReady()
	{
		return inited;
	}

	private void SetPathPrefix(string _pathPrefix)
	{
		pathPrefix = _pathPrefix;
	}

	private void SaveCompactOctrees(Int3 batchIndex, string path)
	{
		using (BinaryWriter binaryWriter = new BinaryWriter(FileUtils.CreateFile(path)))
		{
			binaryWriter.Write(4);
			foreach (Int3 item in Int3.Range(treesPerBatch))
			{
				Int3 @int = batchIndex * treesPerBatch + item;
				if (CheckRoot(@int))
				{
					CompactOctree compactOctree = compactTrees.Get(@int);
					if (compactOctree == null)
					{
						binaryWriter.Write(Convert.ToUInt16(0));
					}
					else
					{
						compactOctree.Write(binaryWriter);
					}
				}
			}
		}
	}

	public IEnumerable<CompactOctreeSaveItem> ReadCompiledOctrees(PoolingBinaryReader reader, int version, Int3 batchId)
	{
		compactOctreeBuffer.Clear();
		Int3.RangeEnumerator rangeEnumerator = Int3.Range(treesPerBatch);
		while (rangeEnumerator.MoveNext())
		{
			Int3 current = rangeEnumerator.Current;
			Int3 @int = batchId * treesPerBatch + current;
			if (CheckRoot(@int))
			{
				CompactOctree compactOctree = compactOctreePool.Get();
				compactOctree.Read(reader, version, batchId, current);
				compactOctreeBuffer.Add(new CompactOctreeSaveItem(@int, compactOctree));
			}
		}
		return compactOctreeBuffer;
	}

	private void FinalizeLoadCompiledOctrees()
	{
		if (compactOctreeBuffer.Count == 0)
		{
			return;
		}
		foreach (CompactOctreeSaveItem item in compactOctreeBuffer)
		{
			Int3 index = item.index;
			CompactOctree compactOctree = compactTrees.Get(index);
			if (compactOctree != null)
			{
				compactOctree.NotifyUnload();
				compactOctreePool.Return(compactOctree);
			}
			CompactOctree octree = item.octree;
			compactTrees.Set(index, octree);
		}
		compactOctreeBuffer.Clear();
	}

	public void SaveBatchCompiledCache(Int3 index)
	{
		SaveBatchCompiledCache(index, pathPrefix);
	}

	private void SaveBatchCompiledCache(Int3 index, string targetPathPrefix)
	{
		string path = Platform.IO.Path.Combine(targetPathPrefix, "CompiledOctreesCache");
		try
		{
			if (!Platform.IO.Directory.Exists(path))
			{
				Platform.IO.Directory.CreateDirectory(path);
			}
		}
		catch (UnauthorizedAccessException exception)
		{
			UnityEngine.Debug.LogException(exception, this);
			ErrorMessage.AddError(Language.main.Get("UnauthorizedAccessException"));
			return;
		}
		SaveCompactOctrees(index, GetCompiledOctreesCachePath(targetPathPrefix, GetCompiledOctreesCacheFilename(index)));
	}

	private IEnumerator SaveBatchTmpAsync(Int3 index)
	{
		yield return cellManager.SaveBatchCellsTmpAsync(index);
	}

	private void Awake()
	{
		main = this;
		inited = false;
		ProfilingUtils.SetMainThreadId(Thread.CurrentThread.ManagedThreadId);
		DevConsole.RegisterConsoleCommand(this, "entstats");
		DevConsole.RegisterConsoleCommand(this, "entreset");
		DevConsole.RegisterConsoleCommand(this, "gamereset");
		DevConsole.RegisterConsoleCommand(this, "dig");
		GameObject obj = new GameObject("Transient Root");
		obj.transform.parent = base.transform.parent;
		transientRoot = obj;
	}

	private void OnConsoleCommand_dig(NotificationCenter.Notification n)
	{
		float radius = 2f;
		float num = 1f;
		if (n.data.Count > 0)
		{
			radius = float.Parse((string)n.data[0]);
		}
		if (n.data.Count > 1)
		{
			num = float.Parse((string)n.data[1]);
		}
		Vector3 center = cachedCameraPosition + num * cachedCameraForward;
		PerformSphereEdit(center, radius, isAdd: false, 1);
	}

	private void OnConsoleCommand_gamereset()
	{
		StartCoroutine(ResetGameAsync());
	}

	private IEnumerator ResetGameAsync()
	{
		cellManager.ResetEntityDistributions();
		ForceUnloadAll();
		UnloadGlobalRoot();
		yield return LoadGlobalRootAsync();
		yield return LoadSceneObjectsAsync();
	}

	private void OnConsoleCommand_entreset()
	{
		cellManager.ResetEntityDistributions();
		ForceUnloadAll();
	}

	private void OnConsoleCommand_entstats()
	{
		cellManager.EntStats();
	}

	public static string GetStreamingSettingsFileForQualityLevel(int qualityLevel)
	{
		string text = QualitySettings.names[qualityLevel];
		string arg = string.Join("_", text.ToLower().Split(Platform.IO.Path.GetInvalidFileNameChars()));
		string result = SNUtils.InsideUnmanaged($"streaming-{arg}.json");
		string argument = CommandLine.GetArgument("-streamingSettings");
		UnityEngine.Debug.Log("Streaming settings from the command line: " + argument);
		if (!string.IsNullOrEmpty(argument))
		{
			result = argument;
		}
		return result;
	}

	private void LoadSettings()
	{
		string json = Platform.IO.File.ReadAllText(GetStreamingSettingsFileForQualityLevel(QualitySettings.GetQualityLevel()));
		settings = JsonUtility.FromJson<Settings>(json);
		UnityEngine.Debug.Log("Read streaming settings:\n" + settings);
	}

	public void ReloadSettings()
	{
		LoadSettings();
		if ((bool)streamerV2)
		{
			streamerV2.ReloadSettings(cellManager);
		}
	}

	public bool IsWorldSettled()
	{
		if (inited && isIdle && (bool)streamerV2 && streamerV2.IsIdle() && cellManager != null)
		{
			return cellManager.IsIdle();
		}
		return false;
	}

	public Result Initialize(WorldStreamer _streamerV2, Voxeland _land, string _pathPrefix)
	{
		if (inited)
		{
			UnityEngine.Debug.Log("LargeWorldStreamer::Initialize - returning, already init'd frame " + Time.frameCount);
			return Result.Failure("StreamerAlreadyInitialized");
		}
		if (!CacheExists(_pathPrefix))
		{
			UnityEngine.Debug.LogError("streamable VL octree cache at prefix = " + _pathPrefix + " DNE!");
			return Result.Failure("MissingOctreeCache");
		}
		UnityEngine.Debug.Log("LargeWorldStreamer::Initialize frame " + Time.frameCount);
		LoadSettings();
		tmpPathPrefix = _pathPrefix;
		tmpPathPrefix = SaveLoadManager.GetTemporarySavePath();
		batchesRoot = null;
		cellsRoot = null;
		waitersRoot = null;
		SetPathPrefix(_pathPrefix);
		streamerV2 = _streamerV2;
		land = _land;
		data = _land.data;
		world = GetComponent<LargeWorld>();
		LargeWorldEntitySpawner component = GetComponent<LargeWorldEntitySpawner>();
		cellManager = new CellManager(this, component);
		maxInstanceLayer = 0;
		minInstanceLayer = 0;
		int chosenFile;
		StreamReader streamReader = UWE.Utils.OpenEitherText(out chosenFile, Platform.IO.Path.Combine(_pathPrefix, "index.txt"));
		int.Parse(streamReader.ReadLine());
		Int3 @int = Int3.ParseLine(streamReader);
		nodeCount = Int3.ParseLine(streamReader);
		int newMaxNodeSize = int.Parse(streamReader.ReadLine());
		data.ClearToNothing(@int.x, @int.y, @int.z, newMaxNodeSize, clear: false);
		compiledVoxels = ScriptableObject.CreateInstance<VoxelandData>();
		compiledVoxels.name = "Compiled Voxel Temp Data";
		compiledVoxels.ClearToNothing(@int.x, @int.y, @int.z, newMaxNodeSize, clear: false);
		bakedVoxels = ScriptableObject.CreateInstance<VoxelandData>();
		bakedVoxels.name = "Height-baked Voxel Data";
		bakedVoxels.ClearToNothing(@int.x, @int.y, @int.z, newMaxNodeSize, clear: false);
		Int3 int2 = @int / 32;
		compactTrees = new Array3<CompactOctree>(int2.x, int2.y, int2.z);
		treesPerBatch = Int3.ParseLine(streamReader);
		batchSizeMBs = new Array3<float>(batchCount.x, batchCount.y, batchCount.z);
		batchOctreesCached = new Array3<bool>(batchCount.x, batchCount.y, batchCount.z);
		UnityEngine.Debug.Log(string.Concat("LargeWorldStreamer allocated space for ", batchCount, " octree batches"));
		foreach (Int3 item in Int3.Range(batchCount))
		{
			float value = float.Parse(streamReader.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture);
			batchSizeMBs.Set(item, value);
		}
		streamReader.Close();
		proxy = new RasterProxy(this);
		_land.overrideRasterizer = proxy;
		_land.faceCreator = new CppVoxelandFaceScanner();
		UnityEngine.Debug.Log("LargeWorldStreamer is disabling grass! Either override disable is on, or this is a standalone build");
		_land.disableGrass = true;
		_land.eventHandler = this;
		if (!AtmosphereDirector.ShadowsEnabled())
		{
			_land.castShadows = false;
			_land.skipHiRes = true;
			_land.chunkSize = 32;
		}
		else
		{
			_land.castShadows = true;
			_land.skipHiRes = false;
			_land.chunkSize = 16;
		}
		if (!settings.overrideDebugSkipEntities)
		{
			ProfilingTimer.Begin("Initialize prefab database (WorldEntities)");
			UnityEngine.Debug.Log("Calling LoadPrefabDatabase frame " + Time.frameCount);
			PrefabDatabase.LoadPrefabDatabase(SNUtils.prefabDatabaseFilename);
			ProfilingTimer.End();
			if (settings.warmupAllShaders)
			{
				ProfilingTimer.Begin("Warm All Shaders");
				Shader.WarmupAllShaders();
				ProfilingTimer.End();
			}
		}
		ProfilingTimer.Begin("SaveLoadManager.Initialize");
		SaveLoadManager.main.InitializeNewGame();
		ProfilingTimer.End();
		isIdle = false;
		ProfilingTimer.Begin("Create worker thread");
		workerThread = ThreadUtils.StartWorkerThread("I/O", "LargeWorldStreamerThread", System.Threading.ThreadPriority.BelowNormal, -2, 128);
		ProfilingTimer.End();
		inited = true;
		return Result.Success();
	}

	public void ForceUnloadAll()
	{
		foreach (Int3 item in new HashSet<Int3>(loadedBatches, Int3.equalityComparer))
		{
			UnloadBatch(item);
		}
	}

	public void Deinitialize()
	{
		UnityEngine.Debug.Log("LargeWorldStreamer::Deinitialize called, frame " + Time.frameCount);
		if (workerThread != null)
		{
			workerThread.Stop();
			workerThread = null;
		}
		if (cellManager != null)
		{
			cellManager.RequestAbort();
		}
		ForceUnloadAll();
		UnloadGlobalRoot();
		if (land != null && land.data != null)
		{
			land.overrideRasterizer = null;
		}
		inited = false;
		data = null;
		land = null;
		if (compiledVoxels != null)
		{
			UnityEngine.Object.DestroyImmediate(compiledVoxels);
		}
		if (bakedVoxels != null)
		{
			UnityEngine.Object.DestroyImmediate(bakedVoxels);
		}
		compactTrees = null;
		streamerV2 = null;
	}

	public bool CheckRoot(Int3 globalRid)
	{
		return CheckRoot(globalRid.x, globalRid.y, globalRid.z);
	}

	private bool CheckRoot(int rx, int ry, int rz)
	{
		if (rx < data.nodesX && ry < data.nodesY && rz < data.nodesZ && rx >= 0 && ry >= 0)
		{
			return rz >= 0;
		}
		return false;
	}

	public bool CheckBatch(Int3 batch)
	{
		if (batch >= Int3.zero)
		{
			return batch < batchCount;
		}
		return false;
	}

	public Vector3 GetBatchCenter(Int3 batchIndex)
	{
		Vector3 position = Vector3.Scale(batchIndex.ToVector3() + UWE.Utils.half3, blocksPerBatch.ToVector3());
		return land.transform.TransformPoint(position);
	}

	public Vector3 GetBatchMins(Int3 batchIndex)
	{
		Vector3 position = (batchIndex * blocksPerBatch).ToVector3();
		return land.transform.TransformPoint(position);
	}

	public Vector3 GetBatchMaxs(Int3 batchIndex)
	{
		Vector3 position = ((batchIndex + 1) * blocksPerBatch).ToVector3();
		return land.transform.TransformPoint(position);
	}

	private float GetSquaredDistanceToBatch(Vector3 p, Int3 batch)
	{
		return UWE.Utils.GetPointToBoxDistanceSquared(p, GetBatchMins(batch), GetBatchMaxs(batch));
	}

	public Int3 GetBatchOriginBlock(Int3 batchIndex)
	{
		return new Int3(batchIndex.x * treesPerBatch.x * data.biggestNode, batchIndex.y * treesPerBatch.y * data.biggestNode, batchIndex.z * treesPerBatch.z * data.biggestNode);
	}

	public Int3 GetContainingBatch(Vector3 wsPos)
	{
		if (!inited)
		{
			return new Int3(-1);
		}
		return Int3.Floor(land.transform.InverseTransformPoint(wsPos)) / blocksPerBatch;
	}

	private IEnumerator Start()
	{
		while (!inited)
		{
			yield return CoroutineUtils.waitForNextFrame;
		}
		while (!land)
		{
			yield return CoroutineUtils.waitForNextFrame;
		}
		PooledStateMachine<CoroutineUtils.PumpCoroutineStateMachine> pooledStateMachine = CoroutineUtils.PumpCoroutine(LoopUpdateBatchStreamingAsync(), "UpdateBatchStreamingFSM", 1f);
		PooledStateMachine<CoroutineUtils.PumpCoroutineStateMachine> pooledStateMachine2 = CoroutineUtils.PumpCoroutine(LoopUpdateCellStreamingAsync(), "UpdateCellStreamingFSM", 1f);
		PooledStateMachine<CoroutineUtils.PumpCoroutineStateMachine> updateCellManagementPriorities = CoroutineUtils.PumpCoroutine(LoopUpdateCellManagementPrioritiesAsync(), "UpdateCellManagementQueue", 1f);
		PooledStateMachine<CoroutineUtils.PumpCoroutineStateMachine> update1 = pooledStateMachine;
		PooledStateMachine<CoroutineUtils.PumpCoroutineStateMachine> update2 = pooledStateMachine2;
		updateCellManagementPriorities.stateMachine.SetMaxFrameMs(0.5f);
		Stopwatch watch = new Stopwatch();
		while (true)
		{
			Transform transform = MainCamera.camera.transform;
			cachedCameraPosition = transform.position;
			cachedCameraForward = transform.forward;
			cachedTime = Time.realtimeSinceStartup;
			streamerV2.UpdateStreamingCenter(cachedCameraPosition);
			while (MainGameController.Instance.HasGarbageCollectedThisFrame())
			{
				yield return CoroutineUtils.waitForNextFrame;
			}
			try
			{
				updateCellManagementPriorities.MoveNext();
				float num = settings.GetMaxFrameMs(Player.main);
				PooledStateMachine<CoroutineUtils.PumpCoroutineStateMachine> pooledStateMachine3 = update2;
				PooledStateMachine<CoroutineUtils.PumpCoroutineStateMachine> pooledStateMachine4 = update1;
				update1 = pooledStateMachine3;
				update2 = pooledStateMachine4;
				float maxFrameMs = Mathf.Max(1f, num * 0.7f);
				update1.stateMachine.SetMaxFrameMs(maxFrameMs);
				watch.Restart();
				update1.MoveNext();
				watch.Stop();
				float timeElapsedMS = UWE.Utils.GetTimeElapsedMS(watch);
				float num2 = num - timeElapsedMS;
				float maxFrameMs2 = Mathf.Max(1f, num2 * 0.5f);
				update2.stateMachine.SetMaxFrameMs(maxFrameMs2);
				watch.Restart();
				update2.MoveNext();
				watch.Stop();
			}
			finally
			{
			}
			bool wait = true;
			if (update1.Current is YieldInstruction)
			{
				wait = false;
				yield return update1.Current;
			}
			if (update2.Current is YieldInstruction)
			{
				wait = false;
				yield return update2.Current;
			}
			if (wait)
			{
				yield return CoroutineUtils.waitForNextFrame;
			}
		}
	}

	private IEnumerator LoopUpdateBatchStreamingAsync()
	{
		while (true)
		{
			yield return UpdateBatchStreaming();
			yield return CoroutineUtils.waitForNextFrame;
		}
	}

	private IEnumerator LoopUpdateCellStreamingAsync()
	{
		ProtobufSerializer serializer = new ProtobufSerializer();
		while (true)
		{
			yield return UpdateCellStreaming(serializer);
			yield return CoroutineUtils.waitForNextFrame;
		}
	}

	private IEnumerator LoopUpdateCellManagementPrioritiesAsync()
	{
		while (true)
		{
			yield return cellManager.cellManagementQueue.UpdateHeap();
			yield return CoroutineUtils.waitForNextFrame;
		}
	}

	private IEnumerator UpdateBatchStreaming()
	{
		return updateBatchStreamingCoroutinePool.Get(this);
	}

	public IEnumerator LoadBatchTaskedAsync(BatchCells batchCells, bool _editMode)
	{
		LoadBatchTask loadBatchTask = new LoadBatchTask(this, batchCells, _editMode);
		UWE.Utils.EnqueueWrap(workerThread, loadBatchTask);
		return new AsyncAwaiter(loadBatchTask);
	}

	private Int3.Bounds GetEffectiveBounds(Int3 camBatch)
	{
		return new Int3.Bounds(camBatch - batchLoadRings, camBatch + batchLoadRings);
	}

	private bool TryGetBestBatch(Vector3 camPos, Int3.Bounds effectiveBounds, out Int3 best)
	{
		best = Int3.zero;
		float num = float.MaxValue;
		bool result = false;
		foreach (Int3 item in effectiveBounds)
		{
			if (CheckBatch(item) && !loadedBatches.Contains(item))
			{
				float squaredDistanceToBatch = GetSquaredDistanceToBatch(camPos, item);
				if (squaredDistanceToBatch < num)
				{
					best = item;
					num = squaredDistanceToBatch;
					result = true;
				}
			}
		}
		return result;
	}

	private bool TryGetWorstBatch(Vector3 camPos, Int3.Bounds effectiveBounds, out Int3 worst)
	{
		worst = Int3.zero;
		float num = float.MinValue;
		bool result = false;
		foreach (Int3 loadedBatch in loadedBatches)
		{
			if (!effectiveBounds.Contains(loadedBatch))
			{
				float squaredDistanceToBatch = GetSquaredDistanceToBatch(camPos, loadedBatch);
				if (squaredDistanceToBatch > num)
				{
					worst = loadedBatch;
					num = squaredDistanceToBatch;
					result = true;
				}
			}
		}
		return result;
	}

	private IEnumerator UpdateCellStreaming(ProtobufSerializer serializer)
	{
		if (frozen)
		{
			return null;
		}
		if (cellManager.IsIdle())
		{
			return null;
		}
		return cellManager.UpdateCellManagement(serializer);
	}

	private void OnDestroy()
	{
		Deinitialize();
	}

	private bool TryUnloadBatch(Int3 batch)
	{
		if (SaveLoadManager.main != null && !SaveLoadManager.main.GetAllowWritingFiles())
		{
			return false;
		}
		_ = StopwatchProfiler.Instance;
		try
		{
			if (cellManager != null && cellManager.IsProcessingBatchCells(batch))
			{
				return false;
			}
		}
		finally
		{
		}
		return true;
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	private static string GetPathIndexString(Int3 index)
	{
		if (index == cachedRequestedIndex)
		{
			return cachedPathIndexString;
		}
		cachedPathIndexString = $"{index.x}-{index.y}-{index.z}";
		cachedRequestedIndex = index;
		return cachedPathIndexString;
	}

	public static string GetCompiledOctreesCacheFilename(Int3 index)
	{
		return $"compiled-batch-{GetPathIndexString(index)}.optoctrees";
	}

	public static string GetCompiledOctreesCachePath(string prefix, string filename)
	{
		if (!combinedOctreeCachePrefix.TryGetValue(prefix, out var value))
		{
			value = Platform.IO.Path.Combine(prefix, "CompiledOctreesCache");
			combinedOctreeCachePrefix.Add(prefix, value);
		}
		return Platform.IO.Path.Combine(value, filename);
	}

	public static string GetBatchObjectsFilename(Int3 index)
	{
		return $"batch-objects-{GetPathIndexString(index)}.bin";
	}

	public static string GetCacheBatchObjectsPath(string prefix, string filename)
	{
		return Platform.IO.Path.Combine(Platform.IO.Path.Combine(prefix, "BatchObjectsCache"), filename);
	}

	private static string GetGlobalRootPath(string prefix)
	{
		return Platform.IO.Path.Combine(prefix, "global-objects.bin");
	}

	private static string GetSceneObjectsPath(string prefix)
	{
		return Platform.IO.Path.Combine(prefix, "scene-objects.bin");
	}

	private static bool CacheExists(string pathPrefix)
	{
		return FileUtils.FileExists(Platform.IO.Path.Combine(pathPrefix, "index.txt"));
	}

	private void UnloadBatchOctrees(Int3 index)
	{
	}

	private void UnloadBatch(Int3 index)
	{
		UnloadBatchOctrees(index);
		UnloadBatchObjects(index);
		cellManager.UnloadBatchCells(index);
		loadedBatches.Remove(index);
	}

	public void SaveSceneObjectsIntoCurrentSlot()
	{
		using (Stream stream = FileUtils.CreateFile(GetSceneObjectsPath(tmpPathPrefix)))
		{
			SceneObjectManager.Instance.Save(stream);
		}
	}

	public IEnumerator LoadSceneObjectsAsync()
	{
		SceneObjectManager manager = SceneObjectManager.Instance;
		string sceneObjectsPath = GetSceneObjectsPath(tmpPathPrefix);
		if (FileUtils.FileExists(sceneObjectsPath))
		{
			using (Stream stream = FileUtils.ReadFile(sceneObjectsPath))
			{
				TaskResult<Exception> exceptionResult = new TaskResult<Exception>();
				yield return CoroutineUtils.YieldSafe(manager.LoadAsync(stream), exceptionResult);
				Exception ex = exceptionResult.Get();
				if (ex != null)
				{
					UnityEngine.Debug.LogException(ex);
					yield break;
				}
			}
		}
		manager.OnLoaded();
	}

	public void SaveGlobalRootIntoCurrentSlot()
	{
		using (PooledObject<ProtobufSerializer> pooledObject = ProtobufSerializerPool.GetProxy())
		{
			string globalRootPath = GetGlobalRootPath(tmpPathPrefix);
			pooledObject.Value.SaveObjectTreeToFile(globalRootPath, globalRoot);
		}
	}

	private void UnloadGlobalRoot()
	{
		UnityEngine.Object.DestroyImmediate(globalRoot);
		globalRoot = null;
	}

	public IEnumerator LoadGlobalRootAsync()
	{
		UnloadGlobalRoot();
		string globalRootPath = GetGlobalRootPath(tmpPathPrefix);
		string globalRootPath2 = GetGlobalRootPath(pathPrefix);
		GameObject gameObject = null;
		int chosenFile;
		using (Stream stream = UWE.Utils.TryOpenEither(out chosenFile, globalRootPath, globalRootPath2))
		{
			bool allowSpawnRestrictions = chosenFile == 1;
			if (stream != null)
			{
				using (PooledObject<ProtobufSerializer> serializerProxy = ProtobufSerializerPool.GetProxy())
				{
					ProtobufSerializer value = serializerProxy.Value;
					if (value.TryDeserializeStreamHeader(stream))
					{
						CoroutineTask<GameObject> task = value.DeserializeObjectTreeAsync(stream, forceInactiveRoot: true, allowSpawnRestrictions, 0);
						yield return task;
						gameObject = task.GetResult();
					}
				}
			}
		}
		if (!gameObject)
		{
			gameObject = new GameObject("Global Root");
			gameObject.AddComponent<StoreInformationIdentifier>();
		}
		OnGlobalRootLoaded(gameObject);
	}

	private void OnGlobalRootLoaded(GameObject root)
	{
		root.transform.parent = base.transform.parent;
		globalRoot = root;
		globalRoot.SetActive(value: true);
		globalRoot.BroadcastMessage("OnGlobalEntitiesLoaded", SendMessageOptions.DontRequireReceiver);
	}

	public void MakeEntityTransient(GameObject entity)
	{
		entity.transform.parent = transientRoot.transform;
		cellManager.UnregisterEntity(entity);
	}

	public bool IsTransientEntity(GameObject entity)
	{
		return entity.transform.parent == transientRoot.transform;
	}

	public IEnumerator LoadBatchAsync(Int3 index)
	{
		using (new EditModeScopeTimer("LoadBatch " + index))
		{
			BatchCells batchCells = cellManager.InitializeBatchCells(index);
			yield return LoadBatchThreadedAsync(batchCells, _editMode: false);
			FinalizeLoadBatch(index);
		}
	}

	private IEnumerator LoadBatchThreadedAsync(BatchCells batchCells, bool _editMode)
	{
		Int3 index = batchCells.batch;
		if (CheckBatch(index))
		{
			_ = StopwatchProfiler.Instance;
			string compiledOctreesCacheFilename = GetCompiledOctreesCacheFilename(index);
			string compiledOctreesCachePath = GetCompiledOctreesCachePath(pathPrefix, compiledOctreesCacheFilename);
			if (FileUtils.FileExists(compiledOctreesCachePath))
			{
			}
			OnLoadActivity(this, null);
			yield return cellManager.LoadBatchCellsThreadedAsync(batchCells, _editMode);
			if ((bool)streamerV2 && streamerV2.clipmapStreamer != null)
			{
				streamerV2.clipmapStreamer.NotifyListeners(GetBatchBlockBounds(index));
			}
			OnLoadActivity(this, null);
			LoadBatchObjectsThreaded(index, _editMode);
		}
	}

	private void FinalizeLoadBatch(Int3 index)
	{
		CoroutineUtils.PumpCoroutine(FinalizeLoadBatchAsync(index, _editMode: false));
	}

	public IEnumerator FinalizeLoadBatchAsync(Int3 index, bool _editMode)
	{
		_ = StopwatchProfiler.Instance;
		LargeWorldStreamer.onLoadActivity?.Invoke(this, null);
		FinalizeLoadCompiledOctrees();
		Int3 @int = index * treesPerBatch * data.biggestNode;
		Int3 int2 = (index + 1) * treesPerBatch * data.biggestNode - 1;
		land.DestroyRelevantChunks(@int.x, @int.y, @int.z, int2.x, int2.y, int2.z);
		return FinalizeLoadBatchObjectsAsync(index, reloadIfExists: true);
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	[SuppressMessage("Gendarme.Rules.Performance", "AvoidConcatenatingCharsRule")]
	[SuppressMessage("Subnautica.Rules", "AvoidStringConcatenation")]
	private void LoadCompiledOctreesThreaded(Int3 index, bool _editMode, string cachePath)
	{
		bool value = false;
		if (!_editMode && !string.IsNullOrEmpty(cachePath))
		{
			using (Stream stream = FileUtils.ReadFile(cachePath))
			{
				using (PooledBinaryReader pooledBinaryReader = new PooledBinaryReader(stream))
				{
					int num = pooledBinaryReader.ReadInt32();
					if (num <= 3)
					{
						UnityEngine.Debug.LogError(string.Concat("Old version of cached octrees found for batch ", index, ", file = ", cachePath));
					}
					else
					{
						ReadCompiledOctrees(pooledBinaryReader, num, index);
					}
				}
			}
			value = true;
		}
		batchOctreesCached.Set(index, value);
	}

	private void OnBatchObjectsLoaded(Int3 batch, GameObject rootObject)
	{
		if (rootObject != null)
		{
			rootObject.name = string.Concat("Batch ", batch, " objects");
			batch2root[batch] = EnsureBatchRootSetup(rootObject, batch);
			rootObject.transform.position = land.transform.TransformPoint((batch * blocksPerBatch).ToVector3());
			Light[] componentsInChildren = rootObject.GetComponentsInChildren<Light>();
			foreach (Light light in componentsInChildren)
			{
				if (light.type == LightType.Directional && light.name.IndexOf("bounce", StringComparison.OrdinalIgnoreCase) > 0)
				{
					light.intensity = 0f;
					DayNightLight component = light.gameObject.GetComponent<DayNightLight>();
					if (component != null)
					{
						component.enabled = false;
					}
					UnityEngine.Object.Destroy(light.gameObject);
				}
			}
		}
		OnBatchFullyLoaded(batch);
	}

	public bool IsBatchReadyToCompile(Int3 checkBatch)
	{
		if (CheckBatch(checkBatch))
		{
			return loadedBatches.Contains(checkBatch);
		}
		return false;
	}

	private void OnBatchFullyLoaded(Int3 batchId)
	{
		loadedBatches.Add(batchId);
	}

	private LargeWorldBatchRoot EnsureBatchRootSetup(GameObject root, Int3 batch)
	{
		LargeWorldBatchRoot component = root.GetComponent<LargeWorldBatchRoot>();
		component.streamer = this;
		component.batchId = batch;
		root.transform.SetParent(batchesRoot, worldPositionStays: false);
		return component;
	}

	private static IAlloc<byte> ReadAllBytesPooled(string path)
	{
		using (FileStream fileStream = Platform.IO.File.OpenRead(path))
		{
			int num = 0;
			int num2 = (int)fileStream.Length;
			IAlloc<byte> alloc = CommonByteArrayAllocator.Allocate(num2);
			while (num2 > 0)
			{
				int num3 = fileStream.Read(alloc.Array, alloc.Offset + num, num2);
				if (num3 == 0)
				{
					break;
				}
				num += num3;
				num2 -= num3;
			}
			return alloc;
		}
	}

	private void LoadBatchObjectsThreaded(Int3 index, bool _editMode)
	{
		if (batchObjectsBuffer != null && batchObjectsBuffer.Length == 0)
		{
			CommonByteArrayAllocator.Free(batchObjectsBuffer);
			batchObjectsBuffer = null;
		}
		if (batchObjectsBuffer != null && batchObjectsBuffer.Length == 0)
		{
			batchObjectsBuffer = null;
		}
		string batchObjectsFilename = GetBatchObjectsFilename(index);
		string cacheBatchObjectsPath = GetCacheBatchObjectsPath(pathPrefix, batchObjectsFilename);
		try
		{
			if (FileUtils.FileExists(cacheBatchObjectsPath))
			{
				batchObjectsBuffer = ReadAllBytesPooled(cacheBatchObjectsPath);
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogErrorFormat(this, "Exception while loading batch {0}: {1}", index, ex);
			UnityEngine.Debug.LogException(ex, this);
		}
	}

	private IEnumerator FinalizeLoadBatchObjectsAsync(Int3 index, bool reloadIfExists)
	{
		_ = StopwatchProfiler.Instance;
		if (reloadIfExists && batch2root.ContainsKey(index))
		{
			UnloadBatchObjects(index);
		}
		if (batchObjectsBuffer != null)
		{
			GameObject rootObject = null;
			using (MemoryStream stream = new MemoryStream(batchObjectsBuffer.Array, batchObjectsBuffer.Offset, batchObjectsBuffer.Length, writable: false))
			{
				using (PooledObject<ProtobufSerializer> serializerProxy = ProtobufSerializerPool.GetProxy())
				{
					ProtobufSerializer value = serializerProxy.Value;
					if (value.TryDeserializeStreamHeader(stream))
					{
						CoroutineTask<GameObject> task = value.DeserializeObjectTreeAsync(stream, forceInactiveRoot: false, allowSpawnRestrictions: false, 0);
						yield return task;
						rootObject = task.GetResult();
					}
				}
			}
			OnBatchObjectsLoaded(index, rootObject);
		}
		else
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(batchRootPrefab);
			batch2root[index] = EnsureBatchRootSetup(gameObject, index);
			gameObject.transform.position = land.transform.TransformPoint((index * blocksPerBatch).ToVector3());
			OnBatchFullyLoaded(index);
		}
		if (batchObjectsBuffer != null)
		{
			CommonByteArrayAllocator.Free(batchObjectsBuffer);
			batchObjectsBuffer = null;
		}
	}

	public void UnloadBatchObjects(Int3 index)
	{
		if (batch2root.TryGetValue(index, out var value))
		{
			if (value != null)
			{
				UWE.Utils.DestroyWrap(value.gameObject);
			}
			batch2root.Remove(index);
		}
	}

	private Int3.Bounds GetBatchBlockBounds(Int3 index)
	{
		return new Int3.Bounds(index * blocksPerBatch, (index + 1) * blocksPerBatch - 1);
	}

	public void LayoutDebugGUI()
	{
		GUILayout.BeginVertical("box");
		GUILayout.Label("-- LargeWorldStreamer --");
		if ((bool)land && land.overrideRasterizer != null)
		{
			land.overrideRasterizer.LayoutDebugGUI();
		}
		frozen = GUILayout.Toggle(frozen, "Freeze Streaming");
		settings.batchLoadRings = UWE.Utils.GUI.LayoutIntField(batchLoadRings, "Batch Load Rings");
		if (GUILayout.Button("Reload settings"))
		{
			ReloadSettings();
		}
		GUILayout.BeginHorizontal();
		for (int i = 0; i < QualitySettings.names.Length; i++)
		{
			if (GUILayout.Button(QualitySettings.names[i]))
			{
				QualitySettings.SetQualityLevel(i, applyExpensiveChanges: true);
				ReloadSettings();
			}
		}
		GUILayout.EndHorizontal();
		if (IsReady())
		{
			GUILayout.Label("Active prefix: " + pathPrefix);
			GUILayout.Label("Loaded " + loadedBatches.Count + " / " + batchCount.Product() + " batches");
			GUILayout.Label("Prefab cache size: " + PrefabDatabase.GetCacheSize());
			GUILayout.Label("Idle: " + isIdle);
			if ((bool)streamerV2 && streamerV2.clipmapStreamer != null)
			{
				streamerV2.clipmapStreamer.OnGUI();
			}
			GUILayout.Label("Cell queue length: " + cellManager.GetQueueLength());
			Int3 containingBatch = GetContainingBatch(MainCamera.camera.transform.position);
			GUILayout.Label("Batch " + containingBatch);
			GUILayout.Label("- loaded? " + loadedBatches.Contains(containingBatch));
			GUILayout.Label("- ready to compile? " + IsBatchReadyToCompile(containingBatch));
		}
		else
		{
			GUILayout.Label("No world mounted");
		}
		if (land != null)
		{
			land.freeze = GUILayout.Toggle(land.freeze, "Freeze VL");
		}
		GUILayout.EndVertical();
	}

	public static void UpgradeAmbientSettings()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		GameObject[] array = UnityEngine.Object.FindObjectsOfType<GameObject>();
		foreach (GameObject gameObject in array)
		{
			if (!(gameObject.name == "Ambient Settings") && !(gameObject.name == "Additional Settings"))
			{
				continue;
			}
			AmbientSettings component = gameObject.GetComponent<AmbientSettings>();
			if ((bool)component && !component.ambientLight.ApproxEquals(AmbientLightSettings.defaultColor, 0.003921569f))
			{
				num2++;
				Transform parent = gameObject.transform.parent;
				if ((bool)parent)
				{
					AtmosphereVolume component2 = parent.GetComponent<AtmosphereVolume>();
					if ((bool)component2)
					{
						component2.amb = component2.amb ?? new AmbientLightSettings();
						component2.amb.enabled = true;
						component2.amb.dayNightColor = UWE.Utils.DayNightGradient(component.ambientLight);
						num3++;
					}
					LargeWorldBatchRoot component3 = parent.GetComponent<LargeWorldBatchRoot>();
					if ((bool)component3)
					{
						component3.amb = component3.amb ?? new AmbientLightSettings();
						component3.amb.enabled = true;
						component3.amb.dayNightColor = UWE.Utils.DayNightGradient(component.ambientLight);
						num4++;
					}
				}
			}
			UnityEngine.Object.DestroyImmediate(gameObject);
			num++;
		}
		if (num == 0)
		{
			UnityEngine.Debug.Log("All good. No obsolete ambient settings found");
			return;
		}
		UnityEngine.Debug.LogWarning("Killed " + num + " obsolete objects. " + num2 + " had data that got upgraded to " + num3 + " atmosphere volumes and " + num4 + " batch roots");
	}

	public void OnChunkBuilt(Voxeland _land, int cx, int cy, int cz)
	{
	}

	public void OnChunkHighLOD(Voxeland _land, int cx, int cy, int cz)
	{
		Int3.Bounds blockRange = Int3.Bounds.FinerBounds(new Int3(cx, cy, cz), new Int3(_land.chunkSize));
		cellManager.ShowEntities(blockRange);
	}

	public void OnChunkLowLOD(Voxeland _land, int cx, int cy, int cz)
	{
		OnChunkDestroyedOrLowLOD(_land, cx, cy, cz);
	}

	public void OnChunkDestroyed(Voxeland _land, int cx, int cy, int cz)
	{
		OnChunkDestroyedOrLowLOD(_land, cx, cy, cz);
	}

	private void OnChunkDestroyedOrLowLOD(Voxeland _land, int cx, int cy, int cz)
	{
		if (IsReady())
		{
			Int3.Bounds blockRange = Int3.Bounds.FinerBounds(new Int3(cx, cy, cz), new Int3(_land.chunkSize));
			cellManager.HideEntities(blockRange);
		}
	}

	public Int3 GetBlock(Vector3 wsPos)
	{
		return Int3.Floor(land.transform.InverseTransformPoint(wsPos));
	}

	public string GetOverrideBiome(Int3 block)
	{
		string result = null;
		Int3 key = block / blocksPerBatch;
		if (batch2root.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value.overrideBiome))
		{
			result = value.overrideBiome;
		}
		return result;
	}

	public bool OctreeRaycast(Vector3 startPoint, Vector3 endPoint, out Int3 hit)
	{
		using (new ProfilingUtils.Sample("OctreeRaycast"))
		{
			int num = blocksPerTree;
			int num2 = (int)(Vector3.Distance(startPoint, endPoint) / (float)num * 3f);
			Int3 block = GetBlock(startPoint);
			Int3 block2 = GetBlock(endPoint);
			for (int i = 0; i <= num2; i++)
			{
				Int3 @int = Int3.Lerp(block, block2, 0, num2, i);
				Int3 int2 = @int / num;
				if (!CheckRoot(int2))
				{
					continue;
				}
				if ((bool)streamerV2)
				{
					BatchOctreesStreamer octreesStreamer = streamerV2.octreesStreamer;
					if (octreesStreamer == null)
					{
						hit = Int3.zero;
						return false;
					}
					Octree octree = octreesStreamer.GetOctree(int2);
					if (octree != null && !octree.IsEmpty())
					{
						hit = @int;
						return true;
					}
				}
				else
				{
					CompactOctree compactOctree = compactTrees.Get(int2);
					if (compactOctree != null && !compactOctree.IsEmpty())
					{
						hit = @int;
						return true;
					}
				}
			}
			hit = Int3.zero;
			return false;
		}
	}

	public IEnumerable<Int3> LoadedBatches()
	{
		return loadedBatches;
	}

	public byte GetBlockType(Vector3 wsPos)
	{
		Int3 block = GetBlock(wsPos);
		Int3 @int = block / blocksPerTree;
		Int3 int2 = block % blocksPerTree;
		if (!CheckRoot(@int))
		{
			return 0;
		}
		if ((bool)streamerV2)
		{
			return streamerV2.octreesStreamer?.GetBlockType(block) ?? 0;
		}
		CompactOctree compactOctree = compactTrees.Get(@int);
		if (compactOctree == null)
		{
			UnityEngine.Debug.LogWarningFormat(this, "Missing octree in LargeWorldStreamer.GetBlockType({0}) for (block {1}, root {2}, coords {3})", wsPos, block, @int, int2);
			return 0;
		}
		int nodeId = compactOctree.GetNodeId(int2, blocksPerTree);
		return compactOctree.GetType(nodeId);
	}

	private void PerformVoxelEdit(Bounds wsBounds, DistanceField df, bool isAdd = false, byte type = 1)
	{
		Int3 mins = Int3.Floor(land.transform.InverseTransformPoint(wsBounds.min));
		Int3 maxs = Int3.Floor(land.transform.InverseTransformPoint(wsBounds.max));
		PerformVoxelEdit(new Int3.Bounds(mins, maxs), df, isAdd, type);
	}

	private void PerformVoxelEdit(Int3.Bounds blockBounds, DistanceField df, bool isAdd = false, byte type = 1)
	{
		VoxelandData.OctNode.BlendArgs args = new VoxelandData.OctNode.BlendArgs((!isAdd) ? VoxelandData.OctNode.BlendOp.Subtraction : VoxelandData.OctNode.BlendOp.Union, replaceTypes: false, (byte)(isAdd ? type : 0));
		blockBounds = blockBounds.Expanded(1);
		foreach (Int3 item in blockBounds / blocksPerTree)
		{
			if (!CheckRoot(item))
			{
				continue;
			}
			CompactOctree compactOctree = compactTrees.Get(item);
			if (compactOctree == null)
			{
				continue;
			}
			Int3.Bounds bounds = item.Refined(blocksPerTree);
			VoxelandData.OctNode root = compactOctree.ToVLOctree();
			foreach (Int3 item2 in bounds.Intersect(blockBounds))
			{
				Vector3 wsPos = land.transform.TransformPoint(item2 + UWE.Utils.half3);
				float num = df(wsPos);
				VoxelandData.OctNode n = new VoxelandData.OctNode((byte)((num >= 0f) ? type : 0), VoxelandData.OctNode.EncodeDensity(num));
				int num2 = blocksPerTree;
				int x = item2.x % num2;
				int y = item2.y % num2;
				int z = item2.z % num2;
				VoxelandData.OctNode octNode = VoxelandData.OctNode.Blend(root.GetNode(x, y, z, num2 / 2), n, args);
				root.SetNode(x, y, z, num2 / 2, octNode.type, octNode.density);
			}
			root.Collapse();
			compactTrees.Set(item, CompactOctree.CreateFrom(root));
			root.Clear();
		}
	}

	public void PerformSphereEdit(Vector3 center, float radius, bool isAdd = false, byte type = 1)
	{
		Vector3 size = 2f * new Vector3(radius, radius, radius);
		Bounds wsBounds = new Bounds(center, size);
		PerformVoxelEdit(wsBounds, (Vector3 wsPos) => radius - Vector3.Distance(wsPos, center), isAdd, type);
	}

	public void PerformBoxEdit(Bounds bb, Quaternion rot, bool isAdd = false, byte type = 1)
	{
		Bounds wsBounds = bb;
		wsBounds.Expand(new Vector3(1f, 1f, 1f));
		Quaternion invRot = Quaternion.Inverse(rot);
		Vector3 c = bb.center;
		PerformVoxelEdit(wsBounds, (Vector3 wsPos) => VoxelandMisc.SignedDistToBox(bb, c + invRot * (wsPos - c)), isAdd, type);
	}

	public bool GetDisableFarColliders()
	{
		return settings.disableFarColliders;
	}

	public bool IsRangeActiveAndBuilt(Bounds bb)
	{
		if (!streamerV2 || streamerV2.clipmapStreamer == null)
		{
			return false;
		}
		Int3.Bounds blockRange = Int3.MinMax(GetBlock(bb.min), GetBlock(bb.max));
		if (streamerV2.clipmapStreamer.IsRangeActiveAndBuilt(blockRange) && cellManager.AreCellsLoaded(bb, LargeWorldEntity.CellLevel.Medium) && cellManager.AreCellsLoaded(bb, LargeWorldEntity.CellLevel.Far))
		{
			return cellManager.AreCellsLoaded(bb, LargeWorldEntity.CellLevel.VeryFar);
		}
		return false;
	}

	public int EstimateCompactOctreeBytes()
	{
		return CompactOctree.EstimateBytes();
	}

	public long EstimateClipMapManagerBytes()
	{
		return streamerV2 ? streamerV2.EstimateBytes() : (-1);
	}

	public Bounds GetBatchBounds(Int3 batch)
	{
		Int3.Bounds bounds = batch.Refined(blocksPerBatch);
		Bounds result = new Bounds(Vector3.zero, Vector3.zero);
		result.SetMinMax(land.transform.TransformPoint(bounds.mins.ToVector3()), land.transform.TransformPoint((bounds.maxs + 1).ToVector3()));
		return result;
	}

	private static void OnLoadActivity(object sender, EventArgs args)
	{
		LargeWorldStreamer.onLoadActivity?.Invoke(sender, args);
	}
}
