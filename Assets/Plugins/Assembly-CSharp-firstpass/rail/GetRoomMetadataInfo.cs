using System.Collections.Generic;

namespace rail
{
	public class GetRoomMetadataInfo : EventBase
	{
		public List<RailKeyValue> key_value = new List<RailKeyValue>();

		public ulong room_id;
	}
}
