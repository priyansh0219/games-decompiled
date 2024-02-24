using System.Collections.Generic;

namespace rail
{
	public class RailInGamePurchasePurchaseProductsToAssetsResponse : EventBase
	{
		public string order_id;

		public List<RailAssetInfo> deliveried_assets = new List<RailAssetInfo>();
	}
}
