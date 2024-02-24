using Newtonsoft.Json;

namespace Oculus.Platform.Models
{
	[JsonObject(MemberSerialization.OptIn)]
	public class User
	{
		[JsonProperty("alias")]
		private string _OculusID;

		[JsonProperty("id")]
		private ulong _ID;

		[JsonProperty("token")]
		private string _InviteToken;

		[JsonProperty("presence")]
		private string _Presence;

		[JsonProperty("profile_url")]
		private string _ProfileURL;

		public string OculusID => _OculusID;

		public ulong ID => _ID;

		public string InviteToken => _InviteToken;

		public string Presence => _Presence;

		public string ImageURL => _ProfileURL;
	}
}
