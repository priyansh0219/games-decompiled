using System.Collections;
using UWE;
using Unity.Jobs;
using UnityEngine;

namespace WorldStreaming
{
	public sealed class ClipmapCell
	{
		private enum State
		{
			Unloaded = 0,
			QueuedForLoading = 1,
			WaitingForOctrees = 2,
			BuildingMesh = 3,
			BuildingLayers = 4,
			Loaded = 5,
			Visible = 6,
			HiddenByParent = 7,
			HiddenByChildren = 8,
			QueuedForUnloading = 9,
			DestroyingChunk = 10,
			BuildingMeshToUnloading = 11,
			BuildingLayersToUnloading = 12
		}

		private struct BakePhysicsJob : IJob
		{
			private int meshId;

			public BakePhysicsJob(int meshId)
			{
				this.meshId = meshId;
			}

			public void Execute()
			{
				Physics.BakeMesh(meshId, convex: false);
			}
		}

		private State state;

		private ClipmapChunk chunk;

		private static readonly Task.Function BeginBuildMeshDelegate = BeginBuildMesh;

		private static readonly Task.Function EndBuildMeshDelegate = EndBuildMesh;

		private static readonly Task.Function BeginBuildLayersDelegate = BeginBuildLayers;

		private static readonly Task.Function EndBuildLayersDelegate = EndBuildLayers;

		private static readonly Task.Function BeginDestroyChunkDelegate = BeginDestroyChunk;

		private static readonly Task.Function EndDestroyChunkDelegate = EndDestroyChunk;

		private static readonly Task.Function FadeInMeshDelegate = FadeInMesh;

		private static readonly Task.Function ShowMeshDelegate = ShowMesh;

		private static readonly Task.Function HideMeshDelegate = HideMesh;

		public ClipmapLevel level { get; private set; }

		public Int3 id { get; private set; }

		public Int3? reloadId { get; private set; }

		private ClipmapStreamer streamer => level.streamer;

		public ClipmapCell(ClipmapLevel level, Int3 id)
		{
			this.level = level;
			this.id = id;
			state = State.Unloaded;
		}

		public void Clear()
		{
			level = null;
		}

		public void Load()
		{
			reloadId = null;
			switch (state)
			{
			case State.Unloaded:
				state = State.QueuedForLoading;
				level.EnqueueForLoading(this);
				break;
			case State.QueuedForUnloading:
				state = State.Loaded;
				OnLoaded();
				break;
			case State.DestroyingChunk:
				reloadId = id;
				break;
			case State.BuildingMeshToUnloading:
				state = State.BuildingMesh;
				break;
			case State.BuildingLayersToUnloading:
				state = State.BuildingLayers;
				break;
			default:
				Debug.LogErrorFormat("ClipmapCell.Load: Unhandled state {0}, cell {1}", state, this);
				break;
			case State.QueuedForLoading:
			case State.WaitingForOctrees:
			case State.BuildingMesh:
			case State.BuildingLayers:
			case State.Loaded:
			case State.Visible:
			case State.HiddenByParent:
			case State.HiddenByChildren:
				break;
			}
		}

