using System;
using System.Collections.Generic;
using System.Linq;
using UWE;
using UnityEngine;

public class BatchCells
{
	private const int cellsPerBatch = 10;

	private const int numCellTiers = 4;

	private CellManager manager;

	private LargeWorldStreamer host;

	public Int3 batch;

	private Array3<EntityCell> cellsTier0;

	private Array3<EntityCell> cellsTier1;

	private Array3<EntityCell> cellsTier2;

	private Array3<EntityCell> cellsTier3;

	private static ObjectPool<BatchCells> batchCellsPool = ObjectPoolHelper.CreatePool<BatchCells>(200);

	public BatchCells()
	{
		InitCellsTiers();
	}

	private void Init(CellManager manager, LargeWorldStreamer host, Int3 batch)
	{
		this.manager = manager;
		this.host = host;
		this.batch = batch;
	}

	private void InitCellsTiers()
	{
		cellsTier0 = new Array3<EntityCell>(UWE.Utils.CeilShiftRight(10, 0));
		cellsTier1 = new Array3<EntityCell>(UWE.Utils.CeilShiftRight(10, 1));
		cellsTier2 = new Array3<EntityCell>(UWE.Utils.CeilShiftRight(10, 1));
		cellsTier3 = new Array3<EntityCell>(UWE.Utils.CeilShiftRight(10, 1));
	}

	private void Reset()
	{
		manager = null;
		host = null;
		batch = Int3.zero;
		foreach (EntityCell item in All())
		{
			EntityCell.ReturnToPool(item);
		}
		cellsTier0.Clear();
		cellsTier1.Clear();
		cellsTier2.Clear();
		cellsTier3.Clear();
	}

	internal EntityCell EnsureCell(Int3 cellId, int level)
	{
		EntityCell entityCell = Get(cellId, level);
		if (entityCell == null)
		{
			entityCell = Add(cellId, level);
			entityCell.Initialize();
		}
		return entityCell;
	}

	public static BatchCells GetFromPool(CellManager manager, LargeWorldStreamer host, Int3 batch)
	{
		BatchCells batchCells = batchCellsPool.Get();
		batchCells.Init(manager, host, batch);
		return batchCells;
	}

	public static void ReturnToPool(BatchCells batchCells)
	{
		batchCells.Reset();
		batchCellsPool.Return(batchCells);
	}

	internal EntityCell Add(Int3 cellId, int level)
	{
		try
		{
			EntityCell entityCell = GetCells(level).Get(cellId);
			if (entityCell != null)
			{
				Debug.LogWarningFormat("Replacing cell {0} with new cell.", entityCell);
			}
			EntityCell fromPool = EntityCell.GetFromPool(manager, host, batch, cellId, level);
			GetCells(level).Set(cellId, fromPool);
			return fromPool;
		}
		catch (Exception ex)
		{
			Debug.LogException(ex, host);
			Debug.LogErrorFormat("Exception while adding entity cell {0}, level {1} in batch {2}: {3}", cellId, level, batch, ex);
			throw;
		}
	}

	internal EntityCell Get(Int3 cellId, int level)
	{
		try
		{
			return GetCells(level).Get(cellId);
		}
		catch (Exception ex)
		{
			Debug.LogException(ex, host);
			Debug.LogErrorFormat("Exception while getting entity cell {0}, level {1} in batch {2}: {3}", cellId, level, batch, ex);
			throw;
		}
	}

	internal void QueueForAwake(Int3.Bounds bsRange, int level, IQueue<EntityCell> queue)
	{
		Int3 cellSize = GetCellSize(level, host.blocksPerBatch);
		Int3.RangeEnumerator enumerator = Int3.Bounds.OuterCoarserBounds(bsRange, cellSize).GetEnumerator();
		while (enumerator.MoveNext())
		{
			Int3 current = enumerator.Current;
			EnsureCell(current, level).QueueForAwake(queue);
		}
	}

