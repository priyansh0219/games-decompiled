using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithUserList : Message<UserList>
	{
		public MessageWithUserList(IntPtr c_message)
			: base(c_message)
		{
		}

		protected override UserList GetDataFromMessage(IntPtr c_message)
		{
			return Message.Deserialize<UserList>(c_message);
		}

		public override UserList GetUserList()
		{
			return base.Data;
		}
	}
}
