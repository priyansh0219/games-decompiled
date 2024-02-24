using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Oculus.Platform.Models
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Room
	{
		[JsonObject(MemberSerialization.OptIn)]
		private class Application
		{
			[JsonProperty("id")]
			private ulong _ID;

			public ulong ID => _ID;
		}

		[JsonObject(MemberSerialization.OptIn)]
		private class Pair
		{
			[JsonProperty("key")]
			public string _Key;

			[JsonProperty("value")]
			public string _Value;
		}

		public Dictionary<string, string> DataStore;

		[JsonProperty("max_users")]
		private uint _MaxUsers;

		[JsonProperty("id")]
		private ulong _ID;

		[JsonProperty("description")]
		private string _Description;

		[JsonProperty("type")]
		private string _TypeRaw;

		private RoomType _Type;

		[JsonProperty("join_policy")]
		private string _JoinPolicyRaw;

		private JoinPolicy _JoinPolicy;

		[JsonProperty("joinability")]
		private string _JoinabilityRaw;

		private RoomJoinability _Joinability;

		[JsonProperty("owner")]
		private User _Owner;

		[JsonProperty("users")]
		private UserList _Users;

		[JsonProperty("application")]
		private Application _Application;

		[JsonProperty("data_store")]
		private List<Pair> _DataStoreRawArray;

		public ulong ID => _ID;

		public uint MaxUsers => _MaxUsers;

		public string Description => _Description;

		public RoomType Type => _Type;

		public JoinPolicy JoinPolicy => _JoinPolicy;

		public RoomJoinability Joinability => _Joinability;

		public User Owner => _Owner;

		public UserList Users => _Users;

		public ulong ApplicationID => _Application.ID;

		internal Room()
		{
			_Application = new Application();
		}

		[OnDeserialized]
		private void OnDeserializedMethod(StreamingContext context)
		{
			if (_DataStoreRawArray != null)
			{
				DataStore = new Dictionary<string, string>();
				foreach (Pair item in _DataStoreRawArray)
				{
					DataStore[item._Key] = item._Value;
				}
				_DataStoreRawArray = null;
			}
			if ("MATCHMAKING".Equals(_TypeRaw))
			{
				_Type = RoomType.Matchmaking;
			}
			else if ("MODERATED".Equals(_TypeRaw))
			{
				_Type = RoomType.Moderated;
			}
			else if ("PRIVATE".Equals(_TypeRaw))
			{
				_Type = RoomType.Private;
			}
			else if ("SOLO".Equals(_TypeRaw))
			{
				_Type = RoomType.Solo;
			}
			else
			{
				_Type = RoomType.Unknown;
			}
			_TypeRaw = null;
			if ("EVERYONE".Equals(_JoinPolicyRaw))
			{
				_JoinPolicy = JoinPolicy.Everyone;
			}
			else if ("FRIENDS_OF_MEMBERS".Equals(_JoinPolicyRaw))
			{
				_JoinPolicy = JoinPolicy.FriendsOfMembers;
			}
			else if ("FRIENDS_OF_OWNER".Equals(_JoinPolicyRaw))
			{
				_JoinPolicy = JoinPolicy.FriendsOfOwner;
			}
			else if ("INVITED_USERS".Equals(_JoinPolicyRaw))
			{
				_JoinPolicy = JoinPolicy.InvitedUsers;
			}
			else
			{
				_JoinPolicy = JoinPolicy.None;
			}
			_JoinPolicyRaw = null;
			if ("ARE_IN".Equals(_JoinabilityRaw))
			{
				_Joinability = RoomJoinability.AreIn;
			}
			else if ("ARE_KICKED".Equals(_JoinabilityRaw))
			{
				_Joinability = RoomJoinability.AreKicked;
			}
			else if ("CAN_JOIN".Equals(_JoinabilityRaw))
			{
				_Joinability = RoomJoinability.CanJoin;
			}
			else if ("IS_FULL".Equals(_JoinabilityRaw))
			{
				_Joinability = RoomJoinability.IsFull;
			}
			else if ("NO_VIEWER".Equals(_JoinabilityRaw))
			{
				_Joinability = RoomJoinability.NoViewer;
			}
			else if ("POLICY_PREVENTS".Equals(_JoinabilityRaw))
			{
				_Joinability = RoomJoinability.PolicyPrevents;
			}
			else
			{
				_Joinability = RoomJoinability.Unknown;
			}
			_JoinabilityRaw = null;
		}
	}
}
