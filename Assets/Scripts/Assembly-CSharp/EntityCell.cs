using System.Collections;
using System.Collections.Generic;
using System.IO;
using UWE;
using UnityEngine;

public sealed class EntityCell : IPriorityQueueItem
{
	internal enum State
	{
		Uninitialized = 0,
		Finalized = 1,
		IsAwake = 2,
		IsAsleep = 3,
		QueuedForAwake = 4,
		QueuedForSleep = 5,
		InAwakeAsync = 6,
		InSleepAsync = 7,
		InAwakeAsyncToSleep = 8,
		InSleepAsyncToAwake = 9,
		InSerializeSerialDataAsync = 10,
		InSerializeWaiterDataAsync = 11
	}

	private CellManager manager;

	private LargeWorldStreamer host;

	private Int3 batchId;

	private Int3 cellId;

	private int level;

	private SerialData serialData = new SerialData();

	private SerialData legacyData = new SerialData();

	private readonly SerialData waiterData = new SerialData();

	internal GameObject liveRoot;

	private State state;

	private List<LargeWorldEntity> waiterQueue;

	private float priority;

	private int numPriorityChanges;

	private Vector3 cachedWorldPos;

	private CellProcessingStats heapStats;

	private static int queueInSerial = 0;

	private static int queueOutSerial = 0;

	private static readonly ObjectPool<EntityCell> cellPool = ObjectPoolHelper.CreatePool<EntityCell>(64000);

	internal Int3 BatchId => batchId;

	internal Int3 CellId => cellId;

	internal int Level => level;

	internal State CurrentState => state;

	internal bool AllowSpawnRestrictions { get; set; }

	public EntityCell()
	{
	}

	internal EntityCell(CellManager manager, LargeWorldStreamer host, Int3 batchId, Int3 cellId, int level)
	{
		InitData(manager, host, batchId, cellId, level);
	}

	internal void InitData(CellManager manager, LargeWorldStreamer host, Int3 batchId, Int3 cellId, int level)
	{
		this.manager = manager;
		this.host = host;
		this.batchId = batchId;
		this.cellId = cellId;
		this.level = level;
		liveRoot = null;
		state = State.Uninitialized;
	}

	internal Vector3 GetCenter()
	{
		return GetCenter(GetBlockBounds());
	}

	internal static Vector3 GetCenter(Int3.Bounds blockBounds)
	{
		Vector3 vector = blockBounds.mins.ToVector3();
		Vector3 vector2 = (blockBounds.maxs + 1).ToVector3();
		return (vector + vector2) / 2f;
	}

	internal Int3 GetSize()
	{
		return BatchCells.GetCellSize(level, host.blocksPerBatch);
	}

	internal Int3.Bounds GetBlockBounds()
	{
		return BatchCells.GetBlockBounds(batchId, cellId, level, host.blocksPerBatch);
	}

	internal void ClearTempSerialData()
	{
		if (state == State.IsAwake || state == State.QueuedForSleep)
		{
			serialData.Clear();
		}
	}

	internal SerialData GetSerialData()
	{
		return serialData;
	}

	internal IEnumerator EnsureSerialDataSerialized(ProtobufSerializer serializer)
	{
		if (state == State.IsAsleep || state == State.QueuedForAwake)
		{
			return null;
		}
		return SerializeSerialDataAsync(serializer);
	}

	internal void ReadSerialDataFromStream(Stream stream, int dataLength)
	{
		serialData.ReadFromStream(stream, dataLength);
	}

	internal void ReadLegacyDataFromStream(Stream stream, int dataLength)
	{
		legacyData.ReadFromStream(stream, dataLength);
	}

	internal void ReadWaiterDataFromStream(Stream stream, int dataLength)
	{
		waiterData.ReadFromStream(stream, dataLength);
	}

	internal void MoveSerialDataToLegacy()
	{
		SerialData serialData = legacyData;
		legacyData = this.serialData;
		this.serialData = serialData;
		this.serialData.Clear();
	}

	internal SerialData GetLegacyData()
	{
		return legacyData;
	}

	internal SerialData GetWaiterData()
	{
		return waiterData;
	}

	internal IEnumerator EnsureWaiterDataSerialized(ProtobufSerializer serializer)
	{
		if (waiterQueue == null || waiterQueue.Count == 0)
		{
			return null;
		}
		return SerializeWaiterDataAsync(serializer);
	}

