using UWE;

public static class GameModeUtils
{
	public static readonly Event<GameModeOption> onGameModeChanged = new Event<GameModeOption>();

	private static GameModeOption currentGameMode = GameModeOption.None;

	private static GameModeOption currentCheats = GameModeOption.None;

	private static GameModeOption currentEffectiveMode => currentGameMode | currentCheats;

	public static void TriggerHandler(Event<GameModeOption>.HandleFunction handler)
	{
		handler(currentEffectiveMode);
	}

	public static void GetGameMode(out GameModeOption mode, out GameModeOption cheats)
	{
		mode = currentGameMode;
		cheats = currentCheats;
	}

	public static bool SetGameMode(GameModeOption mode, GameModeOption cheats)
	{
		if (currentGameMode != mode || currentCheats != cheats)
		{
			currentGameMode = mode;
			currentCheats = cheats;
			onGameModeChanged.Trigger(currentEffectiveMode);
			return true;
		}
		return false;
	}

	public static bool ActivateCheat(GameModeOption cheat)
	{
		return SetGameMode(currentGameMode, currentCheats | cheat);
	}

	public static bool DeactivateCheat(GameModeOption cheat)
	{
		return SetGameMode(currentGameMode, currentCheats & ~cheat);
	}

	public static bool ToggleCheat(GameModeOption cheat)
	{
		return SetGameMode(currentGameMode, currentCheats ^ cheat);
	}

	public static bool IsOptionActive(GameModeOption mode, GameModeOption option)
	{
		return (mode & option) != 0;
	}

	public static bool IsOptionActive(GameModeOption option)
	{
		return IsOptionActive(currentEffectiveMode, option);
	}

	public static bool IsCheatActive(GameModeOption cheat)
	{
		return IsOptionActive(currentCheats, cheat);
	}

	public static bool AllowsAchievements()
	{
		return !IsOptionActive(GameModeOption.Cheats);
	}

	public static bool RequiresSurvival()
	{
		return !IsOptionActive(GameModeOption.NoSurvival);
	}

	public static bool RequiresIngredients()
	{
		return !IsOptionActive(GameModeOption.NoCost);
	}

	public static bool RequiresBlueprints()
	{
		return !IsOptionActive(GameModeOption.NoBlueprints);
	}

	public static bool RequiresPower()
	{
		return !IsOptionActive(GameModeOption.NoEnergy);
	}

	public static bool RequiresReinforcements()
	{
		return !IsOptionActive(GameModeOption.NoPressure);
	}

	public static bool RequiresOxygen()
	{
		return !IsOptionActive(GameModeOption.NoOxygen);
	}

	public static bool IsPermadeath()
	{
		return IsOptionActive(GameModeOption.Permadeath);
	}

	public static bool IsInvisible()
	{
		return IsOptionActive(GameModeOption.NoAggression);
	}

	public static bool HasRadiation()
	{
		return !IsOptionActive(GameModeOption.NoRadiation);
	}

	public static bool SpawnsInitialItems()
	{
		return IsOptionActive(GameModeOption.InitialItems);
	}
}
