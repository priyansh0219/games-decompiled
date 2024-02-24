using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Gendarme;
using Platform.Utils;
using UWE;
using UnityEngine;
using UnityEngine.Profiling;

public class ClipMapManager : MonoBehaviour
{
	[Serializable]
	public class LevelSettings
	{
		public int chunksPerSide = 7;

		public int chunksVertically = 7;

		public bool colliders;

		public int downsamples;

		public int maxBlockTypes = int.MaxValue;

		public int meshOverlap;

		public int contractEntityChunks;

		public bool grass;

		public bool entities;

		public bool skipRelax;

		public bool debug;

		public bool fadeMeshes;

		public bool ignoreMeshes;

		public bool castShadows;

		public bool highPriority;

		public bool keepGrassVisible;

		public VoxelandVisualMeshSimplifier.Settings visual = new VoxelandVisualMeshSimplifier.Settings();

		public VoxelandGrassBuilder.Settings grassSettings = new VoxelandGrassBuilder.Settings();

		public Int3 GetChunksPerSide()
		{
			return new Int3(chunksPerSide, chunksVertically, chunksPerSide);
		}

		public override string ToString()
		{
			return JsonUtility.ToJson(this, prettyPrint: true);
		}
	}

	[Serializable]
	public class Settings
	{
		public int chunkMeshRes = 16;

		public int maxWorkspaces = 32;

		public int maxMeshQueue = 50;

		public int maxThreads = 16;

		public int threadAffinityMask = -2;

		public bool debugSingleBlockType;

		public int vertexBufferV2Max = 131072;

		public int vertexBufferV3Max = 131072;

		public int vertexBufferV4Max = 131072;

		public int vertexBufferC32Max = 131072;

		public int vertexBufferIndexMax = 524288;

		public VoxelandCollisionMeshSimplifier.Settings collision = new VoxelandCollisionMeshSimplifier.Settings();

		public LevelSettings[] levels;

		public override string ToString()
		{
			return JsonUtility.ToJson(this, prettyPrint: true);
		}
	}

	public interface IClipMapEventHandler
	{
		void ShowEntities(Int3.Bounds blockRange, int level);

		void HideEntities(Int3.Bounds blockRange, int level);
	}

	private class MeshingThread
	{
		public enum State
		{
			None = 0,
			Idle = 1,
			Waiting = 2,
			Busy = 3,
			Dead = 4
		}

		private readonly ClipMapManager mgr;

		private readonly int index;

		private readonly int consoleAffinityMask;

		public int chunksDone;

		public Cell busyWithCell;

		public State state { get; private set; }

		public MeshBufferPools bufferPools { get; set; }

		public VoxelandChunkWorkspace workspace { get; set; }

		public VoxelandVisualMeshSimplifier visSimp { get; set; }

		public VoxelandCollisionMeshSimplifier colSimp { get; set; }

		public VoxelandGrassBuilder grasser { get; set; }

		public MeshingThread(ClipMapManager mgr, int index, int consoleAffinityMask)
		{
			this.mgr = mgr;
			this.index = index;
			this.consoleAffinityMask = consoleAffinityMask;
			state = State.Idle;
		}

		public void Main()
		{
			try
			{
				Platform.Utils.ThreadUtils.SetThreadName($"Meshing thread {index}");
				Platform.Utils.ThreadUtils.SetThreadPriority(System.Threading.ThreadPriority.Lowest);
				Platform.Utils.ThreadUtils.SetThreadAffinityMask(consoleAffinityMask);
				MainLoop();
				Profiler.EndThreadProfiling();
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogException(ex, mgr);
				UnityEngine.Debug.LogErrorFormat("MeshingThread {0} is Dead due to exception: {1}!", index, ex);
			}
			finally
			{
				state = State.Dead;
			}
		}

		private void MainLoop()
		{
			while (true)
			{
				if (mgr.isDestroyed)
				{
					UnityEngine.Debug.LogWarning("ClipMapManager is destroyed, killing meshing thread!");
					return;
				}
				if (mgr.debugKillThreads)
				{
					break;
				}
				if (bufferPools == null || !bufferPools.TryReset() || visSimp == null || visSimp.inUse || colSimp == null || colSimp.inUse || grasser == null || grasser.inUse)
				{
					mgr.meshThreadEvent.WaitOne(1000);
					continue;
				}
				Cell cell = null;
				lock (mgr.toMeshByPriorityLock)
				{
					if (mgr.toMeshByPriority.Count > 0)
					{
						cell = mgr.toMeshByPriority.Dequeue();
					}
				}
				if (cell != null)
				{
					if (!cell.LockCell())
					{
						UnityEngine.Debug.LogErrorFormat("Meshing Thread {0} - Failed to lock cell {1}.", index, cell);
						continue;
					}
					cell.OnDequeue();
					state = State.Busy;
					busyWithCell = cell;
					try
					{
						cell.DoThreadablePart(bufferPools, workspace, visSimp, colSimp, grasser);
						busyWithCell = null;
						chunksDone++;
						state = State.Idle;
						cell.UnlockCell();
						if (!mgr.toFinalize.Push(cell))
						{
							bool flag = false;
							do
							{
								mgr.meshToFinalizeEvent.WaitOne(10);
							}
							while (!mgr.toFinalize.Push(cell));
						}
					}
					catch (Exception exception)
					{
						UnityEngine.Debug.LogException(exception, mgr);
					}
					mgr.meshToMeshEvent.Set();
				}
				else
				{
					mgr.meshThreadEvent.WaitOne(33);
				}
			}
			UnityEngine.Debug.LogWarning("ClipMapManager.debugKillThreads is true, killing meshing thread!");
		}
	}

	private class Cell : IPriorityQueueItem
	{
		public enum State
		{
			Uninitialized = 0,
			Dirty = 1,
			Meshing = 2,
			Meshed = 3,
			Unloading = 4,
			Destroyed = 5
		}

		public enum WorkStatus
		{
			None = 0,
			Start = 1,
			BuildMesh = 2,
			ColSimp = 3,
			VisSimp = 4,
			Grass = 5,
			Done = 6
		}

		private enum FinalizeStep
		{
			None = 0,
			MeshUploaded = 1,
			CollisionUploaded = 2,
			GrassUploaded = 3
		}

		public bool isPatchUpMeshing;

		private int queuedForMeshingFrame = -1;

		private int voxelsChangedFrame = -1;

		private VoxelandCollisionMeshSimplifier colSimp;

		private VoxelandVisualMeshSimplifier visSimp;

		public bool hiddenByParent;

		public int numChildrenReady;

		private VoxelandGrassBuilder grasser;

		private bool hasAnyGeometry;

		private float priority;

		private Vector3 cachedWorldPos;

		private CellProcessingStats heapStats;

		private int numPriorityChanges;

		private static int queueInSerial;

		private static int queueOutSerial;

		private float priorityUpdateRate = 1f / 15f;

		private float lastPriorityUpdate;

		private int cellLocked;

		private FinalizeStep finalizeStep;

		private float nextCheckDataReadyTime;

		public ClipMapManager mgr { get; private set; }

		public Level level { get; private set; }

		public VoxelandChunk chunk { get; private set; }

		public State state { get; private set; }

		public WorkStatus threadedWorkStatus { get; private set; }

		public bool hidden { get; private set; }

		public bool entsHidden { get; private set; }

		public bool hasGeometry => hasAnyGeometry;

		public bool ignoreEntities { get; set; }

		public bool isQueued { get; private set; }

		public Cell(ClipMapManager mgr, Level level)
		{
			this.mgr = mgr;
			this.level = level;
			state = State.Uninitialized;
			hidden = false;
			entsHidden = false;
			isQueued = false;
		}

		public bool LockCell()
		{
			return Interlocked.CompareExchange(ref cellLocked, 1, 0) == 0;
		}

		public void UnlockCell()
		{
			Interlocked.Exchange(ref cellLocked, 0);
		}

