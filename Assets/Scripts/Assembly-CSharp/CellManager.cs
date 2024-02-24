using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gendarme;
using Platform.IO;
using ProtoBuf;
using UWE;
using UnityEngine;

public sealed class CellManager : ClipMapManager.IClipMapEventHandler
{
	private sealed class WaitWhileProcessingOperation : IAsyncOperation
	{
		private readonly CellManager manager;

		public bool isDone => !manager.IsProcessing();

		public WaitWhileProcessingOperation(CellManager manager)
		{
			this.manager = manager;
		}
	}

	public sealed class UpdateCellManagementCoroutine : StateMachineBase<CellManager>
	{
		private ProtobufSerializer serializer;

		public void Initialize(ProtobufSerializer _serializer)
		{
			serializer = _serializer;
		}

		public override bool MoveNext()
		{
			if (host.cellManagementQueue.Count > 0 && !host.IsFrozen() && !IngameMenu.IsQuitting())
			{
				EntityCell entityCell = host.cellManagementQueue.Dequeue();
				entityCell.OnDequeue();
				_ = host.processingCell;
				host.processingCell = entityCell;
				current = entityCell.Proceed(serializer);
				return true;
			}
			host.processingCell = null;
			current = null;
			return false;
		}

		public override void Reset()
		{
			serializer = null;
		}
	}

	[ProtoContract]
	public sealed class CellsFileHeader
	{
		[ProtoMember(1)]
		public int version;

		[ProtoMember(2)]
		public int numCells;

		public override string ToString()
		{
			return $"(version={version}, numCells={numCells})";
		}
	}

	[ProtoContract]
	public sealed class CellHeader
	{
		[ProtoMember(1)]
		public Int3 cellId;

		[ProtoMember(2)]
		public int level;

		public override string ToString()
		{
			return $"(cellId={cellId}, level={level})";
		}
	}

	[ProtoContract]
	public sealed class CellHeaderEx
	{
		[ProtoMember(1)]
		public Int3 cellId;

		[ProtoMember(2)]
		public int level;

		[ProtoMember(3)]
		public int dataLength;

		[ProtoMember(4)]
		public int legacyDataLength;

		[ProtoMember(5)]
		public int waiterDataLength;

		[ProtoMember(6)]
		public bool allowSpawnRestrictions;

		public override string ToString()
		{
			return $"(cellId={cellId}, level={level}, dataLength={dataLength}, legacyDataLength={legacyDataLength}, waiterDataLength={waiterDataLength}, allowSpawnRestrictions={allowSpawnRestrictions})";
		}
	}

	public const string CacheFolder = "CellsCache";

	private const int BatchCellsVersion = 10;

	[NonSerialized]
	private readonly LargeWorldStreamer streamer;

	private readonly Dictionary<Int3, BatchCells> batch2cells = new Dictionary<Int3, BatchCells>(Int3.equalityComparer);

	internal readonly DynamicPriorityQueue<EntityCell> cellManagementQueue = new DynamicPriorityQueue<EntityCell>();

	private EntityCell processingCell;

	private readonly AsyncAwaiter processingAwaiter;

	[NonSerialized]
	private int freezeCount;

	[NonSerialized]
	public readonly LargeWorldEntitySpawner spawner;

	private static readonly StateMachinePool<UpdateCellManagementCoroutine, CellManager> updateCellManagementCoroutines = new StateMachinePool<UpdateCellManagementCoroutine, CellManager>();

	public bool AbortRequested { get; private set; }

	public CellManager(LargeWorldStreamer streamer, LargeWorldEntitySpawner spawner)
	{
		this.streamer = streamer;
		this.spawner = spawner;
		processingAwaiter = new AsyncAwaiter(new WaitWhileProcessingOperation(this));
	}

	public void EntStats()
	{
		Timer.Begin("entstats");
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (KeyValuePair<Int3, BatchCells> batch2cell in batch2cells)
		{
			foreach (EntityCell item in batch2cell.Value.All())
			{
				if (!(item.liveRoot != null))
				{
					continue;
				}
				foreach (Transform item2 in item.liveRoot.transform)
				{
					string name = item2.gameObject.name;
					int orDefault = dictionary.GetOrDefault(name, 0);
					dictionary[name] = orDefault + 1;
				}
			}
		}
		int num = 0;
		foreach (KeyValuePair<string, int> item3 in dictionary)
		{
			Debug.LogFormat("{0} --> {1}", item3.Key, item3.Value);
			num += item3.Value;
		}
		Debug.LogFormat("Total ents: {0}", num);
		Timer.End();
	}

