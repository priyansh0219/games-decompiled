using System.Collections.Generic;

namespace rail
{
	public class GetMemberMetadataInfo : EventBase
	{
		public List<RailKeyValue> key_value = new List<RailKeyValue>();

		public ulong room_id;

		public ulong member_id;
	}
}
