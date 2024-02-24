using Newtonsoft.Json;

namespace Oculus.Platform.Models
{
	[JsonObject(MemberSerialization.OptIn)]
	public class MatchmakingEnqueueResultAndRoom
	{
		[JsonProperty("viewer_room")]
		private Room _Room;

		[JsonProperty("average_wait_s")]
		private uint _AverageWait;

		[JsonProperty("max_expected_wait_s")]
		private uint _MaxExpectedWait;

		[JsonProperty("trace_id")]
		private string _RequestHash;

		public Room Room => _Room;

		public uint AverageWait => _AverageWait;

		public uint MaxExpectedWait => _MaxExpectedWait;

		public string RequestHash => _RequestHash;

		private MatchmakingEnqueueResultAndRoom()
		{
			_Room = new Room();
		}
	}
}
