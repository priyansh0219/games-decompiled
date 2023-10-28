using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class DebugWindowsOpener
	{
		private QuickSearchWidget quickSearchWidget = new QuickSearchWidget();

		protected List<Thing> foundThingsCached = new List<Thing>();

		protected Thing lastJumpedObject;

		private int searchJumpLastFrame = -1;

		private Action drawButtonsCached;

		private WidgetRow widgetRow = new WidgetRow();

		private const float SearchBarWidth = 240f;

		public DebugWindowsOpener()
		{
			drawButtonsCached = DrawButtons;
		}

		public void DevToolStarterOnGUI()
		{
			if (Prefs.DevMode)
			{
				Vector2 vector = new Vector2((float)UI.screenWidth * 0.5f, 3f);
				int num = 6;
				if (Current.ProgramState == ProgramState.Playing)
				{
					num += 2;
				}
				float num2 = 25f;
				if (Current.ProgramState == ProgramState.Playing && DebugSettings.godMode)
				{
					num2 += 15f;
				}
				Find.WindowStack.ImmediateWindow(1593759361, new Rect(vector.x, vector.y, (float)num * 28f + 240f, num2).Rounded(), WindowLayer.GameUI, drawButtonsCached, doBackground: false, absorbInputAroundWindow: false, 0f, delegate
				{
					quickSearchWidget.Unfocus();
				});
				if (KeyBindingDefOf.Dev_ToggleDebugLog.KeyDownEvent)
				{
					ToggleLogWindow();
					Event.current.Use();
				}
				if (KeyBindingDefOf.Dev_ToggleDebugActionsMenu.KeyDownEvent)
				{
					ToggleDebugActionsMenu();
					Event.current.Use();
				}
				if (KeyBindingDefOf.Dev_ToggleDebugLogMenu.KeyDownEvent)
				{
					ToggleDebugLogMenu();
					Event.current.Use();
				}
				if (KeyBindingDefOf.Dev_ToggleDebugSettingsMenu.KeyDownEvent)
				{
					ToggleDebugSettingsMenu();
					Event.current.Use();
				}
				if (KeyBindingDefOf.Dev_ToggleDevPalette.KeyDownEvent && Current.ProgramState == ProgramState.Playing)
				{
					DebugSettings.devPalette = !DebugSettings.devPalette;
					TryOpenOrClosePalette();
					Event.current.Use();
				}
				if (KeyBindingDefOf.Dev_ToggleDebugInspector.KeyDownEvent)
				{
					ToggleDebugInspector();
					Event.current.Use();
				}
				if (Current.ProgramState == ProgramState.Playing && KeyBindingDefOf.Dev_ToggleGodMode.KeyDownEvent)
				{
					ToggleGodMode();
					Event.current.Use();
				}
			}
		}

		private void DrawButtons()
		{
			widgetRow.Init(0f, 0f);
			if (widgetRow.ButtonIcon(TexButton.ToggleLog, "Open the debug log."))
			{
				ToggleLogWindow();
			}
			if (widgetRow.ButtonIcon(TexButton.ToggleTweak, "Open tweakvalues menu.\n\nThis lets you change internal values."))
			{
				ToggleTweakValuesMenu();
			}
			if (widgetRow.ButtonIcon(TexButton.OpenInspectSettings, "Open the view settings.\n\nThis lets you see special debug visuals."))
			{
				ToggleDebugSettingsMenu();
			}
			if (widgetRow.ButtonIcon(TexButton.OpenDebugActionsMenu, "Open debug actions menu.\n\nThis lets you spawn items and force various events."))
			{
				ToggleDebugActionsMenu();
			}
			if (widgetRow.ButtonIcon(TexButton.OpenDebugActionsMenu, "Open debug logging menu."))
			{
				ToggleDebugLogMenu();
			}
			if (widgetRow.ButtonIcon(TexButton.OpenInspector, "Open the inspector.\n\nThis lets you inspect what's happening in the game, down to individual variables."))
			{
				ToggleDebugInspector();
			}
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			if (widgetRow.ButtonIcon(DebugSettings.godMode ? TexButton.GodModeEnabled : TexButton.GodModeDisabled, "Toggle god mode.\n\nWhen god mode is on, you can build stuff instantly, for free, and sell things that aren't yours."))
			{
				ToggleGodMode();
			}
			bool toggleable = DebugSettings.devPalette;
			widgetRow.ToggleableIcon(ref toggleable, TexButton.ToggleDevPalette, "Toggle the dev palette.\n\nAllows you to setup a palette of debug actions for ease of use.");
			if (toggleable != DebugSettings.devPalette)
			{
				DebugSettings.devPalette = toggleable;
				TryOpenOrClosePalette();
			}
			bool toggleable2 = Prefs.PauseOnError;
			widgetRow.ToggleableIcon(ref toggleable2, TexButton.TogglePauseOnError, "Pause the game when an error is logged.");
			Prefs.PauseOnError = toggleable2;
			if (Current.Game.CurrentMap != null && Time.frameCount - searchJumpLastFrame > 10 && quickSearchWidget.CurrentlyFocused() && (Event.current.type == EventType.KeyDown || Event.current.type == EventType.Layout) && Event.current.keyCode == KeyCode.Return && quickSearchWidget.filter.Active)
			{
				foundThingsCached = Find.CurrentMap.listerThings.AllThings.Where((Thing t) => t.def.selectable && t.Label.ToLower().Contains(quickSearchWidget.filter.Text.ToLower())).ToList();
				if (!foundThingsCached.NullOrEmpty())
				{
					Find.Selector.ClearSelection();
					foreach (Thing item in foundThingsCached)
					{
						Find.Selector.Select(item);
					}
					Thing thing = foundThingsCached.Where((Thing t) => t != lastJumpedObject).RandomElementWithFallback(foundThingsCached.First());
					CameraJumper.TryJump(thing);
					lastJumpedObject = thing;
					searchJumpLastFrame = Time.frameCount;
				}
			}
			Rect rect = new Rect(widgetRow.FinalX, 0f, 240f, 24f);
			quickSearchWidget.OnGUI(rect, OnSearchChanged);
			if (Event.current.type == EventType.Layout && Event.current.keyCode == KeyCode.Escape)
			{
				quickSearchWidget.Unfocus();
			}
		}

		private void ToggleLogWindow()
		{
			if (!Find.WindowStack.TryRemove(typeof(EditWindow_Log)))
			{
				Find.WindowStack.Add(new EditWindow_Log());
			}
		}

		private void ToggleDebugSettingsMenu()
		{
			Dialog_Debug dialog_Debug = Find.WindowStack.WindowOfType<Dialog_Debug>();
			if (dialog_Debug == null)
			{
				Find.WindowStack.Add(new Dialog_Debug(DebugTabMenuDefOf.Settings));
			}
			else
			{
				dialog_Debug.SwitchTab(DebugTabMenuDefOf.Settings);
			}
		}

		private void ToggleDebugActionsMenu()
		{
			Dialog_Debug dialog_Debug = Find.WindowStack.WindowOfType<Dialog_Debug>();
			if (dialog_Debug == null)
			{
				Find.WindowStack.Add(new Dialog_Debug(DebugTabMenuDefOf.Actions));
			}
			else
			{
				dialog_Debug.SwitchTab(DebugTabMenuDefOf.Actions);
			}
		}

		private void ToggleTweakValuesMenu()
		{
			if (!Find.WindowStack.TryRemove(typeof(EditWindow_TweakValues)))
			{
				Find.WindowStack.Add(new EditWindow_TweakValues());
			}
		}

		private void ToggleDebugLogMenu()
		{
			Dialog_Debug dialog_Debug = Find.WindowStack.WindowOfType<Dialog_Debug>();
			if (dialog_Debug == null)
			{
				Find.WindowStack.Add(new Dialog_Debug(DebugTabMenuDefOf.Output));
			}
			else
			{
				dialog_Debug.SwitchTab(DebugTabMenuDefOf.Output);
			}
		}

		private void ToggleDebugInspector()
		{
			if (!Find.WindowStack.TryRemove(typeof(EditWindow_DebugInspector)))
			{
				Find.WindowStack.Add(new EditWindow_DebugInspector());
			}
		}

		private void ToggleGodMode()
		{
			DebugSettings.godMode = !DebugSettings.godMode;
			if (DebugSettings.godMode)
			{
				SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
			}
			else
			{
				SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
			}
		}

		public void TryOpenOrClosePalette()
		{
			if (DebugSettings.devPalette)
			{
				Find.WindowStack.Add(new Dialog_DevPalette());
			}
			else
			{
				Find.WindowStack.TryRemove(typeof(Dialog_DevPalette));
			}
		}

		private void OnSearchChanged()
		{
			lastJumpedObject = null;
		}
	}
}