	private IEnumerator SerializeSerialDataAsync(ProtobufSerializer serializer)
	{
		State previousState = state;
		state = State.InSerializeSerialDataAsync;
		yield return SerializeAsyncImpl(serializer);
		state = previousState;
	}

	private IEnumerator SerializeWaiterDataAsync(ProtobufSerializer serializer)
	{
		if (waiterQueue == null || waiterQueue.Count == 0)
		{
			yield break;
		}
		State previousState = state;
		state = State.InSerializeWaiterDataAsync;
		using (ScratchMemoryStream stream = new ScratchMemoryStream())
		{
			for (int i = 0; i < waiterQueue.Count; i++)
			{
				LargeWorldEntity largeWorldEntity = waiterQueue[i];
				if ((bool)largeWorldEntity)
				{
					GameObject gameObject = largeWorldEntity.gameObject;
					yield return serializer.SerializeObjectTreeAsync(stream, gameObject);
				}
				else
				{
					Debug.LogWarningFormat(liveRoot, "Skipping destroyed waiter {0} on serialize {1}", i, this);
				}
			}
			ClearWaiterQueue();
			waiterData.Concatenate(stream);
		}
		state = previousState;
	}

	private IEnumerator SerializeAsyncImpl(ProtobufSerializer serializer, bool beforeDestroy = false)
	{
		switch (state)
		{
		default:
			Debug.LogWarningFormat(liveRoot, "Unexpected state {0} in SerializeAsync of cell {1}", state, this);
			break;
		case State.InSleepAsync:
		case State.InSleepAsyncToAwake:
		case State.InSerializeSerialDataAsync:
			break;
		}
		if (!liveRoot || liveRoot.transform.childCount == 0)
		{
			serialData.Clear();
			yield break;
		}
		using (ScratchMemoryStream stream = new ScratchMemoryStream())
		{
			serializer.SerializeStreamHeader(stream);
			yield return serializer.SerializeObjectTreeAsync(stream, liveRoot, beforeDestroy);
			serialData.CopyFrom(stream);
		}
	}

	internal void EnsureRoot()
	{
		if (!liveRoot)
		{
			liveRoot = CreateRoot();
		}
	}

	private GameObject CreateRoot()
	{
		DebugDisplayTimer.Start();
		Vector3 cellRootPosition = GetCellRootPosition();
		GameObject gameObject = Object.Instantiate(host.cellRootPrefab, cellRootPosition, Quaternion.identity, host.cellsRoot);
		gameObject.hideFlags |= HideFlags.NotEditable;
		return gameObject;
	}

	internal Vector3 GetCellRootPosition()
	{
		return host.land.transform.TransformPoint(GetCenter());
	}

	internal bool AddEntity(LargeWorldEntity ent)
	{
		bool result = false;
		switch (state)
		{
		case State.IsAwake:
		case State.QueuedForSleep:
			EnsureRoot();
			ent.transform.SetParent(liveRoot.transform, worldPositionStays: true);
			ent.OnAddToCell();
			result = true;
			break;
		case State.IsAsleep:
		case State.QueuedForAwake:
		case State.InAwakeAsync:
		case State.InSleepAsync:
		case State.InAwakeAsyncToSleep:
		case State.InSleepAsyncToAwake:
		case State.InSerializeSerialDataAsync:
		case State.InSerializeWaiterDataAsync:
			ent.gameObject.SetActive(value: false);
			ent.transform.SetParent(host.waitersRoot, worldPositionStays: true);
			waiterQueue = waiterQueue ?? new List<LargeWorldEntity>();
			waiterQueue.Add(ent);
			break;
		default:
			Debug.LogWarningFormat(ent, "Unexpected state {0} in Cell.AddEntity of cell {1}", state, this);
			break;
		}
		return result;
	}

	internal void RequestAbort()
	{
		switch (state)
		{
		case State.InAwakeAsync:
		case State.InSleepAsync:
		case State.InAwakeAsyncToSleep:
		case State.InSleepAsyncToAwake:
			Debug.LogWarningFormat("Requesting abort in state {0}", state);
			state = (liveRoot ? State.IsAwake : State.IsAsleep);
			break;
		case State.InSerializeSerialDataAsync:
		case State.InSerializeWaiterDataAsync:
			Debug.LogWarningFormat("Requesting abort in state {0}", state);
			state = State.IsAwake;
			break;
		default:
			Debug.LogWarningFormat("Unexpected state {0} in EntityCell::RequestAbort.", state);
			break;
		}
	}