	public bool IsProcessing()
	{
		return processingCell != null;
	}

	public bool IsIdle()
	{
		if (!IsProcessing())
		{
			if (cellManagementQueue.Count != 0)
			{
				return IngameMenu.IsQuitting();
			}
			return true;
		}
		return false;
	}

	public int GetQueueLength()
	{
		return cellManagementQueue.Count;
	}

	private bool IsFrozen()
	{
		return freezeCount > 0;
	}

	public int GetFreezeCount()
	{
		return freezeCount;
	}

	public IEnumerator IncreaseFreezeCount()
	{
		freezeCount++;
		return processingAwaiter;
	}

	public void DecreaseFreezeCount()
	{
		freezeCount--;
	}

	public void RequestAbort()
	{
		AbortRequested = true;
		freezeCount++;
		if (processingCell != null)
		{
			processingCell.RequestAbort();
			processingCell = null;
		}
	}

	public bool AreCellsLoaded(Bounds bounds, LargeWorldEntity.CellLevel level)
	{
		Int3.Bounds bounds2 = Int3.MinMax(streamer.GetBlock(bounds.min), streamer.GetBlock(bounds.max));
		foreach (Int3 item in Int3.Bounds.OuterCoarserBounds(bounds2, streamer.blocksPerBatch))
		{
			if (streamer.CheckBatch(item))
			{
				if (!batch2cells.TryGetValue(item, out var value))
				{
					return false;
				}
				Int3 @int = item * streamer.blocksPerBatch;
				Int3.Bounds bsRange = (bounds2 - @int).Clamp(Int3.zero, streamer.blocksPerBatch - 1);
				if (!value.AreCellsAwake(bsRange, (int)level))
				{
					return false;
				}
			}
		}
		return true;
	}

	public IEnumerator UpdateCellManagement(ProtobufSerializer serializer)
	{
		PooledStateMachine<UpdateCellManagementCoroutine> pooledStateMachine = updateCellManagementCoroutines.Get(this);
		pooledStateMachine.stateMachine.Initialize(serializer);
		return pooledStateMachine;
	}

	public static string GetSplitBatchCellsPath(string prefix, string directory, Int3 index, string suffix)
	{
		return Platform.IO.Path.Combine(Platform.IO.Path.Combine(prefix, directory), $"batch-cells-{index.x}-{index.y}-{index.z}-{suffix}.bin");
	}

	public static string GetCacheBatchCellsPath(string prefix, Int3 index)
	{
		return Platform.IO.Path.Combine(Platform.IO.Path.Combine(prefix, "CellsCache"), $"baked-batch-cells-{index.x}-{index.y}-{index.z}.bin");
	}

	public void UnregisterEntity(GameObject go)
	{
		LargeWorldEntity component = go.GetComponent<LargeWorldEntity>();
		if (!component)
		{
			Debug.LogWarningFormat(go, "UnregisterEntity called on a non-streamed entity {0}", go.GetFullHierarchyPath());
		}
		else
		{
			UnregisterEntity(component);
		}
	}

	public void UnregisterEntity(LargeWorldEntity ent)
	{
		UnregisterCellEntity(ent, checkParent: true);
	}

	private void UnregisterCellEntity(LargeWorldEntity ent, bool checkParent)
	{
		ent.enabled = false;
	}

	public void RegisterEntity(GameObject ent)
	{
		if (!ent)
		{
			Debug.LogErrorFormat(ent, "RegisterEntity called on a destroyed entity '{0}'. Ignoring.", ent);
		}
		else
		{
			LargeWorldEntity lwe = ent.EnsureComponent<LargeWorldEntity>();
			RegisterEntity(lwe);
		}
	}

	public bool RegisterEntity(LargeWorldEntity lwe)
	{
		switch (lwe.cellLevel)
		{
		case LargeWorldEntity.CellLevel.Global:
			UnregisterCellEntity(lwe, checkParent: false);
			RegisterGlobalEntity(lwe.gameObject);
			return true;
		case LargeWorldEntity.CellLevel.Batch:
			UnregisterCellEntity(lwe, checkParent: false);
			return RegisterBatchEntity(lwe.gameObject);
		default:
			return RegisterCellEntity(lwe);
		}
	}

