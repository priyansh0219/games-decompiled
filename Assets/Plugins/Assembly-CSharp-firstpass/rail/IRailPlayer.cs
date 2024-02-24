namespace rail
{
	public interface IRailPlayer
	{
		bool AlreadyLoggedIn();

		RailID GetRailID();

		RailResult GetPlayerDataPath(out string path);

		RailResult AsyncAcquireSessionTicket(string user_data);

		RailResult AsyncStartSessionWithPlayer(RailSessionTicket player_ticket, RailID player_rail_id, string user_data);

		void TerminateSessionOfPlayer(RailID player_rail_id);

		void AbandonSessionTicket(RailSessionTicket session_ticket);

		RailResult GetPlayerName(out string name);

		EnumRailPlayerOwnershipType GetPlayerOwnershipType();

		RailResult AsyncGetGamePurchaseKey(string user_data);

		bool IsGameRevenueLimited();

		float GetRateOfGameRevenue();

		RailResult AsyncQueryPlayerBannedStatus(string user_data);

		RailResult AsyncGetAuthenticateURL(string url, string user_data);
	}
}
