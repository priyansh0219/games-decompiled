using System;
using Newtonsoft.Json;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithCurrentRoom : Message<Room>
	{
		private class CurrentRoom
		{
			[JsonProperty("current_room")]
			public Room Room;
		}

		public MessageWithCurrentRoom(IntPtr c_message)
			: base(c_message)
		{
		}

		protected override Room GetDataFromMessage(IntPtr c_message)
		{
			return Message.Deserialize<CurrentRoom>(c_message).Room;
		}

		public override Room GetRoom()
		{
			return base.Data;
		}
	}
}
