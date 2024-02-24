using Newtonsoft.Json;

namespace Oculus.Platform.Models
{
	public class LeaderboardEntryList : DeserializableList<LeaderboardEntry>
	{
		private class Summary
		{
			[JsonProperty("total_count")]
			public uint TotalCount;
		}

		[JsonProperty("summary")]
		private Summary summary;

		public uint TotalCount
		{
			get
			{
				if (summary == null)
				{
					return 0u;
				}
				return summary.TotalCount;
			}
		}
	}
}