	internal IEnumerator AwakeAsync(ProtobufSerializer serializer)
	{
		state = State.InAwakeAsync;
		if (serialData.Length > 0)
		{
			using (MemoryStream stream2 = new MemoryStream(serialData.Data.Array, serialData.Data.Offset, serialData.Data.Length, writable: false))
			{
				if (serializer.TryDeserializeStreamHeader(stream2))
				{
					CoroutineTask<GameObject> task2 = serializer.DeserializeObjectTreeAsync(stream2, forceInactiveRoot: true, AllowSpawnRestrictions, 0);
					yield return task2;
					liveRoot = task2.GetResult();
				}
			}
			serialData.Clear();
		}
		if (waiterData.Length > 0)
		{
			EnsureRoot();
			using (MemoryStream stream2 = new MemoryStream(waiterData.Data.Array, waiterData.Data.Offset, waiterData.Data.Length, writable: false))
			{
				while (stream2.Position < waiterData.Length)
				{
					CoroutineTask<GameObject> task2 = serializer.DeserializeObjectTreeAsync(stream2, forceInactiveRoot: true, AllowSpawnRestrictions, 0);
					yield return task2;
					GameObject result = task2.GetResult();
					if (result != null)
					{
						result.transform.SetParent(liveRoot.transform, worldPositionStays: true);
						result.SetActive(value: true);
					}
				}
			}
			waiterData.Clear();
		}
		bool backToSleep = state == State.InAwakeAsyncToSleep;
		state = State.IsAwake;
		if ((bool)liveRoot)
		{
			liveRoot.transform.SetParent(host.cellsRoot, worldPositionStays: false);
			liveRoot.SetActive(value: true);
		}
		if (legacyData.Length > 0)
		{
			using (MemoryStream stream2 = new MemoryStream(legacyData.Data.Array, legacyData.Data.Offset, legacyData.Data.Length, writable: false))
			{
				if (serializer.TryDeserializeStreamHeader(stream2))
				{
					CoroutineTask<GameObject> task2 = serializer.DeserializeObjectTreeAsync(stream2, forceInactiveRoot: true, AllowSpawnRestrictions, 0);
					yield return task2;
					GameObject result2 = task2.GetResult();
					Transform transform = result2.transform;
					transform.SetParent(host.cellsRoot, worldPositionStays: false);
					ReregisterEntities(transform, manager);
					UWE.Utils.DestroyWrap(result2);
				}
			}
			legacyData.Clear();
		}
		if (waiterQueue != null)
		{
			EnsureRoot();
			for (int i = 0; i < waiterQueue.Count; i++)
			{
				LargeWorldEntity largeWorldEntity = waiterQueue[i];
				if ((bool)largeWorldEntity)
				{
					largeWorldEntity.transform.SetParent(liveRoot.transform, worldPositionStays: true);
					largeWorldEntity.gameObject.SetActive(value: true);
				}
				else
				{
					Debug.LogWarningFormat(liveRoot, "Skipping destroyed waiter {0} on awake {1}", i, this);
				}
			}
			waiterQueue.Clear();
			waiterQueue = null;
		}
		AllowSpawnRestrictions = false;
		if (backToSleep)
		{
			manager.QueueForSleep(this);
		}
	}

	internal void ReregisterEntities()
	{
		if ((bool)liveRoot)
		{
			ReregisterEntities(liveRoot.transform, manager);
		}
	}

	private static void ReregisterEntities(Transform rootTransform, CellManager manager)
	{
		for (int num = rootTransform.childCount - 1; num >= 0; num--)
		{
			LargeWorldEntity component = rootTransform.GetChild(num).GetComponent<LargeWorldEntity>();
			manager.RegisterEntity(component);
		}
	}

	internal IEnumerator EnsureSleepAsync(ProtobufSerializer serializer)
	{
		switch (state)
		{
		case State.IsAwake:
		case State.QueuedForSleep:
			return SleepAsync(serializer);
		case State.IsAsleep:
			return null;
		case State.QueuedForAwake:
			state = State.IsAsleep;
			return null;
		default:
			Debug.LogWarningFormat("Unexpected state {0} in Cell.EnsureSleep of cell {1}", state, this);
			return null;
		}
	}

