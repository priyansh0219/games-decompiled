using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Oculus.Platform.Models
{
	[JsonObject(MemberSerialization.OptIn)]
	public class LeaderboardEntry
	{
		[JsonProperty("rank")]
		private int rank;

		[JsonProperty("user")]
		private User user;

		[JsonProperty("score")]
		private long score;

		[JsonProperty("timestamp")]
		private uint timestamp;

		[JsonProperty("extra_data_base64")]
		private string extraDataRaw;

		private byte[] extraData;

		public int Rank => rank;

		public User User => user;

		public long Score => score;

		public uint Timestamp => timestamp;

		public byte[] ExtraData => extraData;

		private LeaderboardEntry()
		{
		}

		[OnDeserialized]
		private void OnDeserializedMethod(StreamingContext context)
		{
			if (extraDataRaw != null)
			{
				try
				{
					extraData = Convert.FromBase64String(extraDataRaw);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
				extraDataRaw = null;
			}
		}
	}
}
