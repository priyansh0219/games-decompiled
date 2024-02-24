using System.Collections.Generic;

namespace rail
{
	public interface IRailZoneHelper
	{
		RailResult AsyncGetZoneList(string user_data);

		RailResult AsyncGetRoomListInZone(ulong zone_id, uint start_index, uint end_index, List<RoomInfoListSorter> sorter, List<RoomInfoListFilter> filter, string user_data);
	}
}
