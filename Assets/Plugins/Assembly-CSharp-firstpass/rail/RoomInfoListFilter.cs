using System.Collections.Generic;

namespace rail
{
	public class RoomInfoListFilter
	{
		public string room_name_contained;

		public List<RoomInfoListFilterKey> key_filters = new List<RoomInfoListFilterKey>();

		public EnumRailOptionalValue filter_password;

		public EnumRailOptionalValue filter_friends_owned;

		public uint available_slot_at_least;
	}
}
