using System.Collections.Generic;
using System.Threading;
using UWE;
using UnityEngine;

namespace WorldStreaming
{
	public sealed class ClipmapStreamer : BatchOctreesStreamer.IListener, IStreamer, IPipeline
	{
		public readonly UWE.ThreadPool meshingThreads;

		private readonly int cellSize;

		private readonly ClipmapLevel[] levels;

		private readonly PriorityQueue<ClipmapCell> loadingQueue = new PriorityQueue<ClipmapCell>(100);

		private readonly PriorityQueue<ClipmapCell> unloadingQueue = new PriorityQueue<ClipmapCell>(100);

		public readonly BoundedObjectPool<MeshBuilder> meshBuilderPool;

		private readonly List<ClipMapManager.IClipMapEventHandler> listeners = new List<ClipMapManager.IClipMapEventHandler>();

		private int collisionLevel;

		private static readonly Task.Function BeginNotifyListenersDelegate = BeginNotifyListeners;

		public ClipMapManager.Settings settings { get; private set; }

		public WorldStreamer host { get; private set; }

		public IThread streamingThread { get; private set; }

		public IThread buildLayersThread { get; private set; }

		public IThread toggleChunksThread { get; private set; }

		public IThread destroyChunksThread { get; private set; }

		public ClipmapStreamer(WorldStreamer host, ClipmapVisibilityUpdater visibilityUpdater, IThread streamingThread, IThread buildLayersThread, IThread toggleChunksThread, IThread destroyChunksThread, ClipMapManager.Settings settings)
		{
			this.settings = settings;
			this.host = host;
			this.streamingThread = streamingThread;
			this.buildLayersThread = buildLayersThread;
			this.toggleChunksThread = toggleChunksThread;
			this.destroyChunksThread = destroyChunksThread;
			meshBuilderPool = new BoundedObjectPool<MeshBuilder>(settings.maxWorkspaces);
			meshingThreads = new UWE.ThreadPool("MeshingThreads", settings.maxThreads, System.Threading.ThreadPriority.BelowNormal, settings.threadAffinityMask, 128);
			cellSize = settings.chunkMeshRes;
			levels = new ClipmapLevel[settings.levels.Length];
			for (int i = 0; i < levels.Length; i++)
			{
				int num = cellSize << i;
				ClipmapLevel clipmapLevel = new ClipmapLevel(this, visibilityUpdater, i, num, settings.levels[i]);
				levels[i] = clipmapLevel;
				if (settings.levels[i].colliders)
				{
					collisionLevel = i;
				}
			}
		}

		public void Reset()
		{
			foreach (MeshBuilder item in meshBuilderPool)
			{
				item.Dispose();
			}
			settings = null;
			host = null;
			streamingThread = null;
			buildLayersThread = null;
			toggleChunksThread = null;
			destroyChunksThread = null;
			listeners.Clear();
			for (int i = 0; i < levels.Length; i++)
			{
				levels[i].Clear();
			}
		}

		public void Stop()
		{
			meshingThreads.Stop();
		}

		public bool IsRunning()
		{
			return meshingThreads.IsRunning();
		}

		public bool IsIdle()
		{
			if (meshingThreads.IsIdle())
			{
				return GetQueueLength() <= 0;
			}
			return false;
		}

		public int GetQueueLength()
		{
			return loadingQueue.Count + unloadingQueue.Count + meshingThreads.GetQueueLength();
		}

		public ClipmapLevel GetLevel(int levelId)
		{
			if (levelId < 0 || levelId >= levels.Length)
			{
				return null;
			}
			return levels[levelId];
		}

		public void OnRangeLoaded(BatchOctreesStreamer streamer, Int3.Bounds blockRange, int minLod, int maxLod)
		{
			minLod = Mathf.Clamp(minLod, 0, levels.Length - 1);
			maxLod = Mathf.Clamp(maxLod, 0, levels.Length - 1);
			for (int i = minLod; i <= maxLod; i++)
			{
				levels[i].OnBatchOctreesChanged(streamer, blockRange);
			}
		}

		public void OnRangeUnloaded(BatchOctreesStreamer streamer, Int3.Bounds blockRange, int minLod, int maxLod)
		{
		}

