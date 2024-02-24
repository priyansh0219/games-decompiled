using Story;
using UnityEngine;

public class NoCostConsoleCommand : MonoBehaviour
{
	public delegate void UnlockDoors();

	public static NoCostConsoleCommand main;

	public bool fastBuildCheat { get; private set; }

	public bool fastScanCheat { get; private set; }

	public bool fastHatchCheat { get; private set; }

	public bool fastGrowCheat { get; private set; }

	public bool unlockDoors { get; private set; }

	public bool resetMotorMode { get; private set; }

	public event UnlockDoors UnlockDoorsEvent;

	private void Awake()
	{
		main = this;
		DevConsole.RegisterConsoleCommand(this, "nocost");
		DevConsole.RegisterConsoleCommand(this, "noenergy");
		DevConsole.RegisterConsoleCommand(this, "nosurvival");
		DevConsole.RegisterConsoleCommand(this, "noblueprints");
		DevConsole.RegisterConsoleCommand(this, "nopressure");
		DevConsole.RegisterConsoleCommand(this, "nohints");
		DevConsole.RegisterConsoleCommand(this, "fastbuild");
		DevConsole.RegisterConsoleCommand(this, "fastscan");
		DevConsole.RegisterConsoleCommand(this, "fasthatch");
		DevConsole.RegisterConsoleCommand(this, "fastgrow");
		DevConsole.RegisterConsoleCommand(this, "bobthebuilder");
		DevConsole.RegisterConsoleCommand(this, "hatchingtime");
		DevConsole.RegisterConsoleCommand(this, "unlockdoors");
		DevConsole.RegisterConsoleCommand(this, "precursorkeys");
		DevConsole.RegisterConsoleCommand(this, "resetmotormode");
		DevConsole.RegisterConsoleCommand(this, "cureplayergoaltrigger");
		DevConsole.RegisterConsoleCommand(this, "rib");
	}

	private void OnConsoleCommand_nocost(NotificationCenter.Notification n)
	{
		GameModeUtils.ToggleCheat(GameModeOption.NoCost);
		ErrorMessage.AddDebug("noCost cheat is now " + GameModeUtils.IsCheatActive(GameModeOption.NoCost));
	}

	private void OnConsoleCommand_noenergy(NotificationCenter.Notification n)
	{
		GameModeUtils.ToggleCheat(GameModeOption.NoEnergy);
		ErrorMessage.AddDebug("noEnergy cheat is now " + GameModeUtils.IsCheatActive(GameModeOption.NoEnergy));
	}

	private void OnConsoleCommand_nosurvival(NotificationCenter.Notification n)
	{
		GameModeUtils.ToggleCheat(GameModeOption.NoSurvival);
		ErrorMessage.AddDebug("noSurvival cheat is now " + GameModeUtils.IsCheatActive(GameModeOption.NoSurvival));
	}

	private void OnConsoleCommand_noblueprints(NotificationCenter.Notification n)
	{
		GameModeUtils.ToggleCheat(GameModeOption.NoBlueprints);
		ErrorMessage.AddDebug("noBlueprints cheat is now " + GameModeUtils.IsCheatActive(GameModeOption.NoBlueprints));
	}

	private void OnConsoleCommand_nopressure(NotificationCenter.Notification n)
	{
		GameModeUtils.ToggleCheat(GameModeOption.NoPressure);
		ErrorMessage.AddDebug("noPressure cheat is now " + GameModeUtils.IsCheatActive(GameModeOption.NoPressure));
	}

	private void OnConsoleCommand_nohints(NotificationCenter.Notification n)
	{
		GameModeUtils.ToggleCheat(GameModeOption.NoHints);
		ErrorMessage.AddDebug("noHints cheat is now " + GameModeUtils.IsCheatActive(GameModeOption.NoHints));
	}

	private void OnConsoleCommand_fastbuild(NotificationCenter.Notification n)
	{
		fastBuildCheat = !fastBuildCheat;
		ErrorMessage.AddDebug("fastBuild cheat is now " + fastBuildCheat);
	}

	private void OnConsoleCommand_fastscan(NotificationCenter.Notification n)
	{
		fastScanCheat = !fastScanCheat;
		ErrorMessage.AddDebug("fastScan cheat is now " + fastScanCheat);
	}

	private void OnConsoleCommand_fasthatch(NotificationCenter.Notification n)
	{
		fastHatchCheat = !fastHatchCheat;
		ErrorMessage.AddDebug("fastHatch cheat is now " + fastHatchCheat);
	}

	private void OnConsoleCommand_fastgrow(NotificationCenter.Notification n)
	{
		fastGrowCheat = !fastGrowCheat;
		ErrorMessage.AddDebug("fastGrow cheat is now " + fastGrowCheat);
	}

	private void OnConsoleCommand_unlockdoors(NotificationCenter.Notification n)
	{
		unlockDoors = !unlockDoors;
		ErrorMessage.AddDebug("All doors are now " + (unlockDoors ? "unlocked" : "locked"));
		if (this.UnlockDoorsEvent != null)
		{
			this.UnlockDoorsEvent();
		}
	}

	private void OnConsoleCommand_resetmotormode(NotificationCenter.Notification n)
	{
		ErrorMessage.AddDebug("Precursor Walk Mode Override Disabled");
		Player.main.precursorOutOfWater = false;
	}

	private void OnConsoleCommand_precursorkeys(NotificationCenter.Notification n)
	{
		CraftData.AddToInventory(TechType.PrecursorKey_Red);
		CraftData.AddToInventory(TechType.PrecursorKey_Orange);
		CraftData.AddToInventory(TechType.PrecursorKey_Blue);
		CraftData.AddToInventory(TechType.PrecursorKey_White);
		CraftData.AddToInventory(TechType.PrecursorKey_Purple);
	}

	private void OnConsoleCommand_cureplayergoaltrigger(NotificationCenter.Notification n)
	{
		ErrorMessage.AddDebug("Player Cure Goal Completed");
		new StoryGoal("Infection_Progress5", Story.GoalType.Story, 0f).Trigger();
	}

	private void OnConsoleCommand_rib(NotificationCenter.Notification n)
	{
		Application.runInBackground = !Application.runInBackground;
		ErrorMessage.AddDebug("Game Run in Background is now " + Application.runInBackground);
	}

	private void OnConsoleCommand_bobthebuilder(NotificationCenter.Notification n)
	{
		GameModeUtils.ActivateCheat(GameModeOption.NoCost);
		fastBuildCheat = true;
		fastScanCheat = true;
		fastHatchCheat = true;
		fastGrowCheat = true;
		CraftData.AddToInventory(TechType.Builder);
		CraftData.AddToInventory(TechType.Welder);
		CraftData.AddToInventory(TechType.Knife);
		CraftData.AddToInventory(TechType.Scanner);
		KnownTech.UnlockAll(verbose: false);
	}

	private void OnConsoleCommand_hatchingtime(NotificationCenter.Notification n)
	{
		CraftData.AddToInventory(TechType.PrecursorIonCrystal, 3);
		CraftData.AddToInventory(TechType.HatchingEnzymes);
		CraftData.AddToInventory(TechType.Seaglide);
		CraftData.AddToInventory(TechType.UltraGlideFins);
		CraftData.AddToInventory(TechType.Rebreather);
		CraftData.AddToInventory(TechType.PlasteelTank);
		DevConsole.SendConsoleCommand("goto 0 aquarium");
	}
}
