namespace rail
{
	public class RailFriendPlayedGameInfo
	{
		public bool in_room;

		public RailID friend_id = new RailID();

		public ulong game_server_id;

		public ulong room_id;

		public RailGameID game_id = new RailGameID();

		public bool in_game_server;

		public RailFriendPlayedGamePlayState friend_played_game_play_state;
	}
}
