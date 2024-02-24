using System.Collections.Generic;
using UWE;

namespace WorldStreaming
{
	public sealed class ClipmapLevel
	{
		private ClipmapVisibilityUpdater visibilityUpdater;

		public readonly int id;

		public readonly int cellSize;

		private readonly Int3 arraySize;

		private readonly Array3<ClipmapCell> cells;

		private Int3 centerCell = Int3.negativeOne;

		private static readonly Task.Function BeginNotifyCellLoadedDelegate = BeginNotifyCellLoaded;

		public ClipMapManager.LevelSettings settings { get; private set; }

		public ClipmapStreamer streamer { get; private set; }

		public ClipmapLevel(ClipmapStreamer streamer, ClipmapVisibilityUpdater visibilityUpdater, int id, int cellSize, ClipMapManager.LevelSettings settings)
		{
			this.settings = settings;
			this.streamer = streamer;
			this.visibilityUpdater = visibilityUpdater;
			this.id = id;
			this.cellSize = cellSize;
			arraySize = new Int3(settings.chunksPerSide, settings.chunksVertically, settings.chunksPerSide);
			cells = new Array3<ClipmapCell>(arraySize.x, arraySize.y, arraySize.z);
			foreach (Int3 item in Int3.Range(arraySize))
			{
				cells.Set(item, new ClipmapCell(this, item));
			}
		}

		public void Clear()
		{
			settings = null;
			streamer = null;
			visibilityUpdater = null;
			foreach (ClipmapCell cell in cells)
			{
				cell.Clear();
			}
		}

		public void OnBatchOctreesChanged(BatchOctreesStreamer streamer, Int3.Bounds blockRange)
		{
			foreach (Int3 item in GetCellRange(blockRange))
			{
				GetCell(item)?.OnBatchOctreesChanged(streamer);
			}
		}

		public bool IsProcessing(Int3.Bounds blockRange)
		{
			foreach (Int3 item in GetCellRange(blockRange))
			{
				ClipmapCell cell = GetCell(item);
				if (cell != null && cell.IsProcessing())
				{
					return true;
				}
			}
			return false;
		}

		public bool IsRangeActiveAndBuilt(Int3.Bounds blockRange)
		{
			foreach (Int3 item in GetCellRange(blockRange))
			{
				if (!ClipmapCell.IsLoaded(GetCell(item)))
				{
					return false;
				}
			}
			return true;
		}

		private Int3.Bounds GetCellRange(Int3.Bounds blockRange)
		{
			Int3 min = Int3.FloorDiv(blockRange.mins - (3 << id), cellSize);
			Int3 max = Int3.CeilDiv(blockRange.maxs + 2 * (3 << id), cellSize);
			return Int3.MinMax(min, max);
		}

		public bool UpdateCenter(Int3 position)
		{
			Int3 @int = centerCell;
			Int3 int2 = Int3.FloorDiv(position, cellSize);
			if (int2 == @int)
			{
				return false;
			}
			centerCell = int2;
			foreach (Int3 item in Int3.CenterSize(centerCell, arraySize))
			{
				Int3 p = Int3.PositiveModulo(item, arraySize);
				ClipmapCell clipmapCell = cells.Get(p);
				if (clipmapCell.id == item)
				{
					clipmapCell.Load();
				}
				else
				{
					clipmapCell.Reload(item);
				}
			}
			return true;
		}

		public void Unload()
		{
			foreach (ClipmapCell cell in cells)
			{
				cell.Unload();
			}
		}

		public void BeginNotifyListeners(Int3.Bounds blockRange)
		{
			List<Int3> list = new List<Int3>();
			foreach (Int3 item in GetCellRange(blockRange))
			{
				if (ClipmapCell.IsLoaded(GetCell(item)))
				{
					list.Add(item);
				}
			}
			streamer.buildLayersThread.Enqueue(BeginNotifyCellLoadedDelegate, this, list);
		}

		private static void BeginNotifyCellLoaded(object owner, object state)
		{
			ClipmapLevel obj = (ClipmapLevel)owner;
			List<Int3> cellIds = (List<Int3>)state;
			obj.BeginNotifyCellLoaded(cellIds);
		}

