namespace rail
{
	public interface IRailRoomHelper
	{
		void set_current_zone_id(ulong zone_id);

		ulong get_current_zone_id();

		IRailRoom CreateRoom(RoomOptions options, string room_name, out int result);

		IRailRoom AsyncCreateRoom(RoomOptions options, string room_name, string user_data);

		IRailRoom OpenRoom(ulong zone_id, ulong room_id, out int result);

		RailResult AsyncGetUserRoomList(string user_data);
	}
}