		public void OnEnqueue()
		{
			isQueued = true;
			finalizeStep = FinalizeStep.None;
			cachedWorldPos = chunk.transform.position;
			UpdatePriority();
			numPriorityChanges = 0;
			InitializeHeapStats();
		}

		public void OnDequeue()
		{
			FinalizeHeapStats();
			isQueued = false;
		}

		public float GetPriority()
		{
			return priority;
		}

		public float UpdatePriority()
		{
			float unscaledTime = Time.unscaledTime;
			if (unscaledTime - lastPriorityUpdate > priorityUpdateRate)
			{
				Vector3 cachedCameraPosition = LargeWorldStreamer.main.cachedCameraPosition;
				Vector3 cachedCameraForward = LargeWorldStreamer.main.cachedCameraForward;
				Vector3 rhs = cachedWorldPos - cachedCameraPosition;
				float magnitude = rhs.magnitude;
				float num = Vector3.Dot(cachedCameraForward, rhs) / magnitude;
				float num2 = 2f - num;
				float num3 = num2 * num2;
				priority = magnitude * num3;
				if (level.settings.highPriority)
				{
					priority /= 2f;
				}
				numPriorityChanges++;
				lastPriorityUpdate = unscaledTime;
			}
			return priority;
		}

		private void InitializeHeapStats()
		{
			if (HeapStats.main.IsRecording)
			{
				Vector3 cachedCameraPosition = LargeWorldStreamer.main.cachedCameraPosition;
				Vector3 cachedCameraForward = LargeWorldStreamer.main.cachedCameraForward;
				Vector3 vector = cachedWorldPos - cachedCameraPosition;
				heapStats = new CellProcessingStats();
				heapStats.inId = queueInSerial++;
				heapStats.inTime = LargeWorldStreamer.main.cachedTime;
				heapStats.inAngle = Vector3.Angle(cachedCameraForward, vector);
				heapStats.inDistance = Vector3.Magnitude(vector);
				heapStats.inPriority = GetPriority();
				heapStats.inQueueLength = mgr.toMeshByPriority.Count;
			}
		}

		private void FinalizeHeapStats()
		{
			if (heapStats != null)
			{
				Vector3 cachedCameraPosition = LargeWorldStreamer.main.cachedCameraPosition;
				Vector3 cachedCameraForward = LargeWorldStreamer.main.cachedCameraForward;
				Vector3 vector = cachedWorldPos - cachedCameraPosition;
				heapStats.numPriorityChanges = numPriorityChanges;
				heapStats.outId = queueOutSerial++;
				heapStats.outTime = LargeWorldStreamer.main.cachedTime;
				heapStats.outAngle = Vector3.Angle(cachedCameraForward, vector);
				heapStats.outDistance = Vector3.Magnitude(vector);
				heapStats.outPriority = UpdatePriority();
				HeapStats.main.RecordStats("MeshingHeap", heapStats);
				heapStats = null;
			}
		}

		public void OnVoxelsUnloading()
		{
			if (state != State.Meshing)
			{
				state = State.Unloading;
				mgr.unloadingCells.Add(this);
			}
		}

		public void OnVoxelsChanged()
		{
			voxelsChangedFrame = Time.frameCount;
		}

		public void BeginFrame()
		{
			numChildrenReady = 0;
		}

		public void UpdatePhase1(Int3 currChunkNum)
		{
			switch (state)
			{
			case State.Uninitialized:
				OnDirty(currChunkNum);
				break;
			case State.Dirty:
				OnDirty(currChunkNum);
				break;
			case State.Meshed:
				if (chunk.GetIndex() != currChunkNum || voxelsChangedFrame > queuedForMeshingFrame)
				{
					OnDirty(currChunkNum, chunk.GetIndex() == currChunkNum && voxelsChangedFrame > queuedForMeshingFrame);
				}
				break;
			case State.Unloading:
				Unload(currChunkNum);
				break;
			case State.Meshing:
			case State.Destroyed:
				break;
			}
		}

		public void HideMeshes()
		{
			if (!hidden)
			{
				if (debugForEdits)
				{
					UnityEngine.Debug.Log("Hiding chunk " + chunk.GetIndex());
				}
				hidden = true;
				if (chunk != null)
				{
					chunk.SetRenderersEnabled(enabled: false, fade: false);
				}
			}
		}

		public void HideEntities()
		{
			if (!entsHidden)
			{
				entsHidden = true;
				if (chunk != null && mgr.eventHandler != null && level.settings.entities)
				{
					mgr.eventHandler.HideEntities(chunk.GetIndex().Refined(level.blocksPerChunk), level.level);
				}
			}
		}

		public void ShowMeshes()
		{
			if (hidden)
			{
				if (debugForEdits)
				{
					UnityEngine.Debug.Log("Showing chunk " + chunk.GetIndex());
				}
				hidden = false;
				if (chunk != null && hasAnyGeometry)
				{
					chunk.SetRenderersEnabled(enabled: true, level.settings.fadeMeshes);
				}
			}
		}

		public void ShowEntities()
		{
			if (entsHidden && !ignoreEntities)
			{
				entsHidden = false;
				if (chunk != null && mgr.eventHandler != null && level.settings.entities)
				{
					mgr.eventHandler.ShowEntities(chunk.GetIndex().Refined(level.blocksPerChunk), level.level);
				}
			}
		}

		private void RepoolMeshes(List<MeshFilter> filters, MeshPool pool)
		{
			if (filters == null)
			{
				return;
			}
			int count = filters.Count;
			for (int i = 0; i < count; i++)
			{
				MeshFilter meshFilter = filters[i];
				Mesh sharedMesh = meshFilter.sharedMesh;
				if (sharedMesh != null)
				{
					meshFilter.sharedMesh = null;
					sharedMesh.name = "TERRAIN unused";
					pool.Return(sharedMesh);
				}
			}
		}

		private void RepoolMeshes(VoxelandChunk chunk)
		{
			RepoolMeshes(chunk.hiFilters, mgr.meshPool);
			RepoolMeshes(chunk.grassFilters, mgr.meshPool);
			if (chunk.collision != null)
			{
				MeshCollider collision = chunk.collision;
				if (collision != null && collision.sharedMesh != null)
				{
					Mesh sharedMesh = collision.sharedMesh;
					collision.sharedMesh = null;
					sharedMesh.name = "TERRAIN unused";
					mgr.meshPool.Return(sharedMesh);
				}
			}
		}

		private void Unload(Int3 newChunkNum)
		{
			state = State.Dirty;
			mgr.unloadingCells.Remove(this);
			if (!chunk)
			{
				return;
			}
			if (chunk.GetIndex() != newChunkNum)
			{
				OnDirty(newChunkNum);
				return;
			}
			HideMeshes();
			HideEntities();
			if (chunk.collision != null)
			{
				chunk.collision.gameObject.SetActive(value: false);
			}
			RepoolMeshes(chunk);
			chunk.cx = -1;
		}

