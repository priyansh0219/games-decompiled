using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UWE;
using UnityEngine;

namespace WorldStreaming
{
	public sealed class BatchOctreesStreamer : IStreamer, IPipeline
	{
		public interface IListener
		{
			void OnRangeLoaded(BatchOctreesStreamer streamer, Int3.Bounds blockRange, int minLod, int maxLod);

			void OnRangeUnloaded(BatchOctreesStreamer streamer, Int3.Bounds blockRange, int minLod, int maxLod);

			bool IsProcessing(BatchOctreesStreamer streamer, Int3.Bounds blockRange, int minLod, int maxLod);
		}

		[Serializable]
		public sealed class Settings
		{
			public int batchesPerSide = 3;

			public int batchesVertically = 3;

			public Int3 centerMin;

			public Int3 centerMax;
		}

		public readonly Int3.Bounds octreeBounds;

		public readonly int minLod;

		public readonly int maxLod;

		public readonly int batchSize;

		private readonly int octreeSize;

		private readonly int numOctreesPerBatch;

		private readonly Int3 arraySize;

		private readonly Array3<BatchOctrees> batches;

		private readonly string path;

		private readonly Int3.Bounds centerBounds;

		private Int3 centerBatch = Int3.negativeOne;

		private readonly List<IListener> listeners = new List<IListener>();

		private readonly PriorityQueue<BatchOctrees> loadingQueue = new PriorityQueue<BatchOctrees>(100);

		private readonly PriorityQueue<BatchOctrees> unloadingQueue = new PriorityQueue<BatchOctrees>(100);

		public IThread streamingThread { get; private set; }

		public WorkerThread ioThread { get; private set; }

		public BatchOctreesStreamer(IThread streamingThread, Int3.Bounds octreeBounds, int minLod, int maxLod, int batchSize, int numOctreesPerBatch, string path, Settings settings)
		{
			this.octreeBounds = octreeBounds;
			this.minLod = minLod;
			this.maxLod = maxLod;
			this.batchSize = batchSize;
			octreeSize = batchSize / numOctreesPerBatch;
			this.numOctreesPerBatch = numOctreesPerBatch;
			arraySize = new Int3(settings.batchesPerSide, settings.batchesVertically, settings.batchesPerSide);
			batches = new Array3<BatchOctrees>(arraySize.x, arraySize.y, arraySize.z);
			this.path = path;
			this.streamingThread = streamingThread;
			ioThread = ThreadUtils.StartWorkerThread("I/O", "BatchOctreesStreamerIO", System.Threading.ThreadPriority.Lowest, -2, 64);
			centerBounds = Int3.MinMax(settings.centerMin, settings.centerMax);
			foreach (Int3 item in Int3.Range(arraySize))
			{
				batches.Set(item, new BatchOctrees(this, item, numOctreesPerBatch, BatchOctreesAllocator.octreePool));
			}
		}

		public void Reset()
		{
			streamingThread = null;
			ioThread = null;
			listeners.Clear();
			foreach (BatchOctrees batch in batches)
			{
				batch.Clear();
			}
		}

		public bool IsRunning()
		{
			return ioThread.IsRunning();
		}

		public bool IsIdle()
		{
			if (ioThread.IsIdle())
			{
				return GetQueueLength() <= 0;
			}
			return false;
		}

		public void Stop()
		{
			ioThread.Stop();
		}

		public int GetQueueLength()
		{
			return loadingQueue.Count + unloadingQueue.Count;
		}

		public BatchOctrees GetBatch(Int3 id)
		{
			Int3 p = Int3.PositiveModulo(id, arraySize);
			BatchOctrees batchOctrees = batches.Get(p);
			if (!(batchOctrees.id == id))
			{
				return null;
			}
			return batchOctrees;
		}

		public bool IsRangeLoaded(Int3.Bounds range)
		{
			Int3 min = Int3.FloorDiv(range.mins, batchSize);
			Int3 max = Int3.FloorDiv(range.maxs, batchSize);
			foreach (Int3 item in Int3.MinMax(min, max))
			{
				BatchOctrees batch = GetBatch(item);
				if (batch == null || !batch.IsLoaded())
				{
					return false;
				}
			}
			return true;
		}

		public Octree GetOctree(Int3 id)
		{
			Int3 @int = Int3.FloorDiv(id, numOctreesPerBatch);
			BatchOctrees batch = GetBatch(@int);
			if (batch == null || !batch.IsLoaded())
			{
				return null;
			}
			Int3 int2 = @int * numOctreesPerBatch;
			return batch.GetOctree(id - int2);
		}

