using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public static class IAP
	{
		public static Request<PurchaseList> GetViewerPurchases()
		{
			if (Core.IsInitialized())
			{
				return new Request<PurchaseList>(CAPI.ovr_IAP_GetViewerPurchases());
			}
			return null;
		}

		public static Request<ProductList> GetProductsBySKU(string[] skus)
		{
			if (Core.IsInitialized())
			{
				return new Request<ProductList>(CAPI.ovr_IAP_GetProductsBySKU(skus, skus.Length));
			}
			return null;
		}

		public static Request<Purchase> LaunchCheckoutFlow(string productID)
		{
			if (Core.IsInitialized())
			{
				return new Request<Purchase>(CAPI.ovr_IAP_LaunchCheckoutFlow(productID));
			}
			return null;
		}

		public static Request ConsumePurchase(string sku)
		{
			if (Core.IsInitialized())
			{
				return new Request(CAPI.ovr_IAP_ConsumePurchase(sku));
			}
			return null;
		}
	}
}