	private bool RegisterCellEntity(LargeWorldEntity ent)
	{
		ent.enabled = true;
		Vector3 position = ent.transform.position;
		Int3 block = streamer.GetBlock(position);
		Int3 key = block / streamer.blocksPerBatch;
		Int3 @int = block % streamer.blocksPerBatch;
		int cellLevel = (int)ent.cellLevel;
		bool result = false;
		if (batch2cells.TryGetValue(key, out var value))
		{
			Int3 cellSize = BatchCells.GetCellSize(cellLevel, streamer.blocksPerBatch);
			Int3 cellId = @int / cellSize;
			result = value.EnsureCell(cellId, cellLevel).AddEntity(ent);
		}
		return result;
	}

	private bool RegisterBatchEntity(GameObject ent)
	{
		Int3 containingBatch = streamer.GetContainingBatch(ent.transform.position);
		if (streamer.batch2root.TryGetValue(containingBatch, out var value))
		{
			ent.transform.parent = value.transform;
			return true;
		}
		Debug.LogErrorFormat(ent, "Trying to register batch entity '{0}' to batch '{1}' which is not loaded.", ent.name, containingBatch);
		return false;
	}

	private void RegisterGlobalEntity(GameObject ent)
	{
		ent.transform.parent = streamer.globalRoot.transform;
	}

	public void OnEntityMoved(UniqueIdentifier ent)
	{
		RegisterEntity(ent.gameObject);
	}

	private void SaveCacheBatchCells(BatchCells cells, string targetPathPrefix, bool skipEmpty)
	{
		List<EntityCell> cells2 = cells.All().ToList();
		CoroutineUtils.PumpCoroutine(SaveCacheBatchCellsPhase1(cells2));
		SaveCacheBatchCellsPhase2Threaded(cells.batch, cells2, targetPathPrefix, skipEmpty);
	}

	private IEnumerator SaveCacheBatchCellsPhase1(ICollection<EntityCell> cells)
	{
		using (PooledObject<ProtobufSerializer> serializerProxy = ProtobufSerializerPool.GetProxy())
		{
			foreach (EntityCell cell in cells)
			{
				yield return cell.EnsureSerialDataSerialized(serializerProxy);
				yield return cell.EnsureWaiterDataSerialized(serializerProxy);
			}
		}
	}

