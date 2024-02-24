using System;
using Newtonsoft.Json;

namespace Oculus.Platform.Models
{
	[JsonObject(MemberSerialization.OptIn)]
	public class AchievementProgress
	{
		[JsonProperty("bitfield_progress")]
		private string _BitfieldProgress;

		[JsonProperty("count_progress")]
		private ulong _CountProgress;

		[JsonProperty("is_unlocked")]
		private bool _IsUnlocked;

		[JsonProperty("unlock_time")]
		private uint _UnlockTime;

		[JsonProperty("definition")]
		private AchievementDefinition _Definition;

		public string Name => _Definition.Name;

		public string Bitfield => _BitfieldProgress ?? "";

		public ulong Count => _CountProgress;

		public bool IsUnlocked => _IsUnlocked;

		public DateTime UnlockTime => new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(_UnlockTime).ToLocalTime();
	}
}
