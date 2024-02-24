using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithRoomList : Message<RoomList>
	{
		public MessageWithRoomList(IntPtr c_message)
			: base(c_message)
		{
		}

		protected override RoomList GetDataFromMessage(IntPtr c_message)
		{
			return Message.Deserialize<RoomList>(c_message);
		}

		public override RoomList GetRoomList()
		{
			return base.Data;
		}
	}
}
