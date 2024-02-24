using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Oculus.Platform.Models
{
	[JsonObject(MemberSerialization.OptIn)]
	public class AchievementDefinition
	{
		[JsonProperty("api_name")]
		private string _Name;

		[JsonProperty("achievement_type")]
		private string _TypeRaw;

		private AchievementType _Type;

		[JsonProperty("target")]
		private ulong _Target;

		[JsonProperty("bitfield_length")]
		private uint _BitfieldLength;

		public string Name => _Name;

		public ulong Target => _Target;

		public uint BitfieldLength => _BitfieldLength;

		public AchievementType Type => _Type;

		[OnDeserialized]
		private void OnDeserializedMethod(StreamingContext context)
		{
			if ("SIMPLE".Equals(_TypeRaw))
			{
				_Type = AchievementType.Simple;
			}
			else if ("BITFIELD".Equals(_TypeRaw))
			{
				_Type = AchievementType.Bitfield;
			}
			else if ("COUNT".Equals(_TypeRaw))
			{
				_Type = AchievementType.Count;
			}
			else
			{
				_Type = AchievementType.Unknown;
			}
			_TypeRaw = null;
		}
	}
}