	private IEnumerator SleepAsync(ProtobufSerializer serializer)
	{
		state = State.InSleepAsync;
		if ((bool)liveRoot)
		{
			StopwatchProfiler.GetCachedProfilerTag("Cell-Sleep1-DeactivateRoot-", liveRoot.name);
			liveRoot.SetActive(value: false);
		}
		bool shouldDestroyRoot = liveRoot != null;
		yield return SerializeAsyncImpl(serializer, shouldDestroyRoot);
		if (shouldDestroyRoot)
		{
			UWE.Utils.DestroyWrap(liveRoot);
			liveRoot = null;
		}
		bool num = state == State.InSleepAsyncToAwake;
		state = State.IsAsleep;
		if (num)
		{
			manager.QueueForAwake(this);
		}
	}

	internal void Initialize(SerialData serialData = null, SerialData legacySerialData = null, SerialData waiterSerialData = null)
	{
		state = State.IsAsleep;
		if (serialData != null)
		{
			this.serialData.CopyFrom(serialData);
		}
		if (legacySerialData != null)
		{
			legacyData.CopyFrom(legacySerialData);
		}
		if (waiterSerialData != null)
		{
			waiterData.CopyFrom(waiterSerialData);
		}
	}

	internal void QueueForAwake(IQueue<EntityCell> queue)
	{
		switch (state)
		{
		case State.IsAsleep:
			state = State.QueuedForAwake;
			OnEnqueue();
			queue.Enqueue(this);
			break;
		case State.QueuedForSleep:
			state = State.IsAwake;
			break;
		case State.InAwakeAsyncToSleep:
			state = State.InAwakeAsync;
			break;
		case State.InSleepAsync:
			state = State.InSleepAsyncToAwake;
			break;
		default:
			Debug.LogWarningFormat("Unexpected state {0} in Cell.QueueForAwake of cell {1}", state, this);
			break;
		case State.IsAwake:
		case State.QueuedForAwake:
		case State.InAwakeAsync:
		case State.InSleepAsyncToAwake:
			break;
		}
	}

	internal void QueueForSleep(IQueue<EntityCell> queue)
	{
		switch (state)
		{
		case State.IsAwake:
			state = State.QueuedForSleep;
			OnEnqueue();
			queue.Enqueue(this);
			break;
		case State.QueuedForAwake:
			state = State.IsAsleep;
			break;
		case State.InSleepAsyncToAwake:
			state = State.InSleepAsync;
			break;
		case State.InAwakeAsync:
			state = State.InAwakeAsyncToSleep;
			break;
		default:
			Debug.LogWarningFormat("Unexpected state {0} in Cell.QueueForSleep of cell {1}", state, this);
			break;
		case State.IsAsleep:
		case State.QueuedForSleep:
		case State.InSleepAsync:
		case State.InAwakeAsyncToSleep:
			break;
		}
	}

	internal bool IsAwake()
	{
		return state == State.IsAwake;
	}

	internal IEnumerator Proceed(ProtobufSerializer serializer)
	{
		switch (state)
		{
		case State.IsAwake:
		case State.IsAsleep:
		case State.InAwakeAsync:
		case State.InSleepAsync:
		case State.InAwakeAsyncToSleep:
		case State.InSleepAsyncToAwake:
			return null;
		case State.Finalized:
			return null;
		case State.Uninitialized:
			return null;
		case State.QueuedForAwake:
			return AwakeAsync(serializer);
		case State.QueuedForSleep:
			return SleepAsync(serializer);
		default:
			Debug.LogWarningFormat("Unexpected state {0} in Cell.Proceed of cell {1}", state, this);
			return null;
		}
	}

	private bool IsInitialized()
	{
		State state = this.state;
		if ((uint)state <= 1u)
		{
			return false;
		}
		return true;
	}

	private bool IsProcessing()
	{
		State state = this.state;
		if ((uint)(state - 6) <= 5u)
		{
			return true;
		}
		return false;
	}

	internal bool IsEmpty()
	{
		if ((bool)liveRoot && liveRoot.transform.childCount > 0)
		{
			return false;
		}
		if (!serialData.IsEmpty() || !legacyData.IsEmpty() || !waiterData.IsEmpty())
		{
			return false;
		}
		if (waiterQueue != null && waiterQueue.Count > 0)
		{
			return false;
		}
		return true;
	}