		private void OnDirty(Int3 newChunkNum, bool isPatchUp = false)
		{
			if (!chunk)
			{
				CreateChunk();
				chunk.cx = -1;
			}
			state = State.Dirty;
			if (!(Time.unscaledTime > nextCheckDataReadyTime) || SaveLoadManager.main.isSaving)
			{
				return;
			}
			int size = level.blocksPerChunk >> level.settings.downsamples;
			if (mgr.land.IsChunkDataReady(newChunkNum, level.settings.downsamples, size, level.settings.meshOverlap + 3))
			{
				if (newChunkNum != chunk.GetIndex())
				{
					HideMeshes();
					HideEntities();
					if (chunk.collision != null)
					{
						chunk.collision.gameObject.SetActive(value: false);
					}
					RepoolMeshes(chunk);
					chunk.SetPosition(level.settings.downsamples, newChunkNum, level.blocksPerChunk, level.settings.meshOverlap);
					chunk.generateCollider = level.settings.colliders;
				}
				if (level.settings.ignoreMeshes || mgr.land.IsChunkUniform(newChunkNum, level.settings.downsamples, size, level.settings.meshOverlap + 3))
				{
					queuedForMeshingFrame = Time.frameCount;
					state = State.Meshed;
					hasAnyGeometry = false;
					HideMeshes();
				}
				else if (mgr.debugNonThreaded)
				{
					if (mgr.frameOfLastChunkBuild != Time.frameCount)
					{
						queuedForMeshingFrame = Time.frameCount;
						mgr.frameOfLastChunkBuild = Time.frameCount;
						state = State.Meshing;
						finalizeStep = FinalizeStep.None;
						mgr.meshingCells.Add(this);
						isPatchUpMeshing = isPatchUp;
						threadedWorkStatus = WorkStatus.None;
						DoThreadablePart(mgr.bufferPools[0], mgr.workspaces[0], mgr.visSimpPool[0], mgr.colSimpPool[0], mgr.grassBuilderPool[0]);
						while (!DoFinalizePart())
						{
						}
					}
					else
					{
						chunk.cx = -1;
					}
				}
				else if (!isQueued)
				{
					OnEnqueue();
					queuedForMeshingFrame = Time.frameCount;
					state = State.Meshing;
					mgr.meshingCells.Add(this);
					isPatchUpMeshing = isPatchUp;
					threadedWorkStatus = WorkStatus.None;
					mgr.AddCellToMeshingQueue(this);
					mgr.meshThreadEvent.Set();
				}
			}
			else
			{
				mgr.lastWaitForDataFrame = Time.frameCount;
				nextCheckDataReadyTime = Time.unscaledTime + UnityEngine.Random.Range(0.5f, 1f);
			}
		}

		public bool IntersectsWith(Int3.Bounds blocks)
		{
			if (!chunk)
			{
				return false;
			}
			return chunk.GetIndex().Refined(level.blocksPerChunk).Intersects(blocks);
		}

		private void CreateChunk()
		{
			GameObject gameObject = new GameObject("Chunk");
			gameObject.transform.parent = mgr.transform;
			gameObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
			gameObject.layer = mgr.gameObject.layer;
			chunk = gameObject.AddComponent<VoxelandChunk>();
			chunk.land = mgr.land;
		}

		public void ResetCell()
		{
			chunk.cx = -1;
		}

		public void Destroy()
		{
			chunk.DestroySelf();
			state = State.Destroyed;
		}

		public void DoThreadablePart(MeshBufferPools bufferPools, VoxelandChunkWorkspace ws, VoxelandVisualMeshSimplifier _visSimp, VoxelandCollisionMeshSimplifier _colSimp, VoxelandGrassBuilder _grasser)
		{
			_ = StopwatchProfiler.Instance;
			threadedWorkStatus = WorkStatus.Start;
			ws.SetSize(chunk.meshRes);
			chunk.ws = ws;
			chunk.skipHiRes = level.settings.visual.useLowMesh;
			chunk.disableGrass = true;
			threadedWorkStatus = WorkStatus.BuildMesh;
			chunk.BuildMesh(level.settings.skipRelax, mgr.settings.debugSingleBlockType ? 1 : level.settings.maxBlockTypes);
			hasAnyGeometry = ws.visibleFaces.Count > 0;
			if (hasAnyGeometry)
			{
				if (level.settings.colliders)
				{
					colSimp = _colSimp;
					colSimp.inUse = true;
					colSimp.SetPools(bufferPools);
					threadedWorkStatus = WorkStatus.ColSimp;
					colSimp.Build(ws);
				}
				visSimp = _visSimp;
				visSimp.inUse = true;
				visSimp.settings = level.settings.visual;
				visSimp.debugUseLQShader = mgr.debugUseLQShader;
				visSimp.debugAllOpaque = mgr.debugAllOpaque;
				visSimp.debugSkipMaterials = mgr.debugSkipMaterials;
				threadedWorkStatus = WorkStatus.VisSimp;
				visSimp.Build(chunk, bufferPools, level.settings.debug);
				if (level.settings.grass && !mgr.debugOverrideDisableGrass)
				{
					threadedWorkStatus = WorkStatus.Grass;
					grasser = _grasser;
					grasser.inUse = true;
					grasser.Reset(bufferPools);
					grasser.CreateMeshData(chunk, level.settings.grassSettings);
				}
			}
			chunk.ws = null;
			threadedWorkStatus = WorkStatus.Done;
		}

		[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
		public bool DoFinalizePart()
		{
			_ = StopwatchProfiler.Instance;
			mgr.meshingCells.Remove(this);
			if (hasAnyGeometry)
			{
				chunk.gameObject.SetActive(value: true);
				switch (finalizeStep)
				{
				case FinalizeStep.None:
				{
					int addSortingValue = ((!mgr.debugDisableRenderOrderOpt) ? level.level : 0);
					visSimp.BuildLayerObjects(chunk, level.settings.castShadows, addSortingValue, mgr.streamer.terrainPoolManager);
					visSimp.inUse = false;
					visSimp = null;
					finalizeStep = FinalizeStep.MeshUploaded;
					break;
				}
				case FinalizeStep.MeshUploaded:
					if (level.settings.colliders)
					{
						colSimp.AttachTo(chunk, mgr.streamer.terrainPoolManager);
						colSimp.inUse = false;
						colSimp = null;
					}
					finalizeStep = FinalizeStep.CollisionUploaded;
					break;
				case FinalizeStep.CollisionUploaded:
					if (level.settings.grass && !mgr.debugOverrideDisableGrass && grasser != null)
					{
						grasser.CreateUnityMeshes(chunk, mgr.streamer.terrainPoolManager);
						grasser.inUse = false;
						grasser = null;
					}
					finalizeStep = FinalizeStep.GrassUploaded;
					break;
				}
				if (finalizeStep < FinalizeStep.GrassUploaded)
				{
					return false;
				}
			}
			state = State.Meshed;
			chunk.SetRenderersEnabled(enabled: false, fade: false);
			hidden = true;
			isPatchUpMeshing = false;
			return true;
		}

		public bool IsReady()
		{
			if (state != State.Meshed)
			{
				if (state == State.Meshing)
				{
					return isPatchUpMeshing;
				}
				return false;
			}
			return true;
		}

		public int CountDrawCalls()
		{
			return chunk.CountDrawCalls();
		}

		public override string ToString()
		{
			return $"[Cell: mgr={mgr}, level={level}, chunk={chunk}, state={state}, threadedWorkStatus={threadedWorkStatus}, hidden={hidden}, entsHidden={entsHidden}, hasGeometry={hasGeometry}, ignoreEntities={ignoreEntities}, isQueued={isQueued}]";
		}
	}

	private class Level
	{
		private ClipMapManager mgr;

		private Int3 activeMins;

		private Int3 activeMaxs;

		private Int3 entityContractDist;

		private Int3 cameraChunk;

		public int level { get; private set; }

		public LevelSettings settings { get; private set; }

		public Array3<Cell> cells { get; private set; }

		public Int3.Bounds activeChunks => new Int3.Bounds(activeMins, activeMaxs);

		public Int3.Bounds activeCells => new Int3.Bounds(activeMins % chunksPerSide, activeMaxs % chunksPerSide);

		public Int3 chunksPerSide { get; private set; }

		public int blocksPerChunk => mgr.settings.chunkMeshRes << level;

		public Int3.Bounds activeBlocks => new Int3.Bounds(activeMins * blocksPerChunk, (activeMaxs + 1) * blocksPerChunk - 1);

		public long EstimateBytes()
		{
			return cells.Length * 4;
		}

