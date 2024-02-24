namespace rail
{
	public interface IRailFactory
	{
		IRailPlayer RailPlayer();

		IRailUsersHelper RailUsersHelper();

		IRailFriends RailFriends();

		IRailFloatingWindow RailFloatingWindow();

		IRailBrowserHelper RailBrowserHelper();

		IRailInGamePurchase RailInGamePurchase();

		IRailZoneHelper RailZoneHelper();

		IRailRoomHelper RailRoomHelper();

		IRailGameServerHelper RailGameServerHelper();

		IRailStorageHelper RailStorageHelper();

		IRailUserSpaceHelper RailUserSpaceHelper();

		IRailStatisticHelper RailStatisticHelper();

		IRailLeaderboardHelper RailLeaderboardHelper();

		IRailAchievementHelper RailAchievementHelper();

		IRailNetChannel RailNetChannelHelper();

		IRailNetwork RailNetworkHelper();

		IRailApps RailApps();

		IRailGame RailGame();

		IRailUtils RailUtils();

		IRailAssetsHelper RailAssetsHelper();

		IRailDlcHelper RailDlcHelper();

		IRailScreenshotHelper RailScreenshotHelper();

		IRailVoiceHelper RailVoiceHelper();

		IRailSystemHelper RailSystemHelper();

		IRailTextInputHelper RailTextInputHelper();

		IRailIMEHelper RailIMETextInputHelper();

		IRailHttpSessionHelper RailHttpSessionHelper();
	}
}
