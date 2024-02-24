using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithLeaderboardEntries : Message<LeaderboardEntryList>
	{
		public MessageWithLeaderboardEntries(IntPtr c_message)
			: base(c_message)
		{
		}

		protected override LeaderboardEntryList GetDataFromMessage(IntPtr c_message)
		{
			return Message.Deserialize<LeaderboardEntryList>(c_message);
		}

		public override LeaderboardEntryList GetLeaderboardEntryList()
		{
			return base.Data;
		}
	}
}
