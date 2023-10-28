using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class InspectGizmoGrid
	{
		private static List<object> objList = new List<object>();

		private static List<Gizmo> gizmoList = new List<Gizmo>();

		private static int cacheFrame;

		private static List<object> tmpObjectCacheList = new List<object>();

		public static void DrawInspectGizmoGridFor(IEnumerable<object> selectedObjects, out Gizmo mouseoverGizmo)
		{
			mouseoverGizmo = null;
			ISelectable selectable = null;
			if (Find.ScreenshotModeHandler.Active)
			{
				return;
			}
			try
			{
				bool flag = true;
				int frameCount = Time.frameCount;
				if (cacheFrame == frameCount)
				{
					tmpObjectCacheList.Clear();
					tmpObjectCacheList.AddRange(selectedObjects);
					if (objList.Count == tmpObjectCacheList.Count)
					{
						for (int i = 0; i < objList.Count; i++)
						{
							if (tmpObjectCacheList[i] != objList[i])
							{
								flag = false;
								break;
							}
						}
					}
					else
					{
						flag = false;
					}
				}
				else
				{
					flag = false;
				}
				if (!flag)
				{
					cacheFrame = frameCount;
					objList.Clear();
					objList.AddRange(selectedObjects);
					gizmoList.Clear();
					for (int j = 0; j < objList.Count; j++)
					{
						selectable = objList[j] as ISelectable;
						if (selectable != null)
						{
							gizmoList.AddRange(selectable.GetGizmos());
						}
					}
					selectable = null;
					for (int k = 0; k < objList.Count; k++)
					{
						if (!(objList[k] is Thing t))
						{
							continue;
						}
						List<Designator> allDesignators = Find.ReverseDesignatorDatabase.AllDesignators;
						for (int l = 0; l < allDesignators.Count; l++)
						{
							Command_Action command_Action = allDesignators[l].CreateReverseDesignationGizmo(t);
							if (command_Action != null)
							{
								gizmoList.Add(command_Action);
							}
						}
					}
				}
				GizmoGridDrawer.DrawGizmoGrid(gizmoList, InspectPaneUtility.PaneWidthFor(Find.WindowStack.WindowOfType<IInspectPane>()) + GizmoGridDrawer.GizmoSpacing.y, out mouseoverGizmo);
			}
			catch (Exception ex)
			{
				Log.ErrorOnce(ex.ToString() + " currentSelectable: " + selectable.ToStringSafe(), 3427734);
			}
		}
	}
}
