namespace rail
{
	public class NotifyRoomOwnerChange : EventBase
	{
		public ulong old_owner_id;

		public EnumRoomOwnerChangeReason reason;

		public ulong room_id;

		public ulong new_owner_id;
	}
}
