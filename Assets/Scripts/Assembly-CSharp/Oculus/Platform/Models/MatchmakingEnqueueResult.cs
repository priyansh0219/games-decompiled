using Newtonsoft.Json;

namespace Oculus.Platform.Models
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class MatchmakingEnqueueResult
	{
		[JsonProperty("average_wait_s")]
		private uint averageWait;

		[JsonProperty("max_expected_wait_s")]
		private uint maxExpectedWait;

		[JsonProperty("trace_id")]
		private string requestHash;

		public uint AverageWait => averageWait;

		public uint MaxExpectedWait => maxExpectedWait;

		public string RequestHash => requestHash;

		private MatchmakingEnqueueResult()
		{
		}
	}
}