		private void BeginNotifyCellLoaded(List<Int3> cellIds)
		{
			foreach (Int3 cellId in cellIds)
			{
				streamer.OnCellLoaded(this, cellId);
			}
		}

		public void EnqueueForLoading(ClipmapCell cell)
		{
			streamer.EnqueueForLoading(cell, CalculateLoadingPriority(centerCell, cell.id, id));
		}

		public void EnqueueForUnloading(ClipmapCell cell)
		{
			Int3? reloadId = cell.reloadId;
			int priority = ((!reloadId.HasValue) ? CalculateUnloadingPriority(centerCell, cell.id, id) : CalculateLoadingPriority(centerCell, reloadId.Value, id));
			streamer.EnqueueForUnloading(cell, priority);
		}

		public void EnqueueForVisibilityUpdate(Int3 cellId, bool loading)
		{
			int priority = CalculateUpdateVisibilityPriority(centerCell, cellId, id);
			visibilityUpdater.Enqueue(this, cellId, loading, priority);
		}

		public void ShowDescendants(Int3 cellId)
		{
			ClipmapLevel level = streamer.GetLevel(id - 1);
			if (level == null)
			{
				return;
			}
			foreach (Int3 childId in ClipmapCell.GetChildIds(cellId))
			{
				level.ShowDescendantsOrSelf(childId);
			}
		}

		private void ShowDescendantsOrSelf(Int3 cellId)
		{
			ClipmapCell cell = GetCell(cellId);
			if (ClipmapCell.IsLoaded(cell))
			{
				if (!settings.ignoreMeshes && !ChildrenCanCover(cellId))
				{
					cell.Show();
					HideDescendants(cellId);
				}
				else if (cell.HideByChildren())
				{
					ShowDescendants(cellId);
				}
			}
		}

		public void HideDescendants(Int3 cellId)
		{
			ClipmapLevel level = streamer.GetLevel(id - 1);
			if (level == null)
			{
				return;
			}
			foreach (Int3 childId in ClipmapCell.GetChildIds(cellId))
			{
				ClipmapCell cell = level.GetCell(childId);
				if (ClipmapCell.IsLoaded(cell) && cell.HideByParent())
				{
					level.HideDescendants(childId);
				}
			}
		}

		public bool ChildrenCanCover(Int3 cellId)
		{
			ClipmapLevel level = streamer.GetLevel(id - 1);
			if (level == null)
			{
				return false;
			}
			if (level.settings.ignoreMeshes)
			{
				return false;
			}
			foreach (Int3 childId in ClipmapCell.GetChildIds(cellId))
			{
				if (!ClipmapCell.IsLoaded(level.GetCell(childId)))
				{
					return false;
				}
			}
			return true;
		}

		public ClipmapLevel GetParentLevel()
		{
			return streamer.GetLevel(id + 1);
		}

		public ClipmapCell GetCell(Int3 cellId)
		{
			Int3 p = Int3.PositiveModulo(cellId, arraySize);
			ClipmapCell clipmapCell = cells.Get(p);
			if (!(clipmapCell.id == cellId))
			{
				return null;
			}
			return clipmapCell;
		}

		public override string ToString()
		{
			return $"ClipmapLevel (level {id}, cellSize {cellSize}, arraySize {arraySize})";
		}

		private static int CalculateLoadingPriority(Int3 center, Int3 position, int level)
		{
			return -Int3.SquareDistance(center, position) * (1 << level);
		}

		private static int CalculateUnloadingPriority(Int3 center, Int3 position, int level)
		{
			return -CalculateLoadingPriority(center, position, level);
		}

		private static int CalculateUpdateVisibilityPriority(Int3 center, Int3 position, int level)
		{
			return -Int3.SquareDistance(center, position) - 1000 * level;
		}

		public void DrawGizmos()
		{
			foreach (ClipmapCell cell in cells)
			{
				cell.DrawGizmos();
			}
		}
	}
}
