namespace rail
{
	public class LeaveRoomInfo : EventBase
	{
		public EnumLeaveRoomReason reason;

		public ulong room_id;
	}
}
