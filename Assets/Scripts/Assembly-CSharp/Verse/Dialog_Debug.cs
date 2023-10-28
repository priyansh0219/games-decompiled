using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public class Dialog_Debug : Window
	{
		public static DebugActionNode rootNode;

		private DebugActionNode currentNode;

		private List<TabRecord> tabs = new List<TabRecord>();

		private Dictionary<DebugTabMenuDef, DebugTabMenu> menus = new Dictionary<DebugTabMenuDef, DebugTabMenu>();

		private static Dictionary<DebugTabMenuDef, DebugActionNode> roots = new Dictionary<DebugTabMenuDef, DebugActionNode>();

		private List<DebugTabMenuDef> menuDefsSorted = new List<DebugTabMenuDef>();

		private DebugTabMenu currentTabMenu;

		private float totalOptionsHeight;

		private Listing_Standard listing;

		private string filter;

		private bool focusFilter;

		private int currentHighlightIndex;

		private int prioritizedHighlightedIndex;

		private Vector2 scrollPosition;

		private const string FilterControlName = "DebugFilter";

		private const float DebugOptionsGap = 7f;

		private static readonly Color DisallowedColor = new Color(1f, 1f, 1f, 0.3f);

		private static readonly Vector2 FilterInputSize = new Vector2(200f, 30f);

		private const float AssumedBiggestElementHeight = 50f;

		private const float BackButtonWidth = 120f;

		public override bool IsDebug => true;

		public override Vector2 InitialSize => new Vector2(UI.screenWidth, UI.screenHeight);

		public string Filter => filter;

		private float FilterX
		{
			get
			{
				if (currentNode?.parent == null || !currentNode.parent.IsRoot)
				{
					return 130f;
				}
				return 0f;
			}
		}

		private int HighlightedIndex => currentTabMenu.HighlightedIndex(currentHighlightIndex, prioritizedHighlightedIndex);

		public DebugActionNode CurrentNode => currentNode;

		public Dialog_Debug()
		{
			Setup();
			SwitchTab(DebugTabMenuDefOf.Actions);
		}

		public Dialog_Debug(DebugTabMenuDef def)
		{
			Setup();
			SwitchTab(def);
		}

		private void Setup()
		{
			forcePause = true;
			doCloseX = true;
			onlyOneOfTypeAllowed = true;
			absorbInputAroundWindow = true;
			focusFilter = true;
			menuDefsSorted.AddRange(DefDatabase<DebugTabMenuDef>.AllDefs.ToList());
			menuDefsSorted.SortBy((DebugTabMenuDef x) => x.displayOrder, (DebugTabMenuDef y) => y.label);
		}

		public void SwitchTab(DebugTabMenuDef def)
		{
			TrySetupNodeGraph();
			scrollPosition = Vector2.zero;
			currentHighlightIndex = 0;
			prioritizedHighlightedIndex = 0;
			currentTabMenu = (menus.ContainsKey(def) ? menus[def] : DebugTabMenu.CreateMenu(def, this, rootNode));
			currentTabMenu.Enter(roots[def]);
		}

		public static void TrySetupNodeGraph()
		{
			if (rootNode != null)
			{
				return;
			}
			rootNode = new DebugActionNode("Root");
			foreach (DebugTabMenuDef allDef in DefDatabase<DebugTabMenuDef>.AllDefs)
			{
				roots.Add(allDef, DebugTabMenu.CreateMenu(allDef, null, rootNode).InitActions(rootNode));
			}
		}

		private void DrawTabs(Rect rect)
		{
			tabs.Clear();
			foreach (DebugTabMenuDef d in menuDefsSorted)
			{
				tabs.Add(new TabRecord(d.LabelCap, delegate
				{
					SwitchTab(d);
				}, currentTabMenu.def == d));
			}
			TabDrawer.DrawTabs(rect, tabs);
		}

		public override void DoWindowContents(Rect inRect)
		{
			GUI.SetNextControlName("DebugFilter");
			Text.Font = GameFont.Small;
			Rect rect = new Rect(FilterX, 0f, FilterInputSize.x, FilterInputSize.y);
			filter = Widgets.TextField(rect, filter);
			Rect rect2 = new Rect(rect.xMax + 10f, 32f, inRect.width - rect.width - 10f, 32f);
			DrawTabs(rect2);
			if ((Event.current.type == EventType.KeyDown || Event.current.type == EventType.Repaint) && focusFilter)
			{
				GUI.FocusControl("DebugFilter");
				filter = string.Empty;
				focusFilter = false;
			}
			if (KeyBindingDefOf.Dev_ChangeSelectedDebugAction.IsDownEvent)
			{
				int highlightedIndex = HighlightedIndex;
				if (highlightedIndex >= 0)
				{
					for (int i = 0; i < currentTabMenu.Count; i++)
					{
						int index = (highlightedIndex + i + 1) % currentTabMenu.Count;
						if (FilterAllows(currentTabMenu.LabelAtIndex(index)))
						{
							prioritizedHighlightedIndex = index;
							break;
						}
					}
				}
			}
			if (Event.current.type == EventType.Layout)
			{
				totalOptionsHeight = 0f;
			}
			Rect outRect = new Rect(inRect);
			outRect.yMin += 42f;
			int num = (int)(InitialSize.x / 200f);
			float height = Mathf.Max(outRect.height, (totalOptionsHeight + 50f * (float)(num - 1)) / (float)num);
			Rect rect3 = new Rect(0f, 0f, outRect.width - 16f, height);
			Widgets.BeginScrollView(outRect, ref scrollPosition, rect3);
			listing = new Listing_Standard(inRect, () => scrollPosition);
			listing.ColumnWidth = (rect3.width - 17f * (float)(num - 1)) / (float)num;
			listing.Begin(rect3);
			currentTabMenu.ListOptions(HighlightedIndex);
			listing.End();
			Widgets.EndScrollView();
			if (currentNode.parent != null && !currentNode.parent.IsRoot)
			{
				GameFont font = Text.Font;
				Text.Font = GameFont.Small;
				if (Widgets.ButtonText(new Rect(0f, 0f, 120f, 32f), "Back"))
				{
					currentNode.parent.Enter(this);
				}
				if (!currentNode.IsRoot)
				{
					Text.Anchor = TextAnchor.UpperRight;
					Text.Font = GameFont.Tiny;
					Widgets.Label(new Rect(0f, 0f, outRect.width - 24f - 10f, 32f), currentNode.Path.Colorize(ColoredText.SubtleGrayColor));
					Text.Anchor = TextAnchor.UpperLeft;
				}
				Text.Font = font;
			}
		}

		public override void OnAcceptKeyPressed()
		{
			if (GUI.GetNameOfFocusedControl() == "DebugFilter")
			{
				int highlightedIndex = HighlightedIndex;
				currentTabMenu.OnAcceptKeyPressed(highlightedIndex);
				Event.current.Use();
			}
		}

		public override void OnCancelKeyPressed()
		{
			if (currentNode.parent != null && !currentNode.parent.IsRoot)
			{
				currentNode.parent.Enter(this);
				Event.current.Use();
			}
			else
			{
				base.OnCancelKeyPressed();
			}
		}

		public static DebugActionNode GetNode(string path)
		{
			TrySetupNodeGraph();
			DebugActionNode debugActionNode = rootNode;
			string[] s = path.Split('\\');
			int i;
			for (i = 0; i < s.Length; i++)
			{
				DebugActionNode debugActionNode2 = debugActionNode.children.FirstOrDefault((DebugActionNode x) => x.label == s[i]);
				if (debugActionNode2 == null)
				{
					return null;
				}
				debugActionNode = debugActionNode2;
				debugActionNode.TrySetupChildren();
			}
			return debugActionNode;
		}

		public void SetCurrentNode(DebugActionNode node)
		{
			currentNode = node;
			foreach (DebugActionNode child in currentNode.children)
			{
				child.DirtyLabelCache();
			}
			scrollPosition = Vector2.zero;
			filter = string.Empty;
			currentHighlightIndex = 0;
			prioritizedHighlightedIndex = 0;
			currentTabMenu?.Recache();
		}

		public void DrawNode(DebugActionNode node, bool highlight)
		{
			if (node.settingsField != null)
			{
				DoCheckbox(node, highlight);
			}
			else
			{
				DoButton(node, highlight);
			}
		}

		private void DoButton(DebugActionNode node, bool highlight)
		{
			string labelNow = node.LabelNow;
			if (!FilterAllows(labelNow))
			{
				GUI.color = DisallowedColor;
			}
			switch (listing.ButtonDebugPinnable(labelNow, highlight, Prefs.DebugActionsPalette.Contains(node.Path)))
			{
			case DebugActionButtonResult.ButtonPressed:
				node.Enter(this);
				break;
			case DebugActionButtonResult.PinPressed:
				Dialog_DevPalette.ToggleAction(node.Path);
				break;
			}
			GUI.color = Color.white;
			if (Event.current.type == EventType.Layout)
			{
				totalOptionsHeight += 22f + listing.verticalSpacing;
			}
		}

		private void DoCheckbox(DebugActionNode node, bool highlight)
		{
			string labelNow = node.LabelNow;
			FieldInfo settingsField = node.settingsField;
			bool checkOn = (bool)settingsField.GetValue(null);
			bool flag = checkOn;
			if (!FilterAllows(labelNow))
			{
				GUI.color = DisallowedColor;
			}
			switch (listing.CheckboxPinnable(labelNow, ref checkOn, highlight, Prefs.DebugActionsPalette.Contains(node.Path)))
			{
			case DebugActionButtonResult.ButtonPressed:
				node.Enter(this);
				break;
			case DebugActionButtonResult.PinPressed:
				Dialog_DevPalette.ToggleAction(node.Path);
				break;
			}
			GUI.color = Color.white;
			if (Event.current.type == EventType.Layout)
			{
				totalOptionsHeight += Text.LineHeight;
			}
			if (checkOn != flag)
			{
				settingsField.SetValue(null, checkOn);
				MethodInfo method = settingsField.DeclaringType.GetMethod(settingsField.Name + "Toggled", BindingFlags.Static | BindingFlags.Public);
				if (method != null)
				{
					method.Invoke(null, null);
				}
			}
		}

		public void DoLabel(string label)
		{
			Text.Font = GameFont.Small;
			listing.Label(label);
			if (Event.current.type == EventType.Layout)
			{
				totalOptionsHeight += Text.CalcHeight(label, 300f) + 2f;
			}
		}

		public void DoGap(float gapSize = 7f)
		{
			listing.Gap(gapSize);
			if (Event.current.type == EventType.Layout)
			{
				totalOptionsHeight += gapSize;
			}
		}

		public bool FilterAllows(string label)
		{
			if (filter.NullOrEmpty())
			{
				return true;
			}
			if (label.NullOrEmpty())
			{
				return true;
			}
			return label.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public static void ResetStaticData()
		{
			rootNode = null;
			roots.Clear();
		}
	}
}