		public void Unload()
		{
			reloadId = null;
			switch (state)
			{
			case State.Loaded:
			case State.HiddenByParent:
			case State.HiddenByChildren:
				state = State.QueuedForUnloading;
				level.EnqueueForUnloading(this);
				break;
			case State.Visible:
				HideMesh();
				state = State.QueuedForUnloading;
				level.EnqueueForVisibilityUpdate(id, loading: false);
				level.EnqueueForUnloading(this);
				break;
			case State.QueuedForLoading:
				state = State.Unloaded;
				break;
			case State.BuildingMesh:
				state = State.BuildingMeshToUnloading;
				break;
			case State.BuildingLayers:
				state = State.BuildingLayersToUnloading;
				break;
			case State.WaitingForOctrees:
				state = State.Unloaded;
				break;
			default:
				Debug.LogErrorFormat("ClipmapCell.Unload: Unhandled state {0}, cell {1}", state, this);
				break;
			case State.Unloaded:
			case State.QueuedForUnloading:
			case State.DestroyingChunk:
			case State.BuildingMeshToUnloading:
			case State.BuildingLayersToUnloading:
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
				level.EnqueueForLoading(this);
				break;
			case State.QueuedForLoading:
				id = newId;
				reloadId = null;
				break;
			case State.Visible:
				HideMesh();
				state = State.QueuedForUnloading;
				level.EnqueueForVisibilityUpdate(id, loading: false);
				level.EnqueueForUnloading(this);
				break;
			case State.Loaded:
			case State.HiddenByParent:
			case State.HiddenByChildren:
				state = State.QueuedForUnloading;
				level.EnqueueForUnloading(this);
				break;
			case State.BuildingMesh:
				state = State.BuildingMeshToUnloading;
				break;
			case State.BuildingLayers:
				reloadId = newId;
				state = State.BuildingLayersToUnloading;
				break;
			case State.WaitingForOctrees:
				id = newId;
				reloadId = null;
				state = State.QueuedForLoading;
				level.EnqueueForLoading(this);
				break;
			default:
				Debug.LogErrorFormat("ClipmapCell.Reload: Unhandled state {0}, cell {1}", state, this);
				break;
			case State.QueuedForUnloading:
			case State.DestroyingChunk:
			case State.BuildingMeshToUnloading:
			case State.BuildingLayersToUnloading:
				break;
			}
		}

		public bool BeginLoading()
		{
			if (state != State.QueuedForLoading)
			{
				return false;
			}
			state = State.WaitingForOctrees;
			BatchOctreesStreamer octreesStreamer = streamer.host.GetOctreesStreamer(level.id);
			OnBatchOctreesChanged(octreesStreamer);
			return true;
		}

		public void OnBatchOctreesChanged(BatchOctreesStreamer streamer)
		{
			if (state == State.WaitingForOctrees)
			{
				Int3 @int = id * level.cellSize - (3 << level.id);
				Int3 max = @int + level.cellSize + ((6 << level.id) - 1);
				if (streamer.IsRangeLoaded(Int3.MinMax(@int, max)))
				{
					OnBatchOctreesReady();
				}
			}
		}

		public bool IsProcessing()
		{
			State state = this.state;
			if (state == State.BuildingMesh || state == State.BuildingMeshToUnloading)
			{
				return true;
			}
			return false;
		}

		private bool OnBatchOctreesReady()
		{
			state = State.BuildingMesh;
			streamer.meshingThreads.Enqueue(BeginBuildMeshDelegate, this, null);
			return true;
		}

		private static void BeginBuildMesh(object owner, object state)
		{
			((ClipmapCell)owner).BeginBuildMesh();
		}

		private void BeginBuildMesh()
		{
			MeshBuilder meshBuilder = streamer.meshBuilderPool.Get();
			BatchOctreesStreamer octreesStreamer = streamer.host.GetOctreesStreamer(level.id);
			meshBuilder.Reset(level.id, id, level.cellSize, level.settings, level.streamer.host.blockTypes);
			meshBuilder.DoThreadablePart(octreesStreamer, streamer.settings.collision);
			streamer.streamingThread.Enqueue(EndBuildMeshDelegate, this, meshBuilder);
		}

		private static void EndBuildMesh(object owner, object state)
		{
			ClipmapCell obj = (ClipmapCell)owner;
			MeshBuilder meshBuilder = (MeshBuilder)state;
			obj.EndBuildMesh(meshBuilder);
		}

		private void EndBuildMesh(MeshBuilder meshBuilder)
		{
			bool flag = state == State.BuildingMeshToUnloading;
			state = (flag ? State.BuildingLayersToUnloading : State.BuildingLayers);
			streamer.buildLayersThread.Enqueue(BeginBuildLayersDelegate, this, meshBuilder);
		}

