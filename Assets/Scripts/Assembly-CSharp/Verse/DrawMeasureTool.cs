using System;
using UnityEngine;

namespace Verse
{
	public class DrawMeasureTool
	{
		private string label;

		private Action clickAction;

		private Action onGUIAction;

		public DrawMeasureTool(string label, Action clickAction, Action onGUIAction = null)
		{
			this.label = label;
			this.clickAction = clickAction;
			this.onGUIAction = onGUIAction;
		}

		public DrawMeasureTool(string label, Action clickAction, Vector3 firstRectCorner)
		{
			this.label = label;
			this.clickAction = clickAction;
			onGUIAction = delegate
			{
				Vector3 v = UI.MouseMapPosition();
				Vector2 start = firstRectCorner.MapToUIPosition();
				Vector2 end = v.MapToUIPosition();
				Widgets.DrawLine(start, end, Color.white, 0.25f);
			};
		}

		public void DebugToolOnGUI()
		{
			if (Event.current.type == EventType.MouseDown)
			{
				if (Event.current.button == 0)
				{
					clickAction();
				}
				if (Event.current.button == 1)
				{
					DebugTools.curMeasureTool = null;
				}
				Event.current.Use();
			}
			Vector2 vector = Event.current.mousePosition + new Vector2(15f, 15f);
			Rect rect = new Rect(vector.x, vector.y, 999f, 999f);
			Text.Font = GameFont.Small;
			Widgets.Label(rect, label);
			if (onGUIAction != null)
			{
				onGUIAction();
			}
		}
	}
}
