using System;
using System.IO;
using UWE;
using UnityEngine;

namespace WorldStreaming
{
	public sealed class BatchOctrees
	{
		private enum State
		{
			Unloaded = 0,
			QueuedForLoading = 1,
			Loading = 2,
			LoadingToUnloading = 3,
			Loaded = 4,
			QueuedForUnloading = 5,
			Unloading = 6
		}

		private BatchOctreesStreamer streamer;

		private SplitNativeArrayPool<byte> allocator;

		private readonly int arraySize;

		private readonly Array3<Octree> octrees;

		private State state;

		private static readonly Task.Function BeginLoadOctreesDelegate = BeginLoadOctrees;

		private static readonly Task.Function EndLoadOctreesDelegate = EndLoadOctrees;

		private static readonly Task.Function BeginUnloadOctreesDelegate = BeginUnloadOctrees;

		private static readonly Task.Function EndUnloadOctreesDelegate = EndUnloadOctrees;

		public Int3 id { get; private set; }

		public Int3? reloadId { get; private set; }

		public BatchOctrees(BatchOctreesStreamer streamer, Int3 id, int arraySize, SplitNativeArrayPool<byte> pooledAllocator)
		{
			allocator = pooledAllocator;
			this.streamer = streamer;
			this.id = id;
			state = State.Unloaded;
			this.arraySize = arraySize;
			octrees = new Array3<Octree>(arraySize);
			foreach (Int3 item in Int3.Range(arraySize))
			{
				octrees.Set(item, new Octree(id));
			}
		}

		public void Clear()
		{
			ClearOctrees();
			streamer = null;
			allocator = null;
		}

		public Octree GetOctree(Int3 treeId)
		{
			return octrees.Get(treeId);
		}

		public bool IsLoaded()
		{
			State state = this.state;
			if ((uint)(state - 4) <= 1u)
			{
				return true;
			}
			return false;
		}

		public void Load()
		{
			reloadId = null;
			switch (state)
			{
			case State.Unloaded:
				state = State.QueuedForLoading;
				streamer.EnqueueForLoading(this);
				break;
			case State.QueuedForUnloading:
				state = State.Loaded;
				OnLoaded();
				break;
			case State.LoadingToUnloading:
				state = State.Loading;
				break;
			case State.Unloading:
				reloadId = id;
				break;
			default:
				Debug.LogErrorFormat("BatchOctrees.Load: Unhandled state {0}, cell {1}", state, this);
				break;
			case State.QueuedForLoading:
			case State.Loading:
			case State.Loaded:
				break;
			}
		}

		public void Unload()
		{
			reloadId = null;
			switch (state)
			{
			case State.Loaded:
				state = State.QueuedForUnloading;
				streamer.EnqueueForUnloading(this);
				break;
			case State.QueuedForLoading:
				state = State.Unloaded;
				break;
			case State.Loading:
				state = State.LoadingToUnloading;
				break;
			default:
				Debug.LogErrorFormat("BatchOctrees.Unload: Unhandled state {0}, cell {1}", state, this);
				break;
			case State.Unloaded:
			case State.LoadingToUnloading:
			case State.QueuedForUnloading:
			case State.Unloading:
				break;
			}
		}

		public void Reload(Int3 newId)
		{
			reloadId = newId;
			switch (state)
			{
			case State.Unloaded:
				id = newId;
				reloadId = null;
				state = State.QueuedForLoading;
				streamer.EnqueueForLoading(this);
				break;
			case State.QueuedForLoading:
				id = newId;
				reloadId = null;
				break;
			case State.Loaded:
				state = State.QueuedForUnloading;
				streamer.EnqueueForUnloading(this);
				break;
			case State.Loading:
				state = State.LoadingToUnloading;
				break;
			default:
				Debug.LogErrorFormat("BatchOctrees.Reload: Unhandled state {0}, cell {1}", state, this);
				break;
			case State.LoadingToUnloading:
			case State.QueuedForUnloading:
			case State.Unloading:
				break;
			}
		}

