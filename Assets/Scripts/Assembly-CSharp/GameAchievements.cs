public static class GameAchievements
{
	public enum Id
	{
		None = 0,
		DiveForTheVeryFirstTime = 1,
		RepairAuroraReactor = 2,
		FindPrecursorGun = 3,
		FindPrecursorLavaCastleFacility = 4,
		FindPrecursorLostRiverFacility = 5,
		FindPrecursorPrisonFacility = 6,
		CureInfection = 7,
		DeployTimeCapsule = 8,
		FindDegasiFloatingIslandsBase = 9,
		FindDegasiJellyshroomCavesBase = 10,
		FindDegasiDeepGrandReefBase = 11,
		BuildBase = 12,
		BuildSeamoth = 13,
		BuildCyclops = 14,
		BuildExosuit = 15,
		LaunchRocket = 16,
		HatchCutefish = 17
	}

	public static string GetPlatformId(Id achievementId)
	{
		if (achievementId == Id.None)
		{
			return null;
		}
		return achievementId.ToString();
	}

	public static void Unlock(Id id)
	{
		if (GameModeUtils.AllowsAchievements() && !DevConsole.HasUsedConsole())
		{
			PlatformUtils.main.GetServices().UnlockAchievement(id);
		}
	}
}
