using UnityEngine;

public class GameModeConsoleCommands : MonoBehaviour
{
	public static GameModeConsoleCommands main;

	private void Awake()
	{
		main = this;
		DevConsole.RegisterConsoleCommand(this, "survival");
		DevConsole.RegisterConsoleCommand(this, "creative");
		DevConsole.RegisterConsoleCommand(this, "freedom");
		DevConsole.RegisterConsoleCommand(this, "hardcore");
		DevConsole.RegisterConsoleCommand(this, "damage");
		DevConsole.RegisterConsoleCommand(this, "instagib");
		DevConsole.RegisterConsoleCommand(this, "vranim");
		DevConsole.RegisterConsoleCommand(this, "resetachievements");
		GameModeUtils.onGameModeChanged.AddHandler(this, OnGameModeChanged);
	}

	private void Destroy()
	{
		GameModeUtils.onGameModeChanged.RemoveHandler(this, OnGameModeChanged);
	}

	public void OnConsoleCommand_instagib()
	{
		DamageSystem.instagib = !DamageSystem.instagib;
		ErrorMessage.AddDebug("instagib mode " + DamageSystem.instagib);
	}

	public void OnConsoleCommand_survival()
	{
		GameModeUtils.SetGameMode(GameModeOption.None, GameModeOption.None);
	}

	public void OnConsoleCommand_creative()
	{
		GameModeUtils.SetGameMode(GameModeOption.Creative, GameModeOption.None);
	}

	public void OnConsoleCommand_freedom()
	{
		GameModeUtils.SetGameMode(GameModeOption.NoSurvival, GameModeOption.None);
	}

	public void OnConsoleCommand_hardcore()
	{
		GameModeUtils.SetGameMode(GameModeOption.Hardcore, GameModeOption.None);
	}

	private void OnGameModeChanged(GameModeOption gameMode)
	{
		ErrorMessage.AddDebug($"Game mode now: {gameMode}");
	}

	public void OnConsoleCommand_damage(NotificationCenter.Notification n)
	{
		DevConsole.ParseFloat(n, 0, out DamageSystem.damageMultiplier, 1f);
		ErrorMessage.AddDebug($"damage multiplier set to {DamageSystem.damageMultiplier}");
	}

	public void OnConsoleCommand_vranim(NotificationCenter.Notification n)
	{
		bool enableVrAnimations = GameOptions.enableVrAnimations;
		enableVrAnimations = ((n.data == null) ? (!enableVrAnimations) : bool.Parse((string)n.data[0]));
		GameOptions.enableVrAnimations = enableVrAnimations;
		ErrorMessage.AddDebug(GameOptions.enableVrAnimations ? "VR animations enabled" : "VR animations disabled");
	}

	public void OnConsoleCommand_resetachievements()
	{
		PlatformUtils.main.GetServices().ResetAchievements();
	}
}