		private static void BeginBuildLayers(object owner, object state)
		{
			ClipmapCell obj = (ClipmapCell)owner;
			MeshBuilder meshBuilder = (MeshBuilder)state;
			obj.BeginBuildLayers(meshBuilder);
		}

		private void BeginBuildLayers(MeshBuilder meshBuilder)
		{
			CoroutineHost.StartCoroutine(BeginBuildLayersAsync(meshBuilder));
		}

		private IEnumerator BeginBuildLayersAsync(MeshBuilder meshBuilder)
		{
			if (streamer != null && streamer.host != null)
			{
				WorldStreamer host = streamer.host;
				chunk = meshBuilder.DoFinalizePart(host.chunkRoot, host.terrainPoolManager);
				streamer.meshBuilderPool.Return(meshBuilder);
				yield return ActivateChunkAndCollider(chunk);
				streamer.OnCellLoaded(level, id);
			}
			streamer.streamingThread.Enqueue(EndBuildLayersDelegate, this, null);
		}

		private IEnumerator ActivateChunkAndCollider(ClipmapChunk chunk)
		{
			if (chunk != null)
			{
				JobHandle bakeCollidersJobHandle = BeginBakingCollidersIfNecessary(chunk);
				yield return null;
				chunk.gameObject.SetActive(value: true);
				yield return null;
				FinalizeCollidersIfNecessary(chunk, bakeCollidersJobHandle);
			}
		}

		private JobHandle BeginBakingCollidersIfNecessary(ClipmapChunk chunk)
		{
			if (chunk.collision != null && chunk.collision.sharedMesh != null)
			{
				return new BakePhysicsJob(chunk.collision.sharedMesh.GetInstanceID()).Schedule();
			}
			return default(JobHandle);
		}

		private void FinalizeCollidersIfNecessary(ClipmapChunk chunk, JobHandle jobHandle)
		{
			if (chunk.collision != null && chunk.collision.sharedMesh != null)
			{
				jobHandle.Complete();
				chunk.collision.gameObject.SetActive(value: true);
				chunk.collision.sharedMesh.Clear();
			}
		}

		private static void EndBuildLayers(object owner, object state)
		{
			((ClipmapCell)owner).EndBuildLayers();
		}

		private void EndBuildLayers()
		{
			if (state == State.BuildingLayersToUnloading)
			{
				state = State.QueuedForUnloading;
				level.EnqueueForUnloading(this);
			}
			else
			{
				state = State.Loaded;
				OnLoaded();
			}
		}

		private void OnLoaded()
		{
			level.EnqueueForVisibilityUpdate(id, loading: true);
		}

		public bool BeginUnloading()
		{
			if (state != State.QueuedForUnloading)
			{
				return false;
			}
			state = State.DestroyingChunk;
			streamer.destroyChunksThread.Enqueue(BeginDestroyChunkDelegate, this, null);
			return true;
		}

		private static void BeginDestroyChunk(object owner, object state)
		{
			((ClipmapCell)owner).BeginDestroyChunk();
		}

		private void BeginDestroyChunk()
		{
			streamer.OnCellUnloaded(level, id);
			if ((bool)chunk)
			{
				if (!streamer.host.terrainPoolManager.meshPoolingEnabled)
				{
					MeshBuilder.DestroyMeshes(chunk);
				}
				ReturnChunkToPool(chunk);
			}
			streamer.streamingThread.Enqueue(EndDestroyChunkDelegate, this, null);
		}

		private void ReturnChunkToPool(ClipmapChunk chunk)
		{
			TerrainPoolManager terrainPoolManager = streamer.host.terrainPoolManager;
			foreach (TerrainChunkPiece chunkPiece in chunk.chunkPieces)
			{
				terrainPoolManager.Return(chunkPiece);
			}
			terrainPoolManager.Return(chunk);
		}

		private static void EndDestroyChunk(object owner, object state)
		{
			((ClipmapCell)owner).EndDestroyChunk();
		}

		private void EndDestroyChunk()
		{
			state = State.Unloaded;
			if (reloadId.HasValue)
			{
				id = reloadId.Value;
				reloadId = null;
				state = State.QueuedForLoading;
				level.EnqueueForLoading(this);
			}
		}

