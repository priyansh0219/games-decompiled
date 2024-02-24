using System;
using Newtonsoft.Json;

namespace Oculus.Platform.Models
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Purchase
	{
		[JsonObject(MemberSerialization.OptIn)]
		private class Item
		{
			[JsonProperty("sku")]
			private string _Sku;

			public string Sku => _Sku;
		}

		[JsonProperty("item")]
		private Item _Item;

		[JsonProperty("id")]
		private ulong _ID;

		[JsonProperty("grant_time")]
		private ulong _GrantTime;

		public string Sku => _Item.Sku;

		public ulong ID => _ID;

		public DateTime GrantTime => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(_GrantTime);

		private Purchase()
		{
			_Item = new Item();
		}
	}
}
