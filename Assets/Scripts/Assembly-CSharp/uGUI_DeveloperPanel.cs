using System;
using System.IO;
using UWE;
using UnityEngine;
using UnityEngine.Events;
using UnityStandardAssets.ImageEffects;

public class uGUI_DeveloperPanel : uGUI_TabbedControlsPanel
{
	private HideForScreenshots.HideType screenShotHideFlags;

	protected override void OnEnable()
	{
		base.OnEnable();
		AddTabs();
		HighlightCurrentTab();
	}

	private void AddTabs()
	{
		AddTestingTab();
		AddCommandsTab();
		AddGiveTab();
		AddTeleportTab();
		AddGraphicsTab();
		AddPhotoToursTab();
	}

	private void AddConsoleCommandButton(int tabIndex, string command, string label = null, bool closePanel = false)
	{
		UnityAction callback = delegate
		{
			DevConsole.SendConsoleCommand(command);
			if (closePanel)
			{
				IngameMenu.main.Close();
			}
		};
		AddButton(tabIndex, (label != null) ? label : command, callback);
	}

	private void AddCommandsTab()
	{
		int tabIndex = AddTab("Commands");
		AddConsoleCommandButton(tabIndex, "explodeship");
		AddConsoleCommandButton(tabIndex, "kill");
		AddConsoleCommandButton(tabIndex, "bobthebuilder");
		AddConsoleCommandButton(tabIndex, "nocost");
		AddConsoleCommandButton(tabIndex, "nodamage");
		AddConsoleCommandButton(tabIndex, "oxygen");
		AddConsoleCommandButton(tabIndex, "fastbuild");
		AddConsoleCommandButton(tabIndex, "fastscan");
		AddConsoleCommandButton(tabIndex, "fasthatch");
		AddConsoleCommandButton(tabIndex, "fastgrow");
		AddConsoleCommandButton(tabIndex, "unlockdoors");
		AddConsoleCommandButton(tabIndex, "precursorkeys");
		AddConsoleCommandButton(tabIndex, "resetmotormode");
		AddConsoleCommandButton(tabIndex, "pdalog all");
		AddConsoleCommandButton(tabIndex, "ency all");
		AddConsoleCommandButton(tabIndex, "unlock all");
		AddConsoleCommandButton(tabIndex, "infect 50");
	}

	private void AddGiveTab()
	{
		int tabIndex = AddTab("Give");
		AddConsoleCommandButton(tabIndex, "item builder", "Builder");
		AddConsoleCommandButton(tabIndex, "item tank", "Tank");
		AddConsoleCommandButton(tabIndex, "item fins", "Fins");
		AddConsoleCommandButton(tabIndex, "spawn seamoth", "Seamoth", closePanel: true);
		AddConsoleCommandButton(tabIndex, "spawn exosuit", "Exosuit", closePanel: true);
		AddConsoleCommandButton(tabIndex, "sub cyclops", "Cyclops", closePanel: true);
		AddConsoleCommandButton(tabIndex, "precursorkeys", "Precursor Keys", closePanel: true);
		AddConsoleCommandButton(tabIndex, "item precursorioncrystal", "Ion Crystal", closePanel: true);
		AddConsoleCommandButton(tabIndex, "item constructor", "Mobile Vehicle Bay", closePanel: true);
		AddConsoleCommandButton(tabIndex, "eggs", "Eggs", closePanel: true);
	}

	private void AddTeleportTab()
	{
		int tabIndex = AddTab("Teleport");
		TeleportPosition[] locations = GotoConsoleCommand.main.data.locations;
		for (int i = 0; i < locations.Length; i++)
		{
			string text = locations[i].name;
			AddConsoleCommandButton(tabIndex, $"goto {text}", text, closePanel: true);
		}
		locations = BiomeConsoleCommand.main.data.locations;
		for (int i = 0; i < locations.Length; i++)
		{
			string text2 = locations[i].name;
			AddConsoleCommandButton(tabIndex, $"biome {text2}", text2, closePanel: true);
		}
	}

	private void AddComponentToggle<T>(int tabIndex, string label) where T : MonoBehaviour
	{
		T component = UnityEngine.Object.FindObjectOfType<T>();
		if (component != null)
		{
			UnityAction<bool> callback = delegate(bool value)
			{
				component.enabled = value;
			};
			AddToggleOption(tabIndex, label, component.enabled, callback);
		}
	}

	private void AddGameObjectToggle(int tabIndex, string name, string label)
	{
		GameObject gameObject = GameObject.Find(name);
		if (gameObject != null)
		{
			UnityAction<bool> callback = delegate(bool value)
			{
				gameObject.SetActive(value);
			};
			AddToggleOption(tabIndex, label, gameObject.activeInHierarchy, callback);
		}
	}

	private void OnDestroyEscapePod()
	{
		if (EscapePod.main != null)
		{
			UnityEngine.Object.Destroy(EscapePod.main.gameObject);
		}
	}

	private void AddHideOption(int tabIndex, HideForScreenshots.HideType hideFlag)
	{
		UnityAction<bool> callback = delegate(bool value)
		{
			if (value)
			{
				screenShotHideFlags |= hideFlag;
			}
			else
			{
				screenShotHideFlags &= ~hideFlag;
			}
			HideForScreenshots.Hide(screenShotHideFlags);
		};
		bool value2 = (screenShotHideFlags & hideFlag) != 0;
		AddToggleOption(tabIndex, $"Hide {hideFlag}", value2, callback);
	}

