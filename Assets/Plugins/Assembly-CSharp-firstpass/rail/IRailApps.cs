namespace rail
{
	public interface IRailApps
	{
		bool IsGameInstalled(ulong game_id);

		RailResult AsyncQuerySubscribeWishPlayState(ulong game_id, string user_data);
	}
}
