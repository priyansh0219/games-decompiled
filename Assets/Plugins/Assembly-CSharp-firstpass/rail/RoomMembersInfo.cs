using System.Collections.Generic;

namespace rail
{
	public class RoomMembersInfo : EventBase
	{
		public List<MemberInfo> member_info = new List<MemberInfo>();

		public ulong room_id;

		public uint member_num;
	}
}