		public byte GetBlockType(Int3 block)
		{
			Int3 @int = Int3.FloorDiv(block, octreeSize);
			Octree octree = GetOctree(@int);
			if (octree == null || octree.IsEmpty())
			{
				return 0;
			}
			Int3 int2 = @int * octreeSize;
			Int3 coords = block - int2;
			int nodeId = octree.GetNodeId(coords, octreeSize);
			return octree.GetType(nodeId);
		}

		public bool UpdateCenter(Int3 position)
		{
			Int3 @int = centerBatch;
			Int3 p = Int3.FloorDiv(position, batchSize);
			p = centerBounds.Clamp(p);
			if (p == @int)
			{
				return false;
			}
			centerBatch = p;
			foreach (Int3 item in Int3.CenterSize(centerBatch, arraySize))
			{
				Int3 p2 = Int3.PositiveModulo(item, arraySize);
				BatchOctrees batchOctrees = batches.Get(p2);
				if (batchOctrees.id == item)
				{
					batchOctrees.Load();
				}
				else
				{
					batchOctrees.Reload(item);
				}
			}
			return true;
		}

		public void Unload()
		{
			foreach (BatchOctrees batch in batches)
			{
				batch.Unload();
			}
		}

		public void RegisterListener(IListener listener)
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

		public void DeregisterListener(IListener listener)
		{
			listeners.Remove(listener);
		}

		public void OnBatchLoaded(Int3 batchId)
		{
			Int3.Bounds blockRange = Int3.Bounds.FinerBounds(batchId, batchSize);
			foreach (IListener listener in listeners)
			{
				listener.OnRangeLoaded(this, blockRange, minLod, maxLod);
			}
		}

		public void OnBatchUnloaded(Int3 batchId)
		{
		}

		public void EnqueueForLoading(BatchOctrees batch)
		{
			loadingQueue.Enqueue(batch, CalculateLoadingPriority(centerBatch, batch.id));
		}

		public void EnqueueForUnloading(BatchOctrees batch)
		{
			Int3? reloadId = batch.reloadId;
			int priority = ((!reloadId.HasValue) ? CalculateUnloadingPriority(centerBatch, batch.id) : CalculateLoadingPriority(centerBatch, reloadId.Value));
			unloadingQueue.Enqueue(batch, priority);
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

		private bool ProcessLoadingQueue()
		{
			BatchOctrees result;
			while (loadingQueue.TryDequeue(out result))
			{
				if (result.BeginLoading())
				{
					return true;
				}
			}
			return false;
		}

		private bool ProcessUnloadingQueue()
		{
			BatchOctrees result;
			while (unloadingQueue.TryDequeue(out result))
			{
				if (IsProcessing(result.id))
				{
					EnqueueForUnloading(result);
					return false;
				}
				if (result.BeginUnloading())
				{
					return true;
				}
			}
			return false;
		}

		private bool IsProcessing(Int3 batchId)
		{
			return IsProcessingImpl(batchId);
		}

		private bool IsProcessingImpl(Int3 batchId)
		{
			Int3.Bounds blockRange = Int3.Bounds.FinerBounds(batchId, batchSize);
			foreach (IListener listener in listeners)
			{
				if (listener.IsProcessing(this, blockRange, minLod, maxLod))
				{
					return true;
				}
			}
			return false;
		}

		public string GetPath(Int3 batchId)
		{
			string path = $"compiled-batch-{batchId.x}-{batchId.y}-{batchId.z}.optoctrees";
			return Path.Combine(this.path, path);
		}

		public override string ToString()
		{
			return $"BatchOctreesStreamer (batchSize {batchSize}, arraySize {arraySize})";
		}

		private static int CalculateLoadingPriority(Int3 center, Int3 position)
		{
			return -Int3.SquareDistance(center, position);
		}

		private static int CalculateUnloadingPriority(Int3 center, Int3 position)
		{
			return -CalculateLoadingPriority(center, position);
		}

		public void OnGUI()
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Octrees", GUILayout.Width(60f));
			GUILayout.TextField(loadingQueue.Count.ToString(), GUILayout.Width(60f));
			GUILayout.TextField(unloadingQueue.Count.ToString(), GUILayout.Width(60f));
			GUILayout.EndHorizontal();
		}

		public void DrawGizmos(float alpha)
		{
			foreach (BatchOctrees batch in batches)
			{
				batch.DrawGizmos(alpha);
			}
		}
	}
}