	private void AddGraphicsTab()
	{
		int tabIndex = AddTab("Graphics");
		AddHideOption(tabIndex, HideForScreenshots.HideType.Mask);
		AddHideOption(tabIndex, HideForScreenshots.HideType.HUD);
		AddHideOption(tabIndex, HideForScreenshots.HideType.ViewModel);
		AddComponentToggle<FPSCounter>(tabIndex, "Frame Rate");
		AddComponentToggle<FrameTimeOverlay>(tabIndex, "Frame Time Graph");
		AddComponentToggle<Bloom>(tabIndex, "Bloom");
		AddComponentToggle<Antialiasing>(tabIndex, "Anti-aliasing");
		AddComponentToggle<AmbientParticles>(tabIndex, "Ambient Particles");
		AddComponentToggle<VisualizeDepth>(tabIndex, "Visualize Depth");
		AddComponentToggle<WeatherManager>(tabIndex, "Weather Manager");
		WaterscapeVolumeOnCamera waterVolumeOnCamera = UnityEngine.Object.FindObjectOfType<WaterscapeVolumeOnCamera>();
		if (waterVolumeOnCamera != null)
		{
			UnityAction<bool> callback = delegate(bool value)
			{
				waterVolumeOnCamera.SetVisible(value);
			};
			AddToggleOption(tabIndex, "Water Volume", waterVolumeOnCamera.GetVisible(), callback);
		}
		WaterSurfaceOnCamera waterSurfaceOnCamera = UnityEngine.Object.FindObjectOfType<WaterSurfaceOnCamera>();
		if (waterSurfaceOnCamera != null)
		{
			UnityAction<bool> callback2 = delegate(bool value)
			{
				waterSurfaceOnCamera.SetVisible(value);
			};
			AddToggleOption(tabIndex, "Water Surface", waterSurfaceOnCamera.GetVisible(), callback2);
		}
		AddComponentToggle<WaterSunShaftsOnCamera>(tabIndex, "Light shafts");
		AddComponentToggle<LensWater>(tabIndex, "Water Drip Effect");
		AddComponentToggle<WBOIT>(tabIndex, "WBOIT Composite");
		AddGameObjectToggle(tabIndex, "x_Clouds(Clone)", "Cloud Particles");
		AddGameObjectToggle(tabIndex, "WaterParticles(Clone)", "Water Particles");
		AddChoiceOption(tabIndex, "VSync", new string[3] { "Off", "60hz", "30hz" }, QualitySettings.vSyncCount, delegate(int value)
		{
			QualitySettings.vSyncCount = value;
		});
	}

	private void OnUnityConsoleChanged(bool value)
	{
		Debug.developerConsoleVisible = value;
	}

	private void AddTestingTab()
	{
		int tabIndex = AddTab("Testing");
		DateTime dateTimeOfBuild = SNUtils.GetDateTimeOfBuild();
		string plasticChangeSetOfBuild = SNUtils.GetPlasticChangeSetOfBuild();
		string label = string.Format("Build {0} {1}", string.IsNullOrEmpty(plasticChangeSetOfBuild) ? "CHANGESET" : plasticChangeSetOfBuild, (dateTimeOfBuild == DateTime.MinValue) ? "MMM-YYYY" : dateTimeOfBuild.ToString("MMM-yyyy"));
		AddHeading(tabIndex, label);
		AddComponentToggle<DebugDisplay>(tabIndex, "Debug Text Display");
		AddToggleOption(tabIndex, "Unity Console", Debug.developerConsoleVisible, OnUnityConsoleChanged);
		AddToggleOption(tabIndex, "ClipMap Debug Display", ClipMapManager.debugLinesEnabled, delegate(bool value)
		{
			ClipMapManager.debugLinesEnabled = value;
		});
		AddToggleOption(tabIndex, "Physics.autoSyncTransforms", Physics.autoSyncTransforms, delegate(bool value)
		{
			Physics.autoSyncTransforms = value;
		});
		if (ConsoleDebugGUI.instance != null)
		{
			AddChoiceOption(tabIndex, "ConsoleDebug", new string[8] { "Off", "OctreePools", "BytePool", "ArrayPool", "Streaming", "TextureStreaming", "ObjectPools", "Updaters" }, (int)ConsoleDebugGUI.instance.mode, delegate(int value)
			{
				if (ConsoleDebugGUI.instance != null)
				{
					ConsoleDebugGUI.instance.mode = (ConsoleDebugGUI.EMode)value;
				}
			});
		}
		AddButton(tabIndex, "Destroy Escape Pod", OnDestroyEscapePod);
		AddButton(tabIndex, "Reload Streaming", LargeWorldStreamer.main.ReloadSettings);
	}

	private void AddPhotoToursTab()
	{
		int tabIndex = AddTab("Photo Tours");
		string[] tourFiles = PhotoTour.GetTourFiles();
		foreach (string tourFile in tourFiles)
		{
			UnityAction callback = delegate
			{
				IngameMenu.main.Close();
				PhotoTour.main.PlayFile(tourFile, "normal", ".");
			};
			AddButton(tabIndex, $"Play {Path.GetFileNameWithoutExtension(tourFile)}", callback);
		}
	}

	public override bool OnBack()
	{
		if (base.OnBack())
		{
			return true;
		}
		Close();
		return false;
	}
}
