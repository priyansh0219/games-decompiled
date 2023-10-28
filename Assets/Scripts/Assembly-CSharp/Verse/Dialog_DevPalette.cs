using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class Dialog_DevPalette : Window
	{
		private Vector2 windowPosition;

		private static List<DebugActionNode> cachedNodes;

		private int reorderableGroupID = -1;

		private Dictionary<string, string> nameCache = new Dictionary<string, string>();

		private int lastLabelCacheFrame = -1;

		private const string Title = "Dev palette";

		private const float ButtonSize = 24f;

		private const float ButtonSize_Small = 18f;

		private const string NoActionDesc = "<i>To add commands here, open the debug actions menu and click the pin icons.</i>";

		public override bool IsDebug => true;

		protected override float Margin => 4f;

		private List<DebugActionNode> Nodes
		{
			get
			{
				if (cachedNodes == null)
				{
					cachedNodes = new List<DebugActionNode>();
					for (int i = 0; i < Prefs.DebugActionsPalette.Count; i++)
					{
						DebugActionNode node = Dialog_Debug.GetNode(Prefs.DebugActionsPalette[i]);
						if (node != null)
						{
							cachedNodes.Add(node);
						}
					}
				}
				return cachedNodes;
			}
		}

		public Dialog_DevPalette()
		{
			draggable = true;
			focusWhenOpened = false;
			drawShadow = false;
			closeOnAccept = false;
			closeOnCancel = false;
			preventCameraMotion = false;
			drawInScreenshotMode = false;
			windowPosition = Prefs.DevPalettePosition;
			onlyDrawInDevMode = true;
			lastLabelCacheFrame = RealTime.frameCount;
			EnsureAllNodesValid();
		}

		private void EnsureAllNodesValid()
		{
			cachedNodes = null;
			for (int num = Prefs.DebugActionsPalette.Count - 1; num >= 0; num--)
			{
				string text = Prefs.DebugActionsPalette[num];
				if (Dialog_Debug.GetNode(text) == null)
				{
					Log.Warning("Could not find node from path '" + text + "'. Removing.");
					Prefs.DebugActionsPalette.RemoveAt(num);
					Prefs.Save();
				}
			}
		}

		public override void WindowUpdate()
		{
			base.WindowUpdate();
			if (RealTime.frameCount >= lastLabelCacheFrame + 30)
			{
				nameCache.Clear();
				lastLabelCacheFrame = RealTime.frameCount;
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Small;
			Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 24f), "Dev palette");
			inRect.yMin += 26f;
			if (Prefs.DebugActionsPalette.Count == 0)
			{
				GUI.color = ColoredText.SubtleGrayColor;
				Widgets.Label(inRect, "<i>To add commands here, open the debug actions menu and click the pin icons.</i>");
				GUI.color = Color.white;
			}
			else
			{
				if (Event.current.type == EventType.Repaint)
				{
					reorderableGroupID = ReorderableWidget.NewGroup(delegate(int from, int to)
					{
						string item = Prefs.DebugActionsPalette[from];
						Prefs.DebugActionsPalette.Insert(to, item);
						Prefs.DebugActionsPalette.RemoveAt((from < to) ? from : (from + 1));
						cachedNodes = null;
						Prefs.Save();
					}, ReorderableDirection.Vertical, inRect, -1f, null, playSoundOnStartReorder: false);
				}
				GUI.BeginGroup(inRect);
				float num = 0f;
				Text.Font = GameFont.Tiny;
				for (int i = 0; i < Nodes.Count; i++)
				{
					DebugActionNode debugActionNode = Nodes[i];
					float num2 = 0f;
					Rect rect = new Rect(num2, num, 18f, 18f);
					if (ReorderableWidget.Reorderable(reorderableGroupID, rect.ExpandedBy(4f)))
					{
						Widgets.DrawRectFast(rect, Widgets.WindowBGFillColor * new Color(1f, 1f, 1f, 0.5f));
					}
					Widgets.ButtonImage(rect.ContractedBy(1f), TexButton.DragHash);
					num2 += 18f;
					Rect rect2 = new Rect(num2, num, inRect.width - 36f, 18f);
					if (debugActionNode.ActiveNow)
					{
						if (debugActionNode.settingsField != null)
						{
							Rect rect3 = rect2;
							rect3.xMax -= rect3.height + 4f;
							Widgets.Label(rect3, "  " + PrettifyNodeName(debugActionNode));
							GUI.DrawTexture(new Rect(rect3.xMax, rect3.y, rect3.height, rect3.height), debugActionNode.On ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex);
							Widgets.DrawHighlightIfMouseover(rect2);
							if (Widgets.ButtonInvisible(rect2))
							{
								debugActionNode.Enter(null);
							}
						}
						else if (Widgets.ButtonText(rect2, "  " + PrettifyNodeName(debugActionNode), drawBackground: true, doMouseoverSound: true, active: true, TextAnchor.MiddleLeft))
						{
							debugActionNode.Enter(Find.WindowStack.WindowOfType<Dialog_Debug>());
						}
					}
					else
					{
						Widgets.Label(rect2, "  " + PrettifyNodeName(debugActionNode));
					}
					num2 += rect2.width;
					Rect butRect = new Rect(num2, num, 18f, 18f);
					if (Widgets.ButtonImage(butRect, Widgets.CheckboxOffTex))
					{
						Prefs.DebugActionsPalette.RemoveAt(i);
						cachedNodes = null;
						SetInitialSizeAndPosition();
					}
					num2 += butRect.width;
					num += 20f;
				}
				GUI.EndGroup();
			}
			if (!Mathf.Approximately(windowRect.x, windowPosition.x) || !Mathf.Approximately(windowRect.y, windowPosition.y))
			{
				windowPosition = new Vector2(windowRect.x, windowRect.y);
				Prefs.DevPalettePosition = windowPosition;
			}
		}

		public static void ToggleAction(string actionLabel)
		{
			if (Prefs.DebugActionsPalette.Contains(actionLabel))
			{
				Prefs.DebugActionsPalette.Remove(actionLabel);
			}
			else
			{
				Prefs.DebugActionsPalette.Add(actionLabel);
			}
			Prefs.Save();
			cachedNodes = null;
			Find.WindowStack.WindowOfType<Dialog_DevPalette>()?.SetInitialSizeAndPosition();
		}

		protected override void SetInitialSizeAndPosition()
		{
			GameFont font = Text.Font;
			Text.Font = GameFont.Small;
			Vector2 vector = new Vector2(Text.CalcSize("Dev palette").x + 48f + 10f, 28f);
			if (!Nodes.Any())
			{
				vector.x = Mathf.Max(vector.x, 200f);
				vector.y += Text.CalcHeight("<i>To add commands here, open the debug actions menu and click the pin icons.</i>", vector.x) + Margin * 2f;
			}
			else
			{
				Text.Font = GameFont.Tiny;
				for (int i = 0; i < Nodes.Count; i++)
				{
					vector.x = Mathf.Max(vector.x, Text.CalcSize("  " + PrettifyNodeName(Nodes[i]) + "  ").x + 48f);
				}
				vector.y += (float)Nodes.Count * 18f + (float)((Nodes.Count + 1) * 2) + Margin;
			}
			windowPosition.x = Mathf.Clamp(windowPosition.x, 0f, (float)UI.screenWidth - vector.x);
			windowPosition.y = Mathf.Clamp(windowPosition.y, 0f, (float)UI.screenHeight - vector.y);
			windowRect = new Rect(windowPosition.x, windowPosition.y, vector.x, vector.y);
			windowRect = windowRect.Rounded();
			Text.Font = font;
		}

		private string PrettifyNodeName(DebugActionNode node)
		{
			string path = node.Path;
			if (nameCache.TryGetValue(path, out var value))
			{
				return value;
			}
			DebugActionNode debugActionNode = node;
			value = debugActionNode.LabelNow.Replace("...", "");
			while (debugActionNode.parent != null && !debugActionNode.parent.IsRoot && (debugActionNode.parent.parent == null || !debugActionNode.parent.parent.IsRoot))
			{
				value = debugActionNode.parent.LabelNow.Replace("...", "") + "\\" + value;
				debugActionNode = debugActionNode.parent;
			}
			nameCache[path] = value;
			return value;
		}
	}
}