	private void SaveCacheBatchCellsPhase2Threaded(Int3 batchId, ICollection<EntityCell> cells, string targetPathPrefix, bool skipEmpty)
	{
		_ = StopwatchProfiler.Instance;
		string path = Platform.IO.Path.Combine(targetPathPrefix, "CellsCache");
		try
		{
			Platform.IO.Directory.CreateDirectory(path);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, streamer);
			return;
		}
		finally
		{
		}
		string cacheBatchCellsPath = GetCacheBatchCellsPath(targetPathPrefix, batchId);
		int count = cells.Count;
		if (skipEmpty && count == 0 && !Platform.IO.File.Exists(cacheBatchCellsPath))
		{
			return;
		}
		using (PooledObject<ProtobufSerializer> pooledObject = ProtobufSerializerPool.GetProxy())
		{
			using (Stream stream = FileUtils.CreateFile(cacheBatchCellsPath))
			{
				CellsFileHeader cellsFileHeader = new CellsFileHeader();
				cellsFileHeader.version = 10;
				cellsFileHeader.numCells = count;
				pooledObject.Value.Serialize(stream, cellsFileHeader);
				CellHeaderEx cellHeaderEx = new CellHeaderEx();
				foreach (EntityCell cell in cells)
				{
					SerialData serialData = cell.GetSerialData();
					SerialData legacyData = cell.GetLegacyData();
					SerialData waiterData = cell.GetWaiterData();
					cellHeaderEx.cellId = cell.CellId;
					cellHeaderEx.level = cell.Level;
					cellHeaderEx.dataLength = serialData.Length;
					cellHeaderEx.legacyDataLength = legacyData.Length;
					cellHeaderEx.waiterDataLength = waiterData.Length;
					cellHeaderEx.allowSpawnRestrictions = cell.AllowSpawnRestrictions;
					pooledObject.Value.Serialize(stream, cellHeaderEx);
					stream.Write(serialData.Data.Array, serialData.Data.Offset, serialData.Length);
					stream.Write(legacyData.Data.Array, legacyData.Data.Offset, legacyData.Length);
					stream.Write(waiterData.Data.Array, waiterData.Data.Offset, waiterData.Length);
					cell.ClearTempSerialData();
				}
			}
		}
	}

	public bool TryLoadCacheBatchCells(BatchCells cells)
	{
		string cacheBatchCellsPath = GetCacheBatchCellsPath(streamer.tmpPathPrefix, cells.batch);
		string cacheBatchCellsPath2 = GetCacheBatchCellsPath(streamer.pathPrefix, cells.batch);
		int chosenFile;
		using (Stream stream = UWE.Utils.TryOpenEither(out chosenFile, cacheBatchCellsPath, cacheBatchCellsPath2))
		{
			if (stream == null)
			{
				return false;
			}
			bool allowSpawnRestrictions = chosenFile > 0;
			LoadCacheBatchCellsFromStream(cells, stream, allowSpawnRestrictions);
		}
		return true;
	}

	public static void LoadCacheBatchCellsFromStream(BatchCells cells, Stream stream, bool allowSpawnRestrictions)
	{
		using (PooledObject<ProtobufSerializer> pooledObject = ProtobufSerializerPool.GetProxy())
		{
			CellsFileHeader cellsFileHeader = new CellsFileHeader();
			pooledObject.Value.Deserialize(stream, cellsFileHeader, verbose: false);
			CellHeaderEx cellHeaderEx = new CellHeaderEx();
			for (int i = 0; i < cellsFileHeader.numCells; i++)
			{
				pooledObject.Value.Deserialize(stream, cellHeaderEx, verbose: false);
				Int3 cellId = BatchCells.GetCellId(cellHeaderEx.cellId, cellHeaderEx.level, cellsFileHeader.version);
				EntityCell entityCell = cells.Add(cellId, cellHeaderEx.level);
				entityCell.Initialize();
				entityCell.AllowSpawnRestrictions = allowSpawnRestrictions || cellHeaderEx.allowSpawnRestrictions;
				entityCell.ReadSerialDataFromStream(stream, cellHeaderEx.dataLength);
				if (cellsFileHeader.version < 9)
				{
					if (cellHeaderEx.level > 1)
					{
						entityCell.MoveSerialDataToLegacy();
					}
				}
				else
				{
					entityCell.ReadLegacyDataFromStream(stream, cellHeaderEx.legacyDataLength);
					entityCell.ReadWaiterDataFromStream(stream, cellHeaderEx.waiterDataLength);
				}
			}
		}
	}

	public BatchCells InitializeBatchCells(Int3 index)
	{
		if (batch2cells.ContainsKey(index))
		{
			Debug.LogWarningFormat("BatchCells {0} already loaded. Reloading...", index);
			UnloadBatchCells(index);
		}
		BatchCells fromPool = BatchCells.GetFromPool(this, streamer, index);
		batch2cells[index] = fromPool;
		return fromPool;
	}

	public IEnumerator LoadBatchCellsThreadedAsync(BatchCells cells, bool editMode)
	{
		if (!editMode)
		{
			TryLoadCacheBatchCells(cells);
			yield return null;
		}
	}

	public bool IsProcessingBatchCells(Int3 index)
	{
		if (processingCell != null)
		{
			return processingCell.BatchId == index;
		}
		return false;
	}

	public IEnumerator SaveBatchCellsTmpAsync(Int3 index)
	{
		if (!batch2cells.TryGetValue(index, out var cells))
		{
			yield break;
		}
		cells.RemoveEmpty();
		List<EntityCell> allCells = cells.All().ToList();
		using (PooledObject<ProtobufSerializer> serializerProxy = ProtobufSerializerPool.GetProxy())
		{
			foreach (EntityCell item in allCells)
			{
				yield return item.EnsureSleepAsync(serializerProxy);
			}
		}
		yield return SaveCacheBatchCellsPhase1(allCells);
		yield return WorkerTask.Launch(delegate
		{
			SaveCacheBatchCellsPhase2Threaded(cells.batch, allCells, streamer.tmpPathPrefix, skipEmpty: false);
		});
	}

	public void UnloadBatchCells(Int3 index)
	{
		if (batch2cells.TryGetValue(index, out var value))
		{
			BatchCells.ReturnToPool(value);
			batch2cells.Remove(index);
		}
	}

	public void ShowEntities(Int3.Bounds blockRange)
	{
	}

	public void HideEntities(Int3.Bounds blockRange)
	{
	}

	public void ShowEntities(Int3.Bounds blockRange, int level)
	{
		if (level > 3 || !streamer.IsReady())
		{
			return;
		}
		foreach (Int3 item in Int3.Bounds.OuterCoarserBounds(blockRange, streamer.blocksPerBatch))
		{
			if (batch2cells.TryGetValue(item, out var value))
			{
				Int3 @int = item * streamer.blocksPerBatch;
				Int3.Bounds bsRange = (blockRange - @int).Clamp(Int3.zero, streamer.blocksPerBatch - 1);
				value.QueueForAwake(bsRange, level, cellManagementQueue);
			}
		}
	}

	public void HideEntities(Int3.Bounds blockRange, int level)
	{
		if (level > 3)
		{
			return;
		}
		foreach (Int3 item in Int3.Bounds.OuterCoarserBounds(blockRange, streamer.blocksPerBatch))
		{
			if (batch2cells.TryGetValue(item, out var value))
			{
				Int3 @int = item * streamer.blocksPerBatch;
				Int3.Bounds bsRange = (blockRange - @int).Clamp(Int3.zero, streamer.blocksPerBatch - 1);
				value.QueueForSleep(bsRange, level, cellManagementQueue);
			}
		}
	}

	internal void QueueForAwake(EntityCell cell)
	{
		cell.QueueForAwake(cellManagementQueue);
	}

	internal void QueueForSleep(EntityCell cell)
	{
		cell.QueueForSleep(cellManagementQueue);
	}

	public void ResetEntityDistributions()
	{
		if (spawner != null && !spawner.Equals(null))
		{
			spawner.ResetSpawner();
		}
	}

	public Int3 GetGlobalCell(Vector3 wsPos, int cellLevel)
	{
		Int3 block = streamer.GetBlock(wsPos);
		Int3 cellSize = BatchCells.GetCellSize(cellLevel, streamer.blocksPerBatch);
		return block / cellSize;
	}

	public EntitySlot.Filler GetPrefabForSlot(IEntitySlot slot)
	{
		if (spawner != null && !spawner.Equals(null))
		{
			return spawner.GetPrefabForSlot(slot);
		}
		return default(EntitySlot.Filler);
	}

	public void SaveAllBatchCells()
	{
		Debug.LogFormat("Saving {0} batches to {1}", batch2cells.Count, streamer.tmpPathPrefix);
		foreach (KeyValuePair<Int3, BatchCells> batch2cell in batch2cells)
		{
			SaveCacheBatchCells(batch2cell.Value, streamer.tmpPathPrefix, skipEmpty: true);
		}
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	public void CollectMemoryUsageStats()
	{
		List<BatchCells> list = new List<BatchCells>();
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		int num = 0;
		foreach (KeyValuePair<Int3, BatchCells> batch2cell in batch2cells)
		{
			int num2 = batch2cell.Value.EstimateBytes();
			int item = batch2cell.Value.NumCellsWithData();
			list.Add(batch2cell.Value);
			list2.Add(num2);
			list3.Add(item);
			num += num2;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("BatchCellsID,MemoryUsage(bytes),CellsWithData");
		for (int i = 0; i < list.Count; i++)
		{
			stringBuilder.AppendFormat("{0},{1},{2}\n", list[i].batch.ToCsvString(), list2[i], list3[i]);
		}
		stringBuilder.AppendFormat(",{0},", num);
		Debug.Log(stringBuilder.ToString());
	}

	public int EstimateBytes()
	{
		Timer.Begin("CellManager::EstimateBytes");
		int num = 8;
		num += 256;
		num += 70;
		foreach (KeyValuePair<Int3, BatchCells> batch2cell in batch2cells)
		{
			num += 20;
			num += 4 + batch2cell.Value.EstimateBytes();
		}
		Timer.End();
		return num;
	}
}