	internal void QueueForSleep(Int3.Bounds bsRange, int level, IQueue<EntityCell> queue)
	{
		Int3 cellSize = GetCellSize(level, host.blocksPerBatch);
		Int3.RangeEnumerator enumerator = Int3.Bounds.OuterCoarserBounds(bsRange, cellSize).GetEnumerator();
		while (enumerator.MoveNext())
		{
			Int3 current = enumerator.Current;
			Get(current, level)?.QueueForSleep(queue);
		}
	}

	internal bool AreCellsAwake(Int3.Bounds bsRange, int level)
	{
		Int3 cellSize = GetCellSize(level, host.blocksPerBatch);
		Int3.RangeEnumerator enumerator = Int3.Bounds.OuterCoarserBounds(bsRange, cellSize).GetEnumerator();
		while (enumerator.MoveNext())
		{
			Int3 current = enumerator.Current;
			EntityCell entityCell = Get(current, level);
			if (entityCell != null && !entityCell.IsAwake())
			{
				return false;
			}
		}
		return true;
	}

	public static Int3 GetCellSize(int level, Int3 blocksPerBatch)
	{
		Int3 @int = blocksPerBatch / 10;
		switch (level)
		{
		case 0:
			return @int;
		case 1:
		case 2:
		case 3:
			return @int << 1;
		default:
			Debug.LogErrorFormat("Unexpected cell level {0} in GetCellSize", level);
			return @int;
		}
	}

	public static Int3 GetCellSize(int level, Int3 blocksPerBatch, int version)
	{
		if (version < 9)
		{
			return blocksPerBatch / 10 << level;
		}
		return GetCellSize(level, blocksPerBatch);
	}

	public static Int3 GetCellId(Int3 legacyCellId, int level, int version)
	{
		if (version < 9 && level > 1)
		{
			return legacyCellId << level >> 1;
		}
		return legacyCellId;
	}

	public static Int3.Bounds GetBlockBounds(Int3 batchId, Int3 cellId, int level, Int3 blocksPerBatch)
	{
		Int3 cellSize = GetCellSize(level, blocksPerBatch);
		return GetBlockBounds(batchId, cellId, cellSize, blocksPerBatch);
	}

	public static Int3.Bounds GetBlockBounds(Int3 batchId, Int3 cellId, Int3 cellSize, Int3 blocksPerBatch)
	{
		Int3 @int = batchId * blocksPerBatch + cellId * cellSize;
		Int3 maxs = @int + cellSize - 1;
		return new Int3.Bounds(@int, maxs);
	}

	public void RemoveEmpty()
	{
		for (int i = 0; i < 4; i++)
		{
			Array3<EntityCell> cells = GetCells(i);
			foreach (EntityCell item in cells)
			{
				if (item != null && item.IsEmpty())
				{
					cells.Set(item.CellId, null);
					EntityCell.ReturnToPool(item);
				}
			}
		}
	}

	public IEnumerable<EntityCell> All()
	{
		for (int i = 0; i < 4; i++)
		{
			Array3<EntityCell> cells = GetCells(i);
			foreach (EntityCell item in cells)
			{
				if (item != null)
				{
					yield return item;
				}
			}
		}
	}

	private Array3<EntityCell> GetCells(int level)
	{
		switch (level)
		{
		case 0:
			return cellsTier0;
		case 1:
			return cellsTier1;
		case 2:
			return cellsTier2;
		case 3:
			return cellsTier3;
		default:
			Debug.LogErrorFormat("Cell level {0} not supported", level);
			return null;
		}
	}

	public int NumCellsWithData()
	{
		return (from cell in All()
			where cell.HasData()
			select cell).Count();
	}

	public int EstimateBytes()
	{
		int num = 8;
		num += 4;
		num += 4;
		num += 12;
		for (int i = 0; i < 4; i++)
		{
			Array3<EntityCell> cells = GetCells(i);
			num += 16;
			foreach (EntityCell item in cells)
			{
				num += 4;
				if (item != null)
				{
					num += item.EstimateBytes();
				}
			}
		}
		return num;
	}

	public override string ToString()
	{
		return $"BatchCells {batch}";
	}
}
