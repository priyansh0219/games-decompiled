namespace rail
{
	public class NotifyRoomGameServerChange : EventBase
	{
		public ulong game_server_rail_id;

		public ulong room_id;

		public ulong game_server_channel_id;
	}
}
