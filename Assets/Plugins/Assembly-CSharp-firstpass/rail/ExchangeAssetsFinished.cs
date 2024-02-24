using System.Collections.Generic;

namespace rail
{
	public class ExchangeAssetsFinished : EventBase
	{
		public RailProductItem to_product_info = new RailProductItem();

		public List<RailAssetItem> old_assets = new List<RailAssetItem>();

		public ulong new_asset_id;
	}
}