		private void OnLoaded()
		{
			streamer.OnBatchLoaded(id);
		}

		public bool BeginLoading()
		{
			if (state != State.QueuedForLoading)
			{
				return false;
			}
			state = State.Loading;
			streamer.ioThread.Enqueue(BeginLoadOctreesDelegate, this, null);
			return true;
		}

		private static void BeginLoadOctrees(object owner, object state)
		{
			((BatchOctrees)owner).BeginLoadOctrees();
		}

		private void BeginLoadOctrees()
		{
			if (!LoadOctrees())
			{
				ClearOctrees();
			}
			streamer.streamingThread.Enqueue(EndLoadOctreesDelegate, this, null);
		}

		private static void EndLoadOctrees(object owner, object state)
		{
			((BatchOctrees)owner).EndLoading();
		}

		private void EndLoading()
		{
			state = State.Loaded;
			OnLoaded();
		}

		public bool BeginUnloading()
		{
			if (state != State.QueuedForUnloading)
			{
				return false;
			}
			state = State.Unloading;
			streamer.ioThread.Enqueue(BeginUnloadOctreesDelegate, this, null);
			return true;
		}

		private static void BeginUnloadOctrees(object owner, object state)
		{
			((BatchOctrees)owner).BeginUnloadOctrees();
		}

		private void BeginUnloadOctrees()
		{
			ClearOctrees();
			streamer.streamingThread.Enqueue(EndUnloadOctreesDelegate, this, null);
		}

		private static void EndUnloadOctrees(object owner, object state)
		{
			((BatchOctrees)owner).EndUnloadOctrees();
		}

		private void EndUnloadOctrees()
		{
			streamer.OnBatchUnloaded(id);
			state = State.Unloaded;
			if (reloadId.HasValue)
			{
				id = reloadId.Value;
				reloadId = null;
				state = State.QueuedForLoading;
				streamer.EnqueueForLoading(this);
			}
		}

		private void ClearOctrees()
		{
			foreach (Octree octree in octrees)
			{
				octree.Clear(allocator);
			}
		}

		private bool LoadOctrees()
		{
			string path = streamer.GetPath(id);
			if (!File.Exists(path))
			{
				return false;
			}
			try
			{
				using (PooledBinaryReader reader = new PooledBinaryReader(File.OpenRead(path)))
				{
					if (reader.ReadInt32() < 4)
					{
						return false;
					}
					Int3 @int = id * arraySize;
					foreach (Int3 item in Int3.Range(arraySize))
					{
						Int3 p = item + @int;
						if (streamer.octreeBounds.Contains(p))
						{
							Octree octree = octrees.Get(item);
							octree.Read(reader, id, allocator);
							octree.UnloadChildren(streamer.minLod, allocator);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("Exception while loading octrees, batch {0}, path '{1}', exception {2}", id, path, ex);
				return false;
			}
			return true;
		}

		public override string ToString()
		{
			return $"BatchOctrees (id {id}, state {state}, reloadId {reloadId})";
		}

		public void DrawGizmos(float alpha)
		{
			Color c = Color.white;
			switch (state)
			{
			case State.Unloaded:
				c = Color.black;
				break;
			case State.QueuedForLoading:
				c = Color.red;
				break;
			case State.Loading:
				c = Color.yellow;
				break;
			case State.Loaded:
				c = Color.green;
				break;
			case State.QueuedForUnloading:
				c = Color.cyan;
				break;
			case State.Unloading:
				c = Color.blue;
				break;
			}
			Gizmos.color = c.ToAlpha(alpha);
			int batchSize = streamer.batchSize;
			Gizmos.DrawWireCube(size: new Vector3(batchSize, batchSize, batchSize), center: (Vector3)(id * batchSize + batchSize / 2));
		}
	}
}
