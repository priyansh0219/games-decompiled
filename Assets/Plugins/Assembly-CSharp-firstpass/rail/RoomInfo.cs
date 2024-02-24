using System.Collections.Generic;

namespace rail
{
	public class RoomInfo
	{
		public ulong zone_id;

		public bool has_password;

		public uint create_time;

		public uint max_members;

		public string room_name;

		public ulong game_server_rail_id;

		public ulong room_id;

		public uint current_members;

		public bool is_joinable;

		public EnumRoomStatus room_state;

		public List<RailKeyValue> room_kvs = new List<RailKeyValue>();

		public EnumRoomType type;

		public ulong game_server_channel_id;

		public RailID owner_id = new RailID();
	}
}
