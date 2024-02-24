namespace rail
{
	public class NotifyRoomMemberChange : EventBase
	{
		public ulong changer_id;

		public ulong id_for_making_change;

		public EnumRoomMemberActionStatus state_change;

		public ulong room_id;
	}
}