		private void FadeInMesh()
		{
			streamer.toggleChunksThread.Enqueue(FadeInMeshDelegate, this, null);
		}

		private void ShowMesh()
		{
			streamer.toggleChunksThread.Enqueue(ShowMeshDelegate, this, null);
		}

		private void HideMesh()
		{
			streamer.toggleChunksThread.Enqueue(HideMeshDelegate, this, null);
		}

		private static void FadeInMesh(object owner, object state)
		{
			ClipmapChunk clipmapChunk = ((ClipmapCell)owner).chunk;
			if ((bool)clipmapChunk)
			{
				clipmapChunk.FadeIn();
			}
		}

		private static void ShowMesh(object owner, object state)
		{
			ClipmapChunk clipmapChunk = ((ClipmapCell)owner).chunk;
			if ((bool)clipmapChunk)
			{
				clipmapChunk.Show();
			}
		}

		private static void HideMesh(object owner, object state)
		{
			ClipmapCell clipmapCell = (ClipmapCell)owner;
			ClipmapChunk clipmapChunk = clipmapCell.chunk;
			if ((bool)clipmapChunk)
			{
				clipmapChunk.Hide(clipmapCell.level.settings.keepGrassVisible);
			}
		}

		public void Show()
		{
			switch (state)
			{
			case State.Loaded:
			case State.HiddenByParent:
				state = State.Visible;
				FadeInMesh();
				break;
			case State.HiddenByChildren:
				state = State.Visible;
				ShowMesh();
				break;
			case State.Visible:
				break;
			}
		}

		public bool HideByParent()
		{
			switch (state)
			{
			case State.Visible:
				state = State.HiddenByParent;
				HideMesh();
				return false;
			case State.HiddenByParent:
				return false;
			case State.Loaded:
			case State.HiddenByChildren:
				state = State.HiddenByParent;
				return true;
			default:
				return true;
			}
		}

		public bool HideByChildren()
		{
			switch (state)
			{
			case State.Visible:
				state = State.HiddenByChildren;
				HideMesh();
				return true;
			case State.HiddenByChildren:
				return false;
			case State.Loaded:
			case State.HiddenByParent:
				state = State.HiddenByChildren;
				return true;
			default:
				return true;
			}
		}

		public bool IsLoaded()
		{
			State state = this.state;
			if ((uint)(state - 5) <= 3u)
			{
				return true;
			}
			return false;
		}

		public override string ToString()
		{
			return $"ClipmapCell (level {level}, id {id}, state {state}, reloadId {reloadId})";
		}

		public void DrawGizmos()
		{
			Color color = Color.white;
			switch (state)
			{
			case State.Unloaded:
				color = Color.black;
				break;
			case State.QueuedForLoading:
			case State.WaitingForOctrees:
				color = Color.red;
				break;
			case State.BuildingMesh:
			case State.BuildingLayers:
			case State.BuildingMeshToUnloading:
			case State.BuildingLayersToUnloading:
				color = Color.yellow;
				break;
			case State.Loaded:
				color = Color.blue;
				break;
			case State.Visible:
				color = Color.green;
				break;
			case State.HiddenByParent:
			case State.HiddenByChildren:
				color = Color.gray;
				break;
			case State.QueuedForUnloading:
			case State.DestroyingChunk:
				color = Color.cyan;
				break;
			}
			Gizmos.color = color;
			int cellSize = level.cellSize;
			Gizmos.DrawWireCube(size: (Vector3)new Int3(cellSize - 4 + level.id), center: (Vector3)(id * cellSize + cellSize / 2));
		}

		public static bool IsLoaded(ClipmapCell cell)
		{
			return cell?.IsLoaded() ?? false;
		}

		public static Int3.Bounds GetChildIds(Int3 cellId)
		{
			Int3 @int = cellId << 1;
			Int3 max = @int + 1;
			return Int3.MinMax(@int, max);
		}
	}
}
