using System.Collections.Generic;
using Newtonsoft.Json;

namespace Oculus.Platform.Models
{
	[JsonObject(MemberSerialization.OptIn)]
	public class DeserializableList<T>
	{
		private class Paging
		{
			[JsonProperty("next")]
			public string Next;

			[JsonProperty("previous")]
			public string Previous;
		}

		[JsonProperty("data")]
		public readonly List<T> Data;

		[JsonProperty("paging")]
		private Paging paging;

		public string NextUrl
		{
			get
			{
				if (paging == null)
				{
					return null;
				}
				return paging.Next;
			}
		}

		public bool HasNextUrl
		{
			get
			{
				if (paging != null)
				{
					return !string.IsNullOrEmpty(paging.Next);
				}
				return false;
			}
		}

		public string PreviousUrl
		{
			get
			{
				if (paging == null)
				{
					return null;
				}
				return paging.Previous;
			}
		}

		public bool HasPreviousUrl
		{
			get
			{
				if (paging != null)
				{
					return !string.IsNullOrEmpty(paging.Previous);
				}
				return false;
			}
		}
	}
}