		public Level(int levelNum, ClipMapManager mgr, LevelSettings settings)
		{
			this.mgr = mgr;
			level = levelNum;
			this.settings = settings;
			chunksPerSide = settings.GetChunksPerSide();
			entityContractDist = settings.GetChunksPerSide().CeilDiv(2) - settings.contractEntityChunks;
			cells = new Array3<Cell>(chunksPerSide.x, chunksPerSide.y, chunksPerSide.z);
			Int3.RangeEnumerator rangeEnumerator = cells.Indices();
			while (rangeEnumerator.MoveNext())
			{
				Int3 current = rangeEnumerator.Current;
				Cell value = new Cell(mgr, this);
				cells.Set(current, value);
			}
		}

		public void DrawActiveChunksGizmos()
		{
			Int3.RangeEnumerator enumerator = activeChunks.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Int3 current = enumerator.Current;
				Cell cell = GetCell(current);
				Color color = Color.red;
				switch (cell.state)
				{
				case Cell.State.Meshed:
					color = Color.green;
					break;
				case Cell.State.Meshing:
					color = Color.yellow;
					break;
				}
				Int3 @int = current * blocksPerChunk;
				Int3 int2 = (current + 1) * blocksPerChunk;
				Vector3 vector = mgr.land.transform.TransformPoint(@int.ToVector3());
				Vector3 vector2 = mgr.land.transform.TransformPoint(int2.ToVector3());
				Vector3 center = (vector + vector2) / 2f;
				Vector3 size = vector2 - vector;
				Gizmos.color = color;
				Gizmos.DrawWireCube(center, size);
			}
		}

		public void DrawActiveChunkDebug()
		{
			Int3.RangeEnumerator enumerator = activeChunks.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Int3 current = enumerator.Current;
				Int3 p = current % chunksPerSide;
				Cell cell = cells.Get(p);
				if (!settings.ignoreMeshes)
				{
					Int3 @int = current * blocksPerChunk;
					Int3 int2 = (current + 1) * blocksPerChunk;
					Color color = ((cell.state != Cell.State.Meshed) ? ((cell.state == Cell.State.Meshing) ? Color.yellow : ((cell.state == Cell.State.Dirty) ? Color.red : Color.gray)) : (cell.ignoreEntities ? Color.magenta : Color.green));
					Vector3 min = mgr.land.transform.TransformPoint(@int.ToVector3()) + Vector3.one * 0.25f;
					Vector3 max = mgr.land.transform.TransformPoint(int2.ToVector3()) - Vector3.one * 0.25f;
					DrawChunkDebug(min, max, color);
				}
			}
			if (!settings.ignoreMeshes)
			{
				Int3 int3 = cameraChunk;
				Int3 int4 = int3 * blocksPerChunk;
				Int3 int5 = (int3 + 1) * blocksPerChunk;
				Vector3 min2 = mgr.land.transform.TransformPoint(int4.ToVector3()) + Vector3.one * 0.5f;
				Vector3 max2 = mgr.land.transform.TransformPoint(int5.ToVector3()) - Vector3.one * 0.5f;
				DrawChunkDebug(min2, max2, Color.cyan);
			}
		}

		private void DrawChunkDebug(Vector3 min, Vector3 max, Color color)
		{
			GL.Begin(2);
			GL.Color(color);
			GL.Vertex3(min.x, min.y, min.z);
			GL.Color(color);
			GL.Vertex3(max.x, min.y, min.z);
			GL.Color(color);
			GL.Vertex3(max.x, min.y, max.z);
			GL.Color(color);
			GL.Vertex3(min.x, min.y, max.z);
			GL.Color(color);
			GL.Vertex3(min.x, min.y, min.z);
			GL.End();
			GL.Begin(2);
			GL.Color(color);
			GL.Vertex3(min.x, max.y, min.z);
			GL.Color(color);
			GL.Vertex3(max.x, max.y, min.z);
			GL.Color(color);
			GL.Vertex3(max.x, max.y, max.z);
			GL.Color(color);
			GL.Vertex3(min.x, max.y, max.z);
			GL.Color(color);
			GL.Vertex3(min.x, max.y, min.z);
			GL.End();
			GL.Begin(1);
			GL.Color(color);
			GL.Vertex3(min.x, min.y, min.z);
			GL.Color(color);
			GL.Vertex3(min.x, max.y, min.z);
			GL.Color(color);
			GL.Vertex3(max.x, min.y, min.z);
			GL.Color(color);
			GL.Vertex3(max.x, max.y, min.z);
			GL.Color(color);
			GL.Vertex3(max.x, min.y, max.z);
			GL.Color(color);
			GL.Vertex3(max.x, max.y, max.z);
			GL.Color(color);
			GL.Vertex3(min.x, min.y, max.z);
			GL.Color(color);
			GL.Vertex3(min.x, max.y, max.z);
			GL.End();
		}

		private Cell GetCell(Int3 globalIndex)
		{
			Int3 p = globalIndex % chunksPerSide;
			return cells.Get(p);
		}

		public void BeginFrame()
		{
			foreach (Cell cell in cells)
			{
				cell.BeginFrame();
			}
		}

		public void UpdateActiveBounds(Int3 camBlockPos, Int3 maxBlockPos)
		{
			Int3 @int = blocksPerChunk * chunksPerSide;
			activeMins = (camBlockPos - @int / 2).RoundDiv(blocksPerChunk);
			Int3 b = maxBlockPos.CeilDiv(blocksPerChunk);
			activeMins = Int3.Max(activeMins, Int3.zero);
			activeMaxs = Int3.Min(activeMins + chunksPerSide - 1, b);
			cameraChunk = camBlockPos / blocksPerChunk;
		}

		public Vector3 GetLocalChunkPosition(Int3 chunkId)
		{
			Int3 @int = chunkId * blocksPerChunk;
			Int3 int2 = (chunkId + 1) * blocksPerChunk;
			Vector3 vector = mgr.land.transform.TransformPoint(@int.ToVector3());
			Vector3 vector2 = mgr.land.transform.TransformPoint(int2.ToVector3());
			return (vector + vector2) * 0.5f;
		}

		public Vector3 GetLocalCameraPosition()
		{
			return mgr.streamer.cachedCameraPosition + mgr.camOffset;
		}

