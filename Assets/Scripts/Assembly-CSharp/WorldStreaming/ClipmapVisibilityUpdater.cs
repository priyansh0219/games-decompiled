using System.Collections.Generic;
using UnityEngine;

namespace WorldStreaming
{
	public sealed class ClipmapVisibilityUpdater : IPipeline
	{
		private struct VisibilityUpdate
		{
			public readonly int priority;

			private readonly ClipmapLevel level;

			private readonly Int3 cell;

			private readonly bool loading;

			public VisibilityUpdate(ClipmapLevel level, Int3 cell, bool loading, int priority)
			{
				this.level = level;
				this.cell = cell;
				this.loading = loading;
				this.priority = priority;
			}

			public bool Execute()
			{
				return UpdateVisibility(level, cell, loading);
			}
		}

		private sealed class Comparer : IComparer<VisibilityUpdate>
		{
			public int Compare(VisibilityUpdate a, VisibilityUpdate b)
			{
				return b.priority - a.priority;
			}
		}

		private enum Visibility
		{
			Unloaded = 0,
			Visible = 1,
			HiddenByParent = 2,
			HiddenByChildren = 3
		}

		private static readonly Comparer comparer = new Comparer();

		private readonly Heap<VisibilityUpdate> queue = new Heap<VisibilityUpdate>(comparer, 100);

		public bool IsIdle()
		{
			return GetQueueLength() <= 0;
		}

		public int GetQueueLength()
		{
			return queue.Count;
		}

		public void Enqueue(ClipmapLevel level, Int3 cellId, bool loading, int priority)
		{
			VisibilityUpdate item = new VisibilityUpdate(level, cellId, loading, priority);
			queue.Enqueue(item);
		}

		public bool ProcessQueue()
		{
			VisibilityUpdate result;
			while (TryDequeue(out result))
			{
				if (result.Execute())
				{
					return true;
				}
			}
			return false;
		}

		private bool TryDequeue(out VisibilityUpdate result)
		{
			Heap<VisibilityUpdate> heap = queue;
			if (heap.Count <= 0)
			{
				result = default(VisibilityUpdate);
				return false;
			}
			result = heap.Dequeue();
			return true;
		}

		private static bool UpdateVisibility(ClipmapLevel level, Int3 cellId, bool loading)
		{
			if (!loading)
			{
				return UpdateVisibilityForUnloading(level, cellId);
			}
			return UpdateVisibilityForLoading(level, cellId);
		}

		private static bool UpdateVisibilityForLoading(ClipmapLevel level, Int3 cellId)
		{
			ClipmapCell cell = level.GetCell(cellId);
			if (!ClipmapCell.IsLoaded(cell))
			{
				return false;
			}
			UpdateVisibility(level, cell);
			return true;
		}

		private static bool UpdateVisibilityForUnloading(ClipmapLevel level, Int3 cellId)
		{
			if (ClipmapCell.IsLoaded(level.GetCell(cellId)))
			{
				return false;
			}
			UpdateVisibilityAroundUnloadingCell(level, cellId);
			return true;
		}

		private static Visibility UpdateVisibility(ClipmapLevel level, ClipmapCell cell)
		{
			Visibility visibility = UpdateVisibilityOfParent(level, cell.id);
			switch (visibility)
			{
			case Visibility.Unloaded:
			case Visibility.HiddenByChildren:
				if (level.settings.ignoreMeshes || level.ChildrenCanCover(cell.id))
				{
					cell.HideByChildren();
					level.ShowDescendants(cell.id);
					return Visibility.HiddenByChildren;
				}
				cell.Show();
				level.HideDescendants(cell.id);
				return Visibility.Visible;
			case Visibility.Visible:
			case Visibility.HiddenByParent:
				cell.HideByParent();
				level.HideDescendants(cell.id);
				return Visibility.HiddenByParent;
			default:
				Debug.LogErrorFormat("UpdateVisibility: Unreachable code. Level {0}, cell {1}, parent {2}", level.id, cell, visibility);
				return Visibility.Unloaded;
			}
		}

		private static void UpdateVisibilityAroundUnloadingCell(ClipmapLevel level, Int3 cellId)
		{
			Visibility visibility = UpdateVisibilityOfParent(level, cellId);
			switch (visibility)
			{
			case Visibility.Unloaded:
				level.ShowDescendants(cellId);
				return;
			case Visibility.HiddenByChildren:
				level.ShowDescendants(cellId);
				return;
			case Visibility.Visible:
			case Visibility.HiddenByParent:
				return;
			}
			Debug.LogErrorFormat("UpdateVisibilityAroundUnloadingCell: Unreachable code. Level {0}, cell {1}, parent {2}", level.id, cellId, visibility);
		}

		private static Visibility UpdateVisibilityOfParent(ClipmapLevel level, Int3 cellId)
		{
			ClipmapLevel parentLevel = level.GetParentLevel();
			if (parentLevel == null)
			{
				return Visibility.Unloaded;
			}
			Int3 cellId2 = cellId >> 1;
			ClipmapCell cell = parentLevel.GetCell(cellId2);
			if (!ClipmapCell.IsLoaded(cell))
			{
				return Visibility.Unloaded;
			}
			return UpdateVisibility(parentLevel, cell);
		}

		public void OnGUI()
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Visibility", GUILayout.Width(60f));
			GUILayout.TextField(queue.Count.ToString(), GUILayout.Width(60f));
			GUILayout.EndHorizontal();
		}
	}
}