	private void ClearWaiterQueue()
	{
		if (waiterQueue == null)
		{
			return;
		}
		for (int i = 0; i < waiterQueue.Count; i++)
		{
			LargeWorldEntity largeWorldEntity = waiterQueue[i];
			if ((bool)largeWorldEntity)
			{
				UWE.Utils.DestroyWrap(largeWorldEntity.gameObject);
			}
		}
		waiterQueue.Clear();
		waiterQueue = null;
	}

	internal void Reset()
	{
		if (state == State.Finalized)
		{
			return;
		}
		if ((bool)liveRoot)
		{
			if (liveRoot.transform.childCount > 0 && manager != null && !manager.AbortRequested)
			{
				Debug.LogWarningFormat(liveRoot, "Resetting cell with live root {0}", this);
			}
			UWE.Utils.DestroyWrap(liveRoot);
			liveRoot = null;
		}
		if (waiterQueue != null && waiterQueue.Count > 0)
		{
			Debug.LogWarningFormat(liveRoot, "Resetting cell with {0} waiters {1}", waiterQueue.Count, this);
		}
		ClearWaiterQueue();
		serialData.Clear();
		legacyData.Clear();
		waiterData.Clear();
		manager = null;
		host = null;
		batchId = Int3.zero;
		cellId = Int3.zero;
		level = 0;
		state = State.Finalized;
	}

	internal bool HasData()
	{
		if (serialData.Length <= 0 && legacyData.Length <= 0)
		{
			return waiterData.Length > 0;
		}
		return true;
	}

	internal int EstimateBytes()
	{
		return 8 + 4 + 4 + 12 + 12 + 4 + 4 + 4 + (12 + serialData.Data.Length) + (12 + legacyData.Data.Length) + (12 + waiterData.Data.Length);
	}

	public override string ToString()
	{
		return $"Cell {cellId} (level {level}, batch {batchId}, state {state})";
	}

	internal static EntityCell GetFromPool(CellManager manager, LargeWorldStreamer host, Int3 batchId, Int3 cellId, int level)
	{
		EntityCell entityCell = cellPool.Get();
		entityCell.InitData(manager, host, batchId, cellId, level);
		return entityCell;
	}

	internal static void ReturnToPool(EntityCell cell)
	{
		cell.Reset();
		cellPool.Return(cell);
	}

	internal void OnEnqueue()
	{
		cachedWorldPos = GetCellRootPosition();
		UpdatePriority();
		numPriorityChanges = 0;
		InitializeHeapStats();
	}

	internal void OnDequeue()
	{
		FinalizeHeapStats();
	}

	public float GetPriority()
	{
		return priority;
	}

	public float UpdatePriority()
	{
		Vector3 cachedCameraPosition = LargeWorldStreamer.main.cachedCameraPosition;
		Vector3 cachedCameraForward = LargeWorldStreamer.main.cachedCameraForward;
		Vector3 rhs = cachedWorldPos - cachedCameraPosition;
		float magnitude = rhs.magnitude;
		float num = Vector3.Dot(cachedCameraForward, rhs) / magnitude;
		float num2 = 2f - num;
		float num3 = num2 * num2;
		priority = magnitude * num3;
		if (state == State.QueuedForSleep)
		{
			priority = 5000f - priority;
		}
		numPriorityChanges++;
		return priority;
	}

	private void InitializeHeapStats()
	{
		if (HeapStats.main.IsRecording && state == State.QueuedForAwake)
		{
			Vector3 cachedCameraPosition = LargeWorldStreamer.main.cachedCameraPosition;
			Vector3 cachedCameraForward = LargeWorldStreamer.main.cachedCameraForward;
			Vector3 vector = cachedWorldPos - cachedCameraPosition;
			heapStats = new CellProcessingStats
			{
				inId = queueInSerial++,
				inTime = LargeWorldStreamer.main.cachedTime,
				inAngle = Vector3.Angle(cachedCameraForward, vector),
				inDistance = Vector3.Magnitude(vector),
				inPriority = GetPriority(),
				inQueueLength = manager.GetQueueLength()
			};
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
			HeapStats.main.RecordStats("CellsHeap", heapStats);
			heapStats = null;
		}
	}
}