		public void DebugDraw()
		{
			if (!mgr.debugDrawMeshingChunks)
			{
				return;
			}
			Int3.RangeEnumerator enumerator = activeChunks.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Int3 current = enumerator.Current;
				if (GetCell(current).state == Cell.State.Meshing)
				{
					Int3 @int = current * blocksPerChunk;
					Int3 int2 = (current + 1) * blocksPerChunk;
					Utils.DebugDrawAABB(mgr.land.transform.TransformPoint(@int.ToVector3()).AddScalar(1f), mgr.land.transform.TransformPoint(int2.ToVector3()).AddScalar(-1f), 1, Color.green);
				}
			}
		}

		public void UpdatePhase1(Int3 chunk)
		{
			GetCell(chunk).UpdatePhase1(chunk);
		}

		public void NotifyBlocksUnloading(Int3.Bounds blocks)
		{
			Int3.Bounds other = Int3.Bounds.OuterCoarserBounds(blocks, new Int3(blocksPerChunk));
			Int3.RangeEnumerator enumerator = activeChunks.IntersectionWith(other).GetEnumerator();
			while (enumerator.MoveNext())
			{
				Int3 current = enumerator.Current;
				GetCell(current).OnVoxelsUnloading();
			}
		}

		public void NotifyBlocksChanged(Int3.Bounds blocks)
		{
			blocks.Expand(2 << level);
			Int3.Bounds other = Int3.Bounds.OuterCoarserBounds(blocks, new Int3(blocksPerChunk));
			Int3.RangeEnumerator enumerator = activeChunks.IntersectionWith(other).GetEnumerator();
			while (enumerator.MoveNext())
			{
				Int3 current = enumerator.Current;
				GetCell(current).OnVoxelsChanged();
			}
		}

		public Cell GetCellForBlock(Int3 block)
		{
			if (!activeBlocks.Contains(block))
			{
				return null;
			}
			Int3 globalIndex = block / blocksPerChunk;
			return GetCell(globalIndex);
		}

		public bool IsBlockCovered(Int3 block)
		{
			Cell cellForBlock = GetCellForBlock(block);
			if (cellForBlock == null)
			{
				return false;
			}
			if (cellForBlock.hidden)
			{
				return cellForBlock.hiddenByParent;
			}
			return true;
		}

		public void NotifyChildChunkReady(Int3 block)
		{
			Cell cellForBlock = GetCellForBlock(block);
			if (cellForBlock != null)
			{
				cellForBlock.numChildrenReady++;
			}
		}

		public void UpdateVisPhase1(Level outerLevel)
		{
			foreach (Cell cell in cells)
			{
				if (cell.IsReady())
				{
					Int3 block = cell.chunk.GetIndex() * blocksPerChunk;
					outerLevel.NotifyChildChunkReady(block);
				}
			}
		}

		public void UpdateVisPhase2(Level outerLevel, Level innerLevel)
		{
			foreach (Cell cell in cells)
			{
				if (!cell.IsReady())
				{
					cell.HideMeshes();
					cell.HideEntities();
					cell.hiddenByParent = false;
					continue;
				}
				Int3 index = cell.chunk.GetIndex();
				Int3.Bounds bounds = index.Refined(blocksPerChunk);
				if (settings.contractEntityChunks > 0)
				{
					Int3 @int = (cameraChunk - index).Abs();
					bool flag = @int.x >= entityContractDist.x || @int.y >= entityContractDist.y || @int.z >= entityContractDist.z;
					if (flag != cell.ignoreEntities)
					{
						cell.ignoreEntities = flag;
						if (cell.ignoreEntities)
						{
							cell.HideEntities();
						}
					}
				}
				if (outerLevel != null && !outerLevel.settings.ignoreMeshes && outerLevel.IsBlockCovered(bounds.mins))
				{
					cell.HideMeshes();
					if (settings.ignoreMeshes)
					{
						cell.ShowEntities();
					}
					else
					{
						cell.HideEntities();
					}
					cell.hiddenByParent = true;
					continue;
				}
				cell.hiddenByParent = false;
				if (innerLevel != null)
				{
					if (cell.numChildrenReady == 8)
					{
						if (innerLevel.settings.ignoreMeshes)
						{
							cell.ShowMeshes();
						}
						else
						{
							cell.HideMeshes();
						}
						cell.ShowEntities();
					}
					else
					{
						cell.ShowMeshes();
						cell.ShowEntities();
					}
				}
				else
				{
					cell.ShowMeshes();
					cell.ShowEntities();
				}
			}
		}

		public void ResetLevel()
		{
			foreach (Cell cell in cells)
			{
				cell.ResetCell();
			}
		}

		public void Destroy()
		{
			foreach (Cell cell in cells)
			{
				cell.Destroy();
			}
			cells = null;
		}

		public bool IsRangeActiveAndBuilt(Int3.Bounds blocks)
		{
			if (!activeBlocks.Contains(blocks))
			{
				return false;
			}
			Int3.Bounds other = Int3.Bounds.OuterCoarserBounds(blocks, new Int3(blocksPerChunk));
			Int3.RangeEnumerator enumerator = activeChunks.IntersectionWith(other).GetEnumerator();
			while (enumerator.MoveNext())
			{
				Int3 current = enumerator.Current;
				if (!GetCell(current).IsReady())
				{
					return false;
				}
			}
			return true;
		}

		public int CountDrawCalls()
		{
			int num = 0;
			foreach (Cell cell in cells)
			{
				num += cell.CountDrawCalls();
			}
			return num;
		}
	}

	public sealed class ProcessFinalizeQueueCoroutine : StateMachineBase<ClipMapManager>
	{
		public override bool MoveNext()
		{
			return host.ProcessNextElementInFinalizeQueue();
		}

		public override void Reset()
		{
		}
	}

	public sealed class UpdateVisibilityForMeshingCoroutine : StateMachineBase<ClipMapManager>
	{
		private enum UpdateMeshVisState
		{
			Init = 0,
			SetLevel = 1,
			ProcessChunk = 2
		}

		private UpdateMeshVisState visMeshingState;

		private Int3 visCamPos;

		private Int3 visMaxPos;

		private int curLevelIdx;

		private Level curLevel;

		private Int3.RangeEnumerator rangeEnumerator;

		private Int3.RangeEnumerator cellRangeEnumerator;

		private int numberResultsProcessed;

		private int currentProcessingIndex;

		private long totalMsPerUpdate = 3L;

		private Int3 lastCamPos = new Int3(0, 0, 0);

		private Stopwatch frameTimer = new Stopwatch();

		private bool HasExceededTimeFrame()
		{
			frameTimer.Stop();
			if (frameTimer.ElapsedMilliseconds > totalMsPerUpdate)
			{
				return true;
			}
			frameTimer.Start();
			return false;
		}

		public override bool MoveNext()
		{
			if (host.debugSuspendMeshing)
			{
				return false;
			}
			switch (visMeshingState)
			{
			case UpdateMeshVisState.Init:
			{
				Vector3 wsPos = host.streamer.cachedCameraPosition + host.camOffset;
				visCamPos = host.GetBlock(wsPos);
				visMaxPos = host.land.data.GetSize() - 1;
				curLevelIdx = 0;
				visMeshingState = UpdateMeshVisState.SetLevel;
				numberResultsProcessed = 0;
				currentProcessingIndex = 0;
				return true;
			}
			case UpdateMeshVisState.SetLevel:
				if (curLevelIdx < host.levels.Length)
				{
					if (host.AreMeshQueuesFull())
					{
						UnityEngine.Debug.LogWarningFormat("UpdateVisibilityForMeshingCoroutine- Mesh Queue is full!");
						return false;
					}
					curLevel = host.levels[curLevelIdx];
					curLevel.UpdateActiveBounds(visCamPos, visMaxPos);
					rangeEnumerator = curLevel.activeChunks.GetEnumerator();
					while (cellRangeEnumerator.MoveNext())
					{
						curLevel.cells.Get(cellRangeEnumerator.Current).UpdatePriority();
					}
					visMeshingState = UpdateMeshVisState.ProcessChunk;
					return true;
				}
				break;
			case UpdateMeshVisState.ProcessChunk:
				numberResultsProcessed = 0;
				while (rangeEnumerator.MoveNext())
				{
					curLevel.UpdatePhase1(rangeEnumerator.Current);
					currentProcessingIndex++;
					numberResultsProcessed++;
					if (numberResultsProcessed > 50)
					{
						if (HasExceededTimeFrame())
						{
							return true;
						}
						numberResultsProcessed = 0;
					}
				}
				curLevelIdx++;
				visMeshingState = UpdateMeshVisState.SetLevel;
				lastCamPos = visCamPos;
				return true;
			}
			return false;
		}

		public override void Reset()
		{
			visMeshingState = UpdateMeshVisState.Init;
			curLevelIdx = 0;
		}
	}

	public sealed class UpdateVisibilityForCellsCoroutine : StateMachineBase<ClipMapManager>
	{
		private enum UpdateCellVisState
		{
			Init = 0,
			Phase1 = 1,
			Phase2 = 2
		}

		private bool force;

		private UpdateCellVisState cellVisState;

		private Int3 visCamPos;

		private Int3 visMaxPos;

		private int curLevelIdx;

		private int phase2idx;

		public void Initialize(bool force)
		{
			this.force = force;
		}

		public override bool MoveNext()
		{
			switch (cellVisState)
			{
			case UpdateCellVisState.Init:
			{
				Vector3 wsPos = host.streamer.cachedCameraPosition + host.camOffset;
				visCamPos = host.GetBlock(wsPos);
				visMaxPos = host.land.data.GetSize() - 1;
				Int3 @int = visCamPos / host.settings.chunkMeshRes;
				if (@int != host.lastViewerCell)
				{
					host.visibilityNeedsChecking = true;
				}
				host.lastViewerCell = @int;
				if ((host.visibilityNeedsChecking || force) && !host.debugDisableVisibilityPhase)
				{
					host.visibilityNeedsChecking = false;
					curLevelIdx = 0;
					for (int i = 0; i < host.levels.Length; i++)
					{
						host.levels[i].BeginFrame();
					}
					cellVisState = UpdateCellVisState.Phase1;
					return true;
				}
				break;
			}
			case UpdateCellVisState.Phase1:
				if (curLevelIdx < host.levels.Length - 1)
				{
					host.levels[curLevelIdx].UpdateVisPhase1(host.levels[curLevelIdx + 1]);
					curLevelIdx++;
				}
				else
				{
					phase2idx = host.levels.Length - 1;
					cellVisState = UpdateCellVisState.Phase2;
				}
				return true;
			case UpdateCellVisState.Phase2:
				if (phase2idx >= 0)
				{
					Level outerLevel = ((phase2idx == host.levels.Length - 1) ? null : host.levels[phase2idx + 1]);
					Level innerLevel = ((phase2idx == 0) ? null : host.levels[phase2idx - 1]);
					host.levels[phase2idx].UpdateVisPhase2(outerLevel, innerLevel);
					phase2idx--;
					return true;
				}
				break;
			}
			return false;
		}

		public override void Reset()
		{
			cellVisState = UpdateCellVisState.Init;
			visCamPos = Int3.zero;
			visMaxPos = Int3.zero;
			curLevelIdx = 0;
			phase2idx = 0;
			force = false;
		}
	}

	public static bool debugForEdits = false;

	public Voxeland land;

	public LargeWorldStreamer streamer;

	[NonSerialized]
	public Settings settings;

	public bool autoInit;

	public bool debugDrawMeshingChunks;

	public bool debugNonThreaded;

	public bool debugDisableVisibilityPhase;

	public bool debugOverrideDisableGrass;

	public bool debugSuspendMeshing;

	public bool debugSkipMaterials;

	public bool debugUseLQShader;

	public bool debugAllOpaque;

	public bool debugDisableRenderOrderOpt;

	public Int3 lastViewerCell = new Int3(-1);

	public bool visibilityNeedsChecking = true;

	public MeshBufferPools[] bufferPools;

	[NonSerialized]
	private VoxelandChunkWorkspace[] _workspaces_backingfield;

	private readonly MeshPool meshPool = new MeshPool();

	public Shader debugLineShader;

	private Material debugLineMaterial;

	private bool debugLineCallbackRegistered;

	public int lastWaitForDataFrame = -1;

	private bool inited;

	private Level[] levels;

	private int frameOfLastChunkBuild = -1;

	private readonly List<MeshingThread> meshingThreads = new List<MeshingThread>();

	private readonly List<MeshBufferPools> meshThreadPools = new List<MeshBufferPools>();

	private readonly LocklessQueueMPMC<Cell> toFinalize = new LocklessQueueMPMC<Cell>(2048);

	private object toMeshByPriorityLock = new object();

	private readonly DynamicPriorityQueue<Cell> toMeshByPriority = new DynamicPriorityQueue<Cell>();

	private readonly AutoResetEvent meshThreadEvent = new AutoResetEvent(initialState: false);

	private readonly AutoResetEvent meshToFinalizeEvent = new AutoResetEvent(initialState: false);

	private readonly AutoResetEvent meshToMeshEvent = new AutoResetEvent(initialState: false);

	private readonly HashSet<Cell> meshingCells = new HashSet<Cell>();

	private readonly HashSet<Cell> unloadingCells = new HashSet<Cell>();

	private Cell finalizingCell;

	private bool isDestroyed;

	private bool debugKillThreads;

	public IClipMapEventHandler eventHandler;

	private Stopwatch rebuildWatch = new Stopwatch();

	private readonly Vector3 camOffset = new Vector3(0f, -8f, 0f);

	private int activeQualityLevel = -1;

	private static readonly StateMachinePool<ProcessFinalizeQueueCoroutine, ClipMapManager> processFinalizeQueueCoroutines = new StateMachinePool<ProcessFinalizeQueueCoroutine, ClipMapManager>();

	private static readonly StateMachinePool<UpdateVisibilityForMeshingCoroutine, ClipMapManager> updateVisForMeshingCoroutines = new StateMachinePool<UpdateVisibilityForMeshingCoroutine, ClipMapManager>();

	private static readonly StateMachinePool<UpdateVisibilityForCellsCoroutine, ClipMapManager> updateVisibilityForCellsCoroutines = new StateMachinePool<UpdateVisibilityForCellsCoroutine, ClipMapManager>();

	public VoxelandChunkWorkspace[] workspaces
	{
		get
		{
			return _workspaces_backingfield;
		}
		private set
		{
			_workspaces_backingfield = value;
		}
	}

	public VoxelandCollisionMeshSimplifier[] colSimpPool { get; private set; }

	public VoxelandVisualMeshSimplifier[] visSimpPool { get; private set; }

	public VoxelandGrassBuilder[] grassBuilderPool { get; private set; }

	public static bool debugLinesEnabled { get; set; }

	public int meshingThreadCount => meshingThreads.Count;

	public int numToMesh => toMeshByPriority.Count;

	public int numToFinalize => toFinalize.Count + ((finalizingCell != null) ? 1 : 0);

	public int numMeshing => meshingCells.Count;

	public int numUnloading => unloadingCells.Count;

	public int numProcessing => numToMesh + numMeshing + numToFinalize + numUnloading;

	public bool IsIdle()
	{
		if (meshingCells.Count == 0 && unloadingCells.Count == 0 && toMeshByPriority.Count == 0 && toFinalize.Count == 0)
		{
			return finalizingCell == null;
		}
		return false;
	}

	public bool IsMeshingThreadActive(int i)
	{
		return meshingThreads[i].state == MeshingThread.State.Busy;
	}

	public bool AreMeshQueuesFull()
	{
		return false;
	}

	private void AddMeshingThread(int index, int consoleAffinityMask)
	{
		MeshingThread meshingThread = new MeshingThread(this, index, consoleAffinityMask);
		meshingThreads.Add(meshingThread);
		Thread thread = new Thread(meshingThread.Main);
		thread.IsBackground = true;
		thread.Start();
	}

	private void OnDestroy()
	{
		if (debugLineCallbackRegistered)
		{
			Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(CameraPostRender));
		}
		isDestroyed = true;
		for (int i = 0; i < meshingThreads.Count; i++)
		{
			meshThreadEvent.Set();
		}
		workspaces = null;
		colSimpPool = null;
		visSimpPool = null;
		grassBuilderPool = null;
	}

	private void Start()
	{
		if (autoInit)
		{
			Initialize();
		}
	}

	public static string GetClipMapSettingsFileForQualityLevel(int qualityLevel)
	{
		string text = QualitySettings.names[qualityLevel];
		string arg = string.Join("_", text.ToLower().Split(Path.GetInvalidFileNameChars()));
		return SNUtils.InsideUnmanaged($"clipmaps-{arg}.json");
	}

	public void ReloadSettings()
	{
		toFinalize.Clear();
		lock (toMeshByPriorityLock)
		{
			toMeshByPriority.Clear();
		}
		if (levels != null)
		{
			Level[] array = levels;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Destroy();
			}
			levels = null;
		}
		activeQualityLevel = QualitySettings.GetQualityLevel();
		string clipMapSettingsFileForQualityLevel = GetClipMapSettingsFileForQualityLevel(activeQualityLevel);
		string json = File.ReadAllText(clipMapSettingsFileForQualityLevel);
		settings = JsonUtility.FromJson<Settings>(json);
		UnityEngine.Debug.LogFormat("Read clip map settings from {0}:\n{1}", clipMapSettingsFileForQualityLevel, settings.ToString());
		int levelNum = 0;
		levels = new Level[settings.levels.Length];
		LevelSettings[] array2 = settings.levels;
		foreach (LevelSettings levelSettings in array2)
		{
			Level level = new Level(levelNum, this, levelSettings);
			levels[levelNum++] = level;
		}
	}

	public void Initialize()
	{
		land.freeze = true;
		land.DestroyAllChunks();
		if (streamer == null)
		{
			streamer = GetComponent<LargeWorldStreamer>();
		}
		inited = true;
		ReloadSettings();
		int num = System.Math.Min(settings.maxThreads, Environment.ProcessorCount - Environment.ProcessorCount / 4);
		LaunchMeshingThreads();
		bufferPools = new MeshBufferPools[meshingThreadCount];
		colSimpPool = new VoxelandCollisionMeshSimplifier[num];
		visSimpPool = new VoxelandVisualMeshSimplifier[num];
		grassBuilderPool = new VoxelandGrassBuilder[num];
		workspaces = new VoxelandChunkWorkspace[num];
		int num2 = -1;
		Level[] array = levels;
		foreach (Level level in array)
		{
			int val = level.blocksPerChunk >> level.settings.downsamples;
			num2 = System.Math.Max(num2, val);
		}
		for (int j = 0; j < meshingThreadCount; j++)
		{
			bufferPools[j] = new MeshBufferPools();
			meshingThreads[j].bufferPools = bufferPools[j];
			VoxelandCollisionMeshSimplifier voxelandCollisionMeshSimplifier = new VoxelandCollisionMeshSimplifier();
			voxelandCollisionMeshSimplifier.settings = settings.collision;
			colSimpPool[j] = voxelandCollisionMeshSimplifier;
			meshingThreads[j].colSimp = voxelandCollisionMeshSimplifier;
			visSimpPool[j] = new VoxelandVisualMeshSimplifier();
			meshingThreads[j].visSimp = visSimpPool[j];
			grassBuilderPool[j] = new VoxelandGrassBuilder();
			meshingThreads[j].grasser = grassBuilderPool[j];
			VoxelandChunkWorkspace voxelandChunkWorkspace = new VoxelandChunkWorkspace();
			voxelandChunkWorkspace.SetSize(num2);
			voxelandChunkWorkspace.faces.Capacity = 65536;
			voxelandChunkWorkspace.visibleFaces.Capacity = 65536;
			voxelandChunkWorkspace.verts.Capacity = 65536;
			workspaces[j] = voxelandChunkWorkspace;
			meshingThreads[j].workspace = voxelandChunkWorkspace;
		}
		UnityEngine.Debug.Log("ClipMapManager initialize done");
	}

	private void AddCellToMeshingQueue(Cell inCell)
	{
		if (inCell == null)
		{
			return;
		}
		lock (toMeshByPriorityLock)
		{
			toMeshByPriority.Enqueue(inCell);
		}
	}

	private void Update()
	{
		if (!inited || land.data == null)
		{
			return;
		}
		if (toFinalize.Count == 0 && finalizingCell == null && toMeshByPriority.Count == 0)
		{
			rebuildWatch.Stop();
		}
		else
		{
			meshThreadEvent.Set();
		}
		if (debugLinesEnabled)
		{
			if (!debugLineCallbackRegistered)
			{
				Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(CameraPostRender));
				debugLineCallbackRegistered = true;
			}
		}
		else if (debugLineCallbackRegistered)
		{
			Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(CameraPostRender));
			debugLineCallbackRegistered = false;
		}
	}

	public int GetActiveQualityLevel()
	{
		return activeQualityLevel;
	}

	private void OnDrawGizmosSelected()
	{
		if (levels != null)
		{
			Level[] array = levels;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DrawActiveChunksGizmos();
			}
		}
	}

	[SuppressMessage("Subnautica.Rules", "AvoidCameraMain")]
	public void CameraPostRender(Camera cam)
	{
		if (!(cam != Camera.main) && levels != null)
		{
			if (debugLineMaterial == null)
			{
				debugLineMaterial = new Material(debugLineShader);
			}
			debugLineMaterial.SetPass(0);
			Level[] array = levels;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DrawActiveChunkDebug();
			}
		}
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	private bool ProcessNextElementInFinalizeQueue()
	{
		if (finalizingCell == null && toFinalize.Pop(out finalizingCell))
		{
			if (!finalizingCell.LockCell())
			{
				finalizingCell = null;
				meshToFinalizeEvent.Set();
				return true;
			}
			meshToFinalizeEvent.Set();
		}
		if (finalizingCell != null)
		{
			if (finalizingCell.DoFinalizePart())
			{
				visibilityNeedsChecking = true;
				finalizingCell.UnlockCell();
				finalizingCell = null;
			}
			return true;
		}
		return false;
	}

	public IEnumerator ProcessFinalizeQueue()
	{
		return processFinalizeQueueCoroutines.Get(this);
	}

	public IEnumerator UpdateVisibilityForMeshing()
	{
		return updateVisForMeshingCoroutines.Get(this);
	}

	public IEnumerator UpdateVisibilityForCells(bool force = false)
	{
		PooledStateMachine<UpdateVisibilityForCellsCoroutine> pooledStateMachine = updateVisibilityForCellsCoroutines.Get(this);
		pooledStateMachine.stateMachine.Initialize(force);
		return pooledStateMachine;
	}

	public IEnumerator UpdateVisibilityForMeshingOriginal()
	{
		if (debugSuspendMeshing)
		{
			yield break;
		}
		Vector3 wsPos = streamer.cachedCameraPosition + camOffset;
		Int3 vsCamPos = GetBlock(wsPos);
		Int3 vsMaxPos = land.data.GetSize() - 1;
		for (int i = 0; i < levels.Length; i++)
		{
			if (AreMeshQueuesFull())
			{
				break;
			}
			Level level = levels[i];
			level.UpdateActiveBounds(vsCamPos, vsMaxPos);
			Int3.RangeEnumerator enumerator = level.activeChunks.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Int3 current = enumerator.Current;
				level.UpdatePhase1(current);
				yield return null;
			}
			level.DebugDraw();
		}
	}

	public IEnumerator UpdateVisibilityForCellsOriginal(bool force = false)
	{
		Vector3 wsPos = streamer.cachedCameraPosition + camOffset;
		Int3 block = GetBlock(wsPos);
		_ = land.data.GetSize() - 1;
		Int3 @int = block / settings.chunkMeshRes;
		if (@int != lastViewerCell)
		{
			visibilityNeedsChecking = true;
		}
		lastViewerCell = @int;
		if ((visibilityNeedsChecking || force) && !debugDisableVisibilityPhase)
		{
			visibilityNeedsChecking = false;
			for (int k = 0; k < levels.Length; k++)
			{
				levels[k].BeginFrame();
				yield return null;
			}
			for (int k = 0; k < levels.Length - 1; k++)
			{
				levels[k].UpdateVisPhase1(levels[k + 1]);
				yield return null;
			}
			for (int k = levels.Length - 1; k >= 0; k--)
			{
				levels[k].UpdateVisPhase2((k == levels.Length - 1) ? null : levels[k + 1], (k == 0) ? null : levels[k - 1]);
				yield return null;
			}
		}
	}

	public long EstimateBytes()
	{
		try
		{
			long num = 0L;
			for (int i = 0; i < meshingThreadCount; i++)
			{
				num += workspaces[i].EstimateBytes();
				num += colSimpPool[i].EstimateBytes();
				num += visSimpPool[i].EstimateBytes();
				num += grassBuilderPool[i].EstimateBytes();
			}
			if (bufferPools != null)
			{
				for (int j = 0; j < bufferPools.Length; j++)
				{
					num += bufferPools[j].EstimateBytes();
				}
			}
			return num;
		}
		finally
		{
		}
	}

	public void LayoutMemoryGUI()
	{
		if (workspaces == null)
		{
			return;
		}
		long num = 0L;
		long num2 = 0L;
		long num3 = 0L;
		long num4 = 0L;
		for (int i = 0; i < meshingThreadCount; i++)
		{
			num += workspaces[i].EstimateBytes();
			num2 += colSimpPool[i].EstimateBytes();
			num3 += visSimpPool[i].EstimateBytes();
			num4 += grassBuilderPool[i].EstimateBytes();
		}
		GUILayout.Label($"ChunkWSs:{(float)num / 1024f / 1024f} MB");
		GUILayout.Label($"ColSimplers: {(float)num2 / 1024f / 1024f} MB");
		GUILayout.Label($"VisSimplers: {(float)num3 / 1024f / 1024f} MB");
		GUILayout.Label($"GrassBuilders: {(float)num4 / 1024f / 1024f} MB");
		if (GUILayout.Button("Log chunk workspaces memory profile"))
		{
			for (int j = 0; j < workspaces.Length; j++)
			{
				workspaces[j].LogMemoryProfile();
			}
		}
	}

	public void LayoutDebugGUI()
	{
		if (GUILayout.Button("Log Draw Call Estimates"))
		{
			Level[] array = levels;
			foreach (Level level in array)
			{
				UnityEngine.Debug.Log("Level " + level.level + " has " + level.cells.Length + " cells and " + level.CountDrawCalls() + " draw calls");
			}
		}
		if (GUILayout.Button("Rebuild All"))
		{
			rebuildWatch.Restart();
			Level[] array = levels;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ResetLevel();
			}
		}
		GUILayout.Label("Rebuild ms: " + rebuildWatch.ElapsedMilliseconds);
		GUILayout.BeginVertical(GUI.skin.box);
		int num = Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, QualitySettings.names.Length);
		int num2 = Mathf.Clamp(activeQualityLevel, 0, QualitySettings.names.Length);
		GUILayout.Label($"Quality Level (current: {QualitySettings.names[num]})");
		GUILayout.Label($"ClipMap Level (current: {QualitySettings.names[num2]})");
		GUILayout.BeginHorizontal();
		for (int j = 0; j < QualitySettings.names.Length; j++)
		{
			if (GUILayout.Button(QualitySettings.names[j]))
			{
				GraphicsUtil.SetQualityLevel(j);
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		if (GUILayout.Button("Hide All"))
		{
			Level[] array = levels;
			for (int i = 0; i < array.Length; i++)
			{
				foreach (Cell cell in array[i].cells)
				{
					if (cell.chunk != null)
					{
						cell.HideMeshes();
						if (Event.current.alt)
						{
							cell.HideEntities();
						}
					}
				}
			}
		}
		GUILayout.BeginHorizontal("textarea");
		for (int k = 0; k < levels.Length; k++)
		{
			if (!GUILayout.Button("Hide L" + k))
			{
				continue;
			}
			foreach (Cell cell2 in levels[k].cells)
			{
				if (cell2.chunk != null)
				{
					cell2.HideMeshes();
					if (Event.current.alt)
					{
						cell2.HideEntities();
					}
				}
			}
		}
		GUILayout.EndHorizontal();
		VoxelandVisualMeshSimplifier.debugForceAlphaTest = GUILayout.Toggle(VoxelandVisualMeshSimplifier.debugForceAlphaTest, "Force Alpha Test mats");
		debugNonThreaded = GUILayout.Toggle(debugNonThreaded, "Not Threaded");
		debugDrawMeshingChunks = GUILayout.Toggle(debugDrawMeshingChunks, "Draw Meshing Chunks");
		debugDisableVisibilityPhase = GUILayout.Toggle(debugDisableVisibilityPhase, "Skip Visibility Phase");
		debugOverrideDisableGrass = GUILayout.Toggle(debugOverrideDisableGrass, "Disable All Grass");
		debugSuspendMeshing = GUILayout.Toggle(debugSuspendMeshing, "Suspend Meshing");
		debugSkipMaterials = GUILayout.Toggle(debugSkipMaterials, "Skip Materials");
		debugUseLQShader = GUILayout.Toggle(debugUseLQShader, "Use LQ Shader");
		debugAllOpaque = GUILayout.Toggle(debugAllOpaque, "Use Opaque Shader");
		settings.debugSingleBlockType = GUILayout.Toggle(settings.debugSingleBlockType, "One Layer All");
		debugDisableRenderOrderOpt = GUILayout.Toggle(debugDisableRenderOrderOpt, "Disable Render Order Opt");
		if (GUILayout.Button($"Kill all {meshingThreads.Count} Threads"))
		{
			debugKillThreads = true;
			for (int l = 0; l < meshingThreads.Count; l++)
			{
				meshThreadEvent.Set();
			}
		}
		if (GUILayout.Button("Reload Settings"))
		{
			ReloadSettings();
		}
		foreach (MeshingThread meshingThread in meshingThreads)
		{
			Cell.WorkStatus workStatus = Cell.WorkStatus.None;
			Cell busyWithCell = meshingThread.busyWithCell;
			if (busyWithCell != null)
			{
				workStatus = busyWithCell.threadedWorkStatus;
			}
			GUILayout.Label(string.Concat("Thread, chunksDone=", meshingThread.chunksDone, ", state=", meshingThread.state, ", cellstat=", workStatus));
		}
		GUILayout.Label("# toMesh: " + toMeshByPriority.Count);
		GUILayout.Label("# toFinalize: " + toFinalize.Count);
		if (lastWaitForDataFrame >= Time.frameCount - 5)
		{
			GUILayout.Label("WAITING FOR DATA");
		}
		if (bufferPools != null)
		{
			MeshBufferPools.LayoutGUI(bufferPools);
		}
		LayoutMemoryGUI();
		if (levels != null)
		{
			Level[] array = levels;
			foreach (Level level2 in array)
			{
				GUILayout.Label("Level " + level2.level + ": " + (float)level2.EstimateBytes() / 1024f / 1024f + " MB");
			}
		}
	}

	public void NotifyBlocksUnloading(Int3.Bounds blocks)
	{
		for (int i = 0; i < levels.Length; i++)
		{
			levels[i].NotifyBlocksUnloading(blocks);
		}
	}

	public void NotifyBlocksChanged(Int3.Bounds blocks)
	{
		for (int i = 0; i < levels.Length; i++)
		{
			levels[i].NotifyBlocksChanged(blocks);
		}
	}

	public bool IsRangeActiveAndBuilt(Bounds wsBounds)
	{
		Int3.Bounds blocks = new Int3.Bounds(GetBlock(wsBounds.min), GetBlock(wsBounds.max));
		return IsRangeActiveAndBuilt(blocks);
	}

	public bool IsRangeActiveAndBuilt(Int3.Bounds blocks)
	{
		return levels[0].IsRangeActiveAndBuilt(blocks);
	}

	public bool IsProcessingBlocks(Int3.Bounds blocks)
	{
		if (finalizingCell != null && finalizingCell.IntersectsWith(blocks))
		{
			return true;
		}
		foreach (Cell meshingCell in meshingCells)
		{
			if (meshingCell.IntersectsWith(blocks))
			{
				return true;
			}
		}
		foreach (Cell unloadingCell in unloadingCells)
		{
			if (unloadingCell.IntersectsWith(blocks))
			{
				return true;
			}
		}
		return false;
	}

	public Int3 GetBlock(Vector3 wsPos)
	{
		return Int3.Floor(land.transform.InverseTransformPoint(wsPos));
	}

	public void LaunchMeshingThreads()
	{
		int num = System.Math.Min(settings.maxThreads, Environment.ProcessorCount - Environment.ProcessorCount / 4);
		UnityEngine.Debug.LogFormat(this, "Starting {0} meshing threads.", num);
		for (int i = 0; i < num; i++)
		{
			int consoleAffinityMask = 0;
			AddMeshingThread(i, consoleAffinityMask);
		}
	}
}
