using System.Collections.Generic;

namespace rail
{
	public class RoomInfoList : EventBase
	{
		public uint total_room_num_in_zone;

		public List<RoomInfo> room_info = new List<RoomInfo>();

		public uint end_index;

		public ulong zone_id;

		public uint begin_index;
	}
}
