using System;
using Newtonsoft.Json;

namespace Oculus.Platform
{
	public class MessageWithLeaderboardDidUpdate : Message<bool>
	{
		private class LeaderboardUpdate
		{
			[JsonProperty("did_update")]
			public bool DidUpdate;
		}

		public MessageWithLeaderboardDidUpdate(IntPtr c_message)
			: base(c_message)
		{
		}

		protected override bool GetDataFromMessage(IntPtr c_message)
		{
			return Message.Deserialize<LeaderboardUpdate>(c_message).DidUpdate;
		}

		public override bool GetLeaderboardDidUpdate()
		{
			return base.Data;
		}
	}
}
