namespace rail
{
	public class NotifyRoomMemberKicked : EventBase
	{
		public ulong id_for_making_kick;

		public uint due_to_kicker_lost_connect;

		public ulong room_id;

		public ulong kicked_id;
	}
}
