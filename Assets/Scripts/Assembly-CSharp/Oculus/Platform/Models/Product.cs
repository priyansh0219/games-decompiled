using Newtonsoft.Json;

namespace Oculus.Platform.Models
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Product
	{
		[JsonObject(MemberSerialization.OptIn)]
		private class Offer
		{
			[JsonObject(MemberSerialization.OptIn)]
			private class Price
			{
				[JsonProperty("formatted")]
				private string _Formatted;

				public string Formatted => _Formatted;
			}

			[JsonProperty("description")]
			private string _Description;

			[JsonProperty("id")]
			private string _ID;

			[JsonProperty("name")]
			private string _Name;

			[JsonProperty("price")]
			private Price CurrentPrice;

			public string Description => _Description;

			public string ID => _ID;

			public string Name => _Name;

			public string FormattedPrice => CurrentPrice.Formatted;

			public Offer()
			{
				CurrentPrice = new Price();
			}
		}

		[JsonProperty("sku")]
		private string _Sku;

		[JsonProperty("current_offer")]
		private Offer CurrentOffer;

		public string Sku => _Sku;

		public string Description => CurrentOffer.Description;

		public string ID => CurrentOffer.ID;

		public string Name => CurrentOffer.Name;

		public string FormattedPrice => CurrentOffer.FormattedPrice;

		private Product()
		{
			CurrentOffer = new Offer();
		}
	}
}