		public void RegisterListener(ClipMapManager.IClipMapEventHandler listener)
		{
			if (listeners.Contains(listener))
			{
				Debug.LogErrorFormat("Listener registering twice {0}", listener);
			}
			else
			{
				listeners.Add(listener);
			}
		}

		public void DeregisterListener(ClipMapManager.IClipMapEventHandler listener)
		{
			listeners.Remove(listener);
		}

		public void OnCellLoaded(ClipmapLevel level, Int3 cell)
		{
			Int3.Bounds blockRange = Int3.Bounds.FinerBounds(cell, level.cellSize);
			foreach (ClipMapManager.IClipMapEventHandler listener in listeners)
			{
				listener.ShowEntities(blockRange, level.id);
			}
		}

		public void OnCellUnloaded(ClipmapLevel level, Int3 cellId)
		{
			Int3.Bounds blockRange = Int3.Bounds.FinerBounds(cellId, level.cellSize);
			foreach (ClipMapManager.IClipMapEventHandler listener in listeners)
			{
				listener.HideEntities(blockRange, level.id);
			}
		}

		public bool IsProcessing(BatchOctreesStreamer streamer, Int3.Bounds blockRange, int minLod, int maxLod)
		{
			minLod = Mathf.Clamp(minLod, 0, levels.Length - 1);
			maxLod = Mathf.Clamp(maxLod, 0, levels.Length - 1);
			for (int i = minLod; i <= maxLod; i++)
			{
				if (levels[i].IsProcessing(blockRange))
				{
					return true;
				}
			}
			return false;
		}

		public bool IsRangeActiveAndBuilt(Int3.Bounds blockRange)
		{
			return levels[collisionLevel].IsRangeActiveAndBuilt(blockRange);
		}

		public bool UpdateCenter(Int3 position)
		{
			for (int i = 0; i < levels.Length; i++)
			{
				if (!levels[i].UpdateCenter(position))
				{
					return false;
				}
			}
			return true;
		}

		public void Unload()
		{
			for (int num = levels.Length - 1; num >= 0; num--)
			{
				levels[num].Unload();
			}
		}

		public void EnqueueForLoading(ClipmapCell cell, int priority)
		{
			loadingQueue.Enqueue(cell, priority);
		}

		public void EnqueueForUnloading(ClipmapCell cell, int priority)
		{
			unloadingQueue.Enqueue(cell, priority);
		}

		public bool ProcessQueues()
		{
			if (ProcessLoadingQueue())
			{
				return true;
			}
			if (ProcessUnloadingQueue())
			{
				return true;
			}
			return false;
		}

		public bool ProcessLoadingQueue()
		{
			ClipmapCell result;
			while (loadingQueue.TryDequeue(out result))
			{
				if (result.BeginLoading())
				{
					return true;
				}
			}
			return false;
		}

		public bool ProcessUnloadingQueue()
		{
			ClipmapCell result;
			while (unloadingQueue.TryDequeue(out result))
			{
				if (result.BeginUnloading())
				{
					return true;
				}
			}
			return false;
		}

		public void NotifyListeners(Int3.Bounds blockRange)
		{
			streamingThread.Enqueue(BeginNotifyListenersDelegate, this, blockRange);
		}

		private static void BeginNotifyListeners(object owner, object state)
		{
			ClipmapStreamer obj = (ClipmapStreamer)owner;
			Int3.Bounds blockRange = (Int3.Bounds)state;
			obj.BeginNotifyListeners(blockRange);
		}

		private void BeginNotifyListeners(Int3.Bounds blockRange)
		{
			for (int i = 0; i < levels.Length; i++)
			{
				levels[i].BeginNotifyListeners(blockRange);
			}
		}

		public void OnGUI()
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Clipmap", GUILayout.Width(60f));
			GUILayout.TextField(loadingQueue.Count.ToString(), GUILayout.Width(60f));
			GUILayout.TextField(unloadingQueue.Count.ToString(), GUILayout.Width(60f));
			GUILayout.EndHorizontal();
		}

		public void DrawGizmos()
		{
			for (int i = 0; i < levels.Length; i++)
			{
				levels[i].DrawGizmos();
			}
		}
	}
}
